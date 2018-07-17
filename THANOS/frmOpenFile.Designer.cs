namespace THANOS
{
    partial class frmOpenFile
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmOpenFile));
            this.tvDrives = new System.Windows.Forms.TreeView();
            this.metroLabel1 = new MetroFramework.Controls.MetroLabel();
            this.imgDrives = new System.Windows.Forms.ImageList(this.components);
            this.tvFolder = new System.Windows.Forms.TreeView();
            this.tvFiles = new System.Windows.Forms.TreeView();
            this.metroLabel2 = new MetroFramework.Controls.MetroLabel();
            this.metroLabel3 = new MetroFramework.Controls.MetroLabel();
            this.imgFolder = new System.Windows.Forms.ImageList(this.components);
            this.btnNewDir = new MetroFramework.Controls.MetroButton();
            this.btnSelDir = new MetroFramework.Controls.MetroButton();
            this.btnSelFile = new MetroFramework.Controls.MetroButton();
            this.btnDrive = new MetroFramework.Controls.MetroButton();
            this.SuspendLayout();
            // 
            // tvDrives
            // 
            this.tvDrives.Location = new System.Drawing.Point(23, 82);
            this.tvDrives.Name = "tvDrives";
            this.tvDrives.Size = new System.Drawing.Size(86, 280);
            this.tvDrives.StateImageList = this.imgDrives;
            this.tvDrives.TabIndex = 0;
            // 
            // metroLabel1
            // 
            this.metroLabel1.AutoSize = true;
            this.metroLabel1.Location = new System.Drawing.Point(23, 60);
            this.metroLabel1.Name = "metroLabel1";
            this.metroLabel1.Size = new System.Drawing.Size(42, 19);
            this.metroLabel1.TabIndex = 1;
            this.metroLabel1.Text = "Drive:";
            // 
            // imgDrives
            // 
            this.imgDrives.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imgDrives.ImageStream")));
            this.imgDrives.TransparentColor = System.Drawing.Color.Transparent;
            this.imgDrives.Images.SetKeyName(0, "hdd_normal.png");
            this.imgDrives.Images.SetKeyName(1, "hdd_network.png");
            this.imgDrives.Images.SetKeyName(2, "hdd_ram.png");
            this.imgDrives.Images.SetKeyName(3, "hdd_usb.png");
            this.imgDrives.Images.SetKeyName(4, "hdd_cdrom.png");
            // 
            // tvFolder
            // 
            this.tvFolder.Location = new System.Drawing.Point(115, 82);
            this.tvFolder.Name = "tvFolder";
            this.tvFolder.Size = new System.Drawing.Size(223, 280);
            this.tvFolder.StateImageList = this.imgDrives;
            this.tvFolder.TabIndex = 2;
            // 
            // tvFiles
            // 
            this.tvFiles.Location = new System.Drawing.Point(344, 82);
            this.tvFiles.Name = "tvFiles";
            this.tvFiles.Size = new System.Drawing.Size(270, 280);
            this.tvFiles.StateImageList = this.imgDrives;
            this.tvFiles.TabIndex = 3;
            // 
            // metroLabel2
            // 
            this.metroLabel2.AutoSize = true;
            this.metroLabel2.Location = new System.Drawing.Point(115, 60);
            this.metroLabel2.Name = "metroLabel2";
            this.metroLabel2.Size = new System.Drawing.Size(51, 19);
            this.metroLabel2.TabIndex = 4;
            this.metroLabel2.Text = "Folder:";
            // 
            // metroLabel3
            // 
            this.metroLabel3.AutoSize = true;
            this.metroLabel3.Location = new System.Drawing.Point(344, 60);
            this.metroLabel3.Name = "metroLabel3";
            this.metroLabel3.Size = new System.Drawing.Size(32, 19);
            this.metroLabel3.TabIndex = 5;
            this.metroLabel3.Text = "File:";
            // 
            // imgFolder
            // 
            this.imgFolder.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imgFolder.ImageStream")));
            this.imgFolder.TransparentColor = System.Drawing.Color.Transparent;
            this.imgFolder.Images.SetKeyName(0, "hdd_folder.png");
            // 
            // btnNewDir
            // 
            this.btnNewDir.Location = new System.Drawing.Point(115, 369);
            this.btnNewDir.Name = "btnNewDir";
            this.btnNewDir.Size = new System.Drawing.Size(85, 30);
            this.btnNewDir.TabIndex = 7;
            this.btnNewDir.Text = "New Folder";
            // 
            // btnSelDir
            // 
            this.btnSelDir.Location = new System.Drawing.Point(253, 369);
            this.btnSelDir.Name = "btnSelDir";
            this.btnSelDir.Size = new System.Drawing.Size(85, 30);
            this.btnSelDir.TabIndex = 8;
            this.btnSelDir.Text = "Select Folder";
            // 
            // btnSelFile
            // 
            this.btnSelFile.Location = new System.Drawing.Point(529, 369);
            this.btnSelFile.Name = "btnSelFile";
            this.btnSelFile.Size = new System.Drawing.Size(85, 30);
            this.btnSelFile.TabIndex = 9;
            this.btnSelFile.Text = "Select File(s)";
            this.btnSelFile.Click += new System.EventHandler(this.btnSelFile_Click);
            // 
            // btnDrive
            // 
            this.btnDrive.Location = new System.Drawing.Point(23, 369);
            this.btnDrive.Name = "btnDrive";
            this.btnDrive.Size = new System.Drawing.Size(85, 30);
            this.btnDrive.TabIndex = 10;
            this.btnDrive.Text = "Select Drive";
            // 
            // frmOpenFile
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(641, 416);
            this.Controls.Add(this.btnDrive);
            this.Controls.Add(this.btnSelFile);
            this.Controls.Add(this.btnSelDir);
            this.Controls.Add(this.btnNewDir);
            this.Controls.Add(this.metroLabel3);
            this.Controls.Add(this.metroLabel2);
            this.Controls.Add(this.tvFiles);
            this.Controls.Add(this.tvFolder);
            this.Controls.Add(this.metroLabel1);
            this.Controls.Add(this.tvDrives);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmOpenFile";
            this.Resizable = false;
            this.Text = "Open X";
            this.Load += new System.EventHandler(this.frmOpenFile_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView tvDrives;
        private MetroFramework.Controls.MetroLabel metroLabel1;
        private System.Windows.Forms.ImageList imgDrives;
        private System.Windows.Forms.TreeView tvFolder;
        private System.Windows.Forms.TreeView tvFiles;
        private MetroFramework.Controls.MetroLabel metroLabel2;
        private MetroFramework.Controls.MetroLabel metroLabel3;
        private System.Windows.Forms.ImageList imgFolder;
        private MetroFramework.Controls.MetroButton btnNewDir;
        private MetroFramework.Controls.MetroButton btnSelDir;
        private MetroFramework.Controls.MetroButton btnSelFile;
        private MetroFramework.Controls.MetroButton btnDrive;
    }
}