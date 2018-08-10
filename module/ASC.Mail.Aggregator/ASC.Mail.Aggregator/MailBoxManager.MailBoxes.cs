/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using ARSoft.Tools.Net;
using ASC.Mail.Aggregator.Common.Utils;
using ASC.Api.Exceptions;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Common.Threading;
using ASC.Common.Utils;
using ASC.Core;
using ASC.Mail.Aggregator.Common;
using ASC.Mail.Aggregator.Common.Authorization;
using ASC.Mail.Aggregator.Common.Extension;
using ASC.Mail.Aggregator.Common.Imap;
using ASC.Mail.Aggregator.Common.Logging;
using ASC.Mail.Aggregator.ComplexOperations;
using ASC.Mail.Aggregator.ComplexOperations.Base;
using ASC.Mail.Aggregator.Dal;
using ASC.Mail.Aggregator.DbSchema;
using MailMessage = ASC.Mail.Aggregator.Common.MailMessage;

namespace ASC.Mail.Aggregator
{
    public partial class MailBoxManager
    {
        #region db defines

        public static readonly DateTime MinBeginDate = new DateTime(1975, 1, 1, 0, 0, 0);

        #endregion

        # region public methods

        public bool SaveMailBox(MailBox mailbox, AuthorizationServiceType authType = AuthorizationServiceType.None)
        {
            if (mailbox == null) throw new ArgumentNullException("mailbox");

            var idMailbox = MailBoxExists(GetAddress(mailbox.EMail), mailbox.UserId, mailbox.TenantId);

            using (var db = GetDb())
            {
                using (var tx = db.BeginTransaction())
                {
                    mailbox.InServerId = SaveMailBoxServerSettings(db, mailbox.EMail,
                        new MailBoxServerSettings
                        {
                            AccountName = mailbox.Account,
                            AccountPass = mailbox.Password,
                            AuthenticationType =
                                mailbox.Authentication,
                            EncryptionType =
                                mailbox.Encryption,
                            Port = mailbox.Port,
                            Url = mailbox.Server
                        },
                        mailbox.Imap ? "imap" : "pop3",
                        authType);

                    mailbox.SmtpServerId = SaveMailBoxServerSettings(db, mailbox.EMail,
                        new MailBoxServerSettings
                        {
                            AccountName = mailbox.SmtpAccount,
                            AccountPass = mailbox.SmtpPassword,
                            AuthenticationType =
                                mailbox.SmtpAuthentication,
                            EncryptionType =
                                mailbox.SmtpEncryption,
                            Port = mailbox.SmtpPort,
                            Url = mailbox.SmtpServer
                        },
                        "smtp",
                        authType);

                    int result;

                    var loginDelayTime = GetLoginDelayTime(mailbox);

                    if (idMailbox == 0)
                    {
                        var utcNow = DateTime.UtcNow;

                        result = db.ExecuteScalar<int>(
                            new SqlInsert(MailboxTable.Name)
                                .InColumnValue(MailboxTable.Columns.Id, 0)
                                .InColumnValue(MailboxTable.Columns.Tenant, mailbox.TenantId)
                                .InColumnValue(MailboxTable.Columns.User, mailbox.UserId)
                                .InColumnValue(MailboxTable.Columns.Address, GetAddress(mailbox.EMail))
                                .InColumnValue(MailboxTable.Columns.AddressName, mailbox.Name)
                                .InColumnValue(MailboxTable.Columns.Password, EncryptPassword(mailbox.Password))
                                .InColumnValue(MailboxTable.Columns.MsgCountLast, mailbox.MessagesCount)
                                .InColumnValue(MailboxTable.Columns.SmtpPassword,
                                    mailbox.SmtpAuth
                                        ? EncryptPassword(mailbox.SmtpPassword)
                                        : "")
                                .InColumnValue(MailboxTable.Columns.SizeLast, mailbox.Size)
                                .InColumnValue(MailboxTable.Columns.LoginDelay, loginDelayTime)
                                .InColumnValue(MailboxTable.Columns.Enabled, true)
                                .InColumnValue(MailboxTable.Columns.Imap, mailbox.Imap)
                                .InColumnValue(MailboxTable.Columns.BeginDate, mailbox.BeginDate)
                                .InColumnValue(MailboxTable.Columns.OAuthType, mailbox.OAuthType)
                                .InColumnValue(MailboxTable.Columns.OAuthToken,
                                    !string.IsNullOrEmpty(mailbox.OAuthToken)
                                        ? EncryptPassword(mailbox.OAuthToken)
                                        : "")
                                .InColumnValue(MailboxTable.Columns.SmtpServerId, mailbox.SmtpServerId)
                                .InColumnValue(MailboxTable.Columns.ServerId, mailbox.InServerId)
                                .InColumnValue(MailboxTable.Columns.DateCreated, utcNow)
                                .Identity(0, 0, true));

                        mailbox.MailBoxId = result;
                        mailbox.Enabled = true;
                    }
                    else
                    {
                        mailbox.MailBoxId = idMailbox;

                        var queryUpdate = new SqlUpdate(MailboxTable.Name)
                            .Where(MailboxTable.Columns.Id, idMailbox)
                            .Set(MailboxTable.Columns.Tenant, mailbox.TenantId)
                            .Set(MailboxTable.Columns.User, mailbox.UserId)
                            .Set(MailboxTable.Columns.Address, GetAddress(mailbox.EMail))
                            .Set(MailboxTable.Columns.AddressName, mailbox.Name)
                            .Set(MailboxTable.Columns.Password, EncryptPassword(mailbox.Password))
                            .Set(MailboxTable.Columns.MsgCountLast, mailbox.MessagesCount)
                            .Set(MailboxTable.Columns.SmtpPassword,
                                mailbox.SmtpAuth
                                    ? EncryptPassword(mailbox.SmtpPassword)
                                    : "")
                            .Set(MailboxTable.Columns.SizeLast, mailbox.Size)
                            .Set(MailboxTable.Columns.LoginDelay, loginDelayTime)
                            .Set(MailboxTable.Columns.IsRemoved, false)
                            .Set(MailboxTable.Columns.Imap, mailbox.Imap)
                            .Set(MailboxTable.Columns.BeginDate, mailbox.BeginDate)
                            .Set(MailboxTable.Columns.OAuthType, mailbox.OAuthType)
                            .Set(MailboxTable.Columns.SmtpServerId, mailbox.SmtpServerId)
                            .Set(MailboxTable.Columns.ServerId, mailbox.InServerId);

                        if (!string.IsNullOrEmpty(mailbox.OAuthToken) && mailbox.AccessTokenRefreshed)
                        {
                            queryUpdate.Set(MailboxTable.Columns.OAuthToken, EncryptPassword(mailbox.OAuthToken));
                        }

                        if (mailbox.BeginDate == MinBeginDate)
                        {
                            var currentMailbox = GetMailBox(idMailbox);

                            if (currentMailbox == null)
                                throw new ItemNotFoundException("Mailbox was removed");

                            if (mailbox.BeginDate != currentMailbox.BeginDate)
                            {
                                foreach (var folderName in currentMailbox.ImapIntervals.Keys)
                                {
                                    var imapIntervals =
                                        new ImapIntervals(currentMailbox.ImapIntervals[folderName].UnhandledUidIntervals);

                                    if (currentMailbox.ImapIntervals[folderName].BeginDateUid != 1)
                                        imapIntervals.AddUnhandledInterval(
                                            new UidInterval(1, currentMailbox.ImapIntervals[
                                                folderName].BeginDateUid));

                                    currentMailbox.ImapIntervals[folderName].UnhandledUidIntervals =
                                        new List<int>(imapIntervals.ToIndexes());

                                    currentMailbox.ImapIntervals[folderName].BeginDateUid = 1;
                                }

                                queryUpdate.Set(MailboxTable.Columns.ImapIntervals, currentMailbox.ImapIntervalsJson);
                            }

                        }

                        result = db.ExecuteNonQuery(queryUpdate);
                    }

                    tx.Commit();

                    return result > 0;
                }

            }
        }

