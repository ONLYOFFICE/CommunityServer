/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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
using ASC.Mail.Aggregator.Dal;
using ASC.Mail.Aggregator.Dal.DbSchema;
using ASC.Mail.Aggregator.Common.Extension;
using ActiveUp.Net.Mail;

namespace ASC.Mail.Aggregator
{
    public partial class MailBoxManager
    {
        #region db defines

        // ReSharper disable InconsistentNaming
        public static readonly DateTime MIN_BEGIN_DATE = new DateTime(1975, 1, 1, 0, 0, 0);
        // ReSharper restore InconsistentNaming

        #endregion

        # region public methods

        public bool SaveMailBox(MailBox mail_box)
        {
            if (mail_box == null) throw new ArgumentNullException("mail_box");

            var id_mailbox = MailBoxExists(GetAddress(mail_box.EMail), mail_box.UserId, mail_box.TenantId);

            using (var db = GetDb())
            {
                int result;

                var login_delay_time = GetLoginDelayTime(mail_box);

                if (id_mailbox == 0)
                {
                    result = db.ExecuteScalar<int>(
                        new SqlInsert(MailboxTable.name)
                            .InColumnValue(MailboxTable.Columns.id, 0)
                            .InColumnValue(MailboxTable.Columns.id_tenant, mail_box.TenantId)
                            .InColumnValue(MailboxTable.Columns.id_user, mail_box.UserId)
                            .InColumnValue(MailboxTable.Columns.address, GetAddress(mail_box.EMail))
                            .InColumnValue(MailboxTable.Columns.name, mail_box.Name)
                            .InColumnValue(MailboxTable.Columns.password, EncryptPassword(mail_box.Password))
                            .InColumnValue(MailboxTable.Columns.msg_count_last, mail_box.MessagesCount)
                            .InColumnValue(MailboxTable.Columns.smtp_password,
                                           string.IsNullOrEmpty(mail_box.SmtpPassword)
                                               ? EncryptPassword(mail_box.Password)
                                               : EncryptPassword(mail_box.SmtpPassword))
                            .InColumnValue(MailboxTable.Columns.size_last, mail_box.Size)
                            .InColumnValue(MailboxTable.Columns.login_delay, login_delay_time)
                            .InColumnValue(MailboxTable.Columns.enabled, true)
                            .InColumnValue(MailboxTable.Columns.imap, mail_box.Imap)
                            .InColumnValue(MailboxTable.Columns.begin_date, mail_box.BeginDate)
                            .InColumnValue(MailboxTable.Columns.service_type, mail_box.ServiceType)
                            .InColumnValue(MailboxTable.Columns.refresh_token, mail_box.RefreshToken)
                            .InColumnValue(MailboxTable.Columns.id_smtp_server, mail_box.SmtpServerId)
                            .InColumnValue(MailboxTable.Columns.id_in_server, mail_box.InServerId)
                            .Identity(0, 0, true));

                    mail_box.MailBoxId = result;
                }
                else
                {
                    mail_box.MailBoxId = id_mailbox;

                    var query_update = new SqlUpdate(MailboxTable.name)
                        .Where(MailboxTable.Columns.id, id_mailbox)
                        .Set(MailboxTable.Columns.id_tenant, mail_box.TenantId)
                        .Set(MailboxTable.Columns.id_user, mail_box.UserId)
                        .Set(MailboxTable.Columns.address, GetAddress(mail_box.EMail))
                        .Set(MailboxTable.Columns.name, mail_box.Name)
                        .Set(MailboxTable.Columns.password, EncryptPassword(mail_box.Password))
                        .Set(MailboxTable.Columns.msg_count_last, mail_box.MessagesCount)
                        .Set(MailboxTable.Columns.smtp_password,
                             string.IsNullOrEmpty(mail_box.SmtpPassword)
                                 ? EncryptPassword(mail_box.Password)
                                 : EncryptPassword(mail_box.SmtpPassword))
                        .Set(MailboxTable.Columns.size_last, mail_box.Size)
                        .Set(MailboxTable.Columns.login_delay, login_delay_time)
                        .Set(MailboxTable.Columns.is_removed, false)
                        .Set(MailboxTable.Columns.imap, mail_box.Imap)
                        .Set(MailboxTable.Columns.begin_date, mail_box.BeginDate)
                        .Set(MailboxTable.Columns.service_type, mail_box.ServiceType)
                        .Set(MailboxTable.Columns.refresh_token, mail_box.RefreshToken)
                        .Set(MailboxTable.Columns.id_smtp_server, mail_box.SmtpServerId)
                        .Set(MailboxTable.Columns.id_in_server, mail_box.InServerId);

                    if (mail_box.BeginDate == MIN_BEGIN_DATE)
                        query_update.Set(MailboxTable.Columns.imap_folders, "[]");

                    result = db.ExecuteNonQuery(query_update);
                }

                return result > 0;
            }
        }

        public List<AccountInfo> GetAccountInfo(int id_tenant, string id_user)
        {
            const string mailbox_alias = "ma";
            const string server_address = "sa";
            const string server_domain = "sd";
            const string group_x_address = "ga";
            const string server_group = "sg";

            var query = new SqlQuery(MailboxTable.name + " " + mailbox_alias)
                .LeftOuterJoin(AddressTable.name + " " + server_address,
                               Exp.EqColumns(MailboxTable.Columns.id.Prefix(mailbox_alias),
                                             AddressTable.Columns.id_mailbox.Prefix(server_address)))
                .LeftOuterJoin(DomainTable.name + " " + server_domain,
                               Exp.EqColumns(AddressTable.Columns.id_domain.Prefix(server_address),
                                             DomainTable.Columns.id.Prefix(server_domain)))
                .LeftOuterJoin(MailGroupXAddressesTable.name + " " + group_x_address,
                               Exp.EqColumns(AddressTable.Columns.id.Prefix(server_address),
                                             MailGroupXAddressesTable.Columns.id_address.Prefix(group_x_address)))
                .LeftOuterJoin(MailGroupTable.name + " " + server_group,
                               Exp.EqColumns(MailGroupXAddressesTable.Columns.id_mail_group.Prefix(group_x_address),
                                             MailGroupTable.Columns.id.Prefix(server_group)))
                .Select(MailboxTable.Columns.id.Prefix(mailbox_alias))
                .Select(MailboxTable.Columns.address.Prefix(mailbox_alias))
                .Select(MailboxTable.Columns.enabled.Prefix(mailbox_alias))
                .Select(MailboxTable.Columns.name.Prefix(mailbox_alias))
                .Select(MailboxTable.Columns.quota_error.Prefix(mailbox_alias))
                .Select(MailboxTable.Columns.auth_error.Prefix(mailbox_alias))
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
                .Where(MailboxTable.Columns.is_removed.Prefix(mailbox_alias), false)
                .Where(MailboxTable.Columns.id_tenant.Prefix(mailbox_alias), id_tenant)
                .Where(MailboxTable.Columns.id_user.Prefix(mailbox_alias), id_user.ToLowerInvariant())
                .OrderBy(AddressTable.Columns.is_alias.Prefix(server_address), true);

            List<object[]> result;
            List<SignatureDto> signatures;
            using (var db = GetDb())
            {
                result = db.ExecuteList(query);
                var mailbox_ids = result.ConvertAll(r =>
                    Convert.ToInt32(r[(int)MailAccountFieldSelectPosition.Id])).Distinct().ToList();
                signatures = GetMailboxesSignatures(mailbox_ids, id_tenant, db);
            }

            return ToAccountInfo(result, signatures);
        }


