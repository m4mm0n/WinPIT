using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Engine.Extensions
{
    public class SystemIconsImageList : IDisposable
    {
        #region Win32 declarations
        private const uint SHGFI_ICON = 0x100;
        private const uint SHGFI_LARGEICON = 0x0;
        private const uint SHGFI_SMALLICON = 0x1;

        [StructLayout(LayoutKind.Sequential)]
        public struct SHFILEINFO
        {
            public IntPtr hIcon;
            public IntPtr iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        };

        [DllImport("shell32.dll")]
        public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);
        #endregion

        #region Fields
        private ImageList _smallImageList = new ImageList();
        private ImageList _largeImageList = new ImageList();
        private bool _disposed = false;
        #endregion

        #region Properties
        /// <summary>
        /// Gets System.Windows.Forms.ImageList with small icons in. Assign this property to SmallImageList of ListView, TreeView etc.
        /// </summary>
        public ImageList SmallIconsImageList
        {
            get { return _smallImageList; }
        }

        /// <summary>
        /// Gets System.Windows.Forms.ImageList with large icons in. Assign this property to LargeImageList of ListView, TreeView etc.
        /// </summary>
        public ImageList LargeIconsImageList
        {
            get { return _largeImageList; }
        }

        /// <summary>
        /// Gets number of icons were loaded
        /// </summary>
        public int Count
        {
            get { return _smallImageList.Images.Count; }
        }
        #endregion

        #region Constructor/Destructor
        /// <summary>
        /// Default constructor
        /// </summary>
        public SystemIconsImageList()
            : base()
        {
            _smallImageList.ColorDepth = ColorDepth.Depth32Bit;
            _smallImageList.ImageSize = SystemInformation.SmallIconSize;

            _largeImageList.ColorDepth = ColorDepth.Depth32Bit;
            _largeImageList.ImageSize = SystemInformation.IconSize;
        }

        private void CleanUp(bool disposing)
        {
            if (!this._disposed)
            {
                if (disposing)
                {
                    _smallImageList.Dispose();
                    _largeImageList.Dispose();
                }
            }
            _disposed = true;
        }

        /// <summary>
        /// Performs resource cleaning
        /// </summary>
        public void Dispose()
        {
            CleanUp(true);
            GC.SuppressFinalize(this);
        }

        ~SystemIconsImageList()
        {
            CleanUp(false);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Returns index of an icon based on FileName. Note: File should exists!
        /// </summary>
        /// <param name="FileName">Name of an existing File or Directory</param>
        /// <returns>Index of an Icon</returns>
        public int GetIconIndex(string FileName)
        {
            SHFILEINFO shinfo = new SHFILEINFO();

            FileInfo info = new FileInfo(FileName);

            string ext = info.Extension;
            if (String.IsNullOrEmpty(ext))
            {
                if ((info.Attributes & FileAttributes.Directory) != 0)
                    ext = "5EEB255733234c4dBECF9A128E896A1E"; // for directories
                else
                    ext = "F9EB930C78D2477c80A51945D505E9C4"; // for files without extension
            }
            else
                if (ext.Equals(".exe", StringComparison.InvariantCultureIgnoreCase) ||
                    ext.Equals(".lnk", StringComparison.InvariantCultureIgnoreCase))
                ext = info.Name;

            if (_smallImageList.Images.ContainsKey(ext))
            {
                return _smallImageList.Images.IndexOfKey(ext);
            }
            else
            {
                SHGetFileInfo(FileName, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), SHGFI_ICON | SHGFI_SMALLICON);
                Icon smallIcon;
                try
                {
                    smallIcon = Icon.FromHandle(shinfo.hIcon);
                }
                catch (ArgumentException ex)
                {
                    throw new ArgumentException(String.Format("File \"{0}\" doesn not exist!", FileName), ex);
                }
                _smallImageList.Images.Add(ext, smallIcon);

                SHGetFileInfo(FileName, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), SHGFI_ICON | SHGFI_LARGEICON);
                Icon largeIcon = Icon.FromHandle(shinfo.hIcon);
                _largeImageList.Images.Add(ext, largeIcon);

                return _smallImageList.Images.Count - 1;
            }
        }
        #endregion
    }

}
