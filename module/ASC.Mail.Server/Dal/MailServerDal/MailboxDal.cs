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
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Common.Security.Authentication;
using ASC.Mail.Aggregator.Dal.DbSchema;
using ASC.Mail.Server.Utils;
using ASC.Security.Cryptography;

namespace ASC.Mail.Server.Dal
{
    public class MailboxDal : DalBase
    {
        private readonly int _mailboxCountLimit = 2;
        
        public MailboxDal(int tenant_id) : 
            this("mailserver", tenant_id)
        {
        }

        public MailboxDal(int tenant_id, int mailboxes_count_limit) :
            this("mailserver", tenant_id, mailboxes_count_limit)
        {
        }

        public MailboxDal(string db_connection_string_name, int tenant_id) 
            : this(db_connection_string_name, tenant_id, 2)
        {
        }

        public MailboxDal(string db_connection_string_name, int tenant_id, int mailboxes_count_limit)
            : base(db_connection_string_name, tenant_id)
        {
            _mailboxCountLimit = mailboxes_count_limit;
        }

        public MailboxWithAddressDto CreateMailbox(IAccount teamlab_account,
                                                   string full_address, string password, string address_name,
                                                   DateTime address_created_date,
                                                   int domain_id, string domain_name, bool is_verified, DbManager db)
        {
            if (teamlab_account == null)
                throw new ArgumentNullException("teamlab_account");

            if(string.IsNullOrEmpty(full_address))
                throw new ArgumentNullException("full_address");

            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException("password");

            if (string.IsNullOrEmpty(address_name))
                throw new ArgumentNullException("address_name");

            if (domain_id < 0)
                throw new ArgumentException("Argument domain_id less then zero.", "domain_id");

            if (string.IsNullOrEmpty(domain_name))
                throw new ArgumentNullException("domain_name");

            if (db == null)
                throw new ArgumentNullException("db");

            const string mailbox_alias = "mb";
            const string address_alias = "a";

            var user_teamlab_mailbox_count_query = new SqlQuery(MailboxTable.name.Alias(mailbox_alias))
                .InnerJoin(AddressTable.name.Alias(address_alias),
                           Exp.EqColumns(MailboxTable.Columns.id.Prefix(mailbox_alias),
                                         AddressTable.Columns.id_mailbox.Prefix(address_alias)))
                .SelectCount()
                .Where(MailboxTable.Columns.is_teamlab_mailbox.Prefix(mailbox_alias), true)
                .Where(MailboxTable.Columns.is_removed.Prefix(mailbox_alias), false)
                .Where(MailboxTable.Columns.id_tenant.Prefix(mailbox_alias), tenant_id)
                .Where(MailboxTable.Columns.id_user.Prefix(mailbox_alias), teamlab_account.ID.ToString())
                .Where(AddressTable.Columns.id_domain.Prefix(address_alias), domain_id)
                .Where(AddressTable.Columns.is_alias.Prefix(address_alias), false);

            var user_mailboxes_count = db.ExecuteScalar<int>(user_teamlab_mailbox_count_query);

            if (user_mailboxes_count >= _mailboxCountLimit)
                throw new ArgumentOutOfRangeException("address_name", string.Format("Count of user's mailboxes must be less or equal {0}.", _mailboxCountLimit));

            var server_information = GetTenantServer(db);

            var date_created = DateTime.UtcNow.ToDbStyle();

            var insert_query = new SqlInsert(MailboxTable.name)
                .InColumnValue(MailboxTable.Columns.id, 0)
                .InColumnValue(MailboxTable.Columns.id_tenant, tenant_id)
                .InColumnValue(MailboxTable.Columns.id_user, teamlab_account.ID.ToString())
                .InColumnValue(MailboxTable.Columns.address, full_address) //Todo: Talk with AK about this.
                .InColumnValue(MailboxTable.Columns.name, teamlab_account.Name)
                .InColumnValue(MailboxTable.Columns.password, InstanceCrypto.Encrypt(password))
                .InColumnValue(MailboxTable.Columns.msg_count_last, 0)
                .InColumnValue(MailboxTable.Columns.smtp_password, InstanceCrypto.Encrypt(password))
                .InColumnValue(MailboxTable.Columns.size_last, 0)
                .InColumnValue(MailboxTable.Columns.login_delay, Config.LoginDelayInSeconds)
                .InColumnValue(MailboxTable.Columns.enabled, true)
                .InColumnValue(MailboxTable.Columns.imap, true)
                .InColumnValue(MailboxTable.Columns.service_type, 0)
                .InColumnValue(MailboxTable.Columns.refresh_token, null)
                .InColumnValue(MailboxTable.Columns.date_created, date_created)
                .InColumnValue(MailboxTable.Columns.id_smtp_server, server_information.smtp_settings_id)
                .InColumnValue(MailboxTable.Columns.id_in_server, server_information.imap_settings_id)
                .InColumnValue(MailboxTable.Columns.is_teamlab_mailbox, true)
                .Identity(0, 0, true);


            var result = db.ExecuteScalar<int>(insert_query);

            var created_mailbox = new MailboxDto(result, teamlab_account.ID.ToString(), tenant_id,
                                                 full_address);

            var address_dal = new MailAddressDal(tenant_id);

            var mailbox_address = address_dal.AddMailboxAddress(created_mailbox.id, address_name, address_created_date,
                                                    domain_id, domain_name, is_verified, db);

            var result_dto = new MailboxWithAddressDto(created_mailbox, mailbox_address);

            return result_dto;
        }

