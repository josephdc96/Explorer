using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Explorer.ExplorerPage
{
    public class FileDetails
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public DateTime DateModified { get; set; }
        public string Type { get; set; }
        public long Size { get; set; }
        public BitmapImage Icon { get; set; }
    }
}
