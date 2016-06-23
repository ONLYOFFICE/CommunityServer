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
    public class MailAddressDal : DalBase
    {
        public MailAddressDal(int tenant)
            : base("mailserver", tenant)
        {
        }

        internal SqlQuery GetAddressQuery(string domainAlias, string addressAlias)
        {
            if (string.IsNullOrEmpty(domainAlias))
                throw new ArgumentNullException("domainAlias");

            if (string.IsNullOrEmpty(addressAlias))
                throw new ArgumentNullException("addressAlias");

            return new SqlQuery(AddressTable.Name.Alias(addressAlias))
                .InnerJoin(DomainTable.Name.Alias(domainAlias),
                           Exp.EqColumns(AddressTable.Columns.DomainId.Prefix(addressAlias),
                                         DomainTable.Columns.Id.Prefix(domainAlias)
                               )
                )
                .Select(AddressTable.Columns.Id.Prefix(addressAlias))
                .Select(AddressTable.Columns.Tenant.Prefix(addressAlias))
                .Select(AddressTable.Columns.AddressName.Prefix(addressAlias))
                .Select(AddressTable.Columns.DomainId.Prefix(addressAlias))
                .Select(AddressTable.Columns.MailboxId.Prefix(addressAlias))
                .Select(AddressTable.Columns.IsMailGroup.Prefix(addressAlias))
                .Select(AddressTable.Columns.IsAlias.Prefix(addressAlias))
                .Select(AddressTable.Columns.DateCreated.Prefix(addressAlias))
                .Select(DomainTable.Columns.Id.Prefix(domainAlias))
                .Select(DomainTable.Columns.DomainName.Prefix(domainAlias))
                .Select(DomainTable.Columns.Tenant.Prefix(domainAlias))
                .Select(DomainTable.Columns.DateAdded.Prefix(domainAlias))
                .Select(DomainTable.Columns.IsVerified.Prefix(domainAlias));
        }

        public List<MailAddressDto> GetMailboxAliases(int mailboxId, DbManager db = null)
        {
            if (mailboxId < 0)
                throw new ArgumentException("Argument mailbox_id less then zero.", "mailboxId");

            const string domain_alias = "msd";
            const string address_alias = "msa";
            var addressQuery = GetAddressQuery(domain_alias, address_alias)
                .Where(AddressTable.Columns.MailboxId.Prefix(address_alias), mailboxId)
                .Where(AddressTable.Columns.IsAlias.Prefix(address_alias), true);


            var result = NullSafeExecuteList(db, addressQuery);

            return result.ConvertAll(r => r.ToMailAddressDto());
        }

        public MailAddressDto AddMailboxAddress(int mailboxId, string addressName, DateTime addressCreatedDate,
            int domainId, string domainName, bool isVerified, DbManager db = null)
        {
            return AddRecordToAddressesTable(mailboxId, addressName, addressCreatedDate, domainId, domainName, false, false, isVerified, db);
        }

        public MailAddressDto AddMailboxAlias(int mailboxId, string aliasName, DateTime aliasCreatedDate,
            int domainId, string domainName, bool isVerified, DbManager db = null)
        {
            return AddRecordToAddressesTable(mailboxId, aliasName, aliasCreatedDate, domainId, domainName, false, true, isVerified, db);
        }

        public MailAddressDto AddMailgroupAddress(string addressName, DateTime addressCreatedDate,
            int domainId, string domainName, bool isVerified, DbManager db = null)
        {
            return AddRecordToAddressesTable(-1, addressName, addressCreatedDate, domainId, domainName, true, false, isVerified, db);
        }

        private MailAddressDto AddRecordToAddressesTable(int mailboxId, string addressName, DateTime addressCreatedDate,
            int domainId, string domainName, bool isMailGroup, bool isAlias, bool isVerified, DbManager db = null)
        {
            if (string.IsNullOrEmpty(addressName))
                throw new ArgumentNullException("addressName");

            if (domainId < 0)
                throw new ArgumentException("Argument domain_id less then zero.", "domainId");

            if (string.IsNullOrEmpty(domainName))
                throw new ArgumentNullException("domainName");

            var insertValuesQuery = new SqlInsert(AddressTable.Name)
                .InColumnValue(AddressTable.Columns.Id, 0)
                .InColumnValue(AddressTable.Columns.Tenant, tenant)
                .InColumnValue(AddressTable.Columns.AddressName, addressName)
                .InColumnValue(AddressTable.Columns.DomainId, domainId)
                .InColumnValue(AddressTable.Columns.MailboxId, mailboxId)
                .InColumnValue(AddressTable.Columns.IsMailGroup, isMailGroup)
                .InColumnValue(AddressTable.Columns.IsAlias, isAlias)
                .InColumnValue(AddressTable.Columns.DateCreated, addressCreatedDate)
                .Identity(0, 0, true);

            var addedAddressId = NullSafeExecuteScalar<int>(db, insertValuesQuery);

            var resultDto = new MailAddressDto(addedAddressId, tenant, addressName, domainId, mailboxId,
                                                isMailGroup, isAlias,
                                                new WebDomainDto(domainId, domainName, tenant, isVerified));

            return resultDto;
        }


        public void RemoveMailboxAlias(int aliasId, DbManager db)
        {
            if (aliasId < 0)
                throw new ArgumentException("Argument alias_id less then zero.", "aliasId");

            var deleteAddressQuery = new SqlDelete(AddressTable.Name)
                .Where(AddressTable.Columns.Id, aliasId)
                .Where(AddressTable.Columns.Tenant, tenant);

            db.ExecuteNonQuery(deleteAddressQuery);
        }

        public MailAddressDto GetMailAddress(int addressId, DbManager db)
        {
            if (addressId < 0)
                throw new ArgumentException("Argument address_id less then zero.", "addressId");

            if (db == null)
                throw new ArgumentNullException("db");

            const string domain_alias = "msd";
            const string address_alias = "msa";
            var addressQuery = GetAddressQuery(domain_alias, address_alias)
                                .Where(AddressTable.Columns.Tenant.Prefix(address_alias), tenant)
                                .Where(AddressTable.Columns.Id.Prefix(address_alias), addressId);

            var result = db.ExecuteList(addressQuery);
            
            return result.ConvertAll(r => r.ToMailAddressDto()).FirstOrDefault();
        }

        public List<MailAddressDto> GetMailAddresses(List<int> addressIds, DbManager db)
        {
            if (addressIds == null || !addressIds.Any())
                throw new ArgumentException("Address ids list must be not empty.", "addressIds");

            if (db == null)
                throw new ArgumentNullException("db");

            const string domain_alias = "msd";
            const string address_alias = "msa";
            var addressQuery = GetAddressQuery(domain_alias, address_alias)
                .Where(AddressTable.Columns.Tenant.Prefix(address_alias), tenant)
                .Where(Exp.In(AddressTable.Columns.Id.Prefix(address_alias), addressIds));

            var result = db.ExecuteList(addressQuery);

            var addressDtoList =
                result.ConvertAll(r => r.ToMailAddressDto());

            return addressDtoList;
        }

        public bool IsAddressAlreadyRegistered(string addressName, string domainName, DbManager db)
        {
            if (string.IsNullOrEmpty(addressName))
                throw new ArgumentNullException("addressName");

            if (string.IsNullOrEmpty(domainName))
                throw new ArgumentNullException("domainName");

            if (db == null)
                throw new ArgumentNullException("db");

            const string domain_alias = "msd";
            const string address_alias = "msa";
            var addressQuery = GetAddressQuery(domain_alias, address_alias)
                .Where(AddressTable.Columns.AddressName.Prefix(address_alias), addressName)
                .Where(AddressTable.Columns.Tenant.Prefix(address_alias), tenant)
                .Where(DomainTable.Columns.DomainName.Prefix(domain_alias), domainName);

            return db.ExecuteList(addressQuery).Any();
        }
    }
}
