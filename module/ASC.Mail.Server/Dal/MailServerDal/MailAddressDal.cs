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
using ASC.Mail.Aggregator.Dal.DbSchema;
using ASC.Mail.Server.Utils;

namespace ASC.Mail.Server.Dal
{
    public class MailAddressDal : DalBase
    {
        public MailAddressDal(int tenant_id)
            : base("mailserver", tenant_id)
        {
        }

        internal SqlQuery GetAddressQuery(string domain_alias, string address_alias)
        {
            if (string.IsNullOrEmpty(domain_alias))
                throw new ArgumentNullException("domain_alias");

            if (string.IsNullOrEmpty(address_alias))
                throw new ArgumentNullException("address_alias");

            return new SqlQuery(AddressTable.name.Alias(address_alias))
                .InnerJoin(DomainTable.name.Alias(domain_alias),
                           Exp.EqColumns(AddressTable.Columns.id_domain.Prefix(address_alias),
                                         DomainTable.Columns.id.Prefix(domain_alias)
                               )
                )
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

        public List<MailAddressDto> GetMailboxAliases(int mailbox_id, DbManager db = null)
        {
            if (mailbox_id < 0)
                throw new ArgumentException("Argument mailbox_id less then zero.", "mailbox_id");

            const string domain_alias = "msd";
            const string address_alias = "msa";
            var address_query = GetAddressQuery(domain_alias, address_alias)
                .Where(AddressTable.Columns.id_mailbox.Prefix(address_alias), mailbox_id)
                .Where(AddressTable.Columns.is_alias.Prefix(address_alias), true);


            var result = NullSafeExecuteList(db, address_query);

            return result.ConvertAll(r => r.ToMailAddressDto());
        }

        public MailAddressDto AddMailboxAddress(int mailbox_id, string address_name, DateTime address_created_date,
            int domain_id, string domain_name, bool is_verified, DbManager db = null)
        {
            return AddRecordToAddressesTable(mailbox_id, address_name, address_created_date, domain_id, domain_name, false, false, is_verified, db);
        }

        public MailAddressDto AddMailboxAlias(int mailbox_id, string alias_name, DateTime alias_created_date,
            int domain_id, string domain_name, bool is_verified, DbManager db = null)
        {
            return AddRecordToAddressesTable(mailbox_id, alias_name, alias_created_date, domain_id, domain_name, false, true, is_verified, db);
        }

        public MailAddressDto AddMailgroupAddress(string address_name, DateTime address_created_date,
            int domain_id, string domain_name, bool is_verified, DbManager db = null)
        {
            return AddRecordToAddressesTable(-1, address_name, address_created_date, domain_id, domain_name, true, false, is_verified, db);
        }

        private MailAddressDto AddRecordToAddressesTable(int mailbox_id, string address_name, DateTime address_created_date,
            int domain_id, string domain_name, bool is_mail_group, bool is_alias, bool is_verified, DbManager db = null)
        {
            if (string.IsNullOrEmpty(address_name))
                throw new ArgumentNullException("address_name");

            if (domain_id < 0)
                throw new ArgumentException("Argument domain_id less then zero.", "domain_id");

            if (string.IsNullOrEmpty(domain_name))
                throw new ArgumentNullException("domain_name");

            var insert_values_query = new SqlInsert(AddressTable.name)
                .InColumnValue(AddressTable.Columns.id, 0)
                .InColumnValue(AddressTable.Columns.tenant, tenant_id)
                .InColumnValue(AddressTable.Columns.name, address_name)
                .InColumnValue(AddressTable.Columns.id_domain, domain_id)
                .InColumnValue(AddressTable.Columns.id_mailbox, mailbox_id)
                .InColumnValue(AddressTable.Columns.is_mail_group, is_mail_group)
                .InColumnValue(AddressTable.Columns.is_alias, is_alias)
                .InColumnValue(AddressTable.Columns.date_created, address_created_date)
                .Identity(0, 0, true);

            var added_address_id = NullSafeExecuteScalar<int>(db, insert_values_query);

            var result_dto = new MailAddressDto(added_address_id, tenant_id, address_name, domain_id, mailbox_id,
                                                is_mail_group, is_alias,
                                                new WebDomainDto(domain_id, domain_name, tenant_id, is_verified));

            return result_dto;
        }


        public void RemoveMailboxAlias(int alias_id, DbManager db)
        {
            if (alias_id < 0)
                throw new ArgumentException("Argument alias_id less then zero.", "alias_id");

            var delete_address_query = new SqlDelete(AddressTable.name)
                .Where(AddressTable.Columns.id, alias_id)
                .Where(AddressTable.Columns.tenant, tenant_id);

            db.ExecuteNonQuery(delete_address_query);
        }

        public MailAddressDto GetMailAddress(int address_id, DbManager db)
        {
            if (address_id < 0)
                throw new ArgumentException("Argument address_id less then zero.", "address_id");

            if (db == null)
                throw new ArgumentNullException("db");

            const string domain_alias = "msd";
            const string address_alias = "msa";
            var address_query = GetAddressQuery(domain_alias, address_alias)
                                .Where(AddressTable.Columns.id.Prefix(address_alias), address_id);

            var result = db.ExecuteList(address_query);
            
            return result.ConvertAll(r => r.ToMailAddressDto()).FirstOrDefault();
        }

        public List<MailAddressDto> GetMailAddresses(List<int> address_ids, DbManager db)
        {
            if (address_ids == null || !address_ids.Any())
                throw new ArgumentException("Address ids list must be not empty.", "address_ids");

            if (db == null)
                throw new ArgumentNullException("db");

            const string domain_alias = "msd";
            const string address_alias = "msa";
            var address_query = GetAddressQuery(domain_alias, address_alias)
                .Where(Exp.In(AddressTable.Columns.id.Prefix(address_alias), address_ids));

            var result = db.ExecuteList(address_query);

            var address_dto_list =
                result.ConvertAll(r => r.ToMailAddressDto());

            return address_dto_list;
        }

        public bool IsAddressAlreadyRegistered(string address_name, string domain_name, DbManager db)
        {
            if (string.IsNullOrEmpty(address_name))
                throw new ArgumentNullException("address_name");

            if (string.IsNullOrEmpty(domain_name))
                throw new ArgumentNullException("domain_name");

            if (db == null)
                throw new ArgumentNullException("db");

            const string domain_alias = "msd";
            const string address_alias = "msa";
            var address_query = GetAddressQuery(domain_alias, address_alias)
                .Where(AddressTable.Columns.name.Prefix(address_alias), address_name)
                .Where(AddressTable.Columns.tenant.Prefix(address_alias), tenant_id)
                .Where(DomainTable.Columns.name.Prefix(domain_alias), domain_name);

            return db.ExecuteList(address_query).Any();
        }
    }
}
