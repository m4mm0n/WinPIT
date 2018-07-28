using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Dynamic;
using System.Management;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Engine;
using Engine.Extensions.ListView;
using Engine.ProcessCore;

namespace THANOS
{
    public partial class frmProcesses : Form
    {
        private readonly BackgroundWorker bw = new BackgroundWorker();
        private readonly Logger log;
        private ColumnSorter lvwColumnSorter;

        public frmProcesses(Logger Log = null)
        {
            log = Log ?? new Logger(LoggerType.Console_File, "THANOS.frmProcesses");
            InitializeComponent();
        }

        public Core Selected { get; set; }

        private void frmProcesses_Load(object sender, EventArgs e)
        {
            log.Log("[+] Initializing Processes List (x" + (Environment.Is64BitProcess ? "64" : "32") + ")");
            Text = Environment.Is64BitProcess ? "[WinPIT x64] Processes" : "[WinPIT x32] Processes";
            bw.DoWork += Bw_DoWork;
            bw.RunWorkerCompleted += Bw_RunWorkerCompleted;
            lvwColumnSorter = new ColumnSorter();

            lstProcs.ListViewItemSorter = lvwColumnSorter;
            lstProcs.ColumnClick += LstProcs_ColumnClick;
            lstProcs.MouseDoubleClick += LstProcs_MouseDoubleClick;

            bw.RunWorkerAsync();
        }

        private void LstProcs_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (lstProcs.SelectedIndices.Count == 1)
            {
                //Selected = new Core(Process.GetProcessById(int.Parse(lstProcs.SelectedItems[0].SubItems[1].Text)));
                Program.TargetProcess =
                    new Core(Process.GetProcessById(int.Parse(lstProcs.SelectedItems[0].SubItems[1].Text)));
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void LstProcs_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            // Determine if clicked column is already the column that is being sorted.
            if (e.Column == lvwColumnSorter.SortColumn)
            {
                // Reverse the current sort direction for this column.
                if (lvwColumnSorter.Order == SortOrder.Ascending)
                    lvwColumnSorter.Order = SortOrder.Descending;
                else
                    lvwColumnSorter.Order = SortOrder.Ascending;
            }
            else
            {
                // Set the column number that is to be sorted; default to ascending.
                lvwColumnSorter.SortColumn = e.Column;
                lvwColumnSorter.Order = SortOrder.Ascending;
            }

            // Perform the sort with these new sort options.
            lstProcs.Sort();
        }

        private void Bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            lstProcs.Enabled = true;
        }

        private void Bw_DoWork(object sender, DoWorkEventArgs e)
        {
            renderProcessesOnListView();
        }

        private object threadLock = new object();

        /// <summary>
        ///     This method renders all the processes of Windows on a ListView with some values and icons.
        /// </summary>
        public void renderProcessesOnListView()
        {
            if (InvokeRequired)
            {
                Invoke(new renderDelegate(renderProcessesOnListView));
            }
            else
            {
                lstProcs.Enabled = false;

                // Create an array to store the processes
                var processList = Process.GetProcesses();

                // Create an Imagelist that will store the icons of every process
                var Imagelist = new ImageList();

                // Loop through the array of processes to show information of every process in your console
                foreach (var process in processList)
                    lock (threadLock)
                    {
                        try
                        {
                            //log.Log("Trying to read from Process ID: {0}", process.Id.ToString("X"));
                            // Define the status from a boolean to a simple string
                            var status = process.Responding ? "Responding" : "Not responding";

                            // Retrieve the object of extra information of the process (to retrieve Username and Description)
                            //dynamic extraProcessInfo = GetProcessExtraInformation(process.Id);

                            // Create an array of string that will store the information to display in our 
                            string[] row =
                            {
                                // 1 Process name
                                process.ProcessName,
                                // 2 Process ID
                                process.Id.ToString(),
                                // 3 Process status
                                status,
                                // 4 Username that started the process
                                "?", //extraProcessInfo.Username,
                                // 5 Memory usage
                                BytesToReadableValue(process.PrivateMemorySize64),
                                // 6 Description of the process
                                "?" //extraProcessInfo.Description
                            };

                            //
                            // As not every process has an icon then, prevent the app from crash
                            try
                            {
                                //log.Log("Attempting to get icon from process...");

                                Imagelist.Images.Add(
                                    // Add an unique Key as identifier for the icon (same as the ID of the process)
                                    process.Id.ToString(),
                                    // Add Icon to the List 
                                    Icon.ExtractAssociatedIcon(process.MainModule.FileName).ToBitmap()
                                );
                            }
                            catch (Exception exy)
                            {
                                log.Log(exy, "Failed to retrieve process icon: {0}",
                                    Marshal.GetLastWin32Error().ToString("X"));
                            }

                            // Create a new Item to add into the list view that expects the row of information as first argument
                            var item = new ListViewItem(row)
                            {
                                // Set the ImageIndex of the item as the same defined in the previous try-catch
                                ImageIndex = Imagelist.Images.IndexOfKey(process.Id.ToString())
                            };

                            // Add the Item
                            lstProcs.Items.Add(item);
                        }
                        catch (Exception ex)
                        {
                            log.Log(ex, "Failed to read process: {0}", Marshal.GetLastWin32Error().ToString("X"));
                        }
                    }

                // Set the imagelist of your list view the previous created list :)
                lstProcs.LargeImageList = Imagelist;
                lstProcs.SmallImageList = Imagelist;
            }
        }

        /// <summary>
        ///     Method that converts bytes to its human readable value
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public string BytesToReadableValue(long number)
        {
            var suffixes = new List<string> {" B", " KB", " MB", " GB", " TB", " PB"};

            for (var i = 0; i < suffixes.Count; i++)
            {
                var temp = number / (int) Math.Pow(1024, i + 1);

                if (temp == 0) return number / (int) Math.Pow(1024, i) + suffixes[i];
            }

            return number.ToString();
        }

        /// <summary>
        ///     Returns an Expando object with the description and username of a process from the process ID.
        /// </summary>
        /// <param name="processId"></param>
        /// <returns></returns>
        //public ExpandoObject GetProcessExtraInformation(int processId)
        //{
        //    // Query the Win32_Process
        //    var query = "Select * From Win32_Process Where ProcessID = " + processId;
        //    var searcher = new ManagementObjectSearcher(query);
        //    var processList = searcher.Get();

        //    // Create a dynamic object to store some properties on it
        //    dynamic response = new ExpandoObject();
        //    response.Description = "";
        //    response.Username = "Unknown";

        //    foreach (ManagementObject obj in processList)
        //    {
        //        // Retrieve username 
        //        var argList = {string.Empty, string.Empty};
        //        var returnVal = Convert.ToInt32(obj.InvokeMethod("GetOwner", argList));
        //        if (returnVal == 0) response.Username = argList[0];

        //        // Retrieve process description if exists
        //        if (obj["ExecutablePath"] != null)
        //            try
        //            {
        //                var info = FileVersionInfo.GetVersionInfo(obj["ExecutablePath"].ToString());
        //                response.Description = info.FileDescription;
        //            }
        //            catch
        //            {
        //            }
        //    }

        //    return response;
        //}

        public class ProcessItemClass
        {
            public string ProcessName { get; set; }
            public string ProcessId { get; set; }
            public string ProcessStatus { get; set; }
            public string ProcessOwner { get; set; }
            public string ProcessMemory { get; set; }
            public string ProcessDescription { get; set; }
        }

        private delegate void renderDelegate();
    }
}