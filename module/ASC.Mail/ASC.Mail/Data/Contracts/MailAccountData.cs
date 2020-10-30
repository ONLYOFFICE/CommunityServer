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
using System.Runtime.Serialization;

namespace ASC.Mail.Data.Contracts
{
    [Serializable]
    [DataContract(Namespace = "", Name = "Account")]
    public class MailAccountData
    {
        [DataMember(IsRequired = false, Name = "mailboxId")]
        public int MailboxId { get; set; }

        [DataMember(IsRequired = true, Name = "email")]
        public string Email { get; set; }

        [DataMember(IsRequired = true, Name = "enabled")]
        public bool Enabled { get; set; }

        [DataMember(IsRequired = true, Name = "name")]
        public string Name { get; set; }

        [DataMember(IsRequired = true, Name = "oAuthConnection")]
        public bool OAuthConnection { get; set; }

        [DataMember(IsRequired = true, Name = "signature")]
        public MailSignatureData Signature { get; set; }

        [DataMember(IsRequired = true, Name = "autoreply")]
        public MailAutoreplyData Autoreply { get; set; }

        [DataMember(IsRequired = true, Name = "eMailInFolder")]
        public string EMailInFolder { get; set; }

        [DataMember(IsRequired = true, Name = "quotaError")]
        public bool QuotaError { get; set; }

        [DataMember(IsRequired = true, Name = "authError")]
        public bool AuthError { get; set; }

        [DataMember(IsRequired = true, Name = "isGroup")]
        public bool IsGroup { get; set; }

        [DataMember(IsRequired = true, Name = "isAlias")]
        public bool IsAlias { get; set; }

        [DataMember(IsRequired = true, Name = "isTeamlabMailbox")]
        public bool IsTeamlabMailbox { get; set; }

        [DataMember(IsRequired = true, Name = "isDefault")]
        public bool IsDefault { get; set; }

        [DataMember(IsRequired = true, Name = "isSharedDomainMailbox")]
        public bool IsSharedDomainMailbox { get; set; }
    }
}
