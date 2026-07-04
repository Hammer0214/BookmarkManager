using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using BookmarkManager.ViewModels;

namespace BookmarkManager.Views;

public partial class TagEditDialog : Window
{
    private TagEditViewModel ViewModel => (TagEditViewModel)DataContext;

    public TagEditDialog()
    {
        InitializeComponent();
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

    private void Ok_Click(object? sender, RoutedEventArgs? e)
    {
        if (string.IsNullOrWhiteSpace(ViewModel.Name))
        {
            MessageBox.Show("请输入标签名称", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        DialogResult = true;
    }

    private void Cancel_Click(object? sender, RoutedEventArgs? e)
    {
        DialogResult = false;
    }
}
