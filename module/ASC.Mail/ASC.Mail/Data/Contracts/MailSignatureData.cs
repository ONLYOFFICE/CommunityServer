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


using System.Runtime.Serialization;

namespace ASC.Mail.Data.Contracts
{
    [DataContract(Namespace = "")]
    public class MailSignatureData
    {
        public MailSignatureData(int mailboxId, int tenant, string html, bool isActive)
        {
            Tenant = tenant;
            MailboxId = mailboxId;
            Html = html;
            IsActive = isActive;
        }

        public int Tenant { get; private set; }

        [DataMember(Name = "mailboxId")]
        public int MailboxId { get; private set; }

        [DataMember(Name = "html")]
        public string Html { get; private set; }

        [DataMember(Name = "isActive")]
        public bool IsActive { get; private set; }
    }
}
