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


using System.Net.Mail;
using ASC.Common.Data.Sql.Expressions;
using ASC.Mail.Core.DbSchema.Tables;

namespace ASC.Mail.Core.Dao.Expressions.Mailbox
{
    public class ConcreteUserServerMailboxExp : СoncreteUserMailboxExp
    {
        public ConcreteUserServerMailboxExp(int id, int tenant, string user, bool? isRemoved = null)
            : base(id, tenant, user, isRemoved)
        {
        }

        public ConcreteUserServerMailboxExp(MailAddress address, int tenant, string user, bool? isRemoved = null)
            : base(address, tenant, user, isRemoved)
        {
        }

        public override Exp GetExpression()
        {
            var exp = base.GetExpression();

            exp = exp & Exp.Eq(MailboxTable.Columns.IsServerMailbox, true);

            return exp;
        }
    }
}