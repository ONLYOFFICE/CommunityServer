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
using System.Data;
using System.Linq;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Mail.Aggregator.Common.Extension;
using ASC.Mail.Server.Administration.Interfaces;
using ASC.Mail.Server.Administration.ServerModel;
using ASC.Mail.Server.Administration.ServerModel.Base;
using ASC.Mail.Server.PostfixAdministration.DbSchema;
using ASC.Mail.Server.Utils;

namespace ASC.Mail.Server.PostfixAdministration
{
    class PostfixMailGroup : MailGroupModel
    {
        public PostfixMailGroup(int id, int tenant, IMailAddress address, List<IMailAddress> inAddresses, MailServerBase server) 
            : base(id, tenant, address, inAddresses, server)
        {
        }

        protected override void _AddMember(MailAddressBase address)
        {
            using (var db = GetDb())
            {
                var members = _GetMembers(db);

                if (members.Any(a => a.name == address.ToString()))
                    throw new DuplicateNameException("You want to add already existed address");

                var membersAddresses = members.Select(m => m.name).ToList();
                membersAddresses.Add(address.ToString());

                UpdateGroupMembers(db, membersAddresses);
            }
        }

        protected override void _RemoveMember(MailAddressBase address)
        {
            using (var db = GetDb())
            {
                var members = _GetMembers(db);

                if (members.All(a => a.name != address.ToString()))
                    throw new ArgumentException("You want to remove nonexistent address");

                UpdateGroupMembers(db, members.Select(m => m.name).Where(m => m != address.ToString()));
            }
        }

        protected override ICollection<MailAddressBase> _GetMembers()
        {
            using (var db = GetDb())
            {
                var dtoMembers = _GetMembers(db);
                return dtoMembers.Select(member => member.ToPostfixAddress()).ToList();
            }
        }

        private ICollection<PostfixMailAddressDto> _GetMembers(IDbManager db)
        {
            var members = new List<PostfixMailAddressDto>();

            //Todo: Think about join this two queries
            var queryForJoinedGroupAddressSelection = new SqlQuery(AliasTable.name)
                        .Select(AliasTable.Columns.redirect)
                        .Where(AliasTable.Columns.address, Address.ToString());

            var mailGroupAddresses = db.ExecuteScalar<string>(queryForJoinedGroupAddressSelection);
            if (null == mailGroupAddresses)
                return members;

            var neededGroupAddresses = mailGroupAddresses.Split(',');
            const string address_alias = "msa";
            var groupAddressesQuery = PostfixCommonQueries.GetAddressJoinedWithDomainQuery(address_alias, "msd")
// ReSharper disable CoVariantArrayConversion
                    .Where(Exp.In(AliasTable.Columns.address, neededGroupAddresses));
// ReSharper restore CoVariantArrayConversion

            var adressRecords = db.ExecuteList(groupAddressesQuery);
            foreach (var adressRecord in adressRecords)
            {
                var address = adressRecord.SubArray(0, ToDtoConverters.MAIL_ADDRESS_COLUMNS_COUNT).ToAddressDto();
                var domain = adressRecord.SubArray(ToDtoConverters.MAIL_ADDRESS_COLUMNS_COUNT, ToDtoConverters.DOMAIN_COLUMNS_COUNT).ToWebDomainDto();
                address.Domain = domain;
                members.Add(address);
            }

            return members;
        }

        private IDbManager GetDb()
        {
            var dbProvider = new PostfixAdminDbManager(Server.Id, Server.ConnectionString);
            return dbProvider.GetAdminDb();
        }

        private void UpdateGroupMembers(IDbManager db, IEnumerable<string> membersAddresses)
        {
            var membersString = membersAddresses.Aggregate("",
                    (current, member) => current + (member + ","));

            var updateGroupMembers = new SqlUpdate(AliasTable.name)
                .Set(AliasTable.Columns.redirect, membersString)
                .Set(AliasTable.Columns.modified, DateTime.UtcNow)
                .Where(AliasTable.Columns.address, Address.ToString());

            db.ExecuteNonQuery(updateGroupMembers);
        }
    }
}
