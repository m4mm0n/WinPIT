using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Engine.ProcessCore
{
    public class Module : IDisposable
    {
        public IntPtr hModule;
        public string DllName;

        private Dictionary<string, IntPtr> locatedExports;
        private Logger log = new Logger(LoggerType.Console_File, "ProcessCore.Module");

        public Module(string dllToLocate)
        {
            //StartLogger();
            log.Log(LogType.Normal, "[+] Initiating Module from file: {0}", Path.GetFileName(dllToLocate));

            if (File.Exists(dllToLocate))
            {
                var tmp = WinAPI.GetModuleHandleA(dllToLocate);
                if (tmp != IntPtr.Zero)
                    LoadInternal(tmp);
                else
                {
                    log.Log(LogType.Error, "Failed to GetModuleHandle from {0}: {1}", Path.GetFileName(dllToLocate), Marshal.GetLastWin32Error().ToString("X"));
                    this.Dispose();
                }
            }
            else
            {
                log.Log(LogType.Error, "File does not exist: {0}", Path.GetFileName(dllToLocate));
                this.Dispose();
            }
        }
        public Module(IntPtr loadedMod)
        {
            //StartLogger();
            log.Log(LogType.Normal, "[+] Initiating Module from pointer: 0x{0}",
                (Environment.Is64BitProcess
                    ? loadedMod.ToInt64().ToString("X")
                    : loadedMod.ToInt32().ToString("X")));

            LoadInternal(loadedMod);
        }

        void LoadInternal(IntPtr newModule)
        {
            if (newModule != IntPtr.Zero)
            {
                log.Log(LogType.Debug, "newModule != NULL");
                hModule = newModule;
                locatedExports = new Dictionary<string, IntPtr>();
                StringBuilder sb = new StringBuilder(255);
                uint res = WinAPI.GetModuleFileName(hModule, sb, sb.Capacity);
                if (res != 0)
                    DllName = sb.ToString();
                else
                {
                    DllName = "!Unresolved!";
                    log.Log(LogType.Failure, "Failed to aquire the file name of the module: {0}", Marshal.GetLastWin32Error().ToString("X"));
                }
            }
            else
            {
                log.Log(LogType.Debug, "newModule == NULL");
                log.Log(LogType.Error, "Module cannot be initiated with an empty pointer!");
                this.Dispose();
            }
        }

        public IntPtr GetExportAddress(string exportToLocate)
        {
            log.Log(LogType.Normal, "[+] GetExportAddress of {0}", exportToLocate);

            //Just in-case...
            if (hModule != IntPtr.Zero)
            {
                if (locatedExports.ContainsKey(exportToLocate))
                {
                    log.Log(LogType.Success, "Export address for {0} found: 0x{1}", exportToLocate,
                        (Environment.Is64BitProcess
                            ? locatedExports[exportToLocate].ToInt64().ToString("X16")
                            : locatedExports[exportToLocate].ToInt32().ToString("X8")));
                    return locatedExports[exportToLocate];
                }

                var tmp = WinAPI.GetProcAddress(hModule, exportToLocate);
                if (tmp != IntPtr.Zero)
                {
                    log.Log(LogType.Success, "Export address for {0} found: 0x{1}", exportToLocate,
                        (Environment.Is64BitProcess ? tmp.ToInt64().ToString("X16") : tmp.ToInt32().ToString("X8")));
                    locatedExports.Add(exportToLocate, tmp);
                    return tmp;
                }

                log.Log(LogType.Failure, "Failed to locate address for {0}: {1}", exportToLocate,
                    Marshal.GetLastWin32Error().ToString("X"));
                return IntPtr.Zero;
            }

            log.Log(LogType.Critical, "Module is no longer present!");
            return IntPtr.Zero;
        }


        public void Dispose()
        {
            if (hModule != IntPtr.Zero)
                WinAPI.CloseHandle(hModule);
            log.Dispose();
        }
    }
}
