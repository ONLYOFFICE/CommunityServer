/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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
using System.Linq;

using ASC.Api.Attributes;
using ASC.Mail.Data.Contracts;

// ReSharper disable InconsistentNaming

namespace ASC.Api.Mail
{
    public partial class MailApi
    {
        /// <summary>
        /// Returns a signature of a mailbox with the ID specified in the request.
        /// </summary>
        /// <short>Get a signature</short>
        /// <category>Signature</category>
        /// <param type="System.Int32, System" method="url" name="mailbox_id">Mailbox ID</param>
        /// <returns type="ASC.Mail.Data.Contracts.MailSignatureData, ASC.Mail">Signature object</returns>
        /// <path>api/2.0/mail/signature/{mailbox_id}</path>
        /// <httpMethod>GET</httpMethod>
        [Read(@"signature/{mailbox_id:[0-9]+}")]
        public MailSignatureData GetSignature(int mailbox_id)
        {
            var accounts = GetAccounts(Username);

            var account = accounts.FirstOrDefault(a => a.MailboxId == mailbox_id);

            if (account == null)
                throw new ArgumentException("Mailbox not found");

            return account.Signature;
        }

        /// <summary>
        /// Updates a signature of a mailbox with the ID specified in the request.
        /// </summary>
        /// <short>Update a signature</short>
        /// <category>Signature</category>
        /// <param type="System.Int32, System" method="url" name="mailbox_id">Mailbox ID</param>
        /// <param type="System.String, System" name="html">New signature value in the HTML format</param>
        /// <param type="System.Boolean, System" name="is_active">New signature status (active or not)</param>
        /// <httpMethod>POST</httpMethod>
        /// <path>api/2.0/mail/signature/update/{mailbox_id}</path>
        /// <returns type="ASC.Mail.Data.Contracts.MailSignatureData, ASC.Mail">Updated signature object</returns>
        [Create(@"signature/update/{mailbox_id:[0-9]+}")]
        public MailSignatureData UpdateSignature(int mailbox_id, string html, bool is_active)
        {
            var accounts = GetAccounts(Username);

            var account = accounts.FirstOrDefault(a => a.MailboxId == mailbox_id);

            if (account == null)
                throw new ArgumentException("Mailbox not found");

            return MailEngineFactory.SignatureEngine.SaveSignature(mailbox_id, html, is_active);
        }
    }
}