        public List<AccountInfo> GetAccountInfo(int tenant, string user)
        {
            const string mailbox_alias = "ma";
            const string server_address = "sa";
            const string server_domain = "sd";
            const string group_x_address = "ga";
            const string server_group = "sg";

            var query = new SqlQuery(MailboxTable.Name.Alias(mailbox_alias))
                .LeftOuterJoin(AddressTable.Name.Alias(server_address),
                               Exp.EqColumns(MailboxTable.Columns.Id.Prefix(mailbox_alias),
                                             AddressTable.Columns.MailboxId.Prefix(server_address)))
                .LeftOuterJoin(DomainTable.Name.Alias(server_domain),
                               Exp.EqColumns(AddressTable.Columns.DomainId.Prefix(server_address),
                                             DomainTable.Columns.Id.Prefix(server_domain)))
                .LeftOuterJoin(MailGroupXAddressesTable.Name.Alias(group_x_address),
                               Exp.EqColumns(AddressTable.Columns.Id.Prefix(server_address),
                                             MailGroupXAddressesTable.Columns.AddressId.Prefix(group_x_address)))
                .LeftOuterJoin(MailGroupTable.Name.Alias(server_group),
                               Exp.EqColumns(MailGroupXAddressesTable.Columns.MailGroupId.Prefix(group_x_address),
                                             MailGroupTable.Columns.Id.Prefix(server_group)))
                .Select(MailboxTable.Columns.Id.Prefix(mailbox_alias))
                .Select(MailboxTable.Columns.Address.Prefix(mailbox_alias))
                .Select(MailboxTable.Columns.Enabled.Prefix(mailbox_alias))
                .Select(MailboxTable.Columns.AddressName.Prefix(mailbox_alias))
                .Select(MailboxTable.Columns.QuotaError.Prefix(mailbox_alias))
                .Select(MailboxTable.Columns.DateAuthError.Prefix(mailbox_alias))
                .Select(MailboxTable.Columns.OAuthToken.Prefix(mailbox_alias))
                .Select(MailboxTable.Columns.IsTeamlabMailbox.Prefix(mailbox_alias))
                .Select(MailboxTable.Columns.EmailInFolder.Prefix(mailbox_alias))
                .Select(AddressTable.Columns.Id.Prefix(server_address))
                .Select(AddressTable.Columns.AddressName.Prefix(server_address))
                .Select(AddressTable.Columns.IsAlias.Prefix(server_address))
                .Select(DomainTable.Columns.Id.Prefix(server_domain))
                .Select(DomainTable.Columns.DomainName.Prefix(server_domain))
                .Select(MailGroupTable.Columns.Id.Prefix(server_group))
                .Select(MailGroupTable.Columns.Address.Prefix(server_group))
                .Select(DomainTable.Columns.Tenant.Prefix(server_domain))
                .Where(MailboxTable.Columns.IsRemoved.Prefix(mailbox_alias), false)
                .Where(MailboxTable.Columns.Tenant.Prefix(mailbox_alias), tenant)
                .Where(MailboxTable.Columns.User.Prefix(mailbox_alias), user.ToLowerInvariant())
                .OrderBy(AddressTable.Columns.IsAlias.Prefix(server_address), true);

            List<object[]> result;
            List<MailSignature> signatures;
            List<MailAutoreply> autoreplies;
            using (var db = GetDb())
            {
                result = db.ExecuteList(query);
                var mailboxIds = result.ConvertAll(r =>
                    Convert.ToInt32(r[(int)MailAccountFieldSelectPosition.Id])).Distinct().ToList();
                signatures = GetMailboxesSignatures(mailboxIds, tenant, db);
                autoreplies = GetMailboxesAutoreplies(mailboxIds, tenant, db);
            }

            return ToAccountInfo(result, signatures, autoreplies);
        }

        public List<MailBox> GetMailBoxes(int tenant, string user, bool unremoved = true)
        {
            var where = Exp.Eq(MailboxTable.Columns.Tenant, tenant) &
                        Exp.Eq(MailboxTable.Columns.User, user.ToLowerInvariant());

            if (unremoved)
                where &= Exp.Eq(MailboxTable.Columns.IsRemoved.Prefix(MAILBOX_ALIAS), false);

            return GetMailBoxes(where);
        }

        public MailBox GetMailBox(int tenant, string user, MailAddress email)
        {
            return GetMailBoxes(Exp.Eq(MailboxTable.Columns.Address.Prefix(MAILBOX_ALIAS), GetAddress(email)) &
                Exp.Eq(MailboxTable.Columns.Tenant.Prefix(MAILBOX_ALIAS), tenant) &
                Exp.Eq(MailboxTable.Columns.User.Prefix(MAILBOX_ALIAS), user.ToLowerInvariant()) &
                Exp.Eq(MailboxTable.Columns.IsRemoved.Prefix(MAILBOX_ALIAS), false), false)
                .SingleOrDefault();
        }

        public MailBox GetServerMailBox(int tenant, int mailboxId, DbManager dbManager)
        {
            var mailbox = GetUnremovedMailBox(mailboxId);

            if (mailbox == null || !mailbox.IsTeamlab || mailbox.TenantId != tenant)
                return null;

            return mailbox;
        }

        public MailBox GetUnremovedMailBox(int mailboxId)
        {
            return GetMailBoxes(Exp.Eq(MailboxTable.Columns.Id.Prefix(MAILBOX_ALIAS), mailboxId) &
                                Exp.Eq(MailboxTable.Columns.IsRemoved.Prefix(MAILBOX_ALIAS), false))
                .SingleOrDefault();
        }

        public MailBox GetMailBox(int mailboxId, string userId = null, int tenant = -1)
        {
            var where = Exp.Eq(MailboxTable.Columns.Id.Prefix(MAILBOX_ALIAS), mailboxId);

            if (!string.IsNullOrEmpty(userId))
            {
                where = where & Exp.Eq(MailboxTable.Columns.User, userId);
            }

            if (tenant > -1)
            {
                where = where & Exp.Eq(MailboxTable.Columns.Tenant, tenant);
            }

            return GetMailBoxes(where, false)
                .SingleOrDefault();
        }

        public MailBox GetNextMailBox(int mailboxId, string userId = null, int tenant = -1)
        {
            var where = Exp.Gt(MailboxTable.Columns.Id.Prefix(MAILBOX_ALIAS), mailboxId);

            if (!string.IsNullOrEmpty(userId))
            {
                where = where & Exp.Eq(MailboxTable.Columns.User, userId);
            }

            if (tenant > -1)
            {
                where = where & Exp.Eq(MailboxTable.Columns.Tenant, tenant);
            }

            using (var db = GetDb())
            {
                var query = GetSelectMailBoxFieldsQuery()
                .Where(where)
                .OrderBy(MailboxTable.Columns.Id.Prefix(MAILBOX_ALIAS), true)
                .SetMaxResults(1);

                var mailbox = db.ExecuteList(query)
                            .ConvertAll(ToMailBox)
                            .SingleOrDefault();

                return mailbox;
            }

        }

        public void GetMailboxesRange(out int minMailboxId, out int maxMailboxId, string userId = null, int tenant = -1)
        {
            minMailboxId = 0;
            maxMailboxId = 0;

            using (var db = GetDb())
            {
                var selectQuery = new SqlQuery(MailboxTable.Name)
                    .SelectMin(MailboxTable.Columns.Id)
                    .SelectMax(MailboxTable.Columns.Id);

                if (!string.IsNullOrEmpty(userId))
                {
                    selectQuery.Where(Exp.Eq(MailboxTable.Columns.User, userId));
                }

                if (tenant > -1)
                {
                    selectQuery.Where(Exp.Eq(MailboxTable.Columns.Tenant, tenant));
                }

                var result = db.ExecuteList(selectQuery)
                                       .ConvertAll(r => new
                                       {
                                           minMailboxId = Convert.ToInt32(r[0]),
                                           maxMailboxId = Convert.ToInt32(r[1])
                                       })
                                       .FirstOrDefault();

                if (result == null) return;

                minMailboxId = result.minMailboxId;
                maxMailboxId = result.maxMailboxId;
            }
        }

        public MailMessage GetNextMailBoxNessage(MailBox mailbox, int messageId, bool onlyUnremoved = false)
        {
            using (var db = GetDb())
            {
                var query = new SqlQuery(MailTable.Name)
                    .Select(MailTable.Columns.Id)
                    .Where(GetUserWhere(mailbox.UserId, mailbox.TenantId))
                    .Where(MailTable.Columns.MailboxId, mailbox.MailBoxId)
                    .Where(Exp.Gt(MailTable.Columns.Id, messageId))
                    .OrderBy(MailTable.Columns.Id, true)
                    .SetMaxResults(1);

                if (onlyUnremoved)
                    query.Where(MailTable.Columns.IsRemoved, false);

                var nextMessageId = db.ExecuteScalar<int>(query);

                var message = GetMailInfo(mailbox.TenantId, mailbox.UserId, nextMessageId, new MailMessage.Options
                {
                    LoadImages = false,
                    LoadBody = false,
                    NeedProxyHttp = false,
                    OnlyUnremoved = onlyUnremoved
                });

                return message;
            }

        }

        public void GetMailboxMessagesRange(MailBox mailbox, out int minMessageId, out int maxMessageId)
        {
            minMessageId = 0;
            maxMessageId = 0;

            using (var db = GetDb())
            {
                var selectQuery = new SqlQuery(MailTable.Name)
                    .SelectMin(MailTable.Columns.Id)
                    .SelectMax(MailTable.Columns.Id)
                    .Where(GetUserWhere(mailbox.UserId, mailbox.TenantId))
                    .Where(MailTable.Columns.MailboxId, mailbox.MailBoxId);

                var result = db.ExecuteList(selectQuery)
                                       .ConvertAll(r => new
                                       {
                                           minMessageId = Convert.ToInt32(r[0]),
                                           maxMessageId = Convert.ToInt32(r[1])
                                       })
                                       .FirstOrDefault();

                if (result == null) return;

                minMessageId = result.minMessageId;
                maxMessageId = result.maxMessageId;
            }
        }

        public bool EnableMaibox(MailBox mailbox, bool enabled)
        {
            using (var db = GetDb())
            {
                return EnableMaibox(db, mailbox, enabled);
            }
        }

        public bool EnableMaibox(DbManager db, MailBox mailbox, bool enabled)
        {
            var updateQuery = new SqlUpdate(MailboxTable.Name)
                .Where(MailboxTable.Columns.Id, mailbox.MailBoxId)
                .Where(GetUserWhere(mailbox.UserId, mailbox.TenantId))
                .Where(MailboxTable.Columns.IsRemoved, false)
                .Set(MailboxTable.Columns.Enabled, enabled);

            if (enabled)
                updateQuery.Set(MailboxTable.Columns.DateAuthError, null);

            var result = db.ExecuteNonQuery(updateQuery);

            return result > 0;
        }

