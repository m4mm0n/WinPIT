using System;
using System.Security.Cryptography;
using Engine.ProcessCore;

namespace Engine.Extensions
{
    public enum HashType
    {
        MD5,
        RIPEMD160,
        SHA1,
        SHA256,
        SHA384,
        SHA512
    }

    public enum CRCType
    {
        CRC8,
        CRC16,
        CRC16_ModBus,
        CRC16_Sick,
        CRC_CCITT_XModem,
        CRC_CCITT_0xFFFF,
        CRC_CCITT_0x1D0F,
        CRC_CCITT_Kermit,
        CRC_DNP,
        CRC32
    }

    public static class QuickExt
    {
        private static readonly Logger log = new Logger(LoggerType.Console_File, "Extensions");


        public static string GetHash(byte[] toGetHashFrom, HashType hType)
        {
            byte[] hashed = null;
            var hashedHex = string.Empty;
            switch (hType)
            {
                case HashType.MD5:
                    hashed = new MD5CryptoServiceProvider().ComputeHash(toGetHashFrom);
                    hashedHex = hashed.GetHex();
                    break;
                case HashType.RIPEMD160:
                    hashed = new RIPEMD160Managed().ComputeHash(toGetHashFrom);
                    hashedHex = hashed.GetHex();
                    break;
                case HashType.SHA1:
                    hashed = new SHA1CryptoServiceProvider().ComputeHash(toGetHashFrom);
                    hashedHex = hashed.GetHex();
                    break;
                case HashType.SHA256:
                    hashed = new SHA256CryptoServiceProvider().ComputeHash(toGetHashFrom);
                    hashedHex = hashed.GetHex();
                    break;
                case HashType.SHA384:
                    hashed = new SHA384CryptoServiceProvider().ComputeHash(toGetHashFrom);
                    hashedHex = hashed.GetHex();
                    break;
                case HashType.SHA512:
                    hashed = new SHA512CryptoServiceProvider().ComputeHash(toGetHashFrom);
                    hashedHex = hashed.GetHex();
                    break;
            }

            return hashedHex;
        }

        public static bool ParseFromString(this string toParseFrom)
        {
            if ((toParseFrom == "1") | (toParseFrom.ToLower() == "true"))
                return true;
            if ((toParseFrom == "0") | (toParseFrom.ToLower() == "false"))
                return false;

            log.Log(LogType.Warning, "Parsed string is not a valid string: {0}{1}Only valid is: 1 or TRUE, 0 or FALSE",
                toParseFrom, Environment.NewLine);
            return false;
        }
    }
}