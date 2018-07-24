using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Engine;
using Engine.Assembly;
using Engine.Injectors;
using Engine.ProcessCore;
using Engine.UWP;
using MetroFramework.Forms;

namespace THANOS
{
    public partial class frmInjection : MetroForm
    {
        private IInjector curInjector;
        private string FileToInject;
        private bool hasExports;

        private IntPtr injected;

        private BackgroundWorker injectedWorker;

        private List<IInjector> Injectors;
        private readonly Logger log;
        private Module modToInject;
        private readonly Core proc;

        public frmInjection(Core core)
        {
            InitializeComponent();

            log = new Logger(LoggerType.Console_File, "THANOS.frmInjection", false);

            proc = core;
        }

        private void frmInjection_Load(object sender, EventArgs e)
        {
            log.Log(LogType.Normal, "[+] Loading information about targeted process...");

            Text = string.Format("[WinPIT x{0}] Injector", Environment.Is64BitProcess ? "64" : "32");

            try
            {
                var th = new Thread(LoadInjections);
                th.Start();

                var mods = "";
                var architecture = "";
                var basicInfo = "";

                try
                {
                    foreach (ProcessModule pm in proc.LoadedModules)
                        mods += string.Format("{0} [EP: 0x{1}]{2}", Path.GetFileName(pm.FileName),
                            proc.Is64bit
                                ? pm.EntryPointAddress.ToInt64().ToString("X16")
                                : pm.EntryPointAddress.ToInt32().ToString("X8"), Environment.NewLine);
                }
                catch
                {
                }

                architecture = "Architecture: x" + (proc.Is64bit ? "64" : "32") + Environment.NewLine;
                basicInfo = string.Format(
                    "Status: {0}{1}Memory Usage: {2}{1}Name: {3}{1}Description: {4}{1}Is UWP: {5}{1}Process Priority: {6}{1}",
                    proc.ProcessStatus,
                    Environment.NewLine, proc.ProcessMemoryUsage, proc.ProcessName, proc.ProcessTitle,
                    Helper.IsProcessUWP(proc.ProcessId) ? "Yes" : "No", proc.ProcessPriority);

                txtAnalysis.Text = basicInfo + Environment.NewLine + architecture + Environment.NewLine + mods;
            }
            catch (Exception ex)
            {
                log.Log(ex, "An error occured loading the information about the process: {0}",
                    Marshal.GetLastWin32Error().ToString("X"));
            }

            metroTabControl1.SelectTab(metroTabPage1);
        }

        private void LoadInjections()
        {
            if (InvokeRequired)
            {
                Invoke(new LoadInjectionsDelegate(LoadInjections));
            }
            else
            {
                Injectors = InjectionsLoader.GetInjectors();
                foreach (var injector in Injectors) cbMethods.Items.Add(injector.SelfFileName);
            }
        }

