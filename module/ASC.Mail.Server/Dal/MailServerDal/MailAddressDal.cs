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

            return new SqlQuery(AddressTable.name.Alias(addressAlias))
                .InnerJoin(DomainTable.name.Alias(domainAlias),
                           Exp.EqColumns(AddressTable.Columns.id_domain.Prefix(addressAlias),
                                         DomainTable.Columns.id.Prefix(domainAlias)
                               )
                )
                .Select(AddressTable.Columns.id.Prefix(addressAlias))
                .Select(AddressTable.Columns.tenant.Prefix(addressAlias))
                .Select(AddressTable.Columns.name.Prefix(addressAlias))
                .Select(AddressTable.Columns.id_domain.Prefix(addressAlias))
                .Select(AddressTable.Columns.id_mailbox.Prefix(addressAlias))
                .Select(AddressTable.Columns.is_mail_group.Prefix(addressAlias))
                .Select(AddressTable.Columns.is_alias.Prefix(addressAlias))
                .Select(AddressTable.Columns.date_created.Prefix(addressAlias))
                .Select(DomainTable.Columns.id.Prefix(domainAlias))
                .Select(DomainTable.Columns.name.Prefix(domainAlias))
                .Select(DomainTable.Columns.tenant.Prefix(domainAlias))
                .Select(DomainTable.Columns.date_added.Prefix(domainAlias))
                .Select(DomainTable.Columns.is_verified.Prefix(domainAlias));
        }

        public List<MailAddressDto> GetMailboxAliases(int mailboxId, DbManager db = null)
        {
            if (mailboxId < 0)
                throw new ArgumentException("Argument mailbox_id less then zero.", "mailboxId");

            const string domain_alias = "msd";
            const string address_alias = "msa";
            var addressQuery = GetAddressQuery(domain_alias, address_alias)
                .Where(AddressTable.Columns.id_mailbox.Prefix(address_alias), mailboxId)
                .Where(AddressTable.Columns.is_alias.Prefix(address_alias), true);


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

            var insertValuesQuery = new SqlInsert(AddressTable.name)
                .InColumnValue(AddressTable.Columns.id, 0)
                .InColumnValue(AddressTable.Columns.tenant, tenant)
                .InColumnValue(AddressTable.Columns.name, addressName)
                .InColumnValue(AddressTable.Columns.id_domain, domainId)
                .InColumnValue(AddressTable.Columns.id_mailbox, mailboxId)
                .InColumnValue(AddressTable.Columns.is_mail_group, isMailGroup)
                .InColumnValue(AddressTable.Columns.is_alias, isAlias)
                .InColumnValue(AddressTable.Columns.date_created, addressCreatedDate)
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

            var deleteAddressQuery = new SqlDelete(AddressTable.name)
                .Where(AddressTable.Columns.id, aliasId)
                .Where(AddressTable.Columns.tenant, tenant);

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
                                .Where(AddressTable.Columns.tenant.Prefix(address_alias), tenant)
                                .Where(AddressTable.Columns.id.Prefix(address_alias), addressId);

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
                .Where(AddressTable.Columns.tenant.Prefix(address_alias), tenant)
                .Where(Exp.In(AddressTable.Columns.id.Prefix(address_alias), addressIds));

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
                .Where(AddressTable.Columns.name.Prefix(address_alias), addressName)
                .Where(AddressTable.Columns.tenant.Prefix(address_alias), tenant)
                .Where(DomainTable.Columns.name.Prefix(domain_alias), domainName);

            return db.ExecuteList(addressQuery).Any();
        }
    }
}
