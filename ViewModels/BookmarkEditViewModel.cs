using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BookmarkManager.Models;

namespace BookmarkManager.ViewModels;

public partial class BookmarkEditViewModel : ObservableObject
{
    public int BookmarkId { get; set; }
    [ObservableProperty] private string _url = string.Empty;
    [ObservableProperty] private string _title = string.Empty;
    [ObservableProperty] private string _description = string.Empty;
    [ObservableProperty] private int _categoryId;
    public List<Category> AllCategories { get; set; } = new();
    public List<Tag> AllTags { get; set; } = new();
    public List<int> SelectedTagIds { get; set; } = new();

    [RelayCommand]
    private void ToggleTag(Tag tag)
    {
        if (SelectedTagIds.Contains(tag.Id))
            SelectedTagIds.Remove(tag.Id);
        else
            SelectedTagIds.Add(tag.Id);
    }

    public bool IsTagSelected(Tag tag) => SelectedTagIds.Contains(tag.Id);
}