        public MailOperationStatus RemoveMailbox(MailBox mailbox,
            Func<DistributedTask, string> translateMailOperationStatus = null)
        {
            var tenant = CoreContext.TenantManager.GetCurrentTenant();
            var user = SecurityContext.CurrentAccount;

            var operations = MailOperations.GetTasks()
                .Where(o =>
                {
                    var oTenant = o.GetProperty<int>(MailOperation.TENANT);
                    var oUser = o.GetProperty<string>(MailOperation.OWNER);
                    var oType = o.GetProperty<MailOperationType>(MailOperation.OPERATION_TYPE);
                    return oTenant == tenant.TenantId &&
                           oUser == user.ID.ToString() &&
                           oType == MailOperationType.RemoveMailbox;
                })
                .ToList();

            var sameOperation = operations.FirstOrDefault(o =>
            {
                var oSource = o.GetProperty<string>(MailOperation.SOURCE);
                return oSource == mailbox.MailBoxId.ToString();
            });

            if (sameOperation != null)
            {
                return GetMailOperationStatus(sameOperation.Id, translateMailOperationStatus);
            }

            var runningOperation = operations.FirstOrDefault(o => o.Status <= DistributedTaskStatus.Running);

            if (runningOperation != null)
                throw new MailOperationAlreadyRunningException("Remove mailbox operation already running.");

            var op = new MailRemoveMailboxOperation(tenant, user, mailbox, this);

            return QueueTask(op, translateMailOperationStatus);
        }

        public void RemoveMailBox(MailBox mailbox, bool needRecalculateFolders = true)
        {
            if (mailbox.MailBoxId <= 0)
                throw new Exception("MailBox id is 0");

            long freedQuotaSize;

            using (var db = GetDb())
            {
                using (var tx = db.BeginTransaction())
                {
                    freedQuotaSize = RemoveMailBoxInfo(mailbox, db);

                    tx.Commit();
                }
            }

            QuotaUsedDelete(mailbox.TenantId, freedQuotaSize);

            if (!needRecalculateFolders)
                return;

            RecalculateFolders();
        }

        public void RemoveMailBox(DbManager db, MailBox mailbox, bool needRecalculateFolders = true)
        {
            if (mailbox.MailBoxId <= 0)
                throw new Exception("MailBox id is 0");

            var freedQuotaSize = RemoveMailBoxInfo(mailbox, db);

            QuotaUsedDelete(mailbox.TenantId, freedQuotaSize);

            if (!needRecalculateFolders)
                return;

            RecalculateFolders();
        }

        /// <summary>
        /// Set mailbox removed
        /// </summary>
        /// <param name="mailBox"></param>
        /// <returns>Return freed quota value</returns>
        public long RemoveMailBoxInfo(MailBox mailBox)
        {
            long freedQuotaSize;

            using (var db = GetDb())
            {
                using (var tx = db.BeginTransaction())
                {
                    freedQuotaSize = RemoveMailBoxInfo(mailBox, db);

                    tx.Commit();
                }
            }

            return freedQuotaSize;
        }

        /// <summary>
        /// Set mailbox removed
        /// </summary>
        /// <param name="mailBox"></param>
        /// <param name="db"></param>
        /// <returns>Return freed quota value</returns>
        private long RemoveMailBoxInfo(MailBox mailBox, DbManager db)
        {
            if (mailBox.MailBoxId <= 0)
                throw new Exception("MailBox id is 0");

            var setMaiboxRemovedQuery =
                new SqlUpdate(MailboxTable.Name)
                    .Set(MailboxTable.Columns.IsRemoved, true)
                    .Where(MailboxTable.Columns.Id, mailBox.MailBoxId);

            db.ExecuteNonQuery(setMaiboxRemovedQuery);

            var deleteChainsQuery = new SqlDelete(ChainTable.Name)
                .Where(GetUserWhere(mailBox.UserId, mailBox.TenantId))
                .Where(ChainTable.Columns.MailboxId, mailBox.MailBoxId)
                .Where(Exp.In(ChainTable.Columns.Folder,
                    new[]
                    {
                        MailFolder.Ids.inbox, MailFolder.Ids.sent, MailFolder.Ids.drafts, MailFolder.Ids.trash,
                        MailFolder.Ids.spam, MailFolder.Ids.temp
                    }));

            db.ExecuteNonQuery(deleteChainsQuery);

            var deleteChainXCrmQuery =
                new SqlDelete(ChainXCrmContactEntity.Name)
                    .Where(ChainXCrmContactEntity.Columns.Tenant, mailBox.TenantId)
                    .Where(ChainXCrmContactEntity.Columns.MailboxId, mailBox.MailBoxId);

            db.ExecuteNonQuery(deleteChainXCrmQuery);

            var setMailRemovedQuery = new SqlUpdate(MailTable.Name)
                .Set(MailTable.Columns.IsRemoved, true)
                .Where(MailTable.Columns.MailboxId, mailBox.MailBoxId)
                .Where(GetUserWhere(mailBox.UserId, mailBox.TenantId));

            db.ExecuteNonQuery(setMailRemovedQuery);

            var getTotalAttachmentsSizeQuery = string.Format(
                "select sum(a.size) from {0} a inner join {1} m on a.{2} = m.{3} where m.{4} = @mailbox_id and m.{5} = @tid and a.{5} = @tid and a.{6} != @need_remove",
                AttachmentTable.Name,
                MailTable.Name,
                AttachmentTable.Columns.MailId,
                MailTable.Columns.Id,
                MailTable.Columns.MailboxId,
                MailTable.Columns.Tenant,
                AttachmentTable.Columns.NeedRemove);

            var totalAttachmentsSize = db.ExecuteScalar<long>(getTotalAttachmentsSizeQuery,
                new {tid = mailBox.TenantId, need_remove = true, mailbox_id = mailBox.MailBoxId});

            var setAttachmentsRemovedQuery =
                string.Format(
                    "update {0} a inner join {1} m on a.{2} = m.{3} set a.{4} = @need_remove where m.{5} = @mailbox_id",
                    AttachmentTable.Name, MailTable.Name, AttachmentTable.Columns.MailId, MailTable.Columns.Id,
                    AttachmentTable.Columns.NeedRemove, MailTable.Columns.MailboxId);

            db.ExecuteNonQuery(setAttachmentsRemovedQuery, new { need_remove = true, mailbox_id = mailBox.MailBoxId });

            var getAffectedTagsQuery =
                string.Format("select t.{0} from {1} t inner join {2} m on t.{3} = m.{4} where m.{5} = @mailbox_id",
                    TagMailTable.Columns.TagId, TagMailTable.Name, MailTable.Name, TagMailTable.Columns.MailId,
                    MailTable.Columns.Id, MailTable.Columns.MailboxId);

            var affectedTags = db.ExecuteList(getAffectedTagsQuery, new { mailbox_id = mailBox.MailBoxId })
                .ConvertAll(r => Convert.ToInt32(r[0]))
                .Distinct();

            var deleteTagMailQuery =
                string.Format("delete t from {0} t inner join {1} m on t.{2} = m.{3} where m.{4} = @mailbox_id",
                    TagMailTable.Name, MailTable.Name, TagMailTable.Columns.MailId, MailTable.Columns.Id,
                    MailTable.Columns.MailboxId);

            db.ExecuteNonQuery(deleteTagMailQuery, new { mailbox_id = mailBox.MailBoxId });

            UpdateTagsCount(db, mailBox.TenantId, mailBox.UserId, affectedTags);

            var signatureDal = new SignatureDal(db);
            signatureDal.DeleteSignature(mailBox.MailBoxId, mailBox.TenantId);

            var autoreplyDal = new AutoreplyDal(db);
            autoreplyDal.DeleteAutoreply(mailBox.MailBoxId, mailBox.TenantId);

            DeleteAlerts(db, mailBox.TenantId, mailBox.UserId, mailBox.MailBoxId);

            return totalAttachmentsSize;
        }

