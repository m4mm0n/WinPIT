using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Engine;
using Engine.Extensions;
using Engine.Injectors;
using Engine.ProcessCore;
using Module = Engine.ProcessCore.Module;

namespace Injectors.ManualMap
{
    public class Injector : IInjector
    {
        private Logger log = new Logger(LoggerType.Console_File, "Injector.MM");

        public string SelfFileName
        {
            get { return Path.GetFileName(Assembly.GetExecutingAssembly().Location); }
        }

        public string UniqueId
        {
            get
            {
                return "Injectors.MM-" +
                       QuickExt.GetHash(
                           Encoding.UTF8.GetBytes(UniqueName +
                                                  Marshal.GetTypeLibGuidForAssembly(Assembly.GetExecutingAssembly())
                                                      .ToString()), HashType.MD5);
            }
        }

        public string UniqueName
        {
            get { return "Injection by Manual Mapping"; }
        }

        public string About
        {
            get
            {
                return
                    "API: Manual Mapping" + Environment.NewLine +
                    "DLL: -" + Environment.NewLine + Environment.NewLine +
                    "Stealth: VERY" + Environment.NewLine +
                    "Kernel/System/Normal Access: System" + Environment.NewLine +
                    "Original Author: striek (x64)"
                    ;
            }
        }
        public Module InjectedModule { get; set; }
        private Core targetProc;
        public IntPtr Inject(Core targetProcess, string filePath)
        {
            InjectedModule = new Module(filePath);
            targetProc = targetProcess;

            if(targetProcess.Is64bit)
            {
                try
                {

                    log.Log(LogType.Normal, "Starting injection of the module...");
                    LinkedModules = GetModules();

                    var remoteImage = MapImage(filePath, InjectedModule.DllBytes);
                    CallEntrypoint(InjectedModule.DllBytes, remoteImage);

                    return (IntPtr) remoteImage;
                }
                catch(Exception ex)
                {
                    log.Log(ex, "Something went wrong...");
                    return IntPtr.Zero;
                }
            }
            else
            {
                log.Log(LogType.Error, "32-bit/x86 is NOT supported yet!");
                return IntPtr.Zero;
            }

        }

        private Dictionary<string, ulong> MappedModules = new Dictionary<string, ulong>(StringComparer.InvariantCultureIgnoreCase);
        private Dictionary<string, byte[]> MappedRawImages = new Dictionary<string, byte[]>(StringComparer.InvariantCultureIgnoreCase);
        private Dictionary<string, ulong> LinkedModules = new Dictionary<string, ulong>(StringComparer.InvariantCultureIgnoreCase);

