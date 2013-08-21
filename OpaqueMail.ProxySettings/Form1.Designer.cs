/*
 * OpaqueMail (http://opaquemail.org/).
 * 
 * Licensed according to the MIT License (http://mit-license.org/).
 * 
 * Copyright © Bert Johnson (http://bertjohnson.net) of Bkip Inc. (http://bkip.com).
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the “Software”), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 * 
 */

namespace OpaqueMail.Proxy.Settings
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
            this.AboutTab = new System.Windows.Forms.TabPage();
            this.GettingStartedGroupBox = new System.Windows.Forms.GroupBox();
            this.GettingStartedLabel = new System.Windows.Forms.LinkLabel();
            this.AboutGroupBox = new System.Windows.Forms.GroupBox();
            this.AboutLabel = new System.Windows.Forms.LinkLabel();
            this.SelectAccountsTab = new System.Windows.Forms.TabPage();
            this.AccountGroupBox = new System.Windows.Forms.GroupBox();
            this.AccountsLabel = new System.Windows.Forms.LinkLabel();
            this.AccountGrid = new System.Windows.Forms.DataGridView();
            this.ClientColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.AccountColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ProtectedColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.CertificateColumn = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.RegistryKeyColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ConfirmCertificatesTab = new System.Windows.Forms.TabPage();
            this.ProtectionGroupBox = new System.Windows.Forms.GroupBox();
            this.SmimeOperations = new System.Windows.Forms.ComboBox();
            this.SmimeOperationsLabel = new System.Windows.Forms.Label();
            this.ProtectionModeLabel = new System.Windows.Forms.Label();
            this.SmimeSettingsMode = new System.Windows.Forms.ComboBox();
            this.SmimeSettingsModeLabel = new System.Windows.Forms.Label();
            this.CertificateGroupBox = new System.Windows.Forms.GroupBox();
            this.CertificateLabel = new System.Windows.Forms.LinkLabel();
            this.OtherOptionsTab = new System.Windows.Forms.TabPage();
            this.SpreadTheWordGroupBox = new System.Windows.Forms.GroupBox();
            this.UpdateOutlookSignature = new System.Windows.Forms.CheckBox();
            this.SpreadTheWordLabel = new System.Windows.Forms.Label();
            this.FirewallGroupBox = new System.Windows.Forms.GroupBox();
            this.UpdateFirewall = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.NetworkAccess = new System.Windows.Forms.ComboBox();
            this.NetworkAccessLabel = new System.Windows.Forms.Label();
            this.SaveSettingsButton = new System.Windows.Forms.Button();
            this.ServiceStatusLabel = new System.Windows.Forms.Label();
            this.Tabs.SuspendLayout();
            this.AboutTab.SuspendLayout();
            this.GettingStartedGroupBox.SuspendLayout();
            this.AboutGroupBox.SuspendLayout();
            this.SelectAccountsTab.SuspendLayout();
            this.AccountGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.AccountGrid)).BeginInit();
            this.ConfirmCertificatesTab.SuspendLayout();
            this.ProtectionGroupBox.SuspendLayout();
            this.CertificateGroupBox.SuspendLayout();
            this.OtherOptionsTab.SuspendLayout();
            this.SpreadTheWordGroupBox.SuspendLayout();
            this.FirewallGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // Tabs
            // 
            this.Tabs.Controls.Add(this.AboutTab);
            this.Tabs.Controls.Add(this.SelectAccountsTab);
            this.Tabs.Controls.Add(this.ConfirmCertificatesTab);
            this.Tabs.Controls.Add(this.OtherOptionsTab);
            this.Tabs.Location = new System.Drawing.Point(3, 3);
            this.Tabs.Name = "Tabs";
            this.Tabs.SelectedIndex = 0;
            this.Tabs.Size = new System.Drawing.Size(578, 392);
            this.Tabs.TabIndex = 3;
            // 
            // AboutTab
            // 
            this.AboutTab.Controls.Add(this.GettingStartedGroupBox);
            this.AboutTab.Controls.Add(this.AboutGroupBox);
            this.AboutTab.Location = new System.Drawing.Point(4, 22);
            this.AboutTab.Name = "AboutTab";
            this.AboutTab.Padding = new System.Windows.Forms.Padding(3);
            this.AboutTab.Size = new System.Drawing.Size(570, 366);
            this.AboutTab.TabIndex = 0;
            this.AboutTab.Text = "About OpaqueMail Proxy";
            this.AboutTab.UseVisualStyleBackColor = true;
            // 
            // GettingStartedGroupBox
            // 
            this.GettingStartedGroupBox.Controls.Add(this.GettingStartedLabel);
            this.GettingStartedGroupBox.Location = new System.Drawing.Point(6, 176);
            this.GettingStartedGroupBox.Name = "GettingStartedGroupBox";
            this.GettingStartedGroupBox.Size = new System.Drawing.Size(558, 141);
            this.GettingStartedGroupBox.TabIndex = 2;
            this.GettingStartedGroupBox.TabStop = false;
            this.GettingStartedGroupBox.Text = "Getting Started";
            // 
            // GettingStartedLabel
            // 
            this.GettingStartedLabel.Location = new System.Drawing.Point(6, 28);
            this.GettingStartedLabel.Name = "GettingStartedLabel";
            this.GettingStartedLabel.Size = new System.Drawing.Size(546, 113);
            this.GettingStartedLabel.TabIndex = 3;
            this.GettingStartedLabel.TabStop = true;
            this.GettingStartedLabel.Text = resources.GetString("GettingStartedLabel.Text");
            // 
            // AboutGroupBox
            // 
            this.AboutGroupBox.Controls.Add(this.AboutLabel);
            this.AboutGroupBox.Location = new System.Drawing.Point(6, 6);
            this.AboutGroupBox.Name = "AboutGroupBox";
            this.AboutGroupBox.Size = new System.Drawing.Size(558, 167);
            this.AboutGroupBox.TabIndex = 0;
            this.AboutGroupBox.TabStop = false;
            this.AboutGroupBox.Text = "About OpaqueMail Proxy";
            // 
            // AboutLabel
            // 
            this.AboutLabel.Location = new System.Drawing.Point(6, 28);
            this.AboutLabel.Name = "AboutLabel";
            this.AboutLabel.Size = new System.Drawing.Size(546, 139);
            this.AboutLabel.TabIndex = 1;
            this.AboutLabel.TabStop = true;
            this.AboutLabel.Text = resources.GetString("AboutLabel.Text");
            // 
            // SelectAccountsTab
            // 
            this.SelectAccountsTab.Controls.Add(this.AccountGroupBox);
            this.SelectAccountsTab.Location = new System.Drawing.Point(4, 22);
            this.SelectAccountsTab.Name = "SelectAccountsTab";
            this.SelectAccountsTab.Padding = new System.Windows.Forms.Padding(3);
            this.SelectAccountsTab.Size = new System.Drawing.Size(570, 366);
            this.SelectAccountsTab.TabIndex = 1;
            this.SelectAccountsTab.Text = "1. Select E-mail Accounts";
            this.SelectAccountsTab.UseVisualStyleBackColor = true;
            // 
            // AccountGroupBox
            // 
            this.AccountGroupBox.Controls.Add(this.AccountsLabel);
            this.AccountGroupBox.Controls.Add(this.AccountGrid);
            this.AccountGroupBox.Location = new System.Drawing.Point(6, 6);
            this.AccountGroupBox.Name = "AccountGroupBox";
            this.AccountGroupBox.Size = new System.Drawing.Size(558, 354);
            this.AccountGroupBox.TabIndex = 0;
            this.AccountGroupBox.TabStop = false;
            this.AccountGroupBox.Text = "Select E-Mail Accounts to Protect";
            // 
            // AccountsLabel
            // 
            this.AccountsLabel.Location = new System.Drawing.Point(6, 28);
            this.AccountsLabel.Name = "AccountsLabel";
            this.AccountsLabel.Size = new System.Drawing.Size(546, 62);
            this.AccountsLabel.TabIndex = 1;
            this.AccountsLabel.TabStop = true;
            this.AccountsLabel.Text = resources.GetString("AccountsLabel.Text");
            // 
            // AccountGrid
            // 
            this.AccountGrid.AllowUserToAddRows = false;
            this.AccountGrid.AllowUserToDeleteRows = false;
            this.AccountGrid.AllowUserToResizeRows = false;
            this.AccountGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.AccountGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ClientColumn,
            this.AccountColumn,
            this.ProtectedColumn,
            this.CertificateColumn,
            this.RegistryKeyColumn});
            this.AccountGrid.Location = new System.Drawing.Point(6, 93);
            this.AccountGrid.Name = "AccountGrid";
            this.AccountGrid.RowHeadersVisible = false;
            this.AccountGrid.ShowCellErrors = false;
            this.AccountGrid.ShowCellToolTips = false;
            this.AccountGrid.ShowEditingIcon = false;
            this.AccountGrid.ShowRowErrors = false;
            this.AccountGrid.Size = new System.Drawing.Size(546, 255);
            this.AccountGrid.TabIndex = 2;
            // 
            // ClientColumn
            // 
            this.ClientColumn.HeaderText = "Client";
            this.ClientColumn.Name = "ClientColumn";
            this.ClientColumn.ReadOnly = true;
            this.ClientColumn.Width = 75;
            // 
            // AccountColumn
            // 
            this.AccountColumn.HeaderText = "Account";
            this.AccountColumn.Name = "AccountColumn";
            this.AccountColumn.ReadOnly = true;
            this.AccountColumn.Width = 160;
            // 
            // ProtectedColumn
            // 
            this.ProtectedColumn.HeaderText = "Protected?";
            this.ProtectedColumn.Name = "ProtectedColumn";
            this.ProtectedColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.ProtectedColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.ProtectedColumn.Width = 60;
            // 
            // CertificateColumn
            // 
            this.CertificateColumn.HeaderText = "S/MIME Certificate";
            this.CertificateColumn.Name = "CertificateColumn";
            this.CertificateColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.CertificateColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.CertificateColumn.Width = 248;
            // 
            // RegistryKeyColumn
            // 
            this.RegistryKeyColumn.HeaderText = "Registry Key";
            this.RegistryKeyColumn.Name = "RegistryKeyColumn";
            this.RegistryKeyColumn.Visible = false;
            // 
            // ConfirmCertificatesTab
            // 
            this.ConfirmCertificatesTab.Controls.Add(this.ProtectionGroupBox);
            this.ConfirmCertificatesTab.Controls.Add(this.CertificateGroupBox);
            this.ConfirmCertificatesTab.Location = new System.Drawing.Point(4, 22);
            this.ConfirmCertificatesTab.Name = "ConfirmCertificatesTab";
            this.ConfirmCertificatesTab.Size = new System.Drawing.Size(570, 366);
            this.ConfirmCertificatesTab.TabIndex = 2;
            this.ConfirmCertificatesTab.Text = "2. Confirm Certificates";
            this.ConfirmCertificatesTab.UseVisualStyleBackColor = true;
            // 
            // ProtectionGroupBox
            // 
            this.ProtectionGroupBox.Controls.Add(this.SmimeOperations);
            this.ProtectionGroupBox.Controls.Add(this.SmimeOperationsLabel);
            this.ProtectionGroupBox.Controls.Add(this.ProtectionModeLabel);
            this.ProtectionGroupBox.Controls.Add(this.SmimeSettingsMode);
            this.ProtectionGroupBox.Controls.Add(this.SmimeSettingsModeLabel);
            this.ProtectionGroupBox.Location = new System.Drawing.Point(6, 168);
            this.ProtectionGroupBox.Name = "ProtectionGroupBox";
            this.ProtectionGroupBox.Size = new System.Drawing.Size(559, 170);
            this.ProtectionGroupBox.TabIndex = 2;
            this.ProtectionGroupBox.TabStop = false;
            this.ProtectionGroupBox.Text = "Choose S/MIME Protection Mode";
            // 
            // SmimeOperations
            // 
            this.SmimeOperations.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.SmimeOperations.FormattingEnabled = true;
            this.SmimeOperations.Items.AddRange(new object[] {
            "Apply no S/MIME protection",
            "Sign messages only",
            "Encrypt messages only",
            "Sign and encrypt messages (Recommended)"});
            this.SmimeOperations.Location = new System.Drawing.Point(145, 114);
            this.SmimeOperations.Name = "SmimeOperations";
            this.SmimeOperations.Size = new System.Drawing.Size(408, 21);
            this.SmimeOperations.TabIndex = 5;
            // 
            // SmimeOperationsLabel
            // 
            this.SmimeOperationsLabel.AutoSize = true;
            this.SmimeOperationsLabel.Location = new System.Drawing.Point(7, 117);
            this.SmimeOperationsLabel.Name = "SmimeOperationsLabel";
            this.SmimeOperationsLabel.Size = new System.Drawing.Size(102, 13);
            this.SmimeOperationsLabel.TabIndex = 4;
            this.SmimeOperationsLabel.Text = "S/MIME operations:";
            // 
            // ProtectionModeLabel
            // 
            this.ProtectionModeLabel.Location = new System.Drawing.Point(6, 28);
            this.ProtectionModeLabel.Name = "ProtectionModeLabel";
            this.ProtectionModeLabel.Size = new System.Drawing.Size(546, 72);
            this.ProtectionModeLabel.TabIndex = 3;
            this.ProtectionModeLabel.Text = resources.GetString("ProtectionModeLabel.Text");
            // 
            // SmimeSettingsMode
            // 
            this.SmimeSettingsMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.SmimeSettingsMode.FormattingEnabled = true;
            this.SmimeSettingsMode.Items.AddRange(new object[] {
            "Best effort: try to protect, but allow unprotected mail",
            "Require exact settings"});
            this.SmimeSettingsMode.Location = new System.Drawing.Point(145, 141);
            this.SmimeSettingsMode.Name = "SmimeSettingsMode";
            this.SmimeSettingsMode.Size = new System.Drawing.Size(408, 21);
            this.SmimeSettingsMode.TabIndex = 7;
            // 
            // SmimeSettingsModeLabel
            // 
            this.SmimeSettingsModeLabel.AutoSize = true;
            this.SmimeSettingsModeLabel.Location = new System.Drawing.Point(7, 144);
            this.SmimeSettingsModeLabel.Name = "SmimeSettingsModeLabel";
            this.SmimeSettingsModeLabel.Size = new System.Drawing.Size(132, 13);
            this.SmimeSettingsModeLabel.TabIndex = 6;
            this.SmimeSettingsModeLabel.Text = "S/MIME protection mode: ";
            // 
            // CertificateGroupBox
            // 
            this.CertificateGroupBox.Controls.Add(this.CertificateLabel);
            this.CertificateGroupBox.Location = new System.Drawing.Point(6, 6);
            this.CertificateGroupBox.Name = "CertificateGroupBox";
            this.CertificateGroupBox.Size = new System.Drawing.Size(559, 156);
            this.CertificateGroupBox.TabIndex = 0;
            this.CertificateGroupBox.TabStop = false;
            this.CertificateGroupBox.Text = "Confirm S/MIME Certificate(s)";
            // 
            // CertificateLabel
            // 
            this.CertificateLabel.Location = new System.Drawing.Point(6, 28);
            this.CertificateLabel.Name = "CertificateLabel";
            this.CertificateLabel.Size = new System.Drawing.Size(547, 125);
            this.CertificateLabel.TabIndex = 1;
            this.CertificateLabel.TabStop = true;
            this.CertificateLabel.Text = resources.GetString("CertificateLabel.Text");
            // 
            // OtherOptionsTab
            // 
            this.OtherOptionsTab.Controls.Add(this.SpreadTheWordGroupBox);
            this.OtherOptionsTab.Controls.Add(this.FirewallGroupBox);
            this.OtherOptionsTab.Location = new System.Drawing.Point(4, 22);
            this.OtherOptionsTab.Name = "OtherOptionsTab";
            this.OtherOptionsTab.Size = new System.Drawing.Size(570, 366);
            this.OtherOptionsTab.TabIndex = 3;
            this.OtherOptionsTab.Text = "3. Manage Other Options";
            this.OtherOptionsTab.UseVisualStyleBackColor = true;
            // 
            // SpreadTheWordGroupBox
            // 
            this.SpreadTheWordGroupBox.Controls.Add(this.UpdateOutlookSignature);
            this.SpreadTheWordGroupBox.Controls.Add(this.SpreadTheWordLabel);
            this.SpreadTheWordGroupBox.Location = new System.Drawing.Point(6, 162);
            this.SpreadTheWordGroupBox.Name = "SpreadTheWordGroupBox";
            this.SpreadTheWordGroupBox.Size = new System.Drawing.Size(559, 150);
            this.SpreadTheWordGroupBox.TabIndex = 5;
            this.SpreadTheWordGroupBox.TabStop = false;
            this.SpreadTheWordGroupBox.Text = "Spread the Word";
            // 
            // UpdateOutlookSignature
            // 
            this.UpdateOutlookSignature.AutoSize = true;
            this.UpdateOutlookSignature.Checked = true;
            this.UpdateOutlookSignature.CheckState = System.Windows.Forms.CheckState.Checked;
            this.UpdateOutlookSignature.Location = new System.Drawing.Point(103, 97);
            this.UpdateOutlookSignature.Name = "UpdateOutlookSignature";
            this.UpdateOutlookSignature.Size = new System.Drawing.Size(256, 17);
            this.UpdateOutlookSignature.TabIndex = 7;
            this.UpdateOutlookSignature.Text = "Update Outlook Signature to Link to OpaqueMail";
            this.UpdateOutlookSignature.UseVisualStyleBackColor = true;
            // 
            // SpreadTheWordLabel
            // 
            this.SpreadTheWordLabel.Location = new System.Drawing.Point(6, 28);
            this.SpreadTheWordLabel.Name = "SpreadTheWordLabel";
            this.SpreadTheWordLabel.Size = new System.Drawing.Size(546, 57);
            this.SpreadTheWordLabel.TabIndex = 6;
            this.SpreadTheWordLabel.Text = resources.GetString("SpreadTheWordLabel.Text");
            // 
            // FirewallGroupBox
            // 
            this.FirewallGroupBox.Controls.Add(this.UpdateFirewall);
            this.FirewallGroupBox.Controls.Add(this.label1);
            this.FirewallGroupBox.Controls.Add(this.NetworkAccess);
            this.FirewallGroupBox.Controls.Add(this.NetworkAccessLabel);
            this.FirewallGroupBox.Location = new System.Drawing.Point(6, 6);
            this.FirewallGroupBox.Name = "FirewallGroupBox";
            this.FirewallGroupBox.Size = new System.Drawing.Size(559, 150);
            this.FirewallGroupBox.TabIndex = 0;
            this.FirewallGroupBox.TabStop = false;
            this.FirewallGroupBox.Text = "Configure Firewall";
            // 
            // UpdateFirewall
            // 
            this.UpdateFirewall.AutoSize = true;
            this.UpdateFirewall.Checked = true;
            this.UpdateFirewall.CheckState = System.Windows.Forms.CheckState.Checked;
            this.UpdateFirewall.Location = new System.Drawing.Point(103, 125);
            this.UpdateFirewall.Name = "UpdateFirewall";
            this.UpdateFirewall.Size = new System.Drawing.Size(239, 17);
            this.UpdateFirewall.TabIndex = 4;
            this.UpdateFirewall.Text = "Automatically Configure the Windows Firewall";
            this.UpdateFirewall.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(6, 28);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(546, 57);
            this.label1.TabIndex = 1;
            this.label1.Text = resources.GetString("label1.Text");
            // 
            // NetworkAccess
            // 
            this.NetworkAccess.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.NetworkAccess.FormattingEnabled = true;
            this.NetworkAccess.Items.AddRange(new object[] {
            "Only allow connections from the local computer (0.0.0.0)",
            "Only allow connections from machines on the local network (192.168.*)",
            "Allow connections from anyone (*)"});
            this.NetworkAccess.Location = new System.Drawing.Point(103, 98);
            this.NetworkAccess.Name = "NetworkAccess";
            this.NetworkAccess.Size = new System.Drawing.Size(450, 21);
            this.NetworkAccess.TabIndex = 3;
            // 
            // NetworkAccessLabel
            // 
            this.NetworkAccessLabel.AutoSize = true;
            this.NetworkAccessLabel.Location = new System.Drawing.Point(6, 101);
            this.NetworkAccessLabel.Name = "NetworkAccessLabel";
            this.NetworkAccessLabel.Size = new System.Drawing.Size(91, 13);
            this.NetworkAccessLabel.TabIndex = 2;
            this.NetworkAccessLabel.Text = "Network Access: ";
            // 
            // SaveSettingsButton
            // 
            this.SaveSettingsButton.Location = new System.Drawing.Point(440, 397);
            this.SaveSettingsButton.Name = "SaveSettingsButton";
            this.SaveSettingsButton.Size = new System.Drawing.Size(138, 23);
            this.SaveSettingsButton.TabIndex = 10;
            this.SaveSettingsButton.Text = "Save Settings";
            this.SaveSettingsButton.UseVisualStyleBackColor = true;
            this.SaveSettingsButton.Click += new System.EventHandler(this.SaveSettingsButton_Click);
            // 
            // ServiceStatusLabel
            // 
            this.ServiceStatusLabel.Location = new System.Drawing.Point(4, 402);
            this.ServiceStatusLabel.Name = "ServiceStatusLabel";
            this.ServiceStatusLabel.Size = new System.Drawing.Size(434, 23);
            this.ServiceStatusLabel.TabIndex = 11;
            this.ServiceStatusLabel.Text = "The OpaqueMail Proxy service is currently stopped.";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 421);
            this.Controls.Add(this.ServiceStatusLabel);
            this.Controls.Add(this.SaveSettingsButton);
            this.Controls.Add(this.Tabs);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(600, 460);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(600, 460);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "OpaqueMail Proxy Settings";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Tabs.ResumeLayout(false);
            this.AboutTab.ResumeLayout(false);
            this.GettingStartedGroupBox.ResumeLayout(false);
            this.AboutGroupBox.ResumeLayout(false);
            this.SelectAccountsTab.ResumeLayout(false);
            this.AccountGroupBox.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.AccountGrid)).EndInit();
            this.ConfirmCertificatesTab.ResumeLayout(false);
            this.ProtectionGroupBox.ResumeLayout(false);
            this.ProtectionGroupBox.PerformLayout();
            this.CertificateGroupBox.ResumeLayout(false);
            this.OtherOptionsTab.ResumeLayout(false);
            this.SpreadTheWordGroupBox.ResumeLayout(false);
            this.SpreadTheWordGroupBox.PerformLayout();
            this.FirewallGroupBox.ResumeLayout(false);
            this.FirewallGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl Tabs;
        private System.Windows.Forms.TabPage AboutTab;
        private System.Windows.Forms.GroupBox AboutGroupBox;
        private System.Windows.Forms.LinkLabel AboutLabel;
        private System.Windows.Forms.TabPage SelectAccountsTab;
        private System.Windows.Forms.GroupBox AccountGroupBox;
        private System.Windows.Forms.DataGridView AccountGrid;
        private System.Windows.Forms.TabPage ConfirmCertificatesTab;
        private System.Windows.Forms.GroupBox CertificateGroupBox;
        private System.Windows.Forms.LinkLabel CertificateLabel;
        private System.Windows.Forms.TabPage OtherOptionsTab;
        private System.Windows.Forms.Button SaveSettingsButton;
        private System.Windows.Forms.GroupBox GettingStartedGroupBox;
        private System.Windows.Forms.LinkLabel GettingStartedLabel;
        private System.Windows.Forms.LinkLabel AccountsLabel;
        private System.Windows.Forms.GroupBox ProtectionGroupBox;
        private System.Windows.Forms.ComboBox SmimeSettingsMode;
        private System.Windows.Forms.Label SmimeSettingsModeLabel;
        private System.Windows.Forms.ComboBox SmimeOperations;
        private System.Windows.Forms.Label SmimeOperationsLabel;
        private System.Windows.Forms.Label ProtectionModeLabel;
        private System.Windows.Forms.GroupBox FirewallGroupBox;
        private System.Windows.Forms.ComboBox NetworkAccess;
        private System.Windows.Forms.Label NetworkAccessLabel;
        private System.Windows.Forms.GroupBox SpreadTheWordGroupBox;
        private System.Windows.Forms.CheckBox UpdateOutlookSignature;
        private System.Windows.Forms.Label SpreadTheWordLabel;
        private System.Windows.Forms.CheckBox UpdateFirewall;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DataGridViewTextBoxColumn ClientColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn AccountColumn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn ProtectedColumn;
        private System.Windows.Forms.DataGridViewComboBoxColumn CertificateColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn RegistryKeyColumn;
        private System.Windows.Forms.Label ServiceStatusLabel;
    }
}