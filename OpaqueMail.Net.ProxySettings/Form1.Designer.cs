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
            this.ProxyStatusGroup = new System.Windows.Forms.GroupBox();
            this.ProxyStatusLabel = new System.Windows.Forms.Label();
            this.OperationalModeGroup = new System.Windows.Forms.GroupBox();
            this.OperationalModeWindowsService = new System.Windows.Forms.RadioButton();
            this.OperationalModeApplication = new System.Windows.Forms.RadioButton();
            this.OperationalModeLabel = new System.Windows.Forms.Label();
            this.SmtpSettingsTab = new System.Windows.Forms.TabPage();
            this.SmtpProxyInstanceGroup = new System.Windows.Forms.GroupBox();
            this.SmtpProxyInstanceGrid = new System.Windows.Forms.ListView();
            this.LocalIPColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.LocalPortColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.LocalUseSSL = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.DestinationHost = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.DestinationPort = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.DestinationUseSSL = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SmtpProxySettingsGroup = new System.Windows.Forms.GroupBox();
            this.SmtpProxySettingsLabel = new System.Windows.Forms.Label();
            this.ImapSettingsTab = new System.Windows.Forms.TabPage();
            this.ImapProxyInstanceGroup = new System.Windows.Forms.GroupBox();
            this.ImapProxyInstanceGrid = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ImapProxySettingsGroup = new System.Windows.Forms.GroupBox();
            this.ImapProxySettingsLabel = new System.Windows.Forms.Label();
            this.Pop3SettingsTab = new System.Windows.Forms.TabPage();
            this.Pop3ProxyInstanceGroup = new System.Windows.Forms.GroupBox();
            this.Pop3ProxyInstanceGrid = new System.Windows.Forms.ListView();
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader8 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader9 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader10 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader11 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader12 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Pop3ProxySettingsGroup = new System.Windows.Forms.GroupBox();
            this.Pop3ProxySettingsLabel = new System.Windows.Forms.Label();
            this.CertificatesTab = new System.Windows.Forms.TabPage();
            this.FreeCertificateSourcesGroup = new System.Windows.Forms.GroupBox();
            this.FreeCertificateSourcesLabel = new System.Windows.Forms.LinkLabel();
            this.SigningCertificatesGroup = new System.Windows.Forms.GroupBox();
            this.SigningCertificatesLabel = new System.Windows.Forms.Label();
            this.Tabs.SuspendLayout();
            this.StartupTab.SuspendLayout();
            this.ProxyStatusGroup.SuspendLayout();
            this.OperationalModeGroup.SuspendLayout();
            this.SmtpSettingsTab.SuspendLayout();
            this.SmtpProxyInstanceGroup.SuspendLayout();
            this.SmtpProxySettingsGroup.SuspendLayout();
            this.ImapSettingsTab.SuspendLayout();
            this.ImapProxyInstanceGroup.SuspendLayout();
            this.ImapProxySettingsGroup.SuspendLayout();
            this.Pop3SettingsTab.SuspendLayout();
            this.Pop3ProxyInstanceGroup.SuspendLayout();
            this.Pop3ProxySettingsGroup.SuspendLayout();
            this.CertificatesTab.SuspendLayout();
            this.FreeCertificateSourcesGroup.SuspendLayout();
            this.SigningCertificatesGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // Tabs
            // 
            this.Tabs.Controls.Add(this.StartupTab);
            this.Tabs.Controls.Add(this.SmtpSettingsTab);
            this.Tabs.Controls.Add(this.ImapSettingsTab);
            this.Tabs.Controls.Add(this.Pop3SettingsTab);
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
            // SmtpSettingsTab
            // 
            this.SmtpSettingsTab.Controls.Add(this.SmtpProxyInstanceGroup);
            this.SmtpSettingsTab.Controls.Add(this.SmtpProxySettingsGroup);
            this.SmtpSettingsTab.Location = new System.Drawing.Point(4, 22);
            this.SmtpSettingsTab.Name = "SmtpSettingsTab";
            this.SmtpSettingsTab.Padding = new System.Windows.Forms.Padding(3);
            this.SmtpSettingsTab.Size = new System.Drawing.Size(570, 332);
            this.SmtpSettingsTab.TabIndex = 1;
            this.SmtpSettingsTab.Text = "SMTP Proxy Settings";
            this.SmtpSettingsTab.UseVisualStyleBackColor = true;
            // 
            // SmtpProxyInstanceGroup
            // 
            this.SmtpProxyInstanceGroup.Controls.Add(this.SmtpProxyInstanceGrid);
            this.SmtpProxyInstanceGroup.Location = new System.Drawing.Point(6, 115);
            this.SmtpProxyInstanceGroup.Name = "SmtpProxyInstanceGroup";
            this.SmtpProxyInstanceGroup.Size = new System.Drawing.Size(558, 209);
            this.SmtpProxyInstanceGroup.TabIndex = 5;
            this.SmtpProxyInstanceGroup.TabStop = false;
            this.SmtpProxyInstanceGroup.Text = "Proxy Instances";
            // 
            // SmtpProxyInstanceGrid
            // 
            this.SmtpProxyInstanceGrid.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.LocalIPColumn,
            this.LocalPortColumn,
            this.LocalUseSSL,
            this.DestinationHost,
            this.DestinationPort,
            this.DestinationUseSSL});
            this.SmtpProxyInstanceGrid.GridLines = true;
            this.SmtpProxyInstanceGrid.Location = new System.Drawing.Point(9, 19);
            this.SmtpProxyInstanceGrid.Name = "SmtpProxyInstanceGrid";
            this.SmtpProxyInstanceGrid.Size = new System.Drawing.Size(543, 184);
            this.SmtpProxyInstanceGrid.TabIndex = 1;
            this.SmtpProxyInstanceGrid.UseCompatibleStateImageBehavior = false;
            this.SmtpProxyInstanceGrid.View = System.Windows.Forms.View.Details;
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
            this.DestinationHost.Text = "Remote Server Host";
            this.DestinationHost.Width = 90;
            // 
            // DestinationPort
            // 
            this.DestinationPort.Text = "Remote Server Port";
            this.DestinationPort.Width = 89;
            // 
            // DestinationUseSSL
            // 
            this.DestinationUseSSL.Text = "Remote Server SSL/TLS?";
            this.DestinationUseSSL.Width = 120;
            // 
            // SmtpProxySettingsGroup
            // 
            this.SmtpProxySettingsGroup.Controls.Add(this.SmtpProxySettingsLabel);
            this.SmtpProxySettingsGroup.Location = new System.Drawing.Point(6, 6);
            this.SmtpProxySettingsGroup.Name = "SmtpProxySettingsGroup";
            this.SmtpProxySettingsGroup.Size = new System.Drawing.Size(558, 103);
            this.SmtpProxySettingsGroup.TabIndex = 4;
            this.SmtpProxySettingsGroup.TabStop = false;
            this.SmtpProxySettingsGroup.Text = "Proxy Settings";
            // 
            // SmtpProxySettingsLabel
            // 
            this.SmtpProxySettingsLabel.AutoSize = true;
            this.SmtpProxySettingsLabel.Location = new System.Drawing.Point(6, 28);
            this.SmtpProxySettingsLabel.Name = "SmtpProxySettingsLabel";
            this.SmtpProxySettingsLabel.Size = new System.Drawing.Size(29, 13);
            this.SmtpProxySettingsLabel.TabIndex = 0;
            this.SmtpProxySettingsLabel.Text = "TBD";
            // 
            // ImapSettingsTab
            // 
            this.ImapSettingsTab.Controls.Add(this.ImapProxyInstanceGroup);
            this.ImapSettingsTab.Controls.Add(this.ImapProxySettingsGroup);
            this.ImapSettingsTab.Location = new System.Drawing.Point(4, 22);
            this.ImapSettingsTab.Name = "ImapSettingsTab";
            this.ImapSettingsTab.Size = new System.Drawing.Size(570, 332);
            this.ImapSettingsTab.TabIndex = 4;
            this.ImapSettingsTab.Text = "IMAP Proxy Settings";
            this.ImapSettingsTab.UseVisualStyleBackColor = true;
            // 
            // ImapProxyInstanceGroup
            // 
            this.ImapProxyInstanceGroup.Controls.Add(this.ImapProxyInstanceGrid);
            this.ImapProxyInstanceGroup.Location = new System.Drawing.Point(6, 115);
            this.ImapProxyInstanceGroup.Name = "ImapProxyInstanceGroup";
            this.ImapProxyInstanceGroup.Size = new System.Drawing.Size(558, 209);
            this.ImapProxyInstanceGroup.TabIndex = 7;
            this.ImapProxyInstanceGroup.TabStop = false;
            this.ImapProxyInstanceGroup.Text = "Proxy Instances";
            // 
            // ImapProxyInstanceGrid
            // 
            this.ImapProxyInstanceGrid.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4,
            this.columnHeader5,
            this.columnHeader6});
            this.ImapProxyInstanceGrid.GridLines = true;
            this.ImapProxyInstanceGrid.Location = new System.Drawing.Point(9, 19);
            this.ImapProxyInstanceGrid.Name = "ImapProxyInstanceGrid";
            this.ImapProxyInstanceGrid.Size = new System.Drawing.Size(543, 184);
            this.ImapProxyInstanceGrid.TabIndex = 1;
            this.ImapProxyInstanceGrid.UseCompatibleStateImageBehavior = false;
            this.ImapProxyInstanceGrid.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Local IP";
            this.columnHeader1.Width = 70;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Local Port";
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Local SSL/TLS?";
            this.columnHeader3.Width = 92;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Remote Server Host";
            this.columnHeader4.Width = 90;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "Remote Server Port";
            this.columnHeader5.Width = 89;
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "Remote Server SSL/TLS?";
            this.columnHeader6.Width = 120;
            // 
            // ImapProxySettingsGroup
            // 
            this.ImapProxySettingsGroup.Controls.Add(this.ImapProxySettingsLabel);
            this.ImapProxySettingsGroup.Location = new System.Drawing.Point(6, 6);
            this.ImapProxySettingsGroup.Name = "ImapProxySettingsGroup";
            this.ImapProxySettingsGroup.Size = new System.Drawing.Size(558, 103);
            this.ImapProxySettingsGroup.TabIndex = 6;
            this.ImapProxySettingsGroup.TabStop = false;
            this.ImapProxySettingsGroup.Text = "Proxy Settings";
            // 
            // ImapProxySettingsLabel
            // 
            this.ImapProxySettingsLabel.AutoSize = true;
            this.ImapProxySettingsLabel.Location = new System.Drawing.Point(6, 28);
            this.ImapProxySettingsLabel.Name = "ImapProxySettingsLabel";
            this.ImapProxySettingsLabel.Size = new System.Drawing.Size(29, 13);
            this.ImapProxySettingsLabel.TabIndex = 0;
            this.ImapProxySettingsLabel.Text = "TBD";
            // 
            // Pop3SettingsTab
            // 
            this.Pop3SettingsTab.Controls.Add(this.Pop3ProxyInstanceGroup);
            this.Pop3SettingsTab.Controls.Add(this.Pop3ProxySettingsGroup);
            this.Pop3SettingsTab.Location = new System.Drawing.Point(4, 22);
            this.Pop3SettingsTab.Name = "Pop3SettingsTab";
            this.Pop3SettingsTab.Size = new System.Drawing.Size(570, 332);
            this.Pop3SettingsTab.TabIndex = 5;
            this.Pop3SettingsTab.Text = "POP3 Proxy Settings";
            this.Pop3SettingsTab.UseVisualStyleBackColor = true;
            // 
            // Pop3ProxyInstanceGroup
            // 
            this.Pop3ProxyInstanceGroup.Controls.Add(this.Pop3ProxyInstanceGrid);
            this.Pop3ProxyInstanceGroup.Location = new System.Drawing.Point(6, 115);
            this.Pop3ProxyInstanceGroup.Name = "Pop3ProxyInstanceGroup";
            this.Pop3ProxyInstanceGroup.Size = new System.Drawing.Size(558, 209);
            this.Pop3ProxyInstanceGroup.TabIndex = 7;
            this.Pop3ProxyInstanceGroup.TabStop = false;
            this.Pop3ProxyInstanceGroup.Text = "Proxy Instances";
            // 
            // Pop3ProxyInstanceGrid
            // 
            this.Pop3ProxyInstanceGrid.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader7,
            this.columnHeader8,
            this.columnHeader9,
            this.columnHeader10,
            this.columnHeader11,
            this.columnHeader12});
            this.Pop3ProxyInstanceGrid.GridLines = true;
            this.Pop3ProxyInstanceGrid.Location = new System.Drawing.Point(9, 19);
            this.Pop3ProxyInstanceGrid.Name = "Pop3ProxyInstanceGrid";
            this.Pop3ProxyInstanceGrid.Size = new System.Drawing.Size(543, 184);
            this.Pop3ProxyInstanceGrid.TabIndex = 1;
            this.Pop3ProxyInstanceGrid.UseCompatibleStateImageBehavior = false;
            this.Pop3ProxyInstanceGrid.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader7
            // 
            this.columnHeader7.Text = "Local IP";
            this.columnHeader7.Width = 70;
            // 
            // columnHeader8
            // 
            this.columnHeader8.Text = "Local Port";
            // 
            // columnHeader9
            // 
            this.columnHeader9.Text = "Local SSL/TLS?";
            this.columnHeader9.Width = 92;
            // 
            // columnHeader10
            // 
            this.columnHeader10.Text = "Remote Server Host";
            this.columnHeader10.Width = 90;
            // 
            // columnHeader11
            // 
            this.columnHeader11.Text = "Remote Server Port";
            this.columnHeader11.Width = 89;
            // 
            // columnHeader12
            // 
            this.columnHeader12.Text = "Remote Server SSL/TLS?";
            this.columnHeader12.Width = 120;
            // 
            // Pop3ProxySettingsGroup
            // 
            this.Pop3ProxySettingsGroup.Controls.Add(this.Pop3ProxySettingsLabel);
            this.Pop3ProxySettingsGroup.Location = new System.Drawing.Point(6, 6);
            this.Pop3ProxySettingsGroup.Name = "Pop3ProxySettingsGroup";
            this.Pop3ProxySettingsGroup.Size = new System.Drawing.Size(558, 103);
            this.Pop3ProxySettingsGroup.TabIndex = 6;
            this.Pop3ProxySettingsGroup.TabStop = false;
            this.Pop3ProxySettingsGroup.Text = "Proxy Settings";
            // 
            // Pop3ProxySettingsLabel
            // 
            this.Pop3ProxySettingsLabel.AutoSize = true;
            this.Pop3ProxySettingsLabel.Location = new System.Drawing.Point(6, 28);
            this.Pop3ProxySettingsLabel.Name = "Pop3ProxySettingsLabel";
            this.Pop3ProxySettingsLabel.Size = new System.Drawing.Size(29, 13);
            this.Pop3ProxySettingsLabel.TabIndex = 0;
            this.Pop3ProxySettingsLabel.Text = "TBD";
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
            this.ProxyStatusGroup.ResumeLayout(false);
            this.ProxyStatusGroup.PerformLayout();
            this.OperationalModeGroup.ResumeLayout(false);
            this.OperationalModeGroup.PerformLayout();
            this.SmtpSettingsTab.ResumeLayout(false);
            this.SmtpProxyInstanceGroup.ResumeLayout(false);
            this.SmtpProxySettingsGroup.ResumeLayout(false);
            this.SmtpProxySettingsGroup.PerformLayout();
            this.ImapSettingsTab.ResumeLayout(false);
            this.ImapProxyInstanceGroup.ResumeLayout(false);
            this.ImapProxySettingsGroup.ResumeLayout(false);
            this.ImapProxySettingsGroup.PerformLayout();
            this.Pop3SettingsTab.ResumeLayout(false);
            this.Pop3ProxyInstanceGroup.ResumeLayout(false);
            this.Pop3ProxySettingsGroup.ResumeLayout(false);
            this.Pop3ProxySettingsGroup.PerformLayout();
            this.CertificatesTab.ResumeLayout(false);
            this.FreeCertificateSourcesGroup.ResumeLayout(false);
            this.FreeCertificateSourcesGroup.PerformLayout();
            this.SigningCertificatesGroup.ResumeLayout(false);
            this.SigningCertificatesGroup.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl Tabs;
        private System.Windows.Forms.TabPage StartupTab;
        private System.Windows.Forms.TabPage SmtpSettingsTab;
        private System.Windows.Forms.GroupBox OperationalModeGroup;
        private System.Windows.Forms.RadioButton OperationalModeWindowsService;
        private System.Windows.Forms.RadioButton OperationalModeApplication;
        private System.Windows.Forms.Label OperationalModeLabel;
        private System.Windows.Forms.GroupBox ProxyStatusGroup;
        private System.Windows.Forms.Label ProxyStatusLabel;
        private System.Windows.Forms.GroupBox SmtpProxySettingsGroup;
        private System.Windows.Forms.Label SmtpProxySettingsLabel;
        private System.Windows.Forms.GroupBox SmtpProxyInstanceGroup;
        private System.Windows.Forms.ListView SmtpProxyInstanceGrid;
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
        private System.Windows.Forms.TabPage ImapSettingsTab;
        private System.Windows.Forms.GroupBox ImapProxyInstanceGroup;
        private System.Windows.Forms.ListView ImapProxyInstanceGrid;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.GroupBox ImapProxySettingsGroup;
        private System.Windows.Forms.Label ImapProxySettingsLabel;
        private System.Windows.Forms.TabPage Pop3SettingsTab;
        private System.Windows.Forms.GroupBox Pop3ProxyInstanceGroup;
        private System.Windows.Forms.ListView Pop3ProxyInstanceGrid;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.ColumnHeader columnHeader8;
        private System.Windows.Forms.ColumnHeader columnHeader9;
        private System.Windows.Forms.ColumnHeader columnHeader10;
        private System.Windows.Forms.ColumnHeader columnHeader11;
        private System.Windows.Forms.ColumnHeader columnHeader12;
        private System.Windows.Forms.GroupBox Pop3ProxySettingsGroup;
        private System.Windows.Forms.Label Pop3ProxySettingsLabel;
    }
}

