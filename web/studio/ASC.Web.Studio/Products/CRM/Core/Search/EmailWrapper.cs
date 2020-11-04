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
using System.Linq;
using ASC.Core;
using ASC.CRM.Core;
using ASC.CRM.Core.Entities;
using ASC.ElasticSearch;

namespace ASC.Web.CRM.Core.Search
{
    public class EmailWrapper : ContactsWrapper
    {
        [Join(JoinTypeEnum.Sub, "id:contact_id", "tenant_id:tenant_id")]
        public List<EmailInfoWrapper> EmailInfoWrapper { get; set; }

        protected override string IndexName
        {
            get
            {
                return "crm_contact_email";
            }
        }

        public static EmailWrapper ToEmailWrapper(Contact contact, List<ContactInfo> contactInfo)
        {
            var result = new EmailWrapper();

            var person = contact as Person;
            if (person != null)
            {
                result = new EmailWrapper
                {
                    Id = person.ID,
                    Title = person.JobTitle,
                    FirstName = person.FirstName,
                    LastName = person.LastName,
                    Industry = person.Industry,
                    Notes = person.About,
                    TenantId = CoreContext.TenantManager.GetCurrentTenant().TenantId
                };
            }
            var company = contact as Company;
            if (company != null)
            {
                result = new EmailWrapper
                {
                    Id = company.ID,
                    CompanyName = company.CompanyName,
                    Industry = company.Industry,
                    Notes = company.About,
                    TenantId = CoreContext.TenantManager.GetCurrentTenant().TenantId
                };
            }

            result.EmailInfoWrapper = contactInfo.Select(r => (EmailInfoWrapper)r).ToList();

            return result;
        }
    }

    public class EmailInfoWrapper: Wrapper
    {
        [ColumnLastModified("last_modifed_on")]
        public override DateTime LastModifiedOn { get; set; }

        [ColumnCondition("contact_id", 1)]
        public int ContactId { get; set; }

        [Column("data", 2, Analyzer = Analyzer.uax_url_email)]
        public string Data { get; set; }

        [ColumnCondition("type", 3, 1)]
        public int Type { get; set; }

        protected override string Table { get { return "crm_contact_info"; } }

        public static implicit operator EmailInfoWrapper(ContactInfo cf)
        {
            return new EmailInfoWrapper
            {
                Id = cf.ID,
                ContactId = cf.ContactID,
                Data = cf.Data,
                Type = (int)cf.InfoType,
                TenantId = CoreContext.TenantManager.GetCurrentTenant().TenantId
            };
        }
    }
}