using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using ASC.Mail.Aggregator.Common;
using ASC.Mail.Aggregator.Common.Authorization;
using ASC.Mail.Aggregator.Common.Clients;
using ASC.Mail.Aggregator.Common.Extension;
using ASC.Mail.Aggregator.Common.Imap;
using ASC.Mail.Aggregator.Common.Logging;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Pop3;
using MailKit.Search;
using MailKit.Security;
using MimeKit;
using EncryptionType = ASC.Mail.Aggregator.Common.EncryptionType;
using MailFolder = ASC.Mail.Aggregator.Common.MailFolder;
using Pop3Client = MailKit.Net.Pop3.Pop3Client;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace ASC.Mail.Aggregator.Core.Clients
{
    public class MailClient : IDisposable
    {
        public MailBox Account { get; private set; }
        public ILogger Log { get; private set; }

        public ImapClient Imap { get; private set; }
        public Pop3Client Pop { get; private set; }
        public SmtpClient Smtp { get; private set; }

        private CancellationToken CancelToken { get; set; }
        private CancellationTokenSource StopTokenSource { get; set; }

        /// <summary>
        /// Occurs when the client has been successfully authenticated.
        /// </summary>
        /// <remarks>
        /// The <see cref="E:MailClientBase.Authenticated" /> event is raised whenever the client
        /// has been authenticated.
        /// </remarks>
        public event EventHandler<MailClientEventArgs> Authenticated;

        /// <summary>
        /// Occurs when the client has been successfully loaded message.
        /// </summary>
        /// <remarks>
        /// The <see cref="E:MailClientBase.GetMessage" /> event is raised whenever the client
        /// has been loaded message.
        /// </remarks>
        public event EventHandler<MailClientMessageEventArgs> GetMessage;

        /// <summary>
        /// Occurs when the client has been successfully sent message.
        /// </summary>
        /// <remarks>
        /// The <see cref="E:MailClientBase.SendMessage" /> event is raised whenever the client
        /// has been sent message.
        /// </remarks>
        public event EventHandler<MailClientEventArgs> SendMessage;

        protected virtual void OnAuthenticated(string message)
        {
            var eventHandler = Authenticated;
            if (eventHandler != null)
                eventHandler.Invoke(this, new MailClientEventArgs(message, Account));
        }

        protected virtual void OnGetMessage(MimeMessage message, string messageUid, bool unread, MailFolder folder)
        {
            var eventHandler = GetMessage;
            if (eventHandler != null)
                eventHandler.Invoke(this,
                    new MailClientMessageEventArgs(message, messageUid, unread, folder, Account, Log));
        }

        protected virtual void OnSentMessage(string message)
        {
            var eventHandler = SendMessage;
            if (eventHandler != null)
                eventHandler.Invoke(this, new MailClientEventArgs(message, Account));
        }

        #region .Public

        #region .Constructor

        public MailClient(MailBox mailbox, CancellationToken cancelToken, int tcpTimeout = 30000, bool certificatePermit = false,
            ILogger log = null)
        {
            Account = mailbox;
            Log = log ?? new NullLogger();

            StopTokenSource = new CancellationTokenSource();

            CancelToken = CancellationTokenSource.CreateLinkedTokenSource(cancelToken, StopTokenSource.Token).Token;

            if (Account.Imap)
            {
                Imap = new ImapClient
                {
                    ServerCertificateValidationCallback = (sender, certificate, chain, errors) =>
                        certificatePermit ||
                        MailService.DefaultServerCertificateValidationCallback(sender, certificate, chain, errors),
                    Timeout = tcpTimeout
                };

                Pop = null;
            }
            else
            {
                Pop = new Pop3Client
                {
                    ServerCertificateValidationCallback = (sender, certificate, chain, errors) =>
                        certificatePermit ||
                        MailService.DefaultServerCertificateValidationCallback(sender, certificate, chain, errors),
                    Timeout = tcpTimeout
                };
                Imap = null;
            }

            Smtp = new SmtpClient
            {
                ServerCertificateValidationCallback = (sender, certificate, chain, errors) =>
                    certificatePermit ||
                    MailService.DefaultServerCertificateValidationCallback(sender, certificate, chain, errors),
                Timeout = tcpTimeout
            };
        }

        #endregion

        public MimeMessage GetInboxMessage(string uidl)
        {
            if (string.IsNullOrEmpty(uidl))
                throw new ArgumentNullException("uidl");

            if (Account.Imap)
            {
                if (!Imap.IsAuthenticated)
                    LoginImap();

                var elements = uidl.Split('-');

                var index = Convert.ToInt32(elements[0]);
                var folderId = Convert.ToInt32(elements[1]);

                if (folderId != MailFolder.Ids.inbox)
                    throw new ArgumentException("uidl is invalid. Only INBOX folder is supported.");

                var inbox = Imap.Inbox;

                inbox.Open(FolderAccess.ReadOnly);

                var allUids = (Imap.Capabilities & ImapCapabilities.ESearch) != 0
                    ? inbox.Search(SearchOptions.All, SearchQuery.All, CancelToken).UniqueIds
                    : inbox.Fetch(0, -1, MessageSummaryItems.UniqueId, CancelToken).Select(r => r.UniqueId).ToList();

                var uid = allUids.FirstOrDefault(u => u.Id == index);

                if (!uid.IsValid)
                    throw new Exception("IMAP4 uidl not found");

                return Imap.Inbox.GetMessage(uid, CancelToken);
            }
            else
            {
                if (!Pop.IsAuthenticated)
                    LoginPop3();

                var i = 0;
                var uidls =
                    Pop.GetMessageUids(CancelToken)
                        .Select(u => new KeyValuePair<int, string>(i++, u))
                        .ToDictionary(t => t.Key, t => t.Value);

                var uid = uidls.FirstOrDefault(u => u.Value.Equals(uidl, StringComparison.OrdinalIgnoreCase));

                if (uid.Value == null)
                    throw new Exception("POP3 uidl not found");

                return Pop.GetMessage(uid.Key, CancelToken);
            }
        }

        public void Aggregate(TasksConfig tasksConfig, int limitMessages = -1)
        {
            if (Account.Imap)
                AggregateImap(tasksConfig, limitMessages);
            else
                AggregatePop3(tasksConfig, limitMessages);
        }

        public void Send(MimeMessage message, bool needCopyToSentFolder = true)
        {
            if (!Smtp.IsConnected)
                LoginSmtp();

            Smtp.Send(message, CancelToken);

            if (!Account.Imap || !needCopyToSentFolder)
                return;

            AppendCopyToSenFolder(message);
        }

        public void Cancel()
        {
            Log.Info("MailClient->Cancel()");
            StopTokenSource.Cancel();
        }

        public void Dispose()
        {
            if (Imap != null)
            {
                if (Imap.IsConnected)
                    Imap.Disconnect(true, CancelToken);

                Imap.Dispose();
            }

            if (Pop != null)
            {
                if (Pop.IsConnected)
                    Pop.Disconnect(true, CancelToken);

                Pop.Dispose();
            }

            if (Smtp != null)
            {
                if (Smtp.IsConnected)
                    Smtp.Disconnect(true, CancelToken);

                Smtp.Dispose();
            }

            Authenticated = null;
            SendMessage = null;
            GetMessage = null;

            StopTokenSource.Dispose();
        }

        public LoginResult TestLogin()
        {
            var result = new LoginResult
            {
                Imap = Account.Imap
            };

            try
            {
                if (Account.Imap)
                {
                    if (!Imap.IsAuthenticated)
                        LoginImap(false);
                }
                else
                {
                    if (!Pop.IsAuthenticated)
                        LoginPop3(false);
                }

                result.IngoingSuccess = true;
            }
            catch (Exception inEx)
            {
                result.IngoingSuccess = false;
                result.IngoingException = inEx;
            }

            try
            {
                if (!Smtp.IsAuthenticated)
                    LoginSmtp();

                result.OutgoingSuccess = true;
            }
            catch (Exception outEx)
            {
                result.OutgoingSuccess = false;
                result.OutgoingException = outEx;
            }

            return result;
        }

        public static MimeMessage ParseMimeMessage(Stream emlStream)
        {
           var options = new ParserOptions
            {
                AddressParserComplianceMode = RfcComplianceMode.Loose,
                ParameterComplianceMode = RfcComplianceMode.Loose,
                Rfc2047ComplianceMode = RfcComplianceMode.Loose,
                CharsetEncoding = Encoding.UTF8,
                RespectContentLength = false
            };

            var msg = MimeMessage.Load(options, emlStream);

            msg.FixEncodingIssues();

            return msg;
        }

        public static MimeMessage ParseMimeMessage(string emlPath)
        {
            using (var fs = new FileStream(emlPath, FileMode.Open, FileAccess.Read))
            {
                return ParseMimeMessage(fs);
            }
        }

        #endregion

        #region .Private

        #region .IMAP

        private void LoginImap(bool enableUtf8 = true)
        {
            Imap.Authenticated += (sender, args) => OnAuthenticated(args.Message);

            var secureSocketOptions = SecureSocketOptions.Auto;

            switch (Account.IncomingEncryptionType)
            {
                case EncryptionType.StartTLS:
                    secureSocketOptions = SecureSocketOptions.StartTlsWhenAvailable;
                    break;
                case EncryptionType.SSL:
                    secureSocketOptions = SecureSocketOptions.SslOnConnect;
                    break;
                case EncryptionType.None:
                    secureSocketOptions = SecureSocketOptions.None;
                    break;
            }

            Log.Debug("Imap.Connect({0}:{1}, {2})", Account.Server, Account.Port, Enum.GetName(typeof(SecureSocketOptions), secureSocketOptions));

            Imap.Connect(Account.Server, Account.Port, secureSocketOptions, CancelToken);

            if (enableUtf8 && (Imap.Capabilities & ImapCapabilities.UTF8Accept) != ImapCapabilities.None)
                Imap.EnableUTF8(CancelToken);

            if (string.IsNullOrEmpty(Account.RefreshToken))
            {
                Imap.AuthenticationMechanisms.Remove("XOAUTH2");
                Imap.Authenticate(Account.Account, Account.Password, CancelToken);
            }
            else
            {
                Imap.Authenticate(Account.Account, GetOauthAccessToken(Account.RefreshToken), CancelToken);
            }
        }

        private void AggregateImap(TasksConfig tasksConfig, int limitMessages = -1)
        {
            if (!Imap.IsAuthenticated)
                LoginImap();

            var loaded = 0;

            var folders = GetImapFolders();

            foreach (var folder in folders)
            {
                if (!Imap.IsConnected || CancelToken.IsCancellationRequested)
                    return;

                var mailFolder = DetectFolder(tasksConfig, folder);

                if (mailFolder == null)
                {
                    Log.Info("[folder] x '{0}' (skipped)", folder.Name);
                    continue;
                }

                Log.Info("[folder] >> '{0}' (fId={1})", folder.Name, mailFolder.FolderId);

                try
                {
                    folder.Open(FolderAccess.ReadOnly, CancelToken);
                }
                catch (Exception e)
                {
                    Log.Warn("Open faild: {0} Exception: {1}", folder.Name, e.Message);
                    continue;
                }

                loaded += LoadFolderMessages(folder, mailFolder, limitMessages);

                if (limitMessages <= 0 || loaded < limitMessages)
                    continue;

                Log.Debug("Limit of maximum number messages per session is exceeded!");
                break;
            }
        }

        private static bool CompareFolders(IMailFolder f1, IMailFolder f2)
        {
            return (f1.Attributes.HasFlag(FolderAttributes.Inbox) || 
                    f1.Name.Equals("inbox", StringComparison.InvariantCultureIgnoreCase) ||
                    f1.Attributes.HasFlag(FolderAttributes.Sent) &&
                    !f2.Attributes.HasFlag(FolderAttributes.Inbox) ||
                    f1.Attributes.HasFlag(FolderAttributes.Junk) &&
                    !f2.Attributes.HasFlag(FolderAttributes.Inbox) &&
                    !f2.Attributes.HasFlag(FolderAttributes.Sent));
        }

        private IEnumerable<IMailFolder> GetImapFolders()
        {
            Log.Debug("GetImapFolders()");

            var personal = Imap.GetFolders(Imap.PersonalNamespaces[0], true, CancelToken).ToList();

            if (!personal.Any(mb => mb.Name.Equals("inbox", StringComparison.InvariantCultureIgnoreCase)))
                personal.Add(Imap.Inbox);

            var folders = new List<IMailFolder>(personal);

            foreach (var folder in personal)
                folders.AddRange(GetImapSubFolders(folder));

            folders = folders.Distinct().ToList();

            if (folders.Count <= 1)
                return folders;

            folders.Sort((f1, f2) => CompareFolders(f1, f2) ? -1 : CompareFolders(f2, f1) ? 1 : 0);

            return folders;
        }

        private IEnumerable<IMailFolder> GetImapSubFolders(IMailFolder folder)
        {
            try
            {
                var subfolders = folder.GetSubfolders(true, CancelToken).ToList();

                if (!subfolders.Any())
                {
                    return subfolders;
                }

                var tempList = new List<IMailFolder>();

                foreach (var subfolder in subfolders)
                {
                    try
                    {
                        tempList.AddRange(GetImapSubFolders(subfolder));
                    }
                    catch
                    {
                        //Skip 
                    }
                }

                subfolders.AddRange(tempList);

                return subfolders;
            }
            catch
            {
               //Skip 
            }

            return new List<IMailFolder>();
        }

        private int LoadFolderMessages(IMailFolder folder, MailFolder mailFolder, int limitMessages)
        {
            var loaded = 0;

            ImapFolderUids folderUids;
            if (!Account.ImapIntervals.TryGetValue(folder.Name, out folderUids))
            {
                folderUids = new ImapFolderUids(new List<int> { 1, int.MaxValue }, 1);
                // by default - mailbox never was processed before
            }

            var imapIntervals = new ImapIntervals(folderUids.UnhandledUidIntervals);
            var beginDateUid = folderUids.BeginDateUid;

            IList<UniqueId> allUids;

            try
            {
                allUids = folder.Fetch(0, -1, MessageSummaryItems.UniqueId, CancelToken).Select(r => r.UniqueId).ToList();
            }
            catch (NotSupportedException)
            {
                const int start = 0;
                var end = folder.Count;
                const int increment = 1;

                allUids = Enumerable
                    .Repeat(start, (end - start) / 1 + 1)
                    .Select((tr, ti) => tr + increment * ti)
                    .Select(n => new UniqueId((uint)n))
                    .ToList();
            }

            foreach (var uidsInterval in imapIntervals.GetUnhandledIntervalsCopy())
            {
                var interval = uidsInterval;
                var uidsCollection =
                    allUids.Select(u => u)
                        .Where(u => u.Id <= interval.To && u.Id >= interval.From)
                        .OrderByDescending(x => x)
                        .ToList();

                if (!uidsCollection.Any())
                {
                    if (!uidsInterval.IsToUidMax())
                        imapIntervals.AddHandledInterval(uidsInterval);
                    continue;
                }

                var first = uidsCollection.First().Id;
                var toUid = (int)(uidsInterval.IsToUidMax()
                    ? first
                    : Math.Max(uidsInterval.To, first));

                var infoList =
                    folder.Fetch(limitMessages > 0 ? uidsCollection.Take(limitMessages * 3).ToList() : uidsCollection,
                        MessageSummaryItems.Flags | MessageSummaryItems.GMailLabels | MessageSummaryItems.InternalDate);

                foreach (var uid in uidsCollection)
                {
                    try
                    {
                        if (!Imap.IsConnected || CancelToken.IsCancellationRequested)
                        {
                            break;
                        }

                        var message = folder.GetMessage(uid, CancelToken);

                        var uid1 = uid;
                        var info = infoList.FirstOrDefault(t => t.UniqueId == uid1);

                        message.FixDateIssues(info != null ? info.InternalDate : null, Log);

                        if (message.Date < Account.BeginDate)
                        {
                            Log.Debug("Skip message (Date = {0}) on BeginDate = {1}", message.Date,
                                Account.BeginDate);
                            imapIntervals.SetBeginIndex(toUid);
                            beginDateUid = toUid;
                            break;
                        }

                        var unread = info != null &&
                                     (info.UserFlags.Contains("\\Unseen") ||
                                      info.Flags.HasValue && !info.Flags.Value.HasFlag(MessageFlags.Seen));

                        message.FixEncodingIssues(Log);

                        OnGetMessage(message, uid.Id.ToString(), unread, mailFolder);

                        loaded++;
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception e)
                    {
                        Log.Error(
                            "ProcessMessages() Tenant={0} User='{1}' Account='{2}', MailboxId={3}, UID={4} Exception:\r\n{5}\r\n",
                            Account.TenantId, Account.UserId, Account.EMail.Address, Account.MailBoxId,
                            uid, e);

                        if (uid != uidsCollection.First() && (int)uid.Id != toUid)
                        {
                            imapIntervals.AddHandledInterval(new UidInterval((int)uid.Id + 1, toUid));
                        }
                        toUid = (int)uid.Id - 1;

                        if (e is IOException)
                        {
                            break; // stop checking other mailboxes
                        }

                        continue;
                    }

                    // after successfully message saving - lets update imap intervals state
                    imapIntervals.AddHandledInterval(
                        new UidInterval(
                            uid.Id == uidsCollection.Last().Id && uidsInterval.IsFromUidMin()
                                ? uidsInterval.From
                                : (int)uid.Id, toUid));

                    toUid = (int)uid.Id - 1;

                    if (limitMessages > 0 && loaded >= limitMessages)
                    {
                        break;
                    }
                }

                if (CancelToken.IsCancellationRequested || limitMessages > 0 && loaded >= limitMessages)
                {
                    break;
                }
            }

            var updatedImapFolderUids = new ImapFolderUids(imapIntervals.ToIndexes(), beginDateUid);

            if (!Account.ImapIntervals.Keys.Contains(folder.Name))
            {
                Account.ImapFolderChanged = true;
                Account.ImapIntervals.Add(folder.Name, updatedImapFolderUids);
            }
            else if (Account.ImapIntervals[folder.Name] != updatedImapFolderUids)
            {
                Account.ImapFolderChanged = true;
                Account.ImapIntervals[folder.Name] = updatedImapFolderUids;
            }

            return loaded;
        }

        private MailFolder DetectFolder(TasksConfig tasksConfig, IMailFolder folder)
        {
            var folderName = folder.Name.ToLowerInvariant();

            if (tasksConfig.SkipImapFlags != null &&
                tasksConfig.SkipImapFlags.Any() &&
                tasksConfig.SkipImapFlags.Contains(folderName))
            {
                return null;
            }

            int folderId;

            if ((folder.Attributes & FolderAttributes.Inbox) != 0)
            {
                return new MailFolder(MailFolder.Ids.inbox, folder.Name);
            }
            if ((folder.Attributes & FolderAttributes.Sent) != 0)
            {
                return new MailFolder(MailFolder.Ids.sent, folder.Name);
            }
            if ((folder.Attributes & FolderAttributes.Junk) != 0)
            {
                return new MailFolder(MailFolder.Ids.spam, folder.Name);
            }
            if ((folder.Attributes &
                 (FolderAttributes.All |
                  FolderAttributes.NoSelect |
                  FolderAttributes.NoInferiors |
                  FolderAttributes.NonExistent |
                  FolderAttributes.Unmarked |
                  FolderAttributes.Trash |
                  FolderAttributes.Archive |
                  FolderAttributes.Drafts |
                  FolderAttributes.Flagged |
                  FolderAttributes.Marked)) != 0)
            {
                return null; // Skip folders
            }

            if (tasksConfig.ImapFlags != null &&
                tasksConfig.ImapFlags.Any() &&
                tasksConfig.ImapFlags.ContainsKey(folderName))
            {
                folderId = tasksConfig.ImapFlags[folderName];
                return new MailFolder(folderId, folder.Name);
            }

            if (tasksConfig.SpecialDomainFolders.Any() &&
                tasksConfig.SpecialDomainFolders.ContainsKey(Account.Server))
            {
                var domainSpecialFolders = tasksConfig.SpecialDomainFolders[Account.Server];

                if (domainSpecialFolders.Any() &&
                    domainSpecialFolders.ContainsKey(folderName))
                {
                    var info = domainSpecialFolders[folderName];
                    return info.skip ? null : new MailFolder(info.folder_id, folder.Name);
                }
            }

            if (tasksConfig.DefaultFolders == null || !tasksConfig.DefaultFolders.ContainsKey(folderName))
                return new MailFolder(MailFolder.Ids.inbox, folder.Name, new[] {folder.FullName});

            folderId = tasksConfig.DefaultFolders[folderName];
            return new MailFolder(folderId, folder.Name);
        }

        private void AppendCopyToSenFolder(MimeMessage message)
        {
            if (!Account.Imap)
                throw new NotSupportedException("Only Imap is suppoted");

            if (message == null)
                throw new ArgumentNullException("message");

            try
            {
                if (!Imap.IsAuthenticated)
                    LoginImap();

                var folders = Imap.GetFolders(Imap.PersonalNamespaces[0], true, CancelToken).ToList();

                var sendFolder = folders.FirstOrDefault(f => (f.Attributes & FolderAttributes.Sent) != 0);

                if (sendFolder != null)
                {
                    sendFolder.Open(FolderAccess.ReadWrite);
                    var uid = sendFolder.Append(FormatOptions.Default, message, MessageFlags.Seen, CancelToken);

                    if (uid.HasValue)
                    {
                        Log.Info("AppendCopyToSenFolder(Mailbox: '{0}', Tenant: {1}, User: '{2}') succeed! (uid:{3})",
                            Account.EMail.Address, Account.TenantId, Account.UserId, uid.Value.Id);
                    }
                    else
                    {
                        Log.Error("AppendCopyToSenFolder(Mailbox: '{0}', Tenant: {1}, User: '{2}') failed!",
                            Account.EMail.Address, Account.TenantId, Account.UserId);
                    }
                }
                else
                {
                    Log.Debug(
                        "AppendCopyToSenFolder(Mailbox: '{0}', Tenant: {1}, User: '{2}'): Skip - sent-folder not found",
                        Account.EMail.Address, Account.TenantId, Account.UserId);
                }
            }
            catch (Exception ex)
            {
                Log.Error("AppendCopyToSenFolder(Mailbox: '{0}', Tenant: {1}, User: '{2}'): Exception:\r\n{3}\r\n",
                    Account.EMail.Address, Account.TenantId, Account.UserId, ex.ToString());
            }
        }

        #endregion

        #region .POP3

        private void LoginPop3(bool enableUtf8 = true)
        {
            Pop.Authenticated += (sender, args) => OnAuthenticated(args.Message);

            var secureSocketOptions = SecureSocketOptions.Auto;

            switch (Account.IncomingEncryptionType)
            {
                case EncryptionType.StartTLS:
                    secureSocketOptions = SecureSocketOptions.StartTlsWhenAvailable;
                    break;
                case EncryptionType.SSL:
                    secureSocketOptions = SecureSocketOptions.SslOnConnect;
                    break;
                case EncryptionType.None:
                    secureSocketOptions = SecureSocketOptions.None;
                    break;
            }

            Log.Debug("Pop.Connect({0}:{1}, {2})", Account.Server, Account.Port, Enum.GetName(typeof(SecureSocketOptions), secureSocketOptions));

            Pop.Connect(Account.Server, Account.Port, secureSocketOptions, CancelToken);

            if (enableUtf8 && (Pop.Capabilities & Pop3Capabilities.UTF8) != Pop3Capabilities.None)
                Pop.EnableUTF8(CancelToken);

            if (string.IsNullOrEmpty(Account.RefreshToken))
            {
                Pop.AuthenticationMechanisms.Remove("XOAUTH2");
                Pop.Authenticate(Account.Account, Account.Password, CancelToken);
            }
            else
            {
                Pop.Authenticate(Account.Account, GetOauthAccessToken(Account.RefreshToken), CancelToken);
            }
        }

        private void AggregatePop3(TasksConfig tasksConfig, int limitMessages = -1)
        {
            if (!Pop.IsAuthenticated)
                LoginPop3();

            var mailFolder = new MailFolder(MailFolder.Ids.inbox, "INBOX");

            var loaded = 0;

            var newMessages = GetPop3NewMessagesIDs(tasksConfig);

            if (newMessages.Count == 0)
            {
                Log.Debug("New messages not found.\r\n");
                return;
            }

            Log.Debug("Found {0} new messages.\r\n", newMessages.Count);

            try
            {
                var fstMailHeaders = Pop.GetMessageHeaders(newMessages.First().Key, CancelToken).ToList();
                var lstMailHeaders = Pop.GetMessageHeaders(newMessages.Last().Key, CancelToken).ToList();
                var fstDateHeader =
                    fstMailHeaders.FirstOrDefault(
                        h => h.Field.Equals("Date", StringComparison.InvariantCultureIgnoreCase));
                var lstDateHeader =
                    lstMailHeaders.FirstOrDefault(
                        h => h.Field.Equals("Date", StringComparison.InvariantCultureIgnoreCase));

                var found = false;

                if (fstDateHeader != null && lstDateHeader != null)
                {
                    DateTime fstDate;
                    DateTime lstDate;
                    if (DateTime.TryParse(fstDateHeader.Value, out fstDate) &&
                        DateTime.TryParse(lstDateHeader.Value, out lstDate))
                    {
                        found = true;
                        if (fstDate < lstDate)
                        {
                            Log.Debug("Account '{0}' order is DESC", Account.EMail.Address);
                            newMessages = newMessages
                                .OrderByDescending(item => item.Key)
                                // This is to ensure that the newest message would be handled primarily.
                                .ToDictionary(id => id.Key, id => id.Value);
                        }
                    }
                }

                if (!found)
                {
                    Log.Debug("Account '{0}' order is ASC", Account.EMail.Address);
                }
            }
            catch (Exception)
            {
                Log.Warn("Calculating order skipped! Account '{0}' order is ASC", Account.EMail.Address);
            }

            var skipOnDate = Account.BeginDate != MailBoxManager.MinBeginDate;

            var skipBreakOnDate = tasksConfig.PopUnorderedDomains.Contains(Account.Server.ToLowerInvariant());

            foreach (var newMessage in newMessages)
            {
                if (!Pop.IsConnected || CancelToken.IsCancellationRequested)
                    break;

                Log.Debug("Processing new message\tUID: {0}\tUIDL: {1}\t",
                    newMessage.Key,
                    newMessage.Value);

                try
                {
                    var message = Pop.GetMessage(newMessage.Key, CancelToken);

                    message.FixDateIssues(logger: Log);

                    if (message.Date < Account.BeginDate && skipOnDate)
                    {
                        if (!skipBreakOnDate)
                        {
                            Log.Info("Skip other messages older then {0}.", Account.BeginDate);
                            break;
                        }
                        Log.Debug("Skip message (Date = {0}) on BeginDate = {1}", message.Date,
                            Account.BeginDate);
                        continue;
                    }

                    message.FixEncodingIssues();

                    OnGetMessage(message, newMessage.Value, true, mailFolder);

                    loaded++;

                    if (limitMessages <= 0 || loaded < limitMessages)
                        continue;

                    Log.Debug("Limit of max messages per session is exceeded!");
                    break;
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception e)
                {
                    Log.Error(
                        "ProcessMessages() Tenant={0} User='{1}' Account='{2}', MailboxId={3}, MessageIndex={4}, UIDL='{5}' Exception:\r\n{6}\r\n",
                        Account.TenantId, Account.UserId, Account.EMail.Address, Account.MailBoxId,
                        newMessage.Key, newMessage.Value, e);

                    if (e is IOException)
                    {
                        break; // stop checking other mailboxes
                    }
                }
            }
        }

        private Dictionary<int, string> GetPop3NewMessagesIDs(TasksConfig tasksConfig)
        {
            var newMessages = new Dictionary<int, string>();

            var j = 0;
            var uidls =
                Pop.GetMessageUids(CancelToken)
                    .Select(uidl => new KeyValuePair<int, string>(j++, uidl))
                    .ToDictionary(t => t.Key, t => t.Value);

            if (!uidls.Any() || uidls.Count == Account.MessagesCount)
                return newMessages;

            var i = 0;
            var chunk = tasksConfig.ChunkOfPop3Uidl;

            var chunkUidls = uidls.Skip(i).Take(chunk).ToList();

            var manager = new MailBoxManager();

            do
            {
                var checkList = chunkUidls.Select(u => u.Value).Distinct().ToList();

                var existingUidls = manager.CheckUidlExistance(Account.MailBoxId, checkList);

                if (!existingUidls.Any())
                {
                    var messages = newMessages;
                    foreach (var item in
                        chunkUidls.Select(uidl => new KeyValuePair<int, string>(uidl.Key, uidl.Value))
                            .Where(item => !messages.Contains(item)))
                    {
                        newMessages.Add(item.Key, item.Value);
                    }
                }
                else if (existingUidls.Count != chunkUidls.Count)
                {
                    var messages = newMessages;
                    foreach (var item in (from uidl in chunkUidls
                        where !existingUidls.Contains(uidl.Value)
                        select new KeyValuePair<int, string>(uidl.Key, uidl.Value)).Where(
                            item => !messages.Contains(item)))
                    {
                        newMessages.Add(item.Key, item.Value);
                    }
                }

                i += chunk;

                chunkUidls = uidls.Skip(i).Take(chunk).ToList();
            } while (chunkUidls.Any());

            newMessages = newMessages
                                .OrderByDescending(item => item.Key)
                                // This is to ensure that the newest message would be handled primarily.
                                .ToDictionary(id => id.Key, id => id.Value);

            return newMessages;
        }

        #endregion

        #region .SMTP

        private void LoginSmtp()
        {
            Smtp.Authenticated += (sender, args) => OnAuthenticated(args.Message);

            var secureSocketOptions = SecureSocketOptions.Auto;

            switch (Account.IncomingEncryptionType)
            {
                case EncryptionType.StartTLS:
                    secureSocketOptions = SecureSocketOptions.StartTlsWhenAvailable;
                    break;
                case EncryptionType.SSL:
                    secureSocketOptions = SecureSocketOptions.SslOnConnect;
                    break;
                case EncryptionType.None:
                    secureSocketOptions = SecureSocketOptions.None;
                    break;
            }

            Log.Debug("Smtp.Connect({0}:{1}, {2})", Account.Server, Account.Port, Enum.GetName(typeof(SecureSocketOptions), secureSocketOptions));

            Smtp.Connect(Account.SmtpServer, Account.SmtpPort,
                Account.OutcomingEncryptionType == EncryptionType.StartTLS ||
                Account.OutcomingEncryptionType == EncryptionType.SSL
                    ? SecureSocketOptions.Auto
                    : SecureSocketOptions.None, CancelToken);

            if (!Account.SmtpAuth)
                return;

            if (string.IsNullOrEmpty(Account.RefreshToken))
            {
                Smtp.AuthenticationMechanisms.Remove("XOAUTH2");
                Smtp.Authenticate(Account.SmtpAccount, Account.SmtpPassword, CancelToken);
            }
            else
            {
                Smtp.Authenticate(Account.SmtpAccount, GetOauthAccessToken(Account.RefreshToken), CancelToken);
            }
        }

        #endregion

        #region .XOAUTH

        private string GetOauthAccessToken(string refreshToken)
        {
            var auth = new GoogleOAuth2Authorization(Log);
            var grantedAccess = auth.RequestAccessToken(refreshToken);
            if (grantedAccess == null)
                throw new AuthenticationException("XOAUTH2: Access denied.");

            return grantedAccess.AccessToken;
        }

        #endregion

        #endregion
    }
}