        private ClientConfig GetStoredMailBoxSettings(string host)
        {
            using (var db = GetDb())
            {
                const string domain_alias = "d", provider_alias = "p", server_alias = "s";

                var query = new SqlQuery(MailboxDomainTable.Name.Alias(domain_alias))
                    .Select(
                        MailboxProviderTable.Columns.ProviderName.Prefix(provider_alias),
                        MailboxProviderTable.Columns.DisplayName.Prefix(provider_alias),
                        MailboxProviderTable.Columns.DisplayShortName.Prefix(provider_alias),
                        MailboxProviderTable.Columns.Documentation.Prefix(provider_alias),
                        MailboxServerTable.Columns.Hostname.Prefix(server_alias),
                        MailboxServerTable.Columns.Port.Prefix(server_alias),
                        MailboxServerTable.Columns.Type.Prefix(server_alias),
                        MailboxServerTable.Columns.SocketType.Prefix(server_alias),
                        MailboxServerTable.Columns.Username.Prefix(server_alias),
                        MailboxServerTable.Columns.Authentication.Prefix(server_alias))
                    .InnerJoin(MailboxProviderTable.Name.Alias(provider_alias),
                        Exp.EqColumns(MailboxDomainTable.Columns.ProviderId.Prefix(domain_alias),
                            MailboxProviderTable.Columns.Id.Prefix(provider_alias)))
                    .InnerJoin(MailboxServerTable.Name.Alias(server_alias),
                        Exp.EqColumns(MailboxServerTable.Columns.ProviderId.Prefix(server_alias),
                            MailboxProviderTable.Columns.Id.Prefix(provider_alias)))
                    .Where(MailboxDomainTable.Columns.DomainName.Prefix(domain_alias), host)
                    .Where(MailboxServerTable.Columns.IsUserData.Prefix(server_alias), false);

                var providerSettings = db.ExecuteList(query)
                    .ConvertAll(r => new
                    {
                        Provider = new
                        {
                            Name = Convert.ToString(r[0]),
                            DisplayName = Convert.ToString(r[1]),
                            DisplayShortName = Convert.ToString(r[2]),
                            Url = Convert.ToString(r[3])
                        },
                        Server = new
                        {
                            Host = Convert.ToString(r[4]),
                            Port = Convert.ToInt32(r[5]),
                            Type = Convert.ToString(r[6]),
                            SocketType = Convert.ToString(r[7]),
                            Username = Convert.ToString(r[8]),
                            Authentication = Convert.ToString(r[9])
                        }

                    })
                    .GroupBy(s => s.Provider, s => s.Server,
                        (key, g) => new
                        {
                            Provider = key,
                            Servers = g.ToList()
                        })
                    .FirstOrDefault();

                if (providerSettings == null)
                {
                    return null;
                }

                var config = new ClientConfig();

                config.EmailProvider.Domain.Add(host);
                config.EmailProvider.Id = providerSettings.Provider.Name;
                config.EmailProvider.DisplayName = providerSettings.Provider.DisplayName;
                config.EmailProvider.DisplayShortName = providerSettings.Provider.DisplayShortName;
                config.EmailProvider.Documentation.Url = providerSettings.Provider.Url;

                providerSettings.Servers.ForEach(serv =>
                {
                    if (serv.Type == "smtp")
                    {
                        config.EmailProvider.OutgoingServer.Add(
                            new ClientConfigEmailProviderOutgoingServer
                            {
                                Type = serv.Type,
                                SocketType = serv.SocketType,
                                Hostname = serv.Host,
                                Port = serv.Port,
                                Username = serv.Username,
                                Authentication = serv.Authentication
                            });
                    }
                    else
                    {
                        config.EmailProvider.IncomingServer.Add(
                            new ClientConfigEmailProviderIncomingServer
                            {
                                Type = serv.Type,
                                SocketType = serv.SocketType,
                                Hostname = serv.Host,
                                Port = serv.Port,
                                Username = serv.Username,
                                Authentication = serv.Authentication
                            });
                    }

                });

                if (!config.EmailProvider.IncomingServer.Any() || !config.EmailProvider.OutgoingServer.Any())
                    return null;

                return config;
            }
        }

        public ClientConfig GetMailBoxSettings(string host)
        {
            var config = GetStoredMailBoxSettings(host);
            return config ?? SearchBusinessVendorsSettings(host);
        }

