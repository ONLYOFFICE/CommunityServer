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


using System;
using ASC.Common.Data.Sql.Expressions;
using ASC.Mail.Core.DbSchema.Tables;
using ASC.Mail.Data.Contracts;
using ASC.Mail.Extensions;

namespace ASC.Mail.Core.Dao.Expressions.Message
{
    public class FilterNextMessageExp : FilterMessagesExp
    {
        public DateTime DateSent { get; private set; }

        public FilterNextMessageExp(DateTime dateSent, int tenant, string user, MailSearchFilterData filter)
            : base(null, tenant, user, filter)
        {
            DateSent = dateSent;
        }

        private const string MM_ALIAS = "mm";

        public override Exp GetExpression()
        {
            var exp = base.GetExpression();

            exp &= OrderAsc != null && OrderAsc.Value
                ? Exp.Ge(MailTable.Columns.DateSent.Prefix(MM_ALIAS), DateSent)
                : Exp.Le(MailTable.Columns.DateSent.Prefix(MM_ALIAS), DateSent);

            return exp;
        }
    }
}