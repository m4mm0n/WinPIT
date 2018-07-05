using System;
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
    }
}
