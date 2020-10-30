/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


using System.Collections.Generic;
using System.Linq;
using ASC.Common.Data.Sql.Expressions;
using ASC.Mail.Core.DbSchema.Tables;

namespace ASC.Mail.Core.Dao.Expressions.Conversation
{
    public class SimpleConversationsExp : IConversationsExp
    {
        public int Tenant { get; private set; }
        public string User { get; private set; }

        public List<int> FoldersIds { get; set; }
        public List<string> ChainIds { get; set; }

        public int? Folder { get; set; }
        public int? MailboxId { get; set; }
        public bool? Unread { get; set; }
        public string ChainId { get; set; }

        public SimpleConversationsExp(int tenant, string user)
        {
            Tenant = tenant;
            User = user;
        }

        public static ConversationsExpBuilder CreateBuilder(int tenant, string user)
        {
            return new ConversationsExpBuilder(tenant, user);
        }
        
        public Exp GetExpression()
        {
            var exp = Exp.Eq(ChainTable.Columns.Tenant, Tenant);

            if (!string.IsNullOrEmpty(User))
            {
                exp &= Exp.Eq(ChainTable.Columns.User, User);
            }

            if (FoldersIds != null && FoldersIds.Any())
            {
                exp &= Exp.In(ChainTable.Columns.Folder, FoldersIds.ToArray());
            }

            if (Folder.HasValue)
            {
                exp &= Exp.Eq(ChainTable.Columns.Folder, Folder.Value);
            }

            if (ChainIds != null && ChainIds.Any())
            {
                exp &= Exp.In(ChainTable.Columns.Id, ChainIds);
            }

            if (MailboxId.HasValue)
            {
                exp &= Exp.Eq(ChainTable.Columns.MailboxId, MailboxId.Value);
            }

            if (!string.IsNullOrEmpty(ChainId))
            {
                exp &= Exp.Eq(ChainTable.Columns.Id, ChainId);
            }

            if (Unread.HasValue)
            {
                exp &= Exp.Eq(ChainTable.Columns.Unread, Unread.Value);
            }

            return exp;
        }
    }
}