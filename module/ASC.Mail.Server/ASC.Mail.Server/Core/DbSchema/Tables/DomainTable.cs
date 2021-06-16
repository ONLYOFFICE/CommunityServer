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


using System.Collections.Generic;

using ASC.Mail.Server.Core.DbSchema.Interfaces;

namespace ASC.Mail.Server.Core.DbSchema.Tables
{
    public class DomainTable : ITable
    {
        public const string TABLE_NAME = "domain";

        public static class Columns
        {
            public const string DOMAIN = "domain";
            public const string DESCRIPTION = "description";
            public const string ALIASES = "aliases";
            public const string MAILBOXES = "mailboxes";
            public const string MAXQUOTA = "maxquota";
            public const string QUOTA = "quota";
            public const string TRANSPORT = "transport";
            public const string BACKUPMX = "backupmx";
            public const string CREATED = "created";
            public const string MODIFIED = "modified";
            public const string ACTIVE = "active";
        }

        public string Name { get { return TABLE_NAME; } }

        public IEnumerable<string> OrderedColumnCollection { get; private set; }

        public DomainTable()
        {
            OrderedColumnCollection = new List<string>
            {
                Columns.DOMAIN,
                Columns.DESCRIPTION,
                Columns.ALIASES,
                Columns.MAILBOXES,
                Columns.MAXQUOTA,
                Columns.QUOTA,
                Columns.TRANSPORT,
                Columns.BACKUPMX,
                Columns.CREATED,
                Columns.MODIFIED,
                Columns.ACTIVE
            };
        }
    }
}
