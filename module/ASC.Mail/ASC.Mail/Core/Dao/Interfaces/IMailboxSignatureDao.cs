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
    public interface IMailboxSignatureDao
    {
        /// <summary>
        ///     Get signature by mailbox id.
        /// </summary>
        /// <param name="mailboxId"></param>
        /// <returns>signature</returns>
        MailboxSignature GetSignature(int mailboxId);

        /// <summary>
        ///     Get a list of signature.
        /// </summary>
        /// <param name="mailboxIds"></param>
        /// <returns>list of signature</returns>
        List<MailboxSignature> GetSignatures(List<int> mailboxIds);

        /// <summary>
        ///     Save or update signature
        /// </summary>
        /// <param name="signature"></param>
        /// <returns>count of affected rows: 1 - OK, 0 - Fail</returns>
        int SaveSignature(MailboxSignature signature);

        /// <summary>
        ///     Delete signature
        /// </summary>
        /// <param name="mailboxId">id</param>
        /// <returns>count of affected rows: 1 - OK, 0 - Fail</returns>
        int DeleteSignature(int mailboxId);
    }
}
