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
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Mail.Core.Dao.Interfaces;
using ASC.Mail.Core.DbSchema.Tables;
using ASC.Mail.Core.Entities;
using ASC.Mail.Extensions;

namespace ASC.Mail.Core.Dao
{
    public class AccountDao : IAccountDao
    {
        public IDbManager Db { get; private set; }

        public int Tenant { get; private set; }
        public string User { get; private set; }

        public AccountDao(IDbManager dbManager, int tenant, string user)
        {
            Db = dbManager;

            Tenant = tenant;
            User = user;
        }

        public List<Account> GetAccounts()
        {
            const string mailbox_alias = "ma";
            const string server_address = "sa";
            const string server_domain = "sd";
            const string group_x_address = "ga";
            const string server_group = "sg";

            var query = new SqlQuery(MailboxTable.TABLE_NAME.Alias(mailbox_alias))
                .LeftOuterJoin(ServerAddressTable.TABLE_NAME.Alias(server_address),
                    Exp.EqColumns(MailboxTable.Columns.Id.Prefix(mailbox_alias),
                        ServerAddressTable.Columns.MailboxId.Prefix(server_address)))
                .LeftOuterJoin(ServerDomainTable.TABLE_NAME.Alias(server_domain),
                    Exp.EqColumns(ServerAddressTable.Columns.DomainId.Prefix(server_address),
                        ServerDomainTable.Columns.Id.Prefix(server_domain)))
                .LeftOuterJoin(ServerMailGroupXAddressesTable.TABLE_NAME.Alias(group_x_address),
                    Exp.EqColumns(ServerAddressTable.Columns.Id.Prefix(server_address),
                        ServerMailGroupXAddressesTable.Columns.AddressId.Prefix(group_x_address)))
                .LeftOuterJoin(ServerMailGroupTable.TABLE_NAME.Alias(server_group),
                    Exp.EqColumns(ServerMailGroupXAddressesTable.Columns.MailGroupId.Prefix(group_x_address),
                        ServerMailGroupTable.Columns.Id.Prefix(server_group)))
                .Select(MailboxTable.Columns.Id.Prefix(mailbox_alias))
                .Select(MailboxTable.Columns.Address.Prefix(mailbox_alias))
                .Select(MailboxTable.Columns.Enabled.Prefix(mailbox_alias))
                .Select(MailboxTable.Columns.Name.Prefix(mailbox_alias))
                .Select(MailboxTable.Columns.QuotaError.Prefix(mailbox_alias))
                .Select(MailboxTable.Columns.DateAuthError.Prefix(mailbox_alias))
                .Select(MailboxTable.Columns.OAuthToken.Prefix(mailbox_alias))
                .Select(MailboxTable.Columns.IsServerMailbox.Prefix(mailbox_alias))
                .Select(MailboxTable.Columns.EmailInFolder.Prefix(mailbox_alias))
                .Select(ServerAddressTable.Columns.Id.Prefix(server_address))
                .Select(ServerAddressTable.Columns.AddressName.Prefix(server_address))
                .Select(ServerAddressTable.Columns.IsAlias.Prefix(server_address))
                .Select(ServerDomainTable.Columns.Id.Prefix(server_domain))
                .Select(ServerDomainTable.Columns.DomainName.Prefix(server_domain))
                .Select(ServerMailGroupTable.Columns.Id.Prefix(server_group))
                .Select(ServerMailGroupTable.Columns.Address.Prefix(server_group))
                .Select(ServerDomainTable.Columns.Tenant.Prefix(server_domain))
                .Where(MailboxTable.Columns.IsRemoved.Prefix(mailbox_alias), false)
                .Where(MailboxTable.Columns.Tenant.Prefix(mailbox_alias), Tenant)
                .Where(MailboxTable.Columns.User.Prefix(mailbox_alias), User)
                .OrderBy(ServerAddressTable.Columns.IsAlias.Prefix(server_address), true);

            return Db.ExecuteList(query)
                .ConvertAll(ToAccount);
        }

        private static Account ToAccount(object[] r)
        {
            var a = new Account
            {
                MailboxId = Convert.ToInt32(r[0]),
                MailboxAddress = Convert.ToString(r[1]),
                MailboxEnabled = Convert.ToBoolean(r[2]),
                MailboxAddressName = Convert.ToString(r[3]),
                MailboxQuotaError = Convert.ToBoolean(r[4]),
                MailboxDateAuthError = r[5] != null ? Convert.ToDateTime(r[5]) : (DateTime?) null,
                MailboxOAuthToken = Convert.ToString(r[6]),
                MailboxIsTeamlabMailbox = Convert.ToBoolean(r[7]),
                MailboxEmailInFolder = Convert.ToString(r[8]),
                ServerAddressId = Convert.ToInt32(r[9]),
                ServerAddressName = Convert.ToString(r[10]),
                ServerAddressIsAlias = Convert.ToBoolean(r[11]),
                ServerDomainId = Convert.ToInt32(r[12]),
                ServerDomainName = Convert.ToString(r[13]),
                ServerMailGroupId = Convert.ToInt32(r[14]),
                ServerMailGroupAddress = Convert.ToString(r[15]),
                ServerDomainTenant = Convert.ToInt32(r[16])
            };

            return a;
        }
    }
}