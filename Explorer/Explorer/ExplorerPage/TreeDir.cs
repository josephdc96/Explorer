using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;

namespace Explorer.ExplorerPage
{
    public class TreeDir
    {
        public TreeDir Parent { get; set; }
        public string Path { get; set; }
        public string Name { get; set; }
        public bool HasChildren { get; set; }
        public BitmapImage Image { get; set; }
        public List<TreeDir> Children { get; set; }
        public bool IsExpanded { get; set; }
    }
}
