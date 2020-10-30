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
using ASC.Mail.Server.Core.DbSchema.Interfaces;

namespace ASC.Mail.Server.Core.DbSchema.Tables
{
    public class MailboxTable : ITable
    {
        public const string TABLE_NAME = "mailbox";

        public static class Columns
        {
            public const string USERNAME = "username";
            public const string PASSWORD = "password";
            public const string NAME = "name";
            public const string MAILDIR = "maildir";
            public const string MAXQUOTA = "maxquota";
            public const string QUOTA = "quota";
            public const string LOCAL_PART = "local_part";
            public const string DOMAIN = "domain";
            public const string CREATED = "created";
            public const string MODIFIED = "modified";
            public const string ACTIVE = "active";
            public const string ENABLE_IMAP = "enableimap";
            public const string ENABLE_IMAP_SECURED = "enableimapsecured";
            public const string ENABLE_POP = "enablepop3";
            public const string ENABLE_POP_SECURED = "enablepop3secured";
            public const string ENABLE_DELIVER = "enabledeliver";
            public const string ENABLE_LDA = "enablelda";
        }

        public string Name
        {
            get { return TABLE_NAME; }
        }

        public IEnumerable<string> OrderedColumnCollection { get; private set; }

        public MailboxTable()
        {
            OrderedColumnCollection = new List<string>
            {
                Columns.USERNAME,
                Columns.PASSWORD,
                Columns.NAME,
                Columns.MAILDIR,
                Columns.MAXQUOTA,
                Columns.QUOTA,
                Columns.LOCAL_PART,
                Columns.DOMAIN,
                Columns.CREATED,
                Columns.MODIFIED,
                Columns.ACTIVE,
                Columns.ENABLE_IMAP,
                Columns.ENABLE_IMAP_SECURED,
                Columns.ENABLE_POP,
                Columns.ENABLE_POP_SECURED,
                Columns.ENABLE_DELIVER,
                Columns.ENABLE_LDA
            };
        }
    }
}