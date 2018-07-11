using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Engine;
using Engine.Assembly;
using Engine.ProcessCore;
using PeNet;

namespace THANOS
{
    public partial class frmInjection : MetroFramework.Forms.MetroForm
    {
        private Core proc;
        private Logger log;

        public frmInjection(Core core)
        {
            log = new Logger(LoggerType.Console_File, "THANOS.frmInjection");

            proc = core;
            InitializeComponent();
        }

        private void frmInjection_Load(object sender, EventArgs e)
        {
            log.Log(LogType.Normal, "[+] Loading information about targeted process...");

            try
            {
                //var yx = proc.LoadedModules;
                //foreach (ProcessModule pm in yx)
                //{
                //    txtAnalysis.Text += pm.ModuleName + Environment.NewLine;
                //}

                //var xfile = new PeFile(proc.ReadBytes(proc.BaseAddress, proc.SizeOfProcess));
                //if(xfile.ImportedFunctions != null)
                //    foreach (var imp in xfile.ImportedFunctions)
                //    {
                //        txtAnalysis.Text += imp.Name + Environment.NewLine;
                //    }
                //var expImp = new ImportExportReader(proc);

                //var exportz = expImp.Exports;
                //if (exportz.Count > 0)
                //{
                //    txtAnalysis.Text += Environment.NewLine +
                //                        string.Format("FOUND {0} EXPORTS:", exportz.Count) + Environment.NewLine;

                //    foreach (var exp in exportz)
                //    {
                //        txtAnalysis.Text += exp.Key + " => 0x" + (Environment.Is64BitProcess
                //                                ? exp.Value.ToInt64().ToString("X16")
                //                                : exp.Value.ToInt32().ToString("X8")) + Environment.NewLine;
                //    }
                //}

                //var importz = expImp.Imports;

                //if (importz.ToArray().Length > 0)
                //{
                //    txtAnalysis.Text += Environment.NewLine +
                //                        string.Format("FOUND {0} IMPORTS:", importz.ToArray().Length) + Environment.NewLine;
                //    foreach (var imp in importz)
                //    {
                //        txtAnalysis.Text += imp;// + " => 0x" + (Environment.Is64BitProcess
                //                                //? imp.Value.ToInt64().ToString("X16")
                //                                //: imp.Value.ToInt32().ToString("X8")) + Environment.NewLine;
                //    }
                //}
            }
            catch (Exception ex)
            {
                log.Log(ex, "An error occured loading the information about the process: {0}",
                    Marshal.GetLastWin32Error().ToString("X"));
            }
        }
    }
}
