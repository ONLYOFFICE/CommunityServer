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

namespace ASC.Mail.Core.Dao.Expressions.Conversation
{
    public class ConversationsExpBuilder
    {
        private readonly SimpleConversationsExp _exp;

        public ConversationsExpBuilder(int tenant, string user)
        {
            _exp = new SimpleConversationsExp(tenant, user);
        }

        public ConversationsExpBuilder SetFoldersIds(List<int> foldersIds)
        {
            _exp.FoldersIds = foldersIds;
            return this;
        }

        public ConversationsExpBuilder SetChainIds(List<string> chainIds)
        {
            _exp.ChainIds = chainIds;
            return this;
        }

        public ConversationsExpBuilder SetFolder(int folder)
        {
            _exp.Folder = folder;
            return this;
        }

        public ConversationsExpBuilder SetMailboxId(int mailboxId)
        {
            _exp.MailboxId = mailboxId;
            return this;
        }

        public ConversationsExpBuilder SetChainId(string chainId)
        {
            _exp.ChainId = chainId;
            return this;
        }

        public ConversationsExpBuilder SetUnread(bool unread)
        {
            _exp.Unread = unread;
            return this;
        }

        public SimpleConversationsExp Build()
        {
            return _exp;
        }

        public static implicit operator SimpleConversationsExp(ConversationsExpBuilder builder)
        {
            return builder._exp;
        }
    }
}
