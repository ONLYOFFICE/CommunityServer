/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
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
