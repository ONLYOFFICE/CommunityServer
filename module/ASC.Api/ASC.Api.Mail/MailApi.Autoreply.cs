/*
 *
 * (c) Copyright Ascensio System Limited 2010-2021
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
        /// <param name="mailboxId">Mailbox ID</param>
        /// <param name="turnOn">New autoreply status</param>
        /// <param name="onlyContacts">If true, then send autoreply only to contacts</param>
        /// <param name="turnOnToDate">If true, then the "toDate" field is active</param>
        /// <param name="fromDate">New start date of autoreply sending</param>
        /// <param name="toDate">New end date of autoreply sending</param>
        /// <param name="subject">New autoreply subject</param>
        /// <param name="html">New autoreply value in the HTML format</param>
        /// <returns>Updated autoreply information</returns>
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
