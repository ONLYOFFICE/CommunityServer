/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
using ASC.Common.Data.Sql.Expressions;

namespace ASC.Mail.Core.Dao.Expressions.Message
{
    public class MessagesExpBuilder
    {
        private readonly SimpleMessagesExp _exp;

        public MessagesExpBuilder(int tenant)
        {
            _exp = new SimpleMessagesExp(tenant);
        }

        public MessagesExpBuilder(int tenant, string user, bool? isRemoved = false)
        {
            _exp = new SimpleMessagesExp(tenant, user, isRemoved);
        }

        public MessagesExpBuilder SetMessageId(int messageId)
        {
            _exp.MessageId = messageId;
            return this;
        }

        public MessagesExpBuilder SetFolder(int folder)
        {
            _exp.Folder = folder;
            return this;
        }

        public MessagesExpBuilder SetMailboxId(int mailboxId)
        {
            _exp.MailboxId = mailboxId;
            return this;
        }

        public MessagesExpBuilder SetChainId(string chainId)
        {
            _exp.ChainId = chainId;
            return this;
        }

        public MessagesExpBuilder SetMd5(string md5)
        {
            _exp.Md5 = md5;
            return this;
        }

        public MessagesExpBuilder SetMimeMessageId(string mimeMessageId)
        {
            _exp.MimeMessageId = mimeMessageId;
            return this;
        }

        public MessagesExpBuilder SetMessageIds(List<int> messageIds)
        {
            _exp.MessageIds = messageIds;
            return this;
        }

        public MessagesExpBuilder SetFoldersIds(List<int> foldersIds)
        {
            _exp.FoldersIds = foldersIds;
            return this;
        }

        public MessagesExpBuilder SetChainIds(List<string> chainIds)
        {
            _exp.ChainIds = chainIds;
            return this;
        }

        public MessagesExpBuilder SetTagIds(List<int> tagIds)
        {
            _exp.TagIds = tagIds;
            return this;
        }

        public MessagesExpBuilder SetOrderBy(string orderBy)
        {
            _exp.OrderBy = orderBy;
            return this;
        }

        public MessagesExpBuilder SetOrderAsc(bool orderAsc)
        {
            _exp.OrderAsc = orderAsc;
            return this;
        }

        public MessagesExpBuilder SetStartIndex(int startIndex)
        {
            _exp.StartIndex = startIndex;
            return this;
        }

        public MessagesExpBuilder SetLimit(int limit)
        {
            _exp.Limit = limit;
            return this;
        }

        public MessagesExpBuilder SetUnread(bool unread)
        {
            _exp.Unread = unread;
            return this;
        }

        public MessagesExpBuilder SetExp(Exp exp)
        {
            _exp.Exp = exp;
            return this;
        }

        public MessagesExpBuilder SetUserFolderId(int userFolderId)
        {
            _exp.UserFolderId = userFolderId;
            return this;
        }

        public SimpleMessagesExp Build()
        {
            return _exp;
        }

        public static implicit operator SimpleMessagesExp(MessagesExpBuilder builder)
        {
            return builder._exp;
        }
    }
}
