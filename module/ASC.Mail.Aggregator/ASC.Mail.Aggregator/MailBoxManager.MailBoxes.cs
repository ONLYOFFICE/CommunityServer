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
using System.Linq;
using System.Threading;
using ASC.Api.Exceptions;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using System.Net.Mail;
using System.IO;
using ASC.Mail.Aggregator.Common;
using ASC.Mail.Aggregator.Common.Authorization;
using ASC.Mail.Aggregator.Common.Imap;
using ASC.Mail.Aggregator.Dal;
using ASC.Mail.Aggregator.Dal.DbSchema;
using ASC.Mail.Aggregator.Common.Extension;
using ActiveUp.Net.Mail;

namespace ASC.Mail.Aggregator
{
    public partial class MailBoxManager
    {
        #region db defines

        public static readonly DateTime MinBeginDate = new DateTime(1975, 1, 1, 0, 0, 0);

        #endregion

        # region public methods

        public bool SaveMailBox(MailBox mailbox)
        {
            if (mailbox == null) throw new ArgumentNullException("mailbox");

            var idMailbox = MailBoxExists(GetAddress(mailbox.EMail), mailbox.UserId, mailbox.TenantId);

            using (var db = GetDb())
            {
                int result;

                var loginDelayTime = GetLoginDelayTime(mailbox);

                if (idMailbox == 0)
                {
                    var utcNow = DateTime.UtcNow;
                    
                    result = db.ExecuteScalar<int>(
                        new SqlInsert(MailboxTable.name)
                            .InColumnValue(MailboxTable.Columns.id, 0)
                            .InColumnValue(MailboxTable.Columns.id_tenant, mailbox.TenantId)
                            .InColumnValue(MailboxTable.Columns.id_user, mailbox.UserId)
                            .InColumnValue(MailboxTable.Columns.address, GetAddress(mailbox.EMail))
                            .InColumnValue(MailboxTable.Columns.name, mailbox.Name)
                            .InColumnValue(MailboxTable.Columns.password, EncryptPassword(mailbox.Password))
                            .InColumnValue(MailboxTable.Columns.msg_count_last, mailbox.MessagesCount)
                            .InColumnValue(MailboxTable.Columns.smtp_password,
                                           string.IsNullOrEmpty(mailbox.SmtpPassword)
                                               ? EncryptPassword(mailbox.Password)
                                               : EncryptPassword(mailbox.SmtpPassword))
                            .InColumnValue(MailboxTable.Columns.size_last, mailbox.Size)
                            .InColumnValue(MailboxTable.Columns.login_delay, loginDelayTime)
                            .InColumnValue(MailboxTable.Columns.enabled, true)
                            .InColumnValue(MailboxTable.Columns.imap, mailbox.Imap)
                            .InColumnValue(MailboxTable.Columns.begin_date, mailbox.BeginDate)
                            .InColumnValue(MailboxTable.Columns.service_type, mailbox.ServiceType)
                            .InColumnValue(MailboxTable.Columns.refresh_token, mailbox.RefreshToken)
                            .InColumnValue(MailboxTable.Columns.id_smtp_server, mailbox.SmtpServerId)
                            .InColumnValue(MailboxTable.Columns.id_in_server, mailbox.InServerId)
                            .InColumnValue(MailboxTable.Columns.date_created, utcNow)
                            .Identity(0, 0, true));

                    mailbox.MailBoxId = result;
                    mailbox.Enabled = true;
                }
                else
                {
                    mailbox.MailBoxId = idMailbox;

                    var queryUpdate = new SqlUpdate(MailboxTable.name)
                        .Where(MailboxTable.Columns.id, idMailbox)
                        .Set(MailboxTable.Columns.id_tenant, mailbox.TenantId)
                        .Set(MailboxTable.Columns.id_user, mailbox.UserId)
                        .Set(MailboxTable.Columns.address, GetAddress(mailbox.EMail))
                        .Set(MailboxTable.Columns.name, mailbox.Name)
                        .Set(MailboxTable.Columns.password, EncryptPassword(mailbox.Password))
                        .Set(MailboxTable.Columns.msg_count_last, mailbox.MessagesCount)
                        .Set(MailboxTable.Columns.smtp_password,
                             string.IsNullOrEmpty(mailbox.SmtpPassword)
                                 ? EncryptPassword(mailbox.Password)
                                 : EncryptPassword(mailbox.SmtpPassword))
                        .Set(MailboxTable.Columns.size_last, mailbox.Size)
                        .Set(MailboxTable.Columns.login_delay, loginDelayTime)
                        .Set(MailboxTable.Columns.is_removed, false)
                        .Set(MailboxTable.Columns.imap, mailbox.Imap)
                        .Set(MailboxTable.Columns.begin_date, mailbox.BeginDate)
                        .Set(MailboxTable.Columns.service_type, mailbox.ServiceType)
                        .Set(MailboxTable.Columns.refresh_token, mailbox.RefreshToken)
                        .Set(MailboxTable.Columns.id_smtp_server, mailbox.SmtpServerId)
                        .Set(MailboxTable.Columns.id_in_server, mailbox.InServerId);

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
                                    imapIntervals.AddUnhandledInterval(new UidInterval(1,
                                                                                       currentMailbox.ImapIntervals[
                                                                                           folderName].BeginDateUid));
                                
                                currentMailbox.ImapIntervals[folderName].UnhandledUidIntervals =
                                    new List<int>(imapIntervals.ToIndexes());

                                currentMailbox.ImapIntervals[folderName].BeginDateUid = 1;
                            }

                            queryUpdate.Set(MailboxTable.Columns.imap_intervals, currentMailbox.ImapIntervalsJson);
                        }

                    }

                    result = db.ExecuteNonQuery(queryUpdate);
                }

