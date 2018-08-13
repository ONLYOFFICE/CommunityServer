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
using System.Linq;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Common.Security.Authentication;
using ASC.Mail.Aggregator.Common.Authorization;
using ASC.Mail.Aggregator.Common.Extension;
using ASC.Mail.Aggregator.DbSchema;
using ASC.Security.Cryptography;

namespace ASC.Mail.Server.Dal
{
    public class MailboxDal : DalBase
    {
        private readonly int _mailboxCountLimit;
        
        public MailboxDal(int tenant) : 
            this("mailserver", tenant)
        {
        }

        public MailboxDal(int tenant, int mailboxesCountLimit) :
            this("mailserver", tenant, mailboxesCountLimit)
        {
        }

        public MailboxDal(string dbConnectionStringName, int tenant) 
            : this(dbConnectionStringName, tenant, 2)
        {
        }

        public MailboxDal(string dbConnectionStringName, int tenant, int mailboxesCountLimit)
            : base(dbConnectionStringName, tenant)
        {
            _mailboxCountLimit = mailboxesCountLimit;
        }

        public MailboxWithAddressDto CreateMailbox(IAccount teamlabAccount, string name,
                                                   string fullAddress, string password, string addressName,
                                                   DateTime addressCreatedDate,
                                                   int domainId, string domainName, bool isVerified, DbManager db)
        {
            if (teamlabAccount == null)
                throw new ArgumentNullException("teamlabAccount");

            if(string.IsNullOrEmpty(fullAddress))
                throw new ArgumentNullException("fullAddress");

            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException("password");

            if (string.IsNullOrEmpty(addressName))
                throw new ArgumentNullException("addressName");

            if (domainId < 0)
                throw new ArgumentException("Argument domain_id less then zero.", "domainId");

            if (string.IsNullOrEmpty(domainName))
                throw new ArgumentNullException("domainName");

            if (db == null)
                throw new ArgumentNullException("db");

            const string mailbox_alias = "mb";
            const string address_alias = "a";

            var userTeamlabMailboxCountQuery = new SqlQuery(MailboxTable.Name.Alias(mailbox_alias))
                .InnerJoin(AddressTable.Name.Alias(address_alias),
                           Exp.EqColumns(MailboxTable.Columns.Id.Prefix(mailbox_alias),
                                         AddressTable.Columns.MailboxId.Prefix(address_alias)))
                .SelectCount()
                .Where(MailboxTable.Columns.IsTeamlabMailbox.Prefix(mailbox_alias), true)
                .Where(MailboxTable.Columns.IsRemoved.Prefix(mailbox_alias), false)
                .Where(MailboxTable.Columns.Tenant.Prefix(mailbox_alias), tenant)
                .Where(MailboxTable.Columns.User.Prefix(mailbox_alias), teamlabAccount.ID.ToString())
                .Where(AddressTable.Columns.DomainId.Prefix(address_alias), domainId)
                .Where(AddressTable.Columns.IsAlias.Prefix(address_alias), false);

            var userMailboxesCount = db.ExecuteScalar<int>(userTeamlabMailboxCountQuery);

            if (userMailboxesCount >= _mailboxCountLimit)
                throw new ArgumentOutOfRangeException("addressName", string.Format("Count of user's mailboxes must be less or equal {0}.", _mailboxCountLimit));

            var serverInformation = GetTenantServer(db);

            var dateCreated = DateTime.UtcNow;

            var insertQuery = new SqlInsert(MailboxTable.Name)
                .InColumnValue(MailboxTable.Columns.Id, 0)
                .InColumnValue(MailboxTable.Columns.Tenant, tenant)
                .InColumnValue(MailboxTable.Columns.User, teamlabAccount.ID.ToString())
                .InColumnValue(MailboxTable.Columns.Address, fullAddress)
                .InColumnValue(MailboxTable.Columns.AddressName, name)
                .InColumnValue(MailboxTable.Columns.Password, InstanceCrypto.Encrypt(password))
                .InColumnValue(MailboxTable.Columns.MsgCountLast, 0)
                .InColumnValue(MailboxTable.Columns.SmtpPassword, InstanceCrypto.Encrypt(password))
                .InColumnValue(MailboxTable.Columns.SizeLast, 0)
                .InColumnValue(MailboxTable.Columns.LoginDelay, Config.LoginDelayInSeconds)
                .InColumnValue(MailboxTable.Columns.Enabled, true)
                .InColumnValue(MailboxTable.Columns.Imap, true)
                .InColumnValue(MailboxTable.Columns.OAuthType, AuthorizationServiceType.None)
                .InColumnValue(MailboxTable.Columns.OAuthToken, null)
                .InColumnValue(MailboxTable.Columns.DateCreated, dateCreated)
                .InColumnValue(MailboxTable.Columns.SmtpServerId, serverInformation.smtp_settings_id)
                .InColumnValue(MailboxTable.Columns.ServerId, serverInformation.imap_settings_id)
                .InColumnValue(MailboxTable.Columns.IsTeamlabMailbox, true)
                .Identity(0, 0, true);


            var result = db.ExecuteScalar<int>(insertQuery);

            var createdMailbox = new MailboxDto(result, teamlabAccount.ID.ToString(), tenant,
                                                 fullAddress, name);

            var addressDal = new MailAddressDal(tenant);

            var mailboxAddress = addressDal.AddMailboxAddress(createdMailbox.id, addressName, addressCreatedDate,
                                                    domainId, domainName, isVerified, db);

            var resultDto = new MailboxWithAddressDto(createdMailbox, mailboxAddress);

            return resultDto;
        }

        public void UpdateMailbox(IAccount teamlabAccount, int mailboxId, string name, DbManager db)
        {
            if (teamlabAccount == null)
                throw new ArgumentNullException("teamlabAccount");

            if (db == null)
                throw new ArgumentNullException("db");

            var updateQuery = new SqlUpdate(MailboxTable.Name)
                .Set(MailboxTable.Columns.AddressName, name)
                .Where(MailboxTable.Columns.Tenant, tenant)
                .Where(MailboxTable.Columns.User, teamlabAccount.ID.ToString())
                .Where(MailboxTable.Columns.Id, mailboxId);

            db.ExecuteNonQuery(updateQuery);
        }

        public List<MailboxWithAddressDto> GetMailboxes(DbManager db = null)
        {
            const string mailbox_alias = "mailbox";
            const string address_alias = "address";
            const string domain_alias = "domain";

            var query = GetMailboxQuery(mailbox_alias, domain_alias, address_alias)
                .Where(MailboxTable.Columns.Tenant.Prefix(mailbox_alias), tenant)
                .Where(MailboxTable.Columns.IsTeamlabMailbox.Prefix(mailbox_alias), true)
                .Where(MailboxTable.Columns.IsRemoved.Prefix(mailbox_alias), false);

            var result = NullSafeExecuteList(db, query);

            var mailboxWithAddressDtoList =
                result.ConvertAll(r => r.ToMailboxWithAddressDto());

            return mailboxWithAddressDtoList;
        }

        public MailboxWithAddressDto GetMailbox(int mailboxId, DbManager db = null)
        {
            if (mailboxId < 0)
                throw new ArgumentException("Argument mailbox_id less then zero.", "mailboxId");

            const string mailbox_alias = "mailbox";
            const string address_alias = "address";
            const string domain_alias = "domain";

            var query = GetMailboxQuery(mailbox_alias, domain_alias, address_alias)
                .Where(MailboxTable.Columns.Tenant.Prefix(mailbox_alias), tenant)
                .Where(MailboxTable.Columns.IsTeamlabMailbox.Prefix(mailbox_alias), true)
                .Where(MailboxTable.Columns.IsRemoved.Prefix(mailbox_alias), false)
                .Where(MailboxTable.Columns.Id.Prefix(mailbox_alias), mailboxId);

            var result = NullSafeExecuteList(db, query);

            var mailboxWithAddressDto =
                result.ConvertAll(r => r.ToMailboxWithAddressDto()).FirstOrDefault();

            return mailboxWithAddressDto;
        }

        private static SqlQuery GetMailboxQuery(string mailboxAlias, string domainAlias, string addressAlias)
        {
            if (string.IsNullOrEmpty(mailboxAlias))
                throw new ArgumentNullException("mailboxAlias");

            if (string.IsNullOrEmpty(domainAlias))
                throw new ArgumentNullException("domainAlias");

            if (string.IsNullOrEmpty(addressAlias))
                throw new ArgumentNullException("addressAlias");
            
            return new SqlQuery(MailboxTable.Name.Alias(mailboxAlias))
                .InnerJoin(AddressTable.Name.Alias(addressAlias),
                           Exp.EqColumns(MailboxTable.Columns.Id.Prefix(mailboxAlias),
                                         AddressTable.Columns.MailboxId.Prefix(addressAlias)))
                .InnerJoin(DomainTable.Name.Alias(domainAlias),
                           Exp.EqColumns(AddressTable.Columns.DomainId.Prefix(addressAlias),
                                         DomainTable.Columns.Id.Prefix(domainAlias))
                )
                .Select(MailboxTable.Columns.Id.Prefix(mailboxAlias))
                .Select(MailboxTable.Columns.User.Prefix(mailboxAlias))
                .Select(MailboxTable.Columns.Tenant.Prefix(mailboxAlias))
                .Select(MailboxTable.Columns.Address.Prefix(mailboxAlias))
                .Select(MailboxTable.Columns.AddressName.Prefix(mailboxAlias))
                .Select(MailboxTable.Columns.DateCreated.Prefix(mailboxAlias))
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

        public void DeleteMailbox(int mailboxId, DbManager db)
        {
            if (mailboxId < 0)
                throw new ArgumentException("Argument mailbox_id less then zero.", "mailboxId");
            
            if (db == null)
                throw new ArgumentNullException("db");

            var mailboxManager = GetMailboxManager();
            var mailbox = mailboxManager.GetServerMailBox(tenant, mailboxId, db);
            if (mailbox != null)
            {
                if (!mailbox.IsTeamlab)
                    throw new ArgumentException("Mailbox is not teamlab and it can't be deleted.");

                mailboxManager.RemoveMailBox(db, mailbox, false);
            }

            var deleteAddressesQuery = new SqlDelete(AddressTable.Name)
                .Where(AddressTable.Columns.MailboxId, mailboxId);

            db.ExecuteNonQuery(deleteAddressesQuery);
        }
    }
}
