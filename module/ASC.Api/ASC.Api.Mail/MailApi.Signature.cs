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
using System.Linq;
using ASC.Api.Attributes;
using ASC.Mail.Data.Contracts;

// ReSharper disable InconsistentNaming

namespace ASC.Api.Mail
{
    public partial class MailApi
    {
        /// <summary>
        /// This method needed for getting mailbox signature.
        /// </summary>
        /// <param name="mailbox_id"></param>
        /// <returns>Signature object</returns>
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
        /// This method needed for update or create signature.
        /// </summary>
        /// <param name="mailbox_id">Id of updated mailbox.</param>
        /// <param name="html">New signature value.</param>
        /// <param name="is_active">New signature status.</param>
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
