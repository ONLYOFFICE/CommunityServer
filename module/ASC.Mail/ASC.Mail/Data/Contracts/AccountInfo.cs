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

namespace ASC.Mail.Data.Contracts
{
    public class AccountInfo
    {
        public int Id { get; set; }

        public string Email { get; set; }

        public bool Enabled { get; set; }

        public bool QuotaError { get; set; }

        public bool AuthError { get; set; }

        public bool OAuthConnection { get; set; }

        public string Name { get; set; }

        public string EMailInFolder { get; set; }

        public bool IsTeamlabMailbox { get; set; }

        public MailSignatureData Signature { get; set; }

        public MailAutoreplyData Autoreply { get; set; }

        public List<MailAddressInfo> Aliases { get; set; }

        public List<MailAddressInfo> Groups { get; set; }

        public bool IsSharedDomainMailbox { get; set; }

        public override string ToString()
        {
            return Name + " <" + Email + ">";
        }

        public AccountInfo(int id, string address, string name, bool enabled,
            bool quotaError, MailBoxData.AuthProblemType authError, MailSignatureData signature, MailAutoreplyData autoreply,
            bool oauthConnection, string emailInFolder, bool isTeamlabMailbox, bool isSharedDomainMailbox)
        {
            Id = id;
            Email = address;
            Name = name;
            Enabled = enabled;
            QuotaError = quotaError;
            AuthError = authError > MailBoxData.AuthProblemType.NoProblems;
            Autoreply = autoreply;
            Signature = signature;
            Aliases = new List<MailAddressInfo>();
            Groups = new List<MailAddressInfo>();
            OAuthConnection = oauthConnection;
            EMailInFolder = emailInFolder;
            IsTeamlabMailbox = isTeamlabMailbox;
            IsSharedDomainMailbox = isSharedDomainMailbox;
        }
    }
}
