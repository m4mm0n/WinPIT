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

namespace Injectors.NtCreateThreadEx
{
    public class Injector : IInjector
    {
        private readonly Logger log = new Logger(LoggerType.Console_File, "Injector.NCTE");

        public string SelfFileName => Path.GetFileName(Assembly.GetExecutingAssembly().Location);

        public string UniqueId => "Injectors.NCTE-" +
                                  QuickExt.GetHash(
                                      Encoding.UTF8.GetBytes(UniqueName +
                                                             Marshal.GetTypeLibGuidForAssembly(
                                                                 Assembly.GetExecutingAssembly())), HashType.MD5);

        public string UniqueName => "Injection by NtCreateThreadEx API";

        public string About => "API: NtCreateThreadEx" + Environment.NewLine +
                               "DLL: ntdll.dll" + Environment.NewLine + Environment.NewLine +
                               "Stealth: None" + Environment.NewLine +
                               "Kernel/System/Normal Access: System" + Environment.NewLine +
                               "Original Author: https://github.com/marcin-chwedczuk/dll-inject";

        public Module InjectedModule { get; set; }

        public IntPtr Inject(Core targetProcess, string filePath)
        {
            //Logger.StartLogger(Environment.UserInteractive ? LoggerType.Console : LoggerType.File, "Injector.NCTE");

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

            var ntdllmod = WinAPI.GetModuleHandleA("ntdll.dll");
            if (ntdllmod == IntPtr.Zero)
            {
                log.Log(LogType.Error, "Cannot retrieve module handle for {0}: {1}",
                    '"' + "ntdll.dll" + '"', Marshal.GetLastWin32Error().ToString("X"));
                return IntPtr.Zero;
            }

            var ntCreateThreadExAddress = WinAPI.GetProcAddress(ntdllmod, "NtCreateThreadEx");
            if (ntCreateThreadExAddress == IntPtr.Zero)
            {
                log.Log(LogType.Error, "Cannot retrieve address handle for {0} in {1}: {2}",
                    '"' + "NtCreateThreadEx" + '"', '"' + "ntdll.dll" + '"',
                    Marshal.GetLastWin32Error().ToString("X"));
                return IntPtr.Zero;
            }

            var ntCreateThreadEx = (NtCreateThreadEx)
                Marshal.GetDelegateForFunctionPointer(ntCreateThreadExAddress, typeof(NtCreateThreadEx));

            int temp1 = 0, temp2 = 0;
            unsafe
            {
                var nb = new NtCreateThreadExBuffer
                {
                    Size = sizeof(NtCreateThreadExBuffer),
                    Unknown1 = 0x10003,
                    Unknown2 = 0x8,
                    Unknown3 = new IntPtr(&temp2),
                    Unknown4 = 0,
                    Unknown5 = 0x10004,
                    Unknown6 = 4,
                    Unknown7 = new IntPtr(&temp1),
                    Unknown8 = 0
                };

                var hRemoteThread = IntPtr.Zero;
                ntCreateThreadEx?.Invoke(
                    out hRemoteThread,
                    0x1FFFFF,
                    IntPtr.Zero,
                    targetProcess.ProcessHandle,
                    loadLib,
                    alloc,
                    0,
                    0,
                    Environment.Is64BitProcess ? 0xFFFF : 0u,
                    Environment.Is64BitProcess ? 0xFFFF : 0u,
                    Environment.Is64BitProcess ? IntPtr.Zero : new IntPtr(&nb)
                );

                if (hRemoteThread == IntPtr.Zero)
                    log.Log(LogType.Failure, "Failed to create thread inside attached process: {0}",
                        Marshal.GetLastWin32Error().ToString("X"));
                else
                    log.Log(LogType.Success, "Thread created succesfully inside attached process: 0x{0}",
                        Environment.Is64BitProcess
                            ? hRemoteThread.ToInt64().ToString("X")
                            : hRemoteThread.ToInt32().ToString("X"));

                return hRemoteThread;
            }
        }

        private delegate int NtCreateThreadEx(
            out IntPtr threadHandle,
            uint desiredAccess,
            IntPtr objectAttributes,
            IntPtr processHandle,
            IntPtr lpStartAddress,
            IntPtr lpParameter,
            int createSuspended,
            uint stackZeroBits,
            uint sizeOfStackCommit,
            uint sizeOfStackReserve,
            IntPtr lpBytesBuffer);

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct NtCreateThreadExBuffer
        {
            public int Size;
            public uint Unknown1;
            public uint Unknown2;
            public IntPtr Unknown3;
            public uint Unknown4;
            public uint Unknown5;
            public uint Unknown6;
            public IntPtr Unknown7;
            public uint Unknown8;
        }
    }
}