                return result > 0;
            }
        }

        public List<AccountInfo> GetAccountInfo(int tenant, string user)
        {
            const string mailbox_alias = "ma";
            const string server_address = "sa";
            const string server_domain = "sd";
            const string group_x_address = "ga";
            const string server_group = "sg";

            var query = new SqlQuery(MailboxTable.name.Alias(mailbox_alias))
                .LeftOuterJoin(AddressTable.name.Alias(server_address),
                               Exp.EqColumns(MailboxTable.Columns.id.Prefix(mailbox_alias),
                                             AddressTable.Columns.id_mailbox.Prefix(server_address)))
                .LeftOuterJoin(DomainTable.name.Alias(server_domain),
                               Exp.EqColumns(AddressTable.Columns.id_domain.Prefix(server_address),
                                             DomainTable.Columns.id.Prefix(server_domain)))
                .LeftOuterJoin(MailGroupXAddressesTable.name.Alias(group_x_address),
                               Exp.EqColumns(AddressTable.Columns.id.Prefix(server_address),
                                             MailGroupXAddressesTable.Columns.id_address.Prefix(group_x_address)))
                .LeftOuterJoin(MailGroupTable.name.Alias(server_group),
                               Exp.EqColumns(MailGroupXAddressesTable.Columns.id_mail_group.Prefix(group_x_address),
                                             MailGroupTable.Columns.id.Prefix(server_group)))
                .Select(MailboxTable.Columns.id.Prefix(mailbox_alias))
                .Select(MailboxTable.Columns.address.Prefix(mailbox_alias))
                .Select(MailboxTable.Columns.enabled.Prefix(mailbox_alias))
                .Select(MailboxTable.Columns.name.Prefix(mailbox_alias))
                .Select(MailboxTable.Columns.quota_error.Prefix(mailbox_alias))
                .Select(MailboxTable.Columns.date_auth_error.Prefix(mailbox_alias))
                .Select(MailboxTable.Columns.refresh_token.Prefix(mailbox_alias))
                .Select(MailboxTable.Columns.is_teamlab_mailbox.Prefix(mailbox_alias))
                .Select(MailboxTable.Columns.email_in_folder.Prefix(mailbox_alias))
                .Select(AddressTable.Columns.id.Prefix(server_address))
                .Select(AddressTable.Columns.name.Prefix(server_address))
                .Select(AddressTable.Columns.is_alias.Prefix(server_address))
                .Select(DomainTable.Columns.id.Prefix(server_domain))
                .Select(DomainTable.Columns.name.Prefix(server_domain))
                .Select(MailGroupTable.Columns.id.Prefix(server_group))
                .Select(MailGroupTable.Columns.address.Prefix(server_group))
                .Select(DomainTable.Columns.tenant.Prefix(server_domain))
                .Where(MailboxTable.Columns.is_removed.Prefix(mailbox_alias), false)
                .Where(MailboxTable.Columns.id_tenant.Prefix(mailbox_alias), tenant)
                .Where(MailboxTable.Columns.id_user.Prefix(mailbox_alias), user.ToLowerInvariant())
                .OrderBy(AddressTable.Columns.is_alias.Prefix(server_address), true);

            List<object[]> result;
            List<SignatureDto> signatures;
            using (var db = GetDb())
            {
                result = db.ExecuteList(query);
                var mailboxIds = result.ConvertAll(r =>
                    Convert.ToInt32(r[(int)MailAccountFieldSelectPosition.Id])).Distinct().ToList();
                signatures = GetMailboxesSignatures(mailboxIds, tenant, db);
            }

            return ToAccountInfo(result, signatures);
        }


        public List<MailBox> GetMailBoxes(int tenant, string user, bool unremoved = true)
        {
            var where = Exp.Eq(MailboxTable.Columns.id_tenant, tenant) &
                        Exp.Eq(MailboxTable.Columns.id_user, user.ToLowerInvariant());

            if (unremoved)
                where &= Exp.Eq(MailboxTable.Columns.is_removed.Prefix(MAILBOX_ALIAS), false);

            return GetMailBoxes(where);
        }

        // returns whether user has not removed accounts
        public bool HasMailboxes(int tenant, string user)
        {
            using (var db = GetDb())
            {
                var count = db.ExecuteScalar<int>(new SqlQuery(MailboxTable.name)
                                                      .SelectCount()
                                                      .Where(MailboxTable.Columns.id_tenant, tenant)
                                                      .Where(MailboxTable.Columns.id_user, user.ToLowerInvariant())
                                                      .Where(MailboxTable.Columns.is_removed, 0));
                return count > 0;
            }
        }

        public MailBox GetMailBox(int tenant, string user, MailAddress email)
        {
            return GetMailBoxes(Exp.Eq(MailboxTable.Columns.address.Prefix(MAILBOX_ALIAS), GetAddress(email)) &
                Exp.Eq(MailboxTable.Columns.id_tenant.Prefix(MAILBOX_ALIAS), tenant) &
                Exp.Eq(MailboxTable.Columns.id_user.Prefix(MAILBOX_ALIAS), user.ToLowerInvariant()) &
                Exp.Eq(MailboxTable.Columns.is_removed.Prefix(MAILBOX_ALIAS), false), false)
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
            return GetMailBoxes(Exp.Eq(MailboxTable.Columns.id.Prefix(MAILBOX_ALIAS), mailboxId) &
                                Exp.Eq(MailboxTable.Columns.is_removed.Prefix(MAILBOX_ALIAS), false))
                .SingleOrDefault();
        }

        public MailBox GetMailBox(int mailboxId)
        {
            return GetMailBoxes(Exp.Eq(MailboxTable.Columns.id.Prefix(MAILBOX_ALIAS), mailboxId), false)
                .SingleOrDefault();
        }

        public MailBox GetNextMailBox(int mailboxId)
        {
            using (var db = GetDb())
            {
                var query = GetSelectMailBoxFieldsQuery()
                .Where(Exp.Gt(MailboxTable.Columns.id.Prefix(MAILBOX_ALIAS), mailboxId))
                .OrderBy(MailboxTable.Columns.id.Prefix(MAILBOX_ALIAS), true)
                .SetMaxResults(1);

                var mailbox = db.ExecuteList(query)
                            .ConvertAll(ToMailBox)
                            .SingleOrDefault();

                return mailbox;
            }

        }

        public void GetMailboxesRange(out int minMailboxId, out int maxMailboxId)
        {
            minMailboxId = 0;
            maxMailboxId = 0;

            using (var db = GetDb())
            {
                var selectQuery = new SqlQuery(MailboxTable.name)
                    .SelectMin(MailboxTable.Columns.id)
                    .SelectMax(MailboxTable.Columns.id);

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

        public MailMessageItem GetNextMailBoxNessage(MailBox mailbox, int messageId)
        {
            using (var db = GetDb())
            {
                var query = new SqlQuery(MailTable.name)
                .Select(MailTable.Columns.id)
                .Where(GetUserWhere(mailbox.UserId, mailbox.TenantId))
                    .Where(MailTable.Columns.id_mailbox, mailbox.MailBoxId)
                .Where(Exp.Gt(MailTable.Columns.id, messageId))
                .OrderBy(MailTable.Columns.id, true)
                .SetMaxResults(1);

                var nextMessageId = db.ExecuteScalar<int>(query);

                var message = GetMailInfo(mailbox.TenantId, mailbox.UserId, nextMessageId, false, false, false);

                return message;
            }

        }

        public void GetMailboxMessagesRange(MailBox mailbox, out int minMessageId, out int maxMessageId)
        {
            minMessageId = 0;
            maxMessageId = 0;

            using (var db = GetDb())
            {
                var selectQuery = new SqlQuery(MailTable.name)
                    .SelectMin(MailTable.Columns.id)
                    .SelectMax(MailTable.Columns.id)
                    .Where(GetUserWhere(mailbox.UserId, mailbox.TenantId))
                    .Where(MailTable.Columns.id_mailbox, mailbox.MailBoxId);

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
            var updateQuery = new SqlUpdate(MailboxTable.name)
                .Where(MailboxTable.Columns.id, mailbox.MailBoxId)
                .Where(GetUserWhere(mailbox.UserId, mailbox.TenantId))
                .Where(MailboxTable.Columns.is_removed, false)
                .Set(MailboxTable.Columns.enabled, enabled);

            if (enabled)
                updateQuery.Set(MailboxTable.Columns.date_auth_error, null);

            var result = db.ExecuteNonQuery(updateQuery);

            return result > 0;
        }

        public void RemoveMailBox(MailBox mailbox)
        {
            if (mailbox.MailBoxId <= 0)
                throw new Exception("MailBox id is 0");

            long totalAttachmentsSize;

            using (var db = GetDb())
            {
                using (var tx = db.BeginTransaction())
                {
                    totalAttachmentsSize = RemoveMailBox(mailbox, db);

                    tx.Commit();
                }
            }

            QuotaUsedDelete(mailbox.TenantId, totalAttachmentsSize);
        }

        public long RemoveMailBox(MailBox mailBox, DbManager db)
        {
            if (mailBox.MailBoxId <= 0)
                throw new Exception("MailBox id is 0");

            db.ExecuteNonQuery(
                new SqlUpdate(MailboxTable.name)
                    .Set(MailboxTable.Columns.is_removed, true)
                    .Where(MailboxTable.Columns.id, mailBox.MailBoxId));

            db.ExecuteNonQuery(
                new SqlDelete(ChainTable.name)
                    .Where(GetUserWhere(mailBox.UserId, mailBox.TenantId))
                    .Where(ChainTable.Columns.id_mailbox, mailBox.MailBoxId));


            db.ExecuteNonQuery(
                new SqlUpdate(MailTable.name)
                    .Set(MailTable.Columns.is_removed, true)
                    .Where(MailTable.Columns.id_mailbox, mailBox.MailBoxId)
                    .Where(GetUserWhere(mailBox.UserId, mailBox.TenantId)));

            var totalAttachmentsSize = db.ExecuteScalar<long>(
                string.Format(
                    "select sum(a.size) from {0} a inner join {1} m on a.{2} = m.{3} where m.{4} = @mailbox_id and m.{5} = @tid and a.{5} = @tid and a.{6} != @need_remove",
                    AttachmentTable.name,
                    MailTable.name,
                    AttachmentTable.Columns.id_mail,
                    MailTable.Columns.id,
                    MailTable.Columns.id_mailbox,
                    MailTable.Columns.id_tenant,
                    AttachmentTable.Columns.need_remove), new { tid = mailBox.TenantId, need_remove = true, mailbox_id = mailBox.MailBoxId });

            var query = string.Format("update {0} a inner join {1} m on a.{2} = m.{3} set a.{4} = @need_remove where m.{5} = @mailbox_id",
                    AttachmentTable.name, MailTable.name, AttachmentTable.Columns.id_mail, MailTable.Columns.id, AttachmentTable.Columns.need_remove, MailTable.Columns.id_mailbox);

            db.ExecuteNonQuery(query, new { need_remove = true, mailbox_id = mailBox.MailBoxId });


            query = string.Format("select t.{0} from {1} t inner join {2} m on t.{3} = m.{4} where m.{5} = @mailbox_id",
                TagMailTable.Columns.id_tag, TagMailTable.name, MailTable.name, TagMailTable.Columns.id_mail, MailTable.Columns.id, MailTable.Columns.id_mailbox);

            var affectedTags = db.ExecuteList(query, new { mailbox_id = mailBox.MailBoxId })
                .ConvertAll(r => Convert.ToInt32(r[0]))
                .Distinct();

            query = string.Format("delete t from {0} t inner join {1} m on t.{2} = m.{3} where m.{4} = @mailbox_id",
                                  TagMailTable.name, MailTable.name, TagMailTable.Columns.id_mail, MailTable.Columns.id, MailTable.Columns.id_mailbox);

            db.ExecuteNonQuery(query, new { mailbox_id = mailBox.MailBoxId });

            UpdateTagsCount(db, mailBox.TenantId, mailBox.UserId, affectedTags);

            RecalculateFolders(db, mailBox.TenantId, mailBox.UserId);

            var signatureManager = new SignatureDal(db);

            signatureManager.DeleteSignature(mailBox.MailBoxId, mailBox.TenantId);

            return totalAttachmentsSize;
        }

        public ClientConfig GetMailBoxSettings(string host)
        {
            using (var db = GetDb())
            {
                var idProvider = db.ExecuteScalar<int>(
                                new SqlQuery(MailboxDomainTable.name)
                                    .Select(MailboxDomainTable.Columns.id_provider)
                                    .Where(MailboxDomainTable.Columns.name, host));

                if (idProvider < 1)
                    return null;

                var config = new ClientConfig();

                config.EmailProvider.Domain.Add(host);

                var provider = db.ExecuteList(
                    new SqlQuery(MailboxProviderTable.name)
                        .Select(MailboxProviderTable.Columns.name, MailboxProviderTable.Columns.display_name,
                        MailboxProviderTable.Columns.display_short_name, MailboxProviderTable.Columns.documentation)
                        .Where(MailboxProviderTable.Columns.id, idProvider))
                        .FirstOrDefault();

                if (provider == null)
                    return null;

                config.EmailProvider.Id = Convert.ToString(provider[0]);
                config.EmailProvider.DisplayName = Convert.ToString(provider[1]);
                config.EmailProvider.DisplayShortName = Convert.ToString(provider[2]);
                config.EmailProvider.Documentation.Url = Convert.ToString(provider[3]);

                var servers = db.ExecuteList(
                    new SqlQuery(MailboxServerTable.name)
                        .Select(MailboxServerTable.Columns.hostname, MailboxServerTable.Columns.port, MailboxServerTable.Columns.type,
                        MailboxServerTable.Columns.socket_type, MailboxServerTable.Columns.username, MailboxServerTable.Columns.authentication)
                        .Where(MailboxServerTable.Columns.id_provider, idProvider)
                        .Where(MailboxServerTable.Columns.is_user_data, false)); //This condition excludes new data from MailboxServerTable.name. That needed for resolving security issues.

                if (servers.Count == 0)
                    return null;

                servers.ForEach(serv =>
                {
                    var hostname = Convert.ToString(serv[0]);
                    var port = Convert.ToInt32(serv[1]);
                    var type = Convert.ToString(serv[2]);
                    var socketType = Convert.ToString(serv[3]);
                    var username = Convert.ToString(serv[4]);
                    var authentication = Convert.ToString(serv[5]);

                    if (type == "smtp")
                    {
                        config.EmailProvider.OutgoingServer.Add(new ClientConfigEmailProviderOutgoingServer
                            {
                                Type = type,
                                SocketType = socketType,
                                Hostname = hostname,
                                Port = port,
                                Username = username,
                                Authentication = authentication
                            });
                    }
                    else
                    {
                        config.EmailProvider.IncomingServer.Add(new ClientConfigEmailProviderIncomingServer
                            {
                                Type = type,
                                SocketType = socketType,
                                Hostname = hostname,
                                Port = port,
                                Username = username,
                                Authentication = authentication
                            });
                    }

                });

                if (!config.EmailProvider.IncomingServer.Any() || !config.EmailProvider.OutgoingServer.Any())
                    return null;

                return config;
            }
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
                            new SqlQuery(MailboxProviderTable.name)
                                .Select(MailboxProviderTable.Columns.id)
                                .Where(MailboxProviderTable.Columns.name, config.EmailProvider.Id));

                        if (idProvider < 1)
                        {
                            idProvider = db.ExecuteScalar<int>(
                                new SqlInsert(MailboxProviderTable.name)
                                    .InColumnValue(MailboxProviderTable.Columns.id, 0)
                                    .InColumnValue(MailboxProviderTable.Columns.name, config.EmailProvider.Id)
                                    .InColumnValue(MailboxProviderTable.Columns.display_name, config.EmailProvider.DisplayName)
                                    .InColumnValue(MailboxProviderTable.Columns.display_short_name,
                                                   config.EmailProvider.DisplayShortName)
                                    .InColumnValue(MailboxProviderTable.Columns.documentation,
                                                   config.EmailProvider.Documentation.Url)
                                    .Identity(0, 0, true));

                            if (idProvider < 1)
                                throw new Exception("id_provider not saved in DB");
                        }

                        var insertQuery = new SqlInsert(MailboxDomainTable.name)
                            .IgnoreExists(true)
                            .InColumns(MailboxDomainTable.Columns.id_provider, MailboxDomainTable.Columns.name);

                        config.EmailProvider.Domain
                              .ForEach(domain =>
                                       insertQuery
                                           .Values(idProvider, domain));

                        db.ExecuteNonQuery(insertQuery);

                        insertQuery = new SqlInsert(MailboxServerTable.name)
                            .IgnoreExists(true)
                            .InColumns(
                                MailboxServerTable.Columns.id_provider,
                                MailboxServerTable.Columns.type,
                                MailboxServerTable.Columns.hostname,
                                MailboxServerTable.Columns.port,
                                MailboxServerTable.Columns.socket_type,
                                MailboxServerTable.Columns.username,
                                MailboxServerTable.Columns.authentication
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

        private static EncryptionType ConvertToEncryptionType(string type)
        {
            switch (type.ToLower().Trim())
            {
                case "ssl":
                    return EncryptionType.SSL;
                case "starttls":
                    return EncryptionType.StartTLS;
                case "plain":
                    return EncryptionType.None;
                default:
                    throw new ArgumentException("Unknown mail server socket type: " + type);
            }
        }

        private static string ConvertFromEncryptionType(EncryptionType encryptionType)
        {
            switch (encryptionType)
            {
                case EncryptionType.SSL:
                    return "SSL";
                case EncryptionType.StartTLS:
                    return "STARTTLS";
                case EncryptionType.None:
                    return "plain";
                default:
                    throw new ArgumentException("Unknown mail server EncryptionType: " + Enum.GetName(typeof(EncryptionType), encryptionType));
            }
        }

        private static SaslMechanism ConvertToSaslMechanism(string type)
        {
            switch (type.ToLower().Trim())
            {
                case "":
                case "oauth2":
                case "password-cleartext":
                    return SaslMechanism.Login;
                case "none":
                    return SaslMechanism.None;
                case "password-encrypted":
                    return SaslMechanism.CramMd5;
                default:
                    throw new ArgumentException("Unknown mail server authentication type: " + type);
            }
        }

        private static string ConvertFromSaslMechanism(SaslMechanism saslType)
        {
            switch (saslType)
            {
                case SaslMechanism.Login:
                    return "";
                case SaslMechanism.None:
                    return "none";
                case SaslMechanism.CramMd5:
                    return "password-encrypted";
                default:
                    throw new ArgumentException("Unknown mail server SaslMechanism: " + Enum.GetName(typeof(SaslMechanism), saslType));
            }
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
                                IncomingEncryptionType = ConvertToEncryptionType(incomingServer.SocketType),
                                OutcomingEncryptionType = ConvertToEncryptionType(outgoingServer.SocketType),
                                AuthenticationTypeIn = ConvertToSaslMechanism(incomingServer.Authentication),
                                AuthenticationTypeSmtp = ConvertToSaslMechanism(outgoingServer.Authentication),
                                Imap = imap.Value,

                                SmtpAccount = outgoingServerLogin,
                                SmtpPassword = password,
                                SmtpServer = FormatServerFromDb(outgoingServer.Hostname, host),
                                SmtpPort = outgoingServer.Port,
                                SmtpAuth = !string.IsNullOrEmpty(outgoingServer.Username),

                                Enabled = true,
                                TenantId = tenant,
                                UserId = user,
                                Restrict = true,
                                BeginDate = DateTime.UtcNow.Subtract(new TimeSpan(MailBox.DefaultMailLimitedTimeDelta)),
                                ServiceType = (byte)type
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
                    IncomingEncryptionType = isImap ? EncryptionType.SSL : EncryptionType.None,
                    OutcomingEncryptionType = EncryptionType.None,
                    Imap = isImap,
                    SmtpAccount = email,
                    SmtpPassword = password,
                    SmtpServer = string.Format("smtp.{0}", host),
                    SmtpPort = 25,
                    SmtpAuth = true,
                    Enabled = true,
                    TenantId = tenant,
                    UserId = user,
                    Restrict = true,
                    BeginDate = DateTime.UtcNow.Subtract(new TimeSpan(MailBox.DefaultMailLimitedTimeDelta)),
                    AuthenticationTypeIn = SaslMechanism.Login,
                    AuthenticationTypeSmtp = SaslMechanism.Login
                };
        }

        public MailBox SearchMailboxSettings(string email, string password, string user, int tenant)
        {
            var mbox = new MailBox
                {
                    SmtpAccount = email,
                    Account = email,
                    EMail = new MailAddress(email),
                    SmtpAuth = true,
                    SmtpPassword = password,
                    Password = password,
                    Name = ""
                };

            var settingsFromDb = GetMailBoxSettings(email.Substring(email.IndexOf('@') + 1));

            if (settingsFromDb == null)
                throw new ItemNotFoundException("Unknown mail provider settings.");

            var isSuccessedIn = false;
            var lastInErrorPop = String.Empty;
            var lastInErrorImap = String.Empty;

            var lastError = String.Empty;
            var isSuccessed = false;
            mbox.SmtpServer = String.Format("smtp.{0}", email.Substring(email.IndexOf('@') + 1));
            foreach (var settings in
                GetSmtpSettingsVariants(mbox, settingsFromDb)
                    .Where(settings => MailServerHelper.TryTestSmtp(settings, out lastError)))
            {
                mbox.SmtpPassword = settings.AccountPass;
                mbox.SmtpAccount = settings.AccountName;
                mbox.SmtpServer = settings.Url;
                mbox.SmtpPort = settings.Port;
                mbox.AuthenticationTypeSmtp = settings.AuthenticationType;
                mbox.OutcomingEncryptionType = settings.EncryptionType;
                isSuccessed = true;
                break;
            }

            if (!isSuccessed)
            {
                throw new SmtpConnectionException(lastError);
            }

            mbox.Imap = true;
            mbox.Server = String.Format("imap.{0}", email.Substring(email.IndexOf('@') + 1));
            foreach (var settings in GetImapSettingsVariants(mbox, settingsFromDb))
            {
                if (!MailServerHelper.TryTestImap(settings, out lastInErrorPop)) continue;
                mbox.Account = settings.AccountName;
                mbox.Password = settings.AccountPass;
                mbox.Server = settings.Url;
                mbox.Port = settings.Port;
                mbox.AuthenticationTypeIn = settings.AuthenticationType;
                mbox.IncomingEncryptionType = settings.EncryptionType;
                isSuccessedIn = true;
                break;
            }

            if (!isSuccessedIn)
            {
                mbox.Imap = false;
                mbox.Server = String.Format("pop.{0}", email.Substring(email.IndexOf('@') + 1));
                foreach (var settings in GetPopSettingsVariants(mbox, settingsFromDb))
                {
                    if (!MailServerHelper.TryTestPop(settings, out lastInErrorImap)) continue;
                    mbox.Account = settings.AccountName;
                    mbox.Password = settings.AccountPass;
                    mbox.Server = settings.Url;
                    mbox.Port = settings.Port;
                    mbox.AuthenticationTypeIn = settings.AuthenticationType;
                    mbox.IncomingEncryptionType = settings.EncryptionType;
                    isSuccessedIn = true;
                    break;
                }
            }

            if (!isSuccessedIn)
            {
                if (lastInErrorPop == String.Empty && lastInErrorImap == String.Empty)
                {
                    throw new ImapConnectionTimeoutException();
                }
                throw new ImapConnectionException(lastInErrorPop);
            }

            mbox.UserId = user;
            mbox.TenantId = tenant;
            mbox.Restrict = true;
            mbox.BeginDate = DateTime.UtcNow.Subtract(new TimeSpan(MailBox.DefaultMailLimitedTimeDelta));

            return mbox;
        }

        public int SaveMailServerSettings(MailAddress email, MailServerSettings settings, string serverType,
            AuthorizationServiceType authorizationType)
        {
            var host = (authorizationType == AuthorizationServiceType.Google) ? GOOGLE_HOST : email.Host;

            using (var db = GetDb())
            {
                var providerId = db.ExecuteScalar<int>(new SqlQuery(MailboxDomainTable.name)
                                                            .Select(MailboxDomainTable.Columns.id_provider)
                                                            .Where(MailboxProviderTable.Columns.name, host));

                //Save Mailbox provider if not exists
                if (providerId == 0)
                {
                    providerId = db.ExecuteScalar<int>(new SqlInsert(MailboxProviderTable.name)
                                                            .InColumnValue(MailboxProviderTable.Columns.id, 0)
                                                            .InColumnValue(MailboxProviderTable.Columns.name, email.Host)
                                                            .Identity(0, 0, true));
                    db.ExecuteNonQuery(new SqlInsert(MailboxDomainTable.name)
                                                            .InColumnValue(MailboxDomainTable.Columns.id_provider, providerId)
                                                            .InColumnValue(MailboxDomainTable.Columns.name, email.Host));
                }

                //Identify mask for account name
                var accountNameMask = "";
                if (settings.AuthenticationType != SaslMechanism.None)
                {
                    accountNameMask = GetLoginFormatFrom(email, settings.AccountName);
                    if (String.IsNullOrEmpty(accountNameMask))
                    {
                        accountNameMask = settings.AccountName;
                    }
                }

                var settingsId = db.ExecuteScalar<int>(new SqlQuery(MailboxServerTable.name)
                    .Select(MailboxServerTable.Columns.id)
                    .Where(MailboxServerTable.Columns.id_provider, providerId)
                    .Where(MailboxServerTable.Columns.type, serverType)
                    .Where(MailboxServerTable.Columns.hostname, settings.Url)
                    .Where(MailboxServerTable.Columns.port, settings.Port)
                    .Where(MailboxServerTable.Columns.socket_type,
                           ConvertFromEncryptionType(settings.EncryptionType))
                     .Where(MailboxServerTable.Columns.authentication,
                                    ConvertFromSaslMechanism(settings.AuthenticationType))
                     .Where(MailboxServerTable.Columns.username, accountNameMask)
                     .Where(MailboxServerTable.Columns.is_user_data, false));

                if (settingsId == 0)
                {
                    settingsId = db.ExecuteScalar<int>(new SqlInsert(MailboxServerTable.name)
                                           .InColumnValue(MailboxServerTable.Columns.id, 0)
                                           .InColumnValue(MailboxServerTable.Columns.id_provider, providerId)
                                           .InColumnValue(MailboxServerTable.Columns.type, serverType)
                                           .InColumnValue(MailboxServerTable.Columns.hostname, settings.Url)
                                           .InColumnValue(MailboxServerTable.Columns.port, settings.Port)
                                           .InColumnValue(MailboxServerTable.Columns.socket_type,
                                                          ConvertFromEncryptionType(settings.EncryptionType))
                                           .InColumnValue(MailboxServerTable.Columns.authentication,
                                                          ConvertFromSaslMechanism(settings.AuthenticationType))
                                           .InColumnValue(MailboxServerTable.Columns.username, accountNameMask)
                                           .InColumnValue(MailboxServerTable.Columns.is_user_data, true)
                                           .Identity(0, 0, true));
                }

                return settingsId;
            }
        }

        public SignatureDto GetMailboxSignature(int mailboxId, string user, int tenant)
        {
            using (var db = GetDb())
            {
                CheckMailboxOwnage(mailboxId, user, tenant, db);

                var signatureDal = new SignatureDal(db);
                return signatureDal.GetSignature(mailboxId, tenant);
            }
        }

        public SignatureDto UpdateOrCreateMailboxSignature(int mailboxId, string user, int tenant, string html, bool isActive)
        {
            using (var db = GetDb())
            {
                CheckMailboxOwnage(mailboxId, user, tenant, db);

                var signature = new SignatureDto(mailboxId, tenant, html, isActive);
                var signatureDal = new SignatureDal(db);
                signatureDal.UpdateOrCreateSignature(signature);
                return signature;
            }
        }

        private static List<SignatureDto> GetMailboxesSignatures(List<int> mailboxIds, int tenant, DbManager db)
        {
            var signatureDal = new SignatureDal(db);
            return signatureDal.GetSignatures(mailboxIds, tenant);
        }

        public bool SetMailboxEmailInFolder(int tenant, string user, int mailboxId, string emailInFolder)
        {
            using (var db = GetDb())
            {
                var query = new SqlUpdate(MailboxTable.name)
                    .Set(MailboxTable.Columns.email_in_folder, "" != emailInFolder ? emailInFolder : null)
                    .Where(GetUserWhere(user, tenant))
                    .Where(MailboxTable.Columns.id, mailboxId);

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
            IdTenant, IdUser, Name, Address, Account, Password, InServer, InPort, SizeLast, MsgCountLast, SmtpServer, SmtpPort,
            SmtpPassword, SmtpAccount, LoginDelay, Id, Enabled, QuotaError, AuthError, Imap, BeginDate,
            ServiceType, RefreshToken, ImapIntervals, OutcomingEncryptionType, IncomingEncryptionType,
            AuthTypeIn, AuthtTypeSmtp, IdSmtpServer, IdInServer, EMailInFolder, IsTeamlabMailbox, IsRemoved
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
        const int SELECT_MAIL_ITEM_ATTACHMENT_FIELDS_COUNT = 11;
        const int SELECT_ACCOUNT_FIELDS_COUNT = 17;
        const string MAILBOX_ALIAS = "mb";
        const string OUT_SERVER_ALIAS = "out_s";
        const string IN_SERVER_ALIAS = "in_s";

        private static SqlQuery GetSelectMailBoxFieldsQuery()
        {
            var fieldsForSelect = new string[SELECT_MAIL_BOX_FIELDS_COUNT];
            fieldsForSelect[(int)MailBoxFieldSelectPosition.IdTenant] = MailboxTable.Columns.id_tenant.Prefix(MAILBOX_ALIAS);
            fieldsForSelect[(int)MailBoxFieldSelectPosition.IdUser] = MailboxTable.Columns.id_user.Prefix(MAILBOX_ALIAS);
            fieldsForSelect[(int)MailBoxFieldSelectPosition.Name] = MailboxTable.Columns.name.Prefix(MAILBOX_ALIAS);
            fieldsForSelect[(int)MailBoxFieldSelectPosition.Address] = MailboxTable.Columns.address.Prefix(MAILBOX_ALIAS);
            fieldsForSelect[(int)MailBoxFieldSelectPosition.Account] = MailboxServerTable.Columns.username.Prefix(IN_SERVER_ALIAS);
            fieldsForSelect[(int)MailBoxFieldSelectPosition.Password] = MailboxTable.Columns.password.Prefix(MAILBOX_ALIAS);
            fieldsForSelect[(int)MailBoxFieldSelectPosition.InServer] = MailboxServerTable.Columns.hostname.Prefix(IN_SERVER_ALIAS);
            fieldsForSelect[(int)MailBoxFieldSelectPosition.InPort] = MailboxServerTable.Columns.port.Prefix(IN_SERVER_ALIAS);
            fieldsForSelect[(int)MailBoxFieldSelectPosition.SizeLast] = MailboxTable.Columns.size_last.Prefix(MAILBOX_ALIAS);
            fieldsForSelect[(int)MailBoxFieldSelectPosition.MsgCountLast] = MailboxTable.Columns.msg_count_last.Prefix(MAILBOX_ALIAS);
            fieldsForSelect[(int)MailBoxFieldSelectPosition.SmtpServer] = MailboxServerTable.Columns.hostname.Prefix(OUT_SERVER_ALIAS);
            fieldsForSelect[(int)MailBoxFieldSelectPosition.SmtpPort] = MailboxServerTable.Columns.port.Prefix(OUT_SERVER_ALIAS);
            fieldsForSelect[(int)MailBoxFieldSelectPosition.SmtpPassword] = MailboxTable.Columns.smtp_password.Prefix(MAILBOX_ALIAS);
            fieldsForSelect[(int)MailBoxFieldSelectPosition.SmtpAccount] = MailboxServerTable.Columns.username.Prefix(OUT_SERVER_ALIAS);
            fieldsForSelect[(int)MailBoxFieldSelectPosition.LoginDelay] = MailboxTable.Columns.login_delay.Prefix(MAILBOX_ALIAS);
            fieldsForSelect[(int)MailBoxFieldSelectPosition.Id] = MailboxTable.Columns.id.Prefix(MAILBOX_ALIAS);
            fieldsForSelect[(int)MailBoxFieldSelectPosition.Enabled] = MailboxTable.Columns.enabled.Prefix(MAILBOX_ALIAS);
            fieldsForSelect[(int)MailBoxFieldSelectPosition.QuotaError] = MailboxTable.Columns.quota_error.Prefix(MAILBOX_ALIAS);
            fieldsForSelect[(int)MailBoxFieldSelectPosition.AuthError] = MailboxTable.Columns.date_auth_error.Prefix(MAILBOX_ALIAS);
            fieldsForSelect[(int)MailBoxFieldSelectPosition.Imap] = MailboxTable.Columns.imap.Prefix(MAILBOX_ALIAS);
            fieldsForSelect[(int)MailBoxFieldSelectPosition.BeginDate] = MailboxTable.Columns.begin_date.Prefix(MAILBOX_ALIAS);
            fieldsForSelect[(int)MailBoxFieldSelectPosition.ServiceType] = MailboxTable.Columns.service_type.Prefix(MAILBOX_ALIAS);
            fieldsForSelect[(int)MailBoxFieldSelectPosition.RefreshToken] = MailboxTable.Columns.refresh_token.Prefix(MAILBOX_ALIAS);
            fieldsForSelect[(int)MailBoxFieldSelectPosition.ImapIntervals] = MailboxTable.Columns.imap_intervals.Prefix(MAILBOX_ALIAS);
            fieldsForSelect[(int)MailBoxFieldSelectPosition.EMailInFolder] = MailboxTable.Columns.email_in_folder.Prefix(MAILBOX_ALIAS);
            fieldsForSelect[(int)MailBoxFieldSelectPosition.OutcomingEncryptionType] = MailboxServerTable.Columns.socket_type.Prefix(OUT_SERVER_ALIAS);
            fieldsForSelect[(int)MailBoxFieldSelectPosition.IncomingEncryptionType] = MailboxServerTable.Columns.socket_type.Prefix(IN_SERVER_ALIAS);
            fieldsForSelect[(int)MailBoxFieldSelectPosition.AuthTypeIn] = MailboxServerTable.Columns.authentication.Prefix(IN_SERVER_ALIAS);
            fieldsForSelect[(int)MailBoxFieldSelectPosition.AuthtTypeSmtp] = MailboxServerTable.Columns.authentication.Prefix(OUT_SERVER_ALIAS);
            fieldsForSelect[(int)MailBoxFieldSelectPosition.IdSmtpServer] = MailboxServerTable.Columns.id.Prefix(OUT_SERVER_ALIAS);
            fieldsForSelect[(int)MailBoxFieldSelectPosition.IdInServer] = MailboxServerTable.Columns.id.Prefix(IN_SERVER_ALIAS);
            fieldsForSelect[(int)MailBoxFieldSelectPosition.IsTeamlabMailbox] = MailboxTable.Columns.is_teamlab_mailbox.Prefix(MAILBOX_ALIAS);
            fieldsForSelect[(int)MailBoxFieldSelectPosition.IsRemoved] = MailboxTable.Columns.is_removed.Prefix(MAILBOX_ALIAS);

            return new SqlQuery(MailboxTable.name.Alias(MAILBOX_ALIAS))
                .InnerJoin(MailboxServerTable.name.Alias(OUT_SERVER_ALIAS),
                            Exp.EqColumns(MailboxTable.Columns.id_smtp_server.Prefix(MAILBOX_ALIAS), MailboxServerTable.Columns.id.Prefix(OUT_SERVER_ALIAS)))
                .InnerJoin(MailboxServerTable.name.Alias(IN_SERVER_ALIAS),
                            Exp.EqColumns(MailboxTable.Columns.id_in_server.Prefix(MAILBOX_ALIAS), MailboxServerTable.Columns.id.Prefix(IN_SERVER_ALIAS)))
                .Select(fieldsForSelect);
        }

        private MailBox ToMailBox(object[] r)
        {
            if (r.Length != SELECT_MAIL_BOX_FIELDS_COUNT)
            {
                _log.Error("ToMailBoxCount of returned fields Length = {0} not equal to SELECT_MAIL_BOX_FIELDS_COUNT = {1}",
                    r.Length, SELECT_MAIL_BOX_FIELDS_COUNT);
                return null;
            }

            var inMailAddress = new MailAddress((string)r[(int)MailBoxFieldSelectPosition.Address]);
            var inAccount = FormatLoginFromDb((string)r[(int)MailBoxFieldSelectPosition.Account], inMailAddress);
            var smtpAccount = FormatLoginFromDb((string)r[(int)MailBoxFieldSelectPosition.SmtpAccount], inMailAddress);
            var inServerOldFormat = (string)r[(int)MailBoxFieldSelectPosition.InServer] + ":" + r[(int)MailBoxFieldSelectPosition.InPort];
            var smtpServerOldFormat = (string)r[(int)MailBoxFieldSelectPosition.SmtpServer] + ":" + r[(int)MailBoxFieldSelectPosition.SmtpPort];
            var inEncryption = ConvertToEncryptionType((string)r[(int)MailBoxFieldSelectPosition.IncomingEncryptionType]);
            var smtpEncryption = ConvertToEncryptionType((string)r[(int)MailBoxFieldSelectPosition.OutcomingEncryptionType]);
            var inAuth = ConvertToSaslMechanism((string)r[(int)MailBoxFieldSelectPosition.AuthTypeIn]);
            var smtpAuth = ConvertToSaslMechanism((string)r[(int)MailBoxFieldSelectPosition.AuthtTypeSmtp]);

            string inPassword, outPassword = null;
            TryDecryptPassword((string) r[(int) MailBoxFieldSelectPosition.Password], out inPassword);
            
            if (r[(int)MailBoxFieldSelectPosition.SmtpPassword] != null)
                TryDecryptPassword((string)r[(int)MailBoxFieldSelectPosition.SmtpPassword], out outPassword);

            var res = new MailBox(
                Convert.ToInt32(r[(int) MailBoxFieldSelectPosition.IdTenant]),
                (string) r[(int) MailBoxFieldSelectPosition.IdUser],
                (string) r[(int) MailBoxFieldSelectPosition.Name],
                inMailAddress,
                inAccount,
                inPassword,
                inServerOldFormat,
                Convert.ToBoolean(r[(int) MailBoxFieldSelectPosition.Imap]),
                smtpServerOldFormat,
                outPassword,
                inAuth != SaslMechanism.None,
                Convert.ToInt32(r[(int) MailBoxFieldSelectPosition.Id]),
                (DateTime) r[(int) MailBoxFieldSelectPosition.BeginDate],
                inEncryption,
                smtpEncryption,
                Convert.ToByte(r[(int) MailBoxFieldSelectPosition.ServiceType]),
                (string) r[(int) MailBoxFieldSelectPosition.RefreshToken],
                (string) r[(int) MailBoxFieldSelectPosition.EMailInFolder],
                Convert.ToBoolean(r[(int) MailBoxFieldSelectPosition.IsRemoved])
                )
                {
                    Size = Convert.ToInt64(r[(int) MailBoxFieldSelectPosition.SizeLast]),
                    MessagesCount = Convert.ToInt32(r[(int) MailBoxFieldSelectPosition.MsgCountLast]),
                    SmtpAccount = smtpAccount,
                    ServerLoginDelay = Convert.ToInt32(r[(int) MailBoxFieldSelectPosition.LoginDelay]),
                    Enabled = Convert.ToBoolean(r[(int) MailBoxFieldSelectPosition.Enabled]),
                    IsTeamlab = Convert.ToBoolean(r[(int) MailBoxFieldSelectPosition.IsTeamlabMailbox]),
                    QuotaError = Convert.ToBoolean(r[(int) MailBoxFieldSelectPosition.QuotaError]),
                    AuthErrorDate = r[(int) MailBoxFieldSelectPosition.AuthError] != null
                                        ? Convert.ToDateTime(r[(int) MailBoxFieldSelectPosition.AuthError])
                                        : (DateTime?) null,
                    ImapIntervalsJson = (string) r[(int) MailBoxFieldSelectPosition.ImapIntervals],
                    AuthenticationTypeIn = inAuth,
                    AuthenticationTypeSmtp = smtpAuth,
                    SmtpServerId = (int) r[(int) MailBoxFieldSelectPosition.IdSmtpServer],
                    InServerId = (int) r[(int) MailBoxFieldSelectPosition.IdInServer]
                };

            return res;
        }

        private List<AccountInfo> ToAccountInfo(IEnumerable<object[]> objectList, List<SignatureDto> signatures)
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
                            authErrorType, signature,
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
                            (string)(r[(int)MailAccountFieldSelectPosition.AliasName]) + '@' +
                            (string)(r[(int)MailAccountFieldSelectPosition.DomainName]),
                            Convert.ToInt32(r[(int)MailAccountFieldSelectPosition.DomainId]));

                    accounts[accountIndex].Aliases.Add(alias);
                }
            }
            return accounts;
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
                    contentId = Convert.ToString(r[(int)MailItemAttachmentSelectPosition.ContentId]),
                    mailboxId = Convert.ToInt32(r[(int)MailItemAttachmentSelectPosition.MailboxId]),
                };

            // if StoredName is empty then attachment had been stored by filename (old attachment);
            attachment.storedName = string.IsNullOrEmpty(attachment.storedName) ? attachment.fileName : attachment.storedName;

            return attachment;
        }

        private void GetMailBoxState(int mailboxId, out bool isRemoved, out bool isDeactivated, out DateTime beginDate)
        {
            isRemoved = true;
            isDeactivated = true;
            beginDate = MinBeginDate;

            using (var db = GetDb())
            {
                var res = db.ExecuteList(new SqlQuery(MailboxTable.name)
                        .Select(MailboxTable.Columns.is_removed, MailboxTable.Columns.enabled, MailboxTable.Columns.begin_date)
                        .Where(Exp.Eq(MailboxTable.Columns.id, mailboxId)))
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
                    new SqlQuery(MailboxTable.name)
                        .Select(MailboxTable.Columns.id)
                        .Where(GetUserWhere(user, tenant))
                        .Where(MailboxTable.Columns.address, address)
                        .Where(MailboxTable.Columns.is_removed, false));
            }
        }

        private static IEnumerable<MailServerSettings> GetPopSettingsVariants(MailBox mbox, ClientConfig config)
        {
            var tempList = new List<MailServerSettings>();
            if (config != null && config.EmailProvider.IncomingServer != null)
            {
                var address = mbox.EMail;
                tempList.AddRange(from popServer in config.EmailProvider.IncomingServer.Where(x => x.Type == "pop3")
                                  where popServer.Hostname != null
                                  select new MailServerSettings
                                      {
                                          Url = FormatServerFromDb(popServer.Hostname, address.Host.ToLowerInvariant()),
                                          Port = popServer.Port,
                                          AccountName = FormatLoginFromDb(popServer.Username, address),
                                          AccountPass = mbox.Password,
                                          AuthenticationType = ConvertToSaslMechanism(popServer.Authentication),
                                          EncryptionType = ConvertToEncryptionType(popServer.SocketType)
                                      });
            }

            return tempList;
        }

        private static IEnumerable<MailServerSettings> GetImapSettingsVariants(MailBox mbox, ClientConfig config)
        {
            var tempList = new List<MailServerSettings>();
            if (config != null && config.EmailProvider.IncomingServer != null)
            {
                var address = mbox.EMail;
                tempList.AddRange(from imapServer in config.EmailProvider.IncomingServer.Where(x => x.Type == "imap")
                                  where imapServer.Hostname != null
                                  select new MailServerSettings
                                      {
                                          Url = FormatServerFromDb(imapServer.Hostname, address.Host.ToLowerInvariant()),
                                          Port = imapServer.Port,
                                          AccountName = FormatLoginFromDb(imapServer.Username, address),
                                          AccountPass = mbox.Password,
                                          AuthenticationType = ConvertToSaslMechanism(imapServer.Authentication),
                                          EncryptionType = ConvertToEncryptionType(imapServer.SocketType)
                                      });
            }

            return tempList;
        }

        private static IEnumerable<MailServerSettings> GetSmtpSettingsVariants(MailBox mbox, ClientConfig config)
        {
            var tempList = new List<MailServerSettings>();
            if (config != null && config.EmailProvider.OutgoingServer != null)
            {
                var address = mbox.EMail;
                tempList.AddRange(
                    config.EmailProvider.OutgoingServer.Select(mailServerSettingse => new MailServerSettings
                        {
                            Url = FormatServerFromDb(mailServerSettingse.Hostname, address.Host.ToLowerInvariant()),
                            Port = mailServerSettingse.Port,
                            AccountName = FormatLoginFromDb(mailServerSettingse.Username, address),
                            AccountPass = mbox.SmtpPassword,
                            AuthenticationType = ConvertToSaslMechanism(mailServerSettingse.Authentication),
                            EncryptionType = ConvertToEncryptionType(mailServerSettingse.SocketType)
                        }));
            }

            return tempList;
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
            var checkMailboxOwnage = new SqlQuery(MailboxTable.name)
                .Select(MailboxTable.Columns.id)
                .Where(MailboxTable.Columns.id_user, user)
                .Where(MailboxTable.Columns.id_tenant, tenant)
                .Where(mailboxIds.Count > 1
                           ? Exp.In(MailboxTable.Columns.id, mailboxIds)
                           : Exp.Eq(MailboxTable.Columns.id, mailboxIds[0]));

            var foundIds = db.ExecuteList(checkMailboxOwnage)
                .ConvertAll(res => Convert.ToInt32(res[0]));

            if (!mailboxIds.Any(idMailbox => foundIds.Exists(foundId => foundId == idMailbox)))
                throw new AccessViolationException("Mailbox doesn't owned by user.");
        }

        #endregion
    }
}