        public List<MailBox> GetMailBoxes(int id_tenant, string id_user)
        {
            var where = Exp.Eq(MailboxTable.Columns.id_tenant, id_tenant) & Exp.Eq(MailboxTable.Columns.id_user, id_user.ToLowerInvariant());
            return GetMailBoxes(where);
        }

        // returns whether user has not removed accounts
        public bool HasMailboxes(int id_tenant, string id_user)
        {
            using (var db = GetDb())
            {
                var count = db.ExecuteScalar<int>(new SqlQuery(MailboxTable.name)
                                                      .SelectCount()
                                                      .Where(MailboxTable.Columns.id_tenant, id_tenant)
                                                      .Where(MailboxTable.Columns.id_user, id_user.ToLowerInvariant())
                                                      .Where(MailboxTable.Columns.is_removed, 0));
                return count > 0;
            }
        }

        public MailBox GetMailBox(int id_tenant, string id_user, MailAddress email)
        {
            return GetMailBoxes(Exp.Eq(MailboxTable.Columns.address.Prefix(mail_mailbox_alias), GetAddress(email)) &
                Exp.Eq(MailboxTable.Columns.id_tenant.Prefix(mail_mailbox_alias), id_tenant) &
                Exp.Eq(MailboxTable.Columns.id_user.Prefix(mail_mailbox_alias), id_user.ToLowerInvariant()))
                .SingleOrDefault();
        }

        public MailBox GetServerMailBox(int id_tenant, int mailbox_id, DbManager db_manager)
        {
            return GetMailBoxes(Exp.Eq(MailboxTable.Columns.id.Prefix(mail_mailbox_alias), mailbox_id) &
                                Exp.Eq(MailboxTable.Columns.id_tenant.Prefix(mail_mailbox_alias), id_tenant) &
                                Exp.Eq(MailboxTable.Columns.is_teamlab_mailbox.Prefix(mail_mailbox_alias), true),
                                db_manager)
                .SingleOrDefault();
        }

        public MailBox GetMailBox(int mailbox_id)
        {
            return GetMailBoxes(Exp.Eq(MailboxTable.Columns.id.Prefix(mail_mailbox_alias), mailbox_id))
               .SingleOrDefault();
        }

        public bool EnableMaibox(MailBox mailbox, bool enabled)
        {
            using (var db = GetDb())
            {
                var update_query = new SqlUpdate(MailboxTable.name)
                    .Where(MailboxTable.Columns.id, mailbox.MailBoxId)
                    .Where(GetUserWhere(mailbox.UserId, mailbox.TenantId))
                    .Where(MailboxTable.Columns.is_removed, false)
                    .Set(MailboxTable.Columns.enabled, enabled);

                if (enabled)
                    update_query.Set(MailboxTable.Columns.auth_error, null);

                var result = db.ExecuteNonQuery(update_query);

                return result > 0;
            }
        }
        public void RemoveMailBox(MailBox mail_box)
        {
            if (mail_box.MailBoxId <= 0)
                throw new Exception("MailBox id is 0");

            long total_attachments_size;

            using (var db = GetDb())
            {
                using (var tx = db.BeginTransaction())
                {
                    total_attachments_size = RemoveMailBox(mail_box, db);

                    tx.Commit();
                }
            }

            QuotaUsedDelete(mail_box.TenantId, total_attachments_size);
        }

        public long RemoveMailBox(MailBox mail_box, DbManager db)
        {
            if (mail_box.MailBoxId <= 0)
                throw new Exception("MailBox id is 0");

            db.ExecuteNonQuery(
                new SqlUpdate(MailboxTable.name)
                    .Set(MailboxTable.Columns.is_removed, true)
                    .Where(MailboxTable.Columns.id, mail_box.MailBoxId));

            db.ExecuteNonQuery(
                new SqlDelete(ChainTable.name)
                    .Where(GetUserWhere(mail_box.UserId, mail_box.TenantId))
                    .Where(ChainTable.Columns.id_mailbox, mail_box.MailBoxId));


            db.ExecuteNonQuery(
                new SqlUpdate(MailTable.name)
                    .Set(MailTable.Columns.is_removed, true)
                    .Where(MailTable.Columns.id_mailbox, mail_box.MailBoxId)
                    .Where(GetUserWhere(mail_box.UserId, mail_box.TenantId)));

            var total_attachments_size = db.ExecuteScalar<long>(
                string.Format(
                    "select sum(a.size) from {0} a inner join {1} m on a.{2} = m.{3} where m.{4} = @mailbox_id and m.{5} = @tid and a.{6} != @need_remove",
                    AttachmentTable.name,
                    MailTable.name,
                    AttachmentTable.Columns.id_mail,
                    MailTable.Columns.id,
                    MailTable.Columns.id_mailbox,
                    MailTable.Columns.id_tenant,
                    AttachmentTable.Columns.need_remove), new { tid = mail_box.TenantId, need_remove = true, mailbox_id = mail_box.MailBoxId });

            var query = string.Format("update {0} a inner join {1} m on a.{2} = m.{3} set a.{4} = @need_remove where m.{5} = @mailbox_id",
                    AttachmentTable.name, MailTable.name, AttachmentTable.Columns.id_mail, MailTable.Columns.id, AttachmentTable.Columns.need_remove, MailTable.Columns.id_mailbox);

            db.ExecuteNonQuery(query, new { need_remove = true, mailbox_id = mail_box.MailBoxId });


            query = string.Format("select t.{0} from {1} t inner join {2} m on t.{3} = m.{4} where m.{5} = @mailbox_id",
                TagMailFields.id_tag, MAIL_TAG_MAIL, MailTable.name, TagMailFields.id_mail, MailTable.Columns.id, MailTable.Columns.id_mailbox);

            var affected_tags = db.ExecuteList(query, new { mailbox_id = mail_box.MailBoxId })
                .ConvertAll(r => Convert.ToInt32(r[0]))
                .Distinct();

            query = string.Format("delete t from {0} t inner join {1} m on t.{2} = m.{3} where m.{4} = @mailbox_id",
                                  MAIL_TAG_MAIL, MailTable.name, TagMailFields.id_mail, MailTable.Columns.id, MailTable.Columns.id_mailbox);

            db.ExecuteNonQuery(query, new { mailbox_id = mail_box.MailBoxId });

            UpdateTagsCount(db, mail_box.TenantId, mail_box.UserId, affected_tags);

            RecalculateFolders(db, mail_box.TenantId, mail_box.UserId);

            return total_attachments_size;
        }

