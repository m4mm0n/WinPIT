using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using AsmResolver;

namespace Engine.Assembly
{
    /// <summary>
    ///     General .NET Method Structure
    /// </summary>
    public struct PENetMethod
    {
        public string Name { get; }
        public bool IsStatic { get; }

        public PENetMethod(string name, bool isStatic)
        {
            Name = name;
            IsStatic = isStatic;
        }
    }

    /// <summary>
    ///     General .NET Type Structure
    /// </summary>
    public struct PENetType
    {
        public PENetMethod[] Methods => hasMethods ? methods.ToArray() : null;
        private static List<PENetMethod> methods = new List<PENetMethod>();
        private bool hasMethods { get; }

        public string Name { get; }
        public bool IsClass { get; }

        public PENetType(Type typeToAdd)
        {
            Name = typeToAdd.FullName;
            IsClass = typeToAdd.IsClass;
            if (typeToAdd.GetMethods().Length > 0)
            {
                hasMethods = true;
                try
                {
                    foreach (var meth in typeToAdd.GetMethods())
                    {
                        methods.Add(new PENetMethod(meth.Name, meth.IsStatic));
                    }
                }
                catch { }
            }
            else
            {
                hasMethods = false;
            }
        }
    }

    /// <summary>
    ///     General Export Structure
    /// </summary>
    public struct PEExport
    {
        /// <summary>
        ///     Name of Export
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     RVA of Export
        /// </summary>
        public string Address => is64 ? address.ToString("X16") : address.ToString("X8");

        private readonly bool is64;
        private readonly ulong address;

        /// <summary>
        ///     Initializes a new struct for a given export's details
        /// </summary>
        /// <param name="_name">Name of Export</param>
        /// <param name="_address">RVA of Export</param>
        /// <param name="_is64">Set True if 64bit, Set False if Not</param>
        public PEExport(string _name, ulong _address, bool _is64)
        {
            address = _address;
            Name = _name;
            is64 = _is64;
        }

        /// <summary>
        ///     Initializes a new struct for a given export's details (default 32bit)
        /// </summary>
        /// <param name="_name">Name of Export</param>
        /// <param name="_address">RVA of Export</param>
        public PEExport(string _name, uint _address)
        {
            address = _address;
            Name = _name;
            is64 = false;
        }
    }

    /// <summary>
    ///     General Import Structure
    /// </summary>
    public struct PEImport
    {
        /// <summary>
        ///     Name of Import
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Address of Import
        /// </summary>
        public string Address => is64 ? address.ToString("X16") : address.ToString("X8");

        private readonly ulong address;
        private readonly bool is64;

        /// <summary>
        ///     Initializes a new struct for a given Import's details
        /// </summary>
        /// <param name="_name">Name of the Import</param>
        /// <param name="_address">Address of the Import</param>
        /// <param name="_is64">Set True if 64bit, Set False if Not</param>
        public PEImport(string _name, ulong _address, bool _is64)
        {
            address = _address;
            Name = _name;
            is64 = _is64;
        }

        /// <summary>
        ///     Initializes a new struct for a given Import's details (default 32bit)
        /// </summary>
        /// <param name="_name">Name of the Import</param>
        /// <param name="_address">Address of the Import</param>
        public PEImport(string _name, uint _address)
        {
            address = _address;
            Name = _name;
            is64 = false;
        }
    }

    /// <summary>
    ///     An easy wrapper around the AsmResolver library for reading a PE
    /// </summary>
    public class PEReader : IDisposable
    {
        private readonly Logger log; // = new Logger(LoggerType.Console_File, "Assembly.PEReader");
        private WindowsAssembly wasm;
        private byte[] asmBytes;

        /// <summary>
        ///     Initializes PEReader using the filepath of a PE to read
        /// </summary>
        /// <param name="fileName">Full File-Path of PE</param>
        public PEReader(string fileName)
        {
            log = new Logger(Environment.UserInteractive ? LoggerType.Console_File : LoggerType.File,
                "Assembly.PEReader");

            log.Log(LogType.Normal, "PE read using File-load...");

            try
            {
                asmBytes = File.ReadAllBytes(fileName);
                wasm = WindowsAssembly.FromFile(fileName);
            }
            catch (Exception ex)
            {
                log.Log(ex, "Exception on loading {0}", Path.GetFileName(fileName));
            }
        }

