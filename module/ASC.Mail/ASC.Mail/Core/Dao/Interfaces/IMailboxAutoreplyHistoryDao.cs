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
using ASC.Mail.Core.Entities;

namespace ASC.Mail.Core.Dao.Interfaces
{
    public interface IMailboxAutoreplyHistoryDao
    {
        /// <summary>
        ///     Get autoreplyHistory emails by mailbox id.
        /// </summary>
        /// <param name="mailboxId"></param>
        /// <param name="email"></param>
        /// <param name="autoreplyDaysInterval"></param>
        /// <returns>list of emails</returns>
        List<string> GetAutoreplyHistorySentEmails(int mailboxId, string email, int autoreplyDaysInterval);

        /// <summary>
        ///     Save or update autoreplyHistory
        /// </summary>
        /// <param name="autoreplyHistory"></param>
        /// <returns>count of affected rows: 1 - OK, 0 - Fail</returns>
        int SaveAutoreplyHistory(MailboxAutoreplyHistory autoreplyHistory);

        /// <summary>
        ///     Delete autoreplyHistory
        /// </summary>
        /// <param name="mailboxId">id</param>
        /// <returns>count of affected rows: 1 - OK, 0 - Fail</returns>
        int DeleteAutoreplyHistory(int mailboxId);
    }
}