        ulong CreateSection(WinAPI.MemoryProtection memoryProtection, long size)
        {
            var result = WinAPI.NtCreateSection(out ulong sectionHandle, WinAPI.ACCESS_MASK.GENERIC_ALL, 0, out size, memoryProtection, 0x8000000 /*SEC_COMMIT*/, 0);

            if (result != 0)
            {
                log.Log(LogType.Failure, $"CreateSection - NtCreateSection() failed - {result.ToString("x2")}");
                return 0;
            }

            return sectionHandle;
        }
        ulong MapSection(IntPtr procHandle, ulong sectionHandle, WinAPI.MemoryProtection memoryProtection)
        {
            ulong memoryPointer = 0;
            var result = WinAPI.NtMapViewOfSection(sectionHandle, procHandle, ref memoryPointer, 0, 0, 0, out uint viewSize, 2, 0, memoryProtection);
            if (result != 0)
            {
                log.Log(LogType.Failure, $"MapSection - NtMapViewOfSection() failed - {result.ToString("x2")}");
                return 0;
            }

            return memoryPointer;
        }
        ulong MapImage(string imageName, byte[] rawImage)
        {
            log.Log(LogType.Normal, $"Mapping {imageName}");

            unsafe
            {
                // GET HEADERS
                WinAPI.GetImageHeaders(rawImage, out WinAPI.IMAGE_DOS_HEADER dosHeader,
                    out WinAPI.IMAGE_FILE_HEADER fileHeader, out WinAPI.IMAGE_OPTIONAL_HEADER64 optionalHeader,
                    out WinAPI.IMAGE_NT_HEADERS64* ntHeaderz);

                // CREATE A MEMORY SECTION IN TARGET PROCESS
                ulong sectionHandle =
                    CreateSection(WinAPI.MemoryProtection.ExecuteReadWrite, optionalHeader.SizeOfImage);

                // MAP THE SECTION INTO BOTH OUR OWN AND THE TARGET PROCESS
                // THIS WILL RESULT IN A MIRRORED MEMORY SECTION, WHERE EVERY CHANGE
                // TO THE LOCAL SECTION WILL ALSO CHANGE IN THE TARGET PROCESS
                // AND VICE VERSA
                ulong remoteImage = MapSection(targetProc.ProcessHandle, sectionHandle,
                    WinAPI.MemoryProtection.ExecuteReadWrite);
                ulong localImage = MapSection(WinAPI.GetCurrentProcess(), sectionHandle,
                    WinAPI.MemoryProtection.ExecuteReadWrite);

                // SAVE MAPPED EXECUTABLES IN A LIST
                // SO WE CAN RECURSIVELY MAP DEPENDENCIES, AND THEIR DEPENDENCIES
                // WITHOUT BEING STUCK IN A LOOP :)
                MappedModules[imageName] = remoteImage;
                MappedRawImages[imageName] = rawImage;

                // ADD LOADER REFERENCE
                //if (imageName == Options.LoaderImagePath)
                //{
                //    if (Options.CreateLoaderReference)
                //        AddLoaderEntry(imageName, remoteImage);
                //}
                //else // ALWAYS CREATE REFERENCE FOR DEPENDENCIES
                //{
                AddLoaderEntry(targetProc, imageName, remoteImage);
                //}

                // COPY HEADERS TO SECTION
                Marshal.Copy(rawImage, 0, (IntPtr) localImage, (int) optionalHeader.SizeOfHeaders);

                // DO THE ACTUAL MANUALMAPPING
                WriteImageSections(rawImage, dosHeader, localImage, fileHeader.NumberOfSections);
                RelocateImageByDelta(localImage, remoteImage, optionalHeader, ntHeaderz);
                FixImportTable(localImage, optionalHeader, ntHeaderz);

                //// NUKE HEADERS
                //// TODO: DONT WRITE THEM IN THE FIRST PLACE
                //if (Options.EraseHeaders)
                //{
                //    byte[] headerBuffer = new byte[(int)optionalHeader.SizeOfHeaders];
                //    NTM.RandomEngine.NextBytes(headerBuffer);

                //    Marshal.Copy(headerBuffer, 0, (IntPtr)localImage, (int)optionalHeader.SizeOfHeaders);
                //}

                WinAPI.CloseHandle((IntPtr) sectionHandle);
                WinAPI.NtUnmapViewOfSection(WinAPI.GetCurrentProcess(), localImage);
                //Process.GetCurrentProcess().UnmapSection(localImage);

                return remoteImage;
            }
        }
        unsafe void RelocateImageByDelta(ulong localImage, ulong remoteImage, WinAPI.IMAGE_OPTIONAL_HEADER64 optionalHeader, WinAPI.IMAGE_NT_HEADERS64* ntHeaders)
        {
            unsafe
            {
                // https://github.com/DarthTon/Blackbone/blob/master/src/BlackBone/ManualMap/MMap.cpp#L691
                WinAPI.IMAGE_DATA_DIRECTORY* directory =
                    WinAPI.GET_HEADER_DIRECTORY(ntHeaders, WinAPI.IMAGE_DIRECTORY_ENTRY_BASERELOC);

                WinAPI.IMAGE_BASE_RELOCATION* baseRelocation =
                    (WinAPI.IMAGE_BASE_RELOCATION*) (localImage + directory->VirtualAddress);

                var memoryDelta = remoteImage - (ulong)optionalHeader.ImageBase;
                int relocBaseSize = Marshal.SizeOf<WinAPI.IMAGE_BASE_RELOCATION>();

                while (baseRelocation->SizeOfBlock > 0)
                {
                    // START OF RELOCATION
                    ulong relocStartAddress = localImage + baseRelocation->VirtualAddress;

                    // AMOUNT OF RELOCATIONS IN THIS BLOCK
                    int relocationAmount =
                        ((int) baseRelocation->SizeOfBlock - relocBaseSize /*DONT COUNT THE MEMBERS*/) /
                        sizeof(ushort) /*SIZE OF DATA*/;

                    // ITERATE ALL RELOCATIONS AND FIX THE HIGHLOWS
                    for (int i = 0; i < relocationAmount; i++)
                    {
                        // GET RELOCATION DATA
                        var data = GetRelocationData(i);

                        // WORD Offset : 12; 
                        // WORD Type   : 4;
                        var fixOffset = data & 0x0FFF;
                        var fixType = data & 0xF000;

                        // THIS IS A HIGHLOW ACCORDING TO MY GHETTO MASK
                        // ¯\_(ツ)_/¯
                        if (fixType == 40960)
                            *(ulong*) (relocStartAddress + (uint) fixOffset) +=
                                memoryDelta; // ADD MEMORY DELTA TO SPECIFIED ADDRESS
                    }

                    // GET THE NEXT BLOCK
                    baseRelocation = (WinAPI.IMAGE_BASE_RELOCATION*) ((ulong) baseRelocation + baseRelocation->SizeOfBlock);
                }

                ushort GetRelocationData(int index) =>
                    *(ushort*) ((long) baseRelocation + Marshal.SizeOf<WinAPI.IMAGE_BASE_RELOCATION>() +
                                sizeof(ushort) * index);
            }
        }
        void WriteImageSections(byte[] rawImage, WinAPI.IMAGE_DOS_HEADER dosHeader, ulong localImage, int numberOfSections)
        {
            unsafe
            {
                // GET POINTER TO FIRST MEMORY SECTION - LOCATED RIGHT AFTER HEADERS
                WinAPI.IMAGE_SECTION_HEADER* sections = WinAPI.GetFirstSection(localImage, dosHeader);

                // ITERATE PE SECTIONS
                for (int index = 0; index < numberOfSections; index++)
                {
                    if (sections[index].SizeOfRawData > 0)
                    {
                        ulong localSectionPointer = localImage + sections[index].VirtualAddress;
                        Marshal.Copy(rawImage, (int) sections[index].PointerToRawData, (IntPtr) localSectionPointer,
                            (int) sections[index].SizeOfRawData);
                        //Log.LogInfo($"{sections[index].SectionName} - {sections[index].SizeOfRawData}");
                    }
                }
            }
        }
        void AddLoaderEntry(Core hProc, string imageName, ulong moduleHandle)
        {
            log.Log(LogType.Normal, $"Linking {imageName}({moduleHandle.ToString("x2")}) to module list");

            var imagePath = Exts.FindDll(imageName) ?? imageName;

            var listBase = hProc.GetLoaderData().InLoadOrderModuleList;
            var lastEntry = hProc.Read<WinAPI._LDR_DATA_TABLE_ENTRY>((IntPtr)listBase.Blink);
            var allocatedDllPath = (ulong)hProc.AllocateAndWriteBytes(Encoding.Unicode.GetBytes(imagePath));

            // CRAFT CUSTOM LOADER ENTRY
            var fileName = Path.GetFileName(imagePath);
            WinAPI._LDR_DATA_TABLE_ENTRY myEntry = new WinAPI._LDR_DATA_TABLE_ENTRY()
            {
                InLoadOrderLinks = new WinAPI._LIST_ENTRY()
                {
                    Flink = lastEntry.InLoadOrderLinks.Flink,
                    Blink = listBase.Flink
                },
                InMemoryOrderLinks = lastEntry.InMemoryOrderLinks,
                InInitializationOrderLinks = lastEntry.InInitializationOrderLinks,
                DllBase = moduleHandle,
                EntryPoint = 0,
                SizeOfImage = (ulong)MappedRawImages[imageName].Length,
                FullDllName = new WinAPI.UNICODE_STRING(imagePath) { Buffer = allocatedDllPath },
                BaseDllName = new WinAPI.UNICODE_STRING(fileName) { Buffer = allocatedDllPath + (ulong)imagePath.IndexOf(fileName) * 2/*WIDE CHAR*/ },
                Flags = lastEntry.Flags,
                LoadCount = lastEntry.LoadCount,
                TlsIndex = lastEntry.TlsIndex,
                Reserved4 = lastEntry.Reserved4,
                CheckSum = lastEntry.CheckSum,
                TimeDateStamp = lastEntry.TimeDateStamp,
                EntryPointActivationContext = lastEntry.EntryPointActivationContext,
                PatchInformation = lastEntry.PatchInformation,
                ForwarderLinks = lastEntry.ForwarderLinks,
                ServiceTagLinks = lastEntry.ServiceTagLinks,
                StaticLinks = lastEntry.StaticLinks,
            };

            // ALLOCATE AND WRITE OUR MODULE ENTRY
            var newEntryPointer = hProc.AllocateAndWriteBytes(Exts.GetBytes(myEntry));

            // SET LAST LINK IN InLoadOrderLinks CHAIN TO POINT TO OUR ENTRY
            lastEntry.InLoadOrderLinks.Flink = (ulong)newEntryPointer;
            hProc.Write(lastEntry, (IntPtr)listBase.Blink);

        }
        unsafe void CallEntrypoint(byte[] rawImage, ulong moduleHandle)
        {
            // GET HEADERS
            WinAPI.GetImageHeaders(rawImage, out WinAPI.IMAGE_DOS_HEADER dosHeader, out WinAPI.IMAGE_FILE_HEADER fileHeader, out WinAPI.IMAGE_OPTIONAL_HEADER64 optionalHeader, out WinAPI.IMAGE_NT_HEADERS64* ntHeaderz);

            // GET DLLMAIN
            ulong entrypoint = moduleHandle + optionalHeader.AddressOfEntryPoint;

            if (optionalHeader.AddressOfEntryPoint == 0)
            {
                log.Log(LogType.Error, $"Invalid Entrypoint - skipping {moduleHandle.ToString("x2")}");
                return;
            }

            log.Log(LogType.Normal, "AddressOfEntryPoint", optionalHeader.AddressOfEntryPoint.ToString("x2"));

            // GET PROPER SHELLCODE FOR EXECUTION TYPE
            byte[] shellcode = CallDllMain(moduleHandle, entrypoint, false);

            // EXECUTE DLLMAIN
            //switch (TypeOfExecution)
            //{
            //    #region Create Thread
            //    case ExecutionType.CreateThread:
            //        // INJECT OUR SHELLCODE -> REMOTE PROCESS TO CALL DLLMAIN REMOTELY :)
            //        TargetProcess.InjectShellcode(shellcode);
            //        break;
            //    #endregion
            //}
            var callCode = targetProc.AllocateAndWriteBytes(shellcode);
            if (callCode == IntPtr.Zero)
            {
                log.Log(LogType.Error, "Failed on allocating and injecting shellcode bytes into memory: {0}",
                    Marshal.GetLastWin32Error().ToString("X"));
                return;
            }

            var callThread = targetProc.CreateThread(callCode);
            if (callThread == IntPtr.Zero)
            {
                log.Log(LogType.Error, "Failed on creating a remote thread to our shellcode: {0}",
                    Marshal.GetLastWin32Error().ToString("X"));
                return;
            }

            WinAPI.WaitForSingleObject((ulong) callThread, uint.MaxValue);
            WinAPI.VirtualFreeEx(targetProc.ProcessHandle, callThread, 0, (int) WinAPI.AllocationType.Release);
        }

