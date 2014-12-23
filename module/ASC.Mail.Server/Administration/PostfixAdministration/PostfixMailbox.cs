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

using System.Collections.Generic;
using ASC.Common.Data.Sql;
using ASC.Mail.Server.Administration.Interfaces;
using ASC.Mail.Server.Administration.ServerModel;
using ASC.Mail.Server.Administration.ServerModel.Base;
using ASC.Mail.Server.PostfixAdministration.DbSchema;

namespace ASC.Mail.Server.PostfixAdministration
{
    class PostfixMailbox : MailboxModel
    {
        public PostfixMailbox(int id, int tenant, IMailAddress address, IMailAccount account, List<IMailAddress> aliases, MailServerBase server) 
            : base(id, tenant, address, account, aliases, server)
        {
        }

        protected override void _AddAlias(MailAddressBase alias_to_add)
        {
            var insert_mailbox_alias = new SqlInsert(AliasTable.name)
                .InColumnValue(AliasTable.Columns.address, alias_to_add.ToString())
                .InColumnValue(AliasTable.Columns.redirect, Address.ToString())
                .InColumnValue(AliasTable.Columns.domain, alias_to_add.Domain.Name)
                .InColumnValue(AliasTable.Columns.created, alias_to_add.DateCreated)
                .InColumnValue(AliasTable.Columns.modified, alias_to_add.DateCreated)
                .InColumnValue(AliasTable.Columns.active, true)
                .InColumnValue(AliasTable.Columns.is_group, false);

            var db_manager = new PostfixAdminDbManager(Server.Id, Server.ConnectionString);
            using (var db = db_manager.GetAdminDb())
            {
                 db.ExecuteNonQuery(insert_mailbox_alias);
            }
        }

        protected override void _RemoveAlias(MailAddressBase alias_to_remove)
        {
            var remove_mailbox_alias = new SqlDelete(AliasTable.name)
                .Where(AliasTable.Columns.address, alias_to_remove.ToString());

            var db_manager = new PostfixAdminDbManager(Server.Id, Server.ConnectionString);
            using (var db = db_manager.GetAdminDb())
            {
                db.ExecuteNonQuery(remove_mailbox_alias);
            }
        }
    }
}
