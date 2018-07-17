using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using AsmResolver;

namespace Engine.Assembly
{
    public struct PEExport
    {
        public string Name => name;
        public string Address => is64 ? address.ToString("X16") : address.ToString("X8");

        private bool is64;
        private ulong address;
        private string name;

        public PEExport(string _name, ulong _address, bool _is64)
        {
            address = _address;
            name = _name;
            is64 = _is64;
        }

        public PEExport(string _name, uint _address)
        {
            address = (ulong) _address;
            name = _name;
            is64 = false;
        }
    }

    public struct PEImport
    {

    }

    public class PEReader : IDisposable
    {
        private WindowsAssembly wasm;
        private Logger log = new Logger(LoggerType.Console_File, "Assembly.PEReader");

        public PEReader(string fileName)
        {
            log.Log(LogType.Normal, "PE read using File-load...");

            try
            {
                wasm = WindowsAssembly.FromFile(fileName);
            }
            catch (Exception ex)
            {
                log.Log(ex, "Exception on loading {0}", Path.GetFileName(fileName));
            }
        }

        public PEReader(byte[] fileBytes)
        {
            log.Log(LogType.Normal, "PE read using file-bytes...");

            try
            {
                wasm = WindowsAssembly.FromBytes(fileBytes);
            }
            catch (Exception e)
            {
                log.Log(e, "Exception on loading file");
            }
        }

        public bool Is64 => wasm.NtHeaders.FileHeader.Machine == ImageMachineType.Amd64 |
                            wasm.NtHeaders.FileHeader.Machine == ImageMachineType.Ia64;

        public bool IsDLL => wasm.NtHeaders.FileHeader.Characteristics == ImageCharacteristics.Dll;

        public bool IsDriver => wasm.NtHeaders.FileHeader.Characteristics == ImageCharacteristics.System;

        public PEExport[] GetExports
        {
            get
            {
                if (wasm.ExportDirectory != null)
                {
                    log.Log(LogType.Debug, "PE has exports...");

                    var xp = new List<PEExport>();
                    foreach (var exp in wasm.ExportDirectory.Exports)
                    {
                        xp.Add(new PEExport(exp.Name, (ulong)exp.Rva, Is64));
                    }

                    log.Log(LogType.Debug, "Found {0} exports...", xp.Count.ToString());

                    return xp.ToArray();
                }

                return null;
            }
        }
        

        //public PEReader(Stream fileStream)
        //{
        //    log.Log(LogType.Normal, "PE read using file-stream...");

        //    try
        //    {
        //        wasm = WindowsAssembly.FromReader();
        //    }
        //    catch (Exception e)
        //    {
        //        log.Log(e, "Exception on loading file");
        //    }
        //}
        public void Dispose()
        {
            wasm = null;
            log?.Dispose();
        }
    }
}
