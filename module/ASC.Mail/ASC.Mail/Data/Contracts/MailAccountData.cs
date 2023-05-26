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
using System.Runtime.Serialization;

namespace ASC.Mail.Data.Contracts
{
    [Serializable]
    [DataContract(Namespace = "", Name = "Account")]
    public class MailAccountData
    {
        ///<example name="mailboxId" type="int">12</example>
        [DataMember(IsRequired = false, Name = "mailboxId")]
        public int MailboxId { get; set; }

        ///<example name="email">email@only.com</example>
        [DataMember(IsRequired = true, Name = "email")]
        public string Email { get; set; }

        ///<example name="enabled">true</example>
        [DataMember(IsRequired = true, Name = "enabled")]
        public bool Enabled { get; set; }

        ///<example name="name">name</example>
        [DataMember(IsRequired = true, Name = "name")]
        public string Name { get; set; }

        ///<example name="oAuthConnection">true</example>
        [DataMember(IsRequired = true, Name = "oAuthConnection")]
        public bool OAuthConnection { get; set; }

        ///<type name="signature">ASC.Mail.Data.Contracts.MailSignatureData, ASC.Mail</type>
        [DataMember(IsRequired = true, Name = "signature")]
        public MailSignatureData Signature { get; set; }

        ///<type name="autoreply">ASC.Mail.Data.Contracts.MailAutoreplyData, ASC.Mail</type>
        [DataMember(IsRequired = true, Name = "autoreply")]
        public MailAutoreplyData Autoreply { get; set; }

        ///<example name="eMailInFolder">eMailInFolder</example>
        [DataMember(IsRequired = true, Name = "eMailInFolder")]
        public string EMailInFolder { get; set; }

        ///<example name="quotaError">false</example>
        [DataMember(IsRequired = true, Name = "quotaError")]
        public bool QuotaError { get; set; }

        ///<example name="authError">false</example>
        [DataMember(IsRequired = true, Name = "authError")]
        public bool AuthError { get; set; }

        ///<example name="isGroup">true</example>
        [DataMember(IsRequired = true, Name = "isGroup")]
        public bool IsGroup { get; set; }

        ///<example name="isAlias">true</example>
        [DataMember(IsRequired = true, Name = "isAlias")]
        public bool IsAlias { get; set; }

        ///<example name="isTeamlabMailbox">true</example>
        [DataMember(IsRequired = true, Name = "isTeamlabMailbox")]
        public bool IsTeamlabMailbox { get; set; }

        ///<example name="isDefault">true</example>
        [DataMember(IsRequired = true, Name = "isDefault")]
        public bool IsDefault { get; set; }

        ///<example name="isSharedDomainMailbox">true</example>
        [DataMember(IsRequired = true, Name = "isSharedDomainMailbox")]
        public bool IsSharedDomainMailbox { get; set; }

        [DataMember(IsRequired = true, Name = "dateCreated")]
        public DateTime? DateCreated { get; set; }
    }
}
