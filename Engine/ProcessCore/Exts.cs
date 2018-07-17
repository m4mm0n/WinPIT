using System;
using System.Collections.Generic;
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

        public static unsafe TDest ReinterpretCast<TSource, TDest>(TSource source)
        {
            var sourceRef = __makeref(source);
            var dest = default(TDest);
            var destRef = __makeref(dest);
            *(IntPtr*)&destRef = *(IntPtr*)&sourceRef;
            return __refvalue(destRef, TDest);
        }

        public static string BytesToReadableValue(this long number)
        {
            List<string> suffixes = new List<string> { " B", " KB", " MB", " GB", " TB", " PB" };

            for (int i = 0; i < suffixes.Count; i++)
            {
                long temp = number / (int)Math.Pow(1024, i + 1);

                if (temp == 0)
                {
                    return (number / (int)Math.Pow(1024, i)) + suffixes[i];
                }
            }

            return number.ToString();
        }
    }
}
