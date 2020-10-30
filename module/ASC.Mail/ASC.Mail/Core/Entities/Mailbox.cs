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
    public class Mailbox
    {
        public int Id { get; set; }
        public string User { get; set; }
        public int Tenant { get; set; }
        public string Address { get; set; }
        public bool Enabled { get; set; }
        public string Password { get; set; }
        public int MsgCountLast { get; set; }
        public long SizeLast { get; set; }
        public string SmtpPassword { get; set; }
        public string Name { get; set; }
        public int LoginDelay { get; set; }
        public bool IsProcessed { get; set; }
        public bool IsRemoved { get; set; }
        public bool IsDefault { get; set; }
        public bool QuotaError { get; set; }
        public bool Imap { get; set; }
        public DateTime BeginDate { get; set; }
        public int OAuthType { get; set; }
        public string OAuthToken { get; set; }
        public string ImapIntervals { get; set; }
        public int SmtpServerId { get; set; }
        public int ServerId { get; set; }
        public string EmailInFolder { get; set; }
        public bool IsTeamlabMailbox { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateChecked { get; set; }
        public DateTime DateUserChecked { get; set; }
        public bool UserOnline { get; set; }
        public DateTime DateLoginDelayExpires { get; set; }
        public DateTime? DateAuthError { get; set; }
    }
}
