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


using ASC.Common.Data.Sql.Expressions;
using ASC.Mail.Core.DbSchema.Tables;

namespace ASC.Mail.Core.Dao.Expressions.Mailbox
{
    public class SimpleMailboxesExp : IMailboxesExp
    {
        private readonly bool? _isRemoved;
        public string OrderBy { get; private set; }
        public bool? OrderAsc { get; private set; }
        public int? Limit { get; private set; }

        public SimpleMailboxesExp(bool? isRemoved = false)
        {
            _isRemoved = isRemoved;
            OrderBy = null;
            OrderAsc = false;
            Limit = null;
        }

        public virtual Exp GetExpression()
        {
            if (!_isRemoved.HasValue)
                return Exp.Empty;

            var exp = Exp.Eq(MailboxTable.Columns.IsRemoved, _isRemoved);
            return exp;
        }
    }
}