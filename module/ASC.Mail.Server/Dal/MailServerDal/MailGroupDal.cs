/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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
using ASC.Mail.Aggregator.DbSchema;

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
            var createdTime = DateTime.UtcNow;

            var addressDto = addressDal.AddMailgroupAddress(addressName, addressCreatedDate, domainId, domainName,
                                                              isVerified, db);

            var insertGroupQuery = new SqlInsert(MailGroupTable.Name)
                                                .InColumnValue(MailGroupTable.Columns.Id, 0)
                                                .InColumnValue(MailGroupTable.Columns.Tenant, tenant)
                                                .InColumnValue(MailGroupTable.Columns.DateCreated, createdTime)
                                                .InColumnValue(MailGroupTable.Columns.AddressId, addressDto.id)
                                                .InColumnValue(MailGroupTable.Columns.Address, addressName 
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
            
            var groupAddressId = new SqlQuery(MailGroupTable.Name)
                        .Select(MailGroupTable.Columns.AddressId)
                        .Where(MailGroupTable.Columns.Id, groupId);

            var deleteAddressesQuery = new SqlDelete(AddressTable.Name)
                .Where(Exp.In(AddressTable.Columns.Id, groupAddressId));
            
            var deleteGroupQuery = new SqlDelete(MailGroupTable.Name)
                                        .Where(MailGroupTable.Columns.Tenant, tenant)
                                        .Where(MailGroupTable.Columns.Id, groupId);

            var deleteGroupXAddress = new SqlDelete(MailGroupXAddressesTable.Name)
                .Where(MailGroupXAddressesTable.Columns.MailGroupId, groupId);

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

            if (addressId < 0)
                throw new ArgumentException("Argument address_id less then zero.", "addressId");

            if (db == null)
                throw new ArgumentNullException("db");

            var addAddressToMailgroupQuery = new SqlInsert(MailGroupXAddressesTable.Name)
                                                    .InColumnValue(MailGroupXAddressesTable.Columns.AddressId, addressId)
                                                    .InColumnValue(MailGroupXAddressesTable.Columns.MailGroupId, groupId);

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
            
            var addAddressToMailgroupQuery = new SqlInsert(MailGroupXAddressesTable.Name)
                .InColumns(MailGroupXAddressesTable.Columns.AddressId, MailGroupXAddressesTable.Columns.MailGroupId);

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

            var deleteAddressFromMailgroupQuery = new SqlDelete(MailGroupXAddressesTable.Name)
                                         .Where(MailGroupXAddressesTable.Columns.AddressId, addressId)
                                         .Where(MailGroupXAddressesTable.Columns.MailGroupId, groupId);

            var deletedRowsCount = db.ExecuteNonQuery(deleteAddressFromMailgroupQuery);
            if (deletedRowsCount > 1)
                throw new InvalidOperationException(String.Format("Problem deleting address. GroupId: {0}, AddressId: {1}", groupId, addressId));
        }

        public List<MailGroupDto> GetMailGroups()
        {
            var selectGroupsQuery = GetGroupQuery()
                .Where(MailGroupTable.Columns.Tenant, tenant);

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
            var selectAddressesQuery = new SqlQuery(MailGroupXAddressesTable.Name.Alias(m_x_a_alias))
                .InnerJoin(AddressTable.Name.Alias(address_alias),
                           Exp.EqColumns(MailGroupXAddressesTable.Columns.AddressId.Prefix(m_x_a_alias),
                                         AddressTable.Columns.Id.Prefix(address_alias)))
                .InnerJoin(DomainTable.Name.Alias(domain_alias),
                           Exp.EqColumns(DomainTable.Columns.Id.Prefix(domain_alias),
                                         AddressTable.Columns.DomainId.Prefix(address_alias)))
                .Select(AddressTable.Columns.Id.Prefix(address_alias))
                .Select(AddressTable.Columns.Tenant.Prefix(address_alias))
                .Select(AddressTable.Columns.AddressName.Prefix(address_alias))
                .Select(AddressTable.Columns.DomainId.Prefix(address_alias))
                .Select(AddressTable.Columns.MailboxId.Prefix(address_alias))
                .Select(AddressTable.Columns.IsMailGroup.Prefix(address_alias))
                .Select(AddressTable.Columns.IsAlias.Prefix(address_alias))
                .Select(AddressTable.Columns.DateCreated.Prefix(address_alias))
                .Select(DomainTable.Columns.Id.Prefix(domain_alias))
                .Select(DomainTable.Columns.DomainName.Prefix(domain_alias))
                .Select(DomainTable.Columns.Tenant.Prefix(domain_alias))
                .Select(DomainTable.Columns.DateAdded.Prefix(domain_alias))
                .Select(DomainTable.Columns.IsVerified.Prefix(domain_alias))
                .Where(MailGroupXAddressesTable.Columns.MailGroupId.Prefix(m_x_a_alias), groupId)
                .Where(AddressTable.Columns.Tenant.Prefix(address_alias), tenant);

            return NullSafeExecuteList(db, selectAddressesQuery)
                .ConvertAll(r => r.ToMailAddressDto());
        }

        public MailGroupDto GetMailGroup(int groupId)
        {
            if (groupId < 0)
                throw new ArgumentException("Argument group_id less then zero.", "groupId");

            const string groups_alias = "msg";
            var selectGroupsQuery = GetGroupQuery()
                .Where(MailGroupTable.Columns.Tenant.Prefix(groups_alias), tenant)
                .Where(MailGroupTable.Columns.Id.Prefix(groups_alias), groupId);

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

            return new SqlQuery(MailGroupTable.Name.Alias(groups_alias))
                .InnerJoin(AddressTable.Name.Alias(address_alias),
                           Exp.EqColumns(AddressTable.Columns.Id.Prefix(address_alias),
                                         MailGroupTable.Columns.AddressId.Prefix(groups_alias)))
                .InnerJoin(DomainTable.Name.Alias(domain_alias),
                           Exp.EqColumns(AddressTable.Columns.DomainId.Prefix(address_alias),
                                         DomainTable.Columns.Id.Prefix(domain_alias)))
                .Select(MailGroupTable.Columns.Id.Prefix(groups_alias))
                .Select(MailGroupTable.Columns.AddressId.Prefix(groups_alias))
                .Select(MailGroupTable.Columns.Tenant.Prefix(groups_alias))
                .Select(MailGroupTable.Columns.DateCreated.Prefix(groups_alias))
                .Select(AddressTable.Columns.Id.Prefix(address_alias))
                .Select(AddressTable.Columns.Tenant.Prefix(address_alias))
                .Select(AddressTable.Columns.AddressName.Prefix(address_alias))
                .Select(AddressTable.Columns.DomainId.Prefix(address_alias))
                .Select(AddressTable.Columns.MailboxId.Prefix(address_alias))
                .Select(AddressTable.Columns.IsMailGroup.Prefix(address_alias))
                .Select(AddressTable.Columns.IsAlias.Prefix(address_alias))
                .Select(AddressTable.Columns.DateCreated.Prefix(address_alias))
                .Select(DomainTable.Columns.Id.Prefix(domain_alias))
                .Select(DomainTable.Columns.DomainName.Prefix(domain_alias))
                .Select(DomainTable.Columns.Tenant.Prefix(domain_alias))
                .Select(DomainTable.Columns.DateAdded.Prefix(domain_alias))
                .Select(DomainTable.Columns.IsVerified.Prefix(domain_alias));
        }
    }
}
