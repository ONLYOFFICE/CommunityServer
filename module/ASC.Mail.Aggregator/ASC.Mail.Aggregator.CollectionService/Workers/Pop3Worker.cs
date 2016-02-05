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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using ActiveUp.Net.Common;
using ActiveUp.Net.Mail;
using ASC.Core.Tenants;
using ASC.Mail.Aggregator.Common;
using ASC.Mail.Aggregator.Common.Extension;
using ASC.Mail.Aggregator.Common.Logging;
using ASC.Mail.Aggregator.Exceptions;

namespace ASC.Mail.Aggregator.CollectionService.Workers
{
    public class Pop3Worker : BaseWorker
    {
        private bool _isUidlSupported;

        public bool LoadOriginalEmlData { get; set; }

        public Pop3Worker(MailBoxManager mailBoxManager, MailBox mailBox, TasksConfig tasksConfig, CancellationToken cancelToken, bool loadOriginalEml, ILogger log = null)
            : base(mailBoxManager, mailBox, tasksConfig, cancelToken, log)
        {
            LoadOriginalEmlData = loadOriginalEml;
        }

        protected override BaseProtocolClient Connect()
        {
            try
            {
                var pop = MailClientBuilder.Pop();
                pop.Authenticated += OnAuthenticated;

                log.Debug("Connecting to {0}", Account.EMail);

                pop.Authorize(new MailServerSettings
                {
                    AccountName = Account.Account,
                    AccountPass = Account.Password,
                    AuthenticationType = Account.AuthenticationTypeIn,
                    EncryptionType = Account.IncomingEncryptionType,
                    Port = Account.Port,
                    Url = Account.Server
                }, Account.AuthorizeTimeoutInMilliseconds, log);

                pop.LoadOriginalData = LoadOriginalEmlData;

                return pop;
            }
            catch (Exception)
            {
                if (!Account.AuthErrorDate.HasValue)
                    mailBoxManager.SetMailboxAuthError(Account, true);

                throw;
            }
            finally
            {
                mailBoxManager.SetEmailLoginDelayExpires(Account.EMail.ToString(), Account.ServerLoginDelay);
            }
        }

        void OnAuthenticated(object sender, AuthenticatedEventArgs e)
        {
            if (Account.AuthErrorDate.HasValue)
                mailBoxManager.SetMailboxAuthError(Account, false);
        }

        protected override bool ProcessMessages(BaseProtocolClient baseProtocolClient)
        {
            var client = (Pop3Client) baseProtocolClient;
            
            UpdateTimeCheckedIfNeeded();

            var badMessagesExist = false;

            if (!_isUidlSupported)
            {
                log.Info("UIDL is not supported! Account '{0}' has been skiped.", Account.EMail);
                return true;
            }

            var newMessages = GetPop3NewMessagesIDs(client);

            var quotaErrorFlag = false;

            if (client.IsConnected)
            {
                if (newMessages.Count == 0)
                    log.Debug("New messages not found.\r\n");
                else
                {
                    log.Debug("Found {0} new messages.\r\n", newMessages.Count);

                    if (newMessages.Count > 1)
                    {
                        log.Debug("Calculating order");

                        try
                        {
                            var firstHeader = client.RetrieveHeaderObject(newMessages.First().Key);
                            var lastHeader = client.RetrieveHeaderObject(newMessages.Last().Key);

                            if (firstHeader.Date < lastHeader.Date)
                            {
                                log.Debug("Account '{0}' order is DESC", Account.EMail.Address);
                                newMessages = newMessages
                                    .OrderByDescending(item => item.Key) // This is to ensure that the newest message would be handled primarily.
                                    .ToDictionary(id => id.Key, id => id.Value);
                            }
                            else
                                log.Debug("Account '{0}' order is ASC", Account.EMail.Address);
                        }
                        catch (Exception)
                        {
                            log.Warn("Calculating order skipped! Account '{0}' order is ASC", Account.EMail.Address);
                        }
                    }

                    var skipOnDate = Account.BeginDate != MailBoxManager.MinBeginDate;

                    var skipBreakOnDate = MailQueueItemSettings.PopUnorderedDomains.Contains(Account.Server.ToLowerInvariant());

                    foreach (var newMessage in newMessages)
                    {
                        var hasParseError = false;

                        cancelToken.ThrowIfCancellationRequested();

                        try
                        {
                            if (_maxMessagesPerSession == 0)
                            {
                                log.Debug("Limit of max messages per session is exceeded!");
                                break;
                            }

                            if (!client.IsConnected)
                            {
                                log.Warn("POP3-server is disconnected. Skip another messages.");
                                badMessagesExist = true;
                                break;
                            }

                            log.Debug("Processing new message\tUID: {0}\tUIDL: {1}\t",
                                       newMessage.Key,
                                       newMessage.Value);

                            var message = client.RetrieveMessageObject(newMessage.Key);

                            if (message.ParseException != null)
                            {
                                log.Error(
                                    "ActiveUp: message parsed with some errors. MailboxId = {0} Message UID = \"{1}\" Error: {2}",
                                    Account.MailBoxId, newMessage.Value, message.ParseException.ToString());

                                hasParseError = true;
                            }

                            UpdateTimeCheckedIfNeeded();

                            if (message.Date < Account.BeginDate && skipOnDate)
                            {
                                if (!skipBreakOnDate)
                                {
                                    log.Info("Skip other messages older then {0}.", Account.BeginDate);
                                    break;
                                }
                                log.Debug("Skip message (Date = {0}) on BeginDate = {1}", message.Date,
                                           Account.BeginDate);
                                continue;
                            }

                            var uniqueIdentifier = string.Format("{0}|{1}|{2}|{3}",
                                                                    message.From.Email,
                                                                    message.Subject,
                                                                    message.DateString,
                                                                    message.MessageId);

                            var headerMd5 = uniqueIdentifier.GetMd5();

                            RetrieveMessage(message,
                                             MailFolder.Ids.inbox, newMessage.Value, headerMd5, hasParseError);
                        }
                        catch (Exception e)
                        {
                            var commonError =
                                string.Format(
                                    "ProcessMessages() Tenant={0} User='{1}' Account='{2}', MailboxId={3}, MessageIndex={4}, UIDL='{5}' Exception:\r\n{6}\r\n",
                                    Account.TenantId, Account.UserId, Account.EMail.Address, Account.MailBoxId,
                                    newMessage.Key, newMessage.Value, e);


                            if (e is IOException || e is MailBoxOutException)
                            {
                                _maxMessagesPerSession = 0; //It needed for stop messsages proccessing.
                                badMessagesExist = true;
                                if (e is IOException)
                                    log.Error(commonError);
                                else
                                    log.Info(commonError);
                                break;
                            }

                            if (e is TenantQuotaException)
                            {
                                log.Info("Tenant {0} quota exception: {1}", Account.TenantId, e.Message);
                                quotaErrorFlag = true;
                            }
                            else
                            {
                                badMessagesExist = true;
                                log.Error(commonError);
                            }
                        }

                        UpdateTimeCheckedIfNeeded();
                        _maxMessagesPerSession--;
                    }
                }
            }
            else
            {
                log.Debug("POP3 server is disconnected.");
                badMessagesExist = true;
            }

            if (Account.QuotaError != quotaErrorFlag && !Account.QuotaErrorChanged)
            {
                Account.QuotaError = quotaErrorFlag;
                Account.QuotaErrorChanged = true;
            }

            return !badMessagesExist && _maxMessagesPerSession > 0;
        }

