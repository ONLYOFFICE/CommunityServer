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


// ReSharper disable InconsistentNaming

using System.Collections.Generic;

using ASC.Mail.Core.DbSchema.Interfaces;

namespace ASC.Mail.Core.DbSchema.Tables
{
    public class ImapSpecialMailboxTable : ITable
    {
        public const string TABLE_NAME = "mail_imap_special_mailbox";

        public static class Columns
        {
            public const string MailboxName = "name";
            public const string Server = "server";
            public const string FolderId = "folder_id";
            public const string Skip = "skip";
        }

        public string Name
        {
            get { return TABLE_NAME; }
        }

        public IEnumerable<string> OrderedColumnCollection { get; private set; }

        public ImapSpecialMailboxTable()
        {
            OrderedColumnCollection = new List<string>
            {
                Columns.Server,
                Columns.MailboxName,
                Columns.FolderId,
                Columns.Skip
            };
        }
    }
}