        unsafe void FixImportTable(ulong localImage, WinAPI.IMAGE_OPTIONAL_HEADER64 optionalHeader, WinAPI.IMAGE_NT_HEADERS64* ntHeaders)
        {
            unsafe
            {
                WinAPI.IMAGE_DATA_DIRECTORY* directory =
                    WinAPI.GET_HEADER_DIRECTORY(ntHeaders, WinAPI.IMAGE_DIRECTORY_ENTRY_IMPORT);
                WinAPI.IMAGE_IMPORT_DESCRIPTOR* importDescriptor =
                    (WinAPI.IMAGE_IMPORT_DESCRIPTOR*) (localImage + directory->VirtualAddress);
                for (; importDescriptor->FirstThunk > 0; ++importDescriptor)
                {
                    string libraryName = Marshal.PtrToStringAnsi((IntPtr) (localImage + importDescriptor->Name));

                    // RECODE THIS, THIS IS STUPID & DANGEROUS
                    // I AM ONLY DOING THIS BECAUSE OF API-SET DLLS
                    // I COULDNT BE ARSED TO MAKE A PINVOKE FOR ApiSetResolveToHost
                    ulong localLibraryHandle = (ulong)WinAPI.LoadLibrary(libraryName);
                    libraryName = GetModuleBaseName(WinAPI.GetCurrentProcess(), localLibraryHandle)
                        .ToLower();

                    // IF WE MAPPED DEPENDENCY EARLIER, WE SHOULD USE RVA 
                    // INSTEAD OF STATIC MEMORY ADDRESS
                    bool mappedDependency = MappedModules.TryGetValue(libraryName, out ulong remoteLibraryHandle);
                    bool linkedInProcess = LinkedModules.TryGetValue(libraryName, out remoteLibraryHandle);

                    if (!mappedDependency && !linkedInProcess) // DEPENDENCY NOT FOUND, MAP IT!
                    {
                        string dependencyPath = Exts.FindDll(libraryName);

                        // SKIP IF DEPENDENCY COULDN'T BE FOUND
                        if (dependencyPath == null)
                            continue;

                        // [8:44 PM] markhc: i had something similar
                        // [8:44 PM] markhc: it was deep inside CRT initialization(edited)
                        // [8:45 PM] Ch40zz: how did you fix it?
                        // [8:46 PM] markhc: i didnt fix it
                        // [8:46 PM] markhc: i thought it was something wrong with my manual mapper code, but i couldnt figure out what was it
                        // [8:46 PM] markhc: so i threw it all away
                        if (libraryName == "msvcp140.dll")
                        {
                            //var tempOptions = Options;
                            //tempOptions.EraseHeaders = false;

                            //new LoadLibraryInjection(TargetProcess, TypeOfExecution, tempOptions).InjectImage(
                            //    dependencyPath);
                            InjectDependency(dependencyPath);

                            --importDescriptor;
                            continue;
                        }

                        remoteLibraryHandle = MapImage(libraryName, File.ReadAllBytes(dependencyPath));
                        mappedDependency = true;
                    }

                    ulong* functionAddress = (ulong*) (localImage + importDescriptor->FirstThunk);
                    ulong* importEntry = (ulong*) (localImage + importDescriptor->Characteristics);

                    do
                    {
                        ulong procNamePointer = *importEntry < 0x8000000000000000 /*IMAGE_ORDINAL_FLAG64*/
                            ? // IS ORDINAL?
                            localImage + *importEntry + sizeof(ushort) /*SKIP HINT*/
                            : // FUNCTION BY NAME
                            *importEntry & 0xFFFF; // ORDINAL

                        var localFunctionPointer = (ulong)WinAPI.GetProcAddress((IntPtr)localLibraryHandle, (uint)procNamePointer);
                        var rva = localFunctionPointer - localLibraryHandle;

                        // SET NEW FUNCTION POINTER
                        *functionAddress = mappedDependency ? remoteLibraryHandle + rva : localFunctionPointer;

                        // GET NEXT ENTRY
                        ++functionAddress;
                        ++importEntry;
                    } while (*importEntry > 0);
                }
            }
        }
        string GetModuleBaseName(IntPtr processHandle, ulong moduleHandle)
        {
            var name = new StringBuilder(1024);
            WinAPI.GetModuleBaseName(processHandle, moduleHandle, name, 1024);

            return name.ToString();
        }
        unsafe Dictionary<string, ulong> GetModules()
        {
            var result = new Dictionary<string, ulong>();

            ulong[] moduleHandleArray = new ulong[1000];

            fixed (ulong* hMods = moduleHandleArray)
            {
                if (WinAPI.EnumProcessModules(targetProc.ProcessHandle, (ulong)hMods, (uint)(sizeof(ulong) * moduleHandleArray.Length), out uint cbNeeded) > 0)
                {
                    for (int moduleIndex = 0; moduleIndex < cbNeeded / sizeof(ulong); moduleIndex++)
                    {
                        string name = GetModuleBaseName(targetProc.ProcessHandle, moduleHandleArray[moduleIndex]);

                        result[name.ToLower()] = moduleHandleArray[moduleIndex];

                        //if (String.Equals(name, moduleName, StringComparison.InvariantCultureIgnoreCase))
                        //    return moduleHandleArray[moduleIndex];
                    }
                }
            }

            return result;
        }