        public override void Aggregate()
        {
            Pop3Client pop = null;

            try
            {
                pop = (Pop3Client) Connect();

                UpdateTimeCheckedIfNeeded();

                log.Debug("UpdateStats()");

                pop.UpdateStats();

                log.Debug("GetCAPA()");

                GetCapa(pop);

                log.Info("Account: MessagesCount={0}, TotalSize={1}, UIDL={2}, LoginDelay={3}",
                    pop.MessageCount, pop.TotalSize, _isUidlSupported, Account.ServerLoginDelay);

                // Were we already canceled?
                cancelToken.ThrowIfCancellationRequested();

                if (!ProcessMessages(pop)) return;

                Account.MessagesCount = pop.MessageCount;
                Account.Size = pop.TotalSize;
            }
            finally
            {
                try
                {
                    if (pop != null && pop.IsConnected)
                    {
                        pop.Disconnect();
                    }
                }
                catch(Exception ex)
                {
                    log.Warn("Aggregate() Pop3->Disconnect: {0} Port: {1} Account: '{2}' ErrorMessage:\r\n{3}\r\n",
                        Account.Server, Account.Port, Account.Account, ex.Message);
                }
            }
        }

        private void GetCapa(Pop3Client client)
        {
            try
            {
                var capaParams = client.GetServerCapabilities();

                if (capaParams.Length <= 0) return;
                var index = Array.IndexOf(capaParams, "LOGIN-DELAY");
                if (index > -1)
                {
                    int delay;
                    if (int.TryParse(capaParams[index], NumberStyles.Integer, CultureInfo.InvariantCulture, out delay))
                        Account.ServerLoginDelay = delay;
                }

                _isUidlSupported = Array.IndexOf(capaParams, "UIDL") > -1;
            }
            catch
            { // CAPA NOT SUPPORTED 
                try
                { // CHECK UIDL SUPPORT
                    client.GetUniqueIds();
                    _isUidlSupported = true;
                }
                catch
                { // UIDL NOT SUPPORTED 
                    _isUidlSupported = false;
                }
            }
        }

        private Dictionary<int, string> GetPop3NewMessagesIDs(Pop3Client client)
        {
            var newMessages = new Dictionary<int, string>();

            var emailIds = client.GetUniqueIds();

            if (!emailIds.Any() || emailIds.Count == Account.MessagesCount)
                return newMessages;

            var i = 0;
            var chunk = tasksConfig.ChunkOfPop3Uidl;

            var emails = emailIds.Skip(i).Take(chunk).ToList();

            do
            {
                var checkList = emails.Select(e => e.UniqueId).Distinct().ToList();

                var existingUidls = mailBoxManager.CheckUidlExistance(Account.MailBoxId, checkList);

                if (!existingUidls.Any())
                {
                    foreach (
                        var item in
                            emails.Select(email => new KeyValuePair<int, string>(email.Index, email.UniqueId))
                                  .Where(item => !newMessages.Contains(item)))
                    {
                        newMessages.Add(item.Key, item.Value);
                    }
                }
                else if (existingUidls.Count != emails.Count)
                {
                    foreach (var item in (from email in emails
                                          where !existingUidls.Contains(email.UniqueId)
                                          select new KeyValuePair<int, string>(email.Index, email.UniqueId)).Where(
                                              item => !newMessages.Contains(item)))
                    {
                        newMessages.Add(item.Key, item.Value);
                    }
                }

                i += chunk;

                emails = emailIds.Skip(i).Take(chunk).ToList();

            } while (emails.Any());

            return newMessages;
        }
    }
}