        public List<MailboxWithAddressDto> GetMailboxes(DbManager db = null)
        {
            const string mailbox_alias = "mailbox";
            const string address_alias = "address";
            const string domain_alias = "domain";

            var query = GetMailboxQuery(mailbox_alias, domain_alias, address_alias)
                .Where(MailboxTable.Columns.id_tenant.Prefix(mailbox_alias), tenant_id)
                .Where(MailboxTable.Columns.is_teamlab_mailbox.Prefix(mailbox_alias), true)
                .Where(MailboxTable.Columns.is_removed.Prefix(mailbox_alias), false);

            var result = NullSafeExecuteList(db, query);

            var mailbox_with_address_dto_list =
                result.ConvertAll(r => r.ToMailboxWithAddressDto());

            return mailbox_with_address_dto_list;
        }

        public MailboxWithAddressDto GetMailbox(int mailbox_id, DbManager db = null)
        {
            if (mailbox_id < 0)
                throw new ArgumentException("Argument mailbox_id less then zero.", "mailbox_id");

            const string mailbox_alias = "mailbox";
            const string address_alias = "address";
            const string domain_alias = "domain";

            var query = GetMailboxQuery(mailbox_alias, domain_alias, address_alias)
                .Where(MailboxTable.Columns.id_tenant.Prefix(mailbox_alias), tenant_id)
                .Where(MailboxTable.Columns.is_teamlab_mailbox.Prefix(mailbox_alias), true)
                .Where(MailboxTable.Columns.is_removed.Prefix(mailbox_alias), false)
                .Where(MailboxTable.Columns.id.Prefix(mailbox_alias), mailbox_id);

            var result = NullSafeExecuteList(db, query);

            var mailbox_with_address_dto =
                result.ConvertAll(r => r.ToMailboxWithAddressDto()).FirstOrDefault();

            return mailbox_with_address_dto;
        }

        private SqlQuery GetMailboxQuery(string mailbox_alias, string domain_alias, string address_alias)
        {
            if (string.IsNullOrEmpty(mailbox_alias))
                throw new ArgumentNullException("mailbox_alias");

            if (string.IsNullOrEmpty(domain_alias))
                throw new ArgumentNullException("domain_alias");

            if (string.IsNullOrEmpty(address_alias))
                throw new ArgumentNullException("address_alias");
            
            return new SqlQuery(MailboxTable.name.Alias(mailbox_alias))
                .InnerJoin(AddressTable.name.Alias(address_alias),
                           Exp.EqColumns(MailboxTable.Columns.id.Prefix(mailbox_alias),
                                         AddressTable.Columns.id_mailbox.Prefix(address_alias)))
                .InnerJoin(DomainTable.name.Alias(domain_alias),
                           Exp.EqColumns(AddressTable.Columns.id_domain.Prefix(address_alias),
                                         DomainTable.Columns.id.Prefix(domain_alias))
                )
                .Select(MailboxTable.Columns.id.Prefix(mailbox_alias))
                .Select(MailboxTable.Columns.id_user.Prefix(mailbox_alias))
                .Select(MailboxTable.Columns.id_tenant.Prefix(mailbox_alias))
                .Select(MailboxTable.Columns.address.Prefix(mailbox_alias))
                .Select(MailboxTable.Columns.date_created.Prefix(mailbox_alias))
                .Select(AddressTable.Columns.id.Prefix(address_alias))
                .Select(AddressTable.Columns.tenant.Prefix(address_alias))
                .Select(AddressTable.Columns.name.Prefix(address_alias))
                .Select(AddressTable.Columns.id_domain.Prefix(address_alias))
                .Select(AddressTable.Columns.id_mailbox.Prefix(address_alias))
                .Select(AddressTable.Columns.is_mail_group.Prefix(address_alias))
                .Select(AddressTable.Columns.is_alias.Prefix(address_alias))
                .Select(AddressTable.Columns.date_created.Prefix(address_alias))
                .Select(DomainTable.Columns.id.Prefix(domain_alias))
                .Select(DomainTable.Columns.name.Prefix(domain_alias))
                .Select(DomainTable.Columns.tenant.Prefix(domain_alias))
                .Select(DomainTable.Columns.date_added.Prefix(domain_alias))
                .Select(DomainTable.Columns.is_verified.Prefix(domain_alias));
        }

        public void DeleteMailbox(int mailbox_id, DbManager db)
        {
            if (mailbox_id < 0)
                throw new ArgumentException("Argument mailbox_id less then zero.", "mailbox_id");
            
            if (db == null)
                throw new ArgumentNullException("db");

            var mailbox_manager = GetMailboxManager();
            var mailbox = mailbox_manager.GetServerMailBox(tenant_id, mailbox_id, db);
            if (mailbox != null)
            {
                if (!mailbox.IsTeamlab)
                    throw new ArgumentException("Mailbox is not teamlab and it can't be deleted.");

                var quota_size_for_clean = mailbox_manager.RemoveMailBox(mailbox, db);

                //TODO: Maybe cleaning of quota need to move into RemoveMailBox?
                mailbox_manager.QuotaUsedDelete(mailbox.TenantId, quota_size_for_clean);
            }

            var delete_addresses_query = new SqlDelete(AddressTable.name)
                .Where(AddressTable.Columns.id_mailbox, mailbox_id);

            db.ExecuteNonQuery(delete_addresses_query);
        }
    }
}
