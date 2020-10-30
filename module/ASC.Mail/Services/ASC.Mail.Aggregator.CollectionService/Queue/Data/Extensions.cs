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
using ASC.Mail.Data.Contracts;

namespace ASC.Mail.Aggregator.CollectionService.Queue.Data
{
    public static class Extensions
    {
        public static MailBoxData ToMailbox(this MailboxData mailboxData)
        {
            return new MailBoxData
            {
                TenantId = mailboxData.TenantId,
                UserId = mailboxData.UserId,
                MailBoxId = mailboxData.MailboxId,
                EMail = new MailAddress(mailboxData.EMail),
                Imap = mailboxData.Imap,
                IsTeamlab = mailboxData.IsTeamlab,
                Size = mailboxData.Size,
                MessagesCount = mailboxData.MessagesCount
            };
        }

        public static MailboxData ToMailboxData(this MailBoxData mailbox)
        {
            return new MailboxData
            {
                MailboxId = mailbox.MailBoxId,
                TenantId = mailbox.TenantId,
                UserId = mailbox.UserId,
                EMail = mailbox.EMail.Address,
                Imap = mailbox.Imap,
                IsTeamlab = mailbox.IsTeamlab,
                MessagesCount = mailbox.MessagesCount,
                Size = mailbox.Size
            };
        }
    }
}
