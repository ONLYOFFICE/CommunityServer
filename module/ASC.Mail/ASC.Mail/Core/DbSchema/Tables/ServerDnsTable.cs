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
    public class ServerDnsTable : ITable
    {
        public const string TABLE_NAME = "mail_server_dns";

        public static class Columns
        {
            public const string Id = "id";
            public const string Tenant = "tenant";
            public const string User = "id_user";
            public const string DomainId = "id_domain";

            public const string DkimSelector = "dkim_selector";
            public const string DkimPrivateKey = "dkim_private_key";
            public const string DkimPublicKey = "dkim_public_key";
            public const string DkimTtl = "dkim_ttl";
            public const string DkimVerified = "dkim_verified";
            public const string DkimDateChecked = "dkim_date_checked";

            public const string DomainCheck = "domain_check";
            
            public const string Spf = "spf";
            public const string SpfTtl = "spf_ttl";
            public const string SpfVerified = "spf_verified";
            public const string SpfDateChecked = "spf_date_checked";

            public const string Mx = "mx";
            public const string MxTtl = "mx_ttl";
            public const string MxVerified = "mx_verified";
            public const string MxDateChecked = "mx_date_checked";

            public const string TimeModified = "time_modified";
        }

        public string Name
        {
            get { return TABLE_NAME; }
        }

        public IEnumerable<string> OrderedColumnCollection { get; private set; }

        public ServerDnsTable()
        {
            OrderedColumnCollection = new List<string>
            {
                Columns.Id,
                Columns.Tenant,
                Columns.User,
                Columns.DomainId,
                Columns.DomainCheck,
                Columns.DkimSelector,
                Columns.DkimPrivateKey,
                Columns.DkimPublicKey, 
                Columns.DkimTtl,
                Columns.DkimVerified,
                Columns.DkimDateChecked,
                Columns.Spf,
                Columns.SpfTtl,
                Columns.SpfVerified,
                Columns.SpfDateChecked,
                Columns.Mx,
                Columns.MxTtl,
                Columns.MxVerified,
                Columns.MxDateChecked,
                Columns.TimeModified
            };
        }
    }
}