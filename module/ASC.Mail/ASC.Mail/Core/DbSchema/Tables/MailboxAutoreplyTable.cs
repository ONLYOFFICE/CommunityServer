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


// ReSharper disable InconsistentNaming

using System.Collections.Generic;
using ASC.Mail.Core.DbSchema.Interfaces;

namespace ASC.Mail.Core.DbSchema.Tables
{
    public class MailboxAutoreplyTable : ITable
    {
        public const string TABLE_NAME = "mail_mailbox_autoreply";

        public static class Columns
        {
            public const string MailboxId = "id_mailbox";
            public const string Tenant = "tenant";
            public const string TurnOn = "turn_on";
            public const string OnlyContacts = "only_contacts";
            public const string TurnOnToDate = "turn_on_to_date";
            public const string FromDate = "from_date";
            public const string ToDate = "to_date";
            public const string Subject = "subject";
            public const string Html = "html";
        }

        public string Name
        {
            get { return TABLE_NAME; }
        }

        public IEnumerable<string> OrderedColumnCollection { get; private set; }

        public MailboxAutoreplyTable()
        {
            OrderedColumnCollection = new List<string>
            {
                Columns.MailboxId, 
                Columns.Tenant,
                Columns.TurnOn,
                Columns.OnlyContacts,
                Columns.TurnOnToDate,
                Columns.FromDate,
                Columns.ToDate,
                Columns.Subject,
                Columns.Html
            };
        }
    }
}
