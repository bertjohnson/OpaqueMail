namespace OpaqueMail.Net.ProxySettings
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.Tabs = new System.Windows.Forms.TabControl();
            this.StartupTab = new System.Windows.Forms.TabPage();
            this.OperationalModeGroup = new System.Windows.Forms.GroupBox();
            this.OperationalModeWindowsService = new System.Windows.Forms.RadioButton();
            this.OperationalModeApplication = new System.Windows.Forms.RadioButton();
            this.OperationalModeLabel = new System.Windows.Forms.Label();
            this.SettingsTab = new System.Windows.Forms.TabPage();
            this.ProxyStatusGroup = new System.Windows.Forms.GroupBox();
            this.ProxyStatusLabel = new System.Windows.Forms.Label();
            this.ProxySettingsGroup = new System.Windows.Forms.GroupBox();
            this.ProxySettingsLabel = new System.Windows.Forms.Label();
            this.ProxyInstanceGroup = new System.Windows.Forms.GroupBox();
            this.ProxyInstanceGrid = new System.Windows.Forms.ListView();
            this.LocalIPColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.LocalPortColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.LocalUseSSL = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.DestinationHost = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.DestinationPort = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.DestinationUseSSL = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.CertificatesTab = new System.Windows.Forms.TabPage();
            this.SigningCertificatesGroup = new System.Windows.Forms.GroupBox();
            this.SigningCertificatesLabel = new System.Windows.Forms.Label();
            this.FreeCertificateSourcesGroup = new System.Windows.Forms.GroupBox();
            this.FreeCertificateSourcesLabel = new System.Windows.Forms.LinkLabel();
            this.Tabs.SuspendLayout();
            this.StartupTab.SuspendLayout();
            this.OperationalModeGroup.SuspendLayout();
            this.SettingsTab.SuspendLayout();
            this.ProxyStatusGroup.SuspendLayout();
            this.ProxySettingsGroup.SuspendLayout();
            this.ProxyInstanceGroup.SuspendLayout();
            this.CertificatesTab.SuspendLayout();
            this.SigningCertificatesGroup.SuspendLayout();
            this.FreeCertificateSourcesGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // Tabs
            // 
            this.Tabs.Controls.Add(this.StartupTab);
            this.Tabs.Controls.Add(this.SettingsTab);
            this.Tabs.Controls.Add(this.CertificatesTab);
            this.Tabs.Location = new System.Drawing.Point(3, 3);
            this.Tabs.Name = "Tabs";
            this.Tabs.SelectedIndex = 0;
            this.Tabs.Size = new System.Drawing.Size(578, 358);
            this.Tabs.TabIndex = 0;
            // 
            // StartupTab
            // 
            this.StartupTab.Controls.Add(this.ProxyStatusGroup);
            this.StartupTab.Controls.Add(this.OperationalModeGroup);
            this.StartupTab.Location = new System.Drawing.Point(4, 22);
            this.StartupTab.Name = "StartupTab";
            this.StartupTab.Padding = new System.Windows.Forms.Padding(3);
            this.StartupTab.Size = new System.Drawing.Size(570, 332);
            this.StartupTab.TabIndex = 0;
            this.StartupTab.Text = "Startup";
            this.StartupTab.UseVisualStyleBackColor = true;
            // 
            // OperationalModeGroup
            // 
            this.OperationalModeGroup.Controls.Add(this.OperationalModeWindowsService);
            this.OperationalModeGroup.Controls.Add(this.OperationalModeApplication);
            this.OperationalModeGroup.Controls.Add(this.OperationalModeLabel);
            this.OperationalModeGroup.Location = new System.Drawing.Point(6, 6);
            this.OperationalModeGroup.Name = "OperationalModeGroup";
            this.OperationalModeGroup.Size = new System.Drawing.Size(558, 103);
            this.OperationalModeGroup.TabIndex = 0;
            this.OperationalModeGroup.TabStop = false;
            this.OperationalModeGroup.Text = "Operational Mode";
            // 
            // OperationalModeWindowsService
            // 
            this.OperationalModeWindowsService.AutoSize = true;
            this.OperationalModeWindowsService.Location = new System.Drawing.Point(9, 79);
            this.OperationalModeWindowsService.Name = "OperationalModeWindowsService";
            this.OperationalModeWindowsService.Size = new System.Drawing.Size(152, 17);
            this.OperationalModeWindowsService.TabIndex = 2;
            this.OperationalModeWindowsService.Text = "Run as a Windows service";
            this.OperationalModeWindowsService.UseVisualStyleBackColor = true;
            this.OperationalModeWindowsService.CheckedChanged += new System.EventHandler(this.OperationalModeWindowsService_CheckedChanged);
            // 
            // OperationalModeApplication
            // 
            this.OperationalModeApplication.AutoSize = true;
            this.OperationalModeApplication.Checked = true;
            this.OperationalModeApplication.Location = new System.Drawing.Point(9, 56);
            this.OperationalModeApplication.Name = "OperationalModeApplication";
            this.OperationalModeApplication.Size = new System.Drawing.Size(150, 17);
            this.OperationalModeApplication.TabIndex = 1;
            this.OperationalModeApplication.TabStop = true;
            this.OperationalModeApplication.Text = "Run as an application only";
            this.OperationalModeApplication.UseVisualStyleBackColor = true;
            this.OperationalModeApplication.CheckedChanged += new System.EventHandler(this.OperationalModeApplication_CheckedChanged);
            // 
            // OperationalModeLabel
            // 
            this.OperationalModeLabel.AutoSize = true;
            this.OperationalModeLabel.Location = new System.Drawing.Point(6, 28);
            this.OperationalModeLabel.Name = "OperationalModeLabel";
            this.OperationalModeLabel.Size = new System.Drawing.Size(429, 13);
            this.OperationalModeLabel.TabIndex = 0;
            this.OperationalModeLabel.Text = "OpaqueMail Proxy can run either as a standalone application or using a Windows se" +
    "rvice.";
            // 
            // SettingsTab
            // 
            this.SettingsTab.Controls.Add(this.ProxyInstanceGroup);
            this.SettingsTab.Controls.Add(this.ProxySettingsGroup);
            this.SettingsTab.Location = new System.Drawing.Point(4, 22);
            this.SettingsTab.Name = "SettingsTab";
            this.SettingsTab.Padding = new System.Windows.Forms.Padding(3);
            this.SettingsTab.Size = new System.Drawing.Size(570, 332);
            this.SettingsTab.TabIndex = 1;
            this.SettingsTab.Text = "Proxy Settings";
            this.SettingsTab.UseVisualStyleBackColor = true;
            // 
            // ProxyStatusGroup
            // 
            this.ProxyStatusGroup.Controls.Add(this.ProxyStatusLabel);
            this.ProxyStatusGroup.Location = new System.Drawing.Point(6, 115);
            this.ProxyStatusGroup.Name = "ProxyStatusGroup";
            this.ProxyStatusGroup.Size = new System.Drawing.Size(558, 103);
            this.ProxyStatusGroup.TabIndex = 3;
            this.ProxyStatusGroup.TabStop = false;
            this.ProxyStatusGroup.Text = "Proxy Status";
            // 
            // ProxyStatusLabel
            // 
            this.ProxyStatusLabel.AutoSize = true;
            this.ProxyStatusLabel.Location = new System.Drawing.Point(6, 28);
            this.ProxyStatusLabel.Name = "ProxyStatusLabel";
            this.ProxyStatusLabel.Size = new System.Drawing.Size(29, 13);
            this.ProxyStatusLabel.TabIndex = 0;
            this.ProxyStatusLabel.Text = "TBD";
            // 
            // ProxySettingsGroup
            // 
            this.ProxySettingsGroup.Controls.Add(this.ProxySettingsLabel);
            this.ProxySettingsGroup.Location = new System.Drawing.Point(6, 6);
            this.ProxySettingsGroup.Name = "ProxySettingsGroup";
            this.ProxySettingsGroup.Size = new System.Drawing.Size(558, 103);
            this.ProxySettingsGroup.TabIndex = 4;
            this.ProxySettingsGroup.TabStop = false;
            this.ProxySettingsGroup.Text = "Proxy Settings";
            // 
            // ProxySettingsLabel
            // 
            this.ProxySettingsLabel.AutoSize = true;
            this.ProxySettingsLabel.Location = new System.Drawing.Point(6, 28);
            this.ProxySettingsLabel.Name = "ProxySettingsLabel";
            this.ProxySettingsLabel.Size = new System.Drawing.Size(29, 13);
            this.ProxySettingsLabel.TabIndex = 0;
            this.ProxySettingsLabel.Text = "TBD";
            // 
            // ProxyInstanceGroup
            // 
            this.ProxyInstanceGroup.Controls.Add(this.ProxyInstanceGrid);
            this.ProxyInstanceGroup.Location = new System.Drawing.Point(6, 115);
            this.ProxyInstanceGroup.Name = "ProxyInstanceGroup";
            this.ProxyInstanceGroup.Size = new System.Drawing.Size(558, 209);
            this.ProxyInstanceGroup.TabIndex = 5;
            this.ProxyInstanceGroup.TabStop = false;
            this.ProxyInstanceGroup.Text = "Proxy Instances";
            // 
            // ProxyInstanceGrid
            // 
            this.ProxyInstanceGrid.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.LocalIPColumn,
            this.LocalPortColumn,
            this.LocalUseSSL,
            this.DestinationHost,
            this.DestinationPort,
            this.DestinationUseSSL});
            this.ProxyInstanceGrid.GridLines = true;
            this.ProxyInstanceGrid.Location = new System.Drawing.Point(9, 19);
            this.ProxyInstanceGrid.Name = "ProxyInstanceGrid";
            this.ProxyInstanceGrid.Size = new System.Drawing.Size(543, 184);
            this.ProxyInstanceGrid.TabIndex = 1;
            this.ProxyInstanceGrid.UseCompatibleStateImageBehavior = false;
            this.ProxyInstanceGrid.View = System.Windows.Forms.View.Details;
            // 
            // LocalIPColumn
            // 
            this.LocalIPColumn.Text = "Local IP";
            this.LocalIPColumn.Width = 70;
            // 
            // LocalPortColumn
            // 
            this.LocalPortColumn.Text = "Local Port";
            // 
            // LocalUseSSL
            // 
            this.LocalUseSSL.Text = "Local SSL/TLS?";
            this.LocalUseSSL.Width = 92;
            // 
            // DestinationHost
            // 
            this.DestinationHost.Text = "Destination Host";
            this.DestinationHost.Width = 90;
            // 
            // DestinationPort
            // 
            this.DestinationPort.Text = "Destination Port";
            this.DestinationPort.Width = 89;
            // 
            // DestinationUseSSL
            // 
            this.DestinationUseSSL.Text = "Destination SSL/TLS?";
            this.DestinationUseSSL.Width = 120;
            // 
            // CertificatesTab
            // 
            this.CertificatesTab.Controls.Add(this.FreeCertificateSourcesGroup);
            this.CertificatesTab.Controls.Add(this.SigningCertificatesGroup);
            this.CertificatesTab.Location = new System.Drawing.Point(4, 22);
            this.CertificatesTab.Name = "CertificatesTab";
            this.CertificatesTab.Size = new System.Drawing.Size(570, 332);
            this.CertificatesTab.TabIndex = 3;
            this.CertificatesTab.Text = "Certificates";
            this.CertificatesTab.UseVisualStyleBackColor = true;
            // 
            // SigningCertificatesGroup
            // 
            this.SigningCertificatesGroup.Controls.Add(this.SigningCertificatesLabel);
            this.SigningCertificatesGroup.Location = new System.Drawing.Point(6, 6);
            this.SigningCertificatesGroup.Name = "SigningCertificatesGroup";
            this.SigningCertificatesGroup.Size = new System.Drawing.Size(558, 141);
            this.SigningCertificatesGroup.TabIndex = 4;
            this.SigningCertificatesGroup.TabStop = false;
            this.SigningCertificatesGroup.Text = "Valid Signing Certificates";
            // 
            // SigningCertificatesLabel
            // 
            this.SigningCertificatesLabel.AutoSize = true;
            this.SigningCertificatesLabel.Location = new System.Drawing.Point(6, 28);
            this.SigningCertificatesLabel.Name = "SigningCertificatesLabel";
            this.SigningCertificatesLabel.Size = new System.Drawing.Size(29, 13);
            this.SigningCertificatesLabel.TabIndex = 0;
            this.SigningCertificatesLabel.Text = "TBD";
            // 
            // FreeCertificateSourcesGroup
            // 
            this.FreeCertificateSourcesGroup.Controls.Add(this.FreeCertificateSourcesLabel);
            this.FreeCertificateSourcesGroup.Location = new System.Drawing.Point(6, 153);
            this.FreeCertificateSourcesGroup.Name = "FreeCertificateSourcesGroup";
            this.FreeCertificateSourcesGroup.Size = new System.Drawing.Size(558, 141);
            this.FreeCertificateSourcesGroup.TabIndex = 5;
            this.FreeCertificateSourcesGroup.TabStop = false;
            this.FreeCertificateSourcesGroup.Text = "Free Certificate Sources";
            // 
            // FreeCertificateSourcesLabel
            // 
            this.FreeCertificateSourcesLabel.AutoSize = true;
            this.FreeCertificateSourcesLabel.LinkArea = new System.Windows.Forms.LinkArea(0, 0);
            this.FreeCertificateSourcesLabel.Location = new System.Drawing.Point(6, 28);
            this.FreeCertificateSourcesLabel.Name = "FreeCertificateSourcesLabel";
            this.FreeCertificateSourcesLabel.Size = new System.Drawing.Size(468, 65);
            this.FreeCertificateSourcesLabel.TabIndex = 0;
            this.FreeCertificateSourcesLabel.Text = resources.GetString("FreeCertificateSourcesLabel.Text");
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 361);
            this.Controls.Add(this.Tabs);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(600, 400);
            this.MinimumSize = new System.Drawing.Size(600, 400);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "OpaqueMail Proxy";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Tabs.ResumeLayout(false);
            this.StartupTab.ResumeLayout(false);
            this.OperationalModeGroup.ResumeLayout(false);
            this.OperationalModeGroup.PerformLayout();
            this.SettingsTab.ResumeLayout(false);
            this.ProxyStatusGroup.ResumeLayout(false);
            this.ProxyStatusGroup.PerformLayout();
            this.ProxySettingsGroup.ResumeLayout(false);
            this.ProxySettingsGroup.PerformLayout();
            this.ProxyInstanceGroup.ResumeLayout(false);
            this.CertificatesTab.ResumeLayout(false);
            this.SigningCertificatesGroup.ResumeLayout(false);
            this.SigningCertificatesGroup.PerformLayout();
            this.FreeCertificateSourcesGroup.ResumeLayout(false);
            this.FreeCertificateSourcesGroup.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl Tabs;
        private System.Windows.Forms.TabPage StartupTab;
        private System.Windows.Forms.TabPage SettingsTab;
        private System.Windows.Forms.GroupBox OperationalModeGroup;
        private System.Windows.Forms.RadioButton OperationalModeWindowsService;
        private System.Windows.Forms.RadioButton OperationalModeApplication;
        private System.Windows.Forms.Label OperationalModeLabel;
        private System.Windows.Forms.GroupBox ProxyStatusGroup;
        private System.Windows.Forms.Label ProxyStatusLabel;
        private System.Windows.Forms.GroupBox ProxySettingsGroup;
        private System.Windows.Forms.Label ProxySettingsLabel;
        private System.Windows.Forms.GroupBox ProxyInstanceGroup;
        private System.Windows.Forms.ListView ProxyInstanceGrid;
        private System.Windows.Forms.ColumnHeader LocalIPColumn;
        private System.Windows.Forms.ColumnHeader LocalPortColumn;
        private System.Windows.Forms.ColumnHeader LocalUseSSL;
        private System.Windows.Forms.ColumnHeader DestinationHost;
        private System.Windows.Forms.ColumnHeader DestinationPort;
        private System.Windows.Forms.ColumnHeader DestinationUseSSL;
        private System.Windows.Forms.TabPage CertificatesTab;
        private System.Windows.Forms.GroupBox SigningCertificatesGroup;
        private System.Windows.Forms.Label SigningCertificatesLabel;
        private System.Windows.Forms.GroupBox FreeCertificateSourcesGroup;
        private System.Windows.Forms.LinkLabel FreeCertificateSourcesLabel;
    }
}

