using System;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using Engine.ProcessCore;

namespace Engine.UWP
{
    public static class Helper
    {
        private const uint APPMODEL_ERROR_NO_PACKAGE = 15700;
        private const uint ERROR_SUCCESS = 0;
        private const int ERROR_INSUFFICIENT_BUFFER = 122;
        private const uint SECURITY_MANDATORY_HIGH_RID = 0x00003000;
        private const uint TOKEN_READ = 0x00020008;

        private static readonly Logger log = new Logger(LoggerType.Console_File, "UWP.Helper");

        //static Helper()
        //{
        //    Logger.StartLogger(Environment.UserInteractive ? LoggerType.Console : LoggerType.File, "UWP.Helper");
        //}

        public static unsafe bool IsRunningElevated()
        {
            log.Log(LogType.Normal, "[+] Checking if I am running elevated...");
            // Pre-vista, nothing ran elevated
            if (Environment.OSVersion.Version.Major < 6)
            {
                log.Log(LogType.Warning, "Seems YOU are running ME on a pre-Vista system... Congrats!");
                return false;
            }

            var processToken = WindowsIdentity.GetCurrent().Token;

            uint returnLength;
            if (WinAPI.GetTokenInformation(
                    processToken,
                    25, // TokenIntegrityLevel,
                    IntPtr.Zero, // TokenInformation
                    0, // tokenInformationLength
                    out returnLength) != 0)
            {
                log.Log(LogType.Failure, "Not running elevated :(");
                return false;
            }

            if (Marshal.GetLastWin32Error() != ERROR_INSUFFICIENT_BUFFER)
            {
                log.Log(LogType.Critical,
                    "Not running elevated - and there was a major error: ERROR_INSUFFICIENT_BUFFER");
                return false;
            }

            var length = returnLength;
            var tokenInformation = new byte[length];

            uint integLevel;
            fixed (byte* tokenInformationPtr = tokenInformation)
            {
                if (WinAPI.GetTokenInformation(
                        processToken,
                        25, // TokenIntegrityLevel,
                        (IntPtr) tokenInformationPtr, // TokenInformation
                        length, // tokenInformationLength
                        out returnLength) == 0)
                {
                    log.Log(LogType.Failure, "Not running elevated :(");
                    return false;
                }

                var label = (TOKEN_MANDATORY_LABEL) Marshal.PtrToStructure((IntPtr) tokenInformationPtr,
                    typeof(TOKEN_MANDATORY_LABEL));
                var psid = label.Label.Sid;

                var subAuthCountPtr = WinAPI.GetSidSubAuthorityCount(psid);
                var subAuthCount = Marshal.ReadByte(subAuthCountPtr);
                var subAuthIndex = (uint) (subAuthCount - 1);
                var integLevelPtr = WinAPI.GetSidSubAuthority(psid, subAuthIndex);
                integLevel = (uint) Marshal.ReadInt32(integLevelPtr);
            }


            if (integLevel >= SECURITY_MANDATORY_HIGH_RID)
            {
                log.Log(LogType.Success, "Running elevated!");
                return true;
            }

            log.Log(LogType.Failure, "Not running elevated :(");
            return false;
        }

        public static bool IsProcessUWP(int pid)
        {
            log.Log(LogType.Normal, "[+] Checking if {0} is an UWP package (APPX/EAPPX)...", pid.ToString("X"));
            var tmpHandle = WinAPI.OpenProcess(WinAPI.ProcessAccessFlags.All, false, pid);
            if (tmpHandle != IntPtr.Zero)
            {
                log.Log(LogType.Debug, "Successfully opened {0}'s handle...", pid.ToString("X"));
                uint len = 0;
                var sb = new StringBuilder();
                var ret = WinAPI.GetPackageFamilyName(tmpHandle, ref len, sb);

                if (ret != APPMODEL_ERROR_NO_PACKAGE && len > 0)
                {
                    log.Log(LogType.Success, "Process is in fact an UWP package...");
                    return true;
                }

                log.Log(LogType.Failure, "Process is not an UWP package...");
                return false;
            }

            log.Log(LogType.Error, "Failed to open process: {0}", Marshal.GetLastWin32Error().ToString("X"));
            return false;
        }

        private struct SID_AND_ATTRIBUTES
        {
            public IntPtr Sid;
            public uint Attributes;
        }

        private struct TOKEN_MANDATORY_LABEL
        {
            public SID_AND_ATTRIBUTES Label;
        }
    }
}