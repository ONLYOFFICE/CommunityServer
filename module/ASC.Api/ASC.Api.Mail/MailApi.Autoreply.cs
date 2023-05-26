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

using ASC.Api.Attributes;
using ASC.Mail.Core.Engine;
using ASC.Mail.Data.Contracts;

namespace ASC.Api.Mail
{
    public partial class MailApi
    {
        /// <summary>
        /// Updates or creates an autoreply with the parameters specified in the request.
        /// </summary>
        /// <short>Update an autoreply</short>
        /// <category>Autoreply</category>
        /// <param type="System.Int32, System" method="url" name="mailboxId">Mailbox ID</param>
        /// <param type="System.Boolean, System" name="turnOn">New autoreply status</param>
        /// <param type="System.Boolean, System" name="onlyContacts">Specifies whether to send an autoreply only to the contacts or not</param>
        /// <param type="System.Boolean, System" name="turnOnToDate">Specifies whether to send an autoreply till the specified date or not</param>
        /// <param type="System.DateTime, System" name="fromDate">New start date of autoreply sending</param>
        /// <param type="System.DateTime, System" name="toDate">New end date of autoreply sending</param>
        /// <param type="System.String, System" name="subject">New autoreply subject</param>
        /// <param type="System.String, System" name="html">New autoreply contents in the HTML format</param>
        /// <path>api/2.0/mail/autoreply/update/{mailboxId}</path>
        /// <httpMethod>POST</httpMethod>
        /// <returns type="ASC.Mail.Data.Contracts.MailAutoreplyData, ASC.Mail">Updated autoreply information</returns>
        [Create(@"autoreply/update/{mailboxId:[0-9]+}")]
        public MailAutoreplyData UpdateAutoreply(int mailboxId, bool turnOn, bool onlyContacts,
            bool turnOnToDate, DateTime fromDate, DateTime toDate, string subject, string html)
        {
            var result = MailEngineFactory
                .AutoreplyEngine
                .SaveAutoreply(mailboxId, turnOn, onlyContacts, turnOnToDate, fromDate, toDate, subject, html);

            CacheEngine.Clear(Username);

            return result;
        }
    }
}
