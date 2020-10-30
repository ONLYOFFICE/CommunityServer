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
using ASC.Common.Data.Sql.Expressions;
using ASC.Mail.Core.DbSchema.Tables;
using ASC.Mail.Extensions;

namespace ASC.Mail.Core.Dao.Expressions.Attachment
{
    public class ConcreteMessagesAttachmentsExp : UserAttachmentsExp
    {
        private readonly List<int> _mailIds;
        private readonly bool? _onlyEmbedded;

        public ConcreteMessagesAttachmentsExp(List<int> mailIds, int tenant, string user,
            bool? isRemoved = false, bool? onlyEmbedded = false)
            : base(tenant, user, isRemoved)
        {
            _mailIds = mailIds;
            _onlyEmbedded = onlyEmbedded;
        }

        public override Exp GetExpression()
        {
            var exp = base.GetExpression();

            exp = exp & Exp.In(AttachmentTable.Columns.MailId.Prefix(AttachmentTable.TABLE_NAME), _mailIds);

            if (!_onlyEmbedded.HasValue)
                return exp;

            if (_onlyEmbedded.Value)
            {
                exp = exp &
                      !Exp.Eq(AttachmentTable.Columns.ContentId.Prefix(AttachmentTable.TABLE_NAME),
                          null);
            }
            else
            {
                exp = exp &
                      Exp.Eq(AttachmentTable.Columns.ContentId.Prefix(AttachmentTable.TABLE_NAME),
                          null);
            }

            return exp;
        }
    }
}