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
using System.Data;
using System.Linq;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Mail.Server.Administration.Interfaces;
using ASC.Mail.Server.Administration.ServerModel;
using ASC.Mail.Server.Administration.ServerModel.Base;
using ASC.Mail.Server.PostfixAdministration.DbSchema;
using ASC.Mail.Server.Utils;

namespace ASC.Mail.Server.PostfixAdministration
{
    class PostfixMailGroup : MailGroupModel
    {
        public PostfixMailGroup(int id, int tenant, IMailAddress address, List<IMailAddress> in_addresses, MailServerBase server) 
            : base(id, tenant, address, in_addresses, server)
        {
        }

        protected override void _AddMember(MailAddressBase address)
        {
            using (var db = GetDb())
            {
                var members = _GetMembers(db);

                if (members.Any(a => a.name == address.ToString()))
                    throw new DuplicateNameException("You want to add already existed address");

                var members_addresses = members.Select(m => m.name).ToList();
                members_addresses.Add(address.ToString());

                UpdateGroupMembers(db, members_addresses);
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
                var dto_members = _GetMembers(db);
                return dto_members.Select(member => member.ToPostfixAddress()).Cast<MailAddressBase>().ToList();
            }
        }

        private ICollection<PostfixMailAddressDto> _GetMembers(IDbManager db)
        {
            var members = new List<PostfixMailAddressDto>();

            //Todo: Think about join this two queries
            var query_for_joined_group_address_selection = new SqlQuery(AliasTable.name)
                        .Select(AliasTable.Columns.redirect)
                        .Where(AliasTable.Columns.address, Address.ToString());

            var mail_group_addresses = db.ExecuteScalar<string>(query_for_joined_group_address_selection);
            if (null == mail_group_addresses)
                return members;

            var needed_group_addresses = mail_group_addresses.Split(',');
            const string address_alias = "msa";
            var group_addresses_query = PostfixCommonQueries.GetAddressJoinedWithDomainQuery(address_alias, "msd")
// ReSharper disable CoVariantArrayConversion
                    .Where(Exp.In(AliasTable.Columns.address, needed_group_addresses));
// ReSharper restore CoVariantArrayConversion

            var adress_records = db.ExecuteList(group_addresses_query);
            foreach (var adress_record in adress_records)
            {
                var address = adress_record.SubArray(0, ToDtoConverters.mail_address_columns_count).ToAddressDto();
                var domain = adress_record.SubArray(ToDtoConverters.mail_address_columns_count, ToDtoConverters.domain_columns_count).ToWebDomainDto();
                address.Domain = domain;
                members.Add(address);
            }

            return members;
        }

        private IDbManager GetDb()
        {
            var db_provider = new PostfixAdminDbManager(Server.Id, Server.ConnectionString);
            return db_provider.GetAdminDb();
        }

        private void UpdateGroupMembers(IDbManager db, IEnumerable<string> members_addresses)
        {
            var members_string = members_addresses.Aggregate<string, string>("",
                    (current, member) => current + (member + ","));

            var update_group_members = new SqlUpdate(AliasTable.name)
                .Set(AliasTable.Columns.redirect, members_string)
                .Set(AliasTable.Columns.modified, DateTime.UtcNow.ToDbStyle())
                .Where(AliasTable.Columns.address, Address.ToString());

            db.ExecuteNonQuery(update_group_members);
        }
    }
}
