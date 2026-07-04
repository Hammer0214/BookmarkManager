using CommunityToolkit.Mvvm.ComponentModel;

namespace BookmarkManager.ViewModels;

public partial class TagEditViewModel : ObservableObject
{
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _color = "#4A90D9";
}
