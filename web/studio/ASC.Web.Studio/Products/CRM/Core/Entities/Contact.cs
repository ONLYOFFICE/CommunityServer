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
using ASC.Common.Security;
using ASC.Web.Core.Helpers;
using ASC.Web.CRM.Classes;
using ASC.Web.CRM.Core.Enums;
using ASC.Web.CRM.Resources;

namespace ASC.CRM.Core.Entities
{
    [Serializable]
    public class Person : Contact
    {
        public Person()
        {
            FirstName = String.Empty;
            LastName = String.Empty;
            CompanyID = 0;
            JobTitle = String.Empty;
        }

        public String FirstName { get; set; }

        public String LastName { get; set; }

        public int CompanyID { get; set; }

        public String JobTitle { get; set; }
    }

    [Serializable]
    public class Company : Contact
    {
        public Company()
        {
            CompanyName = String.Empty;
        }

        public String CompanyName { get; set; }
    }

    public static class ContactExtension
    {
        public static String GetTitle(this Contact contact)
        {
            if (contact == null)
                return String.Empty;

            if (contact is Company)
            {
                var company = (Company)contact;

                return company.CompanyName;
            }

            var people = (Person)contact;

            return String.Format("{0} {1}", people.FirstName, people.LastName);
        }

        public static String RenderLinkForCard(this Contact contact)
        {
            var isCompany = contact is Company;
            var popupID = Guid.NewGuid();

            return !CRMSecurity.CanAccessTo(contact) ?
                    String.Format(@"<span class='noAccessToContact'>{0}</span>", GetTitle(contact).HtmlEncode()) :
                    String.Format(@"<a class='linkMedium {0}' id='{5}' data-id='{2}' href='Default.aspx?{1}={2}{3}'>
                                         {4}
                                    </a>",
                                     isCompany ? "crm-companyInfoCardLink" : "crm-peopleInfoCardLink",
                                     UrlConstant.ID, contact != null ? contact.ID : 0,
                                     isCompany ? String.Empty : String.Format("&{0}=people", UrlConstant.Type),
                                     GetTitle(contact).HtmlEncode(), popupID);
        }
    }

    [Serializable]
    public abstract class Contact : DomainObject, ISecurityObjectId
    {
        protected Contact()
        {
            About = String.Empty;
            Industry = String.Empty;
            StatusID = 0;
            ContactTypeID = 0;
            ShareType = ShareType.None;
        }

        public Guid CreateBy { get; set; }

        public DateTime CreateOn { get; set; }

        public Guid? LastModifedBy { get; set; }

        public DateTime? LastModifedOn { get; set; }

        public String About { get; set; }

        public String Industry { get; set; }

        public int StatusID { get; set; }

        public int ContactTypeID { get; set; }

        public ShareType ShareType { get; set; }

        public string Currency { get; set; }

        public object SecurityId
        {
            get { return ID; }
        }

        public Type ObjectType
        {
            get { return GetType(); }
        }
    }
}