using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.WindowsAPICodePack.Shell;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Explorer.ExplorerPage
{
    public sealed partial class ExplorerPage : UserControl
    {
        private string _selectedPath = "";
        private ObservableCollection<TreeDir> MyPCDataSource = new ObservableCollection<TreeDir>();
        private ObservableCollection<FileDetails> CurrentDir = new ObservableCollection<FileDetails>();

        public string SelectedPath
        {
            get => _selectedPath;
            set
            {
                _selectedPath = value;
                _window.Path = _selectedPath;
                ChangeDir(_selectedPath);
            }
        }

        private MainWindow _window;

        public ExplorerPage(MainWindow window)
        {
            InitializeComponent();
            _window = window;

            SetupControl();
        }

        private void SetupControl()
        {
            SelectedPath = @"C:\Users\Joseph";
            ReloadTree();

            QuickAccessImg.Source = ImageService.GetImageFromAssembly(@"C:\Windows\system32\shell32.dll", 320);
            OneDriveImg.Source = ImageService.GetImageFromAssembly(@"C:\Program Files (x86)\Microsoft OneDrive\OneDrive.exe", 0);
            ThisPCImg.Source = ImageService.GetImageFromAssembly(@"C:\Windows\system32\imageres.dll", 104);
            NetworkImg.Source = ImageService.GetImageFromAssembly(@"C:\Windows\system32\shell32.dll", 17);
        }

        public void ReloadTree()
        {
            var tree = new ObservableCollection<TreeDir>();

            foreach (var drive in System.IO.DriveInfo.GetDrives())
            {
                var item = new TreeDir
                {
                    Name = drive.Name,
                    Path = drive.Name,
                    Image = ImageService.GetImageFromPath(drive.Name),
                    HasChildren = drive.RootDirectory.GetDirectories().Count() > 0
                };
                item.Children = GetChildren(item);
                tree.Add(item);
            }

            MyPCDataSource = tree;
        }

        public List<TreeDir> GetChildren(TreeDir node)
        {
            DirectoryInfo info = new DirectoryInfo(node.Path);
            List<TreeDir> dirList = new List<TreeDir>();
            foreach (var dir in info.GetDirectories().Where(x => !x.Attributes.HasFlag(System.IO.FileAttributes.Hidden)))
            {
                try
                {
                    var item = new TreeDir
                    {
                        Parent = node,
                        Name = dir.Name,
                        Path = dir.FullName,
                        Image = ImageService.GetImageFromPath(dir.FullName),
                        HasChildren = dir.GetDirectories().Count() > 0,
                        Children = dir.GetDirectories().Count() > 0 ? new List<TreeDir>()
                        {
                            new TreeDir
                            {
                                Name = "",
                                Path = "",
                                HasChildren = false,
                                Children = new List<TreeDir>(),
                                Image = new BitmapImage()
                            }
                        } : new List<TreeDir>()
                    };
                    //item.Image = await ConvertImage(item.Path);
                    dirList.Add(item);
                }
                catch (UnauthorizedAccessException)
                {
                    continue;
                }
                catch (IOException e)
                {
                    continue;
                }
    
            }
            return dirList;
        }

        public void ChangeDir(string path)
        {
            CurrentDir.Clear();
            DirectoryInfo info = new DirectoryInfo(path);
            _window.CanGoUp = info.Parent != null;
            foreach (var dir in info.GetDirectories().Where(x => !x.Attributes.HasFlag(FileAttributes.Hidden)))
            {
                CurrentDir.Add(new FileDetails
                {
                    Name = dir.Name,
                    Path = dir.FullName,
                    DateModified = dir.LastWriteTime,
                    Type = "File folder",
                    Icon = ImageService.GetImageFromPath(dir.FullName),
                    Size = -1
                });
            }

            foreach (var file in info.GetFiles().Where(x => !x.Attributes.HasFlag(FileAttributes.Hidden)))
            {
                CurrentDir.Add(new FileDetails
                {
                    Name = file.Name,
                    Path = file.FullName,
                    DateModified = file.LastWriteTime,
                    Type = "File",
                    Icon = ImageService.GetImageFromPath(file.FullName),
                    Size = file.Length
                });
            }
        }

        private void ThisPC_Expanding(TreeView sender, TreeViewExpandingEventArgs args)
        {
            TreeDir dir = (TreeDir)args.Item;

            if (dir.HasChildren && !dir.IsExpanded)
            {
                dir.Children = GetChildren(dir);
                dir.IsExpanded = true;
            }
            else
            {
                return;
            }

            TreeDir parent = dir.Parent == null ? dir : dir.Parent;
            while (parent.Parent != null)
                parent = parent.Parent;

            MyPCDataSource.Clear();
            MyPCDataSource.Add(parent);
        }

        private void ThisPC_Collapsed(TreeView sender, TreeViewCollapsedEventArgs args)
        {
            TreeDir dir = (TreeDir)args.Item;

            if (dir.IsExpanded)
            {
                dir.IsExpanded = false;
            }
            else
            {
                return;
            }
        }

        private void ThisPC_SelectionChanged(TreeView sender, TreeViewSelectionChangedEventArgs args)
        {
            if (args.AddedItems.Count > 0)
            {
                _window.AddBack(SelectedPath);
                _window.ClearForward();
                SelectedPath = ((TreeDir)args.AddedItems[0]).Path;
            }
        }

        private void fileListView_DoubleTapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
        {
            if (((ListView)sender).SelectedItems.Count > 0)
            {
                _window.AddBack(SelectedPath);
                _window.ClearForward();
                FileDetails details = ((ListView)sender).SelectedItems[0] as FileDetails;
                SelectedPath = details.Path;
            }
        }

        private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            ShellInterop.GetRightClick(0, 0, "C:\\Windows");
        }

        private void fileListView_RightTapped(object sender, Microsoft.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {
            ShellInterop.GetRightClick((int)e.GetPosition(null).X, (int)e.GetPosition(null).Y, ((FileDetails)fileListView.SelectedItem).Path);
        }
    }
}
