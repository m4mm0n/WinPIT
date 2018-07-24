using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Engine.Extensions
{
    public class SystemIconsImageList : IDisposable
    {
        #region Public Methods

        /// <summary>
        ///     Returns index of an icon based on FileName. Note: File should exists!
        /// </summary>
        /// <param name="FileName">Name of an existing File or Directory</param>
        /// <returns>Index of an Icon</returns>
        public int GetIconIndex(string FileName)
        {
            var shinfo = new SHFILEINFO();

            var info = new FileInfo(FileName);

            var ext = info.Extension;
            if (string.IsNullOrEmpty(ext))
                if ((info.Attributes & FileAttributes.Directory) != 0)
                    ext = "5EEB255733234c4dBECF9A128E896A1E"; // for directories
                else
                    ext = "F9EB930C78D2477c80A51945D505E9C4"; // for files without extension
            else if (ext.Equals(".exe", StringComparison.InvariantCultureIgnoreCase) ||
                     ext.Equals(".lnk", StringComparison.InvariantCultureIgnoreCase))
                ext = info.Name;

            if (SmallIconsImageList.Images.ContainsKey(ext)) return SmallIconsImageList.Images.IndexOfKey(ext);

            SHGetFileInfo(FileName, 0, ref shinfo, (uint) Marshal.SizeOf(shinfo), SHGFI_ICON | SHGFI_SMALLICON);
            Icon smallIcon;
            try
            {
                smallIcon = Icon.FromHandle(shinfo.hIcon);
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException(string.Format("File \"{0}\" doesn not exist!", FileName), ex);
            }

            SmallIconsImageList.Images.Add(ext, smallIcon);

            SHGetFileInfo(FileName, 0, ref shinfo, (uint) Marshal.SizeOf(shinfo), SHGFI_ICON | SHGFI_LARGEICON);
            var largeIcon = Icon.FromHandle(shinfo.hIcon);
            LargeIconsImageList.Images.Add(ext, largeIcon);

            return SmallIconsImageList.Images.Count - 1;
        }

        #endregion

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
        }

        [DllImport("shell32.dll")]
        public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi,
            uint cbSizeFileInfo, uint uFlags);

        #endregion

        #region Fields

        private bool _disposed;

        #endregion

        #region Properties

        /// <summary>
        ///     Gets System.Windows.Forms.ImageList with small icons in. Assign this property to SmallImageList of ListView,
        ///     TreeView etc.
        /// </summary>
        public ImageList SmallIconsImageList { get; } = new ImageList();

        /// <summary>
        ///     Gets System.Windows.Forms.ImageList with large icons in. Assign this property to LargeImageList of ListView,
        ///     TreeView etc.
        /// </summary>
        public ImageList LargeIconsImageList { get; } = new ImageList();

        /// <summary>
        ///     Gets number of icons were loaded
        /// </summary>
        public int Count => SmallIconsImageList.Images.Count;

        #endregion

        #region Constructor/Destructor

        /// <summary>
        ///     Default constructor
        /// </summary>
        public SystemIconsImageList()
        {
            SmallIconsImageList.ColorDepth = ColorDepth.Depth32Bit;
            SmallIconsImageList.ImageSize = SystemInformation.SmallIconSize;

            LargeIconsImageList.ColorDepth = ColorDepth.Depth32Bit;
            LargeIconsImageList.ImageSize = SystemInformation.IconSize;
        }

        private void CleanUp(bool disposing)
        {
            if (!_disposed)
                if (disposing)
                {
                    SmallIconsImageList.Dispose();
                    LargeIconsImageList.Dispose();
                }

            _disposed = true;
        }

        /// <summary>
        ///     Performs resource cleaning
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
    }
}