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
            this.AboutGroupBox = new System.Windows.Forms.GroupBox();
            this.AboutLabel = new System.Windows.Forms.LinkLabel();
            this.AccountGroupBox = new System.Windows.Forms.GroupBox();
            this.SaveSettingsButton = new System.Windows.Forms.Button();
            this.AccountGrid = new System.Windows.Forms.DataGridView();
            this.CertificateGroupBox = new System.Windows.Forms.GroupBox();
            this.CertificateLabel = new System.Windows.Forms.LinkLabel();
            this.ClientColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.AccountColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ProtectedColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.CertificateColumn = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.RegistryKeyColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SmimeSettingsLabel = new System.Windows.Forms.Label();
            this.SmimeSettingsMode = new System.Windows.Forms.ComboBox();
            this.AboutGroupBox.SuspendLayout();
            this.AccountGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.AccountGrid)).BeginInit();
            this.CertificateGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // AboutGroupBox
            // 
            this.AboutGroupBox.Controls.Add(this.AboutLabel);
            this.AboutGroupBox.Location = new System.Drawing.Point(6, 6);
            this.AboutGroupBox.Name = "AboutGroupBox";
            this.AboutGroupBox.Size = new System.Drawing.Size(570, 92);
            this.AboutGroupBox.TabIndex = 0;
            this.AboutGroupBox.TabStop = false;
            this.AboutGroupBox.Text = "About OpaqueMail Proxy";
            // 
            // AboutLabel
            // 
            this.AboutLabel.Location = new System.Drawing.Point(6, 28);
            this.AboutLabel.Name = "AboutLabel";
            this.AboutLabel.Size = new System.Drawing.Size(558, 57);
            this.AboutLabel.TabIndex = 0;
            this.AboutLabel.TabStop = true;
            this.AboutLabel.Text = "OpaqueMail Proxy protects e-mail by automatically encrypting messages using S/MIM" +
    "E.\r\n\r\nInitial settings can be configured below or by editing the following XML f" +
    "ile:\r\n[SETTINGSFILE]";
            // 
            // AccountGroupBox
            // 
            this.AccountGroupBox.Controls.Add(this.SmimeSettingsMode);
            this.AccountGroupBox.Controls.Add(this.SmimeSettingsLabel);
            this.AccountGroupBox.Controls.Add(this.SaveSettingsButton);
            this.AccountGroupBox.Controls.Add(this.AccountGrid);
            this.AccountGroupBox.Location = new System.Drawing.Point(6, 104);
            this.AccountGroupBox.Name = "AccountGroupBox";
            this.AccountGroupBox.Size = new System.Drawing.Size(570, 184);
            this.AccountGroupBox.TabIndex = 1;
            this.AccountGroupBox.TabStop = false;
            this.AccountGroupBox.Text = "Select Accounts to Protect";
            // 
            // SaveSettingsButton
            // 
            this.SaveSettingsButton.Location = new System.Drawing.Point(444, 155);
            this.SaveSettingsButton.Name = "SaveSettingsButton";
            this.SaveSettingsButton.Size = new System.Drawing.Size(120, 24);
            this.SaveSettingsButton.TabIndex = 4;
            this.SaveSettingsButton.Text = "Save Settings";
            this.SaveSettingsButton.UseVisualStyleBackColor = true;
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
            this.AccountGrid.Location = new System.Drawing.Point(6, 20);
            this.AccountGrid.Name = "AccountGrid";
            this.AccountGrid.RowHeadersVisible = false;
            this.AccountGrid.ShowCellErrors = false;
            this.AccountGrid.ShowCellToolTips = false;
            this.AccountGrid.ShowEditingIcon = false;
            this.AccountGrid.ShowRowErrors = false;
            this.AccountGrid.Size = new System.Drawing.Size(558, 129);
            this.AccountGrid.TabIndex = 0;
            // 
            // CertificateGroupBox
            // 
            this.CertificateGroupBox.Controls.Add(this.CertificateLabel);
            this.CertificateGroupBox.Location = new System.Drawing.Point(6, 294);
            this.CertificateGroupBox.Name = "CertificateGroupBox";
            this.CertificateGroupBox.Size = new System.Drawing.Size(570, 154);
            this.CertificateGroupBox.TabIndex = 2;
            this.CertificateGroupBox.TabStop = false;
            this.CertificateGroupBox.Text = "Confirm S/MIME Certificate(s)";
            // 
            // CertificateLabel
            // 
            this.CertificateLabel.Location = new System.Drawing.Point(6, 28);
            this.CertificateLabel.Name = "CertificateLabel";
            this.CertificateLabel.Size = new System.Drawing.Size(558, 123);
            this.CertificateLabel.TabIndex = 0;
            this.CertificateLabel.TabStop = true;
            this.CertificateLabel.Text = resources.GetString("CertificateLabel.Text");
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
            this.CertificateColumn.Width = 243;
            // 
            // RegistryKeyColumn
            // 
            this.RegistryKeyColumn.HeaderText = "Registry Key";
            this.RegistryKeyColumn.Name = "RegistryKeyColumn";
            this.RegistryKeyColumn.Visible = false;
            // 
            // SmimeSettingsLabel
            // 
            this.SmimeSettingsLabel.AutoSize = true;
            this.SmimeSettingsLabel.Location = new System.Drawing.Point(6, 161);
            this.SmimeSettingsLabel.Name = "SmimeSettingsLabel";
            this.SmimeSettingsLabel.Size = new System.Drawing.Size(132, 13);
            this.SmimeSettingsLabel.TabIndex = 5;
            this.SmimeSettingsLabel.Text = "S/MIME protection mode: ";
            // 
            // SmimeSettingsMode
            // 
            this.SmimeSettingsMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.SmimeSettingsMode.FormattingEnabled = true;
            this.SmimeSettingsMode.Items.AddRange(new object[] {
            "Best effort: try to encrypt, but allow unencrypted mail",
            "Encrypted only"});
            this.SmimeSettingsMode.Location = new System.Drawing.Point(144, 157);
            this.SmimeSettingsMode.Name = "SmimeSettingsMode";
            this.SmimeSettingsMode.Size = new System.Drawing.Size(272, 21);
            this.SmimeSettingsMode.TabIndex = 6;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 455);
            this.Controls.Add(this.CertificateGroupBox);
            this.Controls.Add(this.AccountGroupBox);
            this.Controls.Add(this.AboutGroupBox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(600, 494);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(600, 494);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "OpaqueMail Proxy Settings";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.AboutGroupBox.ResumeLayout(false);
            this.AccountGroupBox.ResumeLayout(false);
            this.AccountGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.AccountGrid)).EndInit();
            this.CertificateGroupBox.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox AboutGroupBox;
        private System.Windows.Forms.LinkLabel AboutLabel;
        private System.Windows.Forms.GroupBox AccountGroupBox;
        private System.Windows.Forms.DataGridView AccountGrid;
        private System.Windows.Forms.GroupBox CertificateGroupBox;
        private System.Windows.Forms.LinkLabel CertificateLabel;
        private System.Windows.Forms.Button SaveSettingsButton;
        private System.Windows.Forms.DataGridViewTextBoxColumn ClientColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn AccountColumn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn ProtectedColumn;
        private System.Windows.Forms.DataGridViewComboBoxColumn CertificateColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn RegistryKeyColumn;
        private System.Windows.Forms.Label SmimeSettingsLabel;
        private System.Windows.Forms.ComboBox SmimeSettingsMode;
    }
}