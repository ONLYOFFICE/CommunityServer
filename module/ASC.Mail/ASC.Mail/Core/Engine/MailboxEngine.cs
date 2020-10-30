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
using System.Linq;
using System.Net.Mail;
using ASC.Common.Data;
using ASC.Common.Logging;
using ASC.Mail.Authorization;
using ASC.Mail.Core.Dao.Expressions.Attachment;
using ASC.Mail.Core.Dao.Expressions.Conversation;
using ASC.Mail.Core.Dao.Expressions.Mailbox;
using ASC.Mail.Core.Dao.Expressions.Message;
using ASC.Mail.Core.Dao.Interfaces;
using ASC.Mail.Core.DbSchema.Tables;
using ASC.Mail.Core.Entities;
using ASC.Mail.Data.Contracts;
using ASC.Mail.Enums;
using ASC.Mail.Utils;

namespace ASC.Mail.Core.Engine
{
    public class MailboxEngine
    {
        public int Tenant { get; private set; }
        public string User { get; private set; }

        public ILog Log { get; private set; }

        public EngineFactory Factory { get; private set; }

        public MailboxEngine(int tenant, string user, ILog log = null)
        {
            Tenant = tenant;
            User = user;

            Log = log ?? LogManager.GetLogger("ASC.Mail.MailboxEngine");

            Factory = new EngineFactory(Tenant, User, Log);
        }

        public MailBoxData GetMailboxData(IMailboxExp exp)
        {
            var tuple = GetMailboxFullInfo(exp);
            return tuple == null ? null : tuple.Item1;
        }

        public List<MailBoxData> GetMailboxDataList(IMailboxesExp exp)
        {
            var tuples = GetMailboxFullInfoList(exp);
            return tuples.Select(t => t.Item1).ToList();
        }

        public List<Tuple<MailBoxData, Mailbox>> GetMailboxFullInfoList(IMailboxesExp exp)
        {
            var list = new List<Tuple<MailBoxData, Mailbox>>();

            using (var daoFactory = new DaoFactory())
            {
                var daoMailbox = daoFactory.CreateMailboxDao();

                var mailboxes = daoMailbox.GetMailBoxes(exp);

                list.AddRange(mailboxes.Select(mailbox => GetMailbox(daoFactory, mailbox)).Where(tuple => tuple != null));
            }

            return list;
        }

        public Tuple<MailBoxData, Mailbox> GetMailboxFullInfo(IMailboxExp exp)
        {
            using (var daoFactory = new DaoFactory())
            {
                var daoMailbox = daoFactory.CreateMailboxDao();

                var mailbox = daoMailbox.GetMailBox(exp);

                if (mailbox == null)
                    return null;

                var tuple = GetMailbox(daoFactory, mailbox);

                return tuple;
            }
        }

        public Tuple<int, int> GetRangeMailboxes(IMailboxExp exp)
        {
            using (var daoFactory = new DaoFactory())
            {
                var daoMailbox = daoFactory.CreateMailboxDao();

                return daoMailbox.GetRangeMailboxes(exp);
            }
        }

