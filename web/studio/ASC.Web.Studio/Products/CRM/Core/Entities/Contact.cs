/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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

        public static String GetEmployeesCountString(this Contact contact)
        {
            if (contact is Person) return String.Empty;
            var count = Global.DaoFactory.GetContactDao().GetMembersCount(contact.ID);
            return count + " " + GrammaticalHelper.ChooseNumeralCase(count,
                                                                     CRMContactResource.MembersNominative,
                                                                     CRMContactResource.MembersGenitiveSingular,
                                                                     CRMContactResource.MembersGenitivePlural);
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