        private void cbMethods_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbMethods.SelectedIndex > -1)
            {
                var x = Injectors.First(t => t.SelfFileName == cbMethods.Items[cbMethods.SelectedIndex].ToString());
                txtInjInfo.Text = string.Format("About: {0}{0}{1}{0}{0} Unique ID: {0}{2}{0}", Environment.NewLine,
                    x.About,
                    x.UniqueId);
                curInjector =
                    Injectors.First(a => a.SelfFileName == cbMethods.Items[cbMethods.SelectedIndex].ToString());
            }
            else
            {
                txtInjInfo.Text = "";
            }
        }

        private void metroButton1_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                Filter = "Dynamic Link Library (*.dll)|*.dll|System Driver Library (*.sys/*.drv)|*.sys;*.drv"
            };
            if (ofd.ShowDialog() == DialogResult.OK)
                if (File.Exists(ofd.FileName))
                {
                    var per = new PEReader(ofd.FileName);
                    txtDllToInj.Text = "File: " + Path.GetFileName(ofd.FileName);

                    if (per.GetExports != null)
                    {
                        txtDllToInj.Text += Environment.NewLine +
                                            Environment.NewLine + "EXPORTS:" + Environment.NewLine +
                                            Environment.NewLine;
                        foreach (var exp in per.GetExports)
                            txtDllToInj.Text += string.Format("Name: {0}{1}Address: 0x{2}{1}", exp.Name,
                                Environment.NewLine,
                                exp.Address);

                        FileToInject = ofd.FileName;
                        try
                        {
                            modToInject = new Module(FileToInject);
                            btnInject.Enabled = true;

                            try
                            {
                                foreach (var pex in per.GetExports)
                                {
                                    var tmp = modToInject.GetExportAddress(pex.Name);
                                    cbExports.Items.Add(pex.Name);
                                }

                                hasExports = true;
                            }
                            catch (Exception emm)
                            {
                                log.Log(emm, "Failed to load export from Module...");
                            }
                        }
                        catch (Exception ex)
                        {
                            log.Log(ex, "Failed to initiate Module...");
                        }
                    }
                    else
                    {
                        if (MessageBox.Show(
                                "No exports could be located within the selected PE file to inject\r\n\r\nAre you really sure you wish to continue loading this PE?",
                                "Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            FileToInject = ofd.FileName;
                            try
                            {
                                modToInject = new Module(FileToInject);
                                btnInject.Enabled = true;
                                hasExports = false;
                            }
                            catch (Exception ex)
                            {
                                log.Log(ex, "Failed to initiate Module...");
                            }
                        }
                    }
                }
        }

        private void btnInject_Click(object sender, EventArgs e)
        {
            injectedWorker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };

            injectedWorker.DoWork += InjectedWorker_DoWork;
            injectedWorker.ProgressChanged += InjectedWorker_ProgressChanged;
            injectedWorker.RunWorkerCompleted += InjectedWorker_RunWorkerCompleted;

            injectedWorker.RunWorkerAsync();
        }

        private void InjectedWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new InjectedWorker_RunWorkerCompleted_Delegate(InjectedWorker_RunWorkerCompleted), sender,
                    e);
            }
            else
            {
                txtInjLog.Text += "Module is no longer loaded..." + Environment.NewLine;
                log.Log(LogType.Warning, "Module {0} is no longer loaded inside of {1}", modToInject.DllName,
                    proc.ProcessName);

                injected = IntPtr.Zero;
            }
        }

        private void InjectedWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new InjectedWorked_ProgressChanged_Delegate(InjectedWorker_ProgressChanged), sender, e);
            }
            else
            {
                var isAlive = false;
                var pr = Process.GetProcessById(proc.ProcessId);
                foreach (ProcessModule pm in pr.Modules)
                    if (pm.FileName == Path.GetFileName(FileToInject))
                    {
                        isAlive = true;
                        break;
                    }

                while (isAlive)
                {
                    var tmpAlive = false;
                    if (bSpin.Value < 100)
                    {
                        bSpin.Value++;
                    }
                    else
                    {
                        bSpin.Backwards = !bSpin.Backwards;
                        bSpin.Value = 0;
                    }

                    pr = Process.GetProcessById(proc.ProcessId);
                    foreach (ProcessModule pm in pr.Modules)
                        if (pm.FileName == Path.GetFileName(FileToInject))
                        {
                            tmpAlive = true;
                            break;
                        }

                    isAlive = tmpAlive;
                }
            }
        }

        private void InjectedWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new InjectedWorker_DoWork_Delegate(InjectedWorker_DoWork), sender, e);
            }
            else
            {
                injected = curInjector.Inject(proc, FileToInject);
                if (injected != IntPtr.Zero)
                {
                    txtInjLog.Text +=
                        Path.GetFileName(FileToInject) + " succesfully injected into process: 0x" + (proc.Is64bit
                            ? injected.ToInt64().ToString("X16")
                            : injected.ToInt32().ToString("X8")) + Environment.NewLine;
                    log.Log(LogType.Success, "{0} succesfully injected into {1} (0x{2})",
                        Path.GetFileName(FileToInject),
                        proc.FileName,
                        proc.Is64bit ? injected.ToInt64().ToString("X16") : injected.ToInt32().ToString("X8"));


                    btnExport.Enabled = hasExports;
                    btnInject.Enabled = false;
                }
                else
                {
                    txtInjLog.Text += "Failed to inject module into process (See log for details!)" +
                                      Environment.NewLine;
                    log.Log(LogType.Error, "Injection failed: {0}", Marshal.GetLastWin32Error().ToString("X"));
                }
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
        }

        private delegate void LoadInjectionsDelegate();

        private delegate void InjectedWorker_RunWorkerCompleted_Delegate(object a, RunWorkerCompletedEventArgs b);

        private delegate void InjectedWorked_ProgressChanged_Delegate(object a, ProgressChangedEventArgs b);

        private delegate void InjectedWorker_DoWork_Delegate(object a, DoWorkEventArgs b);
    }
}