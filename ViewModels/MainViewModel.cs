using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BookmarkManager.Models;
using BookmarkManager.Services;

namespace BookmarkManager.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<Category> _categories = new();
    [ObservableProperty] private ObservableCollection<BookmarkItemViewModel> _bookmarks = new();
    [ObservableProperty] private ObservableCollection<Tag> _tags = new();
    [ObservableProperty] private Category? _selectedCategory;
    [ObservableProperty] private BookmarkItemViewModel? _selectedBookmark;
    [ObservableProperty] private string _searchKeyword = string.Empty;
    [ObservableProperty] private bool _isDarkMode;
    [ObservableProperty] private string _statusMessage = "就绪";
    [ObservableProperty] private int _selectedTagFilterId;

    public bool IsAllSelected => SelectedCategory == null;

    public MainViewModel()
    {
        try
        {
            var darkModeTask = BookmarkRepository.GetSettingAsync("DarkMode");
            IsDarkMode = darkModeTask.Result == "true";
        }
        catch
        {
            IsDarkMode = false;
        }
    }

    public async Task LoadDataAsync()
    {
        Categories = new ObservableCollection<Category>(await BookmarkRepository.GetCategoriesAsync());
        Tags = new ObservableCollection<Tag>(await BookmarkRepository.GetTagsAsync());
        await LoadBookmarksAsync();
    }

    private async Task LoadBookmarksAsync()
    {
        try
        {
            if (!string.IsNullOrEmpty(SearchKeyword))
            {
                var results = await BookmarkRepository.SearchBookmarksAsync(SearchKeyword);
                await SetBookmarkTags(results);
                return;
            }
            if (SelectedTagFilterId > 0)
            {
                var results = await BookmarkRepository.GetBookmarksByTagAsync(SelectedTagFilterId);
                await SetBookmarkTags(results);
                return;
            }

            var cats = SelectedCategory != null ? SelectedCategory.Id : 0;
            var list = await BookmarkRepository.GetBookmarksAsync(cats > 0 ? cats : null);
            await SetBookmarkTags(list);
        }
        catch (Exception ex)
        {
            StatusMessage = $"加载书签失败: {ex.Message}";
        }
    }

    private async Task SetBookmarkTags(System.Collections.Generic.List<Bookmark> list)
    {
        if (list.Count == 0)
        {
            Bookmarks = new ObservableCollection<BookmarkItemViewModel>();
            StatusMessage = "共 0 个书签";
            return;
        }
        var bookmarkIds = list.Select(b => b.Id).ToList();
        var tagsDict = await BookmarkRepository.GetTagsForBookmarksAsync(bookmarkIds);
        var vms = new ObservableCollection<BookmarkItemViewModel>();
        foreach (var b in list)
        {
            var bmTags = tagsDict.GetValueOrDefault(b.Id, new List<Tag>());
            vms.Add(new BookmarkItemViewModel(b, bmTags));
        }
        Bookmarks = vms;
        StatusMessage = $"共 {Bookmarks.Count} 个书签";
    }

    [RelayCommand]
    private async Task SelectCategoryAsync(Category? cat)
    {
        SelectedCategory = cat;
        SelectedTagFilterId = 0;
        await LoadBookmarksAsync();
    }

    [RelayCommand]
    private async Task FilterByTagAsync(int tagId)
    {
        SelectedTagFilterId = SelectedTagFilterId == tagId ? 0 : tagId;
        SelectedCategory = null;
        await LoadBookmarksAsync();
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        await LoadBookmarksAsync();
    }

    [RelayCommand]
    private async Task ClearSearchAsync()
    {
        SearchKeyword = string.Empty;
        await LoadBookmarksAsync();
    }

    [RelayCommand]
    private void OpenBookmark(BookmarkItemViewModel? bm)
    {
        if (bm == null) return;
        try
        {
            var url = bm.Url;
            if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                url = "https://" + url;
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            StatusMessage = $"已打开: {bm.Title}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"打开失败: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task AddBookmarkAsync()
    {
        var vm = new BookmarkEditViewModel
        {
            CategoryId = SelectedCategory?.Id ?? Categories.FirstOrDefault()?.Id ?? 0,
            AllCategories = Categories.ToList(),
            AllTags = Tags.ToList()
        };
        if (vm.CategoryId == 0)
        {
            MessageBox.Show("请先创建一个分类", "提示");
            return;
        }
        if (ShowBookmarkEditDialog?.Invoke(vm) == true)
        {
            var bm = new Bookmark
            {
                Url = vm.Url,
                Title = vm.Title,
                Description = vm.Description,
                CategoryId = vm.CategoryId,
                SortOrder = Bookmarks.Count
            };
            bm.Id = await BookmarkRepository.AddBookmarkAsync(bm);
            await BookmarkRepository.SetTagsForBookmarkAsync(bm.Id, vm.SelectedTagIds);
            StatusMessage = $"已添加: {vm.Title}";
            await LoadBookmarksAsync();
        }
    }

    [RelayCommand]
    private async Task EditBookmarkAsync(BookmarkItemViewModel? item)
    {
        if (item == null) return;
        var bmTags = await BookmarkRepository.GetTagsForBookmarkAsync(item.Id);
        var vm = new BookmarkEditViewModel
        {
            BookmarkId = item.Id,
            Url = item.Url,
            Title = item.Title,
            Description = item.Description,
            CategoryId = item.CategoryId,
            SelectedTagIds = bmTags.Select(t => t.Id).ToList(),
            AllCategories = Categories.ToList(),
            AllTags = Tags.ToList()
        };
        if (ShowBookmarkEditDialog?.Invoke(vm) == true)
        {
            var bm = new Bookmark
            {
                Id = item.Id,
                Url = vm.Url,
                Title = vm.Title,
                Description = vm.Description,
                CategoryId = vm.CategoryId,
                SortOrder = item.SortOrder
            };
            await BookmarkRepository.UpdateBookmarkAsync(bm);
            await BookmarkRepository.SetTagsForBookmarkAsync(bm.Id, vm.SelectedTagIds);
            StatusMessage = $"已更新: {vm.Title}";
            await LoadBookmarksAsync();
        }
    }

    [RelayCommand]
    private async Task DeleteBookmarkAsync(BookmarkItemViewModel? item)
    {
        if (item == null) return;
        if (MessageBox.Show($"确定删除 \"{item.Title}\" 吗？", "确认删除",
            MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
        {
            await BookmarkRepository.DeleteBookmarkAsync(item.Id);
            StatusMessage = $"已删除: {item.Title}";
            await LoadBookmarksAsync();
        }
    }

    [RelayCommand]
    private async Task AddCategoryAsync()
    {
        var vm = new CategoryEditViewModel();
        if (ShowCategoryEditDialog?.Invoke(vm) == true)
        {
            var cat = new Category { Name = vm.Name, Icon = vm.Icon, SortOrder = Categories.Count };
            cat.Id = await BookmarkRepository.AddCategoryAsync(cat);
            Categories.Add(cat);
            StatusMessage = $"已添加分类: {vm.Name}";
        }
    }

    [RelayCommand]
    private async Task EditCategoryAsync(Category? cat)
    {
        if (cat == null) return;
        var vm = new CategoryEditViewModel { Name = cat.Name, Icon = cat.Icon };
        if (ShowCategoryEditDialog?.Invoke(vm) == true)
        {
            cat.Name = vm.Name;
            cat.Icon = vm.Icon;
            await BookmarkRepository.UpdateCategoryAsync(cat);
            var newList = new ObservableCollection<Category>(Categories);
            SelectedCategory = newList.FirstOrDefault(c => c.Id == cat.Id);
            Categories = newList;
            StatusMessage = $"已更新分类: {vm.Name}";
        }
    }

    [RelayCommand]
    private async Task DeleteCategoryAsync(Category? cat)
    {
        if (cat == null) return;
        if (MessageBox.Show($"确定删除分类 \"{cat.Name}\" 吗？\n该分类下的书签不会被删除。",
            "确认删除", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
        {
            await BookmarkRepository.DeleteCategoryAsync(cat.Id);
            Categories.Remove(cat);
            if (SelectedCategory?.Id == cat.Id)
                SelectedCategory = null;
            StatusMessage = $"已删除分类: {cat.Name}";
            await LoadBookmarksAsync();
        }
    }

    [RelayCommand]
    private async Task AddTagAsync()
    {
        var vm = new TagEditViewModel();
        if (ShowTagEditDialog?.Invoke(vm) == true)
        {
            var tag = new Tag { Name = vm.Name, Color = vm.Color };
            tag.Id = await BookmarkRepository.AddTagAsync(tag);
            Tags.Add(tag);
            StatusMessage = $"已添加标签: {vm.Name}";
        }
    }

    [RelayCommand]
    private async Task DeleteTagAsync(Tag? tag)
    {
        if (tag == null) return;
        if (MessageBox.Show($"确定删除标签 \"{tag.Name}\" 吗？", "确认删除",
            MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
        {
            await BookmarkRepository.DeleteTagAsync(tag.Id);
            Tags.Remove(tag);
            StatusMessage = $"已删除标签: {tag.Name}";
            if (SelectedTagFilterId == tag.Id)
                SelectedTagFilterId = 0;
            await LoadBookmarksAsync();
        }
    }

    [RelayCommand]
    private void ToggleDarkMode()
    {
        IsDarkMode = !IsDarkMode;
        _ = BookmarkRepository.SetSettingAsync("DarkMode", IsDarkMode.ToString().ToLower());
        ((App)Application.Current).ApplyTheme(IsDarkMode);
    }

    [RelayCommand]
    private async Task ExportAsync()
    {
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "JSON文件|*.json|HTML书签|*.html",
            DefaultExt = ".json",
            FileName = "bookmarks_backup"
        };
        if (dialog.ShowDialog() == true)
        {
            try
            {
                var allBookmarks = await BookmarkRepository.GetBookmarksAsync();
                var allCategories = await BookmarkRepository.GetCategoriesAsync();

                if (dialog.FilterIndex == 1)
                {
                    var exportData = new
                    {
                        Categories = allCategories,
                        Bookmarks = allBookmarks,
                        ExportTime = DateTime.Now
                    };
                    var json = JsonSerializer.Serialize(exportData, new JsonSerializerOptions { WriteIndented = true });
                    await File.WriteAllTextAsync(dialog.FileName, json);
                }
                else
                {
                    var html = GenerateHtmlBookmarks(allCategories, allBookmarks);
                    await File.WriteAllTextAsync(dialog.FileName, html);
                }
                StatusMessage = $"已导出到: {dialog.FileName}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"导出失败: {ex.Message}";
            }
        }
    }

    [RelayCommand]
    private async Task ImportAsync()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "JSON文件|*.json|HTML书签|*.html|所有文件|*.*",
            Multiselect = false
        };
        if (dialog.ShowDialog() == true)
        {
            try
            {
                var content = await File.ReadAllTextAsync(dialog.FileName);
                if (dialog.FileName.EndsWith(".json"))
                {
                    await ImportJsonAsync(content);
                }
                else
                {
                    await ImportHtmlBookmarksAsync(content);
                }
                StatusMessage = "导入成功！";
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                StatusMessage = $"导入失败: {ex.Message}";
            }
        }
    }

    private async Task ImportJsonAsync(string json)
    {
        var data = JsonSerializer.Deserialize<JsonElement>(json);
        var existingCats = await BookmarkRepository.GetCategoriesAsync();
        var existingBms = await BookmarkRepository.GetBookmarksAsync();
        var existingUrls = new HashSet<string>(existingBms.Select(b => b.Url));
        var catNameMap = new Dictionary<string, int>();

        if (data.TryGetProperty("Categories", out var cats))
        {
            foreach (var cat in cats.EnumerateArray())
            {
                var name = cat.GetProperty("Name").GetString() ?? "未分类";
                var match = existingCats.FirstOrDefault(c => c.Name == name);
                if (match != null)
                {
                    catNameMap[name] = match.Id;
                    continue;
                }
                var c = new Category
                {
                    Name = name,
                    Icon = cat.TryGetProperty("Icon", out var icon) ? icon.GetString() ?? "📁" : "📁",
                    SortOrder = cat.TryGetProperty("SortOrder", out var so) ? so.GetInt32() : 0
                };
                c.Id = await BookmarkRepository.AddCategoryAsync(c);
                catNameMap[name] = c.Id;
            }
        }
        if (data.TryGetProperty("Bookmarks", out var bms))
        {
            foreach (var bm in bms.EnumerateArray())
            {
                var url = bm.GetProperty("Url").GetString() ?? "";
                if (existingUrls.Contains(url)) continue;
                var b = new Bookmark
                {
                    Url = url,
                    Title = bm.GetProperty("Title").GetString() ?? "",
                    Description = bm.TryGetProperty("Description", out var desc) ? desc.GetString() ?? "" : "",
                    CategoryId = bm.TryGetProperty("CategoryId", out var catId) ? catId.GetInt32() : 0,
                    SortOrder = bm.TryGetProperty("SortOrder", out var so) ? so.GetInt32() : 0
                };
                await BookmarkRepository.AddBookmarkAsync(b);
            }
        }
    }

    private async Task ImportHtmlBookmarksAsync(string html)
    {
        var defaultCat = Categories.FirstOrDefault();
        if (defaultCat == null)
        {
            var cat = new Category { Name = "导入", Icon = "📥", SortOrder = 0 };
            cat.Id = await BookmarkRepository.AddCategoryAsync(cat);
            defaultCat = cat;
        }

        var lines = html.Split('\n');
        int order = 0;
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (trimmed.Contains("<A ") && trimmed.Contains("HREF="))
            {
                var url = ExtractAttr(trimmed, "HREF");
                var title = ExtractInnerText(trimmed);
                if (!string.IsNullOrEmpty(url))
                {
                    var bm = new Bookmark
                    {
                        Url = url,
                        Title = string.IsNullOrEmpty(title) ? url : title,
                        CategoryId = defaultCat.Id,
                        SortOrder = order++
                    };
                    await BookmarkRepository.AddBookmarkAsync(bm);
                }
            }
        }
    }

    private static string ExtractAttr(string line, string attr)
    {
        var idx = line.IndexOf(attr + "=\"", StringComparison.OrdinalIgnoreCase);
        if (idx < 0) return "";
        idx += attr.Length + 2;
        var end = line.IndexOf('"', idx);
        if (end < 0) return "";
        return line.Substring(idx, end - idx);
    }

    private static string ExtractInnerText(string line)
    {
        var start = line.IndexOf('>');
        var end = line.LastIndexOf('<');
        if (start < 0 || end < 0 || end <= start) return "";
        return line.Substring(start + 1, end - start - 1);
    }

    private string GenerateHtmlBookmarks(System.Collections.Generic.List<Category> cats, System.Collections.Generic.List<Bookmark> bms)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("<!DOCTYPE NETSCAPE-Bookmark-file-1>");
        sb.AppendLine("<META HTTP-EQUIV=\"Content-Type\" CONTENT=\"text/html; charset=UTF-8\">");
        sb.AppendLine("<TITLE>Bookmarks</TITLE>");
        sb.AppendLine("<H1>Bookmarks</H1>");
        sb.AppendLine("<DL><p>");

        var grouped = bms.GroupBy(b => b.CategoryId);
        foreach (var g in grouped)
        {
            var cat = cats.FirstOrDefault(c => c.Id == g.Key);
            sb.AppendLine($"<DT><H3>{cat?.Name ?? "未分类"}</H3>");
            sb.AppendLine("<DL><p>");
            foreach (var bm in g)
            {
                sb.AppendLine($"<DT><A HREF=\"{bm.Url}\">{bm.Title}</A>");
            }
            sb.AppendLine("</DL><p>");
        }
        sb.AppendLine("</DL><p>");
        return sb.ToString();
    }

    [RelayCommand]
    private void FocusSearch()
    {
        FocusSearchRequested?.Invoke();
    }

    public Action? FocusSearchRequested { get; set; }

    public Func<BookmarkEditViewModel, bool>? ShowBookmarkEditDialog { get; set; }
    public Func<CategoryEditViewModel, bool>? ShowCategoryEditDialog { get; set; }
    public Func<TagEditViewModel, bool>? ShowTagEditDialog { get; set; }
}
