using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Engine;
using Engine.Assembly;
using Engine.Injectors;
using Engine.ProcessCore;
using Engine.UWP;
using MetroFramework;
using MetroFramework.Forms;
using PeNet;

namespace THANOS
{
    public partial class frmInjection : MetroFramework.Forms.MetroForm
    {
        private Core proc;
        private Logger log;
        private string FileToInject;

        public frmInjection(Core core)
        {

            InitializeComponent();

            log = new Logger(LoggerType.Console_File, "THANOS.frmInjection", false);

            proc = core;
        }

        private void frmInjection_Load(object sender, EventArgs e)
        {
            log.Log(LogType.Normal, "[+] Loading information about targeted process...");

            this.Text = string.Format("[WinPIT x{0}] Injector", (Environment.Is64BitProcess ? "64" : "32"));

            try
            {

                var th = new Thread(new ThreadStart(LoadInjections));
                th.Start();

                string mods = "";
                string architecture = "";
                string basicInfo = "";

                try
                {
                    foreach (ProcessModule pm in proc.LoadedModules)
                    {
                        mods += string.Format("{0} [EP: 0x{1}]{2}", Path.GetFileName(pm.FileName),
                            (proc.Is64bit
                                ? pm.EntryPointAddress.ToInt64().ToString("X16")
                                : pm.EntryPointAddress.ToInt32().ToString("X8")), Environment.NewLine);
                    }
                }
                catch { }

                architecture = "Architecture: x" + (proc.Is64bit ? "64" : "32") + Environment.NewLine;
                basicInfo = string.Format(
                    "Status: {0}{1}Memory Usage: {2}{1}Name: {3}{1}Description: {4}{1}Is UWP: {5}{1}Process Priority: {6}{1}", proc.ProcessStatus,
                    Environment.NewLine, proc.ProcessMemoryUsage, proc.ProcessName, proc.ProcessTitle,
                    Helper.IsProcessUWP(proc.ProcessId) ? "Yes" : "No", proc.ProcessPriority);

                txtAnalysis.Text = basicInfo + Environment.NewLine + architecture + Environment.NewLine + mods;

                //metroTabControl1.SelectedIndex = 0;
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

        delegate void LoadInjectionsDelegate();

        private List<IInjector> Injectors;

        void LoadInjections()
        {
            if (this.InvokeRequired)
                this.Invoke(new LoadInjectionsDelegate(LoadInjections));
            else
            {
                Injectors = InjectionsLoader.GetInjectors();
                foreach (var injector in Injectors)
                {
                    cbMethods.Items.Add(injector.SelfFileName);
                }
            }
        }

        private void cbMethods_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbMethods.SelectedIndex > -1)
            {
                var x = Injectors.First(t => t.SelfFileName == cbMethods.Items[cbMethods.SelectedIndex].ToString());
                txtInjInfo.Text = string.Format("About: {0}{0}{1}{0}{0} Unique ID: {0}{2}{0}", Environment.NewLine, x.About,
                    x.UniqueId);
            }
            else
            {
                txtInjInfo.Text = "";
            }
        }

        private void metroButton1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog()
            {
                Filter = "Dynamic Link Library (*.dll)|*.dll|System Driver Library (*.sys/*.drv)|*.sys;*.drv"
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                if (File.Exists(ofd.FileName))
                {
                    PEReader per = new PEReader(ofd.FileName);
                    txtDllToInj.Text = "File: " + Path.GetFileName(ofd.FileName) + Environment.NewLine +
                                      Environment.NewLine + "EXPORTS:" + Environment.NewLine + Environment.NewLine;

                    if(per.GetExports != null)
                    {
                        foreach (var exp in per.GetExports)
                        {
                            txtDllToInj.Text += string.Format("Name: {0}{1}Address: 0x{2}{1}", exp.Name, Environment.NewLine,
                                exp.Address);
                        }

                        FileToInject = ofd.FileName;
                    }
                    else
                    {

                        if (MessageBox.Show("No exports could be located within the selected PE file to inject\r\n\r\nAre you really sure you wish to continue loading this PE?", "Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            FileToInject = ofd.FileName;                         
                        }
                    }
                }
            }
        }
    }
}
