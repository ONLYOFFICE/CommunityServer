/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


using System;
using System.IO;
using System.Linq;
using System.Threading;
using ASC.Core.Tenants;
using ASC.Mail.Aggregator.Common;
using ASC.Mail.Aggregator.Common.Extension;
using ASC.Mail.Aggregator.Common.Imap;
using ASC.Mail.Aggregator.Common.Logging;
using ASC.Mail.Aggregator.Exceptions;
using ActiveUp.Net.Common;
using ActiveUp.Net.Mail;

namespace ASC.Mail.Aggregator.CollectionService.Workers
{
    public class Imap4Worker: BaseWorker
    {
        public Imap4Worker(MailBoxManager mailBoxManager, MailBox mailBox, TasksConfig tasksConfig, CancellationToken cancelToken, ILogger log = null)
            : base(mailBoxManager, mailBox, tasksConfig, cancelToken, log)
        {
        }

        protected override BaseProtocolClient Connect()
        {
            try
            {
                var imap = MailClientBuilder.Imap();
                imap.Authenticated += OnAuthenticated;
                imap.AuthenticateImap(Account, log);

                return imap;
            }
            catch (Exception)
            {
                if (!Account.AuthErrorDate.HasValue)
                    mailBoxManager.SetMailboxAuthError(Account, true);

                throw;
            }
            finally
            {
                var expires = DateTime.UtcNow.AddSeconds(Account.ServerLoginDelay);
                mailBoxManager.SetEmailLoginDelayExpires(Account.EMail.ToString(), expires);
            }
        }

        void OnAuthenticated(object sender, AuthenticatedEventArgs e)
        {
            if (Account.AuthErrorDate.HasValue)
                mailBoxManager.SetMailboxAuthError(Account, false);
        }

        private int[] InvokeOnGetOrCreateTags(string[] tagsNames)
        {
            return null != tagsNames && tagsNames.Any() ? mailBoxManager.GetOrCreateTags(Account.TenantId, Account.UserId, tagsNames) : null;
        }

        private int LoadMailboxMessage(Mailbox mb,
                                    int folderId,
                                    int maxMessagesPerSession,
                                    string[] tagsNames, out bool quotaErrorFlag)
        {
            UpdateTimeCheckedIfNeeded();

            ImapFolderUids folderUids;
            if (!Account.ImapIntervals.TryGetValue(mb.Name, out folderUids))
                folderUids = new ImapFolderUids(); // by default - mailbox never was processed before

            var imapIntervals = new ImapIntervals(folderUids.UnhandledUidIntervals);
            var beginDateUid = folderUids.BeginDateUid;

            quotaErrorFlag = false;

            int[] tagsIds = null;
            var tagsRetrieved = false;

            var allUids = mb.UidSearch("1:*").OrderBy(i => i).SkipWhile(i => i < beginDateUid).ToList();

            foreach (var uidsInterval in imapIntervals.GetUnhandledIntervalsCopy())
            {
                cancelToken.ThrowIfCancellationRequested();
                
                if (!mb.SourceClient.IsConnected)
                    break;
                
                if (maxMessagesPerSession == 0)
                    break;

                var interval = uidsInterval;
                var uidsCollection =
                    allUids.Select(u => u)
                            .Where(u => u <= interval.To && u >= interval.From)
                            .OrderByDescending(x => x)
                            .ToList();

                if (!uidsCollection.Any())
                {
                    if (!uidsInterval.IsToUidMax())
                        imapIntervals.AddHandledInterval(uidsInterval);
                    continue;
                }

                var toUid = uidsInterval.IsToUidMax()
                                 ? uidsCollection.First()
                                 : Math.Max(uidsInterval.To, uidsCollection.First());

                #region Loading messages

                foreach (var uid in uidsCollection)
                {
                    var hasParseError = false;

                    cancelToken.ThrowIfCancellationRequested();

                    try
                    {
                        if (!mb.SourceClient.IsConnected)
                        {
                            log.Warn("Imap4-server is disconnected. Skip another messages.");
                            break;
                        }

                        log.Debug("Processing new message\tUID: {0}", uid);

                        // flags should be retrieved before message fetch - because mail server
                        // could add seen flag right after message was retrieved by us
                        var flags = mb.Fetch.UidFlags(uid);

                        //Peek method didn't set \Seen flag on mail
                        var message = mb.Fetch.UidMessageObjectPeek(uid);

                        if (message.HasParseError)
                        {
                            log.Error("ActiveUp: message parsed with some errors. MailboxId = {0} Message UID = {1}", Account.MailBoxId, uid);
                            hasParseError = true;
                        }

                        UpdateTimeCheckedIfNeeded();

                        if (message.Date < Account.BeginDate)
                        {
                            log.Debug("Skip message (Date = {0}) on BeginDate = {1}", message.Date, Account.BeginDate);
                            imapIntervals.SetBeginIndex(toUid);
                            beginDateUid = toUid;
                            break;
                        }

                        var uniqueIdentifier = string.Format("{0}|{1}|{2}|{3}",
                                                              message.From.Email,
                                                              message.Subject,
                                                              message.DateString,
                                                              message.MessageId);

                        var headerMd5 = uniqueIdentifier.GetMd5();

                        //Get tags ids for folder before message proccessing only once
                        if (!tagsRetrieved)
                        {
                            tagsIds = tagsNames.Any() ? InvokeOnGetOrCreateTags(tagsNames) : null;
                            tagsRetrieved = true;
                        }

                        var unread = null == flags["seen"];
                        var uidl = string.Format("{0}-{1}", uid, folderId);
                        RetrieveMessage(message, folderId, uidl, headerMd5, hasParseError, unread, tagsIds);
                    }
                    catch (Exception e)
                    {
                        log.Error(
                            "ProcessMessages() Tenant={0} User='{1}' Account='{2}', MailboxId={3}, UID={4} Exception:\r\n{5}\r\n",
                            Account.TenantId, Account.UserId, Account.EMail.Address, Account.MailBoxId,
                            uid, e);

                        if (e is IOException || e is MailBoxOutException)
                        {
                            maxMessagesPerSession = 0; // stop checking other mailboxes
                        }
                        else if (e is TenantQuotaException)
                        {
                            quotaErrorFlag = true;
                        }

                        if (uid != uidsCollection.First() && uid != toUid)
                        {
                            imapIntervals.AddHandledInterval(new UidInterval(uid + 1, toUid));
                        }
                        toUid = uid - 1;

                        if (maxMessagesPerSession == 0)
                            break;

                        continue;
                    }

                    // after successfully message saving - lets update imap intervals state
                    imapIntervals.AddHandledInterval(
                        new UidInterval(
                            uid == uidsCollection.Last() && uidsInterval.IsFromUidMin() ? uidsInterval.From : uid, toUid));

                    toUid = uid - 1;

                    UpdateTimeCheckedIfNeeded();

                    maxMessagesPerSession--;

                    if (maxMessagesPerSession != 0) continue;

                    log.Info("Limit of max messages per session is exceeded!");
                    break;
                }

                #endregion

            }

            var updatedImapFolderUids = new ImapFolderUids(imapIntervals.ToIndexes(), beginDateUid);

            if (!Account.ImapIntervals.Keys.Contains(mb.Name))
            {
                Account.ImapFolderChanged = true;
                Account.ImapIntervals.Add(mb.Name, updatedImapFolderUids);
            }
            else if (Account.ImapIntervals[mb.Name] != updatedImapFolderUids)
            {
                Account.ImapFolderChanged = true;
                Account.ImapIntervals[mb.Name] = updatedImapFolderUids;
            }

            return maxMessagesPerSession;
        }

