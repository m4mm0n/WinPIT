using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Engine.ProcessCore
{
    public class Core : IDisposable
    {
        private Dictionary<IntPtr, uint> allocatedMem;
        private IntPtr hProcess;
        private int procId;
        private Logger log;

        public IntPtr ProcessHandle => hProcess;
        public int ProcessId => procId;

        public void SetDebugToken()
        {
            Tokenizer.SetProcessDebugToken(procId);
        }

        public void ElevateSelf()
        {
            Tokenizer.ImpersonateSystem();
        }

        public void ElevateProcess()
        {
            Tokenizer.ElevateProcessToSystem(procId);
        }

        public Core(Process process) : this(process.Id)
        { }
        public Core(int processId)
        {
            //Logger.StartLogger(Environment.UserInteractive ? LoggerType.Console : LoggerType.File, "ProcessCore.Core");
            log = new Logger(LoggerType.Console_File, "ProcessCore.Core");
            log.Log("[+] Initiating Core on process ID: {0}", processId.ToString("X"));

            allocatedMem = new Dictionary<IntPtr, uint>();

            procId = processId;
            LoadProcess(processId);
            if (hProcess == IntPtr.Zero)
                this.Dispose(); 
        }

        public IntPtr GetLoadLibraryPtr()
        {
            var a = WinAPI.GetModuleHandleA("kernel32.dll");
            if (a != IntPtr.Zero)
            {
                log.Log(LogType.Debug, "Module Handle for {0} retrieved: 0x{1}", ('"' + "kernel32.dll" + '"'),
                    (Environment.Is64BitProcess ? a.ToInt64().ToString("X16") : a.ToInt32().ToString("X8")));

                var b = WinAPI.GetProcAddress(a, "LoadLibraryA");
                if (b != IntPtr.Zero)
                {
                    log.Log(LogType.Debug, "Address Handle for {0} retrieved: 0x{1}", ('"' + "LoadLibraryA" + '"'),
                        (Environment.Is64BitProcess ? b.ToInt64().ToString("X16") : b.ToInt32().ToString("X8")));

                    return b;
                }

                log.Log(LogType.Failure, "Failed to find {0} in {1}: {2}", ('"' + "LoadLibraryA" + '"'),
                    ('"' + "kernel32.dll" + '"'), Marshal.GetLastWin32Error().ToString("X"));

                return IntPtr.Zero;
            }

            log.Log(LogType.Failure, "Failed to get module handle for {0}: {1}", ('"' + "kernel32.dll" + '"'),
                Marshal.GetLastWin32Error().ToString("X"));
            return IntPtr.Zero;
        }

        public bool WriteString(string toWrite, IntPtr addrToWriteTo, bool Unicode = true)
        {
            var tmpBytes = (Unicode ? Encoding.Unicode.GetBytes(toWrite) : Encoding.ASCII.GetBytes(toWrite));
            uint bytesWritten = 0;
            if (WinAPI.WriteProcessMemory(hProcess, addrToWriteTo, tmpBytes, tmpBytes.Length, out bytesWritten))
                if (bytesWritten == tmpBytes.Length)
                {
                    log.Log(LogType.Success, "Successfully wrote {0} to 0x{1}", toWrite,
                        (Environment.Is64BitProcess
                            ? addrToWriteTo.ToInt64().ToString("X16")
                            : addrToWriteTo.ToInt32().ToString("X8")));
                    return true;
                }
                else
                {
                    log.Log(LogType.Warning, "Partially successfully wrote {0} to 0x{1}: {2}", toWrite,
                        (Environment.Is64BitProcess
                            ? addrToWriteTo.ToInt64().ToString("X16")
                            : addrToWriteTo.ToInt32().ToString("X8")), Marshal.GetLastWin32Error().ToString("X"));

                    return true;
                }
            else
                log.Log(LogType.Failure, "Failed to write {0} to 0x{1}: {2}", toWrite,
                    (Environment.Is64BitProcess
                        ? addrToWriteTo.ToInt64().ToString("X16")
                        : addrToWriteTo.ToInt32().ToString("X8")), Marshal.GetLastWin32Error().ToString("X"));

            return false;
        }

        public bool WriteBytes(byte[] toWrite, IntPtr addrToWriteTo)
        {
            uint bytesWritten = 0;
            if(WinAPI.WriteProcessMemory(hProcess, addrToWriteTo, toWrite, toWrite.Length, out bytesWritten))
                if (bytesWritten == toWrite.Length)
                {
                    log.Log(LogType.Success, "Successfully wrote {0} to 0x{1}", toWrite.GetHex(),
                        (Environment.Is64BitProcess
                            ? addrToWriteTo.ToInt64().ToString("X16")
                            : addrToWriteTo.ToInt32().ToString("X8")));
                    return true;
                }
                else
                {
                    log.Log(LogType.Warning, "Partially successfully wrote {0} to 0x{1}: {2}", toWrite.GetHex(),
                        (Environment.Is64BitProcess
                            ? addrToWriteTo.ToInt64().ToString("X16")
                            : addrToWriteTo.ToInt32().ToString("X8")), Marshal.GetLastWin32Error().ToString("X"));

                    return true;
                }
            else
                log.Log(LogType.Failure, "Failed to write {0} to 0x{1}: {2}", toWrite.GetHex(),
                    (Environment.Is64BitProcess
                        ? addrToWriteTo.ToInt64().ToString("X16")
                        : addrToWriteTo.ToInt32().ToString("X8")), Marshal.GetLastWin32Error().ToString("X"));

            return false;
        }

        public IntPtr Allocate(int size)
        {
            return Allocate((uint) size);
        }
        public IntPtr Allocate(uint size)
        {
            var tmp = WinAPI.VirtualAllocEx(hProcess, IntPtr.Zero, size,
                WinAPI.AllocationType.Commit | WinAPI.AllocationType.Reserve, WinAPI.MemoryProtection.ExecuteReadWrite);

            if (tmp != IntPtr.Zero)
            {
                allocatedMem.Add(tmp, size);

                log.Log(LogType.Success, "Allocation of Memory to: 0x{0}",
                    (Environment.Is64BitProcess ? tmp.ToInt64().ToString("X16") : tmp.ToInt32().ToString("X8")));
                return tmp;
            }

            log.Log(LogType.Failure, "Allocation of Memory failed: {0}", Marshal.GetLastWin32Error().ToString("X"));
            return tmp;
        }

        public IntPtr CreateThread(IntPtr startAddr)
        {
            return CreateThread(startAddr, IntPtr.Zero);
        }
        public IntPtr CreateThread(IntPtr startAddr, IntPtr param)
        {
            return CreateThread(IntPtr.Zero, 0, startAddr, param, 0);
        }
        public IntPtr CreateThread(IntPtr threadAttributes, uint stackSize, IntPtr startAddr, IntPtr param,
            uint creationFlags)
        {
            int threadId = 0;
            var tmp = WinAPI.CreateRemoteThread(hProcess, threadAttributes, stackSize, startAddr, param, creationFlags,
                out threadId);
            if (tmp != IntPtr.Zero)
                log.Log(LogType.Success, "CreateThread was a success: 0x{0}",
                    (Environment.Is64BitProcess ? tmp.ToInt64().ToString("X16") : tmp.ToInt32().ToString("X8")));
            else
                log.Log(LogType.Failure, "Failed to CreateThread on process: {0}", Marshal.GetLastWin32Error().ToString("X"));

            return tmp;
        }

        void LoadProcess(int procId)
        {
            try
            {
                Tokenizer.Initiate();

                var tmp = WinAPI.OpenProcess(WinAPI.ProcessAccessFlags.All, false, procId);
                if (tmp != IntPtr.Zero)
                {
                    log.Log(LogType.Success, "Process Opened succesfully!");
                    hProcess = tmp;
                }
                else
                {
                    log.Log(LogType.Failure, "Failed to OpenProcess from {0}: {1}", procId.ToString("X"), Marshal.GetLastWin32Error().ToString("X"));
                }
            }
            catch (Exception ex)
            {
                log.Log(LogType.Exception, "LoadProcess Exception: {0} ({1})", ex.Message, Marshal.GetLastWin32Error().ToString("X"));
            }
        }

        public void Dispose()
        {
            if (hProcess != IntPtr.Zero)
            {
                if (allocatedMem.Count > 0)
                    foreach (var b in allocatedMem)
                        WinAPI.VirtualFreeEx(hProcess, b.Key, (int) b.Value, 0x4000);

                WinAPI.CloseHandle(hProcess);
            }

            allocatedMem.Clear();
            log.Dispose();
        }
    }
}
