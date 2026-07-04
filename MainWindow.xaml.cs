using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using BookmarkManager.Models;
using BookmarkManager.Services;
using BookmarkManager.ViewModels;

namespace BookmarkManager;

public partial class MainWindow : Window
{
    private MainViewModel ViewModel => (MainViewModel)DataContext;
    private bool _isAllCategorySelected = true;

    public MainWindow()
    {
        InitializeComponent();
        Loaded += MainWindow_Loaded;
        SourceInitialized += (_, _) =>
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            var source = HwndSource.FromHwnd(hwnd);
            source?.AddHook(WndProc);
        };
        StateChanged += (_, _) =>
        {
            MaximizeBtn.Content = WindowState == WindowState.Maximized ? "❐" : "☐";
            MaximizeBtn.ToolTip = WindowState == WindowState.Maximized ? "还原" : "最大化";
        };
    }

    private const int WM_NCCALCSIZE = 0x0083;
    private const int WM_GETMINMAXINFO = 0x0024;

    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);
    [DllImport("user32.dll")]
    private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

    private const uint MONITOR_DEFAULTTONEAREST = 0x00000002;
    private const int MAXIMIZE_MARGIN = 8;

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT { public int X; public int Y; }

    [StructLayout(LayoutKind.Sequential)]
    private struct MINMAXINFO
    {
        public POINT ptReserved;
        public POINT ptMaxSize;
        public POINT ptMaxPosition;
        public POINT ptMinTrackSize;
        public POINT ptMaxTrackSize;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct MONITORINFO
    {
        public int cbSize;
        public RECT rcMonitor;
        public RECT rcWork;
        public uint dwFlags;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left; public int Top; public int Right; public int Bottom;
    }

    private static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_NCCALCSIZE)
        {
            handled = true;
            return IntPtr.Zero;
        }
        if (msg == WM_GETMINMAXINFO)
        {
            var mmi = Marshal.PtrToStructure<MINMAXINFO>(lParam);
            var monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);
            if (monitor != IntPtr.Zero)
            {
                var mi = new MONITORINFO { cbSize = Marshal.SizeOf<MONITORINFO>() };
                GetMonitorInfo(monitor, ref mi);
                mmi.ptMaxPosition.X = mi.rcWork.Left + MAXIMIZE_MARGIN;
                mmi.ptMaxPosition.Y = mi.rcWork.Top + MAXIMIZE_MARGIN;
                mmi.ptMaxSize.X = mi.rcWork.Right - mi.rcWork.Left - MAXIMIZE_MARGIN * 2;
                mmi.ptMaxSize.Y = mi.rcWork.Bottom - mi.rcWork.Top - MAXIMIZE_MARGIN * 2;
            }
            Marshal.StructureToPtr(mmi, lParam, true);
            handled = true;
            return IntPtr.Zero;
        }
        return IntPtr.Zero;
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
            ToggleMaximize();
        else
            DragMove();
    }

    private void MinimizeBtn_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

    private void MaximizeBtn_Click(object sender, RoutedEventArgs e) => ToggleMaximize();

    private void CloseBtn_Click(object sender, RoutedEventArgs e) => Close();

    private void ToggleMaximize()
    {
        if (WindowState == WindowState.Maximized)
            WindowState = WindowState.Normal;
        else
            WindowState = WindowState.Maximized;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        SearchBox.Focus();
        UpdateEmptyState();
        UpdateDarkModeButton();
        UpdateTitle();
        ViewModel.FocusSearchRequested = () => SearchBox.Focus();
        ViewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(ViewModel.Bookmarks))
                UpdateEmptyState();
        };
    }

    private void UpdateEmptyState()
    {
        if (ViewModel.Bookmarks.Count == 0)
            EmptyState.Visibility = Visibility.Visible;
        else
            EmptyState.Visibility = Visibility.Collapsed;
    }

    private void UpdateDarkModeButton()
    {
        DarkModeBtn.Content = ViewModel.IsDarkMode ? "☀️ 浅色" : "🌙 深色";
    }

    private void UpdateTitle()
    {
        if (ViewModel.SelectedCategory != null)
            TitleText.Text = $"{ViewModel.SelectedCategory.Icon} {ViewModel.SelectedCategory.Name}";
        else if (ViewModel.SelectedTagFilterId > 0)
        {
            var tag = ViewModel.Tags.FirstOrDefault(t => t.Id == ViewModel.SelectedTagFilterId);
            TitleText.Text = $"🏷 {tag?.Name ?? "标签"}";
        }
        else
            TitleText.Text = "📋 全部书签";
    }

    private void CategoryListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CategoryListBox.SelectedItem is Category cat)
        {
            ViewModel.SelectCategoryCommand.Execute(cat);
            ViewModel.SelectedCategory = cat;
            _isAllCategorySelected = false;
            AllCategoryItem.Background = System.Windows.Media.Brushes.Transparent;
        }
        else if (CategoryListBox.SelectedIndex == -1)
        {
            ViewModel.SelectCategoryCommand.Execute(null);
            _isAllCategorySelected = true;
            AllCategoryItem.Background = (System.Windows.Media.Brush)FindResource("ItemSelectedBrush");
        }
        UpdateTitle();
        UpdateEmptyState();
    }

    private void AllCategory_Click(object sender, MouseButtonEventArgs e)
    {
        CategoryListBox.UnselectAll();
        ViewModel.SelectCategoryCommand.Execute(null);
        ViewModel.SelectedCategory = null;
        _isAllCategorySelected = true;
        AllCategoryItem.Background = (System.Windows.Media.Brush)FindResource("ItemSelectedBrush");
        UpdateTitle();
        UpdateEmptyState();
    }

    private void AllCategoryItem_MouseEnter(object sender, MouseEventArgs e)
    {
        if (!_isAllCategorySelected)
            AllCategoryItem.Background = (System.Windows.Media.Brush)FindResource("ItemHoverBrush");
    }

    private void AllCategoryItem_MouseLeave(object sender, MouseEventArgs e)
    {
        if (!_isAllCategorySelected)
            AllCategoryItem.Background = System.Windows.Media.Brushes.Transparent;
    }

    private void CategoryMenu_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Category cat)
        {
            var menu = new ContextMenu();
            menu.Style = (Style)FindResource("DarkContextMenu");

            void CloseMenu() => menu.IsOpen = false;

            var editItem = new MenuItem { Header = "✏  编辑", Tag = cat };
            editItem.Style = (Style)FindResource("DarkMenuItem");
            editItem.Click += (s, ev) => { CloseMenu(); ViewModel.EditCategoryCommand.Execute(cat); };

            var deleteItem = new MenuItem { Header = "🗑  删除", Tag = cat };
            deleteItem.Style = (Style)FindResource("DarkDeleteMenuItem");
            deleteItem.Click += (s, ev) => { CloseMenu(); ViewModel.DeleteCategoryCommand.Execute(cat); };

            menu.Items.Add(editItem);
            menu.Items.Add(deleteItem);
            menu.IsOpen = true;
        }
    }

    private void Tag_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is Border border && border.Tag is int tagId)
        {
            ViewModel.FilterByTagCommand.Execute(tagId);
            var isFiltering = ViewModel.SelectedTagFilterId > 0;
            TagFilterIndicator.Visibility = isFiltering ? Visibility.Visible : Visibility.Collapsed;
            if (isFiltering)
            {
                var tag = ViewModel.Tags.FirstOrDefault(t => t.Id == ViewModel.SelectedTagFilterId);
                TagFilterText.Text = tag?.Name ?? "";
            }
            UpdateTitle();
        }
    }

    private void ClearTagFilter_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.SelectedTagFilterId = 0;
        ViewModel.SearchKeyword = string.Empty;
        ViewModel.SearchCommand.Execute(null);
        TagFilterIndicator.Visibility = Visibility.Collapsed;
        UpdateTitle();
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        ClearSearchBtn.Visibility = string.IsNullOrEmpty(SearchBox.Text)
            ? Visibility.Collapsed : Visibility.Visible;

        if (!string.IsNullOrEmpty(SearchBox.Text))
        {
            ViewModel.SearchCommand.Execute(null);
            UpdateTitle();
        }
    }

    private void SearchBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            SearchBox.Text = string.Empty;
            ViewModel.ClearSearchCommand.Execute(null);
            TagFilterIndicator.Visibility = Visibility.Collapsed;
            UpdateTitle();
        }
        else if (e.Key == Key.Enter)
        {
            ViewModel.SearchCommand.Execute(null);
        }
    }

    private void ClearSearch_Click(object sender, RoutedEventArgs e)
    {
        SearchBox.Text = string.Empty;
        ViewModel.ClearSearchCommand.Execute(null);
        TagFilterIndicator.Visibility = Visibility.Collapsed;
        UpdateTitle();
    }

    private void DarkModeBtn_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.ToggleDarkModeCommand.Execute(null);
        UpdateDarkModeButton();
    }

    private void BookmarkListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (BookmarkListBox.SelectedItem is BookmarkItemViewModel bm)
        {
            ViewModel.OpenBookmarkCommand.Execute(bm);
        }
    }

    private void BookmarkListBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Delete && BookmarkListBox.SelectedItem is BookmarkItemViewModel bm)
        {
            ViewModel.DeleteBookmarkCommand.Execute(bm);
        }
        else if (e.Key == Key.Enter && BookmarkListBox.SelectedItem is BookmarkItemViewModel bm2)
        {
            ViewModel.OpenBookmarkCommand.Execute(bm2);
        }
    }

    private void OpenUrl_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is BookmarkItemViewModel bm)
            ViewModel.OpenBookmarkCommand.Execute(bm);
    }

    private void CopyUrl_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string url)
        {
            Clipboard.SetText(url);
            ViewModel.StatusMessage = "已复制链接到剪贴板";
        }
    }

    private void EditBookmark_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is BookmarkItemViewModel bm)
            ViewModel.EditBookmarkCommand.Execute(bm);
    }

    private void DeleteBookmark_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is BookmarkItemViewModel bm)
            ViewModel.DeleteBookmarkCommand.Execute(bm);
    }

    private void Url_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is TextBlock tb && tb.Tag is string url)
        {
            try
            {
                if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                    url = "https://" + url;
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch { }
        }
    }

    // Drag & Drop for bookmarks
    private void BookmarkListBox_DragOver(object sender, DragEventArgs e)
    {
        e.Effects = DragDropEffects.Move;
        e.Handled = true;
    }

    private async void BookmarkListBox_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(typeof(BookmarkItemViewModel)) &&
            sender is ListBox listBox)
        {
            var draggedItem = e.Data.GetData(typeof(BookmarkItemViewModel)) as BookmarkItemViewModel;
            if (draggedItem == null) return;

            var items = ViewModel.Bookmarks;
            var fromIndex = items.IndexOf(draggedItem);

            var position = e.GetPosition(listBox);
            var targetItem = GetItemAtPosition(listBox, position);
            if (targetItem == null || targetItem == draggedItem) return;

            var toIndex = items.IndexOf(targetItem);

            items.RemoveAt(fromIndex);
            if (toIndex > fromIndex) toIndex--;
            items.Insert(toIndex, draggedItem);

            for (int i = 0; i < items.Count; i++)
            {
                await BookmarkRepository.UpdateBookmarkSortOrderAsync(items[i].Id, i);
            }
        }
    }

    private BookmarkItemViewModel? GetItemAtPosition(ListBox listBox, Point position)
    {
        for (int i = 0; i < listBox.Items.Count; i++)
        {
            var item = listBox.ItemContainerGenerator.ContainerFromIndex(i) as ContentPresenter;
            if (item == null) continue;

            var rect = VisualTreeHelper.GetDescendantBounds(item);
            var itemPos = item.TransformToAncestor(listBox).Transform(position);

            if (rect.Contains(itemPos))
                return listBox.Items[i] as BookmarkItemViewModel;
        }
        return null;
    }
}
