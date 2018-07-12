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
            this.cbMethods = new MetroFramework.Controls.MetroComboBox();
            this.metroTabPage3 = new MetroFramework.Controls.MetroTabPage();
            this.txtInjInfo = new MetroFramework.Controls.MetroTextBox();
            this.metroTabControl1.SuspendLayout();
            this.metroTabPage1.SuspendLayout();
            this.metroTabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // metroTabControl1
            // 
            this.metroTabControl1.Controls.Add(this.metroTabPage1);
            this.metroTabControl1.Controls.Add(this.metroTabPage2);
            this.metroTabControl1.Controls.Add(this.metroTabPage3);
            this.metroTabControl1.Location = new System.Drawing.Point(23, 63);
            this.metroTabControl1.Name = "metroTabControl1";
            this.metroTabControl1.SelectedIndex = 1;
            this.metroTabControl1.Size = new System.Drawing.Size(508, 185);
            this.metroTabControl1.Style = MetroFramework.MetroColorStyle.Lime;
            this.metroTabControl1.TabIndex = 0;
            this.metroTabControl1.Theme = MetroFramework.MetroThemeStyle.Dark;
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
            this.metroTabPage3.HorizontalScrollbarBarColor = true;
            this.metroTabPage3.Location = new System.Drawing.Point(4, 35);
            this.metroTabPage3.Name = "metroTabPage3";
            this.metroTabPage3.Size = new System.Drawing.Size(500, 146);
            this.metroTabPage3.TabIndex = 2;
            this.metroTabPage3.Text = "Externals";
            this.metroTabPage3.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.metroTabPage3.VerticalScrollbarBarColor = true;
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
    }
}