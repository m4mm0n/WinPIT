using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace Engine.ProcessCore
{
    public static class Exts
    {
        public static string GetHex(this byte[] bits)
        {
            var sb = new StringBuilder();
            foreach (var b in bits)
                sb.Append(b.ToString("X2"));
            return sb.ToString();
        }

        public static string FindDll(string imageName)
        {
            // https://msdn.microsoft.com/en-us/library/7d83bc18.aspx?f=255&MSPPError=-2147217396
            // The Windows system directory. The GetSystemDirectory function retrieves the path of this directory.
            // The Windows directory. The GetWindowsDirectory function retrieves the path of this directory.

            return
                SearchDirectoryForImage(Environment.GetFolderPath(Environment.SpecialFolder.Windows)) ??
                SearchDirectoryForImage(Environment.GetFolderPath(Environment.SpecialFolder.System));

            // HELPER FUNCTION TO FIND IMAGES
            string SearchDirectoryForImage(string directoryPath)
            {
                foreach (var imagePath in Directory.GetFiles(directoryPath, "*.dll"))
                    if (string.Equals(Path.GetFileName(imagePath), imageName,
                        StringComparison.InvariantCultureIgnoreCase))
                        return imagePath;

                return null;
            }
        }

        public static unsafe T GetStructure<T>(byte[] bytes) where T : struct
        {
            var structure = new T();
            fixed (byte* pByte = &bytes[0])
            {
                Unsafe.Copy(ref structure, pByte);
            }

            return structure;
        }

        public static unsafe byte[] GetBytes<T>(T structure) where T : struct
        {
            var arr = new byte[Unsafe.SizeOf<T>()];

            fixed (byte* pByte = &arr[0])
            {
                Unsafe.Copy(pByte, ref structure);
            }

            return arr;
        }

        public static unsafe TDest ReinterpretCast<TSource, TDest>(TSource source)
        {
            var sourceRef = __makeref(source);
            var dest = default(TDest);
            var destRef = __makeref(dest);
            *(IntPtr*) &destRef = *(IntPtr*) &sourceRef;
            return __refvalue(destRef, TDest);
        }

        public static string BytesToReadableValue(this long number)
        {
            var suffixes = new List<string> {" B", " KB", " MB", " GB", " TB", " PB"};

            for (var i = 0; i < suffixes.Count; i++)
            {
                var temp = number / (int) Math.Pow(1024, i + 1);

                if (temp == 0) return number / (int) Math.Pow(1024, i) + suffixes[i];
            }

            return number.ToString();
        }
    }
}