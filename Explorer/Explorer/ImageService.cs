using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.WindowsAPICodePack.Shell;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAFactory.IconPack;

namespace Explorer
{
    public static class ImageService
    {
        public enum ItemType
        {
            Drive = 0,
            Directory,
            File
        }

        public static BitmapImage GetImageFromPath(string path)
        {
            ShellObject obj;
            try 
            {
                obj = ShellFileSystemFolder.FromFolderPath(path);
            } 
            catch (DirectoryNotFoundException)
            {
                obj = ShellFile.FromFilePath(path);
            }
            Bitmap bitmap = obj.Thumbnail.SmallBitmap;
            bitmap.MakeTransparent(Color.Black);

            using var memory = new MemoryStream();
            bitmap.Save(memory, ImageFormat.Png);
            memory.Position = 0;
            BitmapImage bitmapImage = new BitmapImage();
            memory.Seek(0, SeekOrigin.Begin);
            _ = bitmapImage.SetSourceAsync(memory.AsRandomAccessStream());
            return bitmapImage;
        }

        public static BitmapImage GetImageFromAssembly(string path, int iconNum)
        {
            Icon icon = IconHelper.ExtractBestFitIcon(path, iconNum, new Size(32, 32));
            Bitmap bitmap = icon.ToBitmap();

            using var memory = new MemoryStream();
            bitmap.Save(memory, ImageFormat.Png);
            memory.Position = 0;
            BitmapImage bitmapImage = new BitmapImage();
            memory.Seek(0, SeekOrigin.Begin);
            _ = bitmapImage.SetSourceAsync(memory.AsRandomAccessStream());
            return bitmapImage;
        }
    }
}
