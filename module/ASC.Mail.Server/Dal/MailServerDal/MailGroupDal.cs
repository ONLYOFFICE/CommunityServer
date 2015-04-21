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
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Mail.Aggregator.Common.Extension;
using ASC.Mail.Aggregator.Dal.DbSchema;

namespace ASC.Mail.Server.Dal
{
    public class MailGroupDal : DalBase
    {
        public MailGroupDal(int tenant)
            : base("mailserver", tenant)
        {
        }

        public MailGroupDal(string dbConnectionStringName, int tenant)
            : base(dbConnectionStringName, tenant)
        {
        }

        public MailGroupDto SaveMailGroup(string addressName, DateTime addressCreatedDate,
            int domainId, string domainName, bool isVerified, List<MailAddressDto> addressDtoList, DbManager db)
        {
            if (string.IsNullOrEmpty(addressName))
                throw new ArgumentNullException("addressName");

            if (domainId < 0)
                throw new ArgumentException("Argument domain_id less then zero.", "domainId");

            if (string.IsNullOrEmpty(domainName))
                throw new ArgumentNullException("domainName");

            if (addressDtoList == null || !addressDtoList.Any())
                throw new ArgumentException("Address dto list must be not empty.", "addressDtoList");

            if(db == null)
                throw new ArgumentNullException("db");
            
            var addressDal = new MailAddressDal(tenant);
            var createdTime = DateTime.UtcNow.ToDbStyle();

            var addressDto = addressDal.AddMailgroupAddress(addressName, addressCreatedDate, domainId, domainName,
                                                              isVerified, db);

            var insertGroupQuery = new SqlInsert(MailGroupTable.name)
                                                .InColumnValue(MailGroupTable.Columns.id, 0)
                                                .InColumnValue(MailGroupTable.Columns.id_tenant, tenant)
                                                .InColumnValue(MailGroupTable.Columns.date_created, createdTime)
                                                .InColumnValue(MailGroupTable.Columns.id_address, addressDto.id)
                                                .InColumnValue(MailGroupTable.Columns.address, addressName 
                                                                                             + '@' + domainName)
                                                .Identity(0, 0, true);

            var mailGroupId = db.ExecuteScalar<int>(insertGroupQuery);

            AddAddressesToMailGroup(mailGroupId, addressDtoList.Select(dto => dto.id).ToList(), db);

            return new MailGroupDto(mailGroupId, tenant, addressDto.id, addressDto, addressDtoList);
        }

        public void DeleteMailGroup(int groupId, DbManager db)
        {
            if (groupId < 0)
                throw new ArgumentException("Argument group_id less then zero.", "groupId");

            if (db == null)
                throw new ArgumentNullException("db");
            
            var groupAddressId = new SqlQuery(MailGroupTable.name)
                        .Select(MailGroupTable.Columns.id_address)
                        .Where(MailGroupTable.Columns.id, groupId);

            var deleteAddressesQuery = new SqlDelete(AddressTable.name)
                .Where(Exp.In(AddressTable.Columns.id, groupAddressId));
            
            var deleteGroupQuery = new SqlDelete(MailGroupTable.name)
                                        .Where(MailGroupTable.Columns.id_tenant, tenant)
                                        .Where(MailGroupTable.Columns.id, groupId);

            var deleteGroupXAddress = new SqlDelete(MailGroupXAddressesTable.name)
                .Where(MailGroupXAddressesTable.Columns.id_mail_group, groupId);

            var deletedAddressRowsCount = db.ExecuteNonQuery(deleteAddressesQuery);
            var deletedGroupRowsCount = db.ExecuteNonQuery(deleteGroupQuery);

            if (deletedGroupRowsCount != 0 && deletedAddressRowsCount == 0 ||
                deletedGroupRowsCount == 0 && deletedAddressRowsCount != 0)
                throw new InvalidOperationException("Problem deleting mailgroup.");

            db.ExecuteNonQuery(deleteGroupXAddress);
        }

