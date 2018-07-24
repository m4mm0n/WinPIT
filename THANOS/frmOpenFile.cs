using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Engine;
using Engine.Extensions;
using MetroFramework.Forms;

namespace THANOS
{
    public enum OpenType
    {
        File,
        Folder,
        Drive
    }

    public partial class frmOpenFile : MetroForm
    {
        private readonly Logger log;
        private OpenType openType;

        private readonly SystemIconsImageList siil = new SystemIconsImageList();
        private string startupPath;
        private bool useMultiSelect;

        public frmOpenFile(string startPath = "", OpenType oType = OpenType.File, bool MultiFileSelect = false)
        {
            log = new Logger(LoggerType.Console_File, "THANOS.frmOpenFile");

            openType = oType;
            startupPath = startPath;
            useMultiSelect = MultiFileSelect;

            InitializeComponent();

            switch (oType)
            {
                case OpenType.File:
                    Size = new Size( /*637; 416*/637, 416);
                    Text = "Open File";
                    btnDrive.Enabled = false;
                    btnSelDir.Enabled = false;
                    break;
                case OpenType.Folder:
                    Size = new Size( /*637; 416*/343, 416);
                    Text = "Open Folder";
                    btnDrive.Enabled = false;
                    break;
                case OpenType.Drive:
                    Size = new Size( /*637; 416*/115, 416);
                    Text = "";
                    break;
            }
        }

        private void frmOpenFile_Load(object sender, EventArgs e)
        {
            var hardDrives = DriveInfo.GetDrives();
            tvDrives.ImageList = imgDrives;
            foreach (var drive in hardDrives)
                switch (drive.DriveType)
                {
                    case DriveType.CDRom:
                        tvDrives.Nodes.Add(drive.Name, drive.Name, 4);
                        break;
                    case DriveType.Fixed:
                        tvDrives.Nodes.Add(drive.Name, drive.Name, 0);
                        break;
                    case DriveType.Ram:
                        tvDrives.Nodes.Add(drive.Name, drive.Name, 2);
                        break;
                    case DriveType.Removable:
                        tvDrives.Nodes.Add(drive.Name, drive.Name, 3);
                        break;
                    case DriveType.Network:
                        tvDrives.Nodes.Add(drive.Name, drive.Name, 1);
                        break;
                }

            tvFolder.ImageList = imgFolder;
            tvFiles.ImageList = siil.SmallIconsImageList;

            tvDrives.AfterSelect += TvDrives_AfterSelect;
            tvFolder.BeforeExpand += TvFolder_BeforeExpand;
            tvFolder.AfterSelect += TvFolder_AfterSelect;

            //if (startupPath != string.Empty)
            //{

            //    //DirectoryInfo di = new DirectoryInfo(d);
            //}
        }

        private void TvFolder_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node != null)
            {
                tvFiles.Nodes.Clear();
                if (Directory.Exists(e.Node.Tag.ToString()))
                    try
                    {
                        var files = Directory.GetFiles(e.Node.Tag.ToString());
                        foreach (var fil in files)
                            tvFiles.Nodes.Add(fil, Path.GetFileName(fil), siil.GetIconIndex(fil));
                    }
                    catch (AccessViolationException ave)
                    {
                        log.Log(ave, "Access denied for {0}", e.Node.Tag.ToString());
                    }
                    catch (UnauthorizedAccessException uae)
                    {
                        log.Log(uae, "Unauthorized to access {0}", e.Node.Tag.ToString());
                    }
            }
        }

        private void TvFolder_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            var tn = e.Node.Nodes[0];
            if (tn.Text == "...")
            {
                e.Node.Nodes.AddRange(getFolderNodes(((DirectoryInfo) e.Node.Tag)
                    .FullName, false).ToArray());
                if (tn.Text == "...") tn.Parent.Nodes.Remove(tn);
            }
        }

        private void TvDrives_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node != null)
            {
                tvFolder.Nodes.Clear();
                //var allDirs = Directory.GetDirectories(e.Node.Text);
                //foreach (var dir in allDirs)
                //{
                //    tvFolder.Nodes.Add(dir, dir, 0);
                //}
                tvFolder.Nodes.AddRange(getFolderNodes(e.Node.Text, false).ToArray());
                //if (startupPath != "")
                //{
                //    try
                //    {
                //        var drive = Path.GetPathRoot(startupPath);
                //        tvDrives.SelectedNode = tvDrives.Nodes.Find(drive, false)[0];
                //        tvFolder.SelectedNode = tvFolder.Nodes.Find(new DirectoryInfo(startupPath).FullName, true)[0];
                //    }
                //    catch (Exception exception)
                //    {
                //        Console.WriteLine(exception);
                //    }

                //}
                tvDrives.SelectedNode = e.Node;
            }
        }

        private List<TreeNode> getFolderNodes(string dir, bool expanded)
        {
            var dirs = Directory.GetDirectories(dir).ToArray();
            var nodes = new List<TreeNode>();
            foreach (var d in dirs)
            {
                var di = new DirectoryInfo(d);
                var tn = new TreeNode(di.Name, 0, 0);
                tn.Tag = di;
                var subCount = 0;
                try
                {
                    subCount = Directory.GetDirectories(d).Count();
                }
                catch
                {
                    /* ignore accessdenied */
                }

                if (subCount > 0) tn.Nodes.Add("...");
                if (expanded) tn.Expand(); //  **
                nodes.Add(tn);
            }

            return nodes;
        }

        private void btnSelFile_Click(object sender, EventArgs e)
        {
            if (tvFiles.SelectedNode != null)
            {
            }
        }
    }
}