        public clientConfig GetMailBoxSettings(string host)
        {
            using (var db = GetDb())
            {
                var id_provider = db.ExecuteScalar<int>(
                                new SqlQuery(MailboxDomainTable.name)
                                    .Select(MailboxDomainTable.Columns.id_provider)
                                    .Where(MailboxDomainTable.Columns.name, host));

                if (id_provider < 1)
                    return null;

                var config = new clientConfig();

                config.emailProvider.domain.Add(host);

                var provider = db.ExecuteList(
                    new SqlQuery(MailboxProviderTable.name)
                        .Select(MailboxProviderTable.Columns.name, MailboxProviderTable.Columns.display_name,
                        MailboxProviderTable.Columns.display_short_name, MailboxProviderTable.Columns.documentation)
                        .Where(MailboxProviderTable.Columns.id, id_provider))
                        .FirstOrDefault();

                if (provider == null)
                    return null;

                config.emailProvider.id = Convert.ToString(provider[0]);
                config.emailProvider.displayName = Convert.ToString(provider[1]);
                config.emailProvider.displayShortName = Convert.ToString(provider[2]);
                config.emailProvider.documentation.url = Convert.ToString(provider[3]);

                var servers = db.ExecuteList(
                    new SqlQuery(MailboxServerTable.name)
                        .Select(MailboxServerTable.Columns.hostname, MailboxServerTable.Columns.port, MailboxServerTable.Columns.type,
                        MailboxServerTable.Columns.socket_type, MailboxServerTable.Columns.username, MailboxServerTable.Columns.authentication)
                        .Where(MailboxServerTable.Columns.id_provider, id_provider)
                        .Where(MailboxServerTable.Columns.is_user_data, false)); //This condition excludes new data from MailboxServerTable.name. That needed for resolving security issues.

                if (servers.Count == 0)
                    return null;

                servers.ForEach(serv =>
                {
                    var hostname = Convert.ToString(serv[0]);
                    var port = Convert.ToInt32(serv[1]);
                    var type = Convert.ToString(serv[2]);
                    var socket_type = Convert.ToString(serv[3]);
                    var username = Convert.ToString(serv[4]);
                    var authentication = Convert.ToString(serv[5]);

                    if (type == "smtp")
                    {
                        config.emailProvider.outgoingServer.Add(new clientConfigEmailProviderOutgoingServer
                            {
                                type = type,
                                socketType = socket_type,
                                hostname = hostname,
                                port = port,
                                username = username,
                                authentication = authentication
                            });
                    }
                    else
                    {
                        config.emailProvider.incomingServer.Add(new clientConfigEmailProviderIncomingServer
                            {
                                type = type,
                                socketType = socket_type,
                                hostname = hostname,
                                port = port,
                                username = username,
                                authentication = authentication
                            });
                    }

                });

                if (!config.emailProvider.incomingServer.Any() || !config.emailProvider.outgoingServer.Any())
                    return null;

                return config;
            }
        }