        public void AddAddressToMailGroup(int groupId, int addressId, DbManager db)
        {
            if (groupId < 0)
                throw new ArgumentException("Argument group_id less then zero.", "groupId");

            if (groupId < 0)
                throw new ArgumentException("Argument address_id less then zero.", "addressId");

            if (db == null)
                throw new ArgumentNullException("db");

            var addAddressToMailgroupQuery = new SqlInsert(MailGroupXAddressesTable.name)
                                                    .InColumnValue(MailGroupXAddressesTable.Columns.id_address, addressId)
                                                    .InColumnValue(MailGroupXAddressesTable.Columns.id_mail_group, groupId);

            var insetedRowsCount = db.ExecuteNonQuery(addAddressToMailgroupQuery);
            if (insetedRowsCount == 0)
                throw new InvalidOperationException(String.Format("Problem adding address. GroupId: {0}, AddressId: {1}", groupId, addressId));
        }

        public void AddAddressesToMailGroup(int groupId, List<int> addressIds, DbManager db)
        {
            if (groupId < 0)
                throw new ArgumentException("Argument group_id less then zero.", "groupId");

            if (addressIds == null || !addressIds.Any())
                throw new ArgumentException("Address ids list must be not empty.", "addressIds");

            if(db == null)
                throw new ArgumentNullException("db");
            
            var addAddressToMailgroupQuery = new SqlInsert(MailGroupXAddressesTable.name)
                .InColumns(MailGroupXAddressesTable.Columns.id_address, MailGroupXAddressesTable.Columns.id_mail_group);

            addressIds.ForEach(addressId => addAddressToMailgroupQuery.Values(addressId, groupId));

            db.ExecuteNonQuery(addAddressToMailgroupQuery);
        }

        public void DeleteAddressFromMailGroup(int groupId, int addressId, DbManager db)
        {
            if (groupId < 0)
                throw new ArgumentException("Argument group_id less then zero.", "groupId");

            if (addressId < 0)
                throw new ArgumentException("Argument address_id less then zero.", "addressId");

            if (db == null)
                throw new ArgumentNullException("db");

            var deleteAddressFromMailgroupQuery = new SqlDelete(MailGroupXAddressesTable.name)
                                         .Where(MailGroupXAddressesTable.Columns.id_address, addressId)
                                         .Where(MailGroupXAddressesTable.Columns.id_mail_group, groupId);

            var deletedRowsCount = db.ExecuteNonQuery(deleteAddressFromMailgroupQuery);
            if (deletedRowsCount > 1)
                throw new InvalidOperationException(String.Format("Problem deleting address. GroupId: {0}, AddressId: {1}", groupId, addressId));
        }

        public List<MailGroupDto> GetMailGroups()
        {
            var selectGroupsQuery = GetGroupQuery()
                .Where(MailGroupTable.Columns.id_tenant, tenant);

            List<MailGroupDto> mailgroupDtoList;

            using (var db = GetDb())
            {
                var result = db.ExecuteList(selectGroupsQuery);

                mailgroupDtoList =
                    result.ConvertAll(r => r.ToMailGroupDto(GetGroupAddresses(Convert.ToInt32(r[0]), db)));
            }

            return mailgroupDtoList;
        }

        public List<MailAddressDto> GetGroupAddresses(int groupId, DbManager db = null)
        {
            if (groupId < 0)
                throw new ArgumentException("Argument group_id less then zero.", "groupId");

            const string m_x_a_alias = "mxa";
            const string address_alias = "msa";
            const string domain_alias = "msd";
            var selectAddressesQuery = new SqlQuery(MailGroupXAddressesTable.name.Alias(m_x_a_alias))
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
                .Where(MailGroupXAddressesTable.Columns.id_mail_group.Prefix(m_x_a_alias), groupId)
                .Where(AddressTable.Columns.tenant.Prefix(address_alias), tenant);

            return NullSafeExecuteList(db, selectAddressesQuery)
                .ConvertAll(r => r.ToMailAddressDto());
        }

        public MailGroupDto GetMailGroup(int groupId)
        {
            if (groupId < 0)
                throw new ArgumentException("Argument group_id less then zero.", "groupId");

            const string groups_alias = "msg";
            var selectGroupsQuery = GetGroupQuery()
                .Where(MailGroupTable.Columns.id_tenant.Prefix(groups_alias), tenant)
                .Where(MailGroupTable.Columns.id.Prefix(groups_alias), groupId);

            MailGroupDto groupDto;

            using (var db = GetDb())
            {
                var result = db.ExecuteList(selectGroupsQuery);

                groupDto = result
                    .ConvertAll(r =>
                                r.ToMailGroupDto(GetGroupAddresses(Convert.ToInt32(r[0]), db)))
                    .FirstOrDefault();
            }

            return groupDto;
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
