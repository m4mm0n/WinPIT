using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Engine;
using Engine.Extensions;
using Engine.Injectors;
using Engine.ProcessCore;
using Module = Engine.ProcessCore.Module;

namespace Injectors.RtlCreateUserThread
{
    public class Injector : IInjector
    {
        private Logger log = new Logger(LoggerType.Console_File, "Injector.RCUT");

        public string SelfFileName
        {
            get { return Path.GetFileName(Assembly.GetExecutingAssembly().Location); }
        }

        public string UniqueId
        {
            get
            {
                return "Injectors.RCUT-" + QuickExt.GetHash(
                           Encoding.UTF8.GetBytes(UniqueName +
                                                  Marshal.GetTypeLibGuidForAssembly(Assembly.GetExecutingAssembly())
                                                      .ToString()), HashType.MD5);
            }
        }

        public string UniqueName
        {
            get { return "Injection by RtlCreateUserThread API"; }
        }

        public string About
        {
            get
            {
                return
                    "API: RtlCreateUserThread" + Environment.NewLine +
                    "DLL: ntdll.dll" + Environment.NewLine + Environment.NewLine +
                    "Stealth: None" + Environment.NewLine +
                    "Kernel/System/Normal Access: System" + Environment.NewLine +
                    "Original Author: https://github.com/LordNoteworthy/al-khaser"
                    ;
            }
        }
        public Module InjectedModule { get; set; }
        public IntPtr Inject(Core targetProcess, string filePath)
        {
            //Logger.StartLogger(Environment.UserInteractive ? LoggerType.Console : LoggerType.File, "Injector.RCUT");

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

            var ntdllmod = Engine.ProcessCore.WinAPI.GetModuleHandleA("ntdll.dll");
            if (ntdllmod == IntPtr.Zero)
            {
                log.Log(LogType.Error, "Cannot retrieve module handle for {0}: {1}",
                    ('"' + "ntdll.dll" + '"'), Marshal.GetLastWin32Error().ToString("X"));
                return IntPtr.Zero;
            }

            var rtlCreateUserThreadAddress = Engine.ProcessCore.WinAPI.GetProcAddress(ntdllmod, "RtlCreateUserThread");
            if (rtlCreateUserThreadAddress == IntPtr.Zero)
            {
                log.Log(LogType.Error, "Cannot retrieve address handle for {0} in {1}: {2}",
                    ('"' + "RtlCreateUserThread" + '"'), ('"' + "ntdll.dll" + '"'),
                    Marshal.GetLastWin32Error().ToString("X"));
                return IntPtr.Zero;
            }

            var rtlCreateUserThread =
                (RtlCreateUserThread)Marshal.GetDelegateForFunctionPointer(rtlCreateUserThreadAddress,
                    typeof(RtlCreateUserThread));
            IntPtr hRemoteThread = IntPtr.Zero;
            if (rtlCreateUserThread(targetProcess.ProcessHandle, IntPtr.Zero, 0, 0, 0, 0, loadLib, alloc,
                    out hRemoteThread,
                    IntPtr.Zero) < 0)
                log.Log(LogType.Failure, "Failed to create thread inside attached process: {0}",
                    Marshal.GetLastWin32Error().ToString("X"));
            else
                log.Log(LogType.Success, "Thread created succesfully inside attached process: 0x{0}",
                    (Environment.Is64BitProcess
                        ? hRemoteThread.ToInt64().ToString("X")
                        : hRemoteThread.ToInt32().ToString("X")));

            return hRemoteThread;
        }

        delegate int RtlCreateUserThread(
            IntPtr ProcessHandle,
            IntPtr SecurityDescriptor,
            uint CreateSuspended,
            ulong StackZeroBits,
            ulong StackReserved,
            ulong StackCommit,
            IntPtr StartAddress,
            IntPtr StartParameter,
            out IntPtr ThreadHandle,
            IntPtr ClientID
        );
    }
}
