using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using BookmarkManager.Models;
using BookmarkManager.ViewModels;

namespace BookmarkManager.Views;

public partial class BookmarkEditDialog : Window
{
    private BookmarkEditViewModel ViewModel => (BookmarkEditViewModel)DataContext;

    public BookmarkEditDialog()
    {
        InitializeComponent();
        Loaded += (s, e) => UrlBox.Focus();
        SourceInitialized += (_, _) =>
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            var source = HwndSource.FromHwnd(hwnd);
            source?.AddHook(WndProc);
        };
    }

    private const int WM_NCCALCSIZE = 0x0083;

    private static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_NCCALCSIZE)
        {
            handled = true;
            return IntPtr.Zero;
        }
        return IntPtr.Zero;
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        DragMove();
    }

    private void CloseBtn_Click(object sender, RoutedEventArgs e) => DialogResult = false;

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) Ok_Click(null, null);
        else if (e.Key == Key.Escape) Cancel_Click(null, null);
    }

    private void Tag_Toggle(object sender, MouseButtonEventArgs e)
    {
        if (sender is System.Windows.Controls.Border border && border.Tag is Tag tag)
        {
            ViewModel.ToggleTagCommand.Execute(tag);
        }
    }

    private void Ok_Click(object? sender, RoutedEventArgs? e)
    {
        if (string.IsNullOrWhiteSpace(ViewModel.Url))
        {
            MessageBox.Show("请输入网址", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (string.IsNullOrWhiteSpace(ViewModel.Title))
        {
            ViewModel.Title = ViewModel.Url;
        }
        DialogResult = true;
    }

    private void Cancel_Click(object? sender, RoutedEventArgs? e)
    {
        DialogResult = false;
    }
}
