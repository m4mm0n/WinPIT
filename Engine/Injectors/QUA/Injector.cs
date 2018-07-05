using System;
using System.Runtime.InteropServices;
using System.Text;
using Engine.ProcessCore;

namespace Engine.Injectors.QUA
{
    public class Injector : IInjector
    {
        private Logger log = new Logger(LoggerType.Console_File, "Injector.QUA");

        public string About
        {
            get
            {
                return
                    "API: QueueUserAPC" + Environment.NewLine +
                    "DLL: kernel32.dll" + Environment.NewLine + Environment.NewLine +
                    "Stealth: None" + Environment.NewLine +
                    "Kernel/System/Normal Access: Normal" + Environment.NewLine +
                    "Original Author: https://github.com/LordNoteworthy/al-khaser"
                    ;
            }
        }

        public Module InjectedModule { get; set; }
        public IntPtr Inject(Core targetProcess, string filePath)
        {
            //Logger.StartLogger(Environment.UserInteractive ? LoggerType.Console : LoggerType.File, "Injector.QUA");

            InjectedModule = new Module(filePath);

            var loadLib = targetProcess.GetLoadLibraryPtr();
            if (loadLib == IntPtr.Zero)
            {
                log.Log(LogType.Error, "Cannot retrieve LoadLibraryA pointer - aborting!");
                return IntPtr.Zero;
            }
            var pathBytes = Encoding.Unicode.GetBytes(filePath);

            var alloc = targetProcess.Allocate(pathBytes.Length);
            if (alloc == IntPtr.Zero)
            {
                log.Log(LogType.Error, "Cannot allocate memory in attached process - aborting!");
                return IntPtr.Zero;
            }

            if (!targetProcess.WriteBytes(pathBytes, alloc))
            {
                log.Log(LogType.Error, "Cannot write file-path to memory - aborting!");
                return IntPtr.Zero;
            }

            uint dwThreadId = 0;
            IntPtr hThread = IntPtr.Zero;
            dwThreadId = ProcessCore.WinAPI.GetProcessThreadId(targetProcess.ProcessId, hThread);

            var dResult = ProcessCore.WinAPI.QueueUserAPC(loadLib, hThread, alloc);
            if (dResult == 0)
            {
                log.Log(LogType.Error, "Failed to QueueUserAPC: {0}", Marshal.GetLastWin32Error().ToString("X"));
                return IntPtr.Zero;
            }

            log.Log(LogType.Success, "Injection through QueueUserAPC was a success - opened thread: 0x{0}",
                (Environment.Is64BitProcess ? hThread.ToInt64().ToString("X16") : hThread.ToInt32().ToString("X8")));
            return hThread;
        }
    }
}
