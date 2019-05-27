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


using System.Collections.Generic;
using System.Linq;
using ASC.Common.Data.Sql.Expressions;
using ASC.Mail.Core.DbSchema.Tables;
using ASC.Mail.Extensions;

namespace ASC.Mail.Core.Dao.Expressions.Attachment
{
    public class ConcreteMessageAttachmentsExp : UserAttachmentsExp
    {
        private readonly int _mailId;
        private readonly List<int> _attachIds;
        private readonly bool? _onlyEmbedded;

        public ConcreteMessageAttachmentsExp(int mailId, int tenant, string user, List<int> attachIds = null,
            bool? isRemoved = false, bool? onlyEmbedded = false)
            : base(tenant, user, isRemoved)
        {
            _mailId = mailId;
            _attachIds = attachIds;
            _onlyEmbedded = onlyEmbedded;
        }

        public override Exp GetExpression()
        {
            var exp = base.GetExpression();

            exp = exp & Exp.Eq(AttachmentTable.Columns.MailId.Prefix(AttachmentTable.TABLE_NAME), _mailId);

            if (_attachIds != null && _attachIds.Any())
            {
                exp = exp & Exp.In(AttachmentTable.Columns.Id.Prefix(AttachmentTable.TABLE_NAME), _attachIds);
            }

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