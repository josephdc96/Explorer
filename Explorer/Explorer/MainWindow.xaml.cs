using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Explorer
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window, INotifyPropertyChanged
    {
        private Stack<string> BackStack = new Stack<string>();
        private Stack<string> ForwardStack = new Stack<string>();
        public bool CanGoUp = false;
        private bool IsAddressChanged = false;

        public string Path = "";

        public event PropertyChangedEventHandler PropertyChanged;

        private const string DataIdentifier = "MyTabItem";
        public MainWindow()
        {
            this.InitializeComponent();

            Tabs.TabItems.Add(new TabViewItem() { IconSource = new Microsoft.UI.Xaml.Controls.SymbolIconSource() { Symbol = Symbol.Placeholder }, Header = "New Item", Content = new ExplorerPage.ExplorerPage(this) { DataContext = "New Item" } });

            Tabs.TabItemsChanged += Tabs_TabItemsChanged;
        }

        public MainWindow(TabViewItem tab)
        {
            this.InitializeComponent();

            AddTabToTabs(tab);

            Tabs.TabItemsChanged += Tabs_TabItemsChanged;
        }

        #region Tabs
        private void Tabs_TabItemsChanged(TabView sender, Windows.Foundation.Collections.IVectorChangedEventArgs args)
        {
            // If there are no more tabs, close the window.
            if (sender.TabItems.Count == 0)
            {
                this.Close();
            }
        }

        public void AddTabToTabs(TabViewItem tab)
        {
            Tabs.TabItems.Add(tab);
        }

        // Create a new Window once the Tab is dragged outside.
        private void Tabs_TabDroppedOutside(TabView sender, TabViewTabDroppedOutsideEventArgs args)
        {

            var newWindow = new MainWindow();

            Tabs.TabItems.Remove(args.Tab);
            newWindow.AddTabToTabs(args.Tab);

            newWindow.Activate();
        }

        private void Tabs_TabDragStarting(TabView sender, TabViewTabDragStartingEventArgs args)
        {
            // We can only drag one tab at a time, so grab the first one...
            var firstItem = args.Tab;

            // ... set the drag data to the tab...
            args.Data.Properties.Add(DataIdentifier, firstItem);

            // ... and indicate that we can move it 
            args.Data.RequestedOperation = DataPackageOperation.Move;
        }

        private void Tabs_TabStripDrop(object sender, DragEventArgs e)
        {
            // This event is called when we're dragging between different TabViews
            // It is responsible for handling the drop of the item into the second TabView

            if (e.DataView.Properties.TryGetValue(DataIdentifier, out object obj))
            {
                // Ensure that the obj property is set before continuing. 
                if (obj == null)
                {
                    return;
                }

                var destinationTabView = sender as TabView;
                var destinationItems = destinationTabView.TabItems;

                if (destinationItems != null)
                {
                    // First we need to get the position in the List to drop to
                    var index = -1;

                    // Determine which items in the list our pointer is between.
                    for (int i = 0; i < destinationTabView.TabItems.Count; i++)
                    {
                        var item = destinationTabView.ContainerFromIndex(i) as TabViewItem;

                        if (e.GetPosition(item).X - item.ActualWidth < 0)
                        {
                            index = i;
                            break;
                        }
                    }

                    // The TabView can only be in one tree at a time. Before moving it to the new TabView, remove it from the old.
                    var destinationTabViewListView = ((obj as TabViewItem).Parent as TabViewListView);
                    destinationTabViewListView.Items.Remove(obj);

                    if (index < 0)
                    {
                        // We didn't find a transition point, so we're at the end of the list
                        destinationItems.Add(obj);
                    }
                    else if (index < destinationTabView.TabItems.Count)
                    {
                        // Otherwise, insert at the provided index.
                        destinationItems.Insert(index, obj);
                    }

                    // Select the newly dragged tab
                    destinationTabView.SelectedItem = obj;
                }
            }
        }

        // This method prevents the TabView from handling things that aren't text (ie. files, images, etc.)
        private void Tabs_TabStripDragOver(object sender, DragEventArgs e)
        {
            if (e.DataView.Properties.ContainsKey(DataIdentifier))
            {
                e.AcceptedOperation = DataPackageOperation.Move;
            }
        }

        private void Tabs_AddTabButtonClick(TabView sender, object args)
        {
            sender.TabItems.Add(new TabViewItem() { IconSource = new Microsoft.UI.Xaml.Controls.SymbolIconSource() { Symbol = Symbol.Placeholder }, Header = "New Item", Content = new ExplorerPage.ExplorerPage(this) { DataContext = "New Item" } });
        }

        private void Tabs_TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
        {
            sender.TabItems.Remove(args.Tab);
        }
        #endregion

        private void OpenAddress_Click(object sender, RoutedEventArgs e)
        {
            Address.Text = Path;
        }

        public void AddBack(string path)
        {
            BackStack.Push(path);
            Back.IsEnabled = BackStack.Count > 0;
        }

        public string GetBack()
        {
            string str = BackStack.Pop();
            Back.IsEnabled = BackStack.Count > 0;
            return str;
        }

        public void ClearBack()
        {
            BackStack.Clear();
            Back.IsEnabled = BackStack.Count > 0;
        }

        public void AddForward(string path)
        {
            ForwardStack.Push(path);
            Forward.IsEnabled = ForwardStack.Count > 0;
        }

        public string GetForward()
        {
            string str = ForwardStack.Pop();
            Forward.IsEnabled = ForwardStack.Count > 0;
            return str;
        }

        public void ClearForward()
        {
            ForwardStack.Clear();
            Forward.IsEnabled = ForwardStack.Count > 0;
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            AddForward(Path);
            var back = GetBack();
            ((ExplorerPage.ExplorerPage)((TabViewItem)Tabs.SelectedItem).Content).SelectedPath = back;
        }

        private void Forward_Click(object sender, RoutedEventArgs e)
        {
            AddBack(Path);
            var back = GetForward();
            ((ExplorerPage.ExplorerPage)((TabViewItem)Tabs.SelectedItem).Content).SelectedPath = back;
        }

        private void Up_Click(object sender, RoutedEventArgs e)
        {
            DirectoryInfo info = new DirectoryInfo(Path);
            info = info.Parent;
            if (info != null)
            {
                AddBack(Path);
                ClearForward();
                ((ExplorerPage.ExplorerPage)((TabViewItem)Tabs.SelectedItem).Content).SelectedPath = info.FullName;
            }
        }

        public void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Address_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            Refresh.Content = "\uE76C";
            IsAddressChanged = true;
        }

        private void Flyout_Closing(FlyoutBase sender, FlyoutBaseClosingEventArgs args)
        {
            Refresh.Content = "\uE72C";
            IsAddressChanged = false;
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            if (IsAddressChanged)
            {
                AddBack(Path);
                ClearForward();
            }
            ((ExplorerPage.ExplorerPage)((TabViewItem)Tabs.SelectedItem).Content).SelectedPath = Address.Text;
        }
    }
}
