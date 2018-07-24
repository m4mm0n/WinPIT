using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Engine.ProcessCore
{
    public class Core : IDisposable
    {
        private readonly Process _proc;
        private readonly Dictionary<IntPtr, uint> allocatedMem;
        private readonly Logger log;

        public Core(Process process) : this(process.Id)
        {
        }

        public Core(int processId)
        {
            log = new Logger(LoggerType.Console_File, "ProcessCore.Core");
            log.Log("[+] Initiating Core on process ID: {0}", processId.ToString("X"));

            allocatedMem = new Dictionary<IntPtr, uint>();

            ProcessId = processId;
            _proc = Process.GetProcessById(processId);
            _proc.Exited += _proc_Exited;
            LoadProcess(processId);
            if (ProcessHandle == IntPtr.Zero)
                Dispose();
        }

        public IntPtr ProcessHandle { get; private set; }

        public int ProcessId { get; }

        public IntPtr BaseAddress => _proc.MainModule.BaseAddress;
        public int SizeOfProcess => _proc.MainModule.ModuleMemorySize;
        public ProcessModuleCollection LoadedModules => _proc.Modules;

        public string FileName => Path.GetFileName(_proc.MainModule.FileName);
        public string ProcessName => _proc.ProcessName;

        public string ProcessOwner => _proc.MachineName;
        public string ProcessStatus => _proc.Responding ? "Running" : "Not Running";
        public string ProcessPriority => Enum.GetName(typeof(ProcessPriorityClass), _proc.PriorityClass);

        public string ProcessMemoryUsage => ExactMemUsage() + " MB";
        public string ProcessTitle => _proc.MainWindowTitle;

        public bool Is64bit => is64bitProc();

        public void Dispose()
        {
            log.Debug("Dispose");

            if (ProcessHandle != IntPtr.Zero)
            {
                if (allocatedMem.Count > 0)
                    foreach (var b in allocatedMem)
                        WinAPI.VirtualFreeEx(ProcessHandle, b.Key, (int) b.Value, 0x4000);

                WinAPI.CloseHandle(ProcessHandle);
            }

            allocatedMem.Clear();
            log.Dispose();
        }

        public void SetDebugToken()
        {
            log.Debug("SetDebugToken");
            Tokenizer.SetProcessDebugToken(ProcessId);
        }

        public void ElevateSelf()
        {
            log.Debug("ElevateSelf");
            Tokenizer.ImpersonateSystem();
        }

        public void ElevateProcess()
        {
            log.Debug("ElevateProcess");
            Tokenizer.ElevateProcessToSystem(ProcessId);
        }

        private void _proc_Exited(object sender, EventArgs e)
        {
            log.Log(LogType.Warning, "Attached process has exited - disposing!");
            Dispose();
        }

        public IntPtr GetLoadLibraryPtr()
        {
            log.Debug("GetLoadLibraryPtr");

            var a = WinAPI.GetModuleHandleA("kernel32.dll");
            if (a != IntPtr.Zero)
            {
                log.Log(LogType.Debug, "Module Handle for {0} retrieved: 0x{1}", '"' + "kernel32.dll" + '"',
                    Environment.Is64BitProcess ? a.ToInt64().ToString("X16") : a.ToInt32().ToString("X8"));

                var b = WinAPI.GetProcAddress(a, "LoadLibraryA");
                if (b != IntPtr.Zero)
                {
                    log.Log(LogType.Debug, "Address Handle for {0} retrieved: 0x{1}", '"' + "LoadLibraryA" + '"',
                        Environment.Is64BitProcess ? b.ToInt64().ToString("X16") : b.ToInt32().ToString("X8"));

                    return b;
                }

                log.Log(LogType.Failure, "Failed to find {0} in {1}: {2}", '"' + "LoadLibraryA" + '"',
                    '"' + "kernel32.dll" + '"', Marshal.GetLastWin32Error().ToString("X"));

                return IntPtr.Zero;
            }

            log.Log(LogType.Failure, "Failed to get module handle for {0}: {1}", '"' + "kernel32.dll" + '"',
                Marshal.GetLastWin32Error().ToString("X"));
            return IntPtr.Zero;
        }

        public bool WriteString(string toWrite, IntPtr addrToWriteTo, bool Unicode = true)
        {
            log.Debug("WriteString");

            var tmpBytes = Unicode ? Encoding.Unicode.GetBytes(toWrite) : Encoding.ASCII.GetBytes(toWrite);
            uint bytesWritten = 0;
            if (WinAPI.WriteProcessMemory(ProcessHandle, addrToWriteTo, tmpBytes, tmpBytes.Length, out bytesWritten))
                if (bytesWritten == tmpBytes.Length)
                {
                    log.Log(LogType.Success, "Successfully wrote {0} to 0x{1}", toWrite,
                        Environment.Is64BitProcess
                            ? addrToWriteTo.ToInt64().ToString("X16")
                            : addrToWriteTo.ToInt32().ToString("X8"));
                    return true;
                }
                else
                {
                    log.Log(LogType.Warning, "Partially successfully wrote {0} to 0x{1}: {2}", toWrite,
                        Environment.Is64BitProcess
                            ? addrToWriteTo.ToInt64().ToString("X16")
                            : addrToWriteTo.ToInt32().ToString("X8"), Marshal.GetLastWin32Error().ToString("X"));

                    return true;
                }

            log.Log(LogType.Failure, "Failed to write {0} to 0x{1}: {2}", toWrite,
                Environment.Is64BitProcess
                    ? addrToWriteTo.ToInt64().ToString("X16")
                    : addrToWriteTo.ToInt32().ToString("X8"), Marshal.GetLastWin32Error().ToString("X"));

            return false;
        }

        public bool WriteBytes(byte[] toWrite, IntPtr addrToWriteTo)
        {
            log.Debug("WriteBytes");

            uint bytesWritten = 0;
            if (WinAPI.WriteProcessMemory(ProcessHandle, addrToWriteTo, toWrite, toWrite.Length, out bytesWritten))
                if (bytesWritten == toWrite.Length)
                {
                    log.Log(LogType.Success, "Successfully wrote {0} to 0x{1}", toWrite.GetHex(),
                        Environment.Is64BitProcess
                            ? addrToWriteTo.ToInt64().ToString("X16")
                            : addrToWriteTo.ToInt32().ToString("X8"));
                    return true;
                }
                else
                {
                    log.Log(LogType.Warning, "Partially successfully wrote {0} to 0x{1}: {2}", toWrite.GetHex(),
                        Environment.Is64BitProcess
                            ? addrToWriteTo.ToInt64().ToString("X16")
                            : addrToWriteTo.ToInt32().ToString("X8"), Marshal.GetLastWin32Error().ToString("X"));

                    return true;
                }

            log.Log(LogType.Failure, "Failed to write {0} to 0x{1}: {2}", toWrite.GetHex(),
                Environment.Is64BitProcess
                    ? addrToWriteTo.ToInt64().ToString("X16")
                    : addrToWriteTo.ToInt32().ToString("X8"), Marshal.GetLastWin32Error().ToString("X"));

            return false;
        }

        public byte[] ReadBytes(IntPtr addrToReadFrom, int size)
        {
            log.Debug("ReadBytes");

            uint bytesRead = 0;
            var bytes = new byte[size];
            if (WinAPI.ReadProcessMemory(ProcessHandle, addrToReadFrom, bytes, size, out bytesRead))
                if (bytesRead == size)
                {
                    log.Log(LogType.Success, "Successfully read {0} bytes from 0x{1}", size.ToString(),
                        Environment.Is64BitProcess
                            ? addrToReadFrom.ToInt64().ToString("X16")
                            : addrToReadFrom.ToInt32().ToString("X8"));
                    return bytes;
                }
                else
                {
                    log.Log(LogType.Warning,
                        "Partially successfully read {0} bytes from 0x{1} (Only read {2} bytes!): {3}",
                        size,
                        Environment.Is64BitProcess
                            ? addrToReadFrom.ToInt64().ToString("X16")
                            : addrToReadFrom.ToInt32().ToString("X8"), bytesRead.ToString(),
                        Marshal.GetLastWin32Error().ToString("X"));

                    return bytes;
                }

            log.Log(LogType.Failure, "Failed to read {0} bytes from 0x{1}: {2}", size.ToString(),
                Environment.Is64BitProcess
                    ? addrToReadFrom.ToInt64().ToString("X16")
                    : addrToReadFrom.ToInt32().ToString("X8"), Marshal.GetLastWin32Error().ToString("X"));

            return null;
        }

        public T Read<T>(IntPtr addrToReadFrom) where T : struct
        {
            log.Debug("Read (struct)");

            var buf = ReadBytes(addrToReadFrom, Unsafe.SizeOf<T>());
            if (buf != null && buf.Length > 0)
            {
                log.Log(LogType.Success, "Read from 0x{0} into a structure as specified - completed...",
                    Is64bit ? addrToReadFrom.ToInt64().ToString("X16") : addrToReadFrom.ToInt32().ToString("X8"));
                return Exts.GetStructure<T>(buf);
            }

            log.Log(LogType.Failure, "Failed to read from 0x{0} into a structure as specified: {1}",
                Is64bit ? addrToReadFrom.ToInt64().ToString("X16") : addrToReadFrom.ToInt32().ToString("X8"),
                Marshal.GetLastWin32Error().ToString("X"));
            return new T();
        }

        public void Write<T>(T value, IntPtr memoryPointer) where T : struct
        {
            log.Debug("Write (struct)");

            var buf = Exts.GetBytes(value);
            if (buf != null && buf.Length > 0)
                try
                {
                    if (WriteBytes(buf, memoryPointer))
                        log.Log(LogType.Success, "Wrote {0} from structure as specified...",
                            Exts.BytesToReadableValue(buf.Length));
                    else
                        log.Log(LogType.Failure, "Failed to write bytes to memory: {0}",
                            Marshal.GetLastWin32Error().ToString("X"));
                }
                catch (Exception ex)
                {
                    log.Log(ex, "Failed on writing structure to memory...");
                }
            else
                log.Log(LogType.Failure, "Failed to return an array of bytes of the given structure: {0}",
                    Marshal.GetLastWin32Error().ToString("X"));
        }

        public ulong GetPebAddress()
        {
            log.Debug("GetPebAddress");

            unsafe
            {
                var pbi = new WinAPI.PROCESS_BASIC_INFORMATION();
                WinAPI.NtQueryInformationProcess(ProcessHandle, 0, &pbi, pbi.Size, IntPtr.Zero);

                return pbi.PebBaseAddress;
            }
        }

        public WinAPI._PEB_LDR_DATA GetLoaderData()
        {
            var peb = Read<WinAPI._PEB>((IntPtr) GetPebAddress());
            return Read<WinAPI._PEB_LDR_DATA>((IntPtr) peb.Ldr);
        }

        public void WriteLoaderData(WinAPI._PEB_LDR_DATA ldrData)
        {
            var peb = Read<WinAPI._PEB>((IntPtr) GetPebAddress());
            Write(ldrData, (IntPtr) peb.Ldr);
        }

        public IntPtr Allocate(int size)
        {
            log.Debug("Allocate (+int)");

            return Allocate((uint) size);
        }

        public IntPtr Allocate(uint size)
        {
            log.Debug("Allocate (+uint)");

            var tmp = WinAPI.VirtualAllocEx(ProcessHandle, IntPtr.Zero, size,
                WinAPI.AllocationType.Commit | WinAPI.AllocationType.Reserve, WinAPI.MemoryProtection.ExecuteReadWrite);

            if (tmp != IntPtr.Zero)
            {
                allocatedMem.Add(tmp, size);

                log.Log(LogType.Success, "Allocation of Memory to: 0x{0}",
                    Environment.Is64BitProcess ? tmp.ToInt64().ToString("X16") : tmp.ToInt32().ToString("X8"));
                return tmp;
            }

            log.Log(LogType.Failure, "Allocation of Memory failed: {0}", Marshal.GetLastWin32Error().ToString("X"));
            return tmp;
        }

        public IntPtr AllocateAndWriteBytes(byte[] bytesToWrite)
        {
            log.Debug("AllocateAndWriteBytes");

            var alloc = Allocate(bytesToWrite.Length);
            if (alloc == IntPtr.Zero)
            {
                log.Log(LogType.Failure, "Allocation of memory failed - aborting!");
                return IntPtr.Zero;
            }

            if (!WriteBytes(bytesToWrite, alloc))
            {
                log.Log(LogType.Failure, "Writing bytes to allocated memory failed - aborting!");
                return IntPtr.Zero;
            }

            return alloc;
        }

        public IntPtr CreateThread(IntPtr startAddr)
        {
            log.Debug("CreateThread (+1)");

            return CreateThread(startAddr, IntPtr.Zero);
        }

        public IntPtr CreateThread(IntPtr startAddr, IntPtr param)
        {
            log.Debug("CreateThread (+2)");

            return CreateThread(IntPtr.Zero, 0, startAddr, param, 0);
        }

        public IntPtr CreateThread(IntPtr threadAttributes, uint stackSize, IntPtr startAddr, IntPtr param,
            uint creationFlags)
        {
            log.Debug("CreateThread (+5)");

            var threadId = 0;
            var tmp = WinAPI.CreateRemoteThread(ProcessHandle, threadAttributes, stackSize, startAddr, param,
                creationFlags,
                out threadId);
            if (tmp != IntPtr.Zero)
                log.Log(LogType.Success, "CreateThread was a success: 0x{0}",
                    Environment.Is64BitProcess ? tmp.ToInt64().ToString("X16") : tmp.ToInt32().ToString("X8"));
            else
                log.Log(LogType.Failure, "Failed to CreateThread on process: {0}",
                    Marshal.GetLastWin32Error().ToString("X"));

            return tmp;
        }

        private long ExactMemUsage()
        {
            log.Debug("ExactMemUsage");

            long memsize = 0;
            var pc = new PerformanceCounter("Process", "Working Set - Private", _proc.ProcessName);
            memsize = (long) (pc.NextValue() / (1024 * 1024));

            return memsize;
        }

        private bool is64bitProc()
        {
            log.Debug("is64bitProc");

            var is64 = false;
            var m = WinAPI.IsWow64Process(ProcessHandle, out is64);
            return is64;
        }

        private void LoadProcess(int procId)
        {
            log.Debug("LoadProcess");

            try
            {
                Tokenizer.Initiate();

                ProcessHandle = WinAPI.OpenProcess(
                    WinAPI.ProcessAccessFlags.All | WinAPI.ProcessAccessFlags.CreateProcess |
                    WinAPI.ProcessAccessFlags.CreateThread | WinAPI.ProcessAccessFlags.QueryInformation |
                    WinAPI.ProcessAccessFlags.VirtualMemoryOperation | WinAPI.ProcessAccessFlags.VirtualMemoryRead |
                    WinAPI.ProcessAccessFlags.VirtualMemoryWrite, false, procId);
                if (ProcessHandle != IntPtr.Zero)
                {
                    log.Log(LogType.Success, "Process Opened succesfully!");
                    SetDebugToken();
                    //hProcess = tmp;
                }
                else
                {
                    log.Log(LogType.Failure, "Failed to OpenProcess from {0}: {1}", procId.ToString("X"),
                        Marshal.GetLastWin32Error().ToString("X"));
                }
            }
            catch (Exception ex)
            {
                log.Log(LogType.Exception, "LoadProcess Exception: {0} ({1})", ex.Message,
                    Marshal.GetLastWin32Error().ToString("X"));
            }
        }
    }
}