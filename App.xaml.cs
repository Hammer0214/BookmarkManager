using System;
using System.Windows;
using System.Windows.Threading;
using BookmarkManager.Services;
using BookmarkManager.ViewModels;

namespace BookmarkManager;

public partial class App : Application
{
    private MainViewModel? _viewModel;
    private Window? _mainWindow;

    protected override void OnStartup(StartupEventArgs e)
    {
        DispatcherUnhandledException += (_, args) =>
        {
            MessageBox.Show(args.Exception.ToString(), "未处理异常", MessageBoxButton.OK, MessageBoxImage.Error);
            args.Handled = true;
        };
        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            MessageBox.Show(args.ExceptionObject.ToString(), "严重错误", MessageBoxButton.OK, MessageBoxImage.Error);
        };

        base.OnStartup(e);
        DatabaseService.Initialize();
        _ = InitializeAsync();
    }

    private async System.Threading.Tasks.Task InitializeAsync()
    {
        try
        {
            _viewModel = new MainViewModel();
            _viewModel.ShowBookmarkEditDialog = ShowBookmarkEditDialog;
            _viewModel.ShowCategoryEditDialog = ShowCategoryEditDialog;
            _viewModel.ShowTagEditDialog = ShowTagEditDialog;

            await _viewModel.LoadDataAsync();

            if (_viewModel.IsDarkMode)
                ApplyTheme(true);

            _mainWindow = new MainWindow { DataContext = _viewModel };
            _mainWindow.Show();

            await ShowAnnouncementIfNeededAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"启动失败: {ex.Message}\n\n{ex}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async System.Threading.Tasks.Task ShowAnnouncementIfNeededAsync()
    {
        try
        {
            var setting = await BookmarkRepository.GetSettingAsync("ShowAnnouncement");
            if (setting == "false") return;

            var dialog = new Views.AnnouncementDialog();
            dialog.Owner = _mainWindow;
            dialog.ShowDialog();
        }
        catch { }
    }

    public void ApplyTheme(bool isDark)
    {
        var themeUri = isDark
            ? new Uri("Resources/Themes/Dark.xaml", UriKind.Relative)
            : new Uri("Resources/Themes/Light.xaml", UriKind.Relative);

        var dict = new ResourceDictionary { Source = themeUri };

        Resources.MergedDictionaries.Clear();
        Resources.MergedDictionaries.Add(dict);
    }

    private bool ShowBookmarkEditDialog(BookmarkEditViewModel vm)
    {
        var dialog = new Views.BookmarkEditDialog { DataContext = vm };
        dialog.Owner = _mainWindow;
        return dialog.ShowDialog() == true;
    }

    private bool ShowCategoryEditDialog(CategoryEditViewModel vm)
    {
        var dialog = new Views.CategoryEditDialog { DataContext = vm };
        dialog.Owner = _mainWindow;
        return dialog.ShowDialog() == true;
    }

    private bool ShowTagEditDialog(TagEditViewModel vm)
    {
        var dialog = new Views.TagEditDialog { DataContext = vm };
        dialog.Owner = _mainWindow;
        return dialog.ShowDialog() == true;
    }
}
