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
    public class MailGroupDal : DalBase
    {
        public MailGroupDal(int tenant_id)
            : base("mailserver", tenant_id)
        {
        }

        public MailGroupDal(string db_connection_string_name, int tenant_id) : base(db_connection_string_name, tenant_id)
        {
        }

        public MailGroupDto SaveMailGroup(string address_name, DateTime address_created_date,
            int domain_id, string domain_name, bool is_verified, List<MailAddressDto> address_dto_list, DbManager db)
        {
            if (string.IsNullOrEmpty(address_name))
                throw new ArgumentNullException("address_name");

            if (domain_id < 0)
                throw new ArgumentException("Argument domain_id less then zero.", "domain_id");

            if (string.IsNullOrEmpty(domain_name))
                throw new ArgumentNullException("domain_name");

            if (address_dto_list == null || !address_dto_list.Any())
                throw new ArgumentException("Address dto list must be not empty.", "address_dto_list");

            if(db == null)
                throw new ArgumentNullException("db");
            
            var address_dal = new MailAddressDal(tenant_id);
            var created_time = DateTime.UtcNow.ToDbStyle();

            var address_dto = address_dal.AddMailgroupAddress(address_name, address_created_date, domain_id, domain_name,
                                                              is_verified, db);

            var insert_group_query = new SqlInsert(MailGroupTable.name)
                                                .InColumnValue(MailGroupTable.Columns.id, 0)
                                                .InColumnValue(MailGroupTable.Columns.id_tenant, tenant_id)
                                                .InColumnValue(MailGroupTable.Columns.date_created, created_time)
                                                .InColumnValue(MailGroupTable.Columns.id_address, address_dto.id)
                                                .InColumnValue(MailGroupTable.Columns.address, address_name 
                                                                                             + '@' + domain_name)
                                                .Identity(0, 0, true);

            var mail_group_id = db.ExecuteScalar<int>(insert_group_query);

            AddAddressesToMailGroup(mail_group_id, address_dto_list.Select(dto => dto.id).ToList(), db);

            return new MailGroupDto(mail_group_id, tenant_id, address_dto.id, address_dto, address_dto_list);
        }

        public void DeleteMailGroup(int group_id, DbManager db)
        {
            if (group_id < 0)
                throw new ArgumentException("Argument group_id less then zero.", "group_id");

            if (db == null)
                throw new ArgumentNullException("db");
            
            var group_address_id = new SqlQuery(MailGroupTable.name)
                        .Select(MailGroupTable.Columns.id_address)
                        .Where(MailGroupTable.Columns.id, group_id);

            var delete_addresses_query = new SqlDelete(AddressTable.name)
                .Where(Exp.In(AddressTable.Columns.id, group_address_id));
            
            var delete_group_query = new SqlDelete(MailGroupTable.name)
                                        .Where(MailGroupTable.Columns.id_tenant, tenant_id)
                                        .Where(MailGroupTable.Columns.id, group_id);

            var delete_group_x_address = new SqlDelete(MailGroupXAddressesTable.name)
                .Where(MailGroupXAddressesTable.Columns.id_mail_group, group_id);

            var deleted_address_rows_count = db.ExecuteNonQuery(delete_addresses_query);
            var deleted_group_rows_count = db.ExecuteNonQuery(delete_group_query);

            if (deleted_group_rows_count != 0 && deleted_address_rows_count == 0 ||
                deleted_group_rows_count == 0 && deleted_address_rows_count != 0)
                throw new InvalidOperationException("Problem deleting mailgroup.");

            db.ExecuteNonQuery(delete_group_x_address);
        }

        public void AddAddressToMailGroup(int group_id, int address_id, DbManager db)
        {
            if (group_id < 0)
                throw new ArgumentException("Argument group_id less then zero.", "group_id");

            if (group_id < 0)
                throw new ArgumentException("Argument address_id less then zero.", "address_id");

            if (db == null)
                throw new ArgumentNullException("db");

            var add_address_to_mailgroup_query = new SqlInsert(MailGroupXAddressesTable.name)
                                                    .InColumnValue(MailGroupXAddressesTable.Columns.id_address, address_id)
                                                    .InColumnValue(MailGroupXAddressesTable.Columns.id_mail_group, group_id);

            var inseted_rows_count = db.ExecuteNonQuery(add_address_to_mailgroup_query);
            if (inseted_rows_count == 0)
                throw new InvalidOperationException(String.Format("Problem adding address. GroupId: {0}, AddressId: {1}", group_id, address_id));
        }

        public void AddAddressesToMailGroup(int group_id, List<int> address_ids, DbManager db)
        {
            if (group_id < 0)
                throw new ArgumentException("Argument group_id less then zero.", "group_id");

            if (address_ids == null || !address_ids.Any())
                throw new ArgumentException("Address ids list must be not empty.", "address_ids");

            if(db == null)
                throw new ArgumentNullException("db");
            
            var add_address_to_mailgroup_query = new SqlInsert(MailGroupXAddressesTable.name)
                .InColumns(MailGroupXAddressesTable.Columns.id_address, MailGroupXAddressesTable.Columns.id_mail_group);

            address_ids.ForEach(address_id => add_address_to_mailgroup_query.Values(address_id, group_id));

            db.ExecuteNonQuery(add_address_to_mailgroup_query);
        }

        public void DeleteAddressFromMailGroup(int group_id, int address_id, DbManager db)
        {
            if (group_id < 0)
                throw new ArgumentException("Argument group_id less then zero.", "group_id");

            if (address_id < 0)
                throw new ArgumentException("Argument address_id less then zero.", "address_id");

            if (db == null)
                throw new ArgumentNullException("db");

            var delete_address_from_mailgroup_query = new SqlDelete(MailGroupXAddressesTable.name)
                                         .Where(MailGroupXAddressesTable.Columns.id_address, address_id)
                                         .Where(MailGroupXAddressesTable.Columns.id_mail_group, group_id);

            var deleted_rows_count = db.ExecuteNonQuery(delete_address_from_mailgroup_query);
            if (deleted_rows_count > 1)
                throw new InvalidOperationException(String.Format("Problem deleting address. GroupId: {0}, AddressId: {1}", group_id, address_id));
        }

        public List<MailGroupDto> GetMailGroups()
        {
            var select_groups_query = GetGroupQuery()
                .Where(MailGroupTable.Columns.id_tenant, tenant_id);

            List<MailGroupDto> mailgroup_dto_list;

            using (var db = GetDb())
            {
                var result = db.ExecuteList(select_groups_query);

                mailgroup_dto_list =
                    result.ConvertAll(r => r.ToMailGroupDto(GetGroupAddresses(Convert.ToInt32(r[0]), db)));
            }

            return mailgroup_dto_list;
        }

        public List<MailAddressDto> GetGroupAddresses(int group_id, DbManager db = null)
        {
            if (group_id < 0)
                throw new ArgumentException("Argument group_id less then zero.", "group_id");

            const string m_x_a_alias = "mxa";
            const string address_alias = "msa";
            const string domain_alias = "msd";
            var select_addresses_query = new SqlQuery(MailGroupXAddressesTable.name.Alias(m_x_a_alias))
                .InnerJoin(AddressTable.name.Alias(address_alias),
                           Exp.EqColumns(MailGroupXAddressesTable.Columns.id_address.Prefix(m_x_a_alias),
                                         AddressTable.Columns.id.Prefix(address_alias)))
                .InnerJoin(DomainTable.name.Alias(domain_alias),
                           Exp.EqColumns(DomainTable.Columns.id.Prefix(domain_alias),
                                         AddressTable.Columns.id_domain.Prefix(address_alias)))
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
                .Select(DomainTable.Columns.is_verified.Prefix(domain_alias))
                .Where(MailGroupXAddressesTable.Columns.id_mail_group.Prefix(m_x_a_alias), group_id)
                .Where(AddressTable.Columns.tenant.Prefix(address_alias), tenant_id);

            return NullSafeExecuteList(db, select_addresses_query)
                .ConvertAll(r => r.ToMailAddressDto());
        }

        public MailGroupDto GetMailGroup(int group_id)
        {
            if (group_id < 0)
                throw new ArgumentException("Argument group_id less then zero.", "group_id");

            const string groups_alias = "msg";
            var select_groups_query = GetGroupQuery()
                .Where(MailGroupTable.Columns.id_tenant.Prefix(groups_alias), tenant_id)
                .Where(MailGroupTable.Columns.id.Prefix(groups_alias), group_id);

            MailGroupDto group_dto;

            using (var db = GetDb())
            {
                var result = db.ExecuteList(select_groups_query);

                group_dto = result
                    .ConvertAll(r =>
                                r.ToMailGroupDto(GetGroupAddresses(Convert.ToInt32(r[0]), db)))
                    .FirstOrDefault();
            }

            return group_dto;
        }

        private SqlQuery GetGroupQuery()
        {
            const string groups_alias = "msg";
            const string domain_alias = "msd";
            const string address_alias = "msa";

            return new SqlQuery(MailGroupTable.name.Alias(groups_alias))
                .InnerJoin(AddressTable.name.Alias(address_alias),
                           Exp.EqColumns(AddressTable.Columns.id.Prefix(address_alias),
                                         MailGroupTable.Columns.id_address.Prefix(groups_alias)))
                .InnerJoin(DomainTable.name.Alias(domain_alias),
                           Exp.EqColumns(AddressTable.Columns.id_domain.Prefix(address_alias),
                                         DomainTable.Columns.id.Prefix(domain_alias)))
                .Select(MailGroupTable.Columns.id.Prefix(groups_alias))
                .Select(MailGroupTable.Columns.id_address.Prefix(groups_alias))
                .Select(MailGroupTable.Columns.id_tenant.Prefix(groups_alias))
                .Select(MailGroupTable.Columns.date_created.Prefix(groups_alias))
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
    }
}
