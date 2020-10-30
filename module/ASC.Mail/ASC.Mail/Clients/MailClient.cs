/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Authentication;
using System.Text;
using System.Threading;

using ASC.Common.Logging;
using ASC.Mail.Core;
using ASC.Mail.Data.Contracts;
using ASC.Mail.Data.Imap;
using ASC.Mail.Enums;
using ASC.Mail.Extensions;

using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Pop3;
using MailKit.Net.Smtp;
using MailKit.Search;
using MailKit.Security;

using MimeKit;

using AuthenticationException = MailKit.Security.AuthenticationException;
using MailFolder = ASC.Mail.Data.Contracts.MailFolder;
using Pop3Client = MailKit.Net.Pop3.Pop3Client;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace ASC.Mail.Clients
{
    public class MailClient : IDisposable
    {
        public MailBoxData Account { get; private set; }
        public ILog Log { get; set; }

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

        public MailClient(MailBoxData mailbox, CancellationToken cancelToken, int tcpTimeout = 30000,
            bool certificatePermit = false, string protocolLogPath = "",
            ILog log = null, bool skipSmtp = false, bool enableDsn = false)
        {
            var protocolLogger = !string.IsNullOrEmpty(protocolLogPath)
                ? (IProtocolLogger)
                    new ProtocolLogger(protocolLogPath)
                : new NullProtocolLogger();

            Account = mailbox;
            Log = log ?? new NullLog();

            StopTokenSource = new CancellationTokenSource();

            CancelToken = CancellationTokenSource.CreateLinkedTokenSource(cancelToken, StopTokenSource.Token).Token;

            if (Account.Imap)
            {
                Imap = new ImapClient(protocolLogger)
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
                Pop = new Pop3Client(protocolLogger)
                {
                    ServerCertificateValidationCallback = (sender, certificate, chain, errors) =>
                        certificatePermit ||
                        MailService.DefaultServerCertificateValidationCallback(sender, certificate, chain, errors),
                    Timeout = tcpTimeout
                };
                Imap = null;
            }

            if (skipSmtp)
            {
                Smtp = null;
                return;
            }

            if (enableDsn)
            {
                Smtp = new DsnSmtpClient(protocolLogger,
                    DeliveryStatusNotification.Success |
                    DeliveryStatusNotification.Failure |
                    DeliveryStatusNotification.Delay)
                {
                    ServerCertificateValidationCallback = (sender, certificate, chain, errors) =>
                        certificatePermit ||
                        MailService.DefaultServerCertificateValidationCallback(sender, certificate, chain, errors),
                    Timeout = tcpTimeout
                };
            }
            else
            {
                Smtp = new SmtpClient(protocolLogger)
                {
                    ServerCertificateValidationCallback = (sender, certificate, chain, errors) =>
                        certificatePermit ||
                        MailService.DefaultServerCertificateValidationCallback(sender, certificate, chain, errors),
                    Timeout = tcpTimeout
                };
            }
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

                if (folderId != (int)FolderType.Inbox)
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
                AggregatePop3(limitMessages);
        }

        public void Send(MimeMessage message, bool needCopyToSentFolder = true)
        {
            if (!Smtp.IsConnected)
                LoginSmtp();

            Smtp.Send(message, CancelToken);

            if (!Account.Imap || !needCopyToSentFolder)
                return;

            AppendCopyToSentFolder(message);
        }

        public void Cancel()
        {
            Log.Info("MailClient->Cancel()");
            StopTokenSource.Cancel();
        }

        public void Dispose()
        {
            Log.Info("MailClient->Dispose()");

            try
            {
                if (Imap != null)
                {
                    lock (Imap.SyncRoot)
                    {
                        if (Imap.IsConnected)
                        {
                            Log.Debug("Imap->Disconnect()");
                            Imap.Disconnect(true, CancelToken);
                        }

                        Imap.Dispose();
                    }
                }

                if (Pop != null)
                {
                    lock (Pop.SyncRoot)
                    {
                        if (Pop.IsConnected)
                        {
                            Log.Debug("Pop->Disconnect()");
                            Pop.Disconnect(true, CancelToken);
                        }

                        Pop.Dispose();
                    }
                }

                if (Smtp != null)
                {
                    lock (Smtp.SyncRoot)
                    {
                        if (Smtp.IsConnected)
                        {
                            Log.Debug("Smtp->Disconnect()");
                            Smtp.Disconnect(true, CancelToken);
                        }

                        Smtp.Dispose();
                    }
                }

                Authenticated = null;
                SendMessage = null;
                GetMessage = null;

                StopTokenSource.Dispose();

            }
            catch (Exception ex)
            {
                Log.ErrorFormat("MailClient->Dispose(Mb_Id={0} Mb_Addres: '{1}') Exception: {2}", Account.MailBoxId,
                    Account.EMail.Address, ex.Message);
            }
        }

        public void LoginImapPop()
        {
            if (Account.Imap)
            {
                if (!Imap.IsAuthenticated)
                    LoginImap();
            }
            else
            {
                if (!Pop.IsAuthenticated)
                    LoginPop3();
            }
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
            var secureSocketOptions = SecureSocketOptions.Auto;
            var sslProtocols = SslProtocols.Default;

            switch (Account.Encryption)
            {
                case EncryptionType.StartTLS:
                    secureSocketOptions = SecureSocketOptions.StartTlsWhenAvailable;
                    sslProtocols = SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12;
                    break;
                case EncryptionType.SSL:
                    secureSocketOptions = SecureSocketOptions.SslOnConnect;
                    sslProtocols = SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12;
                    break;
                case EncryptionType.None:
                    secureSocketOptions = SecureSocketOptions.None;
                    sslProtocols = SslProtocols.None;
                    break;
            }

            Log.DebugFormat("Imap.Connect({0}:{1}, {2})", Account.Server, Account.Port,
                Enum.GetName(typeof(SecureSocketOptions), secureSocketOptions));

            try
            {
                Imap.SslProtocols = sslProtocols;

                var t = Imap.ConnectAsync(Account.Server, Account.Port, secureSocketOptions, CancelToken);

                if (!t.Wait(Defines.MailServerConnectionTimeout, CancelToken))
                    throw new TimeoutException("Imap.ConnectAsync timeout");

                if (enableUtf8 && (Imap.Capabilities & ImapCapabilities.UTF8Accept) != ImapCapabilities.None)
                {
                    Log.Debug("Imap.EnableUTF8");

                    t = Imap.EnableUTF8Async(CancelToken);

                    if (!t.Wait(Defines.MailServerEnableUtf8Timeout, CancelToken))
                        throw new TimeoutException("Imap.EnableUTF8Async timeout");
                }

                Imap.Authenticated += ImapOnAuthenticated;

                if (string.IsNullOrEmpty(Account.OAuthToken))
                {
                    Log.DebugFormat("Imap.Authentication({0})", Account.Account);

                    t = Imap.AuthenticateAsync(Account.Account, Account.Password, CancelToken);
                }
                else
                {
                    Log.DebugFormat("Imap.AuthenticationByOAuth({0})", Account.Account);

                    var oauth2 = new SaslMechanismOAuth2(Account.Account, Account.AccessToken);

                    t = Imap.AuthenticateAsync(oauth2, CancelToken);
                }

                if (!t.Wait(Defines.MailServerLoginTimeout, CancelToken))
                {
                    Imap.Authenticated -= ImapOnAuthenticated;
                    throw new TimeoutException("Imap.AuthenticateAsync timeout");
                }

                Imap.Authenticated -= ImapOnAuthenticated;
            }
            catch (AggregateException aggEx)
            {
                if (aggEx.InnerException != null)
                {
                    throw aggEx.InnerException;
                }
                throw new Exception("LoginImap failed", aggEx);
            }
        }

        private void ImapOnAuthenticated(object sender, AuthenticatedEventArgs authenticatedEventArgs)
        {
            OnAuthenticated(authenticatedEventArgs.Message);
        }

        private void AggregateImap(TasksConfig tasksConfig, int limitMessages = -1)
        {
            if (!Imap.IsAuthenticated)
                LoginImap();

            try
            {
                var loaded = 0;

                var folders = GetImapFolders();

                foreach (var folder in folders)
                {
                    if (!Imap.IsConnected || CancelToken.IsCancellationRequested)
                        return;

                    var mailFolder = DetectFolder(tasksConfig, folder);

                    if (mailFolder == null)
                    {
                        Log.InfoFormat("[folder] x '{0}' (skipped)", folder.Name);
                        continue;
                    }

                    Log.InfoFormat("[folder] >> '{0}' (fId={1}) {2}", folder.Name, mailFolder.Folder,
                        mailFolder.Tags.Any() ? string.Format("tag='{0}'", mailFolder.Tags.FirstOrDefault()) : "");

                    try
                    {
                        folder.Open(FolderAccess.ReadOnly, CancelToken);
                    }
                    catch (OperationCanceledException)
                    {
                        // Skip log error
                        continue;
                    }
                    catch (Exception e)
                    {
                        Log.ErrorFormat("Open faild: {0} Exception: {1}", folder.Name, e.Message);
                        continue;
                    }

                    loaded += LoadFolderMessages(folder, mailFolder, limitMessages);

                    if (limitMessages <= 0 || loaded < limitMessages)
                        continue;

                    Log.Debug("Limit of maximum number messages per session is exceeded!");
                    break;
                }
            }
            catch (AggregateException aggEx)
            {
                if (aggEx.InnerException != null)
                {
                    throw aggEx.InnerException;
                }
                throw new Exception("AggregateImap failed", aggEx);
            }
        }

        private static bool IsInbox(IMailFolder folder)
        {
            return folder.Attributes.HasFlag(FolderAttributes.Inbox) ||
                   folder.Name.Equals("inbox", StringComparison.InvariantCultureIgnoreCase) ||
                   folder.FullName.Equals("inbox", StringComparison.InvariantCultureIgnoreCase);
        }

        private static bool IsSent(IMailFolder folder)
        {
            return folder.Attributes.HasFlag(FolderAttributes.Sent) ||
                   folder.Name.Equals("sent", StringComparison.InvariantCultureIgnoreCase) ||
                   folder.FullName.Equals("sent", StringComparison.InvariantCultureIgnoreCase) ||
                   folder.Name.Equals("sent items", StringComparison.InvariantCultureIgnoreCase) ||
                   folder.FullName.Equals("sent items", StringComparison.InvariantCultureIgnoreCase);
        }

        private static bool IsSpam(IMailFolder f)
        {
            return f.Attributes.HasFlag(FolderAttributes.Junk) ||
                f.Name.Equals("spam", StringComparison.InvariantCultureIgnoreCase) ||
                f.FullName.Equals("spam", StringComparison.InvariantCultureIgnoreCase) ||
                f.Name.Equals("junk", StringComparison.InvariantCultureIgnoreCase) ||
                f.FullName.Equals("junk", StringComparison.InvariantCultureIgnoreCase) ||
                f.Name.Equals("bulk", StringComparison.InvariantCultureIgnoreCase) ||
                f.FullName.Equals("bulk", StringComparison.InvariantCultureIgnoreCase);
        }

        private static IEnumerable<IMailFolder> SortByUsage(IEnumerable<IMailFolder> folders)
        {
            var inboxList = new List<IMailFolder>();
            var sentList = new List<IMailFolder>();
            var spamList = new List<IMailFolder>();
            var otherList = new List<IMailFolder>();

            foreach (var folder in folders)
            {
                if (IsInbox(folder))
                {
                    inboxList.Add(folder);
                    continue;
                }
                else if (IsSent(folder))
                {
                    sentList.Add(folder);
                    continue;
                }
                else if (IsSpam(folder))
                {
                    spamList.Add(folder);
                    continue;
                }

                otherList.Add(folder);
            }

            var resultFolders = new List<IMailFolder>();

            resultFolders.AddRange(inboxList);
            resultFolders.AddRange(sentList);
            resultFolders.AddRange(spamList);
            resultFolders.AddRange(otherList);

            return resultFolders;
        }

        private IEnumerable<IMailFolder> GetImapFolders()
        {
            Log.Debug("GetImapFolders()");

            var personalFolders = Imap.GetFolders(Imap.PersonalNamespaces[0], true, CancelToken).ToList();

            var inboxCount = personalFolders.Count(mb => mb.FullName.Equals("inbox", StringComparison.InvariantCultureIgnoreCase));

            if (inboxCount == 0)
            {
                personalFolders.Add(Imap.Inbox);
            }

            var folders = new List<IMailFolder>(personalFolders);

            foreach (var folder in
                personalFolders.Where(
                    f => f.Attributes.HasFlag(FolderAttributes.HasChildren)))
            {
                folders.AddRange(GetImapSubFolders(folder));
            }

            folders =
                folders.Where(
                    f =>
                        !f.Attributes.HasFlag(FolderAttributes.NoSelect) &&
                        !f.Attributes.HasFlag(FolderAttributes.NonExistent))
                    .Distinct()
                    .ToList();

            if (folders.Count <= 1)
                return folders;

            var resultList = SortByUsage(folders);

            return resultList;
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

                foreach (var subfolder in
                    subfolders.Where(
                        f => f.Attributes.HasFlag(FolderAttributes.HasChildren)))
                {
                    tempList.AddRange(GetImapSubFolders(subfolder));
                }

                subfolders.AddRange(tempList);

                return subfolders;
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("GetImapSubFolders: {0} Exception: {1}", folder.Name, ex.Message);
            }

            return new List<IMailFolder>();
        }

        private int LoadFolderMessages(IMailFolder folder, MailFolder mailFolder, int limitMessages)
        {
            var loaded = 0;

            ImapFolderUids folderUids;
            if (!Account.ImapIntervals.TryGetValue(folder.FullName, out folderUids))
            {
                Account.ImapFolderChanged = true;
                folderUids = new ImapFolderUids(new List<int> { 1, int.MaxValue }, 1, folder.UidValidity); // by default - mailbox never was processed before
            }
            else
            {
                if (folderUids.UidValidity.HasValue &&
                    folderUids.UidValidity != folder.UidValidity)
                {
                    Log.DebugFormat("Folder '{0}' UIDVALIDITY changed - need refresh folder", folder.Name);
                    folderUids = new ImapFolderUids(new List<int> { 1, int.MaxValue }, 1, folder.UidValidity); // reset folder check history if uidValidity has been changed
                }
            }

            if (!folderUids.UidValidity.HasValue)
            {
                Log.DebugFormat("Folder Name = '{0}' FullName = '{1}' Save UIDVALIDITY = {2}", folder.Name, folder.FullName, folder.UidValidity);
                folderUids.UidValidity = folder.UidValidity; // Update UidValidity
            }

            var imapIntervals = new ImapIntervals(folderUids.UnhandledUidIntervals);
            var beginDateUid = folderUids.BeginDateUid;

            var allUids = GetFolderUids(folder);

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

                var infoList = GetMessagesSummaryInfo(folder,
                    limitMessages > 0 ? uidsCollection.Take(limitMessages * 3).ToList() : uidsCollection);

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
                            Log.DebugFormat("Skip message (Date = {0}) on BeginDate = {1}", message.Date,
                                Account.BeginDate);
                            imapIntervals.SetBeginIndex(toUid);
                            beginDateUid = toUid;
                            break;
                        }

                        var unread = info != null &&
                                     (info.Keywords.Contains("\\Unseen") ||
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
                        Log.ErrorFormat(
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

            var updatedImapFolderUids = new ImapFolderUids(imapIntervals.ToIndexes(), beginDateUid, folder.UidValidity);

            if (!Account.ImapIntervals.Keys.Contains(folder.FullName))
            {
                Account.ImapFolderChanged = true;
                Account.ImapIntervals.Add(folder.FullName, updatedImapFolderUids);
            }
            else if (Account.ImapIntervals[folder.FullName] != updatedImapFolderUids)
            {
                Account.ImapFolderChanged = true;
                Account.ImapIntervals[folder.FullName] = updatedImapFolderUids;
            }

            return loaded;
        }

        private List<UniqueId> GetFolderUids(IMailFolder folder)
        {
            List<UniqueId> allUids;

            try
            {
                allUids = folder.Fetch(0, -1, MessageSummaryItems.UniqueId, CancelToken).Select(r => r.UniqueId).ToList();
            }
            catch (ImapCommandException ex)
            {
                Log.WarnFormat("GetFolderUids() Exception: {0}", ex.ToString());

                const int start = 0;
                var end = folder.Count;
                const int increment = 1;

                allUids = Enumerable
                    .Repeat(start, (end - start) / 1 + 1)
                    .Select((tr, ti) => tr + increment * ti)
                    .Select(n => new UniqueId((uint)n))
                    .ToList();
            }

            return allUids;
        }

        private List<IMessageSummary> GetMessagesSummaryInfo(IMailFolder folder, IList<UniqueId> uids)
        {
            var infoList = new List<IMessageSummary>();

            try
            {
                infoList =
                    folder.Fetch(uids,
                        MessageSummaryItems.Flags | MessageSummaryItems.GMailLabels |
                        MessageSummaryItems.InternalDate, CancelToken).ToList();

            }
            catch (ImapCommandException ex)
            {
                Log.WarnFormat("GetMessagesSummaryInfo() Exception: {0}", ex.ToString());
            }

            return infoList;
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

            FolderType folderId;

            if ((folder.Attributes & FolderAttributes.Inbox) != 0)
            {
                return new MailFolder(FolderType.Inbox, folder.Name);
            }
            if ((folder.Attributes & FolderAttributes.Sent) != 0)
            {
                return new MailFolder(FolderType.Sent, folder.Name);
            }
            if ((folder.Attributes & FolderAttributes.Junk) != 0)
            {
                return new MailFolder(FolderType.Spam, folder.Name);
            }
            if ((folder.Attributes &
                 (FolderAttributes.All |
                  FolderAttributes.NoSelect |
                  FolderAttributes.NonExistent |
                  FolderAttributes.Trash |
                  FolderAttributes.Archive |
                  FolderAttributes.Drafts |
                  FolderAttributes.Flagged)) != 0)
            {
                return null; // Skip folders
            }

            if (tasksConfig.ImapFlags != null &&
                tasksConfig.ImapFlags.Any() &&
                tasksConfig.ImapFlags.ContainsKey(folderName))
            {
                folderId = (FolderType)tasksConfig.ImapFlags[folderName];
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
                return new MailFolder(FolderType.Inbox, folder.Name, new[] { folder.FullName });

            folderId = (FolderType)tasksConfig.DefaultFolders[folderName];
            return new MailFolder(folderId, folder.Name);
        }

        private IMailFolder GetSentFolder()
        {
            var folders = Imap.GetFolders(Imap.PersonalNamespaces[0], false, CancelToken).ToList();

            if (!folders.Any())
                return null;

            var sendFolder = folders.FirstOrDefault(f => (f.Attributes & FolderAttributes.Sent) != 0);

            if (sendFolder != null)
                return sendFolder;

            var factory = new EngineFactory(Account.TenantId, Account.UserId);

            var specialDomainFolders = factory.FolderEngine.GetSpecialDomainFolders();

            if (!specialDomainFolders.Any() || !specialDomainFolders.ContainsKey(Account.Server))
                return null;

            foreach (var folder in folders)
            {
                var folderName = folder.Name;

                var domainSpecialFolders = specialDomainFolders[Account.Server];

                if (domainSpecialFolders == null)
                    continue;

                var foundKey = domainSpecialFolders.Keys.FirstOrDefault(
                    k => k.Equals(folderName, StringComparison.InvariantCultureIgnoreCase));

                if (foundKey == null)
                    continue;

                var info = domainSpecialFolders[foundKey];

                if (info.skip)
                    continue;

                if (info.folder_id != FolderType.Sent)
                    continue;

                sendFolder = folder;
                break;
            }

            return sendFolder;
        }

        private void AppendCopyToSentFolder(MimeMessage message)
        {
            if (!Account.Imap)
                throw new NotSupportedException("Only Imap is suppoted");

            if (message == null)
                throw new ArgumentNullException("message");

            try
            {
                if (!Imap.IsAuthenticated)
                    LoginImap();

                var sendFolder = GetSentFolder();

                if (sendFolder != null)
                {
                    sendFolder.Open(FolderAccess.ReadWrite);
                    var uid = sendFolder.Append(FormatOptions.Default, message, MessageFlags.Seen, CancelToken);

                    if (uid.HasValue)
                    {
                        Log.InfoFormat("AppendCopyToSenFolder(Mailbox: '{0}', Tenant: {1}, User: '{2}') succeed! (uid:{3})",
                            Account.EMail.Address, Account.TenantId, Account.UserId, uid.Value.Id);
                    }
                    else
                    {
                        Log.ErrorFormat("AppendCopyToSenFolder(Mailbox: '{0}', Tenant: {1}, User: '{2}') failed!",
                            Account.EMail.Address, Account.TenantId, Account.UserId);
                    }
                }
                else
                {
                    Log.DebugFormat(
                        "AppendCopyToSenFolder(Mailbox: '{0}', Tenant: {1}, User: '{2}'): Skip - sent-folder not found",
                        Account.EMail.Address, Account.TenantId, Account.UserId);
                }
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("AppendCopyToSenFolder(Mailbox: '{0}', Tenant: {1}, User: '{2}'): Exception:\r\n{3}\r\n",
                    Account.EMail.Address, Account.TenantId, Account.UserId, ex.ToString());
            }
        }

        #endregion

        #region .POP3

        private void LoginPop3(bool enableUtf8 = true)
        {
            var secureSocketOptions = SecureSocketOptions.Auto;
            var sslProtocols = SslProtocols.Default;

            switch (Account.Encryption)
            {
                case EncryptionType.StartTLS:
                    secureSocketOptions = SecureSocketOptions.StartTlsWhenAvailable;
                    sslProtocols = SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12;
                    break;
                case EncryptionType.SSL:
                    secureSocketOptions = SecureSocketOptions.SslOnConnect;
                    sslProtocols = SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12;
                    break;
                case EncryptionType.None:
                    secureSocketOptions = SecureSocketOptions.None;
                    sslProtocols = SslProtocols.None;
                    break;
            }

            Log.DebugFormat("Pop.Connect({0}:{1}, {2})", Account.Server, Account.Port,
                Enum.GetName(typeof(SecureSocketOptions), secureSocketOptions));
            try
            {
                Pop.SslProtocols = sslProtocols;

                var t = Pop.ConnectAsync(Account.Server, Account.Port, secureSocketOptions, CancelToken);

                if (!t.Wait(Defines.MailServerConnectionTimeout, CancelToken))
                    throw new TimeoutException("Pop.ConnectAsync timeout");

                if (enableUtf8 && (Pop.Capabilities & Pop3Capabilities.UTF8) != Pop3Capabilities.None)
                {
                    Log.Debug("Pop.EnableUTF8");

                    t = Pop.EnableUTF8Async(CancelToken);

                    if (!t.Wait(Defines.MailServerEnableUtf8Timeout, CancelToken))
                        throw new TimeoutException("Pop.EnableUTF8Async timeout");
                }

                Pop.Authenticated += PopOnAuthenticated;

                if (string.IsNullOrEmpty(Account.OAuthToken))
                {
                    Log.DebugFormat("Pop.Authentication({0})", Account.Account);

                    t = Pop.AuthenticateAsync(Account.Account, Account.Password, CancelToken);
                }
                else
                {
                    Log.DebugFormat("Pop.AuthenticationByOAuth({0})", Account.Account);

                    var oauth2 = new SaslMechanismOAuth2(Account.Account, Account.AccessToken);

                    t = Pop.AuthenticateAsync(oauth2, CancelToken);
                }

                if (!t.Wait(Defines.MailServerLoginTimeout, CancelToken))
                {
                    Pop.Authenticated -= PopOnAuthenticated;
                    throw new TimeoutException("Pop.AuthenticateAsync timeout");
                }

                Pop.Authenticated -= PopOnAuthenticated;
            }
            catch (AggregateException aggEx)
            {
                if (aggEx.InnerException != null)
                {
                    throw aggEx.InnerException;
                }
                throw new Exception("LoginPop3 failed", aggEx);
            }
        }

        private void PopOnAuthenticated(object sender, AuthenticatedEventArgs authenticatedEventArgs)
        {
            OnAuthenticated(authenticatedEventArgs.Message);
        }

        public Func<Dictionary<int, string>, Dictionary<int, string>> FuncGetPop3NewMessagesIDs { get; set; }

        private void AggregatePop3(int limitMessages = -1)
        {
            if (!Pop.IsAuthenticated)
                LoginPop3();

            var mailFolder = new MailFolder(FolderType.Inbox, "INBOX");

            try
            {
                var loaded = 0;
                var i = 0;
                var uidls = Pop.GetMessageUids(CancelToken)
                    .Select(uidl => new KeyValuePair<int, string>(i++, uidl))
                    .ToDictionary(t => t.Key, t => t.Value);

                if (!uidls.Any() || uidls.Count == Account.MessagesCount)
                {
                    Account.MessagesCount = uidls.Count;

                    Log.Debug("New messages not found.\r\n");
                    return;
                }

                var newMessages = FuncGetPop3NewMessagesIDs(uidls);

                if (newMessages.Count == 0)
                {
                    Account.MessagesCount = uidls.Count;

                    Log.Debug("New messages not found.\r\n");
                    return;
                }

                Log.DebugFormat("Found {0} new messages.\r\n", newMessages.Count);

                newMessages = FixPop3UidsOrder(newMessages);

                var skipOnDate = Account.BeginDate != Defines.MinBeginDate;

                foreach (var newMessage in newMessages)
                {
                    if (!Pop.IsConnected || CancelToken.IsCancellationRequested)
                        break;

                    Log.DebugFormat("Processing new message\tUID: {0}\tUIDL: {1}\t",
                        newMessage.Key,
                        newMessage.Value);

                    try
                    {
                        var message = Pop.GetMessage(newMessage.Key, CancelToken);

                        message.FixDateIssues(logger: Log);

                        if (message.Date < Account.BeginDate && skipOnDate)
                        {
                            Log.DebugFormat("Skip message (Date = {0}) on BeginDate = {1}", message.Date,
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
                        // Skip log error
                    }
                    catch (Exception e)
                    {
                        Log.ErrorFormat(
                            "ProcessMessages() Tenant={0} User='{1}' Account='{2}', MailboxId={3}, MessageIndex={4}, UIDL='{5}' Exception:\r\n{6}\r\n",
                            Account.TenantId, Account.UserId, Account.EMail.Address, Account.MailBoxId,
                            newMessage.Key, newMessage.Value, e);

                        if (e is IOException)
                        {
                            break; // stop checking other mailboxes
                        }
                    }
                }

                if (loaded < limitMessages)
                {
                    Account.MessagesCount = uidls.Count;
                }
            }
            catch (AggregateException aggEx)
            {
                if (aggEx.InnerException != null)
                {
                    throw aggEx.InnerException;
                }
                throw new Exception("AggregatePop3 failed", aggEx);
            }
        }

        private Dictionary<int, string> FixPop3UidsOrder(Dictionary<int, string> newMessages)
        {
            try
            {
                if (newMessages.Count < 2)
                    return newMessages;

                newMessages = newMessages
                    .OrderBy(item => item.Key)
                    .ToDictionary(id => id.Key, id => id.Value);

                var fstIndex = newMessages.First().Key;
                var lstIndex = newMessages.Last().Key;

                var fstMailHeaders = Pop.GetMessageHeaders(fstIndex, CancelToken).ToList();
                var lstMailHeaders = Pop.GetMessageHeaders(lstIndex, CancelToken).ToList();

                var fstDateHeader =
                    fstMailHeaders.FirstOrDefault(
                        h => h.Field.Equals("Date", StringComparison.InvariantCultureIgnoreCase));

                var lstDateHeader =
                    lstMailHeaders.FirstOrDefault(
                        h => h.Field.Equals("Date", StringComparison.InvariantCultureIgnoreCase));

                DateTime fstDate;
                DateTime lstDate;

                if (fstDateHeader != null && DateTime.TryParse(fstDateHeader.Value, out fstDate) &&
                    lstDateHeader != null &&
                    DateTime.TryParse(lstDateHeader.Value, out lstDate))
                {
                    if (fstDate < lstDate)
                    {
                        Log.DebugFormat("Account '{0}' uids order is DESC", Account.EMail.Address);
                        newMessages = newMessages
                            .OrderByDescending(item => item.Key)
                            .ToDictionary(id => id.Key, id => id.Value);
                        return newMessages;
                    }
                }


                Log.DebugFormat("Account '{0}' uids order is ASC", Account.EMail.Address);
            }
            catch (Exception)
            {
                newMessages = newMessages
                    .OrderByDescending(item => item.Key)
                    .ToDictionary(id => id.Key, id => id.Value);

                Log.WarnFormat("Calculating order skipped! Account '{0}' uids order is DESC", Account.EMail.Address);
            }

            return newMessages;
        }

        #endregion

        #region .SMTP

        private void LoginSmtp()
        {
            var secureSocketOptions = SecureSocketOptions.Auto;
            var sslProtocols = SslProtocols.Default;

            switch (Account.SmtpEncryption)
            {
                case EncryptionType.StartTLS:
                    secureSocketOptions = SecureSocketOptions.StartTlsWhenAvailable;
                    sslProtocols = SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12;
                    break;
                case EncryptionType.SSL:
                    secureSocketOptions = SecureSocketOptions.SslOnConnect;
                    sslProtocols = SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12;
                    break;
                case EncryptionType.None:
                    secureSocketOptions = SecureSocketOptions.None;
                    sslProtocols = SslProtocols.None;
                    break;
            }


            Log.DebugFormat("Smtp.Connect({0}:{1}, {2})", Account.SmtpServer, Account.SmtpPort,
                Enum.GetName(typeof(SecureSocketOptions), secureSocketOptions));
            try
            {
                Smtp.SslProtocols = sslProtocols;

                var t = Smtp.ConnectAsync(Account.SmtpServer, Account.SmtpPort, secureSocketOptions, CancelToken);

                if (!t.Wait(Defines.MailServerConnectionTimeout, CancelToken))
                    throw new TimeoutException("Smtp.ConnectAsync timeout");

                if (!Account.SmtpAuth)
                {
                    if ((Smtp.Capabilities & SmtpCapabilities.Authentication) != 0)
                        throw new AuthenticationException("SmtpAuth is required (setup Authentication Type)");

                    return;
                }

                Smtp.Authenticated += SmtpOnAuthenticated;

                if (string.IsNullOrEmpty(Account.OAuthToken))
                {
                    Log.DebugFormat("Smtp.Authentication({0})", Account.SmtpAccount);

                    t = Smtp.AuthenticateAsync(Account.SmtpAccount, Account.SmtpPassword, CancelToken);
                }
                else
                {
                    Log.DebugFormat("Smtp.AuthenticationByOAuth({0})", Account.SmtpAccount);

                    var oauth2 = new SaslMechanismOAuth2(Account.Account, Account.AccessToken);

                    t = Smtp.AuthenticateAsync(oauth2, CancelToken);
                }

                if (!t.Wait(Defines.MailServerLoginTimeout, CancelToken))
                {
                    Smtp.Authenticated -= SmtpOnAuthenticated;
                    throw new TimeoutException("Smtp.AuthenticateAsync timeout");
                }

                Smtp.Authenticated -= SmtpOnAuthenticated;
            }
            catch (AggregateException aggEx)
            {
                if (aggEx.InnerException != null)
                {
                    throw aggEx.InnerException;
                }
                throw new Exception("LoginSmtp failed", aggEx);
            }
        }

        private void SmtpOnAuthenticated(object sender, AuthenticatedEventArgs authenticatedEventArgs)
        {
            OnAuthenticated(authenticatedEventArgs.Message);
        }

        #endregion

        #endregion
    }
}