        bool InjectDependency(string dependencyFileNamePath)
        {
            using (var depMod = new Module(dependencyFileNamePath))
            {
                var loadProc = targetProc.GetLoadLibraryPtr();
                if (loadProc == IntPtr.Zero)
                {
                    log.Log(LogType.Failure, "Failed to load and initiate LoadLibrary: {0}",
                        Marshal.GetLastWin32Error().ToString("X"));
                    return false;
                }
                var allocated = targetProc.AllocateAndWriteBytes(depMod.DllBytes);
                if (allocated == IntPtr.Zero)
                {
                    log.Log(LogType.Failure, "Failed to allocate and/or write dependency bytes to memory: {0}",
                        Marshal.GetLastWin32Error().ToString("X"));
                    return false;
                }

                var crt = targetProc.CreateThread(loadProc, allocated);
                if (crt == IntPtr.Zero)
                {
                    log.Log(LogType.Failure, "Failed to inject dependency - CreateRemoteThread was a failure: {0}",
                        Marshal.GetLastWin32Error().ToString("X"));
                    return false;
                }

                log.Log(LogType.Success, "Dependency {0} injected, and loaded succesfully!",
                    Path.GetFileName(dependencyFileNamePath));
                return true;
            }
        }

        unsafe byte[] CallLoadLibrary(ulong allocatedImagePath, ulong loadLibraryPointer)
        {
            // threadhijack_loadlibrary_x64.asm
            byte[] shellcode = new byte[]
            {
                0x9C, 0x50, 0x53, 0x51, 0x52, 0x41, 0x50, 0x41, 0x51, 0x41, 0x52, 0x41, 0x53, // push     REGISTERS
                0x48, 0x83, 0xEC, 0x28,                                                       // sub      RSP, 0x28
                0x48, 0xB9, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,                   // movabs   RCX, 0x0000000000000000 ; Image path
                0x48, 0xB8, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,                   // movabs   RAX, 0x0000000000000000 ; Pointer to LoadLibrary
                0xFF, 0xD0,                                                                   // call     RAX
                0x48, 0x83, 0xC4, 0x28,                                                       // add      RSP, 0x28
                0x41, 0x5B, 0x41, 0x5A, 0x41, 0x59, 0x41, 0x58, 0x5A, 0x59, 0x5B, 0x58, 0x9D, // pop      REGISTER
                0xC3                                                                          // ret
            };

            // WRITE POINTERS TO SHELLCODE
            fixed (byte* shellcodePointer = shellcode)
            {
                *(ulong*)(shellcodePointer + 19) = allocatedImagePath;
                *(ulong*)(shellcodePointer + 29) = loadLibraryPointer;
            }

            return shellcode;
        }
        unsafe byte[] CallDllMain(ulong remoteImage, ulong entrypoint, bool hijackSafe)
        {
            byte[] shellcode;

            if (hijackSafe)
            {
                // threadhijack_dllmain_x64.asm
                shellcode = new byte[]
                {
                        0x9C, 0x50, 0x53, 0x51, 0x52, 0x41, 0x50, 0x41, 0x51, 0x41, 0x52, 0x41, 0x53,   // push     REGISTERS
                        0x48, 0x83, 0xEC, 0x28,                                                         // sub      RSP, 0x28
                        0x48, 0xB9, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,                     // movabs   RCX, 0x0000000000000000 
                        0x48, 0xC7, 0xC2, 0x01, 0x00, 0x00, 0x00,                                       // mov      rdx, 0x1
                        0x4D, 0x31, 0xC0,                                                               // xor      r8, r8
                        0x48, 0xB8, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,                     // movabs   RAX, 0x0000000000000000
                        0xFF, 0xD0,                                                                     // call     RAX
                        0x48, 0x83, 0xC4, 0x28,                                                         // add      RSP, 0x28
                        0x41, 0x5B, 0x41, 0x5A, 0x41, 0x59, 0x41, 0x58, 0x5A, 0x59, 0x5B, 0x58, 0x9D,   // pop      REGISTERS
                        0xC3
                };

                // WRITE POINTERS TO SHELLCODE
                fixed (byte* shellcodePointer = shellcode)
                {
                    *(ulong*)(shellcodePointer + 19) = remoteImage;
                    *(ulong*)(shellcodePointer + 39) = entrypoint;
                }
            }
            else
            {
                // call_dllmain_x64.asm
                shellcode = new byte[]
                {
                        0x48, 0x83, 0xEC, 0x28,                                         // sub      RSP, 0x28
                        0x48, 0xB9, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,     // movabs   RCX, 0x0000000000000000
                        0x48, 0xC7, 0xC2, 0x01, 0x00, 0x00, 0x00,                       // mov      rdx, 0x1
                        0x4D, 0x31, 0xC0,                                               // xor      r8, r8
                        0x48, 0xB8, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,     // movabs   RAX, 0x0000000000000000
                        0xFF, 0xD0,                                                     // call     RAX
                        0x48, 0x83, 0xC4, 0x28,                                         // add      RSP, 0x28
                        0xC3                                                            // ret
                    };

                // WRITE POINTERS TO SHELLCODE
                fixed (byte* shellcodePointer = shellcode)
                {
                    *(ulong*)(shellcodePointer + 6) = remoteImage;
                    *(ulong*)(shellcodePointer + 26) = entrypoint;
                }
            }



            return shellcode;
        }
    }
}
