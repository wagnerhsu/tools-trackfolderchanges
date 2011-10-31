using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Collections.Generic;
using System;

namespace Antiufo
{

    public class IconsHandler : IDisposable
    {

        private ImageList _smallImageList;
        private ImageList _largeImageList;

        private bool _useSmallIcons;

        private bool _useLargeIcons;

        private Dictionary<string, int> loadedIcons = new Dictionary<string, int>();

        public ImageList SmallIcons
        {
            get
            {
                return _smallImageList;
            }
        }

        public ImageList LargeIcons
        {
            get
            {
                return _largeImageList;
            }
        }

        public void Clear()
        {
            _smallImageList = null;
            _largeImageList = null;
            if (_useSmallIcons)
            {
                _smallImageList = new ImageList();
                _smallImageList.ColorDepth = ColorDepth.Depth32Bit;
            }
            if (_useLargeIcons)
            {
                _largeImageList = new ImageList();
                _smallImageList.ColorDepth = ColorDepth.Depth32Bit;
            }
            length = 0;
            loadedIcons.Clear();
        }

        public int Add(Image smallImage, Image largeImage)
        {
            length++;
            if (_smallImageList != null) _smallImageList.Images.Add(smallImage);
            if (_largeImageList != null) _largeImageList.Images.Add(largeImage);
            return length - 1;
        }

        public IconsHandler(bool useSmallIcons, bool useLargeIcons)
        {
            if ((!useSmallIcons && !useLargeIcons))
            {
                throw new ArgumentException("Cannot create an IconsHandler without ImageLists");
            }

            _useSmallIcons = useSmallIcons;
            _useLargeIcons = useLargeIcons;
            Clear();
        }

        public IconsHandler(ImageList smallImagesList, ImageList largeImagesList)
        {
            if (largeImagesList == null && smallImagesList == null)
            {
                throw new ArgumentException("Cannot create an IconsHandler without ImageLists");
            }
            if (largeImagesList != null && smallImagesList != null && largeImagesList.Images.Count != smallImagesList.Images.Count)
            {
                throw new ArgumentException("Initial ImageLists do not have the same number of elements");
            }

            _useSmallIcons = smallImagesList != null;
            _useLargeIcons = largeImagesList != null;

            _smallImageList = smallImagesList;
            _largeImageList = largeImagesList;

            length = _useLargeIcons ? largeImagesList.Images.Count : smallImagesList.Images.Count;
        }






        private int length;

        public int GetIcon(string path)
        {
            if (loadedIcons.ContainsKey(path))
            {
                return loadedIcons[path];
            }
            if (_useLargeIcons)
            {
                _largeImageList.Images.Add(GetIcon(path, true));
            }
            if (_useSmallIcons)
            {
                _smallImageList.Images.Add(GetIcon(path, false));
            }
            length++;
            loadedIcons.Add(path, (length - 1));
            return (length - 1);
        }

        public void ApplyToListView(ListView ListView)
        {
            ListView.SmallImageList = SmallIcons;
            ListView.LargeImageList = LargeIcons;
        }

        public static Image GetIcon(string path, bool large)
        {

            var shinfo = new SHFILEINFO();
            var sizeflag = large ? SHGFI_LARGEICON : SHGFI_SMALLICON;

            IntPtr hImg;
            hImg = SHGetFileInfo(path, 0, ref shinfo, Marshal.SizeOf(shinfo), 
                (SHGFI_ICON | sizeflag));
            var icon = System.Drawing.Icon.FromHandle(shinfo.hIcon);
            var image = icon.ToBitmap();
            DestroyIcon(shinfo.hIcon);
            return image;

        }

        [DllImport("shell32.dll")]
        private static extern IntPtr ExtractIcon(IntPtr hInst, string lpszExeFileName, int nIconIndex);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyIcon(IntPtr hIcon);

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SHGetFileInfo(string pszPath, int dwFileAttributes, ref SHFILEINFO psfi, int cbFileInfo, int uFlags);

        private struct SHFILEINFO
        {

            public IntPtr hIcon;

            public int iIcon;

            public int dwAttributes;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        }

        private const int SHGFI_ICON = 0x100;
        private const int SHGFI_SMALLICON = 0x1;
        private const int SHGFI_LARGEICON = 0x0;
        private const int SHGFI_USEFILEATTRIBUTES = 0x10;
        private const int SHGFI_SYSICONINDEX = 0x4000;
        private const int SHGFI_PIDL = 0x8;
        private const int SHGFI_OPENICON = 0x2;



        public void Dispose()
        {
            if (LargeIcons != null) LargeIcons.Dispose();
            if (SmallIcons != null) SmallIcons.Dispose();
        }
    }




}