using CommunityToolkit.Mvvm.ComponentModel;

namespace BookmarkManager.ViewModels;

public partial class CategoryEditViewModel : ObservableObject
{
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _icon = "📁";
}
