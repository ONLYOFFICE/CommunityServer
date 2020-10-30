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
using System.Collections.Generic;
using ASC.ElasticSearch;

namespace ASC.Mail.Data.Search
{
    public sealed class MailContactWrapper : Wrapper
    {
        [ColumnId("id")]
        public override int Id { get; set; }

        [ColumnTenantId("tenant")]
        public override int TenantId { get; set; }

        [ColumnLastModified("last_modified")]
        public override DateTime LastModifiedOn { get; set; }

        [ColumnMeta("id_user", 1)]
        public Guid User { get; set; }

        [Column("name", 2)]
        public string Name { get; set; }

        [Column("description", 3, Analyzer = Analyzer.uax_url_email)]
        public string Description { get; set; }

        [ColumnMeta("type", 4)]
        public int ContactType { get; set; }

        [Join(JoinTypeEnum.Sub, "id:id_contact")]
        public List<MailContactInfoWrapper> InfoList { get; set; }

        protected override string Table
        {
            get { return "mail_contacts"; }
        }
    }

    public sealed class MailContactInfoWrapper : Wrapper
    {
        [ColumnId("id")]
        public override int Id { get; set; }

        [ColumnTenantId("tenant")]
        public override int TenantId { get; set; }

        [ColumnLastModified("last_modified")]
        public override DateTime LastModifiedOn { get; set; }

        [ColumnMeta("id_user", 1)]
        public Guid User { get; set; }

        [ColumnMeta("id_contact", 2)]
        public int ContactId { get; set; }

        [Column("data", 3)]
        public string Text { get; set; }

        [ColumnMeta("type", 4)]
        public int InfoType { get; set; }

        [ColumnMeta("is_primary", 5)]
        public bool IsPrimary { get; set; }

        protected override string Table
        {
            get { return "mail_contact_info"; }
        }
    }
}
