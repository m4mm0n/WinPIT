using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AsmResolver;
using Engine.ProcessCore;
using PeNet;

namespace Engine.Assembly
{
    public class ImportExportReader
    {
        private Core hProc;
        private readonly Logger log;
        private readonly PeFile pef;
        private WindowsAssembly wasm;

        public ImportExportReader(Core process)
        {
            log = new Logger(LoggerType.Console_File, "Assembly.ImportExportReader");
            var tmpBytes = process.ReadBytes(process.BaseAddress, process.SizeOfProcess);
            if (tmpBytes != null)
            {
                log.Log(LogType.Success, "Successfully read PE header...");
                //wasm = WindowsAssembly.FromBytes(tmpBytes);
                try
                {
                    pef = new PeFile(tmpBytes);

                    //Imports = new Dictionary<string, IntPtr>((pef.HasValidImportDir && pef.ImportedFunctions != null) ? pef.ImportedFunctions.Length : 0);
                    //Exports = new Dictionary<string, IntPtr>((pef.HasValidExportDir && pef.ExportedFunctions != null) ? pef.ExportedFunctions.Length : 0);

                    //if (pef.HasValidImportDir && pef.ImportedFunctions != null)
                    //    foreach (var imp in pef.ImportedFunctions)
                    //        Imports.Add(imp.Name, new IntPtr(imp.Hint));
                    //if (pef.HasValidExportDir && pef.ExportedFunctions != null)
                    //    foreach (var exp in pef.ExportedFunctions)
                    //        Exports.Add(exp.Name, new IntPtr(exp.Address));
                }
                catch (Exception ex)
                {
                    log.Log(ex, "Something went wrong loading the PE information...");
                }
                //Imports = new Dictionary<string, IntPtr>(wasm.ImportDirectory.ModuleImports.Count);
                //Exports = new Dictionary<string, IntPtr>(wasm.ExportDirectory.Exports.Count);

                //foreach (ImageModuleImport imp in wasm.ImportDirectory.ModuleImports)
                //    Imports.Add(imp.Name, new IntPtr(imp.ImportAddressTableRva));
                //foreach (ImageSymbolExport exp in wasm.ExportDirectory.Exports)
                //    Exports.Add(exp.Name, new IntPtr(exp.Rva));
            }
            else
            {
                log.Log(LogType.Failure, "Failed to read the PE header: {0}",
                    Marshal.GetLastWin32Error().ToString("X"));
            }
        }

        public IEnumerable<string> Imports => getImports();
        public Dictionary<string, IntPtr> Exports => getExports();

        private IEnumerable<string> getImports()
        {
            var m = new List<string>();
            if (pef.ImportedFunctions != null)
                foreach (var imp in pef.ImportedFunctions)
                    m.Add(imp.Name);

            return m;
        }

        private Dictionary<string, IntPtr> getExports()
        {
            var m = new Dictionary<string, IntPtr>();
            if (pef.ExportedFunctions != null)
                foreach (var exp in pef.ExportedFunctions)
                    m.Add(exp.Name, new IntPtr(exp.Address));

            return m;
        }
    }
}