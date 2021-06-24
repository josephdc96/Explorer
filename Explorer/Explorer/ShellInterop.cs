using Peter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Explorer
{
    public static class ShellInterop
    {
        public static void GetRightClick(int x, int y, string path)
        {
            ShellContextMenu shellContextMenu = new ShellContextMenu();
            try
            {
                FileInfo[] info = new FileInfo[] { new FileInfo(path) };
                shellContextMenu.ShowContextMenu(info, new System.Drawing.Point(x, y));
            }
            catch (Exception e)
            {
                DirectoryInfo[] info = new DirectoryInfo[] { new DirectoryInfo(path) };
                shellContextMenu.ShowContextMenu(info, new System.Drawing.Point(x, y));
            }
        }
    }
}