        protected override bool ProcessMessages(BaseProtocolClient baseProtocolClient)
        {
            var client = (Imap4Client)baseProtocolClient;
            
            var mailboxes =
                     client.GetImapMailboxes(Account.Server, MailQueueItemSettings.SpecialDomainFolders,
                                           MailQueueItemSettings.SkipImapFlags, MailQueueItemSettings.ImapFlags)
                         .Reverse()
                         .OrderBy(m => m.folder_id)
                         .ToList();

            if (!mailboxes.Any())
                log.Error("There was no folder parsed! MailboxId={0}", Account.MailBoxId);

            var index = mailboxes.FindIndex(x => x.name.ToUpperInvariant() == "INBOX");
            if (index > 0)
            {
                //Move folder INBOX to top of list in order to quickly receive new messages
                var item = mailboxes[index];
                mailboxes[index] = mailboxes[0];
                mailboxes[0] = item;
            }

            var quotaErrorByTotalFolders = false;

            foreach (var mailbox in mailboxes)
            {
                log.Info("Select imap folder: {0}", mailbox.name);

                Mailbox mbObj;

                cancelToken.ThrowIfCancellationRequested();

                if (!client.IsConnected)
                    break;

                try
                {
                    mbObj = client.SelectMailbox(mailbox.name);
                }
                catch (Exception ex)
                {
                    log.Error("imap.SelectMailbox(\"{0}\") in MailboxId={1} failed with exception:\r\n{2}",
                               mailbox.name, Account.MailBoxId, ex.ToString());
                    continue;
                }

                bool quotaErrorFlag;

                _maxMessagesPerSession = LoadMailboxMessage(mbObj,
                    mailbox.folder_id,
                    _maxMessagesPerSession,
                    mailbox.tags, out quotaErrorFlag);

                if (quotaErrorFlag)
                    quotaErrorByTotalFolders = true;

                if (0 == _maxMessagesPerSession)
                    break;

                UpdateTimeCheckedIfNeeded();
            }

            if (Account.QuotaError != quotaErrorByTotalFolders && !Account.QuotaErrorChanged)
            {
                Account.QuotaError = quotaErrorByTotalFolders;
                Account.QuotaErrorChanged = true;
            }

            return true;
        }

        public override void Aggregate()
        {
            Imap4Client imap = null;

            try
            {
                imap = (Imap4Client) Connect();

                UpdateTimeCheckedIfNeeded();

                // Were we already canceled?
                cancelToken.ThrowIfCancellationRequested();

                ProcessMessages(imap);

            }
            finally
            {
                try
                {
                    if (imap != null && imap.IsConnected)
                    {
                        imap.Disconnect();
                    }
                }
                catch (Exception ex)
                {
                    log.Warn("Aggregate() Imap4->Disconnect: {0} Port: {1} Account: '{2}' ErrorMessage:\r\n{3}\r\n",
                             Account.Server, Account.Port, Account.Account, ex.Message);
                }
            }
        }
    }
}
