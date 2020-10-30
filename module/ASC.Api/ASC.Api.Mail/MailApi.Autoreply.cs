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


using ASC.Api.Attributes;
using System;
using ASC.Mail.Core.Engine;
using ASC.Mail.Data.Contracts;

namespace ASC.Api.Mail
{
    public partial class MailApi
    {
        /// <summary>
        /// This method needed for update or create autoreply.
        /// </summary>
        /// <param name="mailboxId">Id of updated mailbox.</param>
        /// <param name="turnOn">New autoreply status.</param>
        /// <param name="onlyContacts">If true then send autoreply only for contacts.</param>
        /// <param name="turnOnToDate">If true then field To is active.</param>
        /// <param name="fromDate">Start date of autoreply sending.</param>
        /// <param name="toDate">End date of autoreply sending.</param>
        /// <param name="subject">New autoreply subject.</param>
        /// <param name="html">New autoreply value.</param>
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
