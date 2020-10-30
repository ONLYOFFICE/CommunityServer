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

        public MessagesExpBuilder SetSubject(string subject)
        {
            _exp.Subject = subject;
            return this;
        }

        public MessagesExpBuilder SetDateSent(DateTime dateSent)
        {
            _exp.DateSent = dateSent;
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
