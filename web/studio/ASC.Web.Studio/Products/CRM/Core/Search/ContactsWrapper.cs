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
using ASC.Core;
using ASC.CRM.Core.Entities;
using ASC.ElasticSearch;

namespace ASC.Web.CRM.Core.Search
{
    public class ContactsWrapper : Wrapper
    {
        [ColumnLastModified("last_modifed_on")]
        public override DateTime LastModifiedOn { get; set; }

        [Column("title", 1)]
        public string Title { get; set; }

        [Column("first_name", 2)]
        public string FirstName { get; set; }

        [Column("last_name", 3)]
        public string LastName { get; set; }

        [Column("company_name", 4)]
        public string CompanyName { get; set; }

        [Column("notes", 5)]
        public string Notes { get; set; }

        [Column("industry", 6)]
        public string Industry { get; set; }

        protected override string Table { get { return "crm_contact"; } }

        public static implicit operator ContactsWrapper(Contact d)
        {
            var person = d as Person;
            if (person != null)
            {
                return person;
            }
            return d as Company;
        }

        public static implicit operator ContactsWrapper(Person d)
        {
            return new ContactsWrapper
            {
                Id = d.ID,
                Title = d.JobTitle,
                FirstName = d.FirstName,
                LastName = d.LastName,
                Industry = d.Industry,
                Notes = d.About,
                TenantId = CoreContext.TenantManager.GetCurrentTenant().TenantId
            };
        }

        public static implicit operator ContactsWrapper(Company d)
        {
            return new ContactsWrapper
            {
                Id = d.ID,
                CompanyName = d.CompanyName,
                Industry = d.Industry,
                Notes = d.About,
                TenantId = CoreContext.TenantManager.GetCurrentTenant().TenantId
            };
        }
    }
}