        public bool SetMailBoxSettings(ClientConfig config)
        {
            try
            {
                if (string.IsNullOrEmpty(config.EmailProvider.Id) ||
                    config.EmailProvider.IncomingServer == null ||
                    !config.EmailProvider.IncomingServer.Any() ||
                    config.EmailProvider.OutgoingServer == null ||
                    !config.EmailProvider.OutgoingServer.Any())
                    throw new Exception("Incorrect config");

                using (var db = GetDb())
                {
                    using (var tx = db.BeginTransaction())
                    {
                        var idProvider = db.ExecuteScalar<int>(
                            new SqlQuery(MailboxProviderTable.Name)
                                .Select(MailboxProviderTable.Columns.Id)
                                .Where(MailboxProviderTable.Columns.ProviderName, config.EmailProvider.Id));

                        if (idProvider < 1)
                        {
                            idProvider = db.ExecuteScalar<int>(
                                new SqlInsert(MailboxProviderTable.Name)
                                    .InColumnValue(MailboxProviderTable.Columns.Id, 0)
                                    .InColumnValue(MailboxProviderTable.Columns.ProviderName, config.EmailProvider.Id)
                                    .InColumnValue(MailboxProviderTable.Columns.DisplayName, config.EmailProvider.DisplayName)
                                    .InColumnValue(MailboxProviderTable.Columns.DisplayShortName,
                                                   config.EmailProvider.DisplayShortName)
                                    .InColumnValue(MailboxProviderTable.Columns.Documentation,
                                                   config.EmailProvider.Documentation.Url)
                                    .Identity(0, 0, true));

                            if (idProvider < 1)
                                throw new Exception("id_provider not saved in DB");
                        }

                        var insertQuery = new SqlInsert(MailboxDomainTable.Name)
                            .IgnoreExists(true)
                            .InColumns(MailboxDomainTable.Columns.ProviderId, MailboxDomainTable.Columns.DomainName);

                        config.EmailProvider.Domain
                              .ForEach(domain =>
                                       insertQuery
                                           .Values(idProvider, domain));

                        db.ExecuteNonQuery(insertQuery);

                        insertQuery = new SqlInsert(MailboxServerTable.Name)
                            .IgnoreExists(true)
                            .InColumns(
                                MailboxServerTable.Columns.ProviderId,
                                MailboxServerTable.Columns.Type,
                                MailboxServerTable.Columns.Hostname,
                                MailboxServerTable.Columns.Port,
                                MailboxServerTable.Columns.SocketType,
                                MailboxServerTable.Columns.Username,
                                MailboxServerTable.Columns.Authentication
                            );

                        config.EmailProvider.IncomingServer
                              .ForEach(server =>
                                       insertQuery
                                           .Values(idProvider,
                                                   server.Type,
                                                   server.Hostname,
                                                   server.Port,
                                                   server.SocketType,
                                                   server.Username,
                                                   server.Authentication));

                        config.EmailProvider.OutgoingServer
                              .ForEach(server =>
                                       insertQuery
                                           .Values(idProvider,
                                                   server.Type,
                                                   server.Hostname,
                                                   server.Port,
                                                   server.SocketType,
                                                   server.Username,
                                                   server.Authentication));

                        db.ExecuteNonQuery(insertQuery);

                        tx.Commit();
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public List<string> SearchAccountEmails(int tenant, string user, string searchText)
        {
            var emails = new List<string>();
            var accounts = CachedAccounts.Get(user);
            if (accounts == null)
            {
                accounts = GetAccountInfo(tenant, user);
                CachedAccounts.Set(user, accounts);
            }

            foreach (var account in accounts)
            {
                var email = string.IsNullOrEmpty(account.Name)
                                ? account.Email
                                : MailUtil.CreateFullEmail(account.Name, account.Email);
                emails.Add(email);

                foreach (var alias in account.Aliases)
                {
                    email = string.IsNullOrEmpty(account.Name)
                                ? account.Email
                                : MailUtil.CreateFullEmail(account.Name, alias.Email);
                    emails.Add(email);
                }

                foreach (var @group in account.Groups.Where(@group => emails.IndexOf(@group.Email) == -1))
                {
                    emails.Add(@group.Email);
                }
            }

            return emails.Where(e => e.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) > -1).ToList();
        }

        public MailBox ObtainMailboxSettings(int tenant, string user, string email, string password,
            AuthorizationServiceType type, bool? imap, bool isNullNeeded)
        {
            var address = new MailAddress(email);

            var host = address.Host.ToLowerInvariant();

            if (type == AuthorizationServiceType.Google) host = GOOGLE_HOST;

            MailBox initialMailbox = null;

            if (imap.HasValue)
            {
                try
                {
                    var settings = GetMailBoxSettings(host);

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
                            var incomingServerLogin = FormatLoginFromDb(incomingServer.Username, address);

                            if (!string.IsNullOrEmpty(outgoingServer.Username))
                            {
                                outgoingServerLogin = FormatLoginFromDb(outgoingServer.Username, address);
                            }

                            initialMailbox = new MailBox
                            {
                                EMail = address,
                                Name = "",

                                Account = incomingServerLogin,
                                Password = password,
                                Server = FormatServerFromDb(incomingServer.Hostname, host),
                                Port = incomingServer.Port,
                                Encryption = incomingServer.SocketType.ToEncryptionType(),
                                SmtpEncryption = outgoingServer.SocketType.ToEncryptionType(),
                                Authentication = incomingServer.Authentication.ToSaslMechanism(),
                                SmtpAuthentication = outgoingServer.Authentication.ToSaslMechanism(),
                                Imap = imap.Value,

                                SmtpAccount = outgoingServerLogin,
                                SmtpPassword = password,
                                SmtpServer = FormatServerFromDb(outgoingServer.Hostname, host),
                                SmtpPort = outgoingServer.Port,
                                Enabled = true,
                                TenantId = tenant,
                                UserId = user,
                                BeginDate = DateTime.UtcNow.Subtract(new TimeSpan(MailBox.DefaultMailLimitedTimeDelta)),
                                OAuthType = (byte) type
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
            return new MailBox
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
                    TenantId = tenant,
                    UserId = user,
                    BeginDate = DateTime.UtcNow.Subtract(new TimeSpan(MailBox.DefaultMailLimitedTimeDelta)),
                    Authentication = SaslMechanism.Login,
                    SmtpAuthentication = SaslMechanism.Login
                };
        }

        public int SaveMailBoxServerSettings(MailAddress email, MailBoxServerSettings settings,
            string serverType, AuthorizationServiceType authorizationType)
        {
            using (var db = GetDb())
            {
                return SaveMailBoxServerSettings(db, email, settings, serverType, authorizationType);
            }
        }

        public int SaveMailBoxServerSettings(DbManager db, MailAddress email, MailBoxServerSettings settings,
            string serverType, AuthorizationServiceType authorizationType, int id = -1)
        {
            //TODO: Check id of stored setting if it's not equals -1 for removing old settings

            var host = (authorizationType == AuthorizationServiceType.Google) ? GOOGLE_HOST : email.Host;

            var selectQuery = new SqlQuery(MailboxDomainTable.Name)
                .Select(MailboxDomainTable.Columns.ProviderId)
                .Where(MailboxProviderTable.Columns.ProviderName, host);

            var providerId = db.ExecuteScalar<int>(selectQuery);

            SqlInsert insertQuery;

            //Save Mailbox provider if not exists
            if (providerId == 0)
            {
                insertQuery = new SqlInsert(MailboxProviderTable.Name)
                    .InColumnValue(MailboxProviderTable.Columns.Id, 0)
                    .InColumnValue(MailboxProviderTable.Columns.ProviderName, email.Host)
                    .Identity(0, 0, true);

                providerId = db.ExecuteScalar<int>(insertQuery);

                insertQuery = new SqlInsert(MailboxDomainTable.Name)
                    .InColumnValue(MailboxDomainTable.Columns.ProviderId, providerId)
                    .InColumnValue(MailboxDomainTable.Columns.DomainName, email.Host);

                db.ExecuteNonQuery(insertQuery);
            }

            //Identify mask for account name
            var accountNameMask = "";
            if (settings.AuthenticationType != SaslMechanism.None)
            {
                accountNameMask = GetLoginFormatFrom(email, settings.AccountName);
                if (string.IsNullOrEmpty(accountNameMask))
                {
                    accountNameMask = settings.AccountName;
                }
            }

            selectQuery = new SqlQuery(MailboxServerTable.Name)
                .Select(MailboxServerTable.Columns.Id)
                .Where(MailboxServerTable.Columns.ProviderId, providerId)
                .Where(MailboxServerTable.Columns.Type, serverType)
                .Where(MailboxServerTable.Columns.Hostname, settings.Url)
                .Where(MailboxServerTable.Columns.Port, settings.Port)
                .Where(MailboxServerTable.Columns.SocketType,
                    settings.EncryptionType.ToNameString())
                .Where(MailboxServerTable.Columns.Authentication,
                    settings.AuthenticationType.ToNameString())
                .Where(MailboxServerTable.Columns.Username, accountNameMask)
                .Where(MailboxServerTable.Columns.IsUserData, false);

            var settingsId = db.ExecuteScalar<int>(selectQuery);

            if (settingsId != 0)
                return settingsId;

            insertQuery = new SqlInsert(MailboxServerTable.Name)
                .InColumnValue(MailboxServerTable.Columns.Id, 0)
                .InColumnValue(MailboxServerTable.Columns.ProviderId, providerId)
                .InColumnValue(MailboxServerTable.Columns.Type, serverType)
                .InColumnValue(MailboxServerTable.Columns.Hostname, settings.Url)
                .InColumnValue(MailboxServerTable.Columns.Port, settings.Port)
                .InColumnValue(MailboxServerTable.Columns.SocketType,
                    settings.EncryptionType.ToNameString())
                .InColumnValue(MailboxServerTable.Columns.Authentication,
                    settings.AuthenticationType.ToNameString())
                .InColumnValue(MailboxServerTable.Columns.Username, accountNameMask)
                .InColumnValue(MailboxServerTable.Columns.IsUserData, true)
                .Identity(0, 0, true);

            settingsId = db.ExecuteScalar<int>(insertQuery);

            return settingsId;
        }

        public MailSignature GetMailboxSignature(int mailboxId, string user, int tenant)
        {
            using (var db = GetDb())
            {
                CheckMailboxOwnage(mailboxId, user, tenant, db);

                var signatureDal = new SignatureDal(db);
                return signatureDal.GetSignature(mailboxId, tenant);
            }
        }

        public MailSignature UpdateOrCreateMailboxSignature(int mailboxId, string user, int tenant, string html, bool isActive)
        {
            using (var db = GetDb())
            {
                CheckMailboxOwnage(mailboxId, user, tenant, db);

                var signature = new MailSignature(mailboxId, tenant, html, isActive);
                var signatureDal = new SignatureDal(db);
                signatureDal.UpdateOrCreateSignature(signature);
                return signature;
            }
        }

        private static List<MailSignature> GetMailboxesSignatures(List<int> mailboxIds, int tenant, DbManager db)
        {
            var signatureDal = new SignatureDal(db);
            return signatureDal.GetSignatures(mailboxIds, tenant);
        }

        private static List<MailAutoreply> GetMailboxesAutoreplies(List<int> mailboxIds, int tenant, DbManager db)
        {
            var autoreplyDal = new AutoreplyDal(db);
            return autoreplyDal.GetAutoreplies(mailboxIds, tenant);
        }

        public bool SetMailboxEmailInFolder(int tenant, string user, int mailboxId, string emailInFolder)
        {
            using (var db = GetDb())
            {
                var query = new SqlUpdate(MailboxTable.Name)
                    .Set(MailboxTable.Columns.EmailInFolder, "" != emailInFolder ? emailInFolder : null)
                    .Where(GetUserWhere(user, tenant))
                    .Where(MailboxTable.Columns.Id, mailboxId);

                return 0 < db.ExecuteNonQuery(query);
            }
        }

        #endregion

        #region private methods

        private List<MailBox> GetMailBoxes(Exp where, bool needOrderBy = true)
        {
            using (var db = GetDb())
            {
                return GetMailBoxes(where, db, needOrderBy);
            }
        }

        private List<MailBox> GetMailBoxes(Exp where, DbManager db, bool needOrderBy = true)
        {
            var query = GetSelectMailBoxFieldsQuery()
                .Where(where);

            if (needOrderBy)
                query
                    .OrderBy(1, true)
                    .OrderBy(2, true);

            var res = db.ExecuteList(query)
                        .ConvertAll(ToMailBox);

            return res;
        }

        private enum MailBoxFieldSelectPosition
        {
            IdTenant, IdUser, Name, Address, Account, Password, InServer, InPort, SizeLast, MsgCountLast,
            SmtpServer, SmtpPort, SmtpPassword, SmtpAccount, LoginDelay, Id, Enabled, QuotaError, AuthError,
            Imap, BeginDate, OAuthType, OAuthToken, ImapIntervals, OutcomingEncryptionType, IncomingEncryptionType,
            AuthTypeIn, AuthtTypeSmtp, IdSmtpServer, IdInServer, EMailInFolder, IsTeamlabMailbox, IsRemoved,
            AutoreplyTurnOn, AutoreplyOnlyContacts, AutoreplyTurnOnToDate, AutoreplyFromDate, AutoreplyToDate,
            AutoreplySubject, AutoreplyHtml
        }

        private enum MailItemAttachmentSelectPosition
        {
            Id, Name, StoredName, Type, Size, FileNumber, IdStream, Tenant, User, ContentId, MailboxId
        }

        private enum MailAccountFieldSelectPosition
        {
            Id, Address, Enabled, Name, QoutaError, AuthError, RefreshToken, IsTeamlabMailbox, EmailInFolder,
            AliasId, AliasName, IsAlias, DomainId, DomainName, GroupId, GroupAddress, DomainTenant
        }

        const int SELECT_MAIL_BOX_FIELDS_COUNT = 33;
        const int SELECT_MAIL_BOX_FIELDS_COUNT_WITH_AUTOREPLY = 40;
        const int SELECT_MAIL_ITEM_ATTACHMENT_FIELDS_COUNT = 11;
        const int SELECT_ACCOUNT_FIELDS_COUNT = 17;
        const string MAILBOX_ALIAS = "mb";
        const string AUTOREPLY_ALIAS = "ar";
        const string OUT_SERVER_ALIAS = "out_s";
        const string IN_SERVER_ALIAS = "in_s";

        private static SqlQuery GetSelectMailBoxFieldsQuery()
        {
            var fieldsForSelect = new string[SELECT_MAIL_BOX_FIELDS_COUNT];
            fieldsForSelect[(int)MailBoxFieldSelectPosition.IdTenant] = MailboxTable.Columns.Tenant.Prefix(MAILBOX_ALIAS);
            fieldsForSelect[(int)MailBoxFieldSelectPosition.IdUser] = MailboxTable.Columns.User.Prefix(MAILBOX_ALIAS);
            fieldsForSelect[(int)MailBoxFieldSelectPosition.Name] = MailboxTable.Columns.AddressName.Prefix(MAILBOX_ALIAS);
            fieldsForSelect[(int)MailBoxFieldSelectPosition.Address] = MailboxTable.Columns.Address.Prefix(MAILBOX_ALIAS);
            fieldsForSelect[(int)MailBoxFieldSelectPosition.Account] = MailboxServerTable.Columns.Username.Prefix(IN_SERVER_ALIAS);
            fieldsForSelect[(int)MailBoxFieldSelectPosition.Password] = MailboxTable.Columns.Password.Prefix(MAILBOX_ALIAS);
            fieldsForSelect[(int)MailBoxFieldSelectPosition.InServer] = MailboxServerTable.Columns.Hostname.Prefix(IN_SERVER_ALIAS);
            fieldsForSelect[(int)MailBoxFieldSelectPosition.InPort] = MailboxServerTable.Columns.Port.Prefix(IN_SERVER_ALIAS);
            fieldsForSelect[(int)MailBoxFieldSelectPosition.SizeLast] = MailboxTable.Columns.SizeLast.Prefix(MAILBOX_ALIAS);
            fieldsForSelect[(int)MailBoxFieldSelectPosition.MsgCountLast] = MailboxTable.Columns.MsgCountLast.Prefix(MAILBOX_ALIAS);
            fieldsForSelect[(int)MailBoxFieldSelectPosition.SmtpServer] = MailboxServerTable.Columns.Hostname.Prefix(OUT_SERVER_ALIAS);
            fieldsForSelect[(int)MailBoxFieldSelectPosition.SmtpPort] = MailboxServerTable.Columns.Port.Prefix(OUT_SERVER_ALIAS);
            fieldsForSelect[(int)MailBoxFieldSelectPosition.SmtpPassword] = MailboxTable.Columns.SmtpPassword.Prefix(MAILBOX_ALIAS);
            fieldsForSelect[(int)MailBoxFieldSelectPosition.SmtpAccount] = MailboxServerTable.Columns.Username.Prefix(OUT_SERVER_ALIAS);
            fieldsForSelect[(int)MailBoxFieldSelectPosition.LoginDelay] = MailboxTable.Columns.LoginDelay.Prefix(MAILBOX_ALIAS);
            fieldsForSelect[(int)MailBoxFieldSelectPosition.Id] = MailboxTable.Columns.Id.Prefix(MAILBOX_ALIAS);
            fieldsForSelect[(int)MailBoxFieldSelectPosition.Enabled] = MailboxTable.Columns.Enabled.Prefix(MAILBOX_ALIAS);
            fieldsForSelect[(int)MailBoxFieldSelectPosition.QuotaError] = MailboxTable.Columns.QuotaError.Prefix(MAILBOX_ALIAS);
            fieldsForSelect[(int)MailBoxFieldSelectPosition.AuthError] = MailboxTable.Columns.DateAuthError.Prefix(MAILBOX_ALIAS);
            fieldsForSelect[(int)MailBoxFieldSelectPosition.Imap] = MailboxTable.Columns.Imap.Prefix(MAILBOX_ALIAS);
            fieldsForSelect[(int)MailBoxFieldSelectPosition.BeginDate] = MailboxTable.Columns.BeginDate.Prefix(MAILBOX_ALIAS);
            fieldsForSelect[(int)MailBoxFieldSelectPosition.OAuthType] = MailboxTable.Columns.OAuthType.Prefix(MAILBOX_ALIAS);
            fieldsForSelect[(int)MailBoxFieldSelectPosition.OAuthToken] = MailboxTable.Columns.OAuthToken.Prefix(MAILBOX_ALIAS);
            fieldsForSelect[(int)MailBoxFieldSelectPosition.ImapIntervals] = MailboxTable.Columns.ImapIntervals.Prefix(MAILBOX_ALIAS);
            fieldsForSelect[(int)MailBoxFieldSelectPosition.EMailInFolder] = MailboxTable.Columns.EmailInFolder.Prefix(MAILBOX_ALIAS);
            fieldsForSelect[(int)MailBoxFieldSelectPosition.OutcomingEncryptionType] = MailboxServerTable.Columns.SocketType.Prefix(OUT_SERVER_ALIAS);
            fieldsForSelect[(int)MailBoxFieldSelectPosition.IncomingEncryptionType] = MailboxServerTable.Columns.SocketType.Prefix(IN_SERVER_ALIAS);
            fieldsForSelect[(int)MailBoxFieldSelectPosition.AuthTypeIn] = MailboxServerTable.Columns.Authentication.Prefix(IN_SERVER_ALIAS);
            fieldsForSelect[(int)MailBoxFieldSelectPosition.AuthtTypeSmtp] = MailboxServerTable.Columns.Authentication.Prefix(OUT_SERVER_ALIAS);
            fieldsForSelect[(int)MailBoxFieldSelectPosition.IdSmtpServer] = MailboxServerTable.Columns.Id.Prefix(OUT_SERVER_ALIAS);
            fieldsForSelect[(int)MailBoxFieldSelectPosition.IdInServer] = MailboxServerTable.Columns.Id.Prefix(IN_SERVER_ALIAS);
            fieldsForSelect[(int)MailBoxFieldSelectPosition.IsTeamlabMailbox] = MailboxTable.Columns.IsTeamlabMailbox.Prefix(MAILBOX_ALIAS);
            fieldsForSelect[(int)MailBoxFieldSelectPosition.IsRemoved] = MailboxTable.Columns.IsRemoved.Prefix(MAILBOX_ALIAS);

            return new SqlQuery(MailboxTable.Name.Alias(MAILBOX_ALIAS))
                .InnerJoin(MailboxServerTable.Name.Alias(OUT_SERVER_ALIAS),
                            Exp.EqColumns(MailboxTable.Columns.SmtpServerId.Prefix(MAILBOX_ALIAS), MailboxServerTable.Columns.Id.Prefix(OUT_SERVER_ALIAS)))
                .InnerJoin(MailboxServerTable.Name.Alias(IN_SERVER_ALIAS),
                            Exp.EqColumns(MailboxTable.Columns.ServerId.Prefix(MAILBOX_ALIAS), MailboxServerTable.Columns.Id.Prefix(IN_SERVER_ALIAS)))
                .Select(fieldsForSelect);
        }

        private MailBox ToMailBox(object[] r)
        {
            if (r.Length != SELECT_MAIL_BOX_FIELDS_COUNT && r.Length != SELECT_MAIL_BOX_FIELDS_COUNT_WITH_AUTOREPLY)
            {
                _log.Error("ToMailBoxCount of returned fields Length = {0} not equal to SELECT_MAIL_BOX_FIELDS_COUNT = {1} and SELECT_MAIL_BOX_FIELDS_COUNT_WITH_AUTOREPLY {2}",
                    r.Length, SELECT_MAIL_BOX_FIELDS_COUNT, SELECT_MAIL_BOX_FIELDS_COUNT_WITH_AUTOREPLY);
                return null;
            }

            var mailboxId = Convert.ToInt32(r[(int) MailBoxFieldSelectPosition.Id]);
            var tenant = Convert.ToInt32(r[(int) MailBoxFieldSelectPosition.IdTenant]);
            var user = (string) r[(int) MailBoxFieldSelectPosition.IdUser];
            var name = (string) r[(int) MailBoxFieldSelectPosition.Name];

            var address = new MailAddress((string)r[(int)MailBoxFieldSelectPosition.Address]);

            var account = FormatLoginFromDb((string)r[(int)MailBoxFieldSelectPosition.Account], address);
            var serverOldFormat = string.Format("{0}:{1}", r[(int)MailBoxFieldSelectPosition.InServer], r[(int)MailBoxFieldSelectPosition.InPort]);
            var encryption = ((string)r[(int)MailBoxFieldSelectPosition.IncomingEncryptionType]).ToEncryptionType();
            var auth = ((string)r[(int)MailBoxFieldSelectPosition.AuthTypeIn]).ToSaslMechanism();
            var imap = Convert.ToBoolean(r[(int) MailBoxFieldSelectPosition.Imap]);

            var smtpAccount = FormatLoginFromDb((string)r[(int)MailBoxFieldSelectPosition.SmtpAccount], address);
            var smtpServerOldFormat = string.Format("{0}:{1}", (string)r[(int)MailBoxFieldSelectPosition.SmtpServer], r[(int)MailBoxFieldSelectPosition.SmtpPort]);
            var smtpEncryption = ((string)r[(int)MailBoxFieldSelectPosition.OutcomingEncryptionType]).ToEncryptionType();
            var smtpAuth = ((string)r[(int)MailBoxFieldSelectPosition.AuthtTypeSmtp]).ToSaslMechanism();

            string password, smtpPassword = null, oAuthToken = null;

            TryDecryptPassword((string) r[(int) MailBoxFieldSelectPosition.Password], out password);

            if (r[(int) MailBoxFieldSelectPosition.SmtpPassword] != null)
            {
                TryDecryptPassword((string) r[(int) MailBoxFieldSelectPosition.SmtpPassword], out smtpPassword);
            }

            var accessTokenRefreshed = false;
            var oAuthType = Convert.ToByte(r[(int) MailBoxFieldSelectPosition.OAuthType]);

            if (r[(int)MailBoxFieldSelectPosition.OAuthToken] != null)
            {
                if(!TryDecryptPassword((string)r[(int)MailBoxFieldSelectPosition.OAuthToken], out oAuthToken))
                {
                    // Fix old refresh_token
                    try
                    {
                        if ((AuthorizationServiceType) Convert.ToByte(r[(int) MailBoxFieldSelectPosition.OAuthType]) ==
                            AuthorizationServiceType.Google)
                        {
                            var googleAuth = new GoogleOAuth2Authorization(new NullLogger());
                            oAuthToken = googleAuth.RequestAccessToken((string)r[(int)MailBoxFieldSelectPosition.OAuthToken]).ToJson();
                            accessTokenRefreshed = true;
                        }
                    }
                    catch (Exception)
                    {
                        // skip
                    }
                }
            }

            var beginDate = (DateTime) r[(int) MailBoxFieldSelectPosition.BeginDate];
            var emailInFolder = (string) r[(int) MailBoxFieldSelectPosition.EMailInFolder];
            var isEnabled = Convert.ToBoolean(r[(int) MailBoxFieldSelectPosition.Enabled]);
            var isRemoved = Convert.ToBoolean(r[(int) MailBoxFieldSelectPosition.IsRemoved]);
            var isTeamlabMailbox = Convert.ToBoolean(r[(int) MailBoxFieldSelectPosition.IsTeamlabMailbox]);
            var quotaError = Convert.ToBoolean(r[(int) MailBoxFieldSelectPosition.QuotaError]);
            var authErrorDate = r[(int) MailBoxFieldSelectPosition.AuthError] != null
                ? Convert.ToDateTime(r[(int) MailBoxFieldSelectPosition.AuthError])
                : (DateTime?) null;

            var mailboxSize = Convert.ToInt64(r[(int) MailBoxFieldSelectPosition.SizeLast]);
            var mailboxMessages = Convert.ToInt32(r[(int) MailBoxFieldSelectPosition.MsgCountLast]);
            var loginDelay = Convert.ToInt32(r[(int) MailBoxFieldSelectPosition.LoginDelay]);
            var imapIntervalsJson = (string) r[(int) MailBoxFieldSelectPosition.ImapIntervals];

            var serverSettingsId = (int) r[(int) MailBoxFieldSelectPosition.IdInServer];
            var smtpServerSettingsId = (int) r[(int) MailBoxFieldSelectPosition.IdSmtpServer];

            var mailAutoReply = r.Length == SELECT_MAIL_BOX_FIELDS_COUNT_WITH_AUTOREPLY
                ? new MailAutoreply(Convert.ToInt32(r[(int) MailBoxFieldSelectPosition.Id]),
                    Convert.ToInt32(r[(int) MailBoxFieldSelectPosition.IdTenant]),
                    Convert.ToBoolean(r[(int) MailBoxFieldSelectPosition.AutoreplyTurnOn]),
                    Convert.ToBoolean(r[(int) MailBoxFieldSelectPosition.AutoreplyOnlyContacts]),
                    Convert.ToBoolean(r[(int) MailBoxFieldSelectPosition.AutoreplyTurnOnToDate]),
                    Convert.ToDateTime(r[(int) MailBoxFieldSelectPosition.AutoreplyFromDate]),
                    Convert.ToDateTime(r[(int) MailBoxFieldSelectPosition.AutoreplyToDate]),
                    Convert.ToString(r[(int) MailBoxFieldSelectPosition.AutoreplySubject]),
                    Convert.ToString(r[(int) MailBoxFieldSelectPosition.AutoreplyHtml]))
                : null;

            var res = new MailBox(
                tenant,
                user,
                mailboxId,

                name,
                address,

                account,
                password,
                serverOldFormat,
                encryption,
                auth,
                imap,

                smtpAccount,
                smtpPassword,
                smtpServerOldFormat,
                smtpEncryption,
                smtpAuth,

                oAuthType,
                oAuthToken)
            {
                Size = mailboxSize,
                MessagesCount = mailboxMessages,
                ServerLoginDelay = loginDelay,
                BeginDate = beginDate,
                QuotaError = quotaError,
                AuthErrorDate = authErrorDate,
                ImapIntervalsJson = imapIntervalsJson,
                SmtpServerId = smtpServerSettingsId,
                InServerId = serverSettingsId,
                EMailInFolder = emailInFolder,
                MailAutoreply = mailAutoReply,
                AccessTokenRefreshed = accessTokenRefreshed,

                Enabled = isEnabled,
                IsRemoved = isRemoved,
                IsTeamlab = isTeamlabMailbox,
            };

            return res;
        }

        private List<AccountInfo> ToAccountInfo(IEnumerable<object[]> objectList, List<MailSignature> signatures, List<MailAutoreply> autoreplies)
        {
            var accounts = new List<AccountInfo>();

            foreach (var r in objectList)
            {
                if (r.Length != SELECT_ACCOUNT_FIELDS_COUNT)
                {
                    _log.Error("ToAccountInfo Count of returned fields Length = {0} not equal to SELECT_ACCOUNT_FIELDS_COUNT = {1}",
                        r.Length, SELECT_ACCOUNT_FIELDS_COUNT);
                    return null;
                }

                var mailboxId = Convert.ToInt32(r[(int)MailAccountFieldSelectPosition.Id]);
                var accountIndex = accounts.FindIndex(a => a.Id == mailboxId);

                var signature = signatures.First(s => s.MailboxId == mailboxId);
                var autoreply = autoreplies.First(s => s.MailboxId == mailboxId);
                var isAlias = Convert.ToBoolean(r[(int)MailAccountFieldSelectPosition.IsAlias]);

                if (!isAlias)
                {
                    var groupAddress = (string)(r[(int)MailAccountFieldSelectPosition.GroupAddress]);
                    MailAddressInfo group = null;

                    if (!string.IsNullOrEmpty(groupAddress))
                    {
                        group = new MailAddressInfo(
                            Convert.ToInt32(r[(int)MailAccountFieldSelectPosition.GroupId]),
                            groupAddress,
                            Convert.ToInt32(r[(int)MailAccountFieldSelectPosition.DomainId]));
                    }

                    if (accountIndex == -1)
                    {
                        var authErrorType = MailBox.AuthProblemType.NoProblems;

                        if (r[(int)MailAccountFieldSelectPosition.AuthError] != null)
                        {
                            var authErrorDate = Convert.ToDateTime(r[(int)MailAccountFieldSelectPosition.AuthError]);

                            if (DateTime.UtcNow - authErrorDate > AuthErrorDisableTimeout)
                                authErrorType = MailBox.AuthProblemType.TooManyErrors;
                            else if (DateTime.UtcNow - authErrorDate > AuthErrorWarningTimeout)
                                authErrorType = MailBox.AuthProblemType.ConnectError;
                        }

                        var account = new AccountInfo(
                            mailboxId,
                            (string)(r[(int)MailAccountFieldSelectPosition.Address]),
                            (string)(r[(int)MailAccountFieldSelectPosition.Name]),
                            Convert.ToBoolean(r[(int)MailAccountFieldSelectPosition.Enabled]),
                            Convert.ToBoolean(r[(int)MailAccountFieldSelectPosition.QoutaError]),
                            authErrorType, signature, autoreply,
                            !string.IsNullOrEmpty((string)(r[(int)MailAccountFieldSelectPosition.RefreshToken])),
                            (string)(r[(int)MailAccountFieldSelectPosition.EmailInFolder]),
                            Convert.ToBoolean(r[(int)MailAccountFieldSelectPosition.IsTeamlabMailbox]),
                            Convert.ToInt32(r[(int) MailAccountFieldSelectPosition.DomainTenant]) == Defines.SHARED_TENANT_ID);

                        if (group != null) account.Groups.Add(group);

                        accounts.Add(account);
                    }
                    else if (group != null)
                    {
                        accounts[accountIndex].Groups.Add(group);
                    }
                }
                else
                {
                    var alias = new MailAddressInfo(
                            Convert.ToInt32(r[(int)MailAccountFieldSelectPosition.AliasId]),
                            string.Format("{0}@{1}", r[(int)MailAccountFieldSelectPosition.AliasName],
                            r[(int)MailAccountFieldSelectPosition.DomainName]),
                            Convert.ToInt32(r[(int)MailAccountFieldSelectPosition.DomainId]));

                    accounts[accountIndex].Aliases.Add(alias);
                }
            }
            return accounts;
        }

        private List<MailAddressInfo> GetAliases(int mailboxId)
        {
            const string server_address = "sa";
            const string server_domain = "sd";

            var queryForExecution = new SqlQuery(AddressTable.Name.Alias(server_address))
                .Select(AddressTable.Columns.Id.Prefix(server_address))
                .Select(AddressTable.Columns.AddressName.Prefix(server_address))
                .Select(DomainTable.Columns.DomainName.Prefix(server_domain))
                .Select(DomainTable.Columns.Id.Prefix(server_domain))
                .InnerJoin(DomainTable.Name.Alias(server_domain),
                    Exp.EqColumns(AddressTable.Columns.DomainId.Prefix(server_address),
                        DomainTable.Columns.Id.Prefix(server_domain)))
                .Where(AddressTable.Columns.MailboxId.Prefix(server_address), mailboxId)
                .Where(AddressTable.Columns.IsAlias.Prefix(server_address), true);
            using (var db = GetDb())
            {
                var res = db.ExecuteList(queryForExecution);
                if (res == null)
                {
                    return new List<MailAddressInfo>();
                }
                return res.ConvertAll(r => new MailAddressInfo(Convert.ToInt32(r[0]),
                      string.Format("{0}@{1}", r[1], r[2]), Convert.ToInt32(r[3])));
            }
        }
        
        private List<MailAddressInfo> GetGroups(int mailboxId)
        {
            const string groups = "sg";
            const string group_x_address = "ga";
            const string server_address = "sa";
            const string server_domain = "sd";

            var queryForExecution = new SqlQuery(AddressTable.Name.Alias(server_address))
                .Select(MailGroupTable.Columns.Id.Prefix(groups))
                .Select(MailGroupTable.Columns.Address.Prefix(groups))
                .Select(DomainTable.Columns.Id.Prefix(server_domain))
                .InnerJoin(DomainTable.Name.Alias(server_domain),
                           Exp.EqColumns(AddressTable.Columns.DomainId.Prefix(server_address),
                                         DomainTable.Columns.Id.Prefix(server_domain)))
                .InnerJoin(MailGroupXAddressesTable.Name.Alias(group_x_address),
                           Exp.EqColumns(AddressTable.Columns.Id.Prefix(server_address),
                                         MailGroupXAddressesTable.Columns.AddressId.Prefix(group_x_address)))
                .InnerJoin(MailGroupTable.Name.Alias(groups),
                           Exp.EqColumns(MailGroupXAddressesTable.Columns.MailGroupId.Prefix(group_x_address),
                                         MailGroupTable.Columns.Id.Prefix(groups)))
                .Where(AddressTable.Columns.MailboxId.Prefix(server_address), mailboxId);

            using (var db = GetDb())
            {
                var res = db.ExecuteList(queryForExecution);
                if (res == null)
                {
                    return new List<MailAddressInfo>();
                }
                return res.ConvertAll(r =>
                    new MailAddressInfo(Convert.ToInt32(r[0]), Convert.ToString(r[1]), Convert.ToInt32(r[2])));
            }
        }

        private MailAttachment ToMailItemAttachment(object[] r)
        {
            if (r.Length != SELECT_MAIL_ITEM_ATTACHMENT_FIELDS_COUNT)
            {
                _log.Error("ToMailItemAttachment Count of returned fields Length = {0} not equal to SELECT_MAIL_ITEM_ATTACHMENT_FIELDS_COUNT = {1}",
                    r.Length, SELECT_MAIL_ITEM_ATTACHMENT_FIELDS_COUNT);
                return null;
            }

            var attachment = new MailAttachment
                {
                    fileId = Convert.ToInt32(r[(int)MailItemAttachmentSelectPosition.Id]),
                    fileName = Convert.ToString(r[(int)MailItemAttachmentSelectPosition.Name]),
                    storedName = Convert.ToString(r[(int)MailItemAttachmentSelectPosition.StoredName]),
                    contentType = Convert.ToString(r[(int)MailItemAttachmentSelectPosition.Type]),
                    size = Convert.ToInt64(r[(int)MailItemAttachmentSelectPosition.Size]),
                    fileNumber = Convert.ToInt32(r[(int)MailItemAttachmentSelectPosition.FileNumber]),
                    streamId = Convert.ToString(r[(int)MailItemAttachmentSelectPosition.IdStream]),
                    tenant = Convert.ToInt32(r[(int)MailItemAttachmentSelectPosition.Tenant]),
                    user = Convert.ToString(r[(int)MailItemAttachmentSelectPosition.User]),
                    contentId = r[(int)MailItemAttachmentSelectPosition.ContentId] != null ? 
                        Convert.ToString(r[(int)MailItemAttachmentSelectPosition.ContentId]) : null,
                    mailboxId = Convert.ToInt32(r[(int)MailItemAttachmentSelectPosition.MailboxId]),
                };

            // if StoredName is empty then attachment had been stored by filename (old attachment);
            attachment.storedName = string.IsNullOrEmpty(attachment.storedName) ? attachment.fileName : attachment.storedName;

            return attachment;
        }

        public void GetMailBoxState(int mailboxId, out bool isRemoved, out bool isDeactivated, out DateTime beginDate)
        {
            isRemoved = true;
            isDeactivated = true;
            beginDate = MinBeginDate;

            using (var db = GetDb())
            {
                var res = db.ExecuteList(new SqlQuery(MailboxTable.Name)
                        .Select(MailboxTable.Columns.IsRemoved, MailboxTable.Columns.Enabled, MailboxTable.Columns.BeginDate)
                        .Where(Exp.Eq(MailboxTable.Columns.Id, mailboxId)))
                        .FirstOrDefault();

                if (res == null) return;
                isRemoved = Convert.ToBoolean(res[0]);
                isDeactivated = !Convert.ToBoolean(res[1]);
                beginDate = Convert.ToDateTime(res[2]);
            }
        }

        // Return id_mailbox > 0 if address exists in mail_mailbox table.
        private int MailBoxExists(string address, string user, int tenant)
        {
            using (var db = GetDb())
            {
                return db.ExecuteScalar<int>(
                    new SqlQuery(MailboxTable.Name)
                        .Select(MailboxTable.Columns.Id)
                        .Where(GetUserWhere(user, tenant))
                        .Where(MailboxTable.Columns.Address, address)
                        .Where(MailboxTable.Columns.IsRemoved, false));
            }
        }

        private static string FormatLoginFromDb(string format, MailAddress address)
        {
            return format.Replace("%EMAILADDRESS%", address.Address)
                         .Replace("%EMAILLOCALPART%", address.User)
                         .Replace("%EMAILDOMAIN%", address.Host.ToLowerInvariant())
                         .Replace("%EMAILHOSTNAME%", Path.GetFileNameWithoutExtension(address.Host.ToLowerInvariant()));
        }

        // Documentation in unit tests
        public static string GetLoginFormatFrom(MailAddress address, string username)
        {
            var addressLower = address.Address.ToLower();
            var usernameLower = username.ToLower();
            var mailparts = addressLower.Split('@');

            var localpart = mailparts[0];
            var domain = mailparts[1];
            var hostNameVariant1 = Path.GetFileNameWithoutExtension(domain);
            var hostNameVariant2 = domain.Split('.')[0];

            var resultFormat = usernameLower.Replace(addressLower, "%EMAILADDRESS%");
            var pos = resultFormat.IndexOf(localpart, StringComparison.InvariantCulture);
            if (pos >= 0)
            {
                resultFormat = resultFormat.Substring(0, pos) + "%EMAILLOCALPART%" + resultFormat.Substring(pos + localpart.Length);
            }
            resultFormat = resultFormat.Replace(domain, "%EMAILDOMAIN%");
            if (hostNameVariant1 != null)
                resultFormat = resultFormat.Replace(hostNameVariant1, "%EMAILHOSTNAME%");
            resultFormat = resultFormat.Replace(hostNameVariant2, "%EMAILHOSTNAME%");

            return resultFormat == usernameLower ? "" : resultFormat;
        }

        private static string FormatServerFromDb(string format, string host)
        {
            return format.Replace("%EMAILDOMAIN%", host);
        }

        private const int HARDCODED_LOGIN_TIME_FOR_MS_MAIL = 900;
        private static int GetLoginDelayTime(MailBox mailbox)
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
                return HARDCODED_LOGIN_TIME_FOR_MS_MAIL;

            return mailbox.ServerLoginDelay < MailBox.DefaultServerLoginDelay
                       ? MailBox.DefaultServerLoginDelay
                       : mailbox.ServerLoginDelay;
        }

        private static void CheckMailboxOwnage(int mailboxId, string user, int tenant, DbManager db)
        {
            CheckMailboxesOwnage(new List<int> { mailboxId }, user, tenant, db);
        }

        private static void CheckMailboxesOwnage(List<int> mailboxIds, string user, int tenant, IDbManager db)
        {
            var checkMailboxOwnage = new SqlQuery(MailboxTable.Name)
                .Select(MailboxTable.Columns.Id)
                .Where(MailboxTable.Columns.User, user)
                .Where(MailboxTable.Columns.Tenant, tenant)
                .Where(mailboxIds.Count > 1
                           ? Exp.In(MailboxTable.Columns.Id, mailboxIds)
                           : Exp.Eq(MailboxTable.Columns.Id, mailboxIds[0]));

            var foundIds = db.ExecuteList(checkMailboxOwnage)
                .ConvertAll(res => Convert.ToInt32(res[0]));

            if (!mailboxIds.Any(idMailbox => foundIds.Exists(foundId => foundId == idMailbox)))
                throw new AccessViolationException("Mailbox doesn't owned by user.");
        }

        private ClientConfig SearchBusinessVendorsSettings(string domain)
        {
            ClientConfig settingsFromDb = null;

            try
            {
                var dnsLookup = new DnsLookup();

                var mxRecords = dnsLookup.GetDomainMxRecords(domain);

                if (!mxRecords.Any())
                {
                    return null;
                }

                var knownBusinessMxs =
                    MxToDomainBusinessVendorsList.Where(
                        mx =>
                            mxRecords.FirstOrDefault(
                                r => r.ExchangeDomainName.ToString().ToLowerInvariant().Contains(mx.Key.ToLowerInvariant())) != null)
                        .ToList();

                foreach (var mxXdomain in knownBusinessMxs)
                {
                    settingsFromDb = GetStoredMailBoxSettings(mxXdomain.Value);

                    if (settingsFromDb != null)
                        return settingsFromDb;
                }
            }
            catch (Exception ex)
            {
                _log.Error(string.Format("Failed SearchBusinessVendorsSettings(\"{0}\")", domain), ex);
            }

            return settingsFromDb;
        }

        #endregion
    }
}