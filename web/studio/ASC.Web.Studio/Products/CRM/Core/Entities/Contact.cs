/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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
                    String.Format(@"<a class='linkMedium {0}' id='{5}' data-id='{2}' href='default.aspx?{1}={2}{3}'>
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