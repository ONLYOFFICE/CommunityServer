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