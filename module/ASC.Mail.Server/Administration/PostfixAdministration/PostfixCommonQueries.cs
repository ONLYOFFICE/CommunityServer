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

using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Mail.Aggregator.Common.Extension;
using ASC.Mail.Server.PostfixAdministration.DbSchema;

namespace ASC.Mail.Server.PostfixAdministration
{
    internal static class PostfixCommonQueries
    {
        internal static SqlQuery GetAddressJoinedWithDomainQuery(string address_alias, string domain_alias)
        {
            return new SqlQuery(AliasTable.name + " " + address_alias)
                        .InnerJoin(DomainTable.name + " " + domain_alias,
                                   Exp.EqColumns(AliasTable.Columns.domain.Prefix(address_alias),
                                                 DomainTable.Columns.domain.Prefix(domain_alias)
                                       )
                        )
                        .Select(AliasTable.Columns.address.Prefix(address_alias))
                        .Select(AliasTable.Columns.redirect.Prefix(address_alias))
                        .Select(AliasTable.Columns.domain.Prefix(address_alias))
                        .Select(AliasTable.Columns.created.Prefix(address_alias))
                        .Select(AliasTable.Columns.modified.Prefix(address_alias))
                        .Select(AliasTable.Columns.active.Prefix(address_alias))
                        .Select(DomainTable.Columns.domain.Prefix(domain_alias))
                        .Select(DomainTable.Columns.description.Prefix(domain_alias))
                        .Select(DomainTable.Columns.aliases.Prefix(domain_alias))
                        .Select(DomainTable.Columns.mailboxes.Prefix(domain_alias))
                        .Select(DomainTable.Columns.maxquota.Prefix(domain_alias))
                        .Select(DomainTable.Columns.quota.Prefix(domain_alias))
                        .Select(DomainTable.Columns.transport.Prefix(domain_alias))
                        .Select(DomainTable.Columns.backupmx.Prefix(domain_alias))
                        .Select(DomainTable.Columns.created.Prefix(domain_alias))
                        .Select(DomainTable.Columns.modified.Prefix(domain_alias))
                        .Select(DomainTable.Columns.active.Prefix(domain_alias));
        }
    }
}