        public bool TryGetNextMailboxData(IMailboxExp exp, out MailBoxData mailBoxData, out int failedId)
        {
            failedId = -1;

            try
            {
                using (var daoFactory = new DaoFactory())
                {
                    var daoMailbox = daoFactory.CreateMailboxDao();

                    var mailbox = daoMailbox.GetNextMailBox(exp);

                    if (mailbox == null)
                    {
                        mailBoxData = null;
                        return false;
                    }

                    var tuple = GetMailbox(daoFactory, mailbox);

                    if (tuple == null)
                    {
                        Log.WarnFormat("Mailbox id = {0} is not well-formated.", mailbox.Id);

                        mailBoxData = null;
                        failedId = mailbox.Id;
                        return false;
                    }

                    mailBoxData = tuple.Item1;
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log.Error("TryGetNextMailboxData failed", ex);
            }

            mailBoxData = null;
            return false;
        }

        public MailBoxData GetDefaultMailboxData(string email, string password,
            AuthorizationServiceType type, bool? imap, bool isNullNeeded)
        {
            var address = new MailAddress(email);

            var host = address.Host.ToLowerInvariant();

            if (type == AuthorizationServiceType.Google) host = Defines.GOOGLE_HOST;

            MailBoxData initialMailbox = null;

            if (imap.HasValue)
            {
                try
                {
                    var settings = Factory.MailBoxSettingEngine.GetMailBoxSettings(host);

                    if (settings != null)
                    {
                        var outgoingServerLogin = "";

                        var incommingType = imap.Value ? "imap" : "pop3";

                        var incomingServer =
                            settings.EmailProvider.IncomingServer
                            .FirstOrDefault(serv =>
                                serv.Type
                                .ToLowerInvariant()
                                .Equals(incommingType));

                        var outgoingServer = settings.EmailProvider.OutgoingServer.FirstOrDefault() ?? new ClientConfigEmailProviderOutgoingServer();

                        if (incomingServer != null && !string.IsNullOrEmpty(incomingServer.Username))
                        {
                            var incomingServerLogin = address.ToLogin(incomingServer.Username);

                            if (!string.IsNullOrEmpty(outgoingServer.Username))
                            {
                                outgoingServerLogin = address.ToLogin(outgoingServer.Username);
                            }

                            initialMailbox = new MailBoxData
                            {
                                EMail = address,
                                Name = "",

                                Account = incomingServerLogin,
                                Password = password,
                                Server = host.ToHost(incomingServer.Hostname),
                                Port = incomingServer.Port,
                                Encryption = incomingServer.SocketType.ToEncryptionType(),
                                SmtpEncryption = outgoingServer.SocketType.ToEncryptionType(),
                                Authentication = incomingServer.Authentication.ToSaslMechanism(),
                                SmtpAuthentication = outgoingServer.Authentication.ToSaslMechanism(),
                                Imap = imap.Value,

                                SmtpAccount = outgoingServerLogin,
                                SmtpPassword = password,
                                SmtpServer = host.ToHost(outgoingServer.Hostname),
                                SmtpPort = outgoingServer.Port,
                                Enabled = true,
                                TenantId = Tenant,
                                UserId = User,
                                BeginDate = DateTime.UtcNow.Subtract(new TimeSpan(MailBoxData.DefaultMailLimitedTimeDelta)),
                                OAuthType = (byte)type
                            };
                        }
                    }
                }
                catch (Exception)
                {
                    initialMailbox = null;
                }
            }

            if (initialMailbox != null || isNullNeeded)
            {
                return initialMailbox;
            }

            var isImap = imap.GetValueOrDefault(true);
            return new MailBoxData
            {
                EMail = address,
                Name = "",
                Account = email,
                Password = password,
                Server = string.Format((isImap ? "imap.{0}" : "pop.{0}"), host),
                Port = (isImap ? 993 : 110),
                Encryption = isImap ? EncryptionType.SSL : EncryptionType.None,
                SmtpEncryption = EncryptionType.None,
                Imap = isImap,
                SmtpAccount = email,
                SmtpPassword = password,
                SmtpServer = string.Format("smtp.{0}", host),
                SmtpPort = 25,
                Enabled = true,
                TenantId = Tenant,
                UserId = User,
                BeginDate = DateTime.UtcNow.Subtract(new TimeSpan(MailBoxData.DefaultMailLimitedTimeDelta)),
                Authentication = SaslMechanism.Login,
                SmtpAuthentication = SaslMechanism.Login
            };
        }

        public MailboxStatus GetMailboxStatus(IMailboxExp exp)
        {
            using (var daoFactory = new DaoFactory())
            {
                var daoMailbox = daoFactory.CreateMailboxDao();

                var status = daoMailbox.GetMailBoxStatus(exp);

                return status;
            }
        }

        public bool SaveMailBox(MailBoxData mailbox, AuthorizationServiceType authType = AuthorizationServiceType.None)
        {
            if (mailbox == null)
                throw new ArgumentNullException("mailbox");

            if (mailbox.IsTeamlab)
                throw new ArgumentException("Mailbox with specified email can't be updated");

            using (var daoFactory = new DaoFactory())
            {
                using (var tx = daoFactory.DbManager.BeginTransaction())
                {
                    var daoMailbox = daoFactory.CreateMailboxDao();
                    var daoMailboxServer = daoFactory.CreateMailboxServerDao();
                    var daoMailboxDomain = daoFactory.CreateMailboxDomainDao();
                    var daoMailboxProvider = daoFactory.CreateMailboxProviderDao();

                    var existingMailbox = daoMailbox.GetMailBox(
                        new СoncreteUserMailboxExp(
                            mailbox.EMail,
                            mailbox.TenantId, mailbox.UserId));

                    int newInServerId, newOutServerId;

                    var mailboxId = 0;
                    var dateCreated = DateTime.UtcNow;
                    var enabled = true;
                    var host = authType == AuthorizationServiceType.Google ? Defines.GOOGLE_HOST : mailbox.EMail.Host;

                    // Get new imap/pop3 server from MailBoxData
                    var newInServer = new MailboxServer
                    {
                        Hostname = mailbox.Server,
                        Port = mailbox.Port,
                        Type = mailbox.Imap ? Defines.IMAP : Defines.POP3,
                        Username = mailbox.EMail.ToLoginFormat(mailbox.Account) ?? mailbox.Account,
                        SocketType = mailbox.Encryption.ToNameString(),
                        Authentication = mailbox.Authentication.ToNameString()
                    };

                    // Get new smtp server from MailBoxData
                    var newOutServer = new MailboxServer
                    {
                        Hostname = mailbox.SmtpServer,
                        Port = mailbox.SmtpPort,
                        Type = Defines.SMTP,
                        Username =
                            mailbox.SmtpAuthentication != SaslMechanism.None
                                ? mailbox.EMail.ToLoginFormat(mailbox.SmtpAccount) ?? mailbox.SmtpAccount
                                : "",
                        SocketType = mailbox.SmtpEncryption.ToNameString(),
                        Authentication = mailbox.SmtpAuthentication.ToNameString()
                    };

                    if (existingMailbox != null)
                    {
                        mailboxId = existingMailbox.Id;
                        enabled = existingMailbox.Enabled;
                        dateCreated = existingMailbox.DateCreated;

                        // Get existing settings by existing ids
                        var dbInServer = daoMailboxServer.GetServer(existingMailbox.ServerId);
                        var dbOutServer = daoMailboxServer.GetServer(existingMailbox.SmtpServerId);

                        // Compare existing settings with new
                        if (!dbInServer.Equals(newInServer) || !dbOutServer.Equals(newOutServer))
                        {
                            var domain = daoMailboxDomain.GetDomain(host);

                            List<MailboxServer> trustedServers = null;
                            if (domain != null)
                                trustedServers = daoMailboxServer.GetServers(domain.ProviderId);

                            newInServerId = GetMailboxServerId(daoMailboxServer, dbInServer, newInServer, trustedServers);
                            newOutServerId = GetMailboxServerId(daoMailboxServer, dbOutServer, newOutServer,
                                trustedServers);
                        }
                        else
                        {
                            newInServerId = existingMailbox.ServerId;
                            newOutServerId = existingMailbox.SmtpServerId;
                        }
                    }
                    else
                    {
                        //Find settings by host

                        var domain = daoMailboxDomain.GetDomain(host);

                        if (domain != null)
                        {
                            //Get existing servers with isUserData = 0
                            var trustedServers = daoMailboxServer.GetServers(domain.ProviderId);

                            //Compare existing settings with new

                            var foundInServer = trustedServers.FirstOrDefault(ts => ts.Equals(newInServer));
                            var foundOutServer = trustedServers.FirstOrDefault(ts => ts.Equals(newOutServer));

                            //Use existing or save new servers
                            newInServerId = foundInServer != null
                                ? foundInServer.Id
                                : SaveMailboxServer(daoMailboxServer, newInServer, domain.ProviderId);

                            newOutServerId = foundOutServer != null
                                ? foundOutServer.Id
                                : SaveMailboxServer(daoMailboxServer, newOutServer, domain.ProviderId);
                        }
                        else
                        {
                            //Save new servers
                            var newProvider = new MailboxProvider
                            {
                                Id = 0,
                                Name = host,
                                DisplayShortName = "",
                                DisplayName = "",
                                Url = ""
                            };

                            newProvider.Id = daoMailboxProvider.SaveProvider(newProvider);

                            var newDomain = new MailboxDomain
                            {
                                Id = 0,
                                Name = host,
                                ProviderId = newProvider.Id
                            };

                            daoMailboxDomain.SaveDomain(newDomain);

                            newInServerId = SaveMailboxServer(daoMailboxServer, newInServer, newProvider.Id);
                            newOutServerId = SaveMailboxServer(daoMailboxServer, newOutServer, newProvider.Id);
                        }
                    }

                    var loginDelayTime = GetLoginDelayTime(mailbox);

                    //Save Mailbox to DB
                    var mb = new Mailbox
                    {
                        Id = mailboxId,
                        Tenant = mailbox.TenantId,
                        User = mailbox.UserId,
                        Address = mailbox.EMail.Address.ToLowerInvariant(),
                        Name = mailbox.Name,
                        Password = mailbox.Password,
                        MsgCountLast = mailbox.MessagesCount,
                        SmtpPassword = mailbox.SmtpPassword,
                        SizeLast = mailbox.Size,
                        LoginDelay = loginDelayTime,
                        Enabled = enabled,
                        Imap = mailbox.Imap,
                        BeginDate = mailbox.BeginDate,
                        OAuthType = mailbox.OAuthType,
                        OAuthToken = mailbox.OAuthToken,
                        ServerId = newInServerId,
                        SmtpServerId = newOutServerId,
                        DateCreated = dateCreated
                    };

                    var mailBoxId = daoMailbox.SaveMailBox(mb);

                    mailbox.MailBoxId = mailBoxId;

                    if (mailBoxId < 1)
                    {
                        tx.Rollback();
                        return false;
                    }

                    tx.Commit();
                }
            }

            return true;
        }

        public List<MailBoxData> GetMailboxesForProcessing(TasksConfig tasksConfig, int needTasks)
        {
            var mailboxes = new List<MailBoxData>();

            var boundaryRatio = !(tasksConfig.InactiveMailboxesRatio > 0 && tasksConfig.InactiveMailboxesRatio < 100);

            if (needTasks > 1 || boundaryRatio)
            {
                var inactiveCount = (int)Math.Round(needTasks * tasksConfig.InactiveMailboxesRatio / 100, MidpointRounding.AwayFromZero);

                var activeCount = needTasks - inactiveCount;

                if (activeCount == needTasks)
                {
                    mailboxes.AddRange(GetActiveMailboxesForProcessing(tasksConfig, activeCount));
                }
                else if (inactiveCount == needTasks)
                {
                    mailboxes.AddRange(GetInactiveMailboxesForProcessing(tasksConfig, inactiveCount));
                }
                else
                {
                    mailboxes.AddRange(GetActiveMailboxesForProcessing(tasksConfig, activeCount));

                    var difference = inactiveCount + activeCount - mailboxes.Count;

                    if (difference > 0)
                        mailboxes.AddRange(GetInactiveMailboxesForProcessing(tasksConfig, difference));
                }
            }
            else
            {
                mailboxes.AddRange(GetActiveMailboxesForProcessing(tasksConfig, 1));

                var difference = needTasks - mailboxes.Count;

                if (difference > 0)
                    mailboxes.AddRange(GetInactiveMailboxesForProcessing(tasksConfig, difference));
            }

            return mailboxes;
        }

        public bool LockMaibox(int id)
        {
            using (var daoFactory = new DaoFactory())
            {
                var daoMailbox = daoFactory.CreateMailboxDao();

                var status = daoMailbox.SetMailboxInProcess(id);

                return status;
            }
        }

        public bool ReleaseMaibox(MailBoxData account, TasksConfig tasksConfig)
        {
            var disableMailbox = false;

            var engineFactory = new EngineFactory(account.TenantId, account.UserId);

            if (account.AuthErrorDate.HasValue)
            {
                var difference = DateTime.UtcNow - account.AuthErrorDate.Value;

                if (difference > tasksConfig.AuthErrorDisableMailboxTimeout)
                {
                    disableMailbox = true;

                    engineFactory.AlertEngine.CreateAuthErrorDisableAlert(account.TenantId, account.UserId,
                        account.MailBoxId);
                }
                else if (difference > tasksConfig.AuthErrorWarningTimeout)
                {
                    engineFactory.AlertEngine.CreateAuthErrorWarningAlert(account.TenantId, account.UserId,
                        account.MailBoxId);
                }
            }

            if (account.QuotaErrorChanged)
            {
                if (account.QuotaError)
                {
                    engineFactory.AlertEngine.CreateQuotaErrorWarningAlert(account.TenantId, account.UserId);
                }
                else
                {
                    engineFactory.AlertEngine.DeleteAlert(MailAlertTypes.QuotaError);
                }
            }

            using (var daoFactory = new DaoFactory())
            {
                var daoMailbox = daoFactory.CreateMailboxDao();

                var mailbox =
                    daoMailbox.GetMailBox(new СoncreteUserMailboxExp(account.MailBoxId, account.TenantId,
                        account.UserId));

                if (mailbox == null) // Mailbox has been removed
                    return true;

                bool? enabled = null;
                int? messageCount = null;
                long? size = null;
                bool? quotaError = null;
                string oAuthToken = null;
                string imapIntervalsJson = null;
                bool? resetImapIntervals = null;

                if (account.AuthErrorDate.HasValue)
                {
                    if (disableMailbox)
                    {
                        enabled = false;
                    }
                }

                if (mailbox.MsgCountLast != account.MessagesCount)
                {
                    messageCount = account.MessagesCount;
                }

                if (mailbox.SizeLast != account.Size)
                {
                    size = account.Size;
                }

                if (account.QuotaErrorChanged)
                {
                    quotaError = account.QuotaError;
                }

                if (account.AccessTokenRefreshed)
                {
                    oAuthToken = account.OAuthToken;
                }

                if (account.Imap && account.ImapFolderChanged)
                {
                    if (account.BeginDateChanged)
                    {
                        resetImapIntervals = true;
                    }
                    else
                    {
                        imapIntervalsJson = account.ImapIntervalsJson;
                    }
                }

                return daoMailbox.SetMailboxProcessed(mailbox, account.ServerLoginDelay, enabled, messageCount, size,
                    quotaError, oAuthToken, imapIntervalsJson, resetImapIntervals);
            }
        }

        public bool SetMaiboxAuthError(int id, DateTime? authErroDate)
        {
            using (var daoFactory = new DaoFactory())
            {
                var daoMailbox = daoFactory.CreateMailboxDao();

                return daoMailbox.SetMailboxAuthError(id, authErroDate);
            }
        }

        public List<int> ReleaseMailboxes(int timeoutInMinutes)
        {
            using (var daoFactory = new DaoFactory())
            {
                var daoMailbox = daoFactory.CreateMailboxDao();

                return daoMailbox.SetMailboxesProcessed(timeoutInMinutes);
            }
        }

        public List<Tuple<int, string>> GetMailUsers(IMailboxExp exp)
        {
            using (var daoFactory = new DaoFactory())
            {
                var daoMailbox = daoFactory.CreateMailboxDao();

                return daoMailbox.GetMailUsers(exp);
            }
        }

        public bool DisableMailboxes(IMailboxExp exp)
        {
            using (var daoFactory = new DaoFactory())
            {
                var daoMailbox = daoFactory.CreateMailboxDao();

                return daoMailbox.Enable(exp, false);
            }
        }

        public bool SetNextLoginDelay(IMailboxExp exp, TimeSpan delay)
        {
            using (var daoFactory = new DaoFactory())
            {
                var daoMailbox = daoFactory.CreateMailboxDao();

                return daoMailbox.SetNextLoginDelay(exp, delay);
            }
        }

        public void RemoveMailBox(MailBoxData mailbox, bool needRecalculateFolders = true)
        {
            if (mailbox.MailBoxId <= 0)
                throw new Exception("MailBox id is 0");

            long freedQuotaSize;

            using (var db = new DbManager(Defines.CONNECTION_STRING_NAME, Defines.RemoveMailboxTimeout))
            {
                var daoFactory = new DaoFactory(db);
                using (var tx = daoFactory.DbManager.BeginTransaction())
                {
                    freedQuotaSize = RemoveMailBoxInfo(daoFactory, mailbox);

                    tx.Commit();
                }
            }

            var engine = new EngineFactory(mailbox.TenantId, mailbox.UserId, Log);
            engine.QuotaEngine.QuotaUsedDelete(freedQuotaSize);

            CacheEngine.Clear(mailbox.UserId);

            engine.IndexEngine.Remove(mailbox);

            if (!needRecalculateFolders)
                return;

            engine.OperationEngine.RecalculateFolders();
        }

        public void RemoveMailBox(IDaoFactory daoFactory, MailBoxData mailbox, bool needRecalculateFolders = true)
        {
            if (mailbox.MailBoxId <= 0)
                throw new Exception("MailBox id is 0");

            var freedQuotaSize = RemoveMailBoxInfo(daoFactory, mailbox);

            var engine = new EngineFactory(mailbox.TenantId, mailbox.UserId);

            engine.QuotaEngine.QuotaUsedDelete(freedQuotaSize);

            if (!needRecalculateFolders)
                return;

            engine.OperationEngine.RecalculateFolders();
        }

        /// <summary>
        /// Set mailbox removed
        /// </summary>
        /// <param name="mailBoxData"></param>
        /// <returns>Return freed quota value</returns>
        public long RemoveMailBoxInfo(MailBoxData mailBoxData)
        {
            long freedQuotaSize;

            using (var db = new DbManager(Defines.CONNECTION_STRING_NAME, Defines.RemoveMailboxTimeout))
            {
                var daoFactory = new DaoFactory(db);
                using (var tx = daoFactory.DbManager.BeginTransaction())
                {
                    freedQuotaSize = RemoveMailBoxInfo(daoFactory, mailBoxData);

                    tx.Commit();
                }
            }

            return freedQuotaSize;
        }

        /// <summary>
        /// Set mailbox removed
        /// </summary>
        /// <param name="daoFactory"></param>
        /// <param name="mailBoxData"></param>
        /// <returns>Return freed quota value</returns>
        private static long RemoveMailBoxInfo(IDaoFactory daoFactory, MailBoxData mailBoxData)
        {
            if (mailBoxData.MailBoxId <= 0)
                throw new Exception("MailBox id is 0");

            var daoMailbox = daoFactory.CreateMailboxDao();

            var mailbox = daoMailbox.GetMailBox(
                new СoncreteUserMailboxExp(mailBoxData.MailBoxId, mailBoxData.TenantId, mailBoxData.UserId, null));

            if (mailbox == null)
            {
                throw new Exception(string.Format("MailBox with id = {0} (Tenant={1}, User='{2}') not found",
                    mailBoxData.MailBoxId, mailBoxData.TenantId, mailBoxData.UserId));
            }

            daoMailbox.SetMailboxRemoved(mailbox);

            var daoChain = daoFactory.CreateChainDao(mailBoxData.TenantId, mailBoxData.UserId);

            var folderTypes = Enum.GetValues(typeof(FolderType)).Cast<int>().ToList();

            daoChain.Delete(
                SimpleConversationsExp.CreateBuilder(mailBoxData.TenantId, mailBoxData.UserId)
                    .SetFoldersIds(folderTypes)
                    .SetMailboxId(mailBoxData.MailBoxId)
                    .Build());

            var daoCrmLink = daoFactory.CreateCrmLinkDao(mailBoxData.TenantId, mailBoxData.UserId);

            daoCrmLink.RemoveCrmLinks(mailBoxData.MailBoxId);

            var daoMailInfo = daoFactory.CreateMailInfoDao(mailBoxData.TenantId, mailBoxData.UserId);

            daoMailInfo.SetFieldValue(
                SimpleMessagesExp.CreateBuilder(mailBoxData.TenantId, mailBoxData.UserId)
                    .SetMailboxId(mailBoxData.MailBoxId)
                    .Build(),
                MailTable.Columns.IsRemoved,
                true);

            var exp = new ConcreteMailboxAttachmentsExp(mailBoxData.MailBoxId, mailBoxData.TenantId, mailBoxData.UserId,
                onlyEmbedded: null);

            var daoAttachment = daoFactory.CreateAttachmentDao(mailBoxData.TenantId, mailBoxData.UserId);

            var totalAttachmentsSize = daoAttachment.GetAttachmentsSize(exp);

            daoAttachment.SetAttachmnetsRemoved(exp);

            var tagDao = daoFactory.CreateTagDao(mailBoxData.TenantId, mailBoxData.UserId);

            var tagMailDao = daoFactory.CreateTagMailDao(mailBoxData.TenantId, mailBoxData.UserId);

            var tagIds = tagMailDao.GetTagIds(mailBoxData.MailBoxId);

            tagMailDao.DeleteByMailboxId(mailBoxData.MailBoxId);

            foreach (var tagId in tagIds)
            {
                var tag = tagDao.GetTag(tagId);

                if (tag == null)
                    continue;

                var count = tagMailDao.CalculateTagCount(tag.Id);

                tag.Count = count;

                tagDao.SaveTag(tag);
            }

            daoFactory.CreateMailboxSignatureDao(mailBoxData.TenantId, mailBoxData.UserId).DeleteSignature(mailBoxData.MailBoxId);

            daoFactory.CreateMailboxAutoreplyDao(mailBoxData.TenantId, mailBoxData.UserId)
                .DeleteAutoreply(mailBoxData.MailBoxId);

            daoFactory.CreateMailboxAutoreplyHistoryDao(mailBoxData.TenantId, mailBoxData.UserId).DeleteAutoreplyHistory(mailBoxData.MailBoxId);

            daoFactory.CreateAlertDao(mailBoxData.TenantId, mailBoxData.UserId).DeleteAlerts(mailBoxData.MailBoxId);

            daoFactory.CreateUserFolderXMailDao(mailBoxData.TenantId, mailBoxData.UserId)
                .RemoveByMailbox(mailBoxData.MailBoxId);

            return totalAttachmentsSize;
        }

        private IEnumerable<MailBoxData> GetActiveMailboxesForProcessing(TasksConfig tasksConfig, int tasksLimit)
        {
            if (tasksLimit <= 0)
                return new List<MailBoxData>();

            Log.Debug("GetActiveMailboxForProcessing()");

            var mailboxes = GetMailboxDataList(new MailboxesForProcessingExp(tasksConfig, tasksLimit, true));

            Log.DebugFormat("Found {0} active tasks", mailboxes.Count);

            return mailboxes;
        }

        private IEnumerable<MailBoxData> GetInactiveMailboxesForProcessing(TasksConfig tasksConfig, int tasksLimit)
        {
            if (tasksLimit <= 0)
                return new List<MailBoxData>();

            Log.Debug("GetInactiveMailboxForProcessing()");

            var mailboxes = GetMailboxDataList(new MailboxesForProcessingExp(tasksConfig, tasksLimit, false));

            Log.DebugFormat("Found {0} inactive tasks", mailboxes.Count);

            return mailboxes;
        }

        private static int GetMailboxServerId(IMailboxServerDao daoMailboxServer, MailboxServer dbServer,
            MailboxServer newServer, List<MailboxServer> trustedServers)
        {
            int serverId;

            if (!dbServer.Equals(newServer))
            {
                // Server settings have been changed
                if (dbServer.IsUserData)
                {
                    if (trustedServers != null)
                    {
                        var foundInServer = trustedServers.FirstOrDefault(ts => ts.Equals(newServer));
                        if (foundInServer != null)
                        {
                            daoMailboxServer.DelteServer(dbServer.Id);
                            newServer.Id = foundInServer.Id;
                            newServer.IsUserData = false;
                        }
                        else
                        {
                            newServer.Id = dbServer.Id;
                            newServer.Id = SaveMailboxServer(daoMailboxServer, newServer, dbServer.ProviderId);
                        }
                    }
                    else
                    {
                        newServer.Id = dbServer.Id;
                        newServer.Id = SaveMailboxServer(daoMailboxServer, newServer, dbServer.ProviderId);
                    }
                }
                else
                {
                    if (trustedServers != null)
                    {
                        var foundInServer = trustedServers.FirstOrDefault(ts => ts.Equals(newServer));
                        if (foundInServer != null)
                        {
                            newServer.Id = foundInServer.Id;
                            newServer.IsUserData = false;
                        }
                        else
                        {
                            newServer.Id = SaveMailboxServer(daoMailboxServer, newServer, dbServer.ProviderId);
                        }
                    }
                    else
                    {
                        newServer.Id = SaveMailboxServer(daoMailboxServer, newServer, dbServer.ProviderId);

                    }
                }

                serverId = newServer.Id;
            }
            else
            {
                serverId = dbServer.Id;
            }

            return serverId;
        }

        private static int SaveMailboxServer(IMailboxServerDao daoMailboxServer, MailboxServer server,
            int providerId)
        {
            server.IsUserData = true;
            server.ProviderId = providerId;
            return daoMailboxServer.SaveServer(server);
        }

        private static int GetLoginDelayTime(MailBoxData mailbox)
        {
            //Todo: This hardcode inserted because pop3.live.com doesn't support CAPA command.
            //Right solution for that collision type:
            //1) Create table in DB: mail_login_delays. With REgexs and delays
            //1.1) Example of mail_login_delays data:
            //    .*@outlook.com    900
            //    .*@hotmail.com    900
            //    .*                30
            //1.2) Load this table to aggregator cache. Update it on changing.
            //1.3) Match email addreess of account with regexs from mail_login_delays
            //1.4) If email matched then set delay from that record.
            if (mailbox.Server == "pop3.live.com")
                return Defines.HARDCODED_LOGIN_TIME_FOR_MS_MAIL;

            return mailbox.ServerLoginDelay < MailBoxData.DefaultServerLoginDelay
                       ? MailBoxData.DefaultServerLoginDelay
                       : mailbox.ServerLoginDelay;
        }

        private static Tuple<MailBoxData, Mailbox> GetMailbox(IDaoFactory daoFactory, Mailbox mailbox)
        {
            var daoMailboxServer = daoFactory.CreateMailboxServerDao();

            var inServer = daoMailboxServer.GetServer(mailbox.ServerId);

            if (inServer == null)
                return null;

            var outServer = daoMailboxServer.GetServer(mailbox.SmtpServerId);

            if (outServer == null)
                return null;

            var daoMailboxAutoreply = daoFactory.CreateMailboxAutoreplyDao(mailbox.Tenant, mailbox.User);

            var autoreply = daoMailboxAutoreply.GetAutoreply(mailbox.Id);

            return new Tuple<MailBoxData, Mailbox>(ToMailBoxData(mailbox, inServer, outServer, autoreply), mailbox);
        }

        public static MailBoxData ToMailBoxData(Mailbox mailbox, MailboxServer inServer, MailboxServer outServer,
            MailboxAutoreply autoreply)
        {
            var address = new MailAddress(mailbox.Address);

            var mailAutoReply = autoreply != null
                ? new MailAutoreplyData(autoreply.MailboxId, autoreply.Tenant, autoreply.TurnOn, autoreply.OnlyContacts,
                    autoreply.TurnOnToDate, autoreply.FromDate, autoreply.ToDate, autoreply.Subject, autoreply.Html)
                : null;

            var inServerOldFormat = string.Format("{0}:{1}", inServer.Hostname, inServer.Port);
            var outServerOldFormat = string.Format("{0}:{1}", outServer.Hostname, outServer.Port);

            var mailboxData = new MailBoxData(mailbox.Tenant, mailbox.User, mailbox.Id, mailbox.Name, address,
                address.ToLogin(inServer.Username), mailbox.Password, inServerOldFormat,
                inServer.SocketType.ToEncryptionType(), inServer.Authentication.ToSaslMechanism(), mailbox.Imap,
                address.ToLogin(outServer.Username), mailbox.SmtpPassword, outServerOldFormat,
                outServer.SocketType.ToEncryptionType(), outServer.Authentication.ToSaslMechanism(),
                Convert.ToByte(mailbox.OAuthType), mailbox.OAuthToken)
            {
                Size = mailbox.SizeLast,
                MessagesCount = mailbox.MsgCountLast,
                ServerLoginDelay = mailbox.LoginDelay,
                BeginDate = mailbox.BeginDate,
                QuotaError = mailbox.QuotaError,
                AuthErrorDate = mailbox.DateAuthError,
                ImapIntervalsJson = mailbox.ImapIntervals,
                SmtpServerId = mailbox.SmtpServerId,
                InServerId = mailbox.ServerId,
                EMailInFolder = mailbox.EmailInFolder,
                MailAutoreply = mailAutoReply,
                AccessTokenRefreshed = false, //TODO: ???

                Enabled = mailbox.Enabled,
                IsRemoved = mailbox.IsRemoved,
                IsTeamlab = mailbox.IsTeamlabMailbox
            };

            return mailboxData;
        }
    }
}