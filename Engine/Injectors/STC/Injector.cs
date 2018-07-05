using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Engine.ProcessCore;

namespace Engine.Injectors.STC
{
    public class Injector : IInjector
    {
        private Logger log = new Logger(LoggerType.Console_File, "Injector.STC");

        public string About
        {
            get
            {
                return
                    "API: SetThreadContext" + Environment.NewLine +
                    "DLL: kernel32.dll" + Environment.NewLine + Environment.NewLine +
                    "Stealth: None" + Environment.NewLine +
                    "Kernel/System/Normal Access: Normal" + Environment.NewLine +
                    "Original Author: https://github.com/zodiacon/DllInjectionWithThreadContext"
                    ;
            }
        }
        public Module InjectedModule { get; set; }
        public IntPtr Inject(Core targetProcess, string filePath)
        {
            //Logger.StartLogger(Environment.UserInteractive ? LoggerType.Console : LoggerType.File, "Injector.STC");

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

            var code = (Environment.Is64BitProcess ? shell64 : shell32);
            var hThread = ProcessCore.WinAPI.GetProcessThread(targetProcess.ProcessId);
            if (hThread == IntPtr.Zero)
            {
                log.Log(LogType.Error, "Unable to open process's main thread: {0} - aborting!",
                    Marshal.GetLastWin32Error().ToString("X"));
                return IntPtr.Zero;
            }

            var page_size = 1 << 12;
            var buffer = targetProcess.Allocate(page_size);

            if (!ProcessCore.WinAPI.SuspendThread(hThread))
            {
                log.Log(LogType.Error, "Unable to suspend thread: {0} - aborting!",
                    Marshal.GetLastWin32Error().ToString("X"));
                return IntPtr.Zero;
            }

            if (Environment.Is64BitProcess)
            {
                var ctx = new ProcessCore.WinAPI.CONTEXT64();
                ctx.ContextFlags = ProcessCore.WinAPI.CONTEXT_FLAGS.CONTEXT_FULL;

                if (!ProcessCore.WinAPI.GetThreadContext(hThread, ref ctx))
                {
                    log.Log(LogType.Error, "Failed to retrieve thread's context: {0} - aborting!",
                        Marshal.GetLastWin32Error());
                    return IntPtr.Zero;
                }

                var buf = BitConverter.GetBytes((buffer + page_size / 2).ToInt64());
                var libBuf = BitConverter.GetBytes(loadLib.ToInt64());
                var ripBuf = BitConverter.GetBytes(ctx.Rip);
                
                Array.Copy(buf, 0, code, 0x10, buf.Length);
                Array.Copy(libBuf,0, code, 0x1a, libBuf.Length);
                Array.Copy(ripBuf,0, code, 0x34, ripBuf.Length);

                if (!targetProcess.WriteBytes(code, buffer))
                {
                    log.Log(LogType.Error, "Failed to write code-cave to process: {0} - aborting!",
                        Marshal.GetLastWin32Error().ToString("X"));
                    return IntPtr.Zero;
                }

                ctx.Rip = (ulong)alloc.ToInt64();
                if (!ProcessCore.WinAPI.SetThreadContext(hThread, ref ctx))
                {
                    log.Log(LogType.Failure, "Failed to set thread's context: {0}",
                        Marshal.GetLastWin32Error().ToString("X"));

                    ProcessCore.WinAPI.ResumeThread(hThread);
                    return IntPtr.Zero;
                }

                log.Log(LogType.Success, "Successfully set thread's context - {0} should now be injected!",
                    Path.GetFileName(filePath));
                return hThread;
            }
            else
            {
                var ctx = new ProcessCore.WinAPI.CONTEXT();
                ctx.ContextFlags = ProcessCore.WinAPI.CONTEXT_FLAGS.CONTEXT_FULL;

                if (!ProcessCore.WinAPI.GetThreadContext(hThread, ref ctx))
                {
                    log.Log(LogType.Error, "Failed to retrieve thread's context: {0} - aborting!",
                        Marshal.GetLastWin32Error());
                    return IntPtr.Zero;
                }

                var buf = BitConverter.GetBytes((buffer + page_size / 2).ToInt32());
                var libBuf = BitConverter.GetBytes(loadLib.ToInt32());
                var ripBuf = BitConverter.GetBytes(ctx.Eip);

                Array.Copy(buf, 0, code, 2, buf.Length);
                Array.Copy(libBuf, 0, code, 7, libBuf.Length);
                Array.Copy(ripBuf, 0, code, 0xf, ripBuf.Length);

                if (!targetProcess.WriteBytes(code, buffer))
                {
                    log.Log(LogType.Error, "Failed to write code-cave to process: {0} - aborting!",
                        Marshal.GetLastWin32Error().ToString("X"));
                    return IntPtr.Zero;
                }

                ctx.Eip = (uint)alloc.ToInt32();
                if (!ProcessCore.WinAPI.SetThreadContext(hThread, ref ctx))
                {
                    log.Log(LogType.Failure, "Failed to set thread's context: {0}",
                        Marshal.GetLastWin32Error().ToString("X"));

                    ProcessCore.WinAPI.ResumeThread(hThread);
                    return IntPtr.Zero;
                }

                log.Log(LogType.Success, "Successfully set thread's context - {0} should now be injected!",
                    Path.GetFileName(filePath));
                return hThread;
            }
        }

        private byte[] shell64 =
        {
            // sub rsp, 28h
            0x48, 0x83, 0xec, 0x28,                           
            // mov [rsp + 18], rax
            0x48, 0x89, 0x44, 0x24, 0x18,                     
            // mov [rsp + 10h], rcx
            0x48, 0x89, 0x4c, 0x24, 0x10,
            // mov rcx, 11111111111111111h
            0x48, 0xb9, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11,     
            // mov rax, 22222222222222222h
            0x48, 0xb8, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22,
            // call rax
            0xff, 0xd0,
            // mov rcx, [rsp + 10h]
            0x48, 0x8b, 0x4c, 0x24, 0x10,
            // mov rax, [rsp + 18h]
            0x48, 0x8b, 0x44, 0x24, 0x18,
            // add rsp, 28h
            0x48, 0x83, 0xc4, 0x28,
            // mov r11, 333333333333333333h
            0x49, 0xbb, 0x33, 0x33, 0x33, 0x33, 0x33, 0x33, 0x33, 0x33,
            // jmp r11
            0x41, 0xff, 0xe3
        };

        private byte[] shell32 =
        {
            //pushad
            0x60,
            //push 11111111h
            0x68, 0x11, 0x11, 0x11, 0x11,
            //mov eax, 22222222h
            0xb8, 0x22, 0x22, 0x22, 0x22,
            //call eax
            0xff, 0xd0,
            //popad
            0x61,
            //push 33333333h
            0x68, 0x33, 0x33, 0x33, 0x33,
            //ret
            0xc3
        };
    }
}
