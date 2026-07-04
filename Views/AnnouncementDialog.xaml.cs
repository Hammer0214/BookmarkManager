using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using BookmarkManager.Services;

namespace BookmarkManager.Views;

public partial class AnnouncementDialog : Window
{
    private const int WM_NCCALCSIZE = 0x0083;

    public bool DontShowAgain { get; private set; }

    public AnnouncementDialog()
    {
        InitializeComponent();
        Icon = new BitmapImage(new Uri("pack://application:,,,/app.ico", UriKind.Absolute));
        SourceInitialized += (_, _) =>
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            var source = HwndSource.FromHwnd(hwnd);
            source?.AddHook(WndProc);
        };
    }

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

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
            Close_Click(null, null);
    }

    private void CloseBtn_Click(object sender, RoutedEventArgs e) => Close();

    private void Close_Click(object? sender, RoutedEventArgs? e)
    {
        DontShowAgain = false;
        DialogResult = true;
    }

    private void DontShowAgain_Click(object? sender, RoutedEventArgs? e)
    {
        _ = BookmarkRepository.SetSettingAsync("ShowAnnouncement", "false");
        DontShowAgain = true;
        DialogResult = true;
    }
}
