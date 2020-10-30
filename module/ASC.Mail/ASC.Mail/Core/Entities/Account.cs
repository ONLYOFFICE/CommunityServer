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

namespace ASC.Mail.Core.Entities
{
    public class Account
    {
        public int MailboxId { get; set; }
        public string MailboxAddress { get; set; }
        public bool MailboxEnabled { get; set; }
        public string MailboxAddressName { get; set; }
        public bool MailboxQuotaError { get; set; }
        public DateTime? MailboxDateAuthError { get; set; }
        public string MailboxOAuthToken { get; set; }
        public bool MailboxIsTeamlabMailbox { get; set; }
        public string MailboxEmailInFolder { get; set; }
        public int ServerAddressId { get; set; }
        public string ServerAddressName { get; set; }
        public bool ServerAddressIsAlias { get; set; }
        public int ServerDomainId { get; set; }
        public string ServerDomainName { get; set; }
        public int ServerMailGroupId { get; set; }
        public string ServerMailGroupAddress { get; set; }
        public int ServerDomainTenant { get; set; }
    }
}
