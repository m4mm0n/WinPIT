namespace THANOS
{
    partial class frmInjection
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
            this.metroTabControl1 = new MetroFramework.Controls.MetroTabControl();
            this.metroTabPage1 = new MetroFramework.Controls.MetroTabPage();
            this.txtAnalysis = new MetroFramework.Controls.MetroTextBox();
            this.metroTabPage2 = new MetroFramework.Controls.MetroTabPage();
            this.txtInjInfo = new MetroFramework.Controls.MetroTextBox();
            this.cbMethods = new MetroFramework.Controls.MetroComboBox();
            this.metroTabPage3 = new MetroFramework.Controls.MetroTabPage();
            this.txtDllToInj = new MetroFramework.Controls.MetroTextBox();
            this.metroButton1 = new MetroFramework.Controls.MetroButton();
            this.metroTabPage4 = new MetroFramework.Controls.MetroTabPage();
            this.metroTabControl2 = new MetroFramework.Controls.MetroTabControl();
            this.metroTabPage5 = new MetroFramework.Controls.MetroTabPage();
            this.btnExport = new MetroFramework.Controls.MetroButton();
            this.cbExports = new MetroFramework.Controls.MetroComboBox();
            this.btnInject = new MetroFramework.Controls.MetroButton();
            this.metroTabPage6 = new MetroFramework.Controls.MetroTabPage();
            this.txtInjLog = new MetroFramework.Controls.MetroTextBox();
            this.bSpin = new MetroFramework.Controls.MetroProgressSpinner();
            this.metroTabControl1.SuspendLayout();
            this.metroTabPage1.SuspendLayout();
            this.metroTabPage2.SuspendLayout();
            this.metroTabPage3.SuspendLayout();
            this.metroTabPage4.SuspendLayout();
            this.metroTabControl2.SuspendLayout();
            this.metroTabPage5.SuspendLayout();
            this.metroTabPage6.SuspendLayout();
            this.SuspendLayout();
            // 
            // metroTabControl1
            // 
            this.metroTabControl1.Controls.Add(this.metroTabPage1);
            this.metroTabControl1.Controls.Add(this.metroTabPage4);
            this.metroTabControl1.Controls.Add(this.metroTabPage2);
            this.metroTabControl1.Controls.Add(this.metroTabPage3);
            this.metroTabControl1.Location = new System.Drawing.Point(23, 63);
            this.metroTabControl1.Name = "metroTabControl1";
            this.metroTabControl1.SelectedIndex = 0;
            this.metroTabControl1.Size = new System.Drawing.Size(508, 185);
            this.metroTabControl1.Style = MetroFramework.MetroColorStyle.Lime;
            this.metroTabControl1.TabIndex = 0;
            this.metroTabControl1.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.metroTabControl1.UseStyleColors = true;
            // 
            // metroTabPage1
            // 
            this.metroTabPage1.Controls.Add(this.txtAnalysis);
            this.metroTabPage1.HorizontalScrollbarBarColor = true;
            this.metroTabPage1.Location = new System.Drawing.Point(4, 35);
            this.metroTabPage1.Name = "metroTabPage1";
            this.metroTabPage1.Size = new System.Drawing.Size(500, 146);
            this.metroTabPage1.TabIndex = 0;
            this.metroTabPage1.Text = "Analysis";
            this.metroTabPage1.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.metroTabPage1.VerticalScrollbarBarColor = true;
            // 
            // txtAnalysis
            // 
            this.txtAnalysis.Location = new System.Drawing.Point(3, 3);
            this.txtAnalysis.Multiline = true;
            this.txtAnalysis.Name = "txtAnalysis";
            this.txtAnalysis.ReadOnly = true;
            this.txtAnalysis.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtAnalysis.Size = new System.Drawing.Size(494, 140);
            this.txtAnalysis.Style = MetroFramework.MetroColorStyle.Silver;
            this.txtAnalysis.TabIndex = 2;
            this.txtAnalysis.Text = "metroTextBox1";
            this.txtAnalysis.Theme = MetroFramework.MetroThemeStyle.Dark;
            // 
            // metroTabPage2
            // 
            this.metroTabPage2.Controls.Add(this.txtInjInfo);
            this.metroTabPage2.Controls.Add(this.cbMethods);
            this.metroTabPage2.HorizontalScrollbarBarColor = true;
            this.metroTabPage2.Location = new System.Drawing.Point(4, 35);
            this.metroTabPage2.Name = "metroTabPage2";
            this.metroTabPage2.Size = new System.Drawing.Size(500, 146);
            this.metroTabPage2.TabIndex = 1;
            this.metroTabPage2.Text = "Method";
            this.metroTabPage2.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.metroTabPage2.VerticalScrollbarBarColor = true;
            // 
            // txtInjInfo
            // 
            this.txtInjInfo.Location = new System.Drawing.Point(3, 38);
            this.txtInjInfo.Multiline = true;
            this.txtInjInfo.Name = "txtInjInfo";
            this.txtInjInfo.ReadOnly = true;
            this.txtInjInfo.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtInjInfo.Size = new System.Drawing.Size(494, 105);
            this.txtInjInfo.Style = MetroFramework.MetroColorStyle.Lime;
            this.txtInjInfo.TabIndex = 3;
            this.txtInjInfo.Theme = MetroFramework.MetroThemeStyle.Dark;
            // 
            // cbMethods
            // 
            this.cbMethods.FormattingEnabled = true;
            this.cbMethods.ItemHeight = 23;
            this.cbMethods.Location = new System.Drawing.Point(3, 3);
            this.cbMethods.Name = "cbMethods";
            this.cbMethods.Size = new System.Drawing.Size(494, 29);
            this.cbMethods.Style = MetroFramework.MetroColorStyle.Magenta;
            this.cbMethods.TabIndex = 2;
            this.cbMethods.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.cbMethods.SelectedIndexChanged += new System.EventHandler(this.cbMethods_SelectedIndexChanged);
            // 
            // metroTabPage3
            // 
            this.metroTabPage3.Controls.Add(this.txtDllToInj);
            this.metroTabPage3.Controls.Add(this.metroButton1);
            this.metroTabPage3.HorizontalScrollbarBarColor = true;
            this.metroTabPage3.Location = new System.Drawing.Point(4, 35);
            this.metroTabPage3.Name = "metroTabPage3";
            this.metroTabPage3.Size = new System.Drawing.Size(500, 146);
            this.metroTabPage3.TabIndex = 2;
            this.metroTabPage3.Text = "File To Inject";
            this.metroTabPage3.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.metroTabPage3.VerticalScrollbarBarColor = true;
            // 
            // txtDllToInj
            // 
            this.txtDllToInj.Location = new System.Drawing.Point(3, 32);
            this.txtDllToInj.Multiline = true;
            this.txtDllToInj.Name = "txtDllToInj";
            this.txtDllToInj.ReadOnly = true;
            this.txtDllToInj.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtDllToInj.Size = new System.Drawing.Size(494, 111);
            this.txtDllToInj.Style = MetroFramework.MetroColorStyle.Lime;
            this.txtDllToInj.TabIndex = 3;
            this.txtDllToInj.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.txtDllToInj.UseStyleColors = true;
            // 
            // metroButton1
            // 
            this.metroButton1.Location = new System.Drawing.Point(3, 3);
            this.metroButton1.Name = "metroButton1";
            this.metroButton1.Size = new System.Drawing.Size(116, 23);
            this.metroButton1.TabIndex = 2;
            this.metroButton1.Text = ".browse.";
            this.metroButton1.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.metroButton1.Click += new System.EventHandler(this.metroButton1_Click);
            // 
            // metroTabPage4
            // 
            this.metroTabPage4.Controls.Add(this.metroTabControl2);
            this.metroTabPage4.HorizontalScrollbarBarColor = true;
            this.metroTabPage4.Location = new System.Drawing.Point(4, 35);
            this.metroTabPage4.Name = "metroTabPage4";
            this.metroTabPage4.Size = new System.Drawing.Size(500, 146);
            this.metroTabPage4.TabIndex = 3;
            this.metroTabPage4.Text = "Injection";
            this.metroTabPage4.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.metroTabPage4.VerticalScrollbarBarColor = true;
            // 
            // metroTabControl2
            // 
            this.metroTabControl2.Controls.Add(this.metroTabPage5);
            this.metroTabControl2.Controls.Add(this.metroTabPage6);
            this.metroTabControl2.Location = new System.Drawing.Point(3, 3);
            this.metroTabControl2.Name = "metroTabControl2";
            this.metroTabControl2.SelectedIndex = 1;
            this.metroTabControl2.Size = new System.Drawing.Size(494, 140);
            this.metroTabControl2.Style = MetroFramework.MetroColorStyle.Yellow;
            this.metroTabControl2.TabIndex = 2;
            this.metroTabControl2.Theme = MetroFramework.MetroThemeStyle.Dark;
            // 
            // metroTabPage5
            // 
            this.metroTabPage5.Controls.Add(this.btnExport);
            this.metroTabPage5.Controls.Add(this.cbExports);
            this.metroTabPage5.Controls.Add(this.btnInject);
            this.metroTabPage5.HorizontalScrollbarBarColor = true;
            this.metroTabPage5.Location = new System.Drawing.Point(4, 35);
            this.metroTabPage5.Name = "metroTabPage5";
            this.metroTabPage5.Size = new System.Drawing.Size(486, 101);
            this.metroTabPage5.Style = MetroFramework.MetroColorStyle.White;
            this.metroTabPage5.TabIndex = 0;
            this.metroTabPage5.Text = "Injector";
            this.metroTabPage5.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.metroTabPage5.UseVisualStyleBackColor = true;
            this.metroTabPage5.VerticalScrollbarBarColor = true;
            // 
            // btnExport
            // 
            this.btnExport.Enabled = false;
            this.btnExport.Location = new System.Drawing.Point(135, 38);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(348, 29);
            this.btnExport.TabIndex = 4;
            this.btnExport.Text = ".call export.";
            this.btnExport.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            // 
            // cbExports
            // 
            this.cbExports.FormattingEnabled = true;
            this.cbExports.ItemHeight = 23;
            this.cbExports.Location = new System.Drawing.Point(135, 3);
            this.cbExports.Name = "cbExports";
            this.cbExports.Size = new System.Drawing.Size(348, 29);
            this.cbExports.TabIndex = 3;
            this.cbExports.Theme = MetroFramework.MetroThemeStyle.Dark;
            // 
            // btnInject
            // 
            this.btnInject.Enabled = false;
            this.btnInject.Location = new System.Drawing.Point(3, 3);
            this.btnInject.Name = "btnInject";
            this.btnInject.Size = new System.Drawing.Size(126, 29);
            this.btnInject.TabIndex = 2;
            this.btnInject.Text = ".inject.";
            this.btnInject.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.btnInject.Click += new System.EventHandler(this.btnInject_Click);
            // 
            // metroTabPage6
            // 
            this.metroTabPage6.Controls.Add(this.bSpin);
            this.metroTabPage6.Controls.Add(this.txtInjLog);
            this.metroTabPage6.HorizontalScrollbarBarColor = true;
            this.metroTabPage6.Location = new System.Drawing.Point(4, 35);
            this.metroTabPage6.Name = "metroTabPage6";
            this.metroTabPage6.Size = new System.Drawing.Size(486, 101);
            this.metroTabPage6.Style = MetroFramework.MetroColorStyle.White;
            this.metroTabPage6.TabIndex = 1;
            this.metroTabPage6.Text = "Status";
            this.metroTabPage6.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.metroTabPage6.UseVisualStyleBackColor = true;
            this.metroTabPage6.VerticalScrollbarBarColor = true;
            // 
            // txtInjLog
            // 
            this.txtInjLog.Location = new System.Drawing.Point(3, 3);
            this.txtInjLog.Multiline = true;
            this.txtInjLog.Name = "txtInjLog";
            this.txtInjLog.ReadOnly = true;
            this.txtInjLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtInjLog.Size = new System.Drawing.Size(480, 95);
            this.txtInjLog.Style = MetroFramework.MetroColorStyle.Yellow;
            this.txtInjLog.TabIndex = 2;
            this.txtInjLog.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.txtInjLog.UseStyleColors = true;
            // 
            // bSpin
            // 
            this.bSpin.Location = new System.Drawing.Point(427, 5);
            this.bSpin.Maximum = 100;
            this.bSpin.Name = "bSpin";
            this.bSpin.Size = new System.Drawing.Size(30, 28);
            this.bSpin.Style = MetroFramework.MetroColorStyle.Orange;
            this.bSpin.TabIndex = 4;
            this.bSpin.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.bSpin.Value = 50;
            // 
            // frmInjection
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(553, 272);
            this.Controls.Add(this.metroTabControl1);
            this.MaximizeBox = false;
            this.Name = "frmInjection";
            this.Style = MetroFramework.MetroColorStyle.Orange;
            this.Text = "[WinPIT xARC] Injector";
            this.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.Load += new System.EventHandler(this.frmInjection_Load);
            this.metroTabControl1.ResumeLayout(false);
            this.metroTabPage1.ResumeLayout(false);
            this.metroTabPage2.ResumeLayout(false);
            this.metroTabPage3.ResumeLayout(false);
            this.metroTabPage4.ResumeLayout(false);
            this.metroTabControl2.ResumeLayout(false);
            this.metroTabPage5.ResumeLayout(false);
            this.metroTabPage6.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private MetroFramework.Controls.MetroTabControl metroTabControl1;
        private MetroFramework.Controls.MetroTabPage metroTabPage1;
        private MetroFramework.Controls.MetroTextBox txtAnalysis;
        private MetroFramework.Controls.MetroTabPage metroTabPage2;
        private MetroFramework.Controls.MetroTabPage metroTabPage3;
        private MetroFramework.Controls.MetroComboBox cbMethods;
        private MetroFramework.Controls.MetroTextBox txtInjInfo;
        private MetroFramework.Controls.MetroButton metroButton1;
        private MetroFramework.Controls.MetroTabPage metroTabPage4;
        private MetroFramework.Controls.MetroTextBox txtDllToInj;
        private MetroFramework.Controls.MetroTabControl metroTabControl2;
        private MetroFramework.Controls.MetroTabPage metroTabPage5;
        private MetroFramework.Controls.MetroTabPage metroTabPage6;
        private MetroFramework.Controls.MetroTextBox txtInjLog;
        private MetroFramework.Controls.MetroComboBox cbExports;
        private MetroFramework.Controls.MetroButton btnInject;
        private MetroFramework.Controls.MetroButton btnExport;
        private MetroFramework.Controls.MetroProgressSpinner bSpin;
    }
}