        /// <summary>
        ///     Initializes the PEReader using a byte-array of a given PE
        /// </summary>
        /// <param name="fileBytes">PE's byte-array</param>
        public PEReader(byte[] fileBytes)
        {
            log.Log(LogType.Normal, "PE read using file-bytes...");

            try
            {
                asmBytes = fileBytes;
                wasm = WindowsAssembly.FromBytes(fileBytes);
            }
            catch (Exception e)
            {
                log.Log(e, "Exception on loading file");
            }
        }

        /// <summary>
        ///     Returns wether the PE's base-code is a Managed .NET assembly or not
        /// </summary>
        public bool IsNet => wasm.NetDirectory != null;

        /// <summary>
        ///     Returns wether the PE's architecture is 64-bit or not
        /// </summary>
        public bool Is64 => (wasm.NtHeaders.FileHeader.Machine == ImageMachineType.Amd64) |
                            (wasm.NtHeaders.FileHeader.Machine == ImageMachineType.Ia64);

        /// <summary>
        ///     Returns wether PE is a DLL or not
        /// </summary>
        public bool IsDLL => wasm.NtHeaders.FileHeader.Characteristics == ImageCharacteristics.Dll;

        /// <summary>
        ///     Returns wether the PE is a driver or not
        /// </summary>
        public bool IsDriver => wasm.NtHeaders.FileHeader.Characteristics == ImageCharacteristics.System;

        /// <summary>
        ///     Gets an array of the current PE's exports - if none is found, a null is returned
        /// </summary>
        public PEExport[] GetExports
        {
            get
            {
                if (wasm.ExportDirectory != null)
                {
                    log.Log(LogType.Debug, "PE has exports...");

                    var xp = new List<PEExport>();
                    foreach (var exp in wasm.ExportDirectory.Exports) xp.Add(new PEExport(exp.Name, exp.Rva, Is64));

                    log.Log(LogType.Debug, "Found {0} exports...", xp.Count.ToString());

                    return xp.ToArray();
                }

                return null;
            }
        }

        /// <summary>
        ///     Gets an array of the current PE's imports - if none is found, a null is returned
        /// </summary>
        public PEImport[] GetImports
        {
            get
            {
                if (wasm.ImportDirectory != null)
                {
                    log.Log(LogType.Debug, "PE has imports...");

                    var imp = new List<PEImport>();
                    foreach (var imps in wasm.ImportDirectory.ModuleImports)
                        imp.Add(new PEImport(imps.Name, imps.ImportLookupTableRva, Is64));

                    log.Log(LogType.Debug, "Found {0} imports...", imp.Count.ToString());

                    return imp.ToArray();
                }

                return null;
            }
        }

        /// <summary>
        ///     Gets and array of the current PE's .NET types - if none is found, a null is returned
        /// </summary>
        public PENetType[] GetNetTypes
        {
            get
            {
                if (IsNet)
                {
                    var typs = new List<PENetType>();

                    var nAsm = System.Reflection.Assembly.Load(asmBytes);
                    try
                    {
                        foreach (var tp in nAsm.GetTypes())
                        {
                            if (tp.IsPublic && tp.IsVisible && tp.GetMethods().Length > 0)
                                typs.Add(new PENetType(tp));
                        }

                        log.Log(LogType.Success, "Succesfully read {0} types from the PE...", typs.Count.ToString());

                        return typs.ToArray();
                    }
                    catch { }

                    log.Log(LogType.Failure, "Something went wrong when reading the types of the .NET assembly: {0}",
                        Marshal.GetLastWin32Error().ToString("X"));

                    return null;
                }
                log.Log(LogType.Error, "This PE is NOT a .NET assembly!");

                return null;
            }
        }

        /// <summary>
        ///     Disposes and clears the memory of the read PE
        /// </summary>
        public void Dispose()
        {
            wasm = null;
            log?.Dispose();
        }
    }
}