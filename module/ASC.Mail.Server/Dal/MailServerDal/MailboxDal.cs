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
using ASC.Common.Security.Authentication;
using ASC.Mail.Aggregator.Common.Extension;
using ASC.Mail.Aggregator.Dal.DbSchema;
using ASC.Security.Cryptography;

namespace ASC.Mail.Server.Dal
{
    public class MailboxDal : DalBase
    {
        private readonly int _mailboxCountLimit = 2;
        
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

        public MailboxWithAddressDto CreateMailbox(IAccount teamlabAccount,
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

            var userTeamlabMailboxCountQuery = new SqlQuery(MailboxTable.name.Alias(mailbox_alias))
                .InnerJoin(AddressTable.name.Alias(address_alias),
                           Exp.EqColumns(MailboxTable.Columns.id.Prefix(mailbox_alias),
                                         AddressTable.Columns.id_mailbox.Prefix(address_alias)))
                .SelectCount()
                .Where(MailboxTable.Columns.is_teamlab_mailbox.Prefix(mailbox_alias), true)
                .Where(MailboxTable.Columns.is_removed.Prefix(mailbox_alias), false)
                .Where(MailboxTable.Columns.id_tenant.Prefix(mailbox_alias), tenant)
                .Where(MailboxTable.Columns.id_user.Prefix(mailbox_alias), teamlabAccount.ID.ToString())
                .Where(AddressTable.Columns.id_domain.Prefix(address_alias), domainId)
                .Where(AddressTable.Columns.is_alias.Prefix(address_alias), false);

            var userMailboxesCount = db.ExecuteScalar<int>(userTeamlabMailboxCountQuery);

            if (userMailboxesCount >= _mailboxCountLimit)
                throw new ArgumentOutOfRangeException("addressName", string.Format("Count of user's mailboxes must be less or equal {0}.", _mailboxCountLimit));

            var serverInformation = GetTenantServer(db);

            var dateCreated = DateTime.UtcNow.ToDbStyle();

            var insertQuery = new SqlInsert(MailboxTable.name)
                .InColumnValue(MailboxTable.Columns.id, 0)
                .InColumnValue(MailboxTable.Columns.id_tenant, tenant)
                .InColumnValue(MailboxTable.Columns.id_user, teamlabAccount.ID.ToString())
                .InColumnValue(MailboxTable.Columns.address, fullAddress)
                .InColumnValue(MailboxTable.Columns.name, teamlabAccount.Name)
                .InColumnValue(MailboxTable.Columns.password, InstanceCrypto.Encrypt(password))
                .InColumnValue(MailboxTable.Columns.msg_count_last, 0)
                .InColumnValue(MailboxTable.Columns.smtp_password, InstanceCrypto.Encrypt(password))
                .InColumnValue(MailboxTable.Columns.size_last, 0)
                .InColumnValue(MailboxTable.Columns.login_delay, Config.LoginDelayInSeconds)
                .InColumnValue(MailboxTable.Columns.enabled, true)
                .InColumnValue(MailboxTable.Columns.imap, true)
                .InColumnValue(MailboxTable.Columns.service_type, 0)
                .InColumnValue(MailboxTable.Columns.refresh_token, null)
                .InColumnValue(MailboxTable.Columns.date_created, dateCreated)
                .InColumnValue(MailboxTable.Columns.id_smtp_server, serverInformation.smtp_settings_id)
                .InColumnValue(MailboxTable.Columns.id_in_server, serverInformation.imap_settings_id)
                .InColumnValue(MailboxTable.Columns.is_teamlab_mailbox, true)
                .Identity(0, 0, true);


            var result = db.ExecuteScalar<int>(insertQuery);

            var createdMailbox = new MailboxDto(result, teamlabAccount.ID.ToString(), tenant,
                                                 fullAddress);

            var addressDal = new MailAddressDal(tenant);

            var mailboxAddress = addressDal.AddMailboxAddress(createdMailbox.id, addressName, addressCreatedDate,
                                                    domainId, domainName, isVerified, db);

            var resultDto = new MailboxWithAddressDto(createdMailbox, mailboxAddress);

            return resultDto;
        }

        public List<MailboxWithAddressDto> GetMailboxes(DbManager db = null)
        {
            const string mailbox_alias = "mailbox";
            const string address_alias = "address";
            const string domain_alias = "domain";

            var query = GetMailboxQuery(mailbox_alias, domain_alias, address_alias)
                .Where(MailboxTable.Columns.id_tenant.Prefix(mailbox_alias), tenant)
                .Where(MailboxTable.Columns.is_teamlab_mailbox.Prefix(mailbox_alias), true)
                .Where(MailboxTable.Columns.is_removed.Prefix(mailbox_alias), false);

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
                .Where(MailboxTable.Columns.id_tenant.Prefix(mailbox_alias), tenant)
                .Where(MailboxTable.Columns.is_teamlab_mailbox.Prefix(mailbox_alias), true)
                .Where(MailboxTable.Columns.is_removed.Prefix(mailbox_alias), false)
                .Where(MailboxTable.Columns.id.Prefix(mailbox_alias), mailboxId);

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
            
            return new SqlQuery(MailboxTable.name.Alias(mailboxAlias))
                .InnerJoin(AddressTable.name.Alias(addressAlias),
                           Exp.EqColumns(MailboxTable.Columns.id.Prefix(mailboxAlias),
                                         AddressTable.Columns.id_mailbox.Prefix(addressAlias)))
                .InnerJoin(DomainTable.name.Alias(domainAlias),
                           Exp.EqColumns(AddressTable.Columns.id_domain.Prefix(addressAlias),
                                         DomainTable.Columns.id.Prefix(domainAlias))
                )
                .Select(MailboxTable.Columns.id.Prefix(mailboxAlias))
                .Select(MailboxTable.Columns.id_user.Prefix(mailboxAlias))
                .Select(MailboxTable.Columns.id_tenant.Prefix(mailboxAlias))
                .Select(MailboxTable.Columns.address.Prefix(mailboxAlias))
                .Select(MailboxTable.Columns.date_created.Prefix(mailboxAlias))
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

                var quotaSizeForClean = mailboxManager.RemoveMailBox(mailbox, db);

                //TODO: Maybe cleaning of quota need to move into RemoveMailBox?
                mailboxManager.QuotaUsedDelete(mailbox.TenantId, quotaSizeForClean);
            }

            var deleteAddressesQuery = new SqlDelete(AddressTable.name)
                .Where(AddressTable.Columns.id_mailbox, mailboxId);

            db.ExecuteNonQuery(deleteAddressesQuery);
        }
    }
}
