/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using ASC.Api.Employee;
using ASC.CRM.Core;
using ASC.CRM.Core.Entities;
using ASC.Specific;
using Contact = ASC.CRM.Core.Entities.Contact;
using ASC.Web.CRM;
using ASC.Web.CRM.Classes;
using ASC.Web.CRM.Core.Enums;

namespace ASC.Api.CRM.Wrappers
{
    /// <summary>
    ///   Person
    /// </summary>
    [DataContract(Name = "person", Namespace = "")]
    public class PersonWrapper : ContactWrapper
    {
        public PersonWrapper(int id) :
            base(id)
        {
        }

        public PersonWrapper(Person person)
            : base(person)
        {
            FirstName = person.FirstName;
            LastName = person.LastName;
            Title = person.JobTitle;
        }

        [DataMember(IsRequired = true, EmitDefaultValue = false)]
        public String FirstName { get; set; }

        [DataMember(IsRequired = true, EmitDefaultValue = false)]
        public String LastName { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public ContactBaseWrapper Company { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public String Title { get; set; }

        public new static PersonWrapper GetSample()
        {
            return new PersonWrapper(0)
                {
                    IsPrivate = false,
                    IsShared = false,
                    IsCompany = false,
                    FirstName = "Tadjeddine",
                    LastName = "Bachir",
                    Company = CompanyWrapper.GetSample(),
                    Title = "Programmer",
                    About = "",
                    Created = ApiDateTime.GetSample(),
                    CreateBy = EmployeeWraper.GetSample(),
                    ShareType = ShareType.None
                };
        }
    }

    /// <summary>
    ///  Company
    /// </summary>
    [DataContract(Name = "company", Namespace = "")]
    public class CompanyWrapper : ContactWrapper
    {
        public CompanyWrapper(int id) :
            base(id)
        {
        }

        public CompanyWrapper(Company company)
            : base(company)
        {
            CompanyName = company.CompanyName;
            //  PersonsCount = Global.DaoFactory.GetContactDao().GetMembersCount(company.ID);
        }

        [DataMember(IsRequired = true, EmitDefaultValue = false)]
        public String CompanyName { get; set; }

        [DataMember(IsRequired = true, EmitDefaultValue = false)]
        public IEnumerable<ContactBaseWrapper> Persons { get; set; }

        [DataMember(IsRequired = true, EmitDefaultValue = false)]
        public int PersonsCount { get; set; }

        public new static CompanyWrapper GetSample()
        {
            return new CompanyWrapper(0)
                {
                    IsPrivate = false,
                    IsCompany = true,
                    About = "",
                    CompanyName = "Food and Culture Project",
                    PersonsCount = 0
                };
        }
    }

    [DataContract(Name = "contact", Namespace = "")]
    [KnownType(typeof(PersonWrapper))]
    [KnownType(typeof(CompanyWrapper))]
    public abstract class ContactWrapper : ContactBaseWrapper
    {
        protected ContactWrapper(int id)
            : base(id)
        {
        }

        protected ContactWrapper(Contact contact)
            : base(contact)
        {
            CreateBy = EmployeeWraper.Get(contact.CreateBy);
            Created = (ApiDateTime)contact.CreateOn;
            About = contact.About;
            Industry = contact.Industry;
        }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public IEnumerable<Address> Addresses { get; set; }


        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public EmployeeWraper CreateBy { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public ApiDateTime Created { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public String About { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public String Industry { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public ContactStatusBaseWrapper ContactStatus { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public ContactTypeBaseWrapper ContactType { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public IEnumerable<ContactInfoWrapper> CommonData { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public IEnumerable<CustomFieldBaseWrapper> CustomFields { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public IEnumerable<String> Tags { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int TaskCount { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public bool HaveLateTasks { get; set; }

        public new static ContactWrapper GetSample()
        {
            return new PersonWrapper(0)
            {
                IsPrivate = false,
                IsShared = false,
                IsCompany = false,
                FirstName = "Tadjeddine",
                LastName = "Bachir",
                Company = CompanyWrapper.GetSample(),
                Title = "Programmer",
                About = "",
                Created = ApiDateTime.GetSample(),
                CreateBy = EmployeeWraper.GetSample(),
                CommonData = new List<ContactInfoWrapper>(){ContactInfoWrapper.GetSample()},
                CustomFields = new List<CustomFieldBaseWrapper>(){CustomFieldBaseWrapper.GetSample()},
                ShareType = ShareType.None,
                CanDelete = true,
                CanEdit = true,
                TaskCount = 0,
                HaveLateTasks = false
            };
        }
    }

    [DataContract(Name = "contactBase", Namespace = "")]
    public class ContactBaseWithEmailWrapper : ContactBaseWrapper
    {
        protected ContactBaseWithEmailWrapper(int id)
            : base(id)
        {
        }

        public ContactBaseWithEmailWrapper(Contact contact)
            : base(contact)
        {
        }

        public ContactBaseWithEmailWrapper(ContactWrapper contactWrapper) : base(contactWrapper.ID)
        {
            AccessList = contactWrapper.AccessList;
            CanEdit = contactWrapper.CanEdit;
            DisplayName = contactWrapper.DisplayName;
            IsCompany = contactWrapper.IsCompany;
            IsPrivate = contactWrapper.IsPrivate;
            IsShared = contactWrapper.IsShared;
            ShareType = contactWrapper.ShareType;
            MediumFotoUrl = contactWrapper.MediumFotoUrl;
            SmallFotoUrl = contactWrapper.SmallFotoUrl;

            if (contactWrapper.CommonData != null && contactWrapper.CommonData.Count() != 0)
            {
                Email = contactWrapper.CommonData.FirstOrDefault(item => item.InfoType == ContactInfoType.Email && item.IsPrimary);
            }
            else
            {
                Email = null;
            }
        }


        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public ContactInfoWrapper Email { get; set; }
    }


    [DataContract(Name = "contactBase", Namespace = "")]
    public class ContactBaseWithPhoneWrapper : ContactBaseWrapper
    {
        protected ContactBaseWithPhoneWrapper(int id)
            : base(id)
        {
        }

        public ContactBaseWithPhoneWrapper(Contact contact)
            : base(contact)
        {
        }

        public ContactBaseWithPhoneWrapper(ContactWrapper contactWrapper)
            : base(contactWrapper.ID)
        {
            AccessList = contactWrapper.AccessList;
            CanEdit = contactWrapper.CanEdit;
            DisplayName = contactWrapper.DisplayName;
            IsCompany = contactWrapper.IsCompany;
            IsPrivate = contactWrapper.IsPrivate;
            IsShared = contactWrapper.IsShared;
            ShareType = contactWrapper.ShareType;
            MediumFotoUrl = contactWrapper.MediumFotoUrl;
            SmallFotoUrl = contactWrapper.SmallFotoUrl;

            if (contactWrapper.CommonData != null && contactWrapper.CommonData.Count() != 0)
            {
                Phone = contactWrapper.CommonData.FirstOrDefault(item => item.InfoType == ContactInfoType.Phone && item.IsPrimary);
            }
            else
            {
                Phone = null;
            }
        }


        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public ContactInfoWrapper Phone { get; set; }
    }


    /// <summary>
    ///  Contact base information
    /// </summary>
    [DataContract(Name = "contactBase", Namespace = "")]
    public class ContactBaseWrapper : ObjectWrapperBase
    {
        public ContactBaseWrapper(Contact contact)
            : base(contact.ID)
        {
            DisplayName = contact.GetTitle();
            IsPrivate = CRMSecurity.IsPrivate(contact);
            IsShared = contact.ShareType == ShareType.ReadWrite || contact.ShareType == ShareType.Read;
            ShareType = contact.ShareType;

            if (IsPrivate)
            {
                AccessList = CRMSecurity.GetAccessSubjectTo(contact)
                                        .Select(item => EmployeeWraper.Get(item.Key));
            }
            Currency = !String.IsNullOrEmpty(contact.Currency) ?
                new CurrencyInfoWrapper(CurrencyProvider.Get(contact.Currency)) :
                null;

            SmallFotoUrl = String.Format("{0}HttpHandlers/filehandler.ashx?action=contactphotoulr&cid={1}&isc={2}&ps=1", PathProvider.BaseAbsolutePath, contact.ID, contact is Company).ToLower();
            MediumFotoUrl = String.Format("{0}HttpHandlers/filehandler.ashx?action=contactphotoulr&cid={1}&isc={2}&ps=2", PathProvider.BaseAbsolutePath, contact.ID, contact is Company).ToLower();
            IsCompany = contact is Company;
            CanEdit = CRMSecurity.CanEdit(contact);
            CanDelete = CRMSecurity.CanDelete(contact);
        }


        protected ContactBaseWrapper(int contactID)
            : base(contactID)
        {
        }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public String SmallFotoUrl { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public String MediumFotoUrl { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public String DisplayName { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public bool IsCompany { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public IEnumerable<EmployeeWraper> AccessList { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public bool IsPrivate { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public bool IsShared { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public ShareType ShareType { get; set; }

        [DataMember]
        public CurrencyInfoWrapper Currency { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public bool CanEdit { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public bool CanDelete { get; set; }

        public static ContactBaseWrapper GetSample()
        {
            return new ContactBaseWrapper(0)
                {
                    IsPrivate = false,
                    IsShared = false,
                    IsCompany = false,
                    DisplayName = "Tadjeddine Bachir",
                    SmallFotoUrl = "url to foto"
                };
        }
    }


    [DataContract(Name = "contact_task", Namespace = "")]
    public class ContactWithTaskWrapper
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public TaskBaseWrapper Task { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public ContactWrapper Contact { get; set; }
    }
}