using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Engine.ProcessCore
{

    using LARGE_INTEGER = System.UInt64;
    using DWORD = System.UInt32;
    using PWSTR = System.IntPtr;
    using USHORT = System.UInt16;
    using ULONG = System.UInt32;

    public static unsafe class WinAPI
    {
        #region DLL Names

        private const string kernel32 = "kernel32.dll";
        private const string advapi32 = "advapi32.dll";
        private const string secur32 = "secur32.dll";
        private const string ntdll = "ntdll.dll";
        private const string psapi = "psapi.dll";

        #endregion

        public static uint GetProcessThreadId(int pid, IntPtr hThread)
        {
            var h = CreateToolhelp32Snapshot(SnapshotFlags.Thread, 0);
            if (h != IntPtr.Zero)
            {
                THREADENTRY32 te = new THREADENTRY32();
                te.dwSize = (uint)sizeof(THREADENTRY32);
                if (Thread32First(h, ref te))
                {
                    do
                    {
                        if (te.dwSize >= (te.th32OwnerProcessID + Marshal.SizeOf(te.th32OwnerProcessID)))
                        {
                            if (te.th32OwnerProcessID == (uint) pid)
                            {
                                hThread = OpenThread(ThreadAccess.READ_CONTROL, false, (int) te.th32ThreadID);
                                if (hThread == IntPtr.Zero)
                                    return 0;
                                return te.th32ThreadID;
                            }
                        }
                    } while (Thread32Next(h, ref te));
                }
            }

            return 0;
        }

        public static IntPtr GetProcessThread(int pid)
        {
            var h = CreateToolhelp32Snapshot(SnapshotFlags.Thread, 0);
            if (h != IntPtr.Zero)
            {
                THREADENTRY32 te = new THREADENTRY32();
                te.dwSize = (uint)sizeof(THREADENTRY32);
                if (Thread32First(h, ref te))
                {
                    do
                    {
                        if (te.dwSize >= (te.th32OwnerProcessID + Marshal.SizeOf(te.th32OwnerProcessID)))
                        {
                            if (te.th32OwnerProcessID == (uint)pid)
                            {
                                var hThread = OpenThread(ThreadAccess.READ_CONTROL, false, (int)te.th32ThreadID);
                                if (hThread == IntPtr.Zero)
                                    return IntPtr.Zero;
                                return hThread;
                            }
                        }
                    } while (Thread32Next(h, ref te));
                }
            }

            return IntPtr.Zero;
        }

        public static byte[] GetBytes(void* toGetArrayFrom)
        {
            var tmpPtr = new IntPtr(toGetArrayFrom);
            var size = Marshal.SizeOf(tmpPtr);
            byte[] tmp = new byte[size];
            Marshal.Copy(tmpPtr, tmp, 0, size);
            return tmp;
        }

        public static byte[] GetBytes(IntPtr toGetArrayFrom)
        {
            var size = Marshal.SizeOf(toGetArrayFrom);
            byte[] tmp = new byte[size];
            Marshal.Copy(toGetArrayFrom, tmp, 0, size);
            return tmp;
        }

        public static int SizeOf(byte* bits)
        {
            IntPtr ptr = new IntPtr(bits);
            //Console.WriteLine(Marshal.SizeOf(ptr));
            return Marshal.SizeOf(ptr);
        }

        public static void memcpy(byte* dest, byte* src, uint count)
        {
            for (uint i = 0; i < count; i++)
            {
                *(dest + i) = *(src + i);
            }
        }

        public static IMAGE_SECTION_HEADER* IMAGE_FIRST_SECTION(byte* ptr_image_nt_headers)
        {
            if (Environment.Is64BitProcess)
            {
                IMAGE_NT_HEADERS64* image_nt_headers = (IMAGE_NT_HEADERS64*)ptr_image_nt_headers;
                return (IMAGE_SECTION_HEADER*)((long)image_nt_headers +
                                               (long)Marshal.OffsetOf(typeof(IMAGE_NT_HEADERS64), "OptionalHeader") +
                                               image_nt_headers->FileHeader.SizeOfOptionalHeader);
            }
            else
            {
                IMAGE_NT_HEADERS32* image_nt_headers = (IMAGE_NT_HEADERS32*)ptr_image_nt_headers;
                return (IMAGE_SECTION_HEADER*)((long)image_nt_headers +
                                               (long)Marshal.OffsetOf(typeof(IMAGE_NT_HEADERS32), "OptionalHeader") +
                                               image_nt_headers->FileHeader.SizeOfOptionalHeader);
            }
        }

        #region Natives

        #region psapi.dll

        [DllImport(psapi, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern int EnumProcessModules(IntPtr hProcess, [Out] ulong lphModule, uint cb, out uint lpcbNeeded);

        [DllImport(psapi, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public static extern uint GetModuleFileNameEx(IntPtr hProcess, ulong hModule, [Out] StringBuilder lpBaseName, uint nSize);

        [DllImport(psapi, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public static extern uint GetModuleBaseName(IntPtr hProcess, ulong hModule, [Out] StringBuilder lpBaseName, uint nSize);

        [DllImport(psapi, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool GetModuleInformation(IntPtr processHandle, IntPtr moduleHandle,
            NtModuleInfo ntModuleInfo, uint nSize);

        #endregion

        #region ntdll.dll

        [DllImport(ntdll, SetLastError = true)]
        public static extern Int32 NtSetInformationToken(
            IntPtr TokenHandle,
            Int32 TokenInformationClass,
            ref TOKEN_MANDATORY_LABEL TokenInformation,
            Int32 TokenInformationLength
        );

        [DllImport(ntdll, SetLastError = true)]
        public static extern int NtFilterToken(
            IntPtr TokenHandle,
            UInt32 Flags,
            IntPtr SidsToDisable,
            IntPtr PrivilegesToDelete,
            IntPtr RestrictedSids,
            ref IntPtr hToken
        );

        [DllImport(ntdll, SetLastError = true)]
        public static extern uint NtCreateSection(
            out ulong SectionHandle, ACCESS_MASK DesiredAccess, int ObjectAttributes,
            out long MaximumSize, MemoryProtection SectionPageProtection, uint AllocationAttributes,
            ulong FileHandle);

        [DllImport(ntdll, SetLastError = true)]
        public static extern int NtMapViewOfSection(
            ulong SectionHandle, IntPtr ProcessHandle, ref ulong BaseAddress,
            ulong ZeroBits, uint CommitSize, long SectionOffset,
            out uint ViewSize, uint InheritDisposition, uint AllocationType,
            MemoryProtection Win32Protect);

        [DllImport(ntdll, SetLastError = true)]
        public static extern int NtQueryInformationProcess(IntPtr processHandle, int processInformationClass, void* processInformation, int processInformationLength, IntPtr returnLength);

        [DllImport(ntdll, SetLastError = true)]
        public static extern uint NtUnmapViewOfSection(IntPtr ProcessHandle, ulong BaseAddress);

        [DllImport(ntdll, SetLastError = true)]
        public static extern uint NtQuerySystemInformation(uint InfoClass, ulong Info, uint Size, out uint Length);

        [DllImport(ntdll, SetLastError = true)]
        public static extern uint RtlGetVersion(_OSVERSIONINFOEXW* lpVersionInformation);


        #endregion

        #region secur32.dll

        [DllImport(secur32)]
        public static extern UInt32 LsaGetLogonSessionData(
            IntPtr LogonId,
            out IntPtr ppLogonSessionData
        );

        #endregion

        #region kernel32.dll

        [DllImport(kernel32, CallingConvention = CallingConvention.Winapi)]
        public static extern bool IsWow64Process(IntPtr hSourceProcessHandle, out bool isWow64);

        [DllImport(kernel32)]
        public static extern uint GetCurrentProcessId();

        [DllImport(kernel32, SetLastError = true)]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out uint lpNumberOfBytesRead);

        [DllImport(kernel32, SetLastError = true)]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, ref uint lpNumberOfBytesRead);

        [DllImport(kernel32, CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern uint GetPackageFamilyName(IntPtr hProcess, ref uint packageFamilyNameLength,
            StringBuilder packageFamilyName);

        [DllImport(kernel32, CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern uint GetPackageFullName(IntPtr hProcess, ref uint packageFullNameLength,
            StringBuilder packageFullName);

        [DllImport(kernel32, SetLastError = true)]
        public static extern bool Thread32Next(IntPtr hSnapshot, ref THREADENTRY32 lpte);

        [DllImport(kernel32, SetLastError = true)]
        public static extern bool Thread32First(IntPtr hSnapshot, ref THREADENTRY32 lpte);

        [DllImport(kernel32, SetLastError = true)]
        public static extern IntPtr CreateToolhelp32Snapshot(SnapshotFlags dwFlags, uint th32ProcessID);

        [DllImport(kernel32, SetLastError = true)]
        public static extern uint QueueUserAPC(IntPtr pfnAPC, IntPtr hThread, IntPtr dwData);

        [DllImport(kernel32, SetLastError = true)]
        public static extern bool GetExitCodeThread(ulong hThread, out uint lpExitCode);

        [DllImport(kernel32, SetLastError = true)]
        public static extern uint WaitForSingleObject(ulong hHandle, uint dwMilliseconds);

        [DllImport(kernel32, SetLastError = true)]
        public static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, int dwThreadId);

        [DllImport(kernel32, SetLastError = true)]
        public static extern bool GetThreadContext(ulong hThread, ref CONTEXT lpContext);

        [DllImport(kernel32, SetLastError = true)]
        public static extern bool GetThreadContext(IntPtr hThread, ref CONTEXT lpContext);

        [DllImport(kernel32, SetLastError = true)]
        public static extern bool SetThreadContext(ulong hThread, ref CONTEXT lpContext);

        [DllImport(kernel32, SetLastError = true)]
        public static extern bool SetThreadContext(IntPtr hThread, ref CONTEXT lpContext);

        [DllImport(kernel32, SetLastError = true)]
        public static extern bool GetThreadContext(ulong hThread, ref CONTEXT64 lpContext);

        [DllImport(kernel32, SetLastError = true)]
        public static extern bool GetThreadContext(IntPtr hThread, ref CONTEXT64 lpContext);

        [DllImport(kernel32, SetLastError = true)]
        public static extern bool SetThreadContext(ulong hThread, ref CONTEXT64 lpContext);

        [DllImport(kernel32, SetLastError = true)]
        public static extern bool SetThreadContext(IntPtr hThread, ref CONTEXT64 lpContext);

        [DllImport(kernel32, SetLastError = true)]
        public static extern bool ResumeThread(ulong hThread);

        [DllImport(kernel32, SetLastError = true)]
        public static extern bool ResumeThread(IntPtr hThread);

        [DllImport(kernel32, SetLastError = true)]
        public static extern bool SuspendThread(ulong hThread);

        [DllImport(kernel32, SetLastError = true)]
        public static extern bool SuspendThread(IntPtr hThread);

        [DllImport(kernel32, SetLastError = true)]
        public static extern UInt32 SearchPath(string lpPath, string lpFileName, string lpExtension,
            UInt32 nBufferLength, StringBuilder lpBuffer, ref IntPtr lpFilePart);

        [DllImport(kernel32, EntryPoint = "RtlZeroMemory", SetLastError = false)]
        public static extern void ZeroMemory(IntPtr dest, IntPtr size);

        [DllImport(kernel32, EntryPoint = "RtlZeroMemory", SetLastError = false)]
        public static extern void ZeroMemory(IntPtr dest, uint size);

        [DllImport(kernel32, EntryPoint = "RtlZeroMemory", SetLastError = false)]
        public static extern void ZeroMemory(IntPtr dest, int size);

        [DllImport(kernel32, EntryPoint = "RtlZeroMemory", SetLastError = false)]
        public static extern void ZeroMemory(void* dest, IntPtr size);

        [DllImport(kernel32, EntryPoint = "RtlZeroMemory", SetLastError = false)]
        public static extern void ZeroMemory(void* dest, uint size);

        [DllImport(kernel32, EntryPoint = "RtlZeroMemory", SetLastError = false)]
        public static extern void ZeroMemory(void* dest, int size);

        [DllImport(kernel32, SetLastError = true, CharSet = CharSet.Ansi)]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        [DllImport(kernel32, SetLastError = true, CharSet = CharSet.Ansi)]
        public static extern IntPtr GetProcAddress(IntPtr hModule, uint lpProcName);

        [DllImport(kernel32, CharSet = CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport(kernel32, SetLastError = true)]
        public static extern IntPtr GetModuleHandleA(string lpModuleName);

        [DllImport(kernel32, SetLastError = true)]
        public static extern IntPtr GetModuleHandleW(string lpModuleName);

        [DllImport(kernel32, SetLastError = true)]
        public static extern uint GetModuleFileName(IntPtr hModule, StringBuilder lpFileName, int nSize);

        [DllImport(kernel32, SetLastError = true)]
        public static extern bool CloseHandle(IntPtr handle);

        [DllImport(kernel32, SetLastError = true)]
        public static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, bool bInheritHandle,
            uint dwProcessId);

        [DllImport(kernel32, SetLastError = true)]
        public static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, bool bInheritHandle,
            int dwProcessId);

        [DllImport(kernel32, SetLastError = true)]
        public static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle,
            uint dwProcessId);

        [DllImport(kernel32, SetLastError = true)]
        public static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle,
            int dwProcessId);

        [DllImport(kernel32, SetLastError = true)]
        public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, int flAllocationType,
            int flProtect);

        [DllImport(kernel32, SetLastError = true)]
        public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize,
            AllocationType flAllocationType, MemoryProtection flProtect);

        [DllImport(kernel32, SetLastError = true)]
        public static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, int dwSize, int dwFreeType);

        [DllImport(kernel32, SetLastError = true)]
        public static extern IntPtr CreateRemoteThread(IntPtr hProcess, int lpThreadAttributes, int dwStackSize,
            IntPtr lpStartAddress, uint lpParameter, int dwCreationFlags, int lpThreadId);

        [DllImport(kernel32)]
        public static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize,
            IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

        [DllImport(kernel32)]
        public static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize,
            IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, out int lpThreadId);

        [DllImport(kernel32, SetLastError = true)]
        public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpAddress, byte[] lpBuffer, int dwSize,
            out uint lpNumberOfBytesRead);

        [DllImport(kernel32, SetLastError = true)]
        public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize,
            out IntPtr lpNumberOfBytesWritten);
        #endregion

        #region advapi32.dll

        [DllImport(advapi32, CharSet = CharSet.Unicode)]
        public static extern IntPtr GetSidSubAuthority(IntPtr psid, uint subAuthorityIndex);

        [DllImport(advapi32, CharSet = CharSet.Unicode)]
        public static extern IntPtr GetSidSubAuthorityCount(IntPtr psid);

        [DllImport(advapi32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool OpenProcessToken(IntPtr ProcessHandle, uint DesiredAccess, out IntPtr tokenhandle);

        [DllImport(advapi32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool LookupPrivilegeValue(string lpsystemname, string lpname, ref long lpLuid);

        [DllImport(advapi32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool AdjustTokenPrivileges(IntPtr tokenhandle, bool disableprivs,
            ref TOKEN_PRIVILEGES Newstate, int bufferlength, IntPtr PreivousState, IntPtr Returnlength);

        [DllImport(advapi32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool AdjustTokenPrivileges(IntPtr tokenhandle, bool disableprivs,
            ref _TOKEN_PRIVILEGES Newstate, int bufferlength, ref _TOKEN_PRIVILEGES PreivousState, out uint Returnlength);

        [DllImport(advapi32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool LookupPrivilegeValue(string lpsystemname, string lpname, ref _LUID lpLuid);

        [DllImport(advapi32, SetLastError = true)]
        public static extern Boolean RevertToSelf();

        [DllImport(advapi32, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool CreateProcessWithLogonW(
            string userName,
            string domain,
            string password,
            int logonFlags,
            string applicationName,
            string commandLine,
            int creationFlags,
            IntPtr environment,
            string currentDirectory,
            ref _STARTUPINFO startupInfo,
            out _PROCESS_INFORMATION processInformation
        );

        [DllImport(advapi32, SetLastError = true)]
        public static extern Boolean AllocateAndInitializeSid(
            ref SidIdentifierAuthority pIdentifierAuthority,
            byte nSubAuthorityCount,
            Int32 dwSubAuthority0,
            Int32 dwSubAuthority1,
            Int32 dwSubAuthority2,
            Int32 dwSubAuthority3,
            Int32 dwSubAuthority4,
            Int32 dwSubAuthority5,
            Int32 dwSubAuthority6,
            Int32 dwSubAuthority7,
            out IntPtr pSid
        );

        [DllImport(advapi32, SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool LookupAccountSid(
            String lpSystemName,
            //[MarshalAs(UnmanagedType.LPArray)] 
            IntPtr Sid,
            StringBuilder lpName,
            ref UInt32 cchName,
            StringBuilder ReferencedDomainName,
            ref UInt32 cchReferencedDomainName,
            out _SID_NAME_USE peUse
        );

        [DllImport(advapi32, CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int GetTokenInformation(IntPtr hToken, int tokenInformationClass, IntPtr tokenInformation,
            uint tokenInformationLength, out uint returnLength);

        [DllImport(advapi32, SetLastError = true)]
        public static extern Boolean GetTokenInformation(
            IntPtr TokenHandle,
            _TOKEN_INFORMATION_CLASS TokenInformationClass,
            ref _TOKEN_STATISTICS TokenInformation,
            UInt32 TokenInformationLength,
            out UInt32 ReturnLength
        );

        [DllImport(advapi32, SetLastError = true)]
        public static extern Boolean GetTokenInformation(
            IntPtr TokenHandle,
            _TOKEN_INFORMATION_CLASS TokenInformationClass,
            IntPtr TokenInformation,
            UInt32 TokenInformationLength,
            out UInt32 ReturnLength
        );

        [DllImport(advapi32, SetLastError = true)]
        public static extern bool DuplicateTokenEx(
            IntPtr hExistingToken,
            UInt32 dwDesiredAccess,
            ref SECURITY_ATTRIBUTES lpTokenAttributes,
            _SECURITY_IMPERSONATION_LEVEL ImpersonationLevel,
            TOKEN_TYPE TokenType,
            out IntPtr phNewToken
        );

        [DllImport(advapi32, SetLastError = true)]
        public static extern bool ImpersonateLoggedOnUser(
            IntPtr hToken
        );
        #endregion


        #endregion

        #region Structures

        #region NtModuleInfo

        [StructLayout(LayoutKind.Sequential)]
        public struct NtModuleInfo
        {
            // Token: 0x04002EF0 RID: 12016
            public IntPtr BaseOfDll;// = (IntPtr)0;

            // Token: 0x04002EF1 RID: 12017
            public int SizeOfImage;

            // Token: 0x04002EF2 RID: 12018
            public IntPtr EntryPoint;// = (IntPtr)0;
        }

            #endregion

        #region SECURITY_DESCRIPTOR

        [StructLayout(LayoutKind.Sequential)]
        public struct SECURITY_DESCRIPTOR
        {
            public byte revision;
            public byte size;
            public SECURITY_DESCRIPTOR_CONTROL control;
            public IntPtr owner;
            public IntPtr group;
            public IntPtr sacl;
            public IntPtr dacl;
        }

        #endregion

        #region SECURITY_DESCRIPTOR_CONTROL

        public enum SECURITY_DESCRIPTOR_CONTROL : uint
        {
            SE_DACL_AUTO_INHERIT_REQ = 0x0100,
            SE_DACL_AUTO_INHERITED = 0x0400,
            SE_DACL_DEFAULTED = 0x0008,
            SE_DACL_PRESENT = 0x0004,
            SE_DACL_PROTECTED = 0x1000,
            SE_GROUP_DEFAULTED = 0x0002,
            SE_OWNER_DEFAULTED = 0x0001,
            SE_RM_CONTROL_VALID = 0x4000,
            SE_SACL_AUTO_INHERIT_REQ = 0x0200,
            SE_SACL_AUTO_INHERITED = 0x0800,
            SE_SACL_DEFAULTED = 0x0008,
            SE_SACL_PRESENT = 0x0010,
            SE_SACL_PROTECTED = 0x2000,
            SE_SELF_RELATIVE = 0x8000
        }

            #endregion

        #region THREADENTRY32

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct THREADENTRY32
        {
            public UInt32 dwSize;
            public UInt32 cntUsage;
            public UInt32 th32ThreadID;
            public UInt32 th32OwnerProcessID;
            public UInt32 tpBasePri;
            public UInt32 tpDeltaPri;
            public UInt32 dwFlags;
        }

            #endregion

        #region SnapshotFlags

        [Flags]
        public enum SnapshotFlags : uint
        {
            HeapList = 0x00000001,
            Process = 0x00000002,
            Thread = 0x00000004,
            Module = 0x00000008,
            Module32 = 0x00000010,
            Inherit = 0x80000000,
            All = 0x0000001F,
            NoHeaps = 0x40000000
        }

            #endregion

        #region PROCESSENTRY32

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct PROCESSENTRY32
        {
            public const int MAX_PATH = 260;
            public UInt32 dwSize;
            public UInt32 cntUsage;
            public UInt32 th32ProcessID;
            public IntPtr th32DefaultHeapID;
            public UInt32 th32ModuleID;
            public UInt32 cntThreads;
            public UInt32 th32ParentProcessID;
            public Int32 pcPriClassBase;
            public UInt32 dwFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)]
            public string szExeFile;
        }

            #endregion

        #region M128A

        [StructLayout(LayoutKind.Sequential)]
        public struct M128A
        {
            public ulong High;
            public long Low;

            public override string ToString()
            {
                return string.Format("High:{0}, Low:{1}", this.High, this.Low);
            }
        }

            #endregion

        #region XSAVE_FORMAT64

        [StructLayout(LayoutKind.Sequential, Pack = 16)]
        public struct XSAVE_FORMAT64
        {
            public ushort ControlWord;
            public ushort StatusWord;
            public byte TagWord;
            public byte Reserved1;
            public ushort ErrorOpcode;
            public uint ErrorOffset;
            public ushort ErrorSelector;
            public ushort Reserved2;
            public uint DataOffset;
            public ushort DataSelector;
            public ushort Reserved3;
            public uint MxCsr;
            public uint MxCsr_Mask;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public M128A[] FloatRegisters;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public M128A[] XmmRegisters;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 96)]
            public byte[] Reserved4;
        }

        #endregion

        #region FLOATING_SAVE_AREA

        [StructLayout(LayoutKind.Sequential)]
        public struct FLOATING_SAVE_AREA
        {
            public uint ControlWord;
            public uint StatusWord;
            public uint TagWord;
            public uint ErrorOffset;
            public uint ErrorSelector;
            public uint DataOffset;
            public uint DataSelector;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 80)]
            public byte[] RegisterArea;
            public uint Cr0NpxState;
        }

        #endregion

        #region CONTEXT

        [StructLayout(LayoutKind.Sequential)]
        public struct CONTEXT
        {
            public CONTEXT_FLAGS ContextFlags; //set this to an appropriate value 
            // Retrieved by CONTEXT_DEBUG_REGISTERS 
            public uint Dr0;
            public uint Dr1;
            public uint Dr2;
            public uint Dr3;
            public uint Dr6;
            public uint Dr7;
            // Retrieved by CONTEXT_FLOATING_POINT 
            public FLOATING_SAVE_AREA FloatSave;
            // Retrieved by CONTEXT_SEGMENTS 
            public uint SegGs;
            public uint SegFs;
            public uint SegEs;
            public uint SegDs;
            // Retrieved by CONTEXT_INTEGER 
            public uint Edi;
            public uint Esi;
            public uint Ebx;
            public uint Edx;
            public uint Ecx;
            public uint Eax;
            // Retrieved by CONTEXT_CONTROL 
            public uint Ebp;
            public uint Eip;
            public uint SegCs;
            public uint EFlags;
            public uint Esp;
            public uint SegSs;
            // Retrieved by CONTEXT_EXTENDED_REGISTERS 
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)]
            public byte[] ExtendedRegisters;
        }

            #endregion

        #region CONTEXT64

        [StructLayout(LayoutKind.Sequential, Pack = 16)]
        public struct CONTEXT64
        {
            public ulong P1Home;
            public ulong P2Home;
            public ulong P3Home;
            public ulong P4Home;
            public ulong P5Home;
            public ulong P6Home;

            public CONTEXT_FLAGS ContextFlags;
            public uint MxCsr;

            public ushort SegCs;
            public ushort SegDs;
            public ushort SegEs;
            public ushort SegFs;
            public ushort SegGs;
            public ushort SegSs;
            public uint EFlags;

            public ulong Dr0;
            public ulong Dr1;
            public ulong Dr2;
            public ulong Dr3;
            public ulong Dr6;
            public ulong Dr7;

            public ulong Rax;
            public ulong Rcx;
            public ulong Rdx;
            public ulong Rbx;
            public ulong Rsp;
            public ulong Rbp;
            public ulong Rsi;
            public ulong Rdi;
            public ulong R8;
            public ulong R9;
            public ulong R10;
            public ulong R11;
            public ulong R12;
            public ulong R13;
            public ulong R14;
            public ulong R15;
            public ulong Rip;

            public XSAVE_FORMAT64 DUMMYUNIONNAME;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 26)]
            public M128A[] VectorRegister;
            public ulong VectorControl;

            public ulong DebugControl;
            public ulong LastBranchToRip;
            public ulong LastBranchFromRip;
            public ulong LastExceptionToRip;
            public ulong LastExceptionFromRip;
        }

        #endregion

        #region CONTEXT_FLAGS

        public enum CONTEXT_FLAGS : uint
        {
            CONTEXT_i386 = 0x10000,
            CONTEXT_i486 = 0x10000,   //  same as i386
            CONTEXT_CONTROL = CONTEXT_i386 | 0x01, // SS:SP, CS:IP, FLAGS, BP
            CONTEXT_INTEGER = CONTEXT_i386 | 0x02, // AX, BX, CX, DX, SI, DI
            CONTEXT_SEGMENTS = CONTEXT_i386 | 0x04, // DS, ES, FS, GS
            CONTEXT_FLOATING_POINT = CONTEXT_i386 | 0x08, // 387 state
            CONTEXT_DEBUG_REGISTERS = CONTEXT_i386 | 0x10, // DB 0-3,6,7
            CONTEXT_EXTENDED_REGISTERS = CONTEXT_i386 | 0x20, // cpu specific extensions
            CONTEXT_FULL = CONTEXT_CONTROL | CONTEXT_INTEGER | CONTEXT_SEGMENTS,
            CONTEXT_ALL = CONTEXT_CONTROL | CONTEXT_INTEGER | CONTEXT_SEGMENTS | CONTEXT_FLOATING_POINT | CONTEXT_DEBUG_REGISTERS | CONTEXT_EXTENDED_REGISTERS
        }

            #endregion

        #region ThreadAccess

        [Flags]
        public enum ThreadAccess : int
        {
            TERMINATE = (0x0001),
            SUSPEND_RESUME = (0x0002),
            GET_CONTEXT = (0x0008),
            SET_CONTEXT = (0x0010),
            SET_INFORMATION = (0x0020),
            QUERY_INFORMATION = (0x0040),
            SET_THREAD_TOKEN = (0x0080),
            IMPERSONATE = (0x0100),
            DIRECT_IMPERSONATION = (0x0200),
            READ_CONTROL = (0x00020000)
        }

            #endregion

        #region _OSVERSIONINFOEXW

        [StructLayout(LayoutKind.Sequential)]
        public struct _OSVERSIONINFOEXW
        {
            public uint dwOSVersionInfoSize;
            public uint dwMajorVersion;
            public uint dwMinorVersion;
            public uint dwBuildNumber;
            public uint dwPlatformId;
            public fixed byte szCSDVersion[128 * 2/*WCHAR*/];     // Maintenance string for PSS usage
            public ushort wServicePackMajor;
            public ushort wServicePackMinor;
            public ushort wSuiteMask;
            public byte wProductType;
            public byte wReserved;
        }

        #endregion

        #region ProcessInformationClass

        public enum ProcessInformationClass : int
        {
            ProcessBasicInformation = 0, // 0, q: PROCESS_BASIC_INFORMATION, PROCESS_EXTENDED_BASIC_INFORMATION
            ProcessQuotaLimits, // qs: QUOTA_LIMITS, QUOTA_LIMITS_EX
            ProcessIoCounters, // q: IO_COUNTERS
            ProcessVmCounters, // q: VM_COUNTERS, VM_COUNTERS_EX
            ProcessTimes, // q: KERNEL_USER_TIMES
            ProcessBasePriority, // s: KPRIORITY
            ProcessRaisePriority, // s: ULONG
            ProcessDebugPort, // q: HANDLE
            ProcessExceptionPort, // s: HANDLE
            ProcessAccessToken, // s: PROCESS_ACCESS_TOKEN
            ProcessLdtInformation, // 10
            ProcessLdtSize,
            ProcessDefaultHardErrorMode, // qs: ULONG
            ProcessIoPortHandlers, // (kernel-mode only)
            ProcessPooledUsageAndLimits, // q: POOLED_USAGE_AND_LIMITS
            ProcessWorkingSetWatch, // q: PROCESS_WS_WATCH_INFORMATION[]; s: void
            ProcessUserModeIOPL,
            ProcessEnableAlignmentFaultFixup, // s: BOOLEAN
            ProcessPriorityClass, // qs: PROCESS_PRIORITY_CLASS
            ProcessWx86Information,
            ProcessHandleCount, // 20, q: ULONG, PROCESS_HANDLE_INFORMATION
            ProcessAffinityMask, // s: KAFFINITY
            ProcessPriorityBoost, // qs: ULONG
            ProcessDeviceMap, // qs: PROCESS_DEVICEMAP_INFORMATION, PROCESS_DEVICEMAP_INFORMATION_EX
            ProcessSessionInformation, // q: PROCESS_SESSION_INFORMATION
            ProcessForegroundInformation, // s: PROCESS_FOREGROUND_BACKGROUND
            ProcessWow64Information, // q: ULONG_PTR
            ProcessImageFileName, // q: UNICODE_STRING
            ProcessLUIDDeviceMapsEnabled, // q: ULONG
            ProcessBreakOnTermination, // qs: ULONG
            ProcessDebugObjectHandle, // 30, q: HANDLE
            ProcessDebugFlags, // qs: ULONG
            ProcessHandleTracing, // q: PROCESS_HANDLE_TRACING_QUERY; s: size 0 disables, otherwise enables
            ProcessIoPriority, // qs: ULONG
            ProcessExecuteFlags, // qs: ULONG
            ProcessResourceManagement,
            ProcessCookie, // q: ULONG
            ProcessImageInformation, // q: SECTION_IMAGE_INFORMATION
            ProcessCycleTime, // q: PROCESS_CYCLE_TIME_INFORMATION
            ProcessPagePriority, // q: ULONG
            ProcessInstrumentationCallback, // 40
            ProcessThreadStackAllocation, // s: PROCESS_STACK_ALLOCATION_INFORMATION, PROCESS_STACK_ALLOCATION_INFORMATION_EX
            ProcessWorkingSetWatchEx, // q: PROCESS_WS_WATCH_INFORMATION_EX[]
            ProcessImageFileNameWin32, // q: UNICODE_STRING
            ProcessImageFileMapping, // q: HANDLE (input)
            ProcessAffinityUpdateMode, // qs: PROCESS_AFFINITY_UPDATE_MODE
            ProcessMemoryAllocationMode, // qs: PROCESS_MEMORY_ALLOCATION_MODE
            ProcessGroupInformation, // q: USHORT[]
            ProcessTokenVirtualizationEnabled, // s: ULONG
            ProcessConsoleHostProcess, // q: ULONG_PTR
            ProcessWindowInformation, // 50, q: PROCESS_WINDOW_INFORMATION
            ProcessHandleInformation, // q: PROCESS_HANDLE_SNAPSHOT_INFORMATION // since WIN8
            ProcessMitigationPolicy, // s: PROCESS_MITIGATION_POLICY_INFORMATION
            ProcessDynamicFunctionTableInformation,
            ProcessHandleCheckingMode,
            ProcessKeepAliveCount, // q: PROCESS_KEEPALIVE_COUNT_INFORMATION
            ProcessRevokeFileHandles, // s: PROCESS_REVOKE_FILE_HANDLES_INFORMATION
            MaxProcessInfoClass
        };

        #endregion

        #region TOKEN_ELEVATION_TYPE

        internal enum TOKEN_ELEVATION_TYPE
        {
            TokenElevationTypeDefault = 1,
            TokenElevationTypeFull,
            TokenElevationTypeLimited
        }

        #endregion

        #region _TOKEN_INFORMATION_CLASS

        public enum _TOKEN_INFORMATION_CLASS
        {
            TokenUser = 1,
            TokenGroups,
            TokenPrivileges,
            TokenOwner,
            TokenPrimaryGroup,
            TokenDefaultDacl,
            TokenSource,
            TokenType,
            TokenImpersonationLevel,
            TokenStatistics,
            TokenRestrictedSids,
            TokenSessionId,
            TokenGroupsAndPrivileges,
            TokenSessionReference,
            TokenSandBoxInert,
            TokenAuditPolicy,
            TokenOrigin,
            TokenElevationType,
            TokenLinkedToken,
            TokenElevation,
            TokenHasRestrictions,
            TokenAccessInformation,
            TokenVirtualizationAllowed,
            TokenVirtualizationEnabled,
            TokenIntegrityLevel,
            TokenUIAccess,
            TokenMandatoryPolicy,
            TokenLogonSid,
            TokenIsAppContainer,
            TokenCapabilities,
            TokenAppContainerSid,
            TokenAppContainerNumber,
            TokenUserClaimAttributes,
            TokenDeviceClaimAttributes,
            TokenRestrictedUserClaimAttributes,
            TokenRestrictedDeviceClaimAttributes,
            TokenDeviceGroups,
            TokenRestrictedDeviceGroups,
            TokenSecurityAttributes,
            TokenIsRestricted,
            MaxTokenInfoClass
        }

        #endregion

        #region TOKEN_ACCESS

        public static class TOKEN_ACCESS
        {
            public const UInt32 STANDARD_RIGHTS_REQUIRED = 0x000F0000;
            public const UInt32 STANDARD_RIGHTS_READ = 0x00020000;
            public const UInt32 TOKEN_ASSIGN_PRIMARY = 0x0001;
            public const UInt32 TOKEN_DUPLICATE = 0x0002;
            public const UInt32 TOKEN_IMPERSONATE = 0x0004;
            public const UInt32 TOKEN_QUERY = 0x0008;
            public const UInt32 TOKEN_QUERY_SOURCE = 0x0010;
            public const UInt32 TOKEN_ADJUST_PRIVILEGES = 0x0020;
            public const UInt32 TOKEN_ADJUST_GROUPS = 0x0040;
            public const UInt32 TOKEN_ADJUST_DEFAULT = 0x0080;
            public const UInt32 TOKEN_ADJUST_SESSIONID = 0x0100;
            public const UInt32 TOKEN_READ = (STANDARD_RIGHTS_READ | TOKEN_QUERY);
            public const UInt32 TOKEN_ALL_ACCESS = (STANDARD_RIGHTS_REQUIRED | TOKEN_ASSIGN_PRIMARY |
                                                    TOKEN_DUPLICATE | TOKEN_IMPERSONATE | TOKEN_QUERY | TOKEN_QUERY_SOURCE |
                                                    TOKEN_ADJUST_PRIVILEGES | TOKEN_ADJUST_GROUPS | TOKEN_ADJUST_DEFAULT |
                                                    TOKEN_ADJUST_SESSIONID);
        }

        #endregion

        #region ACCESS_MASK

        [Flags]
        public enum ACCESS_MASK : uint
        {
            DELETE = 0x00010000,
            READ_CONTROL = 0x00020000,
            WRITE_DAC = 0x00040000,
            WRITE_OWNER = 0x00080000,
            SYNCHRONIZE = 0x00100000,
            STANDARD_RIGHTS_REQUIRED = 0x000F0000,
            STANDARD_RIGHTS_READ = 0x00020000,
            STANDARD_RIGHTS_WRITE = 0x00020000,
            STANDARD_RIGHTS_EXECUTE = 0x00020000,
            STANDARD_RIGHTS_ALL = 0x001F0000,
            SPECIFIC_RIGHTS_ALL = 0x0000FFF,
            ACCESS_SYSTEM_SECURITY = 0x01000000,
            MAXIMUM_ALLOWED = 0x02000000,
            GENERIC_READ = 0x80000000,
            GENERIC_WRITE = 0x40000000,
            GENERIC_EXECUTE = 0x20000000,
            GENERIC_ALL = 0x10000000,
            DESKTOP_READOBJECTS = 0x00000001,
            DESKTOP_CREATEWINDOW = 0x00000002,
            DESKTOP_CREATEMENU = 0x00000004,
            DESKTOP_HOOKCONTROL = 0x00000008,
            DESKTOP_JOURNALRECORD = 0x00000010,
            DESKTOP_JOURNALPLAYBACK = 0x00000020,
            DESKTOP_ENUMERATE = 0x00000040,
            DESKTOP_WRITEOBJECTS = 0x00000080,
            DESKTOP_SWITCHDESKTOP = 0x00000100,
            WINSTA_ENUMDESKTOPS = 0x00000001,
            WINSTA_READATTRIBUTES = 0x00000002,
            WINSTA_ACCESSCLIPBOARD = 0x00000004,
            WINSTA_CREATEDESKTOP = 0x00000008,
            WINSTA_WRITEATTRIBUTES = 0x00000010,
            WINSTA_ACCESSGLOBALATOMS = 0x00000020,
            WINSTA_EXITWINDOWS = 0x00000040,
            WINSTA_ENUMERATE = 0x00000100,
            WINSTA_READSCREEN = 0x00000200,
            WINSTA_ALL_ACCESS = 0x0000037F
        };

        #endregion

        #region _SECURITY_IMPERSONATION_LEVEL

        [Flags]
        public enum _SECURITY_IMPERSONATION_LEVEL : int
        {
            SecurityAnonymous = 0,
            SecurityIdentification = 1,
            SecurityImpersonation = 2,
            SecurityDelegation = 3
        };

        #endregion

        #region TOKEN_TYPE

        [Flags]
        public enum TOKEN_TYPE
        {
            TokenPrimary = 1,
            TokenImpersonation
        }

        #endregion

        #region _TOKEN_STATISTICS

        [StructLayout(LayoutKind.Sequential)]
        public struct _TOKEN_STATISTICS
        {
            public _LUID TokenId;
            public _LUID AuthenticationId;
            public LARGE_INTEGER ExpirationTime;
            public TOKEN_TYPE TokenType;
            public _SECURITY_IMPERSONATION_LEVEL ImpersonationLevel;
            public DWORD DynamicCharged;
            public DWORD DynamicAvailable;
            public DWORD GroupCount;
            public DWORD PrivilegeCount;
            public _LUID ModifiedId;
        }

        #endregion

        #region _LSA_UNICODE_STRING

        [StructLayout(LayoutKind.Sequential)]
        public struct _LSA_UNICODE_STRING
        {
            public USHORT Length;
            public USHORT MaximumLength;
            public PWSTR Buffer;
        }

        #endregion

        #region _SECURITY_LOGON_SESSION_DATA

        [StructLayout(LayoutKind.Sequential)]
        public struct _SECURITY_LOGON_SESSION_DATA
        {
            public ULONG Size;
            public _LUID LogonId;
            public _LSA_UNICODE_STRING UserName;
            public _LSA_UNICODE_STRING LogonDomain;
            public _LSA_UNICODE_STRING AuthenticationPackage;
            public ULONG LogonType;
            public ULONG Session;
            public IntPtr Sid;
            public LARGE_INTEGER LogonTime;
            public _LSA_UNICODE_STRING LogonServer;
            public _LSA_UNICODE_STRING DnsDomainName;
            public _LSA_UNICODE_STRING Upn;
            /*
            public ULONG UserFlags;
            public _LSA_LAST_INTER_LOGON_INFO LastLogonInfo;
            public _LSA_UNICODE_STRING LogonScript;
            public _LSA_UNICODE_STRING ProfilePath;
            public _LSA_UNICODE_STRING HomeDirectory;
            public _LSA_UNICODE_STRING HomeDirectoryDrive;
            public LARGE_INTEGER LogoffTime;
            public LARGE_INTEGER KickOffTime;
            public LARGE_INTEGER PasswordLastSet;
            public LARGE_INTEGER PasswordCanChange;
            public LARGE_INTEGER PasswordMustChange;
            */
        }

        #endregion

        #region _SID_NAME_USE

        public enum _SID_NAME_USE
        {
            SidTypeUser = 1,
            SidTypeGroup,
            SidTypeDomain,
            SidTypeAlias,
            SidTypeWellKnownGroup,
            SidTypeDeletedAccount,
            SidTypeInvalid,
            SidTypeUnknown,
            SidTypeComputer,
            SidTypeLabel
        }

        #endregion

        #region SidIdentifierAuthority

        [StructLayout(LayoutKind.Sequential)]
        public struct SidIdentifierAuthority
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6, ArraySubType = UnmanagedType.I1)]
            public byte[] Value;
        }

        #endregion

        #region SID_AND_ATTRIBUTES

        [StructLayout(LayoutKind.Sequential)]
        public struct SID_AND_ATTRIBUTES
        {
            public IntPtr Sid;
            public UInt32 Attributes;
        }

        #endregion

        #region TOKEN_MANDATORY_LABEL

        [StructLayout(LayoutKind.Sequential)]
        public struct TOKEN_MANDATORY_LABEL
        {
            public SID_AND_ATTRIBUTES Label;
        }

        #endregion

        #region _STARTUPINFO

        [StructLayout(LayoutKind.Sequential)]
        public struct _STARTUPINFO
        {
            public UInt32 cb;
            public String lpReserved;
            public String lpDesktop;
            public String lpTitle;
            public UInt32 dwX;
            public UInt32 dwY;
            public UInt32 dwXSize;
            public UInt32 dwYSize;
            public UInt32 dwXCountChars;
            public UInt32 dwYCountChars;
            public UInt32 dwFillAttribute;
            public UInt32 dwFlags;
            public UInt16 wShowWindow;
            public UInt16 cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        };

        #endregion

        #region _PROCESS_INFORMATION

        [StructLayout(LayoutKind.Sequential)]
        public struct _PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public UInt32 dwProcessId;
            public UInt32 dwThreadId;
        };


            #endregion

        #region SECURITY_ATTRIBUTES

        [StructLayout(LayoutKind.Sequential)]
        public struct SECURITY_ATTRIBUTES
        {
            UInt32 nLength;
            IntPtr lpSecurityDescriptor;
            Boolean bInheritHandle;
        };

        #endregion

        #region TOKEN_PRIVILEGES

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct TOKEN_PRIVILEGES
        {
            public int PrivilegeCount;
            public long Luid;
            public int Attributes;
        }

        #endregion

        #region _TOKEN_PRIVILEGES

        [StructLayout(LayoutKind.Sequential)]
        public struct _TOKEN_PRIVILEGES
        {
            public UInt32 PrivilegeCount;
            public _LUID_AND_ATTRIBUTES Privileges;
        }

        #endregion

        #region _TOKEN_PRIVILEGES_ARRAY

        [StructLayout(LayoutKind.Sequential)]
        public struct _TOKEN_PRIVILEGES_ARRAY
        {
            public UInt32 PrivilegeCount;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 30)]
            public _LUID_AND_ATTRIBUTES[] Privileges;
        }

        #endregion

        #region _LUID_AND_ATTRIBUTES

        [StructLayout(LayoutKind.Sequential)]
        public struct _LUID_AND_ATTRIBUTES
        {
            public _LUID Luid;
            public UInt32 Attributes;
        }

        #endregion

        #region _LUID

        [StructLayout(LayoutKind.Sequential)]
        public struct _LUID
        {
            public UInt32 LowPart;
            public UInt32 HighPart;
        }

        #endregion

        #region AllocationType

        public enum AllocationType
        {
            Commit = 0x1000,
            Reserve = 0x2000,
            Decommit = 0x4000,
            Release = 0x8000,
            Reset = 0x80000,
            Physical = 0x400000,
            TopDown = 0x100000,
            WriteWatch = 0x200000,
            LargePages = 0x20000000
        }

        #endregion

        #region MemoryProtection

        public enum MemoryProtection
        {
            Execute = 0x10,
            ExecuteRead = 0x20,
            ExecuteReadWrite = 0x40,
            ExecuteWriteCopy = 0x80,
            NoAccess = 0x01,
            ReadOnly = 0x02,
            ReadWrite = 0x04,
            WriteCopy = 0x08,
            GuardModifierflag = 0x100,
            NoCacheModifierflag = 0x200,
            WriteCombineModifierflag = 0x400
        }

        #endregion

        #region MemoryProtectionFlags

        public enum MemoryProtectionFlags
        {
            /// <summary>
            /// Disables all access to the committed region of pages. An attempt to read from, write to, or execute the committed region results in an access violation.
            /// This value is not officially present in the Microsoft's enumeration but can occur according to the MEMORY_BASIC_INFORMATION structure documentation.
            /// </summary>
            ZeroAccess = 0x0,
            /// <summary>
            /// Enables execute access to the committed region of pages. An attempt to read from or write to the committed region results in an access violation.
            /// This flag is not supported by the CreateFileMapping function.
            /// </summary>
            Execute = 0x10,
            /// <summary>
            /// Enables execute or read-only access to the committed region of pages. An attempt to write to the committed region results in an access violation.
            /// </summary>
            ExecuteRead = 0x20,
            /// <summary>
            /// Enables execute, read-only, or read/write access to the committed region of pages.
            /// </summary>
            ExecuteReadWrite = 0x40,
            /// <summary>
            /// Enables execute, read-only, or copy-on-write access to a mapped view of a file mapping object. 
            /// An attempt to write to a committed copy-on-write page results in a private copy of the page being made for the process. 
            /// The private page is marked as PAGE_EXECUTE_READWRITE, and the change is written to the new page.
            /// This flag is not supported by the VirtualAlloc or <see cref="VirtualAllocEx"/> functions. 
            /// </summary>
            ExecuteWriteCopy = 0x80,
            /// <summary>
            /// Disables all access to the committed region of pages. An attempt to read from, write to, or execute the committed region results in an access violation.
            /// This flag is not supported by the CreateFileMapping function.
            /// </summary>
            NoAccess = 0x01,
            /// <summary>
            /// Enables read-only access to the committed region of pages. An attempt to write to the committed region results in an access violation. 
            /// If Data Execution Prevention is enabled, an attempt to execute code in the committed region results in an access violation.
            /// </summary>
            ReadOnly = 0x02,
            /// <summary>
            /// Enables read-only or read/write access to the committed region of pages. 
            /// If Data Execution Prevention is enabled, attempting to execute code in the committed region results in an access violation.
            /// </summary>
            ReadWrite = 0x04,
            /// <summary>
            /// Enables read-only or copy-on-write access to a mapped view of a file mapping object. 
            /// An attempt to write to a committed copy-on-write page results in a private copy of the page being made for the process. 
            /// The private page is marked as PAGE_READWRITE, and the change is written to the new page. 
            /// If Data Execution Prevention is enabled, attempting to execute code in the committed region results in an access violation.
            /// This flag is not supported by the VirtualAlloc or <see cref="VirtualAllocEx"/> functions.
            /// </summary>
            WriteCopy = 0x08,
            /// <summary>
            /// Pages in the region become guard pages. 
            /// Any attempt to access a guard page causes the system to raise a STATUS_GUARD_PAGE_VIOLATION exception and turn off the guard page status. 
            /// Guard pages thus act as a one-time access alarm. For more information, see Creating Guard Pages.
            /// When an access attempt leads the system to turn off guard page status, the underlying page protection takes over.
            /// If a guard page exception occurs during a system service, the service typically returns a failure status indicator.
            /// This value cannot be used with PAGE_NOACCESS.
            /// This flag is not supported by the CreateFileMapping function.
            /// </summary>
            Guard = 0x100,
            /// <summary>
            /// Sets all pages to be non-cachable. Applications should not use this attribute except when explicitly required for a device. 
            /// Using the interlocked functions with memory that is mapped with SEC_NOCACHE can result in an EXCEPTION_ILLEGAL_INSTRUCTION exception.
            /// The PAGE_NOCACHE flag cannot be used with the PAGE_GUARD, PAGE_NOACCESS, or PAGE_WRITECOMBINE flags.
            /// The PAGE_NOCACHE flag can be used only when allocating private memory with the VirtualAlloc, <see cref="VirtualAllocEx"/>, or VirtualAllocExNuma functions. 
            /// To enable non-cached memory access for shared memory, specify the SEC_NOCACHE flag when calling the CreateFileMapping function.
            /// </summary>
            NoCache = 0x200,
            /// <summary>
            /// Sets all pages to be write-combined.
            /// Applications should not use this attribute except when explicitly required for a device. 
            /// Using the interlocked functions with memory that is mapped as write-combined can result in an EXCEPTION_ILLEGAL_INSTRUCTION exception.
            /// The PAGE_WRITECOMBINE flag cannot be specified with the PAGE_NOACCESS, PAGE_GUARD, and PAGE_NOCACHE flags.
            /// The PAGE_WRITECOMBINE flag can be used only when allocating private memory with the VirtualAlloc, <see cref="VirtualAllocEx"/>, or VirtualAllocExNuma functions. 
            /// To enable write-combined memory access for shared memory, specify the SEC_WRITECOMBINE flag when calling the CreateFileMapping function.
            /// </summary>
            WriteCombine = 0x400
        }

        #endregion

        #region ProcessAccessFlags

        public enum ProcessAccessFlags : uint
        {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VirtualMemoryOperation = 0x00000008,
            VirtualMemoryRead = 0x00000010,
            VirtualMemoryWrite = 0x00000020,
            DuplicateHandle = 0x00000040,
            CreateProcess = 0x000000080,
            SetQuota = 0x00000100,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            QueryLimitedInformation = 0x00001000,
            Synchronize = 0x00100000
        }

        #endregion

        #region IMAGE_THUNK_DATA

        [StructLayout(LayoutKind.Sequential)]
        public struct IMAGE_THUNK_DATA
        {
            public static uint SizeOf = (uint)Marshal.SizeOf(typeof(IMAGE_THUNK_DATA));

            public IntPtr ForwarderString;      // PBYTE 
            public IntPtr Function;             // PDWORD
            public IntPtr Ordinal;
            public IntPtr AddressOfData;        // PIMAGE_IMPORT_BY_NAME
        }

        #endregion

        #region IMAGE_DOS_HEADER

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct IMAGE_DOS_HEADER                         // DOS .EXE header
        {
            public ushort e_magic;                      // Magic number
            public ushort e_cblp;                       // Bytes on last page of file
            public ushort e_cp;                         // Pages in file
            public ushort e_crlc;                       // Relocations
            public ushort e_cparhdr;                    // Size of header in paragraphs
            public ushort e_minalloc;                   // Minimum extra paragraphs needed
            public ushort e_maxalloc;                   // Maximum extra paragraphs needed
            public ushort e_ss;                         // Initial (relative) SS value
            public ushort e_sp;                         // Initial SP value
            public ushort e_csum;                       // Checksum
            public ushort e_ip;                         // Initial IP value
            public ushort e_cs;                         // Initial (relative) CS value
            public ushort e_lfarlc;                     // File address of relocation table
            public ushort e_ovno;                       // Overlay number
            public fixed ushort e_res[4];               // Reserved ushorts
            public ushort e_oemid;                      // OEM identifier (for e_oeminfo)
            public ushort e_oeminfo;                    // OEM information; e_oemid specific
            public fixed ushort e_res2[10];             // Reserved ushorts
            public uint e_lfanew;                       // File address of new exe header
        }

        #endregion

        #region IMAGE_NT_HEADERS32

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct IMAGE_NT_HEADERS32
        {
            public uint Signature;
            public IMAGE_FILE_HEADER FileHeader;
            public IMAGE_OPTIONAL_HEADER32 OptionalHeader;
        }

        #endregion

        #region IMAGE_NT_HEADERS64

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct IMAGE_NT_HEADERS64
        {
            public uint Signature;
            public IMAGE_FILE_HEADER FileHeader;
            public IMAGE_OPTIONAL_HEADER64 OptionalHeader;
        }

        #endregion

        #region IMAGE_FILE_HEADER

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct IMAGE_FILE_HEADER
        {
            public ushort Machine;
            public ushort NumberOfSections;
            public uint TimeDateStamp;
            public uint PointerToSymbolTable;
            public uint NumberOfSymbols;
            public ushort SizeOfOptionalHeader;
            public ushort Characteristics;
        }

        #endregion

        #region IMAGE_OPTIONAL_HEADER32

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct IMAGE_OPTIONAL_HEADER32
        {
            public ushort Magic;
            public byte MajorLinkerVersion;
            public byte MinorLinkerVersion;
            public uint SizeOfCode;
            public uint SizeOfInitializedData;
            public uint SizeOfUninitializedData;
            public uint AddressOfEntryPoint;
            public uint BaseOfCode;
            public uint BaseOfData;
            public IntPtr ImageBase;
            public uint SectionAlignment;
            public uint FileAlignment;
            public ushort MajorOperatingSystemVersion;
            public ushort MinorOperatingSystemVersion;
            public ushort MajorImageVersion;
            public ushort MinorImageVersion;
            public ushort MajorSubsystemVersion;
            public ushort MinorSubsystemVersion;
            public uint Win32VersionValue;
            public uint SizeOfImage;
            public uint SizeOfHeaders;
            public uint CheckSum;
            public ushort Subsystem;
            public ushort DllCharacteristics;
            public uint SizeOfStackReserve;
            public uint SizeOfStackCommit;
            public uint SizeOfHeapReserve;
            public uint SizeOfHeapCommit;
            public uint LoaderFlags;
            public uint NumberOfRvaAndSizes;
            public fixed ulong DataDirectory[IMAGE_NUMBEROF_DIRECTORY_ENTRIES];
        }

        #endregion

        #region IMAGE_OPTIONAL_HEADER64

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct IMAGE_OPTIONAL_HEADER64
        {
            public ushort Magic;
            public byte MajorLinkerVersion;
            public byte MinorLinkerVersion;
            public uint SizeOfCode;
            public uint SizeOfInitializedData;
            public uint SizeOfUninitializedData;
            public uint AddressOfEntryPoint;
            public uint BaseOfCode;
            public IntPtr ImageBase;
            public uint SectionAlignment;
            public uint FileAlignment;
            public ushort MajorOperatingSystemVersion;
            public ushort MinorOperatingSystemVersion;
            public ushort MajorImageVersion;
            public ushort MinorImageVersion;
            public ushort MajorSubsystemVersion;
            public ushort MinorSubsystemVersion;
            public uint Win32VersionValue;
            public uint SizeOfImage;
            public uint SizeOfHeaders;
            public uint CheckSum;
            public ushort Subsystem;
            public ushort DllCharacteristics;
            public ulong SizeOfStackReserve;
            public ulong SizeOfStackCommit;
            public ulong SizeOfHeapReserve;
            public ulong SizeOfHeapCommit;
            public uint LoaderFlags;
            public uint NumberOfRvaAndSizes;
            public fixed ulong DataDirectory[IMAGE_NUMBEROF_DIRECTORY_ENTRIES];
        }

        #endregion

        #region IMAGE_DATA_DIRECTORY

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct IMAGE_DATA_DIRECTORY
        {
            public uint VirtualAddress;
            public uint Size;
        }

        #endregion

        #region IMAGE_SECTION_HEADER

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct IMAGE_SECTION_HEADER
        {
            public fixed byte Name[IMAGE_SIZEOF_SHORT_NAME];
            public uint PhysicalAddress;
            public uint VirtualAddress;
            public uint SizeOfRawData;
            public uint PointerToRawData;
            public uint PointerToRelocations;
            public uint PointerToLinenumbers;
            public ushort NumberOfRelocations;
            public ushort NumberOfLinenumbers;
            public uint Characteristics;
        }

        #endregion

        #region IMAGE_BASE_RELOCATION

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct IMAGE_BASE_RELOCATION
        {
            public uint VirtualAddress;
            public uint SizeOfBlock;
        }

        #endregion

        #region IMAGE_IMPORT_DESCRIPTOR

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct IMAGE_IMPORT_DESCRIPTOR
        {
            public uint Characteristics;
            public uint TimeDateStamp;
            public uint ForwarderChain;
            public uint Name;
            public uint FirstThunk;
        }

        #endregion

        #region IMAGE_IMPORT_BY_NAME

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct IMAGE_IMPORT_BY_NAME
        {
            public ushort Hint;
            public fixed byte Name[1];
        }

        #endregion
        #endregion

        #region Constants

        public const uint IMAGE_DOS_SIGNATURE = 0x5A4D;      // MZ
        public const uint IMAGE_OS2_SIGNATURE = 0x454E;      // NE
        public const uint IMAGE_OS2_SIGNATURE_LE = 0x454C;      // LE
        public const uint IMAGE_VXD_SIGNATURE = 0x454C;      // LE
        public const uint IMAGE_NT_SIGNATURE = 0x00004550;  // PE00

        public const int IMAGE_SIZEOF_SHORT_NAME = 8;

        public const int IMAGE_NUMBEROF_DIRECTORY_ENTRIES = 16;

        public const ulong IMAGE_ORDINAL_FLAG64 = 0x8000000000000000;
        public const uint IMAGE_ORDINAL_FLAG32 = 0x80000000;

        public const uint IMAGE_SCN_TYPE_NO_PAD = 0x00000008;  // Reserved.

        public const uint IMAGE_SCN_CNT_CODE = 0x00000020;  // Section contains code.
        public const uint IMAGE_SCN_CNT_INITIALIZED_DATA = 0x00000040;  // Section contains initialized data.
        public const uint IMAGE_SCN_CNT_UNINITIALIZED_DATA = 0x00000080;  // Section contains uninitialized data.

        public const uint IMAGE_SCN_LNK_OTHER = 0x00000100;  // Reserved.
        public const uint IMAGE_SCN_LNK_INFO = 0x00000200;  // Section contains comments or some other type of information.

        public const uint IMAGE_SCN_LNK_REMOVE = 0x00000800;  // Section contents will not become part of image.
        public const uint IMAGE_SCN_LNK_COMDAT = 0x00001000;  // Section contents comdat.

        public const uint IMAGE_SCN_NO_DEFER_SPEC_EXC = 0x00004000;  // Reset speculative exceptions handling bits in the TLB entries for this section.
        public const uint IMAGE_SCN_GPREL = 0x00008000;  // Section content can be accessed relative to GP
        public const uint IMAGE_SCN_MEM_FARDATA = 0x00008000;

        public const uint IMAGE_SCN_MEM_PURGEABLE = 0x00020000;
        public const uint IMAGE_SCN_MEM_16BIT = 0x00020000;
        public const uint IMAGE_SCN_MEM_LOCKED = 0x00040000;
        public const uint IMAGE_SCN_MEM_PRELOAD = 0x00080000;

        public const uint IMAGE_SCN_ALIGN_1BYTES = 0x00100000;  //
        public const uint IMAGE_SCN_ALIGN_2BYTES = 0x00200000;  //
        public const uint IMAGE_SCN_ALIGN_4BYTES = 0x00300000;  //
        public const uint IMAGE_SCN_ALIGN_8BYTES = 0x00400000;  //
        public const uint IMAGE_SCN_ALIGN_16BYTES = 0x00500000;  // Default alignment if no others are specified.
        public const uint IMAGE_SCN_ALIGN_32BYTES = 0x00600000;  //
        public const uint IMAGE_SCN_ALIGN_64BYTES = 0x00700000;  //
        public const uint IMAGE_SCN_ALIGN_128BYTES = 0x00800000;  //
        public const uint IMAGE_SCN_ALIGN_256BYTES = 0x00900000;  //
        public const uint IMAGE_SCN_ALIGN_512BYTES = 0x00A00000;  //
        public const uint IMAGE_SCN_ALIGN_1024BYTES = 0x00B00000;  //
        public const uint IMAGE_SCN_ALIGN_2048BYTES = 0x00C00000;  //
        public const uint IMAGE_SCN_ALIGN_4096BYTES = 0x00D00000;  //
        public const uint IMAGE_SCN_ALIGN_8192BYTES = 0x00E00000;  //
        // Unused                                    0x00F00000;
        public const uint IMAGE_SCN_ALIGN_MASK = 0x00F00000;

        public const uint IMAGE_SCN_LNK_NRELOC_OVFL = 0x01000000;  // Section contains extended relocations.
        public const uint IMAGE_SCN_MEM_DISCARDABLE = 0x02000000;  // Section can be discarded.
        public const uint IMAGE_SCN_MEM_NOT_CACHED = 0x04000000;  // Section is not cachable.
        public const uint IMAGE_SCN_MEM_NOT_PAGED = 0x08000000;  // Section is not pageable.
        public const uint IMAGE_SCN_MEM_SHARED = 0x10000000;  // Section is shareable.
        public const uint IMAGE_SCN_MEM_EXECUTE = 0x20000000;  // Section is executable.
        public const uint IMAGE_SCN_MEM_READ = 0x40000000;  // Section is readable.
        public const uint IMAGE_SCN_MEM_WRITE = 0x80000000;  // Section is writeable.

        public const uint PAGE_NOACCESS = 0x01;
        public const uint PAGE_READONLY = 0x02;
        public const uint PAGE_READWRITE = 0x04;
        public const uint PAGE_WRITECOPY = 0x08;
        public const uint PAGE_EXECUTE = 0x10;
        public const uint PAGE_EXECUTE_READ = 0x20;
        public const uint PAGE_EXECUTE_READWRITE = 0x40;
        public const uint PAGE_EXECUTE_WRITECOPY = 0x80;
        public const uint PAGE_GUARD = 0x100;
        public const uint PAGE_NOCACHE = 0x200;
        public const uint PAGE_WRITECOMBINE = 0x400;

        public const uint MEM_COMMIT = 0x1000;
        public const uint MEM_RESERVE = 0x2000;
        public const uint MEM_DECOMMIT = 0x4000;
        public const uint MEM_RELEASE = 0x8000;
        public const uint MEM_FREE = 0x10000;
        public const uint MEM_PRIVATE = 0x20000;
        public const uint MEM_MAPPED = 0x40000;
        public const uint MEM_RESET = 0x80000;
        public const uint MEM_TOP_DOWN = 0x100000;
        public const uint MEM_WRITE_WATCH = 0x200000;
        public const uint MEM_PHYSICAL = 0x400000;
        public const uint MEM_ROTATE = 0x800000;
        public const uint MEM_LARGE_PAGES = 0x20000000;
        public const uint MEM_4MB_PAGES = 0x80000000;
        public const uint MEM_IMAGE = SEC_IMAGE;

        public const uint SEC_FILE = 0x800000;
        public const uint SEC_IMAGE = 0x1000000;
        public const uint SEC_PROTECTED_IMAGE = 0x2000000;
        public const uint SEC_RESERVE = 0x4000000;
        public const uint SEC_COMMIT = 0x8000000;
        public const uint SEC_NOCACHE = 0x10000000;
        public const uint SEC_WRITECOMBINE = 0x40000000;
        public const uint SEC_LARGE_PAGES = 0x80000000;

        public const int WRITE_WATCH_FLAG_RESET = 0x01;

        // Directory Entries

        public const int IMAGE_DIRECTORY_ENTRY_EXPORT = 0;   // Export Directory
        public const int IMAGE_DIRECTORY_ENTRY_IMPORT = 1;   // Import Directory
        public const int IMAGE_DIRECTORY_ENTRY_RESOURCE = 2;   // Resource Directory
        public const int IMAGE_DIRECTORY_ENTRY_EXCEPTION = 3;   // Exception Directory
        public const int IMAGE_DIRECTORY_ENTRY_SECURITY = 4;   // Security Directory
        public const int IMAGE_DIRECTORY_ENTRY_BASERELOC = 5;   // Base Relocation Table
        public const int IMAGE_DIRECTORY_ENTRY_DEBUG = 6;   // Debug Directory
        public const int IMAGE_DIRECTORY_ENTRY_ARCHITECTURE = 7;   // Architecture Specific Data
        public const int IMAGE_DIRECTORY_ENTRY_GLOBALPTR = 8;   // RVA of GP
        public const int IMAGE_DIRECTORY_ENTRY_TLS = 9;   // TLS Directory
        public const int IMAGE_DIRECTORY_ENTRY_LOAD_CONFIG = 10;   // Load Configuration Directory
        public const int IMAGE_DIRECTORY_ENTRY_BOUND_IMPORT = 11;   // Bound Import Directory in headers
        public const int IMAGE_DIRECTORY_ENTRY_IAT = 12;   // Import Address Table
        public const int IMAGE_DIRECTORY_ENTRY_DELAY_IMPORT = 13;   // Delay Load Import Descriptors
        public const int IMAGE_DIRECTORY_ENTRY_COM_DESCRIPTOR = 14;   // COM Runtime descriptor

        public const int IMAGE_REL_BASED_ABSOLUTE = 0;
        public const int IMAGE_REL_BASED_HIGH = 1;
        public const int IMAGE_REL_BASED_LOW = 2;
        public const int IMAGE_REL_BASED_HIGHLOW = 3;
        public const int IMAGE_REL_BASED_HIGHADJ = 4;
        public const int IMAGE_REL_BASED_MIPS_JMPADDR = 5;
        public const int IMAGE_REL_BASED_MIPS_JMPADDR16 = 9;
        public const int IMAGE_REL_BASED_IA64_IMM64 = 9;
        public const int IMAGE_REL_BASED_DIR64 = 10;


        public const uint DLL_PROCESS_ATTACH = 1;
        public const uint DLL_THREAD_ATTACH = 2;
        public const uint DLL_THREAD_DETACH = 3;
        public const uint DLL_PROCESS_DETACH = 0;

        /* These are the settings of the Machine field. */
        public const ushort IMAGE_FILE_MACHINE_UNKNOWN = 0;
        public const ushort IMAGE_FILE_MACHINE_I860 = 0x014d;
        public const ushort IMAGE_FILE_MACHINE_I386 = 0x014c;
        public const ushort IMAGE_FILE_MACHINE_R3000 = 0x0162;
        public const ushort IMAGE_FILE_MACHINE_R4000 = 0x0166;
        public const ushort IMAGE_FILE_MACHINE_R10000 = 0x0168;
        public const ushort IMAGE_FILE_MACHINE_WCEMIPSV2 = 0x0169;
        public const ushort IMAGE_FILE_MACHINE_ALPHA = 0x0184;
        public const ushort IMAGE_FILE_MACHINE_SH3 = 0x01a2;
        public const ushort IMAGE_FILE_MACHINE_SH3DSP = 0x01a3;
        public const ushort IMAGE_FILE_MACHINE_SH3E = 0x01a4;
        public const ushort IMAGE_FILE_MACHINE_SH4 = 0x01a6;
        public const ushort IMAGE_FILE_MACHINE_SH5 = 0x01a8;
        public const ushort IMAGE_FILE_MACHINE_ARM = 0x01c0;
        public const ushort IMAGE_FILE_MACHINE_THUMB = 0x01c2;
        public const ushort IMAGE_FILE_MACHINE_ARMNT = 0x01c4;
        public const ushort IMAGE_FILE_MACHINE_ARM64 = 0xaa64;
        public const ushort IMAGE_FILE_MACHINE_AM33 = 0x01d3;
        public const ushort IMAGE_FILE_MACHINE_POWERPC = 0x01f0;
        public const ushort IMAGE_FILE_MACHINE_POWERPCFP = 0x01f1;
        public const ushort IMAGE_FILE_MACHINE_IA64 = 0x0200;
        public const ushort IMAGE_FILE_MACHINE_MIPS16 = 0x0266;
        public const ushort IMAGE_FILE_MACHINE_ALPHA64 = 0x0284;
        public const ushort IMAGE_FILE_MACHINE_MIPSFPU = 0x0366;
        public const ushort IMAGE_FILE_MACHINE_MIPSFPU16 = 0x0466;
        public const ushort IMAGE_FILE_MACHINE_AXP64 = IMAGE_FILE_MACHINE_ALPHA64;
        public const ushort IMAGE_FILE_MACHINE_TRICORE = 0x0520;
        public const ushort IMAGE_FILE_MACHINE_CEF = 0x0cef;
        public const ushort IMAGE_FILE_MACHINE_EBC = 0x0ebc;
        public const ushort IMAGE_FILE_MACHINE_AMD64 = 0x8664;
        public const ushort IMAGE_FILE_MACHINE_M32R = 0x9041;
        public const ushort IMAGE_FILE_MACHINE_CEE = 0xc0ee;

        #endregion
    }
}