        public bool SetMailBoxSettings(clientConfig config)
        {
            try
            {
                if (string.IsNullOrEmpty(config.emailProvider.id) ||
                    config.emailProvider.incomingServer == null ||
                    !config.emailProvider.incomingServer.Any() ||
                    config.emailProvider.outgoingServer == null ||
                    !config.emailProvider.outgoingServer.Any())
                    throw new Exception("Incorrect config");

                using (var db = GetDb())
                {
                    using (var tx = db.BeginTransaction())
                    {
                        var id_provider = db.ExecuteScalar<int>(
                            new SqlQuery(MailboxProviderTable.name)
                                .Select(MailboxProviderTable.Columns.id)
                                .Where(MailboxProviderTable.Columns.name, config.emailProvider.id));

                        if (id_provider < 1)
                        {
                            id_provider = db.ExecuteScalar<int>(
                                new SqlInsert(MailboxProviderTable.name)
                                    .InColumnValue(MailboxProviderTable.Columns.id, 0)
                                    .InColumnValue(MailboxProviderTable.Columns.name, config.emailProvider.id)
                                    .InColumnValue(MailboxProviderTable.Columns.display_name, config.emailProvider.displayName)
                                    .InColumnValue(MailboxProviderTable.Columns.display_short_name,
                                                   config.emailProvider.displayShortName)
                                    .InColumnValue(MailboxProviderTable.Columns.documentation,
                                                   config.emailProvider.documentation.url)
                                    .Identity(0, 0, true));

                            if (id_provider < 1)
                                throw new Exception("id_provider not saved in DB");
                        }

                        var insert_query = new SqlInsert(MailboxDomainTable.name)
                            .IgnoreExists(true)
                            .InColumns(MailboxDomainTable.Columns.id_provider, MailboxDomainTable.Columns.name);

                        config.emailProvider.domain
                              .ForEach(domain =>
                                       insert_query
                                           .Values(id_provider, domain));

                        db.ExecuteNonQuery(insert_query);

                        insert_query = new SqlInsert(MailboxServerTable.name)
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

                        config.emailProvider.incomingServer
                              .ForEach(server =>
                                       insert_query
                                           .Values(id_provider,
                                                   server.type,
                                                   server.hostname,
                                                   server.port,
                                                   server.socketType,
                                                   server.username,
                                                   server.authentication));

                        config.emailProvider.outgoingServer
                              .ForEach(server =>
                                       insert_query
                                           .Values(id_provider,
                                                   server.type,
                                                   server.hostname,
                                                   server.port,
                                                   server.socketType,
                                                   server.username,
                                                   server.authentication));

                        db.ExecuteNonQuery(insert_query);

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

        private static EncryptionType ConvertToEncryptionType(string type_string)
        {
            switch (type_string.ToLower().Trim())
            {
                case "ssl":
                    return EncryptionType.SSL;
                case "starttls":
                    return EncryptionType.StartTLS;
                case "plain":
                    return EncryptionType.None;
                default:
                    throw new ArgumentException("Unknown mail server socket type: " + type_string);
            }
        }

        private static string ConvertFromEncryptionType(EncryptionType enc_type)
        {
            switch (enc_type)
            {
                case EncryptionType.SSL:
                    return "SSL";
                case EncryptionType.StartTLS:
                    return "STARTTLS";
                case EncryptionType.None:
                    return "plain";
                default:
                    throw new ArgumentException("Unknown mail server EncryptionType: " + Enum.GetName(typeof(EncryptionType), enc_type));
            }
        }

        private static SaslMechanism ConvertToSaslMechanism(string type_string)
        {
            switch (type_string.ToLower().Trim())
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
                    throw new ArgumentException("Unknown mail server authentication type: " + type_string);
            }
        }

        private static string ConvertFromSaslMechanism(SaslMechanism sasl_type)
        {
            switch (sasl_type)
            {
                case SaslMechanism.Login:
                    return "";
                case SaslMechanism.None:
                    return "none";
                case SaslMechanism.CramMd5:
                    return "password-encrypted";
                default:
                    throw new ArgumentException("Unknown mail server SaslMechanism: " + Enum.GetName(typeof(SaslMechanism), sasl_type));
            }
        }

        public MailBox ObtainMailboxSettings(int id_tenant, string id_user, string email, string password,
            AuthorizationServiceType type, bool? imap, bool is_null_needed)
        {
            var address = new MailAddress(email);

            var host = address.Host.ToLowerInvariant();

            if (type == AuthorizationServiceType.Google) host = GoogleHost;

            MailBox initial_value = null;

            if (imap.HasValue)
            {
                try
                {
                    var settings = GetMailBoxSettings(host);

                    if (settings != null)
                    {
                        var outgoing_server_login = "";

                        var incomming_type = imap.Value ? "imap" : "pop3";

                        var incoming_server =
                            settings.emailProvider.incomingServer
                            .FirstOrDefault(serv =>
                                serv.type
                                .ToLowerInvariant()
                                .Equals(incomming_type));

                        var outgoing_server = settings.emailProvider.outgoingServer.FirstOrDefault() ?? new clientConfigEmailProviderOutgoingServer();

                        if (incoming_server != null && !string.IsNullOrEmpty(incoming_server.username))
                        {
                            var incoming_server_login = FormatLoginFromDb(incoming_server.username, address);

                            if (!string.IsNullOrEmpty(outgoing_server.username))
                            {
                                outgoing_server_login = FormatLoginFromDb(outgoing_server.username, address);
                            }

                            initial_value = new MailBox
                            {
                                EMail = address,
                                Name = "",

                                Account = incoming_server_login,
                                Password = password,
                                Server = FormatServerFromDb(incoming_server.hostname, host),
                                Port = incoming_server.port,
                                IncomingEncryptionType = ConvertToEncryptionType(incoming_server.socketType),
                                OutcomingEncryptionType = ConvertToEncryptionType(outgoing_server.socketType),
                                AuthenticationTypeIn = ConvertToSaslMechanism(incoming_server.authentication),
                                AuthenticationTypeSmtp = ConvertToSaslMechanism(outgoing_server.authentication),
                                Imap = imap.Value,

                                SmtpAccount = outgoing_server_login,
                                SmtpPassword = password,
                                SmtpServer = FormatServerFromDb(outgoing_server.hostname, host),
                                SmtpPort = outgoing_server.port,
                                SmtpAuth = !string.IsNullOrEmpty(outgoing_server.username),

                                Enabled = true,
                                TenantId = id_tenant,
                                UserId = id_user,
                                Restrict = true,
                                BeginDate = DateTime.Now.Subtract(new TimeSpan(MailBox.DefaultMailLimitedTimeDelta)),
                                ServiceType = (byte)type
                            };
                        }
                    }
                }
                catch (Exception)
                {
                    initial_value = null;
                }
            }

            if (initial_value != null || is_null_needed)
            {
                return initial_value;
            }

            bool is_imap = imap.GetValueOrDefault(true);
            return new MailBox
                {
                    EMail = address,
                    Name = "",
                    Account = email,
                    Password = password,
                    Server = string.Format((is_imap ? "imap.{0}" : "pop.{0}"), host),
                    Port = (is_imap ? 993 : 110),
                    IncomingEncryptionType = is_imap ? EncryptionType.SSL : EncryptionType.None,
                    OutcomingEncryptionType = EncryptionType.None,
                    Imap = is_imap,
                    SmtpAccount = email,
                    SmtpPassword = password,
                    SmtpServer = string.Format("smtp.{0}", host),
                    SmtpPort = 25,
                    SmtpAuth = true,
                    Enabled = true,
                    TenantId = id_tenant,
                    UserId = id_user,
                    Restrict = true,
                    BeginDate = DateTime.Now.Subtract(new TimeSpan(MailBox.DefaultMailLimitedTimeDelta)),
                    AuthenticationTypeIn = SaslMechanism.Login,
                    AuthenticationTypeSmtp = SaslMechanism.Login
                };
        }


        public MailBox SearchMailboxSettings(string email, string password, string id_user, int id_tenant)
        {
            //Mailbox splitted for excluding data race from cocurency.
            //This is out mailbox. In that we store smtp settings.
            var mbox = new MailBox
                {
                    SmtpServer = String.Format("smtp.{0}", email.Substring(email.IndexOf('@') + 1)),
                    SmtpAccount = email,
                    Account = email,
                    EMail = new MailAddress(email),
                    SmtpAuth = true,
                    SmtpPassword = password,
                    Password = password,
                    Name = ""
                };
            //This mailbox using by pop_imap_search_thread for result storing.
            var mbox_in = new MailBox();

            var settings_from_db = GetMailBoxSettings(email.Substring(email.IndexOf('@') + 1));

            if (settings_from_db == null)
                throw new ItemNotFoundException("Unknown mail provider settings.");


            bool is_smtp_failed = false;
            bool is_successed_in = false;
            string last_in_error_pop = String.Empty;
            string last_in_error_imap = String.Empty;
            var pop_imap_search_thread = new Thread(() =>
            {
                mbox_in.Imap = true;
                mbox_in.Server = String.Format("imap.{0}", email.Substring(email.IndexOf('@') + 1));
                foreach (var settings in GetImapSettingsVariants(email, password, mbox_in, settings_from_db))
                {
                    if (is_smtp_failed) return;
                    if (MailServerHelper.TryTestImap(settings, out last_in_error_pop))
                    {
                        mbox_in.Account = settings.AccountName;
                        mbox_in.Password = settings.AccountPass;
                        mbox_in.Server = settings.Url;
                        mbox_in.Port = settings.Port;
                        mbox_in.AuthenticationTypeIn = settings.AuthenticationType;
                        mbox_in.IncomingEncryptionType = settings.EncryptionType;
                        is_successed_in = true;
                        break;
                    }
                }

                if (!is_successed_in && !is_smtp_failed)
                {
                    mbox_in.Imap = false;
                    mbox_in.Server = String.Format("pop.{0}", email.Substring(email.IndexOf('@') + 1));
                    foreach (var settings in GetPopSettingsVariants(email, password, mbox_in, settings_from_db))
                    {
                        if (is_smtp_failed) return;
                        if (MailServerHelper.TryTestPop(settings, out last_in_error_imap))
                        {
                            mbox_in.Account = settings.AccountName;
                            mbox_in.Password = settings.AccountPass;
                            mbox_in.Server = settings.Url;
                            mbox_in.Port = settings.Port;
                            mbox_in.AuthenticationTypeIn = settings.AuthenticationType;
                            mbox_in.IncomingEncryptionType = settings.EncryptionType;
                            is_successed_in = true;
                            break;
                        }
                    }
                }
            });

            pop_imap_search_thread.Start();

            string last_error = String.Empty;
            bool is_successed = false;
            foreach (var settings in GetSmtpSettingsVariants(email, password, mbox, settings_from_db))
            {
                if (MailServerHelper.TryTestSmtp(settings, out last_error))
                {
                    mbox.SmtpPassword = settings.AccountPass;
                    mbox.SmtpAccount = settings.AccountName;
                    mbox.SmtpServer = settings.Url;
                    mbox.SmtpPort = settings.Port;
                    mbox.AuthenticationTypeSmtp = settings.AuthenticationType;
                    mbox.OutcomingEncryptionType = settings.EncryptionType;
                    is_successed = true;
                    break;
                }
            }

            if (!is_successed)
            {
                is_smtp_failed = true;
                Thread.Sleep(0);
                throw new SmtpConnectionException(last_error);
            }

            pop_imap_search_thread.Join(30000);

            if (!is_successed_in)
            {
                if (last_in_error_pop == String.Empty && last_in_error_imap == String.Empty)
                {
                    throw new ImapConnectionTimeoutException();
                }
                throw new ImapConnectionException(last_in_error_pop);
            }

            mbox.Imap = mbox_in.Imap;
            mbox.Account = mbox_in.Account;
            mbox.Password = mbox_in.Password;
            mbox.IncomingEncryptionType = mbox_in.IncomingEncryptionType;
            mbox.AuthenticationTypeIn = mbox_in.AuthenticationTypeIn;
            mbox.Server = mbox_in.Server;
            mbox.Port = mbox_in.Port;
            mbox.UserId = id_user;
            mbox.TenantId = id_tenant;
            mbox.Restrict = true;
            mbox.BeginDate = DateTime.Now.Subtract(new TimeSpan(MailBox.DefaultMailLimitedTimeDelta));

            return mbox;
        }

        public int SaveMailServerSettings(MailAddress email, MailServerSettings settings, string server_type,
            AuthorizationServiceType authorization_type)
        {
            var host = (authorization_type == AuthorizationServiceType.Google) ? GoogleHost : email.Host;

            using (var db = GetDb())
            {
                var provider_id = db.ExecuteScalar<int>(new SqlQuery(MailboxDomainTable.name)
                                                            .Select(MailboxDomainTable.Columns.id_provider)
                                                            .Where(MailboxProviderTable.Columns.name, host));

                //Save Mailbox provider if not exists
                if (provider_id == 0)
                {
                    provider_id = db.ExecuteScalar<int>(new SqlInsert(MailboxProviderTable.name)
                                                            .InColumnValue(MailboxProviderTable.Columns.id, 0)
                                                            .InColumnValue(MailboxProviderTable.Columns.name, email.Host)
                                                            .Identity(0, 0, true));
                    db.ExecuteNonQuery(new SqlInsert(MailboxDomainTable.name)
                                                            .InColumnValue(MailboxDomainTable.Columns.id_provider, provider_id)
                                                            .InColumnValue(MailboxDomainTable.Columns.name, email.Host));
                }

                //Identify mask for account name
                var account_name_mask = "";
                if (settings.AuthenticationType != SaslMechanism.None)
                {
                    account_name_mask = GetLoginFormatFrom(email, settings.AccountName);
                    if (String.IsNullOrEmpty(account_name_mask))
                    {
                        account_name_mask = settings.AccountName;
                    }
                }

                var settings_id = db.ExecuteScalar<int>(new SqlQuery(MailboxServerTable.name)
                    .Select(MailboxServerTable.Columns.id)
                    .Where(MailboxServerTable.Columns.id_provider, provider_id)
                    .Where(MailboxServerTable.Columns.type, server_type)
                    .Where(MailboxServerTable.Columns.hostname, settings.Url)
                    .Where(MailboxServerTable.Columns.port, settings.Port)
                    .Where(MailboxServerTable.Columns.socket_type,
                           ConvertFromEncryptionType(settings.EncryptionType))
                     .Where(MailboxServerTable.Columns.authentication,
                                    ConvertFromSaslMechanism(settings.AuthenticationType))
                     .Where(MailboxServerTable.Columns.username, account_name_mask)
                     .Where(MailboxServerTable.Columns.is_user_data, false));

                if (settings_id == 0)
                {
                    settings_id = db.ExecuteScalar<int>(new SqlInsert(MailboxServerTable.name)
                                           .InColumnValue(MailboxServerTable.Columns.id, 0)
                                           .InColumnValue(MailboxServerTable.Columns.id_provider, provider_id)
                                           .InColumnValue(MailboxServerTable.Columns.type, server_type)
                                           .InColumnValue(MailboxServerTable.Columns.hostname, settings.Url)
                                           .InColumnValue(MailboxServerTable.Columns.port, settings.Port)
                                           .InColumnValue(MailboxServerTable.Columns.socket_type,
                                                          ConvertFromEncryptionType(settings.EncryptionType))
                                           .InColumnValue(MailboxServerTable.Columns.authentication,
                                                          ConvertFromSaslMechanism(settings.AuthenticationType))
                                           .InColumnValue(MailboxServerTable.Columns.username, account_name_mask)
                                           .InColumnValue(MailboxServerTable.Columns.is_user_data, true)
                                           .Identity(0, 0, true));
                }

                return settings_id;
            }
        }

        public SignatureDto GetMailboxSignature(int mailbox_id, string user_id, int tenant)
        {
            using (var db = GetDb())
            {
                CheckMailboxOwnage(mailbox_id, user_id, tenant, db);

                var signature_dal = new SignatureDal(db);
                return signature_dal.GetSignature(mailbox_id, tenant);
            }
        }

        public SignatureDto UpdateOrCreateMailboxSignature(int mailbox_id, string user_id, int tenant, string html, bool is_active)
        {
            using (var db = GetDb())
            {
                CheckMailboxOwnage(mailbox_id, user_id, tenant, db);

                var signature = new SignatureDto(mailbox_id, tenant, html, is_active);
                var signature_dal = new SignatureDal(db);
                signature_dal.UpdateOrCreateSignature(signature);
                return signature;
            }
        }

        private List<SignatureDto> GetMailboxesSignatures(List<int> mailbox_ids, int tenant, DbManager db)
        {
            var signature_dal = new SignatureDal(db);
            return signature_dal.GetSignatures(mailbox_ids, tenant);
        }

        public bool SetMailboxEmailInFolder(int tenant, string id_user, int mailbox_id, string email_in_folder)
        {
            using (var db = GetDb())
            {
                var query = new SqlUpdate(MailboxTable.name)
                    .Set(MailboxTable.Columns.email_in_folder, "" != email_in_folder ? email_in_folder : null)
                    .Where(GetUserWhere(id_user, tenant))
                    .Where(MailboxTable.Columns.id, mailbox_id);

                return 0 < db.ExecuteNonQuery(query);
            }
        }
        #endregion

        #region private methods

        private List<MailBox> GetMailBoxes(Exp where)
        {
            using (var db = GetDb())
            {
                return GetMailBoxes(where, db);
            }
        }

        private List<MailBox> GetMailBoxes(Exp where, DbManager db)
        {
            var query = GetSelectMailBoxFieldsQuery()
                .Where(MailboxTable.Columns.is_removed.Prefix(mail_mailbox_alias), false)
                .Where(where)
                .OrderBy(1, true)
                .OrderBy(2, true);

            var res = db.ExecuteList(query)
                        .ConvertAll(r => ToMailBox(r));

            return res;
        }

        private enum MailBoxFieldSelectPosition
        {
            IdTenant, IdUser, Name, Address, Account, Password, InServer, InPort, SizeLast, MsgCountLast, SmtpServer, SmtpPort,
            SmtpPassword, SmtpAccount, LoginDelay, Id, Enabled, QuotaError, AuthError, Imap, BeginDate,
            ServiceType, RefreshToken, ImapFolders, OutcomingEncryptionType, IncomingEncryptionType,
            AuthTypeIn, AuthtTypeSmtp, IdSmtpServer, IdInServer, EMailInFolder, IsTeamlabMailbox
        };

        private enum MailItemAttachmentSelectPosition
        {
            Id, Name, StoredName, Type, Size, FileNumber, IdStream, Tenant, User, ContentId
        }

        private enum MailAccountFieldSelectPosition
        {
            Id, Address, Enabled, Name, QoutaError, AuthError, RefreshToken, IsTeamlabMailbox, EmailInFolder,
            AliasId, AliasName, IsAlias, DomainId, DomainName, GroupId, GroupAddress
        }

        private const int SELECT_MAIL_BOX_FIELDS_COUNT = 32;
        private const int SELECT_MAIL_ITEM_ATTACHMENT_FIELDS_COUNT = 10;
        private const int SELECT_ACCOUNT_FIELDS_COUNT = 16;

        // ReSharper disable InconsistentNaming
        private const string mail_mailbox_alias = "mm";
        private const string smtp_alias = "smtp";
        private const string in_alias = "ins";
        // ReSharper restore InconsistentNaming

        private static SqlQuery GetSelectMailBoxFieldsQuery()
        {
            var fields_for_select = new string[SELECT_MAIL_BOX_FIELDS_COUNT];
            fields_for_select[(int)MailBoxFieldSelectPosition.IdTenant] = MailboxTable.Columns.id_tenant.Prefix(mail_mailbox_alias);
            fields_for_select[(int)MailBoxFieldSelectPosition.IdUser] = MailboxTable.Columns.id_user.Prefix(mail_mailbox_alias);
            fields_for_select[(int)MailBoxFieldSelectPosition.Name] = MailboxTable.Columns.name.Prefix(mail_mailbox_alias);
            fields_for_select[(int)MailBoxFieldSelectPosition.Address] = MailboxTable.Columns.address.Prefix(mail_mailbox_alias);
            fields_for_select[(int)MailBoxFieldSelectPosition.Account] = MailboxServerTable.Columns.username.Prefix(in_alias);
            fields_for_select[(int)MailBoxFieldSelectPosition.Password] = MailboxTable.Columns.password.Prefix(mail_mailbox_alias);
            fields_for_select[(int)MailBoxFieldSelectPosition.InServer] = MailboxServerTable.Columns.hostname.Prefix(in_alias);
            fields_for_select[(int)MailBoxFieldSelectPosition.InPort] = MailboxServerTable.Columns.port.Prefix(in_alias);
            fields_for_select[(int)MailBoxFieldSelectPosition.SizeLast] = MailboxTable.Columns.size_last.Prefix(mail_mailbox_alias);
            fields_for_select[(int)MailBoxFieldSelectPosition.MsgCountLast] = MailboxTable.Columns.msg_count_last.Prefix(mail_mailbox_alias);
            fields_for_select[(int)MailBoxFieldSelectPosition.SmtpServer] = MailboxServerTable.Columns.hostname.Prefix(smtp_alias);
            fields_for_select[(int)MailBoxFieldSelectPosition.SmtpPort] = MailboxServerTable.Columns.port.Prefix(smtp_alias);
            fields_for_select[(int)MailBoxFieldSelectPosition.SmtpPassword] = MailboxTable.Columns.smtp_password.Prefix(mail_mailbox_alias);
            fields_for_select[(int)MailBoxFieldSelectPosition.SmtpAccount] = MailboxServerTable.Columns.username.Prefix(smtp_alias);
            fields_for_select[(int)MailBoxFieldSelectPosition.LoginDelay] = MailboxTable.Columns.login_delay.Prefix(mail_mailbox_alias);
            fields_for_select[(int)MailBoxFieldSelectPosition.Id] = MailboxTable.Columns.id.Prefix(mail_mailbox_alias);
            fields_for_select[(int)MailBoxFieldSelectPosition.Enabled] = MailboxTable.Columns.enabled.Prefix(mail_mailbox_alias);
            fields_for_select[(int)MailBoxFieldSelectPosition.QuotaError] = MailboxTable.Columns.quota_error.Prefix(mail_mailbox_alias);
            fields_for_select[(int)MailBoxFieldSelectPosition.AuthError] = MailboxTable.Columns.auth_error.Prefix(mail_mailbox_alias);
            fields_for_select[(int)MailBoxFieldSelectPosition.Imap] = MailboxTable.Columns.imap.Prefix(mail_mailbox_alias);
            fields_for_select[(int)MailBoxFieldSelectPosition.BeginDate] = MailboxTable.Columns.begin_date.Prefix(mail_mailbox_alias);
            fields_for_select[(int)MailBoxFieldSelectPosition.ServiceType] = MailboxTable.Columns.service_type.Prefix(mail_mailbox_alias);
            fields_for_select[(int)MailBoxFieldSelectPosition.RefreshToken] = MailboxTable.Columns.refresh_token.Prefix(mail_mailbox_alias);
            fields_for_select[(int)MailBoxFieldSelectPosition.ImapFolders] = MailboxTable.Columns.imap_folders.Prefix(mail_mailbox_alias);
            fields_for_select[(int)MailBoxFieldSelectPosition.EMailInFolder] = MailboxTable.Columns.email_in_folder.Prefix(mail_mailbox_alias);
            fields_for_select[(int)MailBoxFieldSelectPosition.OutcomingEncryptionType] = MailboxServerTable.Columns.socket_type.Prefix(smtp_alias);
            fields_for_select[(int)MailBoxFieldSelectPosition.IncomingEncryptionType] = MailboxServerTable.Columns.socket_type.Prefix(in_alias);
            fields_for_select[(int)MailBoxFieldSelectPosition.AuthTypeIn] = MailboxServerTable.Columns.authentication.Prefix(in_alias);
            fields_for_select[(int)MailBoxFieldSelectPosition.AuthtTypeSmtp] = MailboxServerTable.Columns.authentication.Prefix(smtp_alias);
            fields_for_select[(int)MailBoxFieldSelectPosition.IdSmtpServer] = MailboxServerTable.Columns.id.Prefix(smtp_alias);
            fields_for_select[(int)MailBoxFieldSelectPosition.IdInServer] = MailboxServerTable.Columns.id.Prefix(in_alias);
            fields_for_select[(int)MailBoxFieldSelectPosition.IsTeamlabMailbox] = MailboxTable.Columns.is_teamlab_mailbox.Prefix(mail_mailbox_alias);

            return new SqlQuery(MailboxTable.name + " " + mail_mailbox_alias)
                .InnerJoin(MailboxServerTable.name + " " + smtp_alias,
                            Exp.EqColumns(MailboxTable.Columns.id_smtp_server.Prefix(mail_mailbox_alias), MailboxServerTable.Columns.id.Prefix(smtp_alias)))
                .InnerJoin(MailboxServerTable.name + " " + in_alias,
                            Exp.EqColumns(MailboxTable.Columns.id_in_server.Prefix(mail_mailbox_alias), MailboxServerTable.Columns.id.Prefix(in_alias)))
                .Select(fields_for_select);
        }

        private MailBox ToMailBox(object[] r)
        {
            if (r.Length != SELECT_MAIL_BOX_FIELDS_COUNT)
            {
                Console.WriteLine("Count of returned fields not equal to");
                var results = r;
                foreach (var field in results)
                {
                    Console.WriteLine(field == null ? "null" : field.ToString());
                }
                return null;
            }

            var in_mail_address = new MailAddress((string)r[(int)MailBoxFieldSelectPosition.Address]);
            var in_account = FormatLoginFromDb((string)r[(int)MailBoxFieldSelectPosition.Account], in_mail_address);
            var smtp_account = FormatLoginFromDb((string)r[(int)MailBoxFieldSelectPosition.SmtpAccount], in_mail_address);
            var in_server_old_format = (string)r[(int)MailBoxFieldSelectPosition.InServer] + ":" + r[(int)MailBoxFieldSelectPosition.InPort];
            var smtp_server_old_format = (string)r[(int)MailBoxFieldSelectPosition.SmtpServer] + ":" + r[(int)MailBoxFieldSelectPosition.SmtpPort];
            var in_encryption = ConvertToEncryptionType((string)r[(int)MailBoxFieldSelectPosition.IncomingEncryptionType]);
            var smtp_encryption = ConvertToEncryptionType((string)r[(int)MailBoxFieldSelectPosition.OutcomingEncryptionType]);
            var in_auth = ConvertToSaslMechanism((string)r[(int)MailBoxFieldSelectPosition.AuthTypeIn]);
            var smtp_auth = ConvertToSaslMechanism((string)r[(int)MailBoxFieldSelectPosition.AuthtTypeSmtp]);
            var auth_error_ticks = r[(int)MailBoxFieldSelectPosition.AuthError] != null
                                       ? Convert.ToInt64(r[(int)MailBoxFieldSelectPosition.AuthError])
                                       : 0;

            var auth_error_type = MailBox.AuthProblemType.NoProblems;

            if (auth_error_ticks > 0)
            {
                if (DateTime.UtcNow.Ticks - auth_error_ticks > AuthErrorDisableTimeout.Ticks)
                    auth_error_type = MailBox.AuthProblemType.TooManyErrors;
                else if (DateTime.UtcNow.Ticks - auth_error_ticks > AuthErrorWarningTimeout.Ticks)
                    auth_error_type = MailBox.AuthProblemType.ConnectError;
            }

            var res = new MailBox(
                Convert.ToInt32(r[(int)MailBoxFieldSelectPosition.IdTenant]),
                (string)r[(int)MailBoxFieldSelectPosition.IdUser],
                (string)r[(int)MailBoxFieldSelectPosition.Name],
                in_mail_address,
                in_account,
                DecryptPassword((string)r[(int)MailBoxFieldSelectPosition.Password]),
                in_server_old_format,
                Convert.ToBoolean(r[(int)MailBoxFieldSelectPosition.Imap]),
                smtp_server_old_format,
                (string)r[(int)MailBoxFieldSelectPosition.SmtpPassword] == null ? null : DecryptPassword((string)r[(int)MailBoxFieldSelectPosition.SmtpPassword]),
                in_auth != SaslMechanism.None,
                Convert.ToInt32(r[(int)MailBoxFieldSelectPosition.Id]),
                (DateTime)r[(int)MailBoxFieldSelectPosition.BeginDate],
                in_encryption,
                smtp_encryption,
                Convert.ToByte(r[(int)MailBoxFieldSelectPosition.ServiceType]),
                (string)r[(int)MailBoxFieldSelectPosition.RefreshToken],
                (string)r[(int)MailBoxFieldSelectPosition.EMailInFolder]
                )
            {
                Size = Convert.ToInt64(r[(int)MailBoxFieldSelectPosition.SizeLast]),
                MessagesCount = Convert.ToInt32(r[(int)MailBoxFieldSelectPosition.MsgCountLast]),
                SmtpAccount = smtp_account,
                ServerLoginDelay = Convert.ToInt32(r[(int)MailBoxFieldSelectPosition.LoginDelay]),
                Enabled = Convert.ToBoolean(r[(int)MailBoxFieldSelectPosition.Enabled]),
                IsTeamlab = Convert.ToBoolean(r[(int)MailBoxFieldSelectPosition.IsTeamlabMailbox]),
                QuotaError = Convert.ToBoolean(r[(int)MailBoxFieldSelectPosition.QuotaError]),
                AuthError = auth_error_type,
                ImapFoldersJson = (string)r[(int)MailBoxFieldSelectPosition.ImapFolders],
                AuthenticationTypeIn = in_auth,
                AuthenticationTypeSmtp = smtp_auth,
                SmtpServerId = (int)r[(int)MailBoxFieldSelectPosition.IdSmtpServer],
                InServerId = (int)r[(int)MailBoxFieldSelectPosition.IdInServer]
            };

            return res;
        }

        private List<AccountInfo> ToAccountInfo(IEnumerable<object[]> object_list, List<SignatureDto> signatures)
        {
            var accounts = new List<AccountInfo>();

            foreach (var r in object_list)
            {
                if (r.Length != SELECT_ACCOUNT_FIELDS_COUNT)
                {
                    Console.WriteLine("Count of returned fields not equal to");
                    var results = r;
                    foreach (var field in results)
                    {
                        Console.WriteLine(field == null ? "null" : field.ToString());
                    }
                    return null;
                }

                var mailbox_id = Convert.ToInt32(r[(int)MailAccountFieldSelectPosition.Id]);
                var account_index = accounts.FindIndex(a => a.Id == mailbox_id);

                var signature = signatures.First(s => s.MailboxId == mailbox_id);
                var is_alias = Convert.ToBoolean(r[(int)MailAccountFieldSelectPosition.IsAlias]);

                if (!is_alias)
                {
                    var group_address = (string)(r[(int)MailAccountFieldSelectPosition.GroupAddress]);
                    MailAddressInfo group = null;

                    if (!string.IsNullOrEmpty(group_address))
                    {
                        group = new MailAddressInfo(
                            Convert.ToInt32(r[(int)MailAccountFieldSelectPosition.GroupId]),
                            group_address,
                            Convert.ToInt32(r[(int)MailAccountFieldSelectPosition.DomainId]));
                    }

                    if (account_index == -1)
                    {
                        var auth_error_ticks = r[(int)MailAccountFieldSelectPosition.AuthError] != null
                                                   ? Convert.ToInt64(r[(int)MailAccountFieldSelectPosition.AuthError])
                                                   : 0;

                        var auth_error_type = MailBox.AuthProblemType.NoProblems;

                        if (auth_error_ticks > 0)
                        {
                            if (DateTime.UtcNow.Ticks - auth_error_ticks > AuthErrorDisableTimeout.Ticks)
                                auth_error_type = MailBox.AuthProblemType.TooManyErrors;
                            else if (DateTime.UtcNow.Ticks - auth_error_ticks > AuthErrorWarningTimeout.Ticks)
                                auth_error_type = MailBox.AuthProblemType.ConnectError;
                        }

                        var account = new AccountInfo(
                            mailbox_id,
                            (string)(r[(int)MailAccountFieldSelectPosition.Address]),
                            (string)(r[(int)MailAccountFieldSelectPosition.Name]),
                            Convert.ToBoolean(r[(int)MailAccountFieldSelectPosition.Enabled]),
                            Convert.ToBoolean(r[(int)MailAccountFieldSelectPosition.QoutaError]),
                            auth_error_type, signature,
                            !string.IsNullOrEmpty((string)(r[(int)MailAccountFieldSelectPosition.RefreshToken])),
                            (string)(r[(int)MailAccountFieldSelectPosition.EmailInFolder]),
                            Convert.ToBoolean(r[(int)MailAccountFieldSelectPosition.IsTeamlabMailbox]));

                        if (group != null) account.Groups.Add(group);

                        accounts.Add(account);
                    }
                    else if (group != null)
                    {
                        accounts[account_index].Groups.Add(group);
                    }
                }
                else
                {
                    var alias = new MailAddressInfo(
                            Convert.ToInt32(r[(int)MailAccountFieldSelectPosition.AliasId]),
                            (string)(r[(int)MailAccountFieldSelectPosition.AliasName]) + '@' +
                            (string)(r[(int)MailAccountFieldSelectPosition.DomainName]),
                            Convert.ToInt32(r[(int)MailAccountFieldSelectPosition.DomainId]));

                    accounts[account_index].Aliases.Add(alias);
                }
            }
            return accounts;
        }

        private MailAttachment ToMailItemAttachment(object[] r)
        {
            if (r.Length != SELECT_MAIL_ITEM_ATTACHMENT_FIELDS_COUNT)
            {
                Console.WriteLine("Count of returned fields not equal to");
                var results = r;
                foreach (var field in results)
                {
                    Console.WriteLine(field == null ? "null" : field.ToString());
                }
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
                    contentId = Convert.ToString(r[(int)MailItemAttachmentSelectPosition.ContentId])
                };

            // if StoredName is empty then attachment had been stored by filename (old attachment);
            attachment.storedName = string.IsNullOrEmpty(attachment.storedName) ? attachment.fileName : attachment.storedName;

            return attachment;
        }

        private void GetMailBoxState(int id, out bool is_removed, out bool is_deactivated, out DateTime begin_date)
        {
            is_removed = true;
            is_deactivated = true;
            begin_date = MIN_BEGIN_DATE;

            using (var db = GetDb())
            {
                var res = db.ExecuteList(new SqlQuery(MailboxTable.name)
                        .Select(MailboxTable.Columns.is_removed, MailboxTable.Columns.enabled, MailboxTable.Columns.begin_date)
                        .Where(Exp.Eq(MailboxTable.Columns.id, id)))
                        .FirstOrDefault();

                if (res == null) return;
                is_removed = Convert.ToBoolean(res[0]);
                is_deactivated = !Convert.ToBoolean(res[1]);
                begin_date = Convert.ToDateTime(res[2]);
            }
        }

        // Return id_mailbox > 0 if address exists in mail_mailbox table.
        private int MailBoxExists(string address, string id_user, int tenant)
        {
            using (var db = GetDb())
            {
                return db.ExecuteScalar<int>(
                    new SqlQuery(MailboxTable.name)
                        .Select(MailboxTable.Columns.id)
                        .Where(GetUserWhere(id_user, tenant))
                        .Where(MailboxTable.Columns.address, address)
                        .Where(MailboxTable.Columns.is_removed, false));
            }
        }

        private static List<MailServerSettings> GetPopSettingsVariants(string email, string password, MailBox mbox, clientConfig config)
        {
            var temp_list = new List<MailServerSettings>();
            if (config != null && config.emailProvider.incomingServer != null)
            {
                var address = new MailAddress(email);
                foreach (var pop_server in config.emailProvider.incomingServer.Where(x => x.type == "pop3"))
                {
                    if (pop_server.hostname == null) continue;
                    temp_list.Add(new MailServerSettings
                    {
                        Url = FormatServerFromDb(pop_server.hostname, address.Host.ToLowerInvariant()),
                        Port = pop_server.port,
                        AccountName = FormatLoginFromDb(pop_server.username, address),
                        AccountPass = password,
                        AuthenticationType = ConvertToSaslMechanism(pop_server.authentication),
                        EncryptionType = ConvertToEncryptionType(pop_server.socketType)
                    });
                }
            }

            return temp_list;
        }

        private static List<MailServerSettings> GetImapSettingsVariants(string email, string password, MailBox mbox, clientConfig config)
        {
            var temp_list = new List<MailServerSettings>();
            if (config != null && config.emailProvider.incomingServer != null)
            {
                var address = new MailAddress(email);
                foreach (var imap_server in config.emailProvider.incomingServer.Where(x => x.type == "imap"))
                {
                    if (imap_server.hostname == null) continue;
                    temp_list.Add(new MailServerSettings
                    {
                        Url = FormatServerFromDb(imap_server.hostname, address.Host.ToLowerInvariant()),
                        Port = imap_server.port,
                        AccountName = FormatLoginFromDb(imap_server.username, address),
                        AccountPass = password,
                        AuthenticationType = ConvertToSaslMechanism(imap_server.authentication),
                        EncryptionType = ConvertToEncryptionType(imap_server.socketType)
                    });
                }
            }

            return temp_list;
        }

        private static List<MailServerSettings> GetSmtpSettingsVariants(string email, string password, MailBox mbox, clientConfig config)
        {

            var temp_list = new List<MailServerSettings>();
            if (config != null && config.emailProvider.outgoingServer != null)
            {
                var address = new MailAddress(email);
                foreach (var mail_server_settingse in config.emailProvider.outgoingServer)
                {

                    temp_list.Add(new MailServerSettings
                        {
                            Url = FormatServerFromDb(mail_server_settingse.hostname, address.Host.ToLowerInvariant()),
                            Port = mail_server_settingse.port,
                            AccountName = FormatLoginFromDb(mail_server_settingse.username, address),
                            AccountPass = password,
                            AuthenticationType = ConvertToSaslMechanism(mail_server_settingse.authentication),
                            EncryptionType = ConvertToEncryptionType(mail_server_settingse.socketType)
                        });
                }
            }

            return temp_list;
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
            var address_lower = address.Address.ToLower();
            var username_lower = username.ToLower();
            var mailparts = address_lower.Split('@');

            var localpart = mailparts[0];
            var domain = mailparts[1];
            var host_name_variant_1 = Path.GetFileNameWithoutExtension(domain);
            var host_name_variant_2 = domain.Split('.')[0];

            var result_format = username_lower.Replace(address_lower, "%EMAILADDRESS%");
            int pos = result_format.IndexOf(localpart, StringComparison.InvariantCulture);
            if (pos >= 0)
            {
                result_format = result_format.Substring(0, pos) + "%EMAILLOCALPART%" + result_format.Substring(pos + localpart.Length);
            }
            result_format = result_format.Replace(domain, "%EMAILDOMAIN%");
            if (host_name_variant_1 != null)
                result_format = result_format.Replace(host_name_variant_1, "%EMAILHOSTNAME%");
            result_format = result_format.Replace(host_name_variant_2, "%EMAILHOSTNAME%");

            return result_format == username_lower ? "" : result_format;
        }

        private static string FormatServerFromDb(string format, string host)
        {
            return format.Replace("%EMAILDOMAIN%", host);
        }

        private const int HardcodedLoginTimeForMsMail = 900;
        private static int GetLoginDelayTime(MailBox mail_box)
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
            if (mail_box.Server == "pop3.live.com")
                return HardcodedLoginTimeForMsMail;

            return mail_box.ServerLoginDelay < MailBox.DefaultServerLoginDelay
                       ? MailBox.DefaultServerLoginDelay
                       : mail_box.ServerLoginDelay;
        }

        private static void CheckMailboxOwnage(int mailbox_id, string user_id, int tenant, DbManager db)
        {
            CheckMailboxesOwnage(new List<int> { mailbox_id }, user_id, tenant, db);
        }

        private static void CheckMailboxesOwnage(List<int> mailbox_ids, string user_id, int tenant, DbManager db)
        {
            var check_mailbox_ownage = new SqlQuery(MailboxTable.name)
                .Select(MailboxTable.Columns.id)
                .Where(MailboxTable.Columns.id_user, user_id)
                .Where(MailboxTable.Columns.id_tenant, tenant)
                .Where(mailbox_ids.Count > 1
                           ? Exp.In(MailboxTable.Columns.id, mailbox_ids)
                           : Exp.Eq(MailboxTable.Columns.id, mailbox_ids[0]));

            var found_ids = db.ExecuteList(check_mailbox_ownage)
                .ConvertAll(res => Convert.ToInt32(res[0]));

            if (!mailbox_ids.Any(id_mailbox => found_ids.Exists(found_id => found_id == id_mailbox)))
                throw new AccessViolationException("Mailbox doesn't owned by user.");
        }

        #endregion
    }
}