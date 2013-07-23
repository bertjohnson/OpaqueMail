using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;

namespace OpaqueMail.TestClient
{
    public partial class Form1 : Form
    {
        #region Private Members
        /// <summary>Collection of mailbox/UID pairs for messages loaded over IMAP.</summary>
        private Tuple<string, int>[] ImapMessageIDs;
        /// <summary>List of message IDs for messages loaded over POP3.</summary>
        private List<int> Pop3MessageIDs;
        /// <summary>An OpaqueMail.ImapClient to be reused across operations.</summary>
        ImapClient myImapClient;
        /// <summary>An OpaqueMail.Pop3Client to be reused across operations.</summary>
        Pop3Client myPop3Client;
        /// <summary>A file dialog box to be used when loading or saving *.eml files.</summary>
        OpenFileDialog FileDialog = new OpenFileDialog();
        #endregion Private Members

        #region Event Handlers
        /// <summary>
        /// Test Client constructor.
        /// </summary>
        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Test Client load event handler.
        /// </summary>
        private void Form1_Load(object sender, EventArgs e)
        {
            // Attempt to initialize settings based on values from a previous session.
            LoadSettings();
            // Resize the form in case loaded settings changed the window size.
            ResizeForm();

            // Handle the resize event.
            this.Resize += Form1_Resize;
            this.FormClosing += Form1_FormClosing;
            ImapSearchText.GotFocus += ImapSearchText_GotFocus;
            ImapSearchText.KeyDown += ImapSearchText_KeyDown;
            ImapSearchText.LostFocus += ImapSearchText_LostFocus;
        }

        /// <summary>
        /// Clean up when the Test Client is unloading.
        /// </summary>
        void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (myImapClient != null)
                myImapClient.Dispose();
            if (myPop3Client != null)
                myPop3Client.Dispose();
        }

        /// <summary>
        /// Handle the window resized event.
        /// </summary>
        void Form1_Resize(object sender, System.EventArgs e)
        {
            ResizeForm();
        }

        /// <summary>
        /// Append a sample message to the Inbox.
        /// </summary>
        private void ImapAppendMessageButton_Click(object sender, EventArgs e)
        {
            // If we're not currently connected to the IMAP server, connect and authenticate.
            if (myImapClient == null)
            {
                int imapPort = 993;
                int.TryParse(ImapPort.Text, out imapPort);

                myImapClient = new ImapClient(ImapHost.Text, imapPort, ImapUsername.Text, ImapPassword.Text, ImapSsl.Checked);
            }

            // If our connection has timed out or been closed, reconnect.
            if (!myImapClient.IsConnected)
            {
                myImapClient.Connect();
                myImapClient.Authenticate();

                // Ensure we connected successfully.
                if (!myImapClient.IsConnected)
                {
                    MessageBox.Show("Unable to connect to the IMAP server. Please double-check your settings.", "Unable to connect.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            myImapClient.AppendMessage("INBOX",
@"Date: " + DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss zzzz") + @"
From: Test User <testuser@example.com>
To: Me <" + ImapUsername.Text + @">
Message-Id: <" + Guid.NewGuid().ToString().Replace("{", "").Replace("}", "") + "@" + ImapHost.Text + @">
Subject: Test Append Message
MIME-Version: 1.0
Content-Type: Text/Plain; Charset=US-ASCII

This is a test of the APPEND command.", new string[] { @"\Seen" }, DateTime.Now);
        }

        /// <summary>
        /// Copy server settings from POP3 to IMAP.
        /// </summary>
        private void ImapCopyPop3Button_Click(object sender, EventArgs e)
        {
            ImapHost.Text = Pop3Host.Text;
            ImapUsername.Text = Pop3Username.Text;
            ImapPassword.Text = Pop3Password.Text;
            ImapSsl.Checked = Pop3Ssl.Checked;
        }

        /// <summary>
        /// Copy server settings from SMTP to IMAP.
        /// </summary>
        private void ImapCopySmtpButton_Click(object sender, EventArgs e)
        {
            ImapHost.Text = SmtpHost.Text;
            ImapUsername.Text = SmtpUsername.Text;
            ImapPassword.Text = SmtpPassword.Text;
            ImapSsl.Checked = SmtpSsl.Checked;
        }

        /// <summary>
        /// Delete the selected IMAP message.
        /// </summary>
        private async void ImapDeleteMessageButton_Click(object sender, EventArgs e)
        {
            await myImapClient.DeleteMessageUidAsync(ImapMessageIDs[ImapMessageList.SelectedIndex].Item1, ImapMessageIDs[ImapMessageList.SelectedIndex].Item2);
            ImapDeleteMessageButton.Enabled = false;
            await RefreshImapMessages();
        }

        /// <summary>
        /// Retrieve the overall quota and the quota for the users Inbox.
        /// </summary>
        private async void ImapGetQuotaButton_Click(object sender, EventArgs e)
        {
            // If we're not currently connected to the IMAP server, connect and authenticate.
            if (myImapClient == null)
            {
                int imapPort = 993;
                int.TryParse(ImapPort.Text, out imapPort);

                myImapClient = new ImapClient(ImapHost.Text, imapPort, ImapUsername.Text, ImapPassword.Text, ImapSsl.Checked);
            }

            // If our connection has timed out or been closed, reconnect.
            if (!myImapClient.IsConnected)
            {
                myImapClient.Connect();
                myImapClient.Authenticate();

                // Ensure we connected successfully.
                if (!myImapClient.IsConnected)
                {
                    MessageBox.Show("Unable to connect to the IMAP server. Please double-check your settings.", "Unable to connect.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            StringBuilder messageBuilder = new StringBuilder();

            QuotaUsage totalUsage = await myImapClient.GetQuotaAsync("");
            QuotaUsage inboxUsage = await myImapClient.GetQuotaRootAsync("INBOX");

            messageBuilder.Append("Overall quota information:\r\n\r\nUsed: " + totalUsage.Usage + "\r\nTotal: " + totalUsage.QuotaMaximum + "\r\n\r\nINBOX quota information:\r\n\r\nUsed: " + inboxUsage.Usage + "\r\nTotal: " + inboxUsage.QuotaMaximum);

            string[] mailboxes = await myImapClient.ListMailboxNamesAsync(true);
            messageBuilder.Append("\r\n\r\nFolders (" + mailboxes.Length.ToString() + " Total):\r\n");

            foreach (string mailbox in mailboxes)
                messageBuilder.Append("\r\n" + mailbox);

            MessageBox.Show(messageBuilder.ToString(), "Quota and Mailbox Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Load and preview an *.eml file.
        /// </summary>
        private void ImapLoadFileButton_Click(object sender, EventArgs e)
        {
            FileDialog.CheckFileExists = true;
            FileDialog.Filter = "Email messages (*.eml)|*.eml|All files|*.*";
            FileDialog.Multiselect = false;

            if (FileDialog.ShowDialog() == DialogResult.OK)
            {
                ReadOnlyMailMessage message = ReadOnlyMailMessage.LoadFile(FileDialog.FileName);
                RenderMessage(message, ImapHeaders, ImapWebPreview, ImapWebPreviewPanel);
            }
        }

        /// <summary>
        /// When a message has been selected, attempt to load its preview.
        /// </summary>
        private void ImapMessageList_SelectedIndexChanged(object sender, EventArgs e)
        {
            ImapDeleteMessageButton.Enabled = ImapMessageList.SelectedIndex > -1;
            if (ImapMessageList.SelectedIndex > -1)
            {
                if (myImapClient != null && ImapMessageIDs != null)
                {
                    if (ImapMessageList.SelectedIndex < ImapMessageIDs.Length)
                    {
                        if (ImapMessageIDs[ImapMessageList.SelectedIndex].Item2 > 0)
                        {
                            // If our connection has timed out or been closed, reconnect.
                            if (!myImapClient.IsConnected)
                            {
                                myImapClient.Connect();
                                myImapClient.Authenticate();
                            }

                            if (myImapClient.IsConnected)
                            {
                                // Retrieve the selected message.
                                ReadOnlyMailMessage message = myImapClient.GetMessageUid(ImapMessageIDs[ImapMessageList.SelectedIndex].Item1, ImapMessageIDs[ImapMessageList.SelectedIndex].Item2);

                                if (message != null)
                                {
                                    // Populate the IMAP viewport with this message's headers and body.
                                    RenderMessage(message, ImapHeaders, ImapWebPreview, ImapWebPreviewPanel);
                                }
                                else
                                {
                                    // If the message was deleted, null will be returned.
                                    ImapHeaders.BackColor = Color.White;
                                    ImapHeaders.Text = "";
                                    ImapWebPreview.Document.Write("Message not found.");
                                    ImapWebPreviewPanel.Refresh();
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                // If no message was retrieved, clear the viewport.
                ImapWebPreview.DocumentText = "";
                ImapWebPreviewPanel.Refresh();
            }
        }

        /// <summary>
        /// Attempt to retrieve up to 25 IMAP messages and populate the list of messages.
        /// </summary>
        private async void ImapRetrieveMessagesButton_Click(object sender, EventArgs e)
        {
            ImapRetrieveMessagesButton.Enabled = false;
            ImapMessageList.Enabled = false;
            await RefreshImapMessages();
            ImapMessageList.Enabled = true;
            ImapRetrieveMessagesButton.Enabled = true;
        }

        /// <summary>
        /// Clear the default search message when clicked.
        /// </summary>
        private void ImapSearchText_GotFocus(object sender, System.EventArgs e)
        {
            if (ImapSearchText.Text == "Search...")
                ImapSearchText.Text = "";
        }

        /// <summary>
        /// Process a search when clicked.
        /// </summary>
        private async void ImapSearchText_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                // If we're not currently connected to the IMAP server, connect and authenticate.
                if (myImapClient == null)
                {
                    int imapPort = 993;
                    int.TryParse(ImapPort.Text, out imapPort);

                    myImapClient = new ImapClient(ImapHost.Text, imapPort, ImapUsername.Text, ImapPassword.Text, ImapSsl.Checked);
                }

                // If our connection has timed out or been closed, reconnect.
                if (!myImapClient.IsConnected)
                {
                    myImapClient.Connect();
                    myImapClient.Authenticate();

                    // Ensure we connected successfully.
                    if (!myImapClient.IsConnected)
                    {
                        MessageBox.Show("Unable to connect to the IMAP server. Please double-check your settings.", "Unable to connect.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }

                if (myImapClient != null)
                {
                    if (myImapClient.IsConnected)
                    {
                        await myImapClient.SelectMailboxAsync("INBOX");
                        List<ReadOnlyMailMessage> messages = await myImapClient.SearchAsync("TEXT \"" + ImapSearchText.Text + "\"");

                        ImapMessageIDs = new Tuple<string, int>[messages.Count];

                        // Repopulate the message list with the subjects of messages retrieved.
                        ImapMessageList.Items.Clear();
                        for (int i = 0; i < messages.Count; i++)
                        {
                            ImapMessageList.Items.Add(messages[i].Subject);
                            ImapMessageIDs[i] = new Tuple<string, int>(messages[i].Mailbox, messages[i].ImapUid);
                        }

                        // Reset the preview viewport.
                        ImapHeaders.BackColor = Color.White;
                        ImapHeaders.Text = "";
                        ImapWebPreview.DocumentText = "Please select a message from the left-hand panel.";
                        ImapWebPreviewPanel.Refresh();
                    }
                }
            }
        }

        /// <summary>
        /// Add the default search message if the query is blank.
        /// </summary>
        private void ImapSearchText_LostFocus(object sender, System.EventArgs e)
        {
            if (string.IsNullOrEmpty(ImapSearchText.Text))
                ImapSearchText.Text = "Search...";
        }

        /// <summary>
        /// Load settings for this Test Client from an XML file.
        /// </summary>
        private void LoadSettingsButton_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure?  All settings will be overwritten by the contents of \"OpaqueMail.TestClient.xml\" on your Desktop.", "Confirm Load", MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
            if (result == DialogResult.OK)
                LoadSettings();
        }

        /// <summary>
        /// Copy server settings from IMAP to POP3.
        /// </summary>
        private void Pop3CopyImapButton_Click(object sender, EventArgs e)
        {
            Pop3Host.Text = ImapHost.Text;
            Pop3Username.Text = ImapUsername.Text;
            Pop3Password.Text = ImapPassword.Text;
            Pop3Ssl.Checked = ImapSsl.Checked;
        }

        /// <summary>
        /// Copy server settings from SMTP to POP3.
        /// </summary>
        private void Pop3CopySmtpButton_Click(object sender, EventArgs e)
        {
            Pop3Host.Text = SmtpHost.Text;
            Pop3Username.Text = SmtpUsername.Text;
            Pop3Password.Text = SmtpPassword.Text;
            Pop3Ssl.Checked = SmtpSsl.Checked;
        }

        /// <summary>
        /// Delete the selected IMAP message.
        /// </summary>
        private async void Pop3DeleteMessageButton_Click(object sender, EventArgs e)
        {
            await myPop3Client.DeleteMessageAsync(Pop3MessageIDs[Pop3MessageList.SelectedIndex]);
            Pop3DeleteMessageButton.Enabled = false;
            await RefreshPop3Messages();
        }

        /// <summary>
        /// Attempt to retrieve up to 25 POP3 messages and populate the list of messages.
        /// </summary>
        private async void Pop3RetrieveMessageButton_Click(object sender, EventArgs e)
        {
            Pop3RetrieveMessageButton.Enabled = false;
            Pop3MessageList.Enabled = false;
            await RefreshPop3Messages();
            Pop3MessageList.Enabled = true;
            Pop3RetrieveMessageButton.Enabled = true;
        }

        /// <summary>
        /// When a message has been selected, attempt to load its preview.
        /// </summary>
        private void Pop3MessageList_SelectedIndexChanged(object sender, EventArgs e)
        {
            Pop3DeleteMessageButton.Enabled = Pop3MessageList.SelectedIndex > -1;
            if (Pop3MessageList.SelectedIndex > -1)
            {
                if (myPop3Client != null && Pop3MessageIDs != null)
                {
                    if (Pop3MessageList.SelectedIndex < Pop3MessageIDs.Count)
                    {
                        if (Pop3MessageIDs[Pop3MessageList.SelectedIndex] > -1)
                        {
                            // If our connection has timed out or been closed, reconnect.
                            if (!myPop3Client.IsConnected)
                            {
                                myPop3Client.Connect();
                                myPop3Client.Authenticate();
                            }

                            if (myPop3Client.IsConnected)
                            {
                                // Retrieve the selected message.
                                ReadOnlyMailMessage message = myPop3Client.GetMessage(Pop3MessageIDs[Pop3MessageList.SelectedIndex]);

                                if (message != null)
                                {
                                    // Populate the IMAP viewport with this message's headers and body.
                                    RenderMessage(message, Pop3Headers, Pop3WebPreview, Pop3WebPreviewPanel);
                                }
                                else
                                {
                                    // If the message was deleted, null will be returned.
                                    Pop3Headers.BackColor = Color.White;
                                    Pop3Headers.Text = "";
                                    Pop3WebPreview.Document.Write("Message not found.");
                                    Pop3WebPreviewPanel.Refresh();
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                // If no message was retrieved, clear the viewport.
                Pop3WebPreview.DocumentText = "";
                Pop3WebPreviewPanel.Refresh();
            }
        }

        /// <summary>
        /// Save all current Test Client settings to an XML file.
        /// </summary>
        private void SaveSettingsButton_Click(object sender, EventArgs e)
        {
            SaveSettings();
        }

        /// <summary>
        /// Copy server settings from IMAP to SMTP.
        /// </summary>
        private void SmtpCopyImapButton_Click(object sender, EventArgs e)
        {
            SmtpHost.Text = ImapHost.Text;
            SmtpUsername.Text = ImapUsername.Text;
            SmtpPassword.Text = ImapPassword.Text;
            SmtpSsl.Checked = ImapSsl.Checked;
        }

        /// <summary>
        /// Copy server settings from POP3 to SMTP.
        /// </summary>
        private void SmtpCopyPop3Button_Click(object sender, EventArgs e)
        {
            SmtpHost.Text = Pop3Host.Text;
            SmtpUsername.Text = Pop3Username.Text;
            SmtpPassword.Text = Pop3Password.Text;
            SmtpSsl.Checked = Pop3Ssl.Checked;
        }

        /// <summary>
        /// Send an e-mail using the SMTP settings specified.
        /// </summary>
        private async void SmtpSendButton_Click(object sender, EventArgs e)
        {
            X509Certificate2 signingCertificate = null;
            if (SmtpSmimeSign.Checked || SmtpSmimeTripleWrap.Checked)
            {
                // If S/MIME signing the message, attempt to look up a certificate from the Windows certificate store matching the serial number specified.
                if (SmtpSmimeSerialNumber.Text.Length < 1)
                {
                    MessageBox.Show("SMTP send exception:\r\n\r\nA signing certificate must be passed prior to signing.", "SMTP send exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    SmtpSmimeSerialNumber.Focus();
                    return;
                }

                signingCertificate = CertHelper.GetCertificateBySerialNumber(StoreLocation.CurrentUser, SmtpSmimeSerialNumber.Text);
                if (signingCertificate == null)
                    signingCertificate = CertHelper.GetCertificateBySerialNumber(StoreLocation.LocalMachine, SmtpSmimeSerialNumber.Text);

                if (signingCertificate == null)
                {
                    MessageBox.Show("Certificate with signature \"" + SmtpSmimeSerialNumber.Text + "\" not found.");
                    SmtpSmimeSerialNumber.Focus();
                    return;
                }
            }

            try
            {
                int smtpPort = 25;
                int.TryParse(SmtpPort.Text, out smtpPort);

                SmtpClient smtpClient = new SmtpClient(SmtpHost.Text, smtpPort);
                smtpClient.Credentials = new NetworkCredential(SmtpUsername.Text, SmtpPassword.Text);
                smtpClient.EnableSsl = true;

                MailMessage message = new MailMessage();
                message.From = new MailAddress(SmtpFrom.Text);

                // Parse all addresses provided into MailAddress objects.
                if (SmtpTo.Text.Length > 0)
                {
                    MailAddressCollection toAddresses = Functions.FromMailAddressString(SmtpTo.Text);
                    foreach (MailAddress toAddress in toAddresses)
                        message.To.Add(toAddress);
                }

                if (SmtpCC.Text.Length > 0)
                {
                    MailAddressCollection ccAddresses = Functions.FromMailAddressString(SmtpCC.Text);
                    foreach (MailAddress ccAddress in ccAddresses)
                        message.CC.Add(ccAddress);
                }

                if (SmtpBcc.Text.Length > 0)
                {
                    MailAddressCollection bccAddresses = Functions.FromMailAddressString(SmtpBcc.Text);
                    foreach (MailAddress bccAddress in bccAddresses)
                        message.Bcc.Add(bccAddress);
                }

                message.Subject = SmtpSubject.Text;
                message.Body = SmtpBody.Text;

                // Process attachments.
                string[] attachmentLines = SmtpAttachments.Text.Replace("\r", "").Split('\n');
                foreach (string attachmentLine in attachmentLines)
                {
                    if (attachmentLine.Trim().Length > 0)
                        message.Attachments.Add(new Attachment(attachmentLine.Trim()));
                }

                message.IsBodyHtml = SmtpIsHtml.Checked;
                message.SmimeSigningCertificate = signingCertificate;
                message.SmimeSigned = SmtpSmimeSign.Checked;
                message.SmimeEncryptedEnvelope = SmtpSmimeEncrypt.Checked;
                message.SmimeTripleWrapped = SmtpSmimeTripleWrap.Checked;

                message.SmimeEncryptionOptionFlags = message.SmimeEncryptionOptionFlags | SmimeEncryptionOptionFlags.EncryptSubject;

                await smtpClient.SendAsync(message);
                MessageBox.Show("Message successfully sent.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("SMTP send exception:\r\n\r\n" + ex.ToString(), "SMTP send exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion Event Handlers

        #region Private Methods
        /// <summary>
        /// Return the path to where the Test Client's settings should be saved and loaded.
        /// </summary>
        private string GetSettingsFileName()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\OpaqueMail.TestClient.xml";
        }

        /// <summary>
        /// Retrieve a saved XML setting.
        /// </summary>
        private string GetValue(XPathNavigator navigator, string xpathExpression)
        {
            XPathNavigator valueNavigator = navigator.SelectSingleNode(xpathExpression);
            if (valueNavigator != null)
                return valueNavigator.Value;

            return "";
        }

        /// <summary>
        /// Load settings for this Test Client from an XML file.
        /// </summary>
        private void LoadSettings()
        {
            string fileName = GetSettingsFileName();
            if (File.Exists(fileName))
            {
                try
                {
                    XPathDocument document = new XPathDocument(fileName);
                    XPathNavigator navigator = document.CreateNavigator();

                    SetTextBoxValue(navigator, ImapHost, "/Settings/IMAP/Host");
                    SetTextBoxValue(navigator, ImapPort, "/Settings/IMAP/Port");
                    SetTextBoxValue(navigator, ImapUsername, "/Settings/IMAP/Username");
                    SetTextBoxValue(navigator, ImapPassword, "/Settings/IMAP/Password");
                    SetCheckBoxValue(navigator, ImapSsl, "/Settings/IMAP/SSL");

                    SetTextBoxValue(navigator, Pop3Host, "/Settings/POP3/Host");
                    SetTextBoxValue(navigator, Pop3Port, "/Settings/POP3/Port");
                    SetTextBoxValue(navigator, Pop3Username, "/Settings/POP3/Username");
                    SetTextBoxValue(navigator, Pop3Password, "/Settings/POP3/Password");
                    SetCheckBoxValue(navigator, Pop3Ssl, "/Settings/POP3/SSL");

                    SetTextBoxValue(navigator, SmtpHost, "/Settings/SMTP/Host");
                    SetTextBoxValue(navigator, SmtpPort, "/Settings/SMTP/Port");
                    SetTextBoxValue(navigator, SmtpUsername, "/Settings/SMTP/Username");
                    SetTextBoxValue(navigator, SmtpPassword, "/Settings/SMTP/Password");
                    SetCheckBoxValue(navigator, SmtpSsl, "/Settings/SMTP/SSL");

                    SetTextBoxValue(navigator, SmtpFrom, "/Settings/SMTP/From");
                    SetTextBoxValue(navigator, SmtpTo, "/Settings/SMTP/To");
                    SetTextBoxValue(navigator, SmtpCC, "/Settings/SMTP/CC");
                    SetTextBoxValue(navigator, SmtpBcc, "/Settings/SMTP/BCC");
                    SetTextBoxValue(navigator, SmtpSubject, "/Settings/SMTP/Subject");
                    SetTextBoxValue(navigator, SmtpBody, "/Settings/SMTP/Body");
                    SetTextBoxValue(navigator, SmtpAttachments, "/Settings/SMTP/Attachments");
                    SetTextBoxValue(navigator, SmtpSmimeSerialNumber, "/Settings/SMTP/SMIMESerialNumber");
                    SetCheckBoxValue(navigator, SmtpSmimeSign, "/Settings/SMTP/SMIMESign");
                    SetCheckBoxValue(navigator, SmtpSmimeEncrypt, "/Settings/SMTP/SMIMEEncrypt");
                    SetCheckBoxValue(navigator, SmtpSmimeTripleWrap, "/Settings/SMTP/SMIMETripleWrap");
                    SetCheckBoxValue(navigator, SmtpIsHtml, "/Settings/SMTP/IsHTML");

                    switch (GetValue(navigator, "/Settings/Window/State"))
                    {
                        case "Maximized":
                            WindowState = FormWindowState.Maximized;
                            break;
                        case "Minimized":
                            WindowState = FormWindowState.Minimized;
                            break;
                        case "Normal":
                            WindowState = FormWindowState.Normal;
                            break;
                    }
                    int value;
                    int.TryParse(GetValue(navigator, "/Settings/Window/Width"), out value);
                    if (value > 0)
                        Width = value;
                    int.TryParse(GetValue(navigator, "/Settings/Window/Height"), out value);
                    if (value > 0)
                        Height = value;
                    int.TryParse(GetValue(navigator, "/Settings/Window/Left"), out value);
                    if (value > 0)
                        Left = value;
                    int.TryParse(GetValue(navigator, "/Settings/Window/Top"), out value);
                    if (value > 0)
                        Top  = value;
                    int.TryParse(GetValue(navigator, "/Settings/Window/SelectedTab"), out value);
                    if (value > 0)
                        TabsControl.SelectedIndex = value;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading settings from \"" + fileName + "\".\r\n\r\nException: " + ex.ToString(), "Error loading settings", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// Attempt to retrieve up to 25 IMAP messages and populate the list of messages.
        /// </summary>
        private async Task RefreshImapMessages()
        {
            // If we're not currently connected to the IMAP server, connect and authenticate.
            if (myImapClient == null)
            {
                int imapPort = 993;
                int.TryParse(ImapPort.Text, out imapPort);

                myImapClient = new ImapClient(ImapHost.Text, imapPort, ImapUsername.Text, ImapPassword.Text, ImapSsl.Checked);
            }

            // If our connection has timed out or been closed, reconnect.
            if (!myImapClient.IsConnected || !myImapClient.IsAuthenticated)
            {
                myImapClient.Connect();
                myImapClient.Authenticate();

                // Ensure we connected successfully.
                if (!myImapClient.IsConnected || !myImapClient.IsAuthenticated)
                {
                    MessageBox.Show("Unable to connect to the IMAP server. Please double-check your settings.", "Unable to connect.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            // Retrieve the headers of up to 25 messages and remember their mailbox/UID pairs for later opening.
            await myImapClient.SelectMailboxAsync("INBOX");
            List<ReadOnlyMailMessage> messages = await myImapClient.GetMessagesAsync("INBOX", 25, 1, true, false, false);
            ImapMessageIDs = new Tuple<string, int>[messages.Count];

            // Repopulate the message list with the subjects of messages retrieved.
            ImapMessageList.Items.Clear();
            for (int i = 0; i < messages.Count; i++)
            {
                ImapMessageList.Items.Add(messages[i].Subject);
                ImapMessageIDs[i] = new Tuple<string, int>(messages[i].Mailbox, messages[i].ImapUid);
            }

            // Reset the preview viewport.
            ImapHeaders.BackColor = Color.White;
            ImapHeaders.Text = "";
            ImapWebPreview.DocumentText = "Please select a message from the left-hand panel.";
            ImapWebPreviewPanel.Refresh();
        }

        /// <summary>
        /// Attempt to retrieve up to 25 POP3 messages and populate the list of messages.
        /// </summary>
        private async Task RefreshPop3Messages()
        {
            // If we're not currently connected to the POP3 server, connect and authenticate.
            if (myPop3Client == null)
            {
                int pop3Port = 993;
                int.TryParse(Pop3Port.Text, out pop3Port);

                myPop3Client = new Pop3Client(Pop3Host.Text, pop3Port, Pop3Username.Text, Pop3Password.Text, Pop3Ssl.Checked);
            }

            // If our connection has timed out or been closed, reconnect.
            if (!myPop3Client.IsConnected || !myPop3Client.IsAuthenticated)
            {
                myPop3Client.Connect();
                myPop3Client.Authenticate();

                // Ensure we connected successfully.
                if (!myPop3Client.IsConnected || !myPop3Client.IsAuthenticated)
                {
                    MessageBox.Show("Unable to connect to the POP3 server. Please double-check your settings.", "Unable to connect.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            // Retrieve the headers of up to 25 messages and remember their mailbox/UID pairs for later opening.
            List<ReadOnlyMailMessage> messages = await myPop3Client.GetMessagesAsync(25, 1, true, true);
            Pop3MessageIDs = new List<int>();

            // Repopulate the message list with the subjects of messages retrieved.
            Pop3MessageList.Items.Clear();
            for (int i = 0; i < messages.Count; i++)
            {
                Pop3MessageList.Items.Add(messages[i].Subject);
                Pop3MessageIDs.Add(messages[i].Index);
            }

            // Reset the preview viewport.
            Pop3Headers.BackColor = Color.White;
            Pop3Headers.Text = "";
            Pop3WebPreview.DocumentText = "Please select a message from the left-hand panel.";
            Pop3WebPreviewPanel.Refresh();
        }

        /// <summary>
        /// Render the selected message's body and selected headers to the specified controls.
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <param name="headerTextBox">The textbox to populate with selected headers.</param>
        /// <param name="bodyWebBrowser">The web browser in which to render the body.</param>
        /// <param name="bodyWebBrowserPanel">The panel containing the web browser.</param>
        private void RenderMessage(ReadOnlyMailMessage message, TextBox headerTextBox, WebBrowser bodyWebBrowser, Panel bodyWebBrowserPanel)
        {
            StringBuilder headersText = new StringBuilder();

            // Output selected headers.
            headersText.Append("Date: " + message.Date.ToString() + "\r\n");
            headersText.Append("From: ");
            if (message.From != null)
                headersText.Append(Functions.ToMailAddressString(new MailAddressCollection() { message.From }));
            else if (message.Sender != null)
                headersText.Append(Functions.ToMailAddressString(new MailAddressCollection() { message.Sender }));
            headersText.Append("\r\n");
            if (message.To.Count > 0)
                headersText.Append("To: " + Functions.ToMailAddressString(message.To) + "\r\n");
            if (message.CC.Count > 0)
                headersText.Append("CC: " + Functions.ToMailAddressString(message.CC) + "\r\n");
            headersText.Append("Subject: " + message.Subject + "\r\n");
            headersText.Append("S/MIME Signed: " + message.SmimeSigned.ToString() + "\r\n");
            headersText.Append("S/MIME Envelope Encrypted: " + message.SmimeEncryptedEnvelope.ToString() + "\r\n");
            headersText.Append("S/MIME Triple Wrapped: " + message.SmimeTripleWrapped.ToString() + "\r\n");
            headersText.Append("Size: " + string.Format("{0:n0}", message.Size));
            if (message.RawFlags.Count > 0)
            {
                headersText.Append("\r\nFlags: ");
                bool firstFlag = true;
                foreach (string flag in message.RawFlags)
                {
                    if (!firstFlag)
                        headersText.Append("; ");
                    headersText.Append(flag);
                    firstFlag = false;
                }
            }
            if (message.Attachments.Count > 0)
            {
                headersText.Append("\r\nAttachments: ");
                for (int i = 0; i < message.Attachments.Count; i++)
                {
                    if (i > 0)
                        headersText.Append("; ");
                    headersText.Append(message.Attachments[i].Name + " (" + message.Attachments[i].ContentType.MediaType + ")");
                }
            }

            headerTextBox.Text = headersText.ToString();

            if (message.SmimeTripleWrapped)
                headerTextBox.BackColor = Color.LightGreen;
            else if (message.SmimeEncryptedEnvelope)
            {
                if (message.SmimeSigned)
                    headerTextBox.BackColor = Color.LightGreen;
                else
                    headerTextBox.BackColor = Color.GreenYellow;
            }
            else if (message.SmimeSigned)
                headerTextBox.BackColor = Color.LightBlue;
            else
                headerTextBox.BackColor = Color.White;

            if (message.IsBodyHtml)
                bodyWebBrowser.DocumentText = Functions.EmbedAttachments(Functions.RemoveScriptTags(message.Body), message.Attachments);
            else
                bodyWebBrowser.DocumentText = Functions.EmbedAttachments(Functions.ConvertPlainTextToHTML(Functions.RemoveScriptTags(message.Body)), null);
            bodyWebBrowserPanel.Refresh();
        }
        
        /// <summary>
        /// Resize and reposition controls in response to a window resize.
        /// </summary>
        private void ResizeForm()
        {
            TabsControl.Width = this.Width - 22;
            TabsControl.Height = this.Width - 68;

            LoadSettingsButton.Top = this.Height - 63;
            SaveSettingsButton.Top = this.Height - 63;
            SaveSettingsButton.Left = this.Width - 160;

            ImapSettingsGroup.Width = this.Width - 42;
            Pop3SettingsGroup.Width = this.Width - 42;
            SmtpSettingsGroup.Width = this.Width - 42;

            ImapTestGroup.Width = this.Width - 42;
            ImapMessageGroup.Height = this.Height - 156;
            ImapMessageList.Height = this.Height - 209;
            ImapDeleteMessageButton.Top = this.Height - 188;
            ImapHeaders.Width = this.Width - 246;
            ImapPreviewGroup.Width = this.Width - 232;
            ImapPreviewGroup.Height = this.Height - 156;
            ImapWebPreview.Width = this.Width - 246;
            ImapWebPreview.Height = this.Width - 326;
            ImapWebPreviewPanel.Width = this.Width - 246;
            ImapWebPreviewPanel.Height = this.Height - 331;
            ImapWebPreviewPanel.Refresh();

            Pop3TestGroup.Width = this.Width - 42;
            Pop3MessageGroup.Height = this.Height - 156;
            Pop3MessageList.Height = this.Height - 209;
            Pop3DeleteMessageButton.Top = this.Height - 188;
            Pop3Headers.Width = this.Width - 246;
            Pop3PreviewGroup.Width = this.Width - 232;
            Pop3PreviewGroup.Height = this.Height - 156;
            Pop3WebPreview.Width = this.Width - 246;
            Pop3WebPreview.Height = this.Width - 326;
            Pop3WebPreviewPanel.Width = this.Width - 246;
            Pop3WebPreviewPanel.Height = this.Height - 331;
            Pop3WebPreviewPanel.Refresh();

            SmtpTestGroup.Width = this.Width - 42;
            SmtpTestSettingsGroup.Width = this.Width - 42;
            SmtpTestSettingsGroup.Height = this.Height - 153;
            SmtpFrom.Width = this.Width - 94;
            SmtpTo.Width = this.Width - 94;
            SmtpCC.Width = this.Width - 94;
            SmtpBcc.Width = this.Width - 94;
            SmtpSubject.Width = this.Width - 94;
            SmtpBody.Width = this.Width - 94;
            SmtpBody.Height = this.Height - 408;
            SmtpAttachmentsLabel.Top = this.Height - 248;
            SmtpAttachments.Top = this.Height - 248;
            SmtpAttachments.Width = this.Width - 129;
            SmtpSmimeLabel.Top = this.Height - 199;
            SmtpSmimeSerialNumber.Top = this.Height - 199;
            SmtpSmimeSerialNumber.Width = this.Width - 129;
            SmtpSmimeSign.Top = this.Height - 176;
            SmtpSmimeEncrypt.Top = this.Height - 176;
            SmtpSmimeTripleWrap.Top = this.Height - 176;
            SmtpIsHtml.Top = this.Height - 176;
        }

        /// <summary>
        /// Save all current Test Client settings to an XML file.
        /// </summary>
        private void SaveSettings()
        {
            DialogResult result = MessageBox.Show("Are you sure?  Settings (including passwords) will be saved as plaintext into \"OpaqueMail.TestClient.xml\" on your Desktop.", "Confirm Save", MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
            if (result == DialogResult.OK)
            {
                XmlWriterSettings streamWriterSettings = new XmlWriterSettings();
                streamWriterSettings.Indent = true;
                streamWriterSettings.IndentChars = "  ";
                streamWriterSettings.NewLineChars = "\r\n";
                streamWriterSettings.NewLineHandling = NewLineHandling.Replace;

                using (XmlWriter streamWriter = XmlWriter.Create(GetSettingsFileName(), streamWriterSettings))
                {
                    streamWriter.WriteStartDocument();
                    streamWriter.WriteStartElement("Settings");

                    streamWriter.WriteStartElement("IMAP");

                    streamWriter.WriteElementString("Host", ImapHost.Text.ToString());
                    streamWriter.WriteElementString("Port", ImapPort.Text.ToString());
                    streamWriter.WriteElementString("Username", ImapUsername.Text.ToString());
                    streamWriter.WriteElementString("Password", ImapPassword.Text.ToString());
                    streamWriter.WriteElementString("SSL", ImapSsl.Checked.ToString());

                    streamWriter.WriteEndElement();

                    streamWriter.WriteStartElement("POP3");

                    streamWriter.WriteElementString("Host", Pop3Host.Text.ToString());
                    streamWriter.WriteElementString("Port", Pop3Port.Text.ToString());
                    streamWriter.WriteElementString("Username", Pop3Username.Text.ToString());
                    streamWriter.WriteElementString("Password", Pop3Password.Text.ToString());
                    streamWriter.WriteElementString("SSL", Pop3Ssl.Checked.ToString());

                    streamWriter.WriteEndElement();

                    streamWriter.WriteStartElement("SMTP");

                    streamWriter.WriteElementString("Host", SmtpHost.Text.ToString());
                    streamWriter.WriteElementString("Port", SmtpPort.Text.ToString());
                    streamWriter.WriteElementString("Username", SmtpUsername.Text.ToString());
                    streamWriter.WriteElementString("Password", SmtpPassword.Text.ToString());
                    streamWriter.WriteElementString("SSL", SmtpSsl.Checked.ToString());

                    streamWriter.WriteElementString("From", SmtpFrom.Text.ToString());
                    streamWriter.WriteElementString("To", SmtpTo.Text.ToString());
                    streamWriter.WriteElementString("CC", SmtpCC.Text.ToString());
                    streamWriter.WriteElementString("BCC", SmtpBcc.Text.ToString());
                    streamWriter.WriteElementString("Body", SmtpBody.Text.ToString());
                    streamWriter.WriteElementString("Subject", SmtpSubject.Text.ToString());
                    streamWriter.WriteElementString("Attachments", SmtpAttachments.Text.ToString());
                    streamWriter.WriteElementString("SMIMESign", SmtpSmimeSign.Checked.ToString());
                    streamWriter.WriteElementString("SMIMESerialNumber", SmtpSmimeSerialNumber.Text.ToString());
                    streamWriter.WriteElementString("SMIMEEncrypt", SmtpSmimeEncrypt.Checked.ToString());
                    streamWriter.WriteElementString("SMIMETripleWrap", SmtpSmimeTripleWrap.Checked.ToString());
                    streamWriter.WriteElementString("IsHTML", SmtpIsHtml.Checked.ToString());

                    streamWriter.WriteEndElement();

                    streamWriter.WriteStartElement("Window");
                    streamWriter.WriteElementString("State", WindowState.ToString());
                    streamWriter.WriteElementString("Width", Width.ToString());
                    streamWriter.WriteElementString("Height", Height.ToString());
                    streamWriter.WriteElementString("Left", Left.ToString());
                    streamWriter.WriteElementString("Top", Top.ToString());
                    streamWriter.WriteElementString("SelectedTab", TabsControl.SelectedIndex.ToString());

                    streamWriter.WriteEndElement();

                    streamWriter.WriteEndElement();
                    streamWriter.WriteEndDocument();
                }
            }
        }

        /// <summary>
        /// Set a checkbox's value based on its saved XML setting.
        /// </summary>
        private void SetCheckBoxValue(XPathNavigator navigator, CheckBox checkbox, string xpathExpression)
        {
            XPathNavigator valueNavigator = navigator.SelectSingleNode(xpathExpression);
            if (valueNavigator != null)
            {
                bool sslChecked = true;
                if (bool.TryParse(valueNavigator.Value, out sslChecked))
                    checkbox.Checked = sslChecked;
            }
        }

        /// <summary>
        /// Set a textbox's value based on its saved XML setting.
        /// </summary>
        private void SetTextBoxValue(XPathNavigator navigator, TextBox textbox, string xpathExpression)
        {
            XPathNavigator valueNavigator = navigator.SelectSingleNode(xpathExpression);
            if (valueNavigator != null)
                textbox.Text = valueNavigator.Value;
        }
        #endregion Private Methods
    }
}
