using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using BookmarkManager.Models;

namespace BookmarkManager.ViewModels;

public partial class BookmarkItemViewModel : ObservableObject
{
    public int Id { get; }
    [ObservableProperty] private string _url;
    [ObservableProperty] private string _title;
    [ObservableProperty] private string _description;
    public int CategoryId { get; }
    public int SortOrder { get; }
    [ObservableProperty] private ObservableCollection<Tag> _tags = new();

    public string DisplayTags => string.Join(" ", Tags.Select(t => $"#{t.Name}"));

    public BookmarkItemViewModel(Bookmark bm, List<Tag> tags)
    {
        Id = bm.Id;
        _url = bm.Url;
        _title = bm.Title;
        _description = bm.Description;
        CategoryId = bm.CategoryId;
        SortOrder = bm.SortOrder;
        _tags = new ObservableCollection<Tag>(tags);
    }
}
