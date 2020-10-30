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
    public class ContactInfoTable : ITable
    {
        public const string TABLE_NAME = "mail_contact_info";

        public static class Columns
        {
            public const string Id = "id";
            public const string Tenant = "tenant";
            public const string User = "id_user";
            public const string ContactId = "id_contact";
            public const string Data = "data";
            public const string Type = "type";
            public const string IsPrimary = "is_primary";
            public const string LastModified = "last_modified";
        }

        public string Name
        {
            get { return TABLE_NAME; }
        }

        public IEnumerable<string> OrderedColumnCollection { get; private set; }

        public ContactInfoTable()
        {
            OrderedColumnCollection = new List<string>
            {
                Columns.Id,
                Columns.Tenant,
                Columns.User,
                Columns.ContactId,
                Columns.Data,
                Columns.Type,
                Columns.IsPrimary,
                Columns.LastModified
            };
        }
    }
}