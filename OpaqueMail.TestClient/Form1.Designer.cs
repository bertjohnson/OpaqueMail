/*
 * OpaqueMail (http://opaquemail.org/).
 * 
 * Licensed according to the MIT License (http://mit-license.org/).
 * 
 * Copyright © Bert Johnson (http://bertjohnson.com/) of Apidae Inc. (http://apidae.com/).
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the “Software”), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 * 
 */

namespace OpaqueMail.TestClient
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
            this.TabsControl = new System.Windows.Forms.TabControl();
            this.SettingsTab = new System.Windows.Forms.TabPage();
            this.SmtpSettingsGroup = new System.Windows.Forms.GroupBox();
            this.SmtpCopyImapButton = new System.Windows.Forms.Button();
            this.SmtpCopyPop3Button = new System.Windows.Forms.Button();
            this.SmtpSslLabel = new System.Windows.Forms.Label();
            this.SmtpSsl = new System.Windows.Forms.CheckBox();
            this.SmtpPortLabel = new System.Windows.Forms.Label();
            this.SmtpPort = new System.Windows.Forms.TextBox();
            this.SmtpPasswordLabel = new System.Windows.Forms.Label();
            this.SmtpPassword = new System.Windows.Forms.TextBox();
            this.SmtpUsername = new System.Windows.Forms.TextBox();
            this.SmtpUsernameLabel = new System.Windows.Forms.Label();
            this.SmtpHost = new System.Windows.Forms.TextBox();
            this.SmtpHostLabel = new System.Windows.Forms.Label();
            this.Pop3SettingsGroup = new System.Windows.Forms.GroupBox();
            this.Pop3CopyImapButton = new System.Windows.Forms.Button();
            this.Pop3CopySmtpButton = new System.Windows.Forms.Button();
            this.Pop3SslLabel = new System.Windows.Forms.Label();
            this.Pop3Ssl = new System.Windows.Forms.CheckBox();
            this.Pop3PortLabel = new System.Windows.Forms.Label();
            this.Pop3Port = new System.Windows.Forms.TextBox();
            this.Pop3PasswordLabel = new System.Windows.Forms.Label();
            this.Pop3Password = new System.Windows.Forms.TextBox();
            this.Pop3Username = new System.Windows.Forms.TextBox();
            this.Pop3UsernameLabel = new System.Windows.Forms.Label();
            this.Pop3Host = new System.Windows.Forms.TextBox();
            this.Pop3HostLabel = new System.Windows.Forms.Label();
            this.ImapSettingsGroup = new System.Windows.Forms.GroupBox();
            this.ImapCopyPop3Button = new System.Windows.Forms.Button();
            this.ImapCopySmtpButton = new System.Windows.Forms.Button();
            this.ImapSslLabel = new System.Windows.Forms.Label();
            this.ImapSsl = new System.Windows.Forms.CheckBox();
            this.ImapPortLabel = new System.Windows.Forms.Label();
            this.ImapPort = new System.Windows.Forms.TextBox();
            this.ImapPasswordLabel = new System.Windows.Forms.Label();
            this.ImapPassword = new System.Windows.Forms.TextBox();
            this.ImapUsername = new System.Windows.Forms.TextBox();
            this.ImapUsernameLabel = new System.Windows.Forms.Label();
            this.ImapHost = new System.Windows.Forms.TextBox();
            this.ImapHostLabel = new System.Windows.Forms.Label();
            this.ImapTab = new System.Windows.Forms.TabPage();
            this.ImapPreviewGroup = new System.Windows.Forms.GroupBox();
            this.ImapHeaders = new System.Windows.Forms.TextBox();
            this.ImapWebPreviewPanel = new System.Windows.Forms.Panel();
            this.ImapWebPreview = new System.Windows.Forms.WebBrowser();
            this.ImapTestGroup = new System.Windows.Forms.GroupBox();
            this.ImapSearchText = new System.Windows.Forms.TextBox();
            this.ImapGetQuotaButton = new System.Windows.Forms.Button();
            this.ImapAppendMessageButton = new System.Windows.Forms.Button();
            this.ImapLoadFileButton = new System.Windows.Forms.Button();
            this.ImapRetrieveMessagesButton = new System.Windows.Forms.Button();
            this.ImapMessageGroup = new System.Windows.Forms.GroupBox();
            this.ImapFirst1k = new System.Windows.Forms.CheckBox();
            this.ImapIncludeBody = new System.Windows.Forms.CheckBox();
            this.ImapIncludeHeaders = new System.Windows.Forms.CheckBox();
            this.ImapMailboxList = new System.Windows.Forms.ComboBox();
            this.ImapDeleteMessageButton = new System.Windows.Forms.Button();
            this.ImapMessageList = new System.Windows.Forms.ListBox();
            this.Pop3Tab = new System.Windows.Forms.TabPage();
            this.Pop3PreviewGroup = new System.Windows.Forms.GroupBox();
            this.Pop3Headers = new System.Windows.Forms.TextBox();
            this.Pop3WebPreviewPanel = new System.Windows.Forms.Panel();
            this.Pop3WebPreview = new System.Windows.Forms.WebBrowser();
            this.Pop3TestGroup = new System.Windows.Forms.GroupBox();
            this.Pop3RetrieveMessageButton = new System.Windows.Forms.Button();
            this.Pop3MessageGroup = new System.Windows.Forms.GroupBox();
            this.Pop3DeleteMessageButton = new System.Windows.Forms.Button();
            this.Pop3MessageList = new System.Windows.Forms.ListBox();
            this.SmtpTab = new System.Windows.Forms.TabPage();
            this.SmtpTestGroup = new System.Windows.Forms.GroupBox();
            this.SmtpSendButton = new System.Windows.Forms.Button();
            this.SmtpTestSettingsGroup = new System.Windows.Forms.GroupBox();
            this.SmtpSmimeSerialNumber = new System.Windows.Forms.TextBox();
            this.SmtpIsHtml = new System.Windows.Forms.CheckBox();
            this.SmtpSubjectLabel = new System.Windows.Forms.Label();
            this.SmtpSubject = new System.Windows.Forms.TextBox();
            this.SmtpSmimeTripleWrap = new System.Windows.Forms.CheckBox();
            this.SmtpSmimeEncrypt = new System.Windows.Forms.CheckBox();
            this.SmtpSmimeSign = new System.Windows.Forms.CheckBox();
            this.SmtpSmimeLabel = new System.Windows.Forms.Label();
            this.SmtpAttachmentsLabel = new System.Windows.Forms.Label();
            this.SmtpAttachments = new System.Windows.Forms.TextBox();
            this.SmtpBodyLabel = new System.Windows.Forms.Label();
            this.SmtpBody = new System.Windows.Forms.TextBox();
            this.SmtpBccLabel = new System.Windows.Forms.Label();
            this.SmtpBcc = new System.Windows.Forms.TextBox();
            this.SmtpCCLabel = new System.Windows.Forms.Label();
            this.SmtpCC = new System.Windows.Forms.TextBox();
            this.SmtpToLabel = new System.Windows.Forms.Label();
            this.SmtpTo = new System.Windows.Forms.TextBox();
            this.SmtpFrom = new System.Windows.Forms.TextBox();
            this.SmtpFromLabel = new System.Windows.Forms.Label();
            this.LoadSettingsButton = new System.Windows.Forms.Button();
            this.SaveSettingsButton = new System.Windows.Forms.Button();
            this.TabsControl.SuspendLayout();
            this.SettingsTab.SuspendLayout();
            this.SmtpSettingsGroup.SuspendLayout();
            this.Pop3SettingsGroup.SuspendLayout();
            this.ImapSettingsGroup.SuspendLayout();
            this.ImapTab.SuspendLayout();
            this.ImapPreviewGroup.SuspendLayout();
            this.ImapWebPreviewPanel.SuspendLayout();
            this.ImapTestGroup.SuspendLayout();
            this.ImapMessageGroup.SuspendLayout();
            this.Pop3Tab.SuspendLayout();
            this.Pop3PreviewGroup.SuspendLayout();
            this.Pop3WebPreviewPanel.SuspendLayout();
            this.Pop3TestGroup.SuspendLayout();
            this.Pop3MessageGroup.SuspendLayout();
            this.SmtpTab.SuspendLayout();
            this.SmtpTestGroup.SuspendLayout();
            this.SmtpTestSettingsGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // TabsControl
            // 
            this.TabsControl.Controls.Add(this.SettingsTab);
            this.TabsControl.Controls.Add(this.ImapTab);
            this.TabsControl.Controls.Add(this.Pop3Tab);
            this.TabsControl.Controls.Add(this.SmtpTab);
            this.TabsControl.Location = new System.Drawing.Point(6, 6);
            this.TabsControl.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.TabsControl.Name = "TabsControl";
            this.TabsControl.SelectedIndex = 0;
            this.TabsControl.Size = new System.Drawing.Size(1156, 754);
            this.TabsControl.TabIndex = 0;
            // 
            // SettingsTab
            // 
            this.SettingsTab.Controls.Add(this.SmtpSettingsGroup);
            this.SettingsTab.Controls.Add(this.Pop3SettingsGroup);
            this.SettingsTab.Controls.Add(this.ImapSettingsGroup);
            this.SettingsTab.Location = new System.Drawing.Point(8, 39);
            this.SettingsTab.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.SettingsTab.Name = "SettingsTab";
            this.SettingsTab.Padding = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.SettingsTab.Size = new System.Drawing.Size(1140, 707);
            this.SettingsTab.TabIndex = 0;
            this.SettingsTab.Text = "Settings";
            this.SettingsTab.UseVisualStyleBackColor = true;
            // 
            // SmtpSettingsGroup
            // 
            this.SmtpSettingsGroup.Controls.Add(this.SmtpCopyImapButton);
            this.SmtpSettingsGroup.Controls.Add(this.SmtpCopyPop3Button);
            this.SmtpSettingsGroup.Controls.Add(this.SmtpSslLabel);
            this.SmtpSettingsGroup.Controls.Add(this.SmtpSsl);
            this.SmtpSettingsGroup.Controls.Add(this.SmtpPortLabel);
            this.SmtpSettingsGroup.Controls.Add(this.SmtpPort);
            this.SmtpSettingsGroup.Controls.Add(this.SmtpPasswordLabel);
            this.SmtpSettingsGroup.Controls.Add(this.SmtpPassword);
            this.SmtpSettingsGroup.Controls.Add(this.SmtpUsername);
            this.SmtpSettingsGroup.Controls.Add(this.SmtpUsernameLabel);
            this.SmtpSettingsGroup.Controls.Add(this.SmtpHost);
            this.SmtpSettingsGroup.Controls.Add(this.SmtpHostLabel);
            this.SmtpSettingsGroup.Location = new System.Drawing.Point(12, 448);
            this.SmtpSettingsGroup.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.SmtpSettingsGroup.Name = "SmtpSettingsGroup";
            this.SmtpSettingsGroup.Padding = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.SmtpSettingsGroup.Size = new System.Drawing.Size(1116, 204);
            this.SmtpSettingsGroup.TabIndex = 3;
            this.SmtpSettingsGroup.TabStop = false;
            this.SmtpSettingsGroup.Text = "SMTP Settings";
            // 
            // SmtpCopyImapButton
            // 
            this.SmtpCopyImapButton.Location = new System.Drawing.Point(510, 148);
            this.SmtpCopyImapButton.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.SmtpCopyImapButton.Name = "SmtpCopyImapButton";
            this.SmtpCopyImapButton.Size = new System.Drawing.Size(292, 44);
            this.SmtpCopyImapButton.TabIndex = 6;
            this.SmtpCopyImapButton.Text = "Copy from IMAP Settings";
            this.SmtpCopyImapButton.UseVisualStyleBackColor = true;
            this.SmtpCopyImapButton.Click += new System.EventHandler(this.SmtpCopyImapButton_Click);
            // 
            // SmtpCopyPop3Button
            // 
            this.SmtpCopyPop3Button.Location = new System.Drawing.Point(814, 148);
            this.SmtpCopyPop3Button.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.SmtpCopyPop3Button.Name = "SmtpCopyPop3Button";
            this.SmtpCopyPop3Button.Size = new System.Drawing.Size(292, 44);
            this.SmtpCopyPop3Button.TabIndex = 7;
            this.SmtpCopyPop3Button.Text = "Copy from POP3 Settings";
            this.SmtpCopyPop3Button.UseVisualStyleBackColor = true;
            this.SmtpCopyPop3Button.Click += new System.EventHandler(this.SmtpCopyPop3Button_Click);
            // 
            // SmtpSslLabel
            // 
            this.SmtpSslLabel.AutoSize = true;
            this.SmtpSslLabel.Location = new System.Drawing.Point(12, 148);
            this.SmtpSslLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.SmtpSslLabel.Name = "SmtpSslLabel";
            this.SmtpSslLabel.Size = new System.Drawing.Size(70, 25);
            this.SmtpSslLabel.TabIndex = 9;
            this.SmtpSslLabel.Text = "SSL?:";
            this.SmtpSslLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // SmtpSsl
            // 
            this.SmtpSsl.AutoSize = true;
            this.SmtpSsl.Checked = true;
            this.SmtpSsl.CheckState = System.Windows.Forms.CheckState.Checked;
            this.SmtpSsl.Location = new System.Drawing.Point(94, 148);
            this.SmtpSsl.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.SmtpSsl.Name = "SmtpSsl";
            this.SmtpSsl.Size = new System.Drawing.Size(28, 27);
            this.SmtpSsl.TabIndex = 5;
            this.SmtpSsl.UseVisualStyleBackColor = true;
            // 
            // SmtpPortLabel
            // 
            this.SmtpPortLabel.AutoSize = true;
            this.SmtpPortLabel.Location = new System.Drawing.Point(12, 102);
            this.SmtpPortLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.SmtpPortLabel.Name = "SmtpPortLabel";
            this.SmtpPortLabel.Size = new System.Drawing.Size(63, 25);
            this.SmtpPortLabel.TabIndex = 7;
            this.SmtpPortLabel.Text = "Port: ";
            this.SmtpPortLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // SmtpPort
            // 
            this.SmtpPort.Location = new System.Drawing.Point(94, 96);
            this.SmtpPort.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.SmtpPort.Name = "SmtpPort";
            this.SmtpPort.Size = new System.Drawing.Size(166, 31);
            this.SmtpPort.TabIndex = 3;
            this.SmtpPort.Text = "587";
            // 
            // SmtpPasswordLabel
            // 
            this.SmtpPasswordLabel.AutoSize = true;
            this.SmtpPasswordLabel.Location = new System.Drawing.Point(574, 102);
            this.SmtpPasswordLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.SmtpPasswordLabel.Name = "SmtpPasswordLabel";
            this.SmtpPasswordLabel.Size = new System.Drawing.Size(118, 25);
            this.SmtpPasswordLabel.TabIndex = 5;
            this.SmtpPasswordLabel.Text = "Password: ";
            this.SmtpPasswordLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // SmtpPassword
            // 
            this.SmtpPassword.Location = new System.Drawing.Point(708, 96);
            this.SmtpPassword.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.SmtpPassword.Name = "SmtpPassword";
            this.SmtpPassword.PasswordChar = '*';
            this.SmtpPassword.Size = new System.Drawing.Size(394, 31);
            this.SmtpPassword.TabIndex = 4;
            // 
            // SmtpUsername
            // 
            this.SmtpUsername.Location = new System.Drawing.Point(708, 46);
            this.SmtpUsername.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.SmtpUsername.Name = "SmtpUsername";
            this.SmtpUsername.Size = new System.Drawing.Size(394, 31);
            this.SmtpUsername.TabIndex = 2;
            // 
            // SmtpUsernameLabel
            // 
            this.SmtpUsernameLabel.AutoSize = true;
            this.SmtpUsernameLabel.Location = new System.Drawing.Point(574, 52);
            this.SmtpUsernameLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.SmtpUsernameLabel.Name = "SmtpUsernameLabel";
            this.SmtpUsernameLabel.Size = new System.Drawing.Size(122, 25);
            this.SmtpUsernameLabel.TabIndex = 2;
            this.SmtpUsernameLabel.Text = "Username: ";
            this.SmtpUsernameLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // SmtpHost
            // 
            this.SmtpHost.Location = new System.Drawing.Point(94, 46);
            this.SmtpHost.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.SmtpHost.Name = "SmtpHost";
            this.SmtpHost.Size = new System.Drawing.Size(394, 31);
            this.SmtpHost.TabIndex = 1;
            this.SmtpHost.Text = "smtp.gmail.com";
            // 
            // SmtpHostLabel
            // 
            this.SmtpHostLabel.AutoSize = true;
            this.SmtpHostLabel.Location = new System.Drawing.Point(12, 52);
            this.SmtpHostLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.SmtpHostLabel.Name = "SmtpHostLabel";
            this.SmtpHostLabel.Size = new System.Drawing.Size(68, 25);
            this.SmtpHostLabel.TabIndex = 0;
            this.SmtpHostLabel.Text = "Host: ";
            this.SmtpHostLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // Pop3SettingsGroup
            // 
            this.Pop3SettingsGroup.Controls.Add(this.Pop3CopyImapButton);
            this.Pop3SettingsGroup.Controls.Add(this.Pop3CopySmtpButton);
            this.Pop3SettingsGroup.Controls.Add(this.Pop3SslLabel);
            this.Pop3SettingsGroup.Controls.Add(this.Pop3Ssl);
            this.Pop3SettingsGroup.Controls.Add(this.Pop3PortLabel);
            this.Pop3SettingsGroup.Controls.Add(this.Pop3Port);
            this.Pop3SettingsGroup.Controls.Add(this.Pop3PasswordLabel);
            this.Pop3SettingsGroup.Controls.Add(this.Pop3Password);
            this.Pop3SettingsGroup.Controls.Add(this.Pop3Username);
            this.Pop3SettingsGroup.Controls.Add(this.Pop3UsernameLabel);
            this.Pop3SettingsGroup.Controls.Add(this.Pop3Host);
            this.Pop3SettingsGroup.Controls.Add(this.Pop3HostLabel);
            this.Pop3SettingsGroup.Location = new System.Drawing.Point(12, 227);
            this.Pop3SettingsGroup.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.Pop3SettingsGroup.Name = "Pop3SettingsGroup";
            this.Pop3SettingsGroup.Padding = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.Pop3SettingsGroup.Size = new System.Drawing.Size(1116, 204);
            this.Pop3SettingsGroup.TabIndex = 2;
            this.Pop3SettingsGroup.TabStop = false;
            this.Pop3SettingsGroup.Text = "POP3 Settings";
            // 
            // Pop3CopyImapButton
            // 
            this.Pop3CopyImapButton.Location = new System.Drawing.Point(510, 148);
            this.Pop3CopyImapButton.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.Pop3CopyImapButton.Name = "Pop3CopyImapButton";
            this.Pop3CopyImapButton.Size = new System.Drawing.Size(292, 44);
            this.Pop3CopyImapButton.TabIndex = 6;
            this.Pop3CopyImapButton.Text = "Copy from IMAP Settings";
            this.Pop3CopyImapButton.UseVisualStyleBackColor = true;
            this.Pop3CopyImapButton.Click += new System.EventHandler(this.Pop3CopyImapButton_Click);
            // 
            // Pop3CopySmtpButton
            // 
            this.Pop3CopySmtpButton.Location = new System.Drawing.Point(814, 148);
            this.Pop3CopySmtpButton.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.Pop3CopySmtpButton.Name = "Pop3CopySmtpButton";
            this.Pop3CopySmtpButton.Size = new System.Drawing.Size(292, 44);
            this.Pop3CopySmtpButton.TabIndex = 7;
            this.Pop3CopySmtpButton.Text = "Copy from SMTP Settings";
            this.Pop3CopySmtpButton.UseVisualStyleBackColor = true;
            this.Pop3CopySmtpButton.Click += new System.EventHandler(this.Pop3CopySmtpButton_Click);
            // 
            // Pop3SslLabel
            // 
            this.Pop3SslLabel.AutoSize = true;
            this.Pop3SslLabel.Location = new System.Drawing.Point(12, 148);
            this.Pop3SslLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.Pop3SslLabel.Name = "Pop3SslLabel";
            this.Pop3SslLabel.Size = new System.Drawing.Size(70, 25);
            this.Pop3SslLabel.TabIndex = 9;
            this.Pop3SslLabel.Text = "SSL?:";
            this.Pop3SslLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // Pop3Ssl
            // 
            this.Pop3Ssl.AutoSize = true;
            this.Pop3Ssl.Checked = true;
            this.Pop3Ssl.CheckState = System.Windows.Forms.CheckState.Checked;
            this.Pop3Ssl.Location = new System.Drawing.Point(94, 148);
            this.Pop3Ssl.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.Pop3Ssl.Name = "Pop3Ssl";
            this.Pop3Ssl.Size = new System.Drawing.Size(28, 27);
            this.Pop3Ssl.TabIndex = 5;
            this.Pop3Ssl.UseVisualStyleBackColor = true;
            // 
            // Pop3PortLabel
            // 
            this.Pop3PortLabel.AutoSize = true;
            this.Pop3PortLabel.Location = new System.Drawing.Point(12, 102);
            this.Pop3PortLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.Pop3PortLabel.Name = "Pop3PortLabel";
            this.Pop3PortLabel.Size = new System.Drawing.Size(63, 25);
            this.Pop3PortLabel.TabIndex = 7;
            this.Pop3PortLabel.Text = "Port: ";
            this.Pop3PortLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // Pop3Port
            // 
            this.Pop3Port.Location = new System.Drawing.Point(94, 96);
            this.Pop3Port.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.Pop3Port.Name = "Pop3Port";
            this.Pop3Port.Size = new System.Drawing.Size(166, 31);
            this.Pop3Port.TabIndex = 3;
            this.Pop3Port.Text = "995";
            // 
            // Pop3PasswordLabel
            // 
            this.Pop3PasswordLabel.AutoSize = true;
            this.Pop3PasswordLabel.Location = new System.Drawing.Point(574, 102);
            this.Pop3PasswordLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.Pop3PasswordLabel.Name = "Pop3PasswordLabel";
            this.Pop3PasswordLabel.Size = new System.Drawing.Size(118, 25);
            this.Pop3PasswordLabel.TabIndex = 5;
            this.Pop3PasswordLabel.Text = "Password: ";
            this.Pop3PasswordLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // Pop3Password
            // 
            this.Pop3Password.Location = new System.Drawing.Point(708, 96);
            this.Pop3Password.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.Pop3Password.Name = "Pop3Password";
            this.Pop3Password.PasswordChar = '*';
            this.Pop3Password.Size = new System.Drawing.Size(394, 31);
            this.Pop3Password.TabIndex = 4;
            // 
            // Pop3Username
            // 
            this.Pop3Username.Location = new System.Drawing.Point(708, 46);
            this.Pop3Username.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.Pop3Username.Name = "Pop3Username";
            this.Pop3Username.Size = new System.Drawing.Size(394, 31);
            this.Pop3Username.TabIndex = 2;
            // 
            // Pop3UsernameLabel
            // 
            this.Pop3UsernameLabel.AutoSize = true;
            this.Pop3UsernameLabel.Location = new System.Drawing.Point(574, 52);
            this.Pop3UsernameLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.Pop3UsernameLabel.Name = "Pop3UsernameLabel";
            this.Pop3UsernameLabel.Size = new System.Drawing.Size(122, 25);
            this.Pop3UsernameLabel.TabIndex = 2;
            this.Pop3UsernameLabel.Text = "Username: ";
            this.Pop3UsernameLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // Pop3Host
            // 
            this.Pop3Host.Location = new System.Drawing.Point(94, 46);
            this.Pop3Host.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.Pop3Host.Name = "Pop3Host";
            this.Pop3Host.Size = new System.Drawing.Size(394, 31);
            this.Pop3Host.TabIndex = 1;
            this.Pop3Host.Text = "pop.gmail.com";
            // 
            // Pop3HostLabel
            // 
            this.Pop3HostLabel.AutoSize = true;
            this.Pop3HostLabel.Location = new System.Drawing.Point(12, 52);
            this.Pop3HostLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.Pop3HostLabel.Name = "Pop3HostLabel";
            this.Pop3HostLabel.Size = new System.Drawing.Size(68, 25);
            this.Pop3HostLabel.TabIndex = 0;
            this.Pop3HostLabel.Text = "Host: ";
            this.Pop3HostLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // ImapSettingsGroup
            // 
            this.ImapSettingsGroup.Controls.Add(this.ImapCopyPop3Button);
            this.ImapSettingsGroup.Controls.Add(this.ImapCopySmtpButton);
            this.ImapSettingsGroup.Controls.Add(this.ImapSslLabel);
            this.ImapSettingsGroup.Controls.Add(this.ImapSsl);
            this.ImapSettingsGroup.Controls.Add(this.ImapPortLabel);
            this.ImapSettingsGroup.Controls.Add(this.ImapPort);
            this.ImapSettingsGroup.Controls.Add(this.ImapPasswordLabel);
            this.ImapSettingsGroup.Controls.Add(this.ImapPassword);
            this.ImapSettingsGroup.Controls.Add(this.ImapUsername);
            this.ImapSettingsGroup.Controls.Add(this.ImapUsernameLabel);
            this.ImapSettingsGroup.Controls.Add(this.ImapHost);
            this.ImapSettingsGroup.Controls.Add(this.ImapHostLabel);
            this.ImapSettingsGroup.Location = new System.Drawing.Point(12, 12);
            this.ImapSettingsGroup.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.ImapSettingsGroup.Name = "ImapSettingsGroup";
            this.ImapSettingsGroup.Padding = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.ImapSettingsGroup.Size = new System.Drawing.Size(1116, 204);
            this.ImapSettingsGroup.TabIndex = 1;
            this.ImapSettingsGroup.TabStop = false;
            this.ImapSettingsGroup.Text = "IMAP Settings";
            // 
            // ImapCopyPop3Button
            // 
            this.ImapCopyPop3Button.Location = new System.Drawing.Point(510, 148);
            this.ImapCopyPop3Button.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.ImapCopyPop3Button.Name = "ImapCopyPop3Button";
            this.ImapCopyPop3Button.Size = new System.Drawing.Size(292, 44);
            this.ImapCopyPop3Button.TabIndex = 6;
            this.ImapCopyPop3Button.Text = "Copy from POP3 Settings";
            this.ImapCopyPop3Button.UseVisualStyleBackColor = true;
            this.ImapCopyPop3Button.Click += new System.EventHandler(this.ImapCopyPop3Button_Click);
            // 
            // ImapCopySmtpButton
            // 
            this.ImapCopySmtpButton.Location = new System.Drawing.Point(814, 148);
            this.ImapCopySmtpButton.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.ImapCopySmtpButton.Name = "ImapCopySmtpButton";
            this.ImapCopySmtpButton.Size = new System.Drawing.Size(292, 44);
            this.ImapCopySmtpButton.TabIndex = 7;
            this.ImapCopySmtpButton.Text = "Copy from SMTP Settings";
            this.ImapCopySmtpButton.UseVisualStyleBackColor = true;
            this.ImapCopySmtpButton.Click += new System.EventHandler(this.ImapCopySmtpButton_Click);
            // 
            // ImapSslLabel
            // 
            this.ImapSslLabel.AutoSize = true;
            this.ImapSslLabel.Location = new System.Drawing.Point(12, 148);
            this.ImapSslLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.ImapSslLabel.Name = "ImapSslLabel";
            this.ImapSslLabel.Size = new System.Drawing.Size(70, 25);
            this.ImapSslLabel.TabIndex = 9;
            this.ImapSslLabel.Text = "SSL?:";
            this.ImapSslLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // ImapSsl
            // 
            this.ImapSsl.AutoSize = true;
            this.ImapSsl.Checked = true;
            this.ImapSsl.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ImapSsl.Location = new System.Drawing.Point(94, 148);
            this.ImapSsl.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.ImapSsl.Name = "ImapSsl";
            this.ImapSsl.Size = new System.Drawing.Size(28, 27);
            this.ImapSsl.TabIndex = 5;
            this.ImapSsl.UseVisualStyleBackColor = true;
            // 
            // ImapPortLabel
            // 
            this.ImapPortLabel.AutoSize = true;
            this.ImapPortLabel.Location = new System.Drawing.Point(12, 102);
            this.ImapPortLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.ImapPortLabel.Name = "ImapPortLabel";
            this.ImapPortLabel.Size = new System.Drawing.Size(63, 25);
            this.ImapPortLabel.TabIndex = 7;
            this.ImapPortLabel.Text = "Port: ";
            this.ImapPortLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // ImapPort
            // 
            this.ImapPort.Location = new System.Drawing.Point(94, 96);
            this.ImapPort.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.ImapPort.Name = "ImapPort";
            this.ImapPort.Size = new System.Drawing.Size(166, 31);
            this.ImapPort.TabIndex = 3;
            this.ImapPort.Text = "993";
            // 
            // ImapPasswordLabel
            // 
            this.ImapPasswordLabel.AutoSize = true;
            this.ImapPasswordLabel.Location = new System.Drawing.Point(574, 102);
            this.ImapPasswordLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.ImapPasswordLabel.Name = "ImapPasswordLabel";
            this.ImapPasswordLabel.Size = new System.Drawing.Size(118, 25);
            this.ImapPasswordLabel.TabIndex = 5;
            this.ImapPasswordLabel.Text = "Password: ";
            this.ImapPasswordLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // ImapPassword
            // 
            this.ImapPassword.Location = new System.Drawing.Point(708, 96);
            this.ImapPassword.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.ImapPassword.Name = "ImapPassword";
            this.ImapPassword.PasswordChar = '*';
            this.ImapPassword.Size = new System.Drawing.Size(394, 31);
            this.ImapPassword.TabIndex = 4;
            // 
            // ImapUsername
            // 
            this.ImapUsername.Location = new System.Drawing.Point(708, 46);
            this.ImapUsername.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.ImapUsername.Name = "ImapUsername";
            this.ImapUsername.Size = new System.Drawing.Size(394, 31);
            this.ImapUsername.TabIndex = 2;
            // 
            // ImapUsernameLabel
            // 
            this.ImapUsernameLabel.AutoSize = true;
            this.ImapUsernameLabel.Location = new System.Drawing.Point(574, 52);
            this.ImapUsernameLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.ImapUsernameLabel.Name = "ImapUsernameLabel";
            this.ImapUsernameLabel.Size = new System.Drawing.Size(122, 25);
            this.ImapUsernameLabel.TabIndex = 2;
            this.ImapUsernameLabel.Text = "Username: ";
            this.ImapUsernameLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // ImapHost
            // 
            this.ImapHost.Location = new System.Drawing.Point(94, 46);
            this.ImapHost.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.ImapHost.Name = "ImapHost";
            this.ImapHost.Size = new System.Drawing.Size(394, 31);
            this.ImapHost.TabIndex = 1;
            this.ImapHost.Text = "imap.gmail.com";
            // 
            // ImapHostLabel
            // 
            this.ImapHostLabel.AutoSize = true;
            this.ImapHostLabel.Location = new System.Drawing.Point(12, 52);
            this.ImapHostLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.ImapHostLabel.Name = "ImapHostLabel";
            this.ImapHostLabel.Size = new System.Drawing.Size(68, 25);
            this.ImapHostLabel.TabIndex = 0;
            this.ImapHostLabel.Text = "Host: ";
            this.ImapHostLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // ImapTab
            // 
            this.ImapTab.Controls.Add(this.ImapPreviewGroup);
            this.ImapTab.Controls.Add(this.ImapTestGroup);
            this.ImapTab.Controls.Add(this.ImapMessageGroup);
            this.ImapTab.Location = new System.Drawing.Point(8, 39);
            this.ImapTab.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.ImapTab.Name = "ImapTab";
            this.ImapTab.Padding = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.ImapTab.Size = new System.Drawing.Size(1140, 707);
            this.ImapTab.TabIndex = 1;
            this.ImapTab.Text = "IMAP";
            this.ImapTab.UseVisualStyleBackColor = true;
            // 
            // ImapPreviewGroup
            // 
            this.ImapPreviewGroup.Controls.Add(this.ImapHeaders);
            this.ImapPreviewGroup.Controls.Add(this.ImapWebPreviewPanel);
            this.ImapPreviewGroup.Location = new System.Drawing.Point(392, 108);
            this.ImapPreviewGroup.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.ImapPreviewGroup.Name = "ImapPreviewGroup";
            this.ImapPreviewGroup.Padding = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.ImapPreviewGroup.Size = new System.Drawing.Size(726, 585);
            this.ImapPreviewGroup.TabIndex = 4;
            this.ImapPreviewGroup.TabStop = false;
            this.ImapPreviewGroup.Text = "IMAP Preview";
            // 
            // ImapHeaders
            // 
            this.ImapHeaders.Location = new System.Drawing.Point(12, 37);
            this.ImapHeaders.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.ImapHeaders.Multiline = true;
            this.ImapHeaders.Name = "ImapHeaders";
            this.ImapHeaders.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.ImapHeaders.Size = new System.Drawing.Size(694, 266);
            this.ImapHeaders.TabIndex = 1;
            // 
            // ImapWebPreviewPanel
            // 
            this.ImapWebPreviewPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ImapWebPreviewPanel.Controls.Add(this.ImapWebPreview);
            this.ImapWebPreviewPanel.Location = new System.Drawing.Point(12, 317);
            this.ImapWebPreviewPanel.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.ImapWebPreviewPanel.Name = "ImapWebPreviewPanel";
            this.ImapWebPreviewPanel.Size = new System.Drawing.Size(696, 250);
            this.ImapWebPreviewPanel.TabIndex = 0;
            // 
            // ImapWebPreview
            // 
            this.ImapWebPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ImapWebPreview.Location = new System.Drawing.Point(0, 0);
            this.ImapWebPreview.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.ImapWebPreview.MinimumSize = new System.Drawing.Size(40, 38);
            this.ImapWebPreview.Name = "ImapWebPreview";
            this.ImapWebPreview.Size = new System.Drawing.Size(694, 248);
            this.ImapWebPreview.TabIndex = 0;
            // 
            // ImapTestGroup
            // 
            this.ImapTestGroup.Controls.Add(this.ImapSearchText);
            this.ImapTestGroup.Controls.Add(this.ImapGetQuotaButton);
            this.ImapTestGroup.Controls.Add(this.ImapAppendMessageButton);
            this.ImapTestGroup.Controls.Add(this.ImapLoadFileButton);
            this.ImapTestGroup.Controls.Add(this.ImapRetrieveMessagesButton);
            this.ImapTestGroup.Location = new System.Drawing.Point(12, 12);
            this.ImapTestGroup.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.ImapTestGroup.Name = "ImapTestGroup";
            this.ImapTestGroup.Padding = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.ImapTestGroup.Size = new System.Drawing.Size(1106, 85);
            this.ImapTestGroup.TabIndex = 3;
            this.ImapTestGroup.TabStop = false;
            this.ImapTestGroup.Text = "IMAP Tests";
            // 
            // ImapSearchText
            // 
            this.ImapSearchText.Location = new System.Drawing.Point(866, 35);
            this.ImapSearchText.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.ImapSearchText.Name = "ImapSearchText";
            this.ImapSearchText.Size = new System.Drawing.Size(218, 31);
            this.ImapSearchText.TabIndex = 5;
            this.ImapSearchText.Text = "Search...";
            // 
            // ImapGetQuotaButton
            // 
            this.ImapGetQuotaButton.Location = new System.Drawing.Point(654, 33);
            this.ImapGetQuotaButton.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.ImapGetQuotaButton.Name = "ImapGetQuotaButton";
            this.ImapGetQuotaButton.Size = new System.Drawing.Size(200, 44);
            this.ImapGetQuotaButton.TabIndex = 4;
            this.ImapGetQuotaButton.Text = "Quota/Mailboxes";
            this.ImapGetQuotaButton.UseVisualStyleBackColor = true;
            this.ImapGetQuotaButton.Click += new System.EventHandler(this.ImapGetQuotaButton_Click);
            // 
            // ImapAppendMessageButton
            // 
            this.ImapAppendMessageButton.Location = new System.Drawing.Point(442, 33);
            this.ImapAppendMessageButton.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.ImapAppendMessageButton.Name = "ImapAppendMessageButton";
            this.ImapAppendMessageButton.Size = new System.Drawing.Size(200, 44);
            this.ImapAppendMessageButton.TabIndex = 3;
            this.ImapAppendMessageButton.Text = "Append Msg";
            this.ImapAppendMessageButton.UseVisualStyleBackColor = true;
            this.ImapAppendMessageButton.Click += new System.EventHandler(this.ImapAppendMessageButton_Click);
            // 
            // ImapLoadFileButton
            // 
            this.ImapLoadFileButton.Location = new System.Drawing.Point(230, 33);
            this.ImapLoadFileButton.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.ImapLoadFileButton.Name = "ImapLoadFileButton";
            this.ImapLoadFileButton.Size = new System.Drawing.Size(200, 44);
            this.ImapLoadFileButton.TabIndex = 2;
            this.ImapLoadFileButton.Text = "Load .EML File";
            this.ImapLoadFileButton.UseVisualStyleBackColor = true;
            this.ImapLoadFileButton.Click += new System.EventHandler(this.ImapLoadFileButton_Click);
            // 
            // ImapRetrieveMessagesButton
            // 
            this.ImapRetrieveMessagesButton.Location = new System.Drawing.Point(18, 33);
            this.ImapRetrieveMessagesButton.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.ImapRetrieveMessagesButton.Name = "ImapRetrieveMessagesButton";
            this.ImapRetrieveMessagesButton.Size = new System.Drawing.Size(200, 44);
            this.ImapRetrieveMessagesButton.TabIndex = 1;
            this.ImapRetrieveMessagesButton.Text = "Retrieve Msgs";
            this.ImapRetrieveMessagesButton.UseVisualStyleBackColor = true;
            this.ImapRetrieveMessagesButton.Click += new System.EventHandler(this.ImapRetrieveMessagesButton_Click);
            // 
            // ImapMessageGroup
            // 
            this.ImapMessageGroup.Controls.Add(this.ImapFirst1k);
            this.ImapMessageGroup.Controls.Add(this.ImapIncludeBody);
            this.ImapMessageGroup.Controls.Add(this.ImapIncludeHeaders);
            this.ImapMessageGroup.Controls.Add(this.ImapMailboxList);
            this.ImapMessageGroup.Controls.Add(this.ImapDeleteMessageButton);
            this.ImapMessageGroup.Controls.Add(this.ImapMessageList);
            this.ImapMessageGroup.Location = new System.Drawing.Point(12, 108);
            this.ImapMessageGroup.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.ImapMessageGroup.Name = "ImapMessageGroup";
            this.ImapMessageGroup.Padding = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.ImapMessageGroup.Size = new System.Drawing.Size(368, 585);
            this.ImapMessageGroup.TabIndex = 0;
            this.ImapMessageGroup.TabStop = false;
            this.ImapMessageGroup.Text = "IMAP Messages";
            // 
            // ImapFirst1k
            // 
            this.ImapFirst1k.AutoSize = true;
            this.ImapFirst1k.Location = new System.Drawing.Point(16, 546);
            this.ImapFirst1k.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.ImapFirst1k.Name = "ImapFirst1k";
            this.ImapFirst1k.Size = new System.Drawing.Size(250, 29);
            this.ImapFirst1k.TabIndex = 6;
            this.ImapFirst1k.Text = "First 1000 Bytes Only";
            this.ImapFirst1k.UseVisualStyleBackColor = true;
            // 
            // ImapIncludeBody
            // 
            this.ImapIncludeBody.AutoSize = true;
            this.ImapIncludeBody.Checked = true;
            this.ImapIncludeBody.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ImapIncludeBody.Location = new System.Drawing.Point(16, 502);
            this.ImapIncludeBody.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.ImapIncludeBody.Name = "ImapIncludeBody";
            this.ImapIncludeBody.Size = new System.Drawing.Size(168, 29);
            this.ImapIncludeBody.TabIndex = 5;
            this.ImapIncludeBody.Text = "Include Body";
            this.ImapIncludeBody.UseVisualStyleBackColor = true;
            // 
            // ImapIncludeHeaders
            // 
            this.ImapIncludeHeaders.AutoSize = true;
            this.ImapIncludeHeaders.Checked = true;
            this.ImapIncludeHeaders.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ImapIncludeHeaders.Location = new System.Drawing.Point(16, 458);
            this.ImapIncludeHeaders.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.ImapIncludeHeaders.Name = "ImapIncludeHeaders";
            this.ImapIncludeHeaders.Size = new System.Drawing.Size(200, 29);
            this.ImapIncludeHeaders.TabIndex = 4;
            this.ImapIncludeHeaders.Text = "Include Headers";
            this.ImapIncludeHeaders.UseVisualStyleBackColor = true;
            // 
            // ImapMailboxList
            // 
            this.ImapMailboxList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ImapMailboxList.FormattingEnabled = true;
            this.ImapMailboxList.Location = new System.Drawing.Point(12, 37);
            this.ImapMailboxList.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.ImapMailboxList.Name = "ImapMailboxList";
            this.ImapMailboxList.Size = new System.Drawing.Size(340, 33);
            this.ImapMailboxList.TabIndex = 1;
            this.ImapMailboxList.SelectedIndexChanged += new System.EventHandler(this.ImapMailboxList_SelectedIndexChanged);
            // 
            // ImapDeleteMessageButton
            // 
            this.ImapDeleteMessageButton.Enabled = false;
            this.ImapDeleteMessageButton.Location = new System.Drawing.Point(12, 406);
            this.ImapDeleteMessageButton.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.ImapDeleteMessageButton.Name = "ImapDeleteMessageButton";
            this.ImapDeleteMessageButton.Size = new System.Drawing.Size(344, 44);
            this.ImapDeleteMessageButton.TabIndex = 3;
            this.ImapDeleteMessageButton.Text = "Delete Message";
            this.ImapDeleteMessageButton.UseVisualStyleBackColor = true;
            this.ImapDeleteMessageButton.Click += new System.EventHandler(this.ImapDeleteMessageButton_Click);
            // 
            // ImapMessageList
            // 
            this.ImapMessageList.FormattingEnabled = true;
            this.ImapMessageList.ItemHeight = 25;
            this.ImapMessageList.Location = new System.Drawing.Point(12, 87);
            this.ImapMessageList.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.ImapMessageList.Name = "ImapMessageList";
            this.ImapMessageList.Size = new System.Drawing.Size(340, 304);
            this.ImapMessageList.TabIndex = 2;
            this.ImapMessageList.SelectedIndexChanged += new System.EventHandler(this.ImapMessageList_SelectedIndexChanged);
            // 
            // Pop3Tab
            // 
            this.Pop3Tab.Controls.Add(this.Pop3PreviewGroup);
            this.Pop3Tab.Controls.Add(this.Pop3TestGroup);
            this.Pop3Tab.Controls.Add(this.Pop3MessageGroup);
            this.Pop3Tab.Location = new System.Drawing.Point(8, 39);
            this.Pop3Tab.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.Pop3Tab.Name = "Pop3Tab";
            this.Pop3Tab.Size = new System.Drawing.Size(1140, 707);
            this.Pop3Tab.TabIndex = 2;
            this.Pop3Tab.Text = "POP3";
            this.Pop3Tab.UseVisualStyleBackColor = true;
            // 
            // Pop3PreviewGroup
            // 
            this.Pop3PreviewGroup.Controls.Add(this.Pop3Headers);
            this.Pop3PreviewGroup.Controls.Add(this.Pop3WebPreviewPanel);
            this.Pop3PreviewGroup.Location = new System.Drawing.Point(392, 108);
            this.Pop3PreviewGroup.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.Pop3PreviewGroup.Name = "Pop3PreviewGroup";
            this.Pop3PreviewGroup.Padding = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.Pop3PreviewGroup.Size = new System.Drawing.Size(726, 585);
            this.Pop3PreviewGroup.TabIndex = 7;
            this.Pop3PreviewGroup.TabStop = false;
            this.Pop3PreviewGroup.Text = "POP3 Preview";
            // 
            // Pop3Headers
            // 
            this.Pop3Headers.Location = new System.Drawing.Point(12, 37);
            this.Pop3Headers.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.Pop3Headers.Multiline = true;
            this.Pop3Headers.Name = "Pop3Headers";
            this.Pop3Headers.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.Pop3Headers.Size = new System.Drawing.Size(694, 266);
            this.Pop3Headers.TabIndex = 1;
            // 
            // Pop3WebPreviewPanel
            // 
            this.Pop3WebPreviewPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Pop3WebPreviewPanel.Controls.Add(this.Pop3WebPreview);
            this.Pop3WebPreviewPanel.Location = new System.Drawing.Point(12, 317);
            this.Pop3WebPreviewPanel.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.Pop3WebPreviewPanel.Name = "Pop3WebPreviewPanel";
            this.Pop3WebPreviewPanel.Size = new System.Drawing.Size(696, 250);
            this.Pop3WebPreviewPanel.TabIndex = 0;
            // 
            // Pop3WebPreview
            // 
            this.Pop3WebPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Pop3WebPreview.Location = new System.Drawing.Point(0, 0);
            this.Pop3WebPreview.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.Pop3WebPreview.MinimumSize = new System.Drawing.Size(40, 38);
            this.Pop3WebPreview.Name = "Pop3WebPreview";
            this.Pop3WebPreview.Size = new System.Drawing.Size(694, 248);
            this.Pop3WebPreview.TabIndex = 0;
            // 
            // Pop3TestGroup
            // 
            this.Pop3TestGroup.Controls.Add(this.Pop3RetrieveMessageButton);
            this.Pop3TestGroup.Location = new System.Drawing.Point(12, 12);
            this.Pop3TestGroup.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.Pop3TestGroup.Name = "Pop3TestGroup";
            this.Pop3TestGroup.Padding = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.Pop3TestGroup.Size = new System.Drawing.Size(1106, 85);
            this.Pop3TestGroup.TabIndex = 6;
            this.Pop3TestGroup.TabStop = false;
            this.Pop3TestGroup.Text = "POP3 Tests";
            // 
            // Pop3RetrieveMessageButton
            // 
            this.Pop3RetrieveMessageButton.Location = new System.Drawing.Point(18, 33);
            this.Pop3RetrieveMessageButton.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.Pop3RetrieveMessageButton.Name = "Pop3RetrieveMessageButton";
            this.Pop3RetrieveMessageButton.Size = new System.Drawing.Size(276, 44);
            this.Pop3RetrieveMessageButton.TabIndex = 1;
            this.Pop3RetrieveMessageButton.Text = "Retrieve Messages";
            this.Pop3RetrieveMessageButton.UseVisualStyleBackColor = true;
            this.Pop3RetrieveMessageButton.Click += new System.EventHandler(this.Pop3RetrieveMessageButton_Click);
            // 
            // Pop3MessageGroup
            // 
            this.Pop3MessageGroup.Controls.Add(this.Pop3DeleteMessageButton);
            this.Pop3MessageGroup.Controls.Add(this.Pop3MessageList);
            this.Pop3MessageGroup.Location = new System.Drawing.Point(12, 108);
            this.Pop3MessageGroup.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.Pop3MessageGroup.Name = "Pop3MessageGroup";
            this.Pop3MessageGroup.Padding = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.Pop3MessageGroup.Size = new System.Drawing.Size(368, 585);
            this.Pop3MessageGroup.TabIndex = 5;
            this.Pop3MessageGroup.TabStop = false;
            this.Pop3MessageGroup.Text = "POP3 Messages";
            // 
            // Pop3DeleteMessageButton
            // 
            this.Pop3DeleteMessageButton.Enabled = false;
            this.Pop3DeleteMessageButton.Location = new System.Drawing.Point(12, 523);
            this.Pop3DeleteMessageButton.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.Pop3DeleteMessageButton.Name = "Pop3DeleteMessageButton";
            this.Pop3DeleteMessageButton.Size = new System.Drawing.Size(344, 44);
            this.Pop3DeleteMessageButton.TabIndex = 4;
            this.Pop3DeleteMessageButton.Text = "Delete Message";
            this.Pop3DeleteMessageButton.UseVisualStyleBackColor = true;
            this.Pop3DeleteMessageButton.Click += new System.EventHandler(this.Pop3DeleteMessageButton_Click);
            // 
            // Pop3MessageList
            // 
            this.Pop3MessageList.FormattingEnabled = true;
            this.Pop3MessageList.ItemHeight = 25;
            this.Pop3MessageList.Location = new System.Drawing.Point(12, 37);
            this.Pop3MessageList.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.Pop3MessageList.Name = "Pop3MessageList";
            this.Pop3MessageList.Size = new System.Drawing.Size(340, 479);
            this.Pop3MessageList.TabIndex = 1;
            this.Pop3MessageList.SelectedIndexChanged += new System.EventHandler(this.Pop3MessageList_SelectedIndexChanged);
            // 
            // SmtpTab
            // 
            this.SmtpTab.Controls.Add(this.SmtpTestGroup);
            this.SmtpTab.Controls.Add(this.SmtpTestSettingsGroup);
            this.SmtpTab.Location = new System.Drawing.Point(8, 39);
            this.SmtpTab.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.SmtpTab.Name = "SmtpTab";
            this.SmtpTab.Size = new System.Drawing.Size(1140, 707);
            this.SmtpTab.TabIndex = 3;
            this.SmtpTab.Text = "SMTP";
            this.SmtpTab.UseVisualStyleBackColor = true;
            // 
            // SmtpTestGroup
            // 
            this.SmtpTestGroup.Controls.Add(this.SmtpSendButton);
            this.SmtpTestGroup.Location = new System.Drawing.Point(12, 12);
            this.SmtpTestGroup.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.SmtpTestGroup.Name = "SmtpTestGroup";
            this.SmtpTestGroup.Padding = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.SmtpTestGroup.Size = new System.Drawing.Size(1118, 85);
            this.SmtpTestGroup.TabIndex = 2;
            this.SmtpTestGroup.TabStop = false;
            this.SmtpTestGroup.Text = "SMTP Tests";
            // 
            // SmtpSendButton
            // 
            this.SmtpSendButton.Location = new System.Drawing.Point(18, 33);
            this.SmtpSendButton.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.SmtpSendButton.Name = "SmtpSendButton";
            this.SmtpSendButton.Size = new System.Drawing.Size(276, 44);
            this.SmtpSendButton.TabIndex = 2;
            this.SmtpSendButton.Text = "Send Message";
            this.SmtpSendButton.UseVisualStyleBackColor = true;
            this.SmtpSendButton.Click += new System.EventHandler(this.SmtpSendButton_Click);
            // 
            // SmtpTestSettingsGroup
            // 
            this.SmtpTestSettingsGroup.Controls.Add(this.SmtpSmimeSerialNumber);
            this.SmtpTestSettingsGroup.Controls.Add(this.SmtpIsHtml);
            this.SmtpTestSettingsGroup.Controls.Add(this.SmtpSubjectLabel);
            this.SmtpTestSettingsGroup.Controls.Add(this.SmtpSubject);
            this.SmtpTestSettingsGroup.Controls.Add(this.SmtpSmimeTripleWrap);
            this.SmtpTestSettingsGroup.Controls.Add(this.SmtpSmimeEncrypt);
            this.SmtpTestSettingsGroup.Controls.Add(this.SmtpSmimeSign);
            this.SmtpTestSettingsGroup.Controls.Add(this.SmtpSmimeLabel);
            this.SmtpTestSettingsGroup.Controls.Add(this.SmtpAttachmentsLabel);
            this.SmtpTestSettingsGroup.Controls.Add(this.SmtpAttachments);
            this.SmtpTestSettingsGroup.Controls.Add(this.SmtpBodyLabel);
            this.SmtpTestSettingsGroup.Controls.Add(this.SmtpBody);
            this.SmtpTestSettingsGroup.Controls.Add(this.SmtpBccLabel);
            this.SmtpTestSettingsGroup.Controls.Add(this.SmtpBcc);
            this.SmtpTestSettingsGroup.Controls.Add(this.SmtpCCLabel);
            this.SmtpTestSettingsGroup.Controls.Add(this.SmtpCC);
            this.SmtpTestSettingsGroup.Controls.Add(this.SmtpToLabel);
            this.SmtpTestSettingsGroup.Controls.Add(this.SmtpTo);
            this.SmtpTestSettingsGroup.Controls.Add(this.SmtpFrom);
            this.SmtpTestSettingsGroup.Controls.Add(this.SmtpFromLabel);
            this.SmtpTestSettingsGroup.Location = new System.Drawing.Point(12, 108);
            this.SmtpTestSettingsGroup.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.SmtpTestSettingsGroup.Name = "SmtpTestSettingsGroup";
            this.SmtpTestSettingsGroup.Padding = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.SmtpTestSettingsGroup.Size = new System.Drawing.Size(1118, 590);
            this.SmtpTestSettingsGroup.TabIndex = 1;
            this.SmtpTestSettingsGroup.TabStop = false;
            this.SmtpTestSettingsGroup.Text = "SMTP Test Message";
            // 
            // SmtpSmimeSerialNumber
            // 
            this.SmtpSmimeSerialNumber.Location = new System.Drawing.Point(164, 496);
            this.SmtpSmimeSerialNumber.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.SmtpSmimeSerialNumber.Name = "SmtpSmimeSerialNumber";
            this.SmtpSmimeSerialNumber.Size = new System.Drawing.Size(938, 31);
            this.SmtpSmimeSerialNumber.TabIndex = 8;
            // 
            // SmtpIsHtml
            // 
            this.SmtpIsHtml.AutoSize = true;
            this.SmtpIsHtml.Checked = true;
            this.SmtpIsHtml.CheckState = System.Windows.Forms.CheckState.Checked;
            this.SmtpIsHtml.Location = new System.Drawing.Point(878, 546);
            this.SmtpIsHtml.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.SmtpIsHtml.Name = "SmtpIsHtml";
            this.SmtpIsHtml.Size = new System.Drawing.Size(219, 29);
            this.SmtpIsHtml.TabIndex = 12;
            this.SmtpIsHtml.Text = "Render as HTML?";
            this.SmtpIsHtml.UseVisualStyleBackColor = true;
            // 
            // SmtpSubjectLabel
            // 
            this.SmtpSubjectLabel.AutoSize = true;
            this.SmtpSubjectLabel.Location = new System.Drawing.Point(12, 252);
            this.SmtpSubjectLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.SmtpSubjectLabel.Name = "SmtpSubjectLabel";
            this.SmtpSubjectLabel.Size = new System.Drawing.Size(61, 25);
            this.SmtpSubjectLabel.TabIndex = 19;
            this.SmtpSubjectLabel.Text = "Subj:";
            this.SmtpSubjectLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // SmtpSubject
            // 
            this.SmtpSubject.Location = new System.Drawing.Point(94, 246);
            this.SmtpSubject.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.SmtpSubject.Name = "SmtpSubject";
            this.SmtpSubject.Size = new System.Drawing.Size(1008, 31);
            this.SmtpSubject.TabIndex = 5;
            this.SmtpSubject.Text = "Example Subject";
            // 
            // SmtpSmimeTripleWrap
            // 
            this.SmtpSmimeTripleWrap.AutoSize = true;
            this.SmtpSmimeTripleWrap.Location = new System.Drawing.Point(618, 546);
            this.SmtpSmimeTripleWrap.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.SmtpSmimeTripleWrap.Name = "SmtpSmimeTripleWrap";
            this.SmtpSmimeTripleWrap.Size = new System.Drawing.Size(167, 29);
            this.SmtpSmimeTripleWrap.TabIndex = 11;
            this.SmtpSmimeTripleWrap.Text = "Triple Wrap?";
            this.SmtpSmimeTripleWrap.UseVisualStyleBackColor = true;
            // 
            // SmtpSmimeEncrypt
            // 
            this.SmtpSmimeEncrypt.AutoSize = true;
            this.SmtpSmimeEncrypt.Location = new System.Drawing.Point(374, 546);
            this.SmtpSmimeEncrypt.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.SmtpSmimeEncrypt.Name = "SmtpSmimeEncrypt";
            this.SmtpSmimeEncrypt.Size = new System.Drawing.Size(225, 29);
            this.SmtpSmimeEncrypt.TabIndex = 10;
            this.SmtpSmimeEncrypt.Text = "Encrypt Envelope?";
            this.SmtpSmimeEncrypt.UseVisualStyleBackColor = true;
            // 
            // SmtpSmimeSign
            // 
            this.SmtpSmimeSign.AutoSize = true;
            this.SmtpSmimeSign.Checked = true;
            this.SmtpSmimeSign.CheckState = System.Windows.Forms.CheckState.Checked;
            this.SmtpSmimeSign.Location = new System.Drawing.Point(164, 546);
            this.SmtpSmimeSign.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.SmtpSmimeSign.Name = "SmtpSmimeSign";
            this.SmtpSmimeSign.Size = new System.Drawing.Size(193, 29);
            this.SmtpSmimeSign.TabIndex = 9;
            this.SmtpSmimeSign.Text = "Sign Message?";
            this.SmtpSmimeSign.UseVisualStyleBackColor = true;
            // 
            // SmtpSmimeLabel
            // 
            this.SmtpSmimeLabel.AutoSize = true;
            this.SmtpSmimeLabel.Location = new System.Drawing.Point(12, 502);
            this.SmtpSmimeLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.SmtpSmimeLabel.Name = "SmtpSmimeLabel";
            this.SmtpSmimeLabel.Size = new System.Drawing.Size(109, 75);
            this.SmtpSmimeLabel.TabIndex = 17;
            this.SmtpSmimeLabel.Text = "S/MIME\r\nCertificate\r\nSerial #:";
            // 
            // SmtpAttachmentsLabel
            // 
            this.SmtpAttachmentsLabel.AutoSize = true;
            this.SmtpAttachmentsLabel.Location = new System.Drawing.Point(14, 408);
            this.SmtpAttachmentsLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.SmtpAttachmentsLabel.Name = "SmtpAttachmentsLabel";
            this.SmtpAttachmentsLabel.Size = new System.Drawing.Size(139, 50);
            this.SmtpAttachmentsLabel.TabIndex = 15;
            this.SmtpAttachmentsLabel.Text = "Attachments:\r\n(one per line)";
            // 
            // SmtpAttachments
            // 
            this.SmtpAttachments.Location = new System.Drawing.Point(164, 408);
            this.SmtpAttachments.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.SmtpAttachments.Multiline = true;
            this.SmtpAttachments.Name = "SmtpAttachments";
            this.SmtpAttachments.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.SmtpAttachments.Size = new System.Drawing.Size(938, 73);
            this.SmtpAttachments.TabIndex = 7;
            // 
            // SmtpBodyLabel
            // 
            this.SmtpBodyLabel.AutoSize = true;
            this.SmtpBodyLabel.Location = new System.Drawing.Point(12, 302);
            this.SmtpBodyLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.SmtpBodyLabel.Name = "SmtpBodyLabel";
            this.SmtpBodyLabel.Size = new System.Drawing.Size(67, 25);
            this.SmtpBodyLabel.TabIndex = 13;
            this.SmtpBodyLabel.Text = "Body:";
            this.SmtpBodyLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // SmtpBody
            // 
            this.SmtpBody.Location = new System.Drawing.Point(94, 296);
            this.SmtpBody.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.SmtpBody.Multiline = true;
            this.SmtpBody.Name = "SmtpBody";
            this.SmtpBody.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.SmtpBody.Size = new System.Drawing.Size(1008, 96);
            this.SmtpBody.TabIndex = 6;
            // 
            // SmtpBccLabel
            // 
            this.SmtpBccLabel.AutoSize = true;
            this.SmtpBccLabel.Location = new System.Drawing.Point(12, 202);
            this.SmtpBccLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.SmtpBccLabel.Name = "SmtpBccLabel";
            this.SmtpBccLabel.Size = new System.Drawing.Size(68, 25);
            this.SmtpBccLabel.TabIndex = 11;
            this.SmtpBccLabel.Text = "BCC: ";
            this.SmtpBccLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // SmtpBcc
            // 
            this.SmtpBcc.Location = new System.Drawing.Point(94, 196);
            this.SmtpBcc.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.SmtpBcc.Name = "SmtpBcc";
            this.SmtpBcc.Size = new System.Drawing.Size(1008, 31);
            this.SmtpBcc.TabIndex = 4;
            // 
            // SmtpCCLabel
            // 
            this.SmtpCCLabel.AutoSize = true;
            this.SmtpCCLabel.Location = new System.Drawing.Point(12, 152);
            this.SmtpCCLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.SmtpCCLabel.Name = "SmtpCCLabel";
            this.SmtpCCLabel.Size = new System.Drawing.Size(54, 25);
            this.SmtpCCLabel.TabIndex = 9;
            this.SmtpCCLabel.Text = "CC: ";
            this.SmtpCCLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // SmtpCC
            // 
            this.SmtpCC.Location = new System.Drawing.Point(94, 146);
            this.SmtpCC.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.SmtpCC.Name = "SmtpCC";
            this.SmtpCC.Size = new System.Drawing.Size(1008, 31);
            this.SmtpCC.TabIndex = 3;
            // 
            // SmtpToLabel
            // 
            this.SmtpToLabel.AutoSize = true;
            this.SmtpToLabel.Location = new System.Drawing.Point(12, 102);
            this.SmtpToLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.SmtpToLabel.Name = "SmtpToLabel";
            this.SmtpToLabel.Size = new System.Drawing.Size(49, 25);
            this.SmtpToLabel.TabIndex = 7;
            this.SmtpToLabel.Text = "To: ";
            this.SmtpToLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // SmtpTo
            // 
            this.SmtpTo.Location = new System.Drawing.Point(94, 96);
            this.SmtpTo.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.SmtpTo.Name = "SmtpTo";
            this.SmtpTo.Size = new System.Drawing.Size(1008, 31);
            this.SmtpTo.TabIndex = 2;
            this.SmtpTo.Text = "recipient@example.com";
            // 
            // SmtpFrom
            // 
            this.SmtpFrom.Location = new System.Drawing.Point(94, 46);
            this.SmtpFrom.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.SmtpFrom.Name = "SmtpFrom";
            this.SmtpFrom.Size = new System.Drawing.Size(1008, 31);
            this.SmtpFrom.TabIndex = 1;
            this.SmtpFrom.Text = "user@example.com";
            // 
            // SmtpFromLabel
            // 
            this.SmtpFromLabel.AutoSize = true;
            this.SmtpFromLabel.Location = new System.Drawing.Point(12, 52);
            this.SmtpFromLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.SmtpFromLabel.Name = "SmtpFromLabel";
            this.SmtpFromLabel.Size = new System.Drawing.Size(73, 25);
            this.SmtpFromLabel.TabIndex = 0;
            this.SmtpFromLabel.Text = "From: ";
            this.SmtpFromLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // LoadSettingsButton
            // 
            this.LoadSettingsButton.Location = new System.Drawing.Point(14, 763);
            this.LoadSettingsButton.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.LoadSettingsButton.Name = "LoadSettingsButton";
            this.LoadSettingsButton.Size = new System.Drawing.Size(276, 44);
            this.LoadSettingsButton.TabIndex = 1;
            this.LoadSettingsButton.Text = "Load Settings";
            this.LoadSettingsButton.UseVisualStyleBackColor = true;
            this.LoadSettingsButton.Click += new System.EventHandler(this.LoadSettingsButton_Click);
            // 
            // SaveSettingsButton
            // 
            this.SaveSettingsButton.Location = new System.Drawing.Point(880, 763);
            this.SaveSettingsButton.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.SaveSettingsButton.Name = "SaveSettingsButton";
            this.SaveSettingsButton.Size = new System.Drawing.Size(276, 44);
            this.SaveSettingsButton.TabIndex = 2;
            this.SaveSettingsButton.Text = "Save Settings";
            this.SaveSettingsButton.UseVisualStyleBackColor = true;
            this.SaveSettingsButton.Click += new System.EventHandler(this.SaveSettingsButton_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1168, 810);
            this.Controls.Add(this.SaveSettingsButton);
            this.Controls.Add(this.LoadSettingsButton);
            this.Controls.Add(this.TabsControl);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.MinimumSize = new System.Drawing.Size(1174, 819);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "OpaqueMail Test Client";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.TabsControl.ResumeLayout(false);
            this.SettingsTab.ResumeLayout(false);
            this.SmtpSettingsGroup.ResumeLayout(false);
            this.SmtpSettingsGroup.PerformLayout();
            this.Pop3SettingsGroup.ResumeLayout(false);
            this.Pop3SettingsGroup.PerformLayout();
            this.ImapSettingsGroup.ResumeLayout(false);
            this.ImapSettingsGroup.PerformLayout();
            this.ImapTab.ResumeLayout(false);
            this.ImapPreviewGroup.ResumeLayout(false);
            this.ImapPreviewGroup.PerformLayout();
            this.ImapWebPreviewPanel.ResumeLayout(false);
            this.ImapTestGroup.ResumeLayout(false);
            this.ImapTestGroup.PerformLayout();
            this.ImapMessageGroup.ResumeLayout(false);
            this.ImapMessageGroup.PerformLayout();
            this.Pop3Tab.ResumeLayout(false);
            this.Pop3PreviewGroup.ResumeLayout(false);
            this.Pop3PreviewGroup.PerformLayout();
            this.Pop3WebPreviewPanel.ResumeLayout(false);
            this.Pop3TestGroup.ResumeLayout(false);
            this.Pop3MessageGroup.ResumeLayout(false);
            this.SmtpTab.ResumeLayout(false);
            this.SmtpTestGroup.ResumeLayout(false);
            this.SmtpTestSettingsGroup.ResumeLayout(false);
            this.SmtpTestSettingsGroup.PerformLayout();
            this.ResumeLayout(false);

        }
        #endregion

        private System.Windows.Forms.TabControl TabsControl;
        private System.Windows.Forms.TabPage SettingsTab;
        private System.Windows.Forms.GroupBox SmtpSettingsGroup;
        private System.Windows.Forms.Button SmtpCopyImapButton;
        private System.Windows.Forms.Button SmtpCopyPop3Button;
        private System.Windows.Forms.Label SmtpSslLabel;
        private System.Windows.Forms.CheckBox SmtpSsl;
        private System.Windows.Forms.Label SmtpPortLabel;
        private System.Windows.Forms.TextBox SmtpPort;
        private System.Windows.Forms.Label SmtpPasswordLabel;
        private System.Windows.Forms.TextBox SmtpPassword;
        private System.Windows.Forms.TextBox SmtpUsername;
        private System.Windows.Forms.Label SmtpUsernameLabel;
        private System.Windows.Forms.TextBox SmtpHost;
        private System.Windows.Forms.Label SmtpHostLabel;
        private System.Windows.Forms.GroupBox Pop3SettingsGroup;
        private System.Windows.Forms.Button Pop3CopyImapButton;
        private System.Windows.Forms.Button Pop3CopySmtpButton;
        private System.Windows.Forms.Label Pop3SslLabel;
        private System.Windows.Forms.CheckBox Pop3Ssl;
        private System.Windows.Forms.Label Pop3PortLabel;
        private System.Windows.Forms.TextBox Pop3Port;
        private System.Windows.Forms.Label Pop3PasswordLabel;
        private System.Windows.Forms.TextBox Pop3Password;
        private System.Windows.Forms.TextBox Pop3Username;
        private System.Windows.Forms.Label Pop3UsernameLabel;
        private System.Windows.Forms.TextBox Pop3Host;
        private System.Windows.Forms.Label Pop3HostLabel;
        private System.Windows.Forms.GroupBox ImapSettingsGroup;
        private System.Windows.Forms.Button ImapCopyPop3Button;
        private System.Windows.Forms.Button ImapCopySmtpButton;
        private System.Windows.Forms.Label ImapSslLabel;
        private System.Windows.Forms.CheckBox ImapSsl;
        private System.Windows.Forms.Label ImapPortLabel;
        private System.Windows.Forms.TextBox ImapPort;
        private System.Windows.Forms.Label ImapPasswordLabel;
        private System.Windows.Forms.TextBox ImapPassword;
        private System.Windows.Forms.TextBox ImapUsername;
        private System.Windows.Forms.Label ImapUsernameLabel;
        private System.Windows.Forms.TextBox ImapHost;
        private System.Windows.Forms.Label ImapHostLabel;
        private System.Windows.Forms.TabPage ImapTab;
        private System.Windows.Forms.TabPage Pop3Tab;
        private System.Windows.Forms.TabPage SmtpTab;
        private System.Windows.Forms.Button LoadSettingsButton;
        private System.Windows.Forms.Button SaveSettingsButton;
        private System.Windows.Forms.GroupBox SmtpTestGroup;
        private System.Windows.Forms.Button SmtpSendButton;
        private System.Windows.Forms.GroupBox SmtpTestSettingsGroup;
        private System.Windows.Forms.CheckBox SmtpSmimeTripleWrap;
        private System.Windows.Forms.CheckBox SmtpSmimeEncrypt;
        private System.Windows.Forms.CheckBox SmtpSmimeSign;
        private System.Windows.Forms.Label SmtpSmimeLabel;
        private System.Windows.Forms.Label SmtpAttachmentsLabel;
        private System.Windows.Forms.TextBox SmtpAttachments;
        private System.Windows.Forms.Label SmtpBodyLabel;
        private System.Windows.Forms.TextBox SmtpBody;
        private System.Windows.Forms.Label SmtpBccLabel;
        private System.Windows.Forms.TextBox SmtpBcc;
        private System.Windows.Forms.Label SmtpCCLabel;
        private System.Windows.Forms.TextBox SmtpCC;
        private System.Windows.Forms.Label SmtpToLabel;
        private System.Windows.Forms.TextBox SmtpTo;
        private System.Windows.Forms.TextBox SmtpFrom;
        private System.Windows.Forms.Label SmtpFromLabel;
        private System.Windows.Forms.CheckBox SmtpIsHtml;
        private System.Windows.Forms.Label SmtpSubjectLabel;
        private System.Windows.Forms.TextBox SmtpSubject;
        private System.Windows.Forms.TextBox SmtpSmimeSerialNumber;
        private System.Windows.Forms.GroupBox ImapPreviewGroup;
        private System.Windows.Forms.Panel ImapWebPreviewPanel;
        private System.Windows.Forms.WebBrowser ImapWebPreview;
        private System.Windows.Forms.GroupBox ImapTestGroup;
        private System.Windows.Forms.Button ImapRetrieveMessagesButton;
        private System.Windows.Forms.GroupBox ImapMessageGroup;
        private System.Windows.Forms.ListBox ImapMessageList;
        private System.Windows.Forms.TextBox ImapHeaders;
        private System.Windows.Forms.Button ImapLoadFileButton;
        private System.Windows.Forms.GroupBox Pop3PreviewGroup;
        private System.Windows.Forms.TextBox Pop3Headers;
        private System.Windows.Forms.Panel Pop3WebPreviewPanel;
        private System.Windows.Forms.WebBrowser Pop3WebPreview;
        private System.Windows.Forms.GroupBox Pop3TestGroup;
        private System.Windows.Forms.Button Pop3RetrieveMessageButton;
        private System.Windows.Forms.GroupBox Pop3MessageGroup;
        private System.Windows.Forms.ListBox Pop3MessageList;
        private System.Windows.Forms.Button ImapDeleteMessageButton;
        private System.Windows.Forms.Button Pop3DeleteMessageButton;
        private System.Windows.Forms.Button ImapAppendMessageButton;
        private System.Windows.Forms.Button ImapGetQuotaButton;
        private System.Windows.Forms.TextBox ImapSearchText;
        private System.Windows.Forms.ComboBox ImapMailboxList;
        private System.Windows.Forms.CheckBox ImapFirst1k;
        private System.Windows.Forms.CheckBox ImapIncludeBody;
        private System.Windows.Forms.CheckBox ImapIncludeHeaders;
    }
}

