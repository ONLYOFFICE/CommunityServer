/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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
using System.Security;
using System.Web;

using ASC.Api.Attributes;
using ASC.Api.Collections;
using ASC.Api.CRM.Wrappers;
using ASC.Api.Employee;
using ASC.Api.Exceptions;
using ASC.Common.Threading.Progress;
using ASC.CRM.Core;
using ASC.CRM.Core.Entities;
using ASC.MessagingSystem;
using ASC.Projects.Engine;
using ASC.Specific;
using ASC.Web.CRM.Classes;
using ASC.Web.CRM.Core.Enums;
using ASC.Web.CRM.Resources;
using ASC.Web.Projects.Core;
using ASC.Web.Studio.Core;

using Autofac;

using Contact = ASC.CRM.Core.Entities.Contact;

namespace ASC.Api.CRM
{
    public partial class CRMApi
    {
        /// <summary>
        /// Returns the detailed information about a contact with the ID specified in the request.
        /// </summary>
        /// <param type="System.Int32, System" method="url" name="contactid">Contact ID</param>
        /// <returns type="ASC.Api.CRM.Wrappers.ContactWrapper, ASC.Api.CRM">Contact</returns>
        /// <short>Get a contact by ID</short> 
        /// <category>Contacts</category>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <path>api/2.0/crm/contact/{contactid}</path>
        /// <httpMethod>GET</httpMethod>
        [Read(@"contact/{contactid:[0-9]+}")]
        public ContactWrapper GetContactByID(int contactid)
        {
            if (contactid <= 0) throw new ArgumentException();

            var contact = DaoFactory.ContactDao.GetByID(contactid);
            if (contact == null || !CRMSecurity.CanAccessTo(contact)) throw new ItemNotFoundException();

            return ToContactWrapper(contact);
        }

        public IEnumerable<ContactWrapper> GetContactsByID(IEnumerable<int> contactid)
        {
            var contacts = DaoFactory.ContactDao.GetContacts(contactid.ToArray()).Where(r => r != null && CRMSecurity.CanAccessTo(r));
            return ToListContactWrapper(contacts.ToList());
        }

        /// <summary>
        /// Returns contacts for the project with the ID specified in the request.
        /// </summary>
        /// <short>
        /// Get contacts by project ID
        /// </short>
        /// <param type="System.Int32, System" method="url" name="projectid">Project ID</param>
        /// <category>Contacts</category>
        /// <returns type="ASC.Api.CRM.Wrappers.ContactWrapper, ASC.Api.CRM">
        /// List of contacts
        /// </returns>
        ///<exception cref="ArgumentException"></exception>
        ///<path>api/2.0/crm/contact/project/{projectid}</path>
        ///<httpMethod>GET</httpMethod>
        /// <collection>list</collection>
        [Read(@"contact/project/{projectid:[0-9]+}")]
        public IEnumerable<ContactWrapper> GetContactsByProjectID(int projectid)
        {
            if (projectid <= 0) throw new ArgumentException();

            var contacts = DaoFactory.ContactDao.GetContactsByProjectID(projectid);
            return ToListContactWrapper(contacts.ToList());
        }

        /// <summary>
        /// Links the selected contact to the project with the ID specified in the request.
        /// </summary>
        /// <param type="System.Int32, System" method="url" name="contactid">Contact ID</param>
        /// <param type="System.Int32, System" method="url" name="projectid">Project ID</param>
        /// <category>Contacts</category>
        /// <short>Link a contact to the project</short> 
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <returns type="ASC.Api.CRM.Wrappers.ContactWrapper, ASC.Api.CRM">Contact information</returns>
        /// <path>api/2.0/crm/contact/{contactid}/project/{projectid}</path>
        /// <httpMethod>POST</httpMethod>
        [Create(@"contact/{contactid:[0-9]+}/project/{projectid:[0-9]+}")]
        public ContactWrapper SetRelativeContactToProject(int contactid, int projectid)
        {
            if (contactid <= 0 || projectid <= 0) throw new ArgumentException();

            var contact = DaoFactory.ContactDao.GetByID(contactid);
            if (contact == null || !CRMSecurity.CanAccessTo(contact)) throw new ItemNotFoundException();

            var project = ProjectsDaoFactory.ProjectDao.GetById(projectid);
            if (project == null) throw new ItemNotFoundException();

            using (var scope = DIHelper.Resolve())
            {
                if (!scope.Resolve<ProjectSecurity>().CanLinkContact(project)) throw CRMSecurity.CreateSecurityException();
            }

            DaoFactory.ContactDao.SetRelativeContactProject(new List<int> { contactid }, projectid);

            var messageAction = contact is Company ? MessageAction.ProjectLinkedCompany : MessageAction.ProjectLinkedPerson;
            MessageService.Send(Request, messageAction, MessageTarget.Create(contact.ID), project.Title, contact.GetTitle());

            return ToContactWrapper(contact);
        }

        /// <summary>
        /// Links the selected contacts to the project with the ID specified in the request.
        /// </summary>
        /// <param type="System.Collections.Generic.IEnumerable{System.Int32}, System.Collections.Generic" name="contactid">Array of contact IDs</param>
        /// <param type="System.Int32, System" method="url" name="projectid">Project ID</param>
        /// <category>Contacts</category>
        /// <short>Link contacts to the project</short> 
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <returns type="ASC.Api.CRM.Wrappers.ContactWrapper, ASC.Api.CRM">
        /// List of contacts
        /// </returns>
        /// <path>api/2.0/crm/contact/project/{projectid}</path>
        /// <httpMethod>POST</httpMethod>
        /// <collection>list</collection>
        [Create(@"contact/project/{projectid:[0-9]+}")]
        public IEnumerable<ContactWrapper> SetRelativeContactListToProject(IEnumerable<int> contactid, int projectid)
        {
            if (contactid == null) throw new ArgumentException();

            var contactIds = contactid.ToList();

            if (!contactIds.Any() || projectid <= 0) throw new ArgumentException();

            var project = ProjectsDaoFactory.ProjectDao.GetById(projectid);
            if (project == null) throw new ItemNotFoundException();

            using (var scope = DIHelper.Resolve())
            {
                if (!scope.Resolve<ProjectSecurity>().CanLinkContact(project))
                    throw CRMSecurity.CreateSecurityException();
            }


            var contacts = DaoFactory.ContactDao.GetContacts(contactIds.ToArray()).Where(CRMSecurity.CanAccessTo).ToList();
            contactIds = contacts.Select(c => c.ID).ToList();

            DaoFactory.ContactDao.SetRelativeContactProject(contactIds, projectid);

            MessageService.Send(Request, MessageAction.ProjectLinkedContacts, MessageTarget.Create(contactIds), project.Title, contacts.Select(x => x.GetTitle()));

            return contacts.ConvertAll(ToContactWrapper);
        }

        /// <summary>
        /// Removes a link to the selected project from the contact with the ID specified in the request.
        /// </summary>
        /// <param type="System.Int32, System" method="url" name="contactid">Contact ID</param>
        /// <param type="System.Int32, System" method="url" name="projectid">Project ID</param>
        /// <category>Contacts</category>
        /// <short>Remove a contact from the project</short> 
        /// <returns type="ASC.Api.CRM.Wrappers.ContactBaseWrapper, ASC.Api.CRM">
        /// Contact information
        /// </returns>
        /// <path>api/2.0/crm/contact/{contactid}/project/{projectid}</path>
        /// <httpMethod>DELETE</httpMethod>
        [Delete(@"contact/{contactid:[0-9]+}/project/{projectid:[0-9]+}")]
        public ContactBaseWrapper RemoveRelativeContactToProject(int contactid, int projectid)
        {
            if (contactid <= 0 || projectid <= 0) throw new ArgumentException();

            var contact = DaoFactory.ContactDao.GetByID(contactid);
            if (contact == null || !CRMSecurity.CanAccessTo(contact)) throw new ItemNotFoundException();

            var project = ProjectsDaoFactory.ProjectDao.GetById(projectid);

            using (var scope = DIHelper.Resolve())
            {
                if (project == null || !scope.Resolve<ProjectSecurity>().CanLinkContact(project)) throw new ItemNotFoundException();
            }

            DaoFactory.ContactDao.RemoveRelativeContactProject(contactid, projectid);

            var action = contact is Company ? MessageAction.ProjectUnlinkedCompany : MessageAction.ProjectUnlinkedPerson;
            MessageService.Send(Request, action, MessageTarget.Create(contact.ID), project.Title, contact.GetTitle());

            return ToContactBaseWrapper(contact);
        }

        /// <summary>
        /// Adds the selected opportunity to the contact with the ID specified in the request.
        /// </summary>
        /// <param type="System.Int32, System" method="url" name="opportunityid">Opportunity ID</param>
        /// <param type="System.Int32, System" method="url" name="contactid">Contact ID</param>
        /// <short>Add a contact opportunity</short> 
        /// <category>Contacts</category>
        /// <exception cref="ArgumentException"></exception>
        /// <returns type="ASC.Api.CRM.Wrappers.OpportunityWrapper, ASC.Api.CRM">
        /// Opportunity
        /// </returns>
        /// <path>api/2.0/crm/contact/{contactid}/opportunity/{opportunityid}</path>
        /// <httpMethod>POST</httpMethod>
        [Create(@"contact/{contactid:[0-9]+}/opportunity/{opportunityid:[0-9]+}")]
        public OpportunityWrapper AddDealToContact(int contactid, int opportunityid)
        {
            if ((opportunityid <= 0) || (contactid <= 0)) throw new ArgumentException();

            var contact = DaoFactory.ContactDao.GetByID(contactid);
            if (contact == null || !CRMSecurity.CanAccessTo(contact)) throw new ItemNotFoundException();

            var opportunity = DaoFactory.DealDao.GetByID(opportunityid);
            if (opportunity == null || !CRMSecurity.CanAccessTo(opportunity)) throw new ItemNotFoundException();

            DaoFactory.DealDao.AddMember(opportunityid, contactid);

            var messageAction = contact is Company ? MessageAction.OpportunityLinkedCompany : MessageAction.OpportunityLinkedPerson;
            MessageService.Send(Request, messageAction, MessageTarget.Create(contact.ID), opportunity.Title, contact.GetTitle());

            return ToOpportunityWrapper(opportunity);
        }

        /// <summary>
        /// Deletes the selected opportunity from the contact with the ID specified in the request.
        /// </summary>
        /// <param type="System.Int32, System" method="url" name="opportunityid">Opportunity ID</param>
        /// <param type="System.Int32, System" method="url" name="contactid">Contact ID</param>
        /// <short>Delete a contact opportunity</short> 
        /// <category>Contacts</category>
        /// <exception cref="ArgumentException"></exception>
        /// <returns type="ASC.Api.CRM.Wrappers.OpportunityWrapper, ASC.Api.CRM">
        /// Opportunity
        /// </returns>
        /// <path>api/2.0/crm/contact/{contactid}/opportunity/{opportunityid}</path>
        /// <httpMethod>DELETE</httpMethod>
        [Delete(@"contact/{contactid:[0-9]+}/opportunity/{opportunityid:[0-9]+}")]
        public OpportunityWrapper DeleteDealFromContact(int contactid, int opportunityid)
        {
            if ((opportunityid <= 0) || (contactid <= 0)) throw new ArgumentException();

            var contact = DaoFactory.ContactDao.GetByID(contactid);
            if (contact == null || !CRMSecurity.CanAccessTo(contact)) throw new ItemNotFoundException();

            var opportunity = DaoFactory.DealDao.GetByID(opportunityid);
            if (opportunity == null || !CRMSecurity.CanAccessTo(opportunity)) throw new ItemNotFoundException();
            if(opportunity.ContactID == contactid) throw new SecurityException("Can't unlink base contact");

            DaoFactory.DealDao.RemoveMember(opportunityid, contactid);

            return ToOpportunityWrapper(opportunity);
        }

        /// <summary>
        /// Returns a list of all the contacts in the CRM module matching the parameters specified in the request.
        /// </summary>
        /// <param type="System.Collections.Generic.IEnumerable{System.String}, System.Collections.Generic" method="url" optional="true" name="tags">Contact tag</param>
        /// <param type="System.Nullable{System.Int32}, System" method="url" optional="true" name="contactStage">Contact stage ID (warmth)</param>
        /// <param type="System.Nullable{System.Int32}, System" method="url" optional="true" name="contactType">Contact type ID</param>
        /// <param type="ASC.CRM.Core.ContactListViewType, ASC.CRM.Core" method="url" optional="true" name="contactListView" remark="Allowed values: Company, Person, WithOpportunity">Contact list view</param>
        /// <param type="System.Nullable{System.Guid}, System" method="url" optional="true" name="responsibleid">Responsible ID</param>
        /// <param type="System.Nullable{System.Boolean}, System" method="url" optional="true" name="isShared">Contact privacy: private or not</param>
        /// <param type="ASC.Specific.ApiDateTime, ASC.Specific" method="url" optional="true" name="fromDate">Start date</param>
        /// <param type="ASC.Specific.ApiDateTime, ASC.Specific" method="url" optional="true" name="toDate">End date</param>
        /// <short>Get filtered contacts</short> 
        /// <category>Contacts</category>
        /// <returns type="ASC.Api.CRM.Wrappers.ContactWrapper, ASC.Api.CRM">
        /// List of contacts
        /// </returns>
        /// <path>api/2.0/crm/contact/filter</path>
        /// <httpMethod>GET</httpMethod>
        /// <collection>list</collection>
        [Read(@"contact/filter")]
        public IEnumerable<ContactWrapper> GetContacts(
            IEnumerable<String> tags,
            int? contactStage,
            int? contactType,
            ContactListViewType contactListView,
            Guid? responsibleid,
            bool? isShared,
            ApiDateTime fromDate,
            ApiDateTime toDate)
        {
            IEnumerable<ContactWrapper> result;

            OrderBy contactsOrderBy;

            ContactSortedByType sortBy;

            var searchString = _context.FilterValue;

            if (Web.CRM.Classes.EnumExtension.TryParse(_context.SortBy, true, out sortBy))
            {
                contactsOrderBy = new OrderBy(sortBy, !_context.SortDescending);
            }
            else if (String.IsNullOrEmpty(_context.SortBy))
            {
                contactsOrderBy = new OrderBy(ContactSortedByType.Created, false);
            }
            else
            {
                contactsOrderBy = null;
            }


            var fromIndex = (int)_context.StartIndex;
            var count = (int)_context.Count;
            var contactStageInt = contactStage.HasValue ? contactStage.Value : -1;
            var contactTypeInt = contactType.HasValue ? contactType.Value : -1;

            if (contactsOrderBy != null)
            {
                result = ToListContactWrapper(DaoFactory.ContactDao.GetContacts(
                    searchString,
                    tags,
                    contactStageInt,
                    contactTypeInt,
                    contactListView,
                    fromDate,
                    toDate,
                    fromIndex,
                    count,
                    contactsOrderBy,
                    responsibleid,
                    isShared));
                _context.SetDataPaginated();
                _context.SetDataFiltered();
                _context.SetDataSorted();
            }
            else
            {
                result = ToListContactWrapper(DaoFactory.ContactDao.GetContacts(
                    searchString,
                    tags,
                    contactStageInt,
                    contactTypeInt,
                    contactListView,
                    fromDate,
                    toDate,
                    0,
                    0,
                    null,
                    responsibleid,
                    isShared));
            }

            int totalCount;

            if (result.Count() < count)
            {
                totalCount = fromIndex + result.Count();
            }
            else
            {
                totalCount = DaoFactory.ContactDao.GetContactsCount(
                    searchString,
                    tags,
                    contactStageInt,
                    contactTypeInt,
                    contactListView,
                    fromDate,
                    toDate,
                    responsibleid,
                    isShared);
            }

            _context.SetTotalCount(totalCount);

            return result;
        }

        /// <summary>
        /// Searches for contacts by their emails.
        /// </summary>
        /// <param type="System.String, System" name="term">String part of contact name, lastname or email</param>
        /// <param type="System.Int32, System" name="maxCount">Maximum result count</param>
        /// <short>Search contacts by email</short> 
        /// <category>Contacts</category>
        /// <returns>
        /// List of contacts
        /// </returns>
        /// <path>api/2.0/crm/contact/simple/byEmail</path>
        /// <httpMethod>GET</httpMethod>
        /// <collection>list</collection>
        /// <visible>false</visible>
        [Read(@"contact/simple/byEmail")]
        public IEnumerable<ContactWithTaskWrapper> SearchContactsByEmail(string term, int maxCount)
        {
            var result = ToSimpleListContactWrapper(DaoFactory.ContactDao.SearchContactsByEmail(
                term,
                maxCount));

            return result;
        }

        /// <summary>
        /// Returns a list of all the contacts with their tasks in the CRM module matching the parameters specified in the request.
        /// </summary>
        /// <param type="System.Collections.Generic.IEnumerable{System.String}, System.Collections.Generic" optional="true" name="tags">Contact tags</param>
        /// <param type="System.Nullable{System.Int32}, System" optional="true" name="contactStage">Contact stage ID (warmth)</param>
        /// <param type="System.Nullable{System.Int32}, System" optional="true" name="contactType">Contact type ID</param>
        /// <param type="ASC.CRM.Core.ContactListViewType, ASC.CRM.Core" optional="true" name="contactListView" remark="Allowed values: Company, Person, WithOpportunity">Contact list view</param>
        /// <param type="System.Nullable{System.Guid}, System" optional="true" name="responsibleid">Responsible ID</param>
        /// <param type="System.Nullable{System.Boolean}, System" optional="true" name="isShared">Contact privacy: private or not</param>
        /// <param type="ASC.Specific.ApiDateTime, ASC.Specific" optional="true" name="fromDate">Start date</param>
        /// <param type="ASC.Specific.ApiDateTime, ASC.Specific" optional="true" name="toDate">End date</param>
        /// <short>Get filtered contacts with tasks</short> 
        /// <category>Contacts</category>
        /// <returns>
        /// List of contacts with tasks
        /// </returns>
        /// <path>api/2.0/crm/contact/simple/filter</path>
        /// <httpMethod>GET</httpMethod>
        /// <collection>list</collection>
        /// <visible>false</visible>
        [Read(@"contact/simple/filter")]
        public IEnumerable<ContactWithTaskWrapper> GetSimpleContacts(
            IEnumerable<string> tags,
            int? contactStage,
            int? contactType,
            ContactListViewType contactListView,
            Guid? responsibleid,
            bool? isShared,
            ApiDateTime fromDate,
            ApiDateTime toDate)
        {
            IEnumerable<ContactWithTaskWrapper> result;

            OrderBy contactsOrderBy;

            ContactSortedByType sortBy;

            var searchString = _context.FilterValue;
            if (Web.CRM.Classes.EnumExtension.TryParse(_context.SortBy, true, out sortBy))
            {
                contactsOrderBy = new OrderBy(sortBy, !_context.SortDescending);
            }
            else if (String.IsNullOrEmpty(_context.SortBy))
            {
                contactsOrderBy = new OrderBy(ContactSortedByType.DisplayName, true);
            }
            else
            {
                contactsOrderBy = null;
            }

            var fromIndex = (int)_context.StartIndex;
            var count = (int)_context.Count;
            var contactStageInt = contactStage.HasValue ? contactStage.Value : -1;
            var contactTypeInt = contactType.HasValue ? contactType.Value : -1;

            if (contactsOrderBy != null)
            {
                result = ToSimpleListContactWrapper(DaoFactory.ContactDao.GetContacts(
                    searchString,
                    tags,
                    contactStageInt,
                    contactTypeInt,
                    contactListView,
                    fromDate,
                    toDate,
                    fromIndex,
                    count,
                    contactsOrderBy,
                    responsibleid,
                    isShared));
                _context.SetDataPaginated();
                _context.SetDataFiltered();
                _context.SetDataSorted();
            }
            else
            {
                result = ToSimpleListContactWrapper(DaoFactory.ContactDao.GetContacts(
                    searchString,
                    tags,
                    contactStageInt,
                    contactTypeInt,
                    contactListView,
                    fromDate,
                    toDate,
                    0,
                    0,
                    null,
                    responsibleid,
                    isShared));
            }

            int totalCount;

            if (result.Count() < count)
            {
                totalCount = fromIndex + result.Count();
            }
            else
            {
                totalCount = DaoFactory.ContactDao.GetContactsCount(
                    searchString,
                    tags,
                    contactStageInt,
                    contactTypeInt,
                    contactListView,
                    fromDate,
                    toDate,
                    responsibleid,
                    isShared);
            }

            _context.SetTotalCount(totalCount);

            return result;
        }

        /// <summary>
        /// Returns a group of contacts with the IDs specified in the request and their emails.
        /// </summary>
        /// <param type="System.Collections.Generic.IEnumerable{System.Int32}, System.Collections.Generic" name="contactids">List of contact IDs</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <short>Get contacts with emails</short> 
        /// <category>Contacts</category>
        /// <returns>
        /// List of contacts with their emails
        /// </returns>
        /// <path>api/2.0/crm/contact/mail</path>
        /// <httpMethod>GET</httpMethod>
        /// <collection>list</collection>
        /// <visible>false</visible>
        [Read(@"contact/mail")]
        public IEnumerable<ContactBaseWithEmailWrapper> GetContactsForMail(IEnumerable<int> contactids)
        {
            if (contactids == null) throw new ArgumentException();

            var contacts = DaoFactory.ContactDao.GetContacts(contactids.ToArray());

            var result = contacts.Select(ToContactBaseWithEmailWrapper);
            return result;
        }

        /// <summary>
        /// Deletes a list of all the contacts in the CRM module matching the parameters specified in the request.
        /// </summary>
        /// <param type="System.Collections.Generic.IEnumerable{System.String}, System.Collections.Generic" optional="true" name="tags">Contact tags</param>
        /// <param type="System.Int32, System" optional="true" name="contactStage">Contact stage ID (warmth)</param>
        /// <param type="System.Int32, System" optional="true" name="contactType">Contact type ID</param>
        /// <param type="ASC.CRM.Core.ContactListViewType, ASC.CRM.Core" optional="true" name="contactListView" remark="Allowed values: Company, Person, WithOpportunity">Contact list view</param>
        /// <param type="ASC.Specific.ApiDateTime, ASC.Specific" optional="true" name="fromDate">Start date</param>
        /// <param type="ASC.Specific.ApiDateTime, ASC.Specific" optional="true" name="toDate">End date</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <short>Delete contacts by parameters</short> 
        /// <category>Contacts</category>
        /// <returns type="ASC.Api.CRM.Wrappers.ContactBaseWrapper, ASC.Api.CRM">
        /// List of contacts
        /// </returns>
        /// <path>api/2.0/crm/contact/filter</path>
        /// <httpMethod>DELETE</httpMethod>
        /// <collection>list</collection>
        [Delete(@"contact/filter")]
        public IEnumerable<ContactBaseWrapper> DeleteBatchContacts(
            IEnumerable<String> tags,
            int? contactStage,
            int? contactType,
            ContactListViewType contactListView,
            ApiDateTime fromDate,
            ApiDateTime toDate)
        {
            int contactStageInt = contactStage.HasValue ? contactStage.Value : -1;
            int contactTypeInt = contactType.HasValue ? contactType.Value : -1;


            var contacts = DaoFactory.ContactDao.GetContacts(
                _context.FilterValue,
                tags,
                contactStageInt,
                contactTypeInt,
                contactListView,
                fromDate,
                toDate,
                0,
                0,
                null);

            contacts = DaoFactory.ContactDao.DeleteBatchContact(contacts);

            MessageService.Send(Request, MessageAction.ContactsDeleted, MessageTarget.Create(contacts.Select(c => c.ID)), contacts.Select(c => c.GetTitle()));

            return contacts.Select(ToContactBaseWrapper);
        }


        /// <summary>
        /// Returns a list of all the persons linked to the company with the ID specified in the request.
        /// </summary>
        /// <param type="System.Int32, System" method="url" name="companyid">Company ID</param>
        /// <exception cref="ArgumentException"></exception>
        /// <short>Get company persons</short> 
        /// <category>Contacts</category>
        /// <returns type="ASC.Api.CRM.Wrappers.ContactWrapper, ASC.Api.CRM">
        /// Company persons
        /// </returns>
        /// <path>api/2.0/crm/contact/company/{companyid}/person</path>
        /// <httpMethod>GET</httpMethod>
        /// <collection>list</collection>
        [Read(@"contact/company/{companyid:[0-9]+}/person")]
        public IEnumerable<ContactWrapper> GetPeopleFromCompany(int companyid)
        {
            if (companyid <= 0) throw new ArgumentException();

            var company = DaoFactory.ContactDao.GetByID(companyid);
            if (company == null || !CRMSecurity.CanAccessTo(company)) throw new ItemNotFoundException();

            return ToListContactWrapper(DaoFactory.ContactDao.GetMembers(companyid));
        }

        /// <summary>
        /// Adds the selected person to the company with the ID specified in the request.
        /// </summary>
        /// <param type="System.Int32, System" method="url" optional="true"  name="companyid">Company ID</param>
        /// <param type="System.Int32, System" optional="true" name="personid">Person ID</param>
        /// <short>Add a person to the company</short> 
        /// <category>Contacts</category>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <returns type="ASC.Api.CRM.Wrappers.PersonWrapper, ASC.Api.CRM">
        /// Person
        /// </returns>
        /// <path>api/2.0/crm/contact/company/{companyid}/person</path>
        /// <httpMethod>POST</httpMethod>
        [Create(@"contact/company/{companyid:[0-9]+}/person")]
        public PersonWrapper AddPeopleToCompany(int companyid, int personid)
        {
            if ((companyid <= 0) || (personid <= 0)) throw new ArgumentException();

            var company = DaoFactory.ContactDao.GetByID(companyid);
            var person = DaoFactory.ContactDao.GetByID(personid) as Person;

            if (person == null || company == null || !CRMSecurity.CanAccessTo(person) || !CRMSecurity.CanAccessTo(company)) throw new ItemNotFoundException();

            DaoFactory.ContactDao.AddMember(personid, companyid);
            MessageService.Send(Request, MessageAction.CompanyLinkedPerson, MessageTarget.Create(new[] { company.ID, person.ID }), company.GetTitle(), person.GetTitle());

            return (PersonWrapper)ToContactWrapper(person);
        }

        /// <summary>
        /// Deletes the selected person from the company with the ID specified in the request.
        /// </summary>
        /// <param type="System.Int32, System" method="url" name="companyid">Company ID</param>
        /// <param type="System.Int32, System" optional="true" name="personid">Person ID</param>
        /// <short>Delete a person from the company</short> 
        /// <category>Contacts</category>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <returns type="ASC.Api.CRM.Wrappers.PersonWrapper, ASC.Api.CRM">
        /// Person
        /// </returns>
        /// <path>api/2.0/crm/contact/company/{companyid}/person</path>
        /// <httpMethod>DELETE</httpMethod>
        [Delete(@"contact/company/{companyid:[0-9]+}/person")]
        public PersonWrapper DeletePeopleFromCompany(int companyid, int personid)
        {
            if ((companyid <= 0) || (personid <= 0)) throw new ArgumentException();

            var company = DaoFactory.ContactDao.GetByID(companyid);
            var person = DaoFactory.ContactDao.GetByID(personid);
            if (person == null || company == null || !CRMSecurity.CanAccessTo(person) || !CRMSecurity.CanAccessTo(company)) throw new ItemNotFoundException();

            DaoFactory.ContactDao.RemoveMember(personid);

            MessageService.Send(Request, MessageAction.CompanyUnlinkedPerson, MessageTarget.Create(new[] { company.ID, person.ID }), company.GetTitle(), person.GetTitle());

            return (PersonWrapper)ToContactWrapper(person);
        }

        /// <summary>
        /// Creates a person with the parameters (first name, last name, description, etc.) specified in the request.
        /// </summary>
        /// <param type="System.String, System" name="firstName">First name</param>
        /// <param type="System.String, System" name="lastName">Last name</param>
        /// <param type="System.String, System" optional="true"  name="jobTitle">Job title</param>
        /// <param type="System.Int32, System" optional="true" name="companyId">Company ID</param>
        /// <param type="System.String, System" optional="true" name="about">Person description text</param>
        /// <param type="ASC.Web.CRM.Core.Enums.ShareType, ASC.Web.CRM.Core.Enums" name="shareType">Person privacy: 0 - not shared, 1 - shared for reading/writing, 2 - shared for reading only</param>
        /// <param type="System.Collections.Generic.IEnumerable{System.Guid}, System.Collections.Generic" optional="true" name="managerList">List of person managers</param>
        /// <param type="System.Collections.Generic.IEnumerable{ASC.Api.Collections.ItemKeyValuePair{System.Int32, System.String}}" optional="true" name="customFieldList">Custom field list</param>
        /// <param type="System.Collections.Generic.IEnumerable{System.Web.HttpPostedFileBase}, System.Collections.Generic" optional="true" name="photo">Contact photo (upload using multipart/form-data)</param>
        /// <short>Create a person</short> 
        /// <category>Contacts</category>
        /// <returns type="ASC.Api.CRM.Wrappers.PersonWrapper, ASC.Api.CRM">Person</returns>
        /// <httpMethod>POST</httpMethod>
        /// <path>api/2.0/crm/contact/person</path>
        /// <exception cref="ArgumentException"></exception>
        [Create(@"contact/person")]
        public PersonWrapper CreatePerson(
            string firstName,
            string lastName,
            string jobTitle,
            int companyId,
            string about,
            ShareType shareType,
            IEnumerable<Guid> managerList,
            IEnumerable<ItemKeyValuePair<int, string>> customFieldList,
            IEnumerable<HttpPostedFileBase> photo)
        {
            if (companyId > 0)
            {
                var company = DaoFactory.ContactDao.GetByID(companyId);
                if (company == null || !CRMSecurity.CanAccessTo(company)) throw new ItemNotFoundException();
            }

            var peopleInst = new Person
            {
                FirstName = firstName,
                LastName = lastName,
                JobTitle = jobTitle,
                CompanyID = companyId,
                About = about,
                ShareType = shareType
            };

            peopleInst.ID = DaoFactory.ContactDao.SaveContact(peopleInst);
            peopleInst.CreateBy = Core.SecurityContext.CurrentAccount.ID;
            peopleInst.CreateOn = DateTime.UtcNow;

            var managerListLocal = managerList != null ? managerList.ToList() : new List<Guid>();
            if (managerListLocal.Any())
            {
                CRMSecurity.SetAccessTo(peopleInst, managerListLocal);
            }

            if (customFieldList != null)
            {
                foreach (var field in customFieldList)
                {
                    if (string.IsNullOrEmpty(field.Value)) continue;
                    DaoFactory.CustomFieldDao.SetFieldValue(EntityType.Person, peopleInst.ID, field.Key, field.Value);
                }
            }

            var wrapper = (PersonWrapper)ToContactWrapper(peopleInst);

            var photoList = photo != null ? photo.ToList() : new List<HttpPostedFileBase>();
            if (photoList.Any())
            {
                wrapper.SmallFotoUrl = ChangeContactPhoto(peopleInst.ID, photoList);
            }

            MessageService.Send(Request, MessageAction.PersonCreated, MessageTarget.Create(peopleInst.ID), peopleInst.GetTitle());

            return wrapper;
        }

        /// <summary>
        /// Changes a photo for the contact with the ID specified in the request.
        /// </summary>
        /// <param type="System.Int32, System" method="url" name="contactid">Contact ID</param>
        /// <param type="System.Collections.Generic.IEnumerable{System.Web.HttpPostedFileBase}, System.Collections.Generic" name="photo">Contact photo (upload using multipart/form-data)</param>
        /// <short>Change a contact photo</short> 
        /// <category>Contacts</category>
        /// <exception cref="ArgumentException"></exception>
        /// <returns>
        /// Path to the contact photo
        /// </returns>
        /// <path>api/2.0/crm/contact/{contactid}/changephoto</path>
        /// <httpMethod>PUT</httpMethod>
        [Update(@"contact/{contactid:[0-9]+}/changephoto")]
        public string ChangeContactPhoto(int contactid, IEnumerable<HttpPostedFileBase> photo)
        {
            if (contactid <= 0)
                throw new ArgumentException();

            var contact = DaoFactory.ContactDao.GetByID(contactid);
            if (contact == null || !CRMSecurity.CanAccessTo(contact))
                throw new ItemNotFoundException();

            var firstPhoto = photo != null ? photo.FirstOrDefault() : null;
            if (firstPhoto == null)
                throw new ArgumentException();

            if (firstPhoto.ContentLength == 0 ||
                !firstPhoto.ContentType.StartsWith("image/") ||
                !firstPhoto.InputStream.CanRead)
                throw new InvalidOperationException(CRMErrorsResource.InvalidFile);

            if (SetupInfo.MaxImageUploadSize > 0 &&
                SetupInfo.MaxImageUploadSize < firstPhoto.ContentLength)
                throw new Exception(FileSizeComment.GetFileImageSizeNote(CRMCommonResource.ErrorMessage_UploadFileSize, false));

            return ContactPhotoManager.UploadPhoto(firstPhoto.InputStream, contactid, false).Url;
        }

        /// <summary>
        /// Changes a photo using its URL for the contact with the ID specified in the request.
        /// </summary>
        /// <param type="System.Int32, System" method="url" name="contactid">Contact ID</param>
        /// <param type="System.String, System" name="photourl">Contact photo URL</param>
        /// <short>Change a contact photo by URL</short> 
        /// <category>Contacts</category>
        /// <exception cref="ArgumentException"></exception>
        /// <returns>
        /// Path to the contact photo
        /// </returns>
        /// <path>api/2.0/crm/contact/{contactid}/changephotobyurl</path>
        /// <httpMethod>PUT</httpMethod>
        [Update(@"contact/{contactid:[0-9]+}/changephotobyurl")]
        public string ChangeContactPhoto(int contactid, string photourl)
        {
            if (contactid <= 0 || string.IsNullOrEmpty(photourl)) throw new ArgumentException();

            var contact = DaoFactory.ContactDao.GetByID(contactid);
            if (contact == null || !CRMSecurity.CanAccessTo(contact)) throw new ItemNotFoundException();

            return ContactPhotoManager.UploadPhoto(photourl, contactid, false).Url;
        }

        /// <summary>
        /// Merges two contacts specified in the request.
        /// </summary>
        /// <param type="System.Int32, System" name="fromcontactid">The first contact ID to merge</param>
        /// <param type="System.Int32, System" name="tocontactid">The second contact ID to merge</param>
        /// <short>Merge contacts</short> 
        /// <category>Contacts</category>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <exception cref="SecurityException"></exception>
        /// <returns type="ASC.Api.CRM.Wrappers.ContactWrapper, ASC.Api.CRM">
        /// Contact
        /// </returns>
        /// <path>api/2.0/crm/contact/merge</path>
        /// <httpMethod>PUT</httpMethod>
        [Update(@"contact/merge")]
        public ContactWrapper MergeContacts(int fromcontactid, int tocontactid)
        {
            if (fromcontactid <= 0 || tocontactid <= 0) throw new ArgumentException();

            var fromContact = DaoFactory.ContactDao.GetByID(fromcontactid);
            var toContact = DaoFactory.ContactDao.GetByID(tocontactid);

            if (fromContact == null || toContact == null) throw new ItemNotFoundException();

            if (!CRMSecurity.CanEdit(fromContact) || !CRMSecurity.CanEdit(toContact)) throw CRMSecurity.CreateSecurityException();

            DaoFactory.ContactDao.MergeDublicate(fromcontactid, tocontactid);
            var resultContact = DaoFactory.ContactDao.GetByID(tocontactid);

            var messageAction = resultContact is Person ? MessageAction.PersonsMerged : MessageAction.CompaniesMerged;
            MessageService.Send(Request, messageAction, MessageTarget.Create(new[] { fromContact.ID, toContact.ID }), fromContact.GetTitle(), toContact.GetTitle());

            return ToContactWrapper(resultContact);
        }

        /// <summary>
        /// Updates the selected person with the parameters (first name, last name, description, etc.) specified in the request.
        /// </summary>
        /// <param type="System.Int32, System" method="url" name="personid">Person ID</param>
        /// <param type="System.String, System" name="firstName">New first name</param>
        /// <param type="System.String, System" name="lastName">New last name</param>
        /// <param type="System.String, System" optional="true"  name="jobTitle">New job title</param>
        /// <param type="System.Int32, System" optional="true" name="companyId">New company ID</param>
        /// <param type="System.String, System" optional="true" name="about">New person description text</param>
        /// <param type="ASC.Web.CRM.Core.Enums.ShareType, ASC.Web.CRM.Core.Enums" name="shareType">New person privacy: 0 - not shared, 1 - shared for reading/writing, 2 - shared for reading only</param>
        /// <param type="System.Collections.Generic.IEnumerable{System.Guid}, System.Collections.Generic" optional="true" name="managerList">New list of person managers</param>
        /// <param type="System.Collections.Generic.IEnumerable{ASC.Api.Collections.ItemKeyValuePair{System.Int32, System.String}}" optional="true" name="customFieldList">New custom field list</param>
        /// <param type="System.Collections.Generic.IEnumerable{System.Web.HttpPostedFileBase}, System.Collections.Generic" optional="true" name="photo">New contact photo (upload using multipart/form-data)</param>
        /// <short>Update a person</short> 
        /// <category>Contacts</category>
        /// <returns type="ASC.Api.CRM.Wrappers.PersonWrapper, ASC.Api.CRM">Person</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <path>api/2.0/crm/contact/person/{personid}</path>
        /// <httpMethod>PUT</httpMethod>
        [Update(@"contact/person/{personid:[0-9]+}")]
        public PersonWrapper UpdatePerson(
            int personid,
            string firstName,
            string lastName,
            string jobTitle,
            int companyId,
            string about,
            ShareType shareType,
            IEnumerable<Guid> managerList,
            IEnumerable<ItemKeyValuePair<int, string>> customFieldList,
            IEnumerable<HttpPostedFileBase> photo)
        {
            if (personid <= 0 || string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName)) throw new ArgumentException();

            var peopleInst = new Person
            {
                ID = personid,
                FirstName = firstName,
                LastName = lastName,
                JobTitle = jobTitle,
                CompanyID = companyId,
                About = about,
                ShareType = shareType
            };

            DaoFactory.ContactDao.UpdateContact(peopleInst);

            peopleInst = (Person)DaoFactory.ContactDao.GetByID(peopleInst.ID);

            var managerListLocal = managerList != null ? managerList.ToList() : new List<Guid>();
            if (managerListLocal.Any())
            {
                CRMSecurity.SetAccessTo(peopleInst, managerListLocal);
            }

            if (customFieldList != null)
            {
                var existingCustomFieldList = DaoFactory.CustomFieldDao.GetFieldsDescription(EntityType.Person).Select(fd => fd.ID).ToList();
                foreach (var field in customFieldList)
                {
                    if (string.IsNullOrEmpty(field.Value) || !existingCustomFieldList.Contains(field.Key)) continue;
                    DaoFactory.CustomFieldDao.SetFieldValue(EntityType.Person, peopleInst.ID, field.Key, field.Value);
                }
            }

            var wrapper = (PersonWrapper)ToContactWrapper(peopleInst);

            var photoList = photo != null ? photo.ToList() : new List<HttpPostedFileBase>();
            if (photoList.Any())
            {
                wrapper.SmallFotoUrl = ChangeContactPhoto(peopleInst.ID, photoList);
            }

            MessageService.Send(Request, MessageAction.PersonUpdated, MessageTarget.Create(peopleInst.ID), peopleInst.GetTitle());

            return wrapper;
        }

        /// <summary>
        /// Creates a company with the parameters specified in the request.
        /// </summary>
        /// <param type="System.String, System" name="companyName">Company name</param>
        /// <param type="System.String, System" optional="true" name="about">Company description text</param>
        /// <param type="System.Collections.Generic.IEnumerable{System.Int32}, System.Collections.Generic" optional="true" name="personList">List of persons linked to the company</param>
        /// <param type="ASC.Web.CRM.Core.Enums.ShareType, ASC.Web.CRM.Core.Enums" name="shareType">Company privacy: 0 - not shared, 1 - shared for reading/writing, 2 - shared for reading only</param>
        /// <param type="System.Collections.Generic.IEnumerable{System.Guid}, System.Collections.Generic" optional="true" name="managerList">List of company managers</param>
        /// <param type="System.Collections.Generic.IEnumerable{ASC.Api.Collections.ItemKeyValuePair{System.Int32, System.String}}" optional="true" name="customFieldList">Custom field list</param>
        /// <param type="System.Collections.Generic.IEnumerable{System.Web.HttpPostedFileBase}, System.Collections.Generic" optional="true" name="photo">Contact photo (upload using multipart/form-data)</param>
        /// <short>Create a company</short> 
        /// <category>Contacts</category>
        /// <returns type="ASC.Api.CRM.Wrappers.CompanyWrapper, ASC.Api.CRM">Company</returns>
        /// <path>api/2.0/crm/contact/company</path>
        /// <httpMethod>POST</httpMethod>
        /// <exception cref="ArgumentException"></exception>
        [Create(@"contact/company")]
        public CompanyWrapper CreateCompany(
            string companyName,
            string about,
            IEnumerable<int> personList,
            ShareType shareType,
            IEnumerable<Guid> managerList,
            IEnumerable<ItemKeyValuePair<int, string>> customFieldList,
            IEnumerable<HttpPostedFileBase> photo)
        {
            var companyInst = new Company
            {
                CompanyName = companyName,
                About = about,
                ShareType = shareType
            };

            companyInst.ID = DaoFactory.ContactDao.SaveContact(companyInst);
            companyInst.CreateBy = Core.SecurityContext.CurrentAccount.ID;
            companyInst.CreateOn = DateTime.UtcNow;

            if (personList != null)
            {
                foreach (var personID in personList)
                {
                    var person = DaoFactory.ContactDao.GetByID(personID) as Person; ;
                    if (person == null || !CRMSecurity.CanAccessTo(person)) continue;

                    AddPeopleToCompany(companyInst.ID, personID);
                }
            }

            var managerListLocal = managerList != null ? managerList.ToList() : new List<Guid>();
            if (managerListLocal.Any())
            {
                CRMSecurity.SetAccessTo(companyInst, managerListLocal);
            }

            if (customFieldList != null)
            {
                var existingCustomFieldList = DaoFactory.CustomFieldDao.GetFieldsDescription(EntityType.Company).Select(fd => fd.ID).ToList();
                foreach (var field in customFieldList)
                {
                    if (string.IsNullOrEmpty(field.Value) || !existingCustomFieldList.Contains(field.Key)) continue;
                    DaoFactory.CustomFieldDao.SetFieldValue(EntityType.Company, companyInst.ID, field.Key, field.Value);
                }
            }

            var wrapper = (CompanyWrapper)ToContactWrapper(companyInst);

            var photoList = photo != null ? photo.ToList() : new List<HttpPostedFileBase>();
            if (photoList.Any())
            {
                wrapper.SmallFotoUrl = ChangeContactPhoto(companyInst.ID, photoList);
            }

            MessageService.Send(Request, MessageAction.CompanyCreated, MessageTarget.Create(companyInst.ID), companyInst.GetTitle());

            return wrapper;
        }

        /// <summary>
        /// Creates a list of companies with the names specified in the request.
        /// </summary>
        /// <short>
        /// Create companies
        /// </short>
        /// <param type="System.Collections.Generic.IEnumerable{System.String}, System.Collections.Generic" name="companyName">Company names</param>
        /// <category>Contacts</category>
        /// <returns type="ASC.Api.CRM.Wrappers.ContactBaseWrapper, ASC.Api.CRM">List of contacts</returns>
        /// <path>api/2.0/crm/contact/company/quick</path>
        /// <httpMethod>POST</httpMethod>
        /// <exception cref="ArgumentException"></exception>
        /// <collection>list</collection>
        [Create(@"contact/company/quick")]
        public IEnumerable<ContactBaseWrapper> CreateCompany(IEnumerable<string> companyName)
        {
            if (companyName == null) throw new ArgumentException();

            var contacts = new List<Contact>();
            var recordIndex = 0;

            foreach (var item in companyName)
            {
                if (string.IsNullOrEmpty(item)) continue;

                contacts.Add(new Company
                {
                    ID = recordIndex++,
                    CompanyName = item,
                    ShareType = ShareType.None
                });
            }

            if (contacts.Count == 0) return null;

            DaoFactory.ContactDao.SaveContactList(contacts);

            var selectedManagers = new List<Guid> { Core.SecurityContext.CurrentAccount.ID };

            foreach (var ct in contacts)
            {
                CRMSecurity.SetAccessTo(ct, selectedManagers);
            }

            return contacts.ConvertAll(ToContactBaseWrapper);
        }

        /// <summary>
        /// Creates a list of persons with the first and last names specified in the request.
        /// </summary>
        /// <short>
        /// Create persons
        /// </short>
        /// <param type="System.Collections.Generic.IEnumerable{ASC.Api.Collections.ItemKeyValuePair{System.String, System.String}}, System.Collections.Generic" name="data">Pairs: user first name, user last name</param>
        /// <remarks>
        /// <![CDATA[
        ///  Data has the following format:
        ///  [{key: 'First name 1', value: 'Last name 1'}, {key: 'First name 2', value: 'Last name 2'}].
        /// ]]>
        /// </remarks>
        /// <category>Contacts</category>
        /// <returns type="ASC.Api.CRM.Wrappers.ContactBaseWrapper, ASC.Api.CRM">List of contacts</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <path>api/2.0/crm/contact/person/quick</path>
        /// <httpMethod>POST</httpMethod>
        /// <collection>list</collection>
        [Create(@"contact/person/quick")]
        public IEnumerable<ContactBaseWrapper> CreatePerson(IEnumerable<ItemKeyValuePair<string, string>> data)
        {
            if (data == null) return null;

            var contacts = new List<Contact>();
            var recordIndex = 0;

            foreach (var item in data)
            {
                if (string.IsNullOrEmpty(item.Key) || string.IsNullOrEmpty(item.Value)) continue;

                contacts.Add(new Person
                {
                    ID = recordIndex++,
                    FirstName = item.Key,
                    LastName = item.Value,
                    ShareType = ShareType.None
                });
            }

            if (contacts.Count == 0) return null;

            DaoFactory.ContactDao.SaveContactList(contacts);

            var selectedManagers = new List<Guid> { Core.SecurityContext.CurrentAccount.ID };

            foreach (var ct in contacts)
            {
                CRMSecurity.SetAccessTo(ct, selectedManagers);
            }

            MessageService.Send(Request, MessageAction.PersonsCreated, MessageTarget.Create(contacts.Select(x => x.ID)), contacts.Select(x => x.GetTitle()));

            return contacts.ConvertAll(ToContactBaseWrapper);
        }

        /// <summary>
        /// Updates the selected company with the parameters specified in the request.
        /// </summary>
        /// <param type="System.Int32, System"  method="url" name="companyid">Company ID</param>
        /// <param type="System.String, System"  name="companyName">New company name</param>
        /// <param type="System.String, System"  optional="true" name="about">New company description text</param>
        /// <param type="ASC.Web.CRM.Core.Enums.ShareType, ASC.Web.CRM.Core.Enums" name="shareType">New company privacy: 0 - not shared, 1 - shared for reading/writnig, 2 - shared for reading only</param>
        /// <param type="System.Collections.Generic.IEnumerable{System.Guid}, System.Collections.Generic" optional="true" name="managerList">New list of company managers</param>
        /// <param type="System.Collections.Generic.IEnumerable{ASC.Api.Collections.ItemKeyValuePair{System.Int32, System.String}}" optional="true" name="customFieldList">New custom field list</param>
        /// <short>Update a company</short> 
        /// <category>Contacts</category>
        /// <exception cref="ArgumentException"></exception>
        /// <returns type="ASC.Api.CRM.Wrappers.CompanyWrapper, ASC.Api.CRM">
        /// Company
        /// </returns>
        /// <path>api/2.0/crm/contact/company/{companyid}</path>
        /// <httpMethod>PUT</httpMethod>
        [Update(@"contact/company/{companyid:[0-9]+}")]
        public CompanyWrapper UpdateCompany(
            int companyid,
            string companyName,
            string about,
            ShareType shareType,
            IEnumerable<Guid> managerList,
            IEnumerable<ItemKeyValuePair<int, string>> customFieldList)
        {
            var companyInst = new Company
            {
                ID = companyid,
                CompanyName = companyName,
                About = about,
                ShareType = shareType
            };

            DaoFactory.ContactDao.UpdateContact(companyInst);

            companyInst = (Company)DaoFactory.ContactDao.GetByID(companyInst.ID);

            var managerListLocal = managerList != null ? managerList.ToList() : new List<Guid>();
            if (managerListLocal.Any())
            {
                CRMSecurity.SetAccessTo(companyInst, managerListLocal);
            }

            if (customFieldList != null)
            {
                var existingCustomFieldList = DaoFactory.CustomFieldDao.GetFieldsDescription(EntityType.Company).Select(fd => fd.ID).ToList();
                foreach (var field in customFieldList)
                {
                    if (string.IsNullOrEmpty(field.Value) || !existingCustomFieldList.Contains(field.Key)) continue;
                    DaoFactory.CustomFieldDao.SetFieldValue(EntityType.Company, companyInst.ID, field.Key, field.Value);
                }
            }

            MessageService.Send(Request, MessageAction.CompanyUpdated, MessageTarget.Create(companyInst.ID), companyInst.GetTitle());

            return (CompanyWrapper)ToContactWrapper(companyInst);
        }

        /// <summary>
        /// Updates a status of the contact with the ID specified in the request.
        /// </summary>
        /// <param type="System.Int32, System" method="url" name="contactid">Contact ID</param>
        /// <param type="System.Int32, System" name="contactStatusid">New contact status ID</param>
        /// <short>Update a contact status by ID</short> 
        /// <category>Contacts</category>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <returns type="ASC.Api.CRM.Wrappers.ContactWrapper, ASC.Api.CRM">
        /// Contact
        /// </returns>
        /// <path>api/2.0/crm/contact/{contactid}/status</path>
        /// <httpMethod>PUT</httpMethod>
        [Update(@"contact/{contactid:[0-9]+}/status")]
        public ContactWrapper UpdateContactStatus(int contactid, int contactStatusid)
        {
            if (contactid <= 0 || contactStatusid < 0) throw new ArgumentException();

            var dao = DaoFactory.ContactDao;

            if (contactStatusid > 0)
            {
                var curListItem = DaoFactory.ListItemDao.GetByID(contactStatusid);
                if (curListItem == null) throw new ItemNotFoundException();
            }

            var companyInst = dao.GetByID(contactid);
            if (companyInst == null || !CRMSecurity.CanAccessTo(companyInst)) throw new ItemNotFoundException();

            if (!CRMSecurity.CanEdit(companyInst)) throw CRMSecurity.CreateSecurityException();

            dao.UpdateContactStatus(new List<int> { companyInst.ID }, contactStatusid);
            companyInst.StatusID = contactStatusid;

            var messageAction = companyInst is Company ? MessageAction.CompanyUpdatedTemperatureLevel : MessageAction.PersonUpdatedTemperatureLevel;
            MessageService.Send(Request, messageAction, MessageTarget.Create(companyInst.ID), companyInst.GetTitle());

            return ToContactWrapper(companyInst);
        }

        /// <summary>
        /// Updates a status of the selected company and all its participants.
        /// </summary>
        /// <param type="System.Int32, System" method="url" name="companyid">Company ID</param>
        /// <param type="System.Int32, System" name="contactStatusid">New contact status ID</param>
        /// <short>Update a company status</short> 
        /// <category>Contacts</category>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <returns type="ASC.Api.CRM.Wrappers.ContactWrapper, ASC.Api.CRM">
        /// Company
        /// </returns>
        /// <path>api/2.0/crm/contact/company/{companyid}/status</path>
        /// <httpMethod>PUT</httpMethod>
        [Update(@"contact/company/{companyid:[0-9]+}/status")]
        public ContactWrapper UpdateCompanyAndParticipantsStatus(int companyid, int contactStatusid)
        {
            if (companyid <= 0 || contactStatusid < 0) throw new ArgumentException();

            var dao = DaoFactory.ContactDao;

            if (contactStatusid > 0)
            {
                var curListItem = DaoFactory.ListItemDao.GetByID(contactStatusid);
                if (curListItem == null) throw new ItemNotFoundException();
            }

            var companyInst = dao.GetByID(companyid);
            if (companyInst == null || !CRMSecurity.CanAccessTo(companyInst)) throw new ItemNotFoundException();

            if (companyInst is Person) throw new Exception(CRMErrorsResource.ContactIsNotCompany);

            var forUpdateStatus = new List<int>();
            forUpdateStatus.Add(companyInst.ID);

            var members = dao.GetMembersIDsAndShareType(companyInst.ID);
            foreach (var m in members)
            {
                if (CRMSecurity.CanAccessTo(m.Key, EntityType.Person, m.Value, 0))
                {
                    forUpdateStatus.Add(m.Key);
                }
            }

            dao.UpdateContactStatus(forUpdateStatus, contactStatusid);

            MessageService.Send(Request, MessageAction.CompanyUpdatedTemperatureLevel, MessageTarget.Create(companyInst.ID), companyInst.GetTitle());
            MessageService.Send(Request, MessageAction.CompanyUpdatedPersonsTemperatureLevel, MessageTarget.Create(companyInst.ID), companyInst.GetTitle());

            return ToContactWrapper(companyInst);
        }

        /// <summary>
        /// Updates a status of the selected person, related company and all its participants.
        /// </summary>
        /// <param type="System.Int32, System" method="url" name="personid">Person ID</param>
        /// <param type="System.Int32, System" name="contactStatusid">New contact status ID</param>
        /// <short>Update a person and his company status</short> 
        /// <category>Contacts</category>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <returns type="ASC.Api.CRM.Wrappers.ContactWrapper, ASC.Api.CRM">
        /// Person
        /// </returns>
        /// <path>api/2.0/crm/contact/person/{personid}/status</path>
        /// <httpMethod>PUT</httpMethod>
        [Update(@"contact/person/{personid:[0-9]+}/status")]
        public ContactWrapper UpdatePersonAndItsCompanyStatus(int personid, int contactStatusid)
        {
            if (personid <= 0 || contactStatusid < 0) throw new ArgumentException();

            if (contactStatusid > 0)
            {
                var curListItem = DaoFactory.ListItemDao.GetByID(contactStatusid);
                if (curListItem == null) throw new ItemNotFoundException();
            }

            var dao = DaoFactory.ContactDao;

            var personInst = dao.GetByID(personid);
            if (personInst == null || !CRMSecurity.CanAccessTo(personInst)) throw new ItemNotFoundException();

            if (personInst is Company) throw new Exception(CRMErrorsResource.ContactIsNotPerson);

            var forUpdateStatus = new List<int>();

            var companyID = ((Person)personInst).CompanyID;
            if (companyID != 0)
            {
                var companyInst = dao.GetByID(companyID);
                if (companyInst == null) throw new ItemNotFoundException();

                if (!CRMSecurity.CanAccessTo(companyInst))
                {
                    forUpdateStatus.Add(personInst.ID);
                    dao.UpdateContactStatus(forUpdateStatus, contactStatusid);
                }
                else
                {
                    forUpdateStatus.Add(companyInst.ID);

                    var members = dao.GetMembersIDsAndShareType(companyInst.ID);
                    foreach (var m in members)
                    {
                        if (CRMSecurity.CanAccessTo(m.Key, EntityType.Person, m.Value, 0))
                        {
                            forUpdateStatus.Add(m.Key);
                        }
                    }
                    dao.UpdateContactStatus(forUpdateStatus, contactStatusid);
                }
            }
            else
            {
                forUpdateStatus.Add(personInst.ID);
                dao.UpdateContactStatus(forUpdateStatus, contactStatusid);
            }

            MessageService.Send(Request, MessageAction.PersonUpdatedTemperatureLevel, MessageTarget.Create(personInst.ID), personInst.GetTitle());
            MessageService.Send(Request, MessageAction.PersonUpdatedCompanyTemperatureLevel, MessageTarget.Create(personInst.ID), personInst.GetTitle());

            personInst = dao.GetByID(personInst.ID);
            return ToContactWrapper(personInst);
        }

        /// <summary>
        /// Returns access rights of the contact with the ID specified in the request.
        /// </summary>
        /// <param type="System.Int32, System" name="contactid">Contact ID</param>
        /// <short>Get contact access rights</short> 
        /// <category>Contacts</category>
        ///<exception cref="ArgumentException"></exception>
        ///<exception cref="ItemNotFoundException"></exception>
        ///<exception cref="SecurityException"></exception>
        /// <returns type="ASC.Api.Employee.EmployeeWraper, ASC.Api.Employee">List of contacts</returns>
        /// <path>api/2.0/crm/contact/{contactid}/access</path>
        /// <httpMethod>GET</httpMethod>
        /// <collection>list</collection>
        [Read(@"contact/{contactid:[0-9]+}/access")]
        public IEnumerable<EmployeeWraper> GetContactAccessList(int contactid)
        {
            if (contactid <= 0) throw new ArgumentException();

            var contact = DaoFactory.ContactDao.GetByID(contactid);

            if (contact == null) throw new ItemNotFoundException();

            if (!CRMSecurity.CanAccessTo(contact)) throw CRMSecurity.CreateSecurityException();

            return CRMSecurity.IsPrivate(contact)
                       ? CRMSecurity.GetAccessSubjectTo(contact)
                                    .Select(item => EmployeeWraper.Get(item.Key))
                       : new List<EmployeeWraper>();
        }

        /// <summary>
        /// Sets access rights to the contact with the ID specified in the request.
        /// </summary>
        /// <param type="System.Int32, System" method="url" name="contactid">Contact ID</param>
        /// <param type="System.Boolean, System" name="isShared">Contact privacy: private or not</param>
        /// <param type="System.Collections.Generic.IEnumerable{System.Guid}, System.Collections.Generic" name="managerList">List of managers</param>
        /// <short>Set contact access rights</short> 
        /// <category>Contacts</category>
        ///<exception cref="ArgumentException"></exception>
        ///<exception cref="SecurityException"></exception>
        ///<exception cref="ItemNotFoundException"></exception>
        /// <returns type="ASC.Api.CRM.Wrappers.ContactWrapper, ASC.Api.CRM">
        /// Contact
        /// </returns>
        /// <path>api/2.0/crm/contact/{contactid}/access</path>
        /// <httpMethod>PUT</httpMethod>
        [Update(@"contact/{contactid:[0-9]+}/access")]
        public ContactWrapper SetAccessToContact(int contactid, bool isShared, IEnumerable<Guid> managerList)
        {
            if (contactid <= 0) throw new ArgumentException();

            var contact = DaoFactory.ContactDao.GetByID(contactid);
            if (contact == null) throw new ItemNotFoundException();

            if (!CRMSecurity.CanEdit(contact)) throw CRMSecurity.CreateSecurityException();

            SetAccessToContact(contact, isShared, managerList, false);

            var wrapper = ToContactWrapper(contact);
            return wrapper;
        }

        private void SetAccessToContact(Contact contact, bool isShared, IEnumerable<Guid> managerList, bool isNotify)
        {
            var managerListLocal = managerList != null ? managerList.Distinct().ToList() : new List<Guid>();
            if (managerListLocal.Any())
            {
                if (isNotify)
                {
                    var notifyUsers = managerListLocal.Where(n => n != ASC.Core.SecurityContext.CurrentAccount.ID).ToArray();
                    if (contact is Person)
                        ASC.Web.CRM.Services.NotifyService.NotifyClient.Instance.SendAboutSetAccess(EntityType.Person, contact.ID, DaoFactory, notifyUsers);
                    else
                        ASC.Web.CRM.Services.NotifyService.NotifyClient.Instance.SendAboutSetAccess(EntityType.Company, contact.ID, DaoFactory, notifyUsers);

                }

                CRMSecurity.SetAccessTo(contact, managerListLocal);
            }
            else
            {
                CRMSecurity.MakePublic(contact);
            }

            DaoFactory.ContactDao.MakePublic(contact.ID, isShared);
        }

        /// <summary>
        /// Sets access rights to the list of contacts with the IDs specified in the request.
        /// </summary>
        /// <param type="System.Collections.Generic.IEnumerable{System.Int32}, System.Collections.Generic" name="contactid">List of contact IDs</param>
        /// <param type="System.Boolean, System" name="isShared">Company privacy: shared or not</param>
        /// <param type="System.Collections.Generic.IEnumerable{System.Guid}, System.Collections.Generic" name="managerList">List of managers</param>
        /// <short>Set access rights to the contacts by IDs</short> 
        /// <category>Contacts</category>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <returns type="ASC.Api.CRM.Wrappers.ContactWrapper, ASC.Api.CRM">
        /// List of contacts
        /// </returns>
        /// <path>api/2.0/crm/contact/access</path>
        /// <httpMethod>PUT</httpMethod>
        /// <collection>list</collection>
        [Update(@"contact/access")]
        public IEnumerable<ContactWrapper> SetAccessToBatchContact(IEnumerable<int> contactid, bool isShared, IEnumerable<Guid> managerList)
        {
            if (contactid == null) throw new ArgumentException();

            var result = new List<ContactWrapper>();

            foreach (var id in contactid)
            {
                var contactWrapper = SetAccessToContact(id, isShared, managerList);
                result.Add(contactWrapper);
            }

            return result;
        }

        /// <summary>
        /// Sets access rights to the list of contacts with the parameters specified in the request.
        /// </summary>
        /// <param type="System.Collections.Generic.IEnumerable{System.String}, System.Collections.Generic" optional="true" name="tags">Contact tags</param>
        /// <param type="System.Nullable{System.Int32}, System" optional="true" name="contactStage">Contact stage ID (warmth)</param>
        /// <param type="System.Nullable{System.Int32}, System" optional="true" name="contactType">Contact type ID</param>
        /// <param type="ASC.CRM.Core.ContactListViewType, ASC.CRM.Core" optional="true" name="contactListView" remark="Allowed values: Company, Person, WithOpportunity">Contact list view</param>
        /// <param type="ASC.Specific.ApiDateTime, ASC.Specific" optional="true" name="fromDate">Start date</param>
        /// <param type="ASC.Specific.ApiDateTime, ASC.Specific" optional="true" name="toDate">End date</param>
        /// <param type="System.Boolean, System" name="isPrivate">Contact privacy: private or not</param>
        /// <param type="System.Collections.Generic.IEnumerable{System.Guid}, System.Collections.Generic" name="managerList">List of managers</param>
        /// <short>Set access rights to the contacts by parameters</short> 
        /// <category>Contacts</category>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <returns type="ASC.Api.CRM.Wrappers.ContactWrapper, ASC.Api.CRM">
        /// List of contacts
        /// </returns>
        /// <path>api/2.0/crm/contact/filter/access</path>
        /// <httpMethod>PUT</httpMethod>
        /// <collection>list</collection>
        [Update(@"contact/filter/access")]
        public IEnumerable<ContactWrapper> SetAccessToBatchContact(
            IEnumerable<String> tags,
            int? contactStage,
            int? contactType,
            ContactListViewType contactListView,
            ApiDateTime fromDate,
            ApiDateTime toDate,
            bool isPrivate,
            IEnumerable<Guid> managerList
            )
        {
            int contactStageInt = contactStage.HasValue ? contactStage.Value : -1;
            int contactTypeInt = contactType.HasValue ? contactType.Value : -1;

            var result = new List<Contact>();

            var contacts = DaoFactory.ContactDao.GetContacts(
                _context.FilterValue,
                tags,
                contactStageInt,
                contactTypeInt,
                contactListView,
                fromDate, toDate,
                0, 0, null);

            if (!contacts.Any())
                return Enumerable.Empty<ContactWrapper>();

            foreach (var contact in contacts)
            {
                if (contact == null)
                    throw new ItemNotFoundException();

                if (!CRMSecurity.CanEdit(contact)) continue;

                SetAccessToContact(contact, isPrivate, managerList, false);

                result.Add(contact);
            }
            return ToListContactWrapper(result);
        }

        /// <summary>
        /// Deletes a contact with the ID specified in the request from the portal.
        /// </summary>
        /// <short>Delete a contact</short> 
        /// <category>Contacts</category>
        /// <param type="System.Int32, System" method="url" name="contactid">Contact ID</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <returns type="ASC.Api.CRM.Wrappers.ContactWrapper, ASC.Api.CRM">
        /// Contact
        /// </returns>
        /// <path>api/2.0/crm/contact/{contactid}</path>
        /// <httpMethod>DELETE</httpMethod>
        [Delete(@"contact/{contactid:[0-9]+}")]
        public ContactWrapper DeleteContact(int contactid)
        {
            if (contactid <= 0) throw new ArgumentException();

            var contact = DaoFactory.ContactDao.DeleteContact(contactid);
            if (contact == null) throw new ItemNotFoundException();

            var messageAction = contact is Person ? MessageAction.PersonDeleted : MessageAction.CompanyDeleted;
            MessageService.Send(Request, messageAction, MessageTarget.Create(contact.ID), contact.GetTitle());

            return ToContactWrapper(contact);
        }

        /// <summary>
        /// Deletes a group of contacts with the IDs specified in the request.
        /// </summary>
        /// <param type="System.Collections.Generic.IEnumerable{System.Int32}, System.Collections.Generic" name="contactids">List of contact IDs</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <short>Delete contacts by IDs</short> 
        /// <category>Contacts</category>
        /// <returns type="ASC.Api.CRM.Wrappers.ContactBaseWrapper, ASC.Api.CRM">
        /// List of contacts
        /// </returns>
        /// <path>api/2.0/crm/contact</path>
        /// <httpMethod>PUT</httpMethod>
        /// <collection>list</collection>
        [Update(@"contact")]
        public IEnumerable<ContactBaseWrapper> DeleteBatchContacts(IEnumerable<int> contactids)
        {
            if (contactids == null) throw new ArgumentException();

            var contacts = DaoFactory.ContactDao.DeleteBatchContact(contactids.ToArray());
            MessageService.Send(Request, MessageAction.ContactsDeleted, MessageTarget.Create(contactids), contacts.Select(c => c.GetTitle()));

            return contacts.Select(ToContactBaseWrapper);
        }

        /// <summary>
        /// Returns a list of 30 contacts from the CRM module with a prefix specified in the request.
        /// </summary>
        /// <short>Get contacts by prefix</short>
        /// <param type="System.String, System" optional="true" name="prefix">Contact prefix</param>
        /// <param type="System.Int32, System" optional="false" name="searchType" remark="Allowed values: -1 (Any), 0 (Company), 1 (Persons), 2 (PersonsWithoutCompany), 3 (CompaniesAndPersonsWithoutCompany)">Contact search type</param>
        /// <param type="ASC.CRM.Core.EntityType, ASC.CRM.Core" optional="true" name="entityType">Contact entity type</param>
        /// <param type="System.Int32, System" optional="true" name="entityID">Contact entity ID</param>
        /// <category>Contacts</category>
        /// <returns>
        /// List of contacts
        /// </returns>
        /// <path>api/2.0/crm/contact/byprefix</path>
        /// <httpMethod>GET</httpMethod>
        /// <collection>list</collection>
        /// <visible>false</visible>
        [Read(@"contact/byprefix")]
        public IEnumerable<ContactBaseWithPhoneWrapper> GetContactsByPrefix(string prefix, int searchType, EntityType entityType, int entityID)
        {
            var result = new List<ContactBaseWithPhoneWrapper>();
            var allContacts = new List<Contact>();

            if (entityID > 0)
            {
                var findedContacts = new List<Contact>();
                switch (entityType)
                {
                    case EntityType.Opportunity:
                        allContacts = DaoFactory.ContactDao.GetContacts(DaoFactory.DealDao.GetMembers(entityID));
                        break;
                    case EntityType.Case:
                        allContacts = DaoFactory.ContactDao.GetContacts(DaoFactory.CasesDao.GetMembers(entityID));
                        break;
                }

                foreach (var c in allContacts)
                {
                    var person = c as Person;
                    if (person != null)
                    {
                        var people = person;
                        if (Core.Users.UserFormatter.GetUserName(people.FirstName, people.LastName).IndexOf(prefix, StringComparison.Ordinal) != -1)
                        {
                            findedContacts.Add(person);
                        }
                    }
                    else
                    {
                        var company = (Company)c;
                        if (company.CompanyName.IndexOf(prefix, StringComparison.Ordinal) != -1)
                        {
                            findedContacts.Add(c);
                        }
                    }
                }
                result.AddRange(findedContacts.Select(ToContactBaseWithPhoneWrapper));
                _context.SetTotalCount(findedContacts.Count);
            }
            else
            {
                const int maxItemCount = 30;
                if (searchType < -1 || searchType > 3) throw new ArgumentException();

                allContacts = DaoFactory.ContactDao.GetContactsByPrefix(prefix, searchType, 0, maxItemCount);
                result.AddRange(allContacts.Select(ToContactBaseWithPhoneWrapper));
            }

            return result;
        }


        /// <summary>
        /// Returns a list of contacts from the CRM module with the contact information specified in the request.
        /// </summary>
        /// <param type="System.Nullable{ASC.CRM.Core.ContactInfoType}, System" method="url" optional="false" name="infoType">Contact information type</param>
        /// <param type="System.String, System" method="url" optional="false" name="data">Contact data</param>
        /// <param type="System.Nullable{System.Int32}, System" method="url" optional="true" name="category">Contact category</param>
        /// <param type="System.Nullable{System.Boolean}, System" method="url" optional="true" name="isPrimary">Contact importance: primary or not</param>
        /// <short>Get contacts by contact information</short>
        /// <category>Contacts</category>
        /// <remarks>Please note that if the contact data from the "data" parameter refers to one of the contact information types, then the "infoType" parameter must be specified. For example, the "Paris" contact information is related to the "Address" information type.</remarks>
        /// <returns type="ASC.Api.CRM.Wrappers.ContactWrapper, ASC.Api.CRM">
        /// List of contacts
        /// </returns>
        /// <path>api/2.0/crm/contact/bycontactinfo</path>
        /// <httpMethod>GET</httpMethod>
        /// <collection>list</collection>
        [Read(@"contact/bycontactinfo")]
        public IEnumerable<ContactWrapper> GetContactsByContactInfo(ContactInfoType? infoType, String data, int? category, bool? isPrimary)
        {
            if (!infoType.HasValue) throw new ArgumentException();

            var ids = DaoFactory.ContactDao.GetContactIDsByContactInfo(infoType.Value, data, category, isPrimary);

            var result = DaoFactory.ContactDao.GetContacts(ids.ToArray()).ConvertAll(ToContactWrapper);

            return result;
        }


        /// <summary>
        /// Deletes an avatar of the contact with the ID specified in the request.
        /// </summary>
        /// <param type="System.Int32, System" method="url" name="contactId">Contact ID</param>
        /// <param type="System.String, System" name="contactType">Contact type</param>
        /// <param type="System.Boolean, System" name="uploadOnly">Defines whether to upload a new avatar only or also delete an old one</param>
        /// <short>Delete a contact avatar</short>
        /// <category>Contacts</category>
        /// <returns>Default photo</returns>
        /// <path>api/2.0/crm/contact/{contactid}/avatar</path>
        /// <httpMethod>DELETE</httpMethod>
        [Delete(@"contact/{contactid:[0-9]+}/avatar")]
        public string DeleteContactAvatar(int contactId, string contactType, bool uploadOnly)
        {
            bool isCompany;

            if (contactId != 0)
            {
                var contact = DaoFactory.ContactDao.GetByID(contactId);
                if (contact == null || !CRMSecurity.CanAccessTo(contact)) throw new ItemNotFoundException();

                if (!CRMSecurity.CanEdit(contact)) throw CRMSecurity.CreateSecurityException();

                isCompany = contact is Company;
            }
            else
            {
                isCompany = contactType != "people";
            }

            if (!uploadOnly)
            {
                ContactPhotoManager.DeletePhoto(contactId);
                return ContactPhotoManager.GetBigSizePhoto(0, isCompany);
            }
            return "";
        }


        /// <summary>
        /// Sends a mail through SMTP to the contacts with the IDs specified in the request.
        /// </summary>
        /// <param type="System.Collections.Generic.List{System.Int32}, System.Collections.Generic" name="fileIDs">File IDs</param>
        /// <param type="System.Collections.Generic.List{System.Int32}, System.Collections.Generic" name="contactIds">Contact IDs</param>
        /// <param type="System.String, System" name="subject">Mail subject</param>
        /// <param type="System.String, System" name="body">Mail body</param>
        /// <param type="System.Boolean, System" name="storeInHistory" visible="false">Defines if a mail will be stored in the history or not</param>
        /// <short>Send a mail</short>
        /// <category>Contacts</category>
        /// <returns>Mail</returns>
        /// <path>api/2.0/crm/contact/mailsmtp/send</path>
        /// <httpMethod>POST</httpMethod>
        /// <visible>false</visible>
        [Create(@"contact/mailsmtp/send")]
        public IProgressItem SendMailSMTPToContacts(List<int> fileIDs, List<int> contactIds, String subject, String body, bool storeInHistory)
        {
            if (contactIds == null || contactIds.Count == 0 || String.IsNullOrEmpty(body)) throw new ArgumentException();

            var contacts = DaoFactory.ContactDao.GetContacts(contactIds.ToArray());
            MessageService.Send(Request, MessageAction.CrmSmtpMailSent, MessageTarget.Create(contactIds), contacts.Select(c => c.GetTitle()));

            return MailSender.Start(fileIDs, contactIds, subject, body, storeInHistory);
        }

        /// <summary>
        /// Returns a preview of a mail sent through SMTP to the contact with the ID specified in the request.
        /// </summary>
        /// <param type="System.String, System" name="template">Mail template</param>
        /// <param type="System.Int32, System" name="contactId">Contact ID</param>
        /// <short>Get a mail preview</short>
        /// <category>Contacts</category>
        /// <returns>Mail preview</returns>
        /// <path>api/2.0/crm/contact/mailsmtp/preview</path>
        /// <httpMethod>POST</httpMethod>
        /// <visible>false</visible>
        [Create(@"contact/mailsmtp/preview")]
        public string GetMailSMTPToContactsPreview(string template, int contactId)
        {
            if (contactId == 0 || String.IsNullOrEmpty(template)) throw new ArgumentException();

            var manager = new MailTemplateManager(DaoFactory);

            return manager.Apply(template, contactId);
        }


        /// <summary>
        /// Returns a status of a mail sent through SMTP to the current contact.
        /// </summary>
        /// <short>Get a mail status</short>
        /// <category>Contacts</category>
        /// <returns>Mail status</returns>
        /// <path>api/2.0/crm/contact/mailsmtp/status</path>
        /// <httpMethod>GET</httpMethod>
        /// <visible>false</visible>
        [Read(@"contact/mailsmtp/status")]
        public IProgressItem GetMailSMTPToContactsStatus()
        {
            return MailSender.GetStatus();
        }

        /// <summary>
        /// Cancels the mail sending through SMTP to the current contacts.
        /// </summary>
        /// <short>Cancel mail sending</short>
        /// <category>Contacts</category>
        /// <returns>Mail status</returns>
        /// <path>api/2.0/crm/contact/mailsmtp/cancel</path>
        /// <httpMethod>PUT</httpMethod>
        /// <visible>false</visible>
        [Update(@"contact/mailsmtp/cancel")]
        public IProgressItem CancelMailSMTPToContacts()
        {
            var progressItem = MailSender.GetStatus();
            MailSender.Cancel();
            return progressItem;
        }

        /// <summary>
        /// Sets the creation date of a contact with the ID specified in the request.
        /// </summary>
        /// <param type="System.Int32, System" name="contactId">Contact ID</param>
        /// <param type="ASC.Specific.ApiDateTime, ASC.Specific" name="creationDate">Contact creation date</param>
        /// <short>Set the contact creation date</short>
        /// <category>Contacts</category>
        /// <path>api/2.0/crm/contact/{contactid}/creationdate</path>
        /// <httpMethod>PUT</httpMethod>
        /// <visible>false</visible>
        [Update(@"contact/{contactid:[0-9]+}/creationdate")]
        public void SetContactCreationDate(int contactId, ApiDateTime creationDate)
        {
            var dao = DaoFactory.ContactDao;
            var contact = dao.GetByID(contactId);

            if (contact == null || !CRMSecurity.CanAccessTo(contact))
                throw new ItemNotFoundException();

            dao.SetContactCreationDate(contactId, creationDate);
        }

        /// <summary>
        /// Sets the last modified date of a contact with the ID specified in the request.
        /// </summary>
        /// <param type="System.Int32, System" name="contactId">Contact ID</param>
        /// <param type="ASC.Specific.ApiDateTime, ASC.Specific" name="lastModifedDate">Contact last modified date</param>
        /// <short>Set the contact last modified date</short>
        /// <path>api/2.0/crm/contact/{contactid}/lastmodifeddate</path>
        /// <httpMethod>PUT</httpMethod>
        /// <visible>false</visible>
        [Update(@"contact/{contactid:[0-9]+}/lastmodifeddate")]
        public void SetContactLastModifedDate(int contactId, ApiDateTime lastModifedDate)
        {
            var dao = DaoFactory.ContactDao;
            var contact = dao.GetByID(contactId);

            if (contact == null || !CRMSecurity.CanAccessTo(contact))
                throw new ItemNotFoundException();

            dao.SetContactLastModifedDate(contactId, lastModifedDate);
        }


        private IEnumerable<ContactWithTaskWrapper> ToSimpleListContactWrapper(IReadOnlyList<Contact> itemList)
        {
            if (itemList.Count == 0) return new List<ContactWithTaskWrapper>();

            var result = new List<ContactWithTaskWrapper>();

            var personsIDs = new List<int>();
            var companyIDs = new List<int>();
            var contactIDs = new int[itemList.Count];

            var peopleCompanyIDs = new List<int>();
            var peopleCompanyList = new Dictionary<int, ContactBaseWrapper>();

            var contactDao = DaoFactory.ContactDao;

            for (var index = 0; index < itemList.Count; index++)
            {
                var contact = itemList[index];

                if (contact is Company)
                {
                    companyIDs.Add(contact.ID);
                }
                else
                {
                    var person = contact as Person;
                    if (person != null)
                    {
                        personsIDs.Add(person.ID);

                        if (person.CompanyID > 0)
                        {
                            peopleCompanyIDs.Add(person.CompanyID);
                        }
                    }
                }

                contactIDs[index] = itemList[index].ID;
            }

            if (peopleCompanyIDs.Count > 0)
            {
                var tmpList = contactDao.GetContacts(peopleCompanyIDs.ToArray()).ConvertAll(item => ToContactBaseWrapperQuick(item));
                var tmpListCanDelete = contactDao.CanDelete(tmpList.Select(item => item.ID).ToArray());

                foreach (var contactBaseWrapperQuick in tmpList)
                {
                    contactBaseWrapperQuick.CanDelete = contactBaseWrapperQuick.CanEdit && tmpListCanDelete[contactBaseWrapperQuick.ID];
                    peopleCompanyList.Add(contactBaseWrapperQuick.ID, contactBaseWrapperQuick);
                }
            }

            var contactInfos = new Dictionary<int, List<ContactInfoWrapper>>();

            var addresses = new Dictionary<int, List<Address>>();

            DaoFactory.ContactInfoDao.GetAll(contactIDs).ForEach(
                item =>
                    {
                        if (item.InfoType == ContactInfoType.Address)
                        {
                            if (!addresses.ContainsKey(item.ContactID))
                            {
                                addresses.Add(item.ContactID, new List<Address>
                                    {
                                        new Address(item)
                                    });
                            }
                            else
                            {
                                addresses[item.ContactID].Add(new Address(item));
                            }
                        }
                        else
                        {
                            if (!contactInfos.ContainsKey(item.ContactID))
                            {
                                contactInfos.Add(item.ContactID, new List<ContactInfoWrapper> { new ContactInfoWrapper(item) });
                            }
                            else
                            {
                                contactInfos[item.ContactID].Add(new ContactInfoWrapper(item));
                            }
                        }
                    }
                );

            var nearestTasks = DaoFactory.TaskDao.GetNearestTask(contactIDs.ToArray());

            IEnumerable<TaskCategoryBaseWrapper> taskCategories = new List<TaskCategoryBaseWrapper>();

            if (nearestTasks.Any())
            {
                taskCategories = DaoFactory.ListItemDao.GetItems(ListType.TaskCategory).ConvertAll(item => new TaskCategoryBaseWrapper(item));
            }

            foreach (var contact in itemList)
            {
                ContactWrapper contactWrapper;

                var person = contact as Person;
                if (person != null)
                {
                    var people = person;

                    var peopleWrapper = PersonWrapper.ToPersonWrapperQuick(people);

                    if (people.CompanyID > 0 && peopleCompanyList.ContainsKey(people.CompanyID))
                    {
                        peopleWrapper.Company = peopleCompanyList[people.CompanyID];
                    }

                    contactWrapper = peopleWrapper;
                }
                else
                {
                    var company = contact as Company;
                    if (company != null)
                    {
                        contactWrapper = CompanyWrapper.ToCompanyWrapperQuick(company);
                    }
                    else
                    {
                        throw new ArgumentException();
                    }
                }

                contactWrapper.CommonData = contactInfos.ContainsKey(contact.ID) ? contactInfos[contact.ID] : new List<ContactInfoWrapper>();

                TaskBaseWrapper taskWrapper = null;

                if (nearestTasks.ContainsKey(contactWrapper.ID))
                {
                    var task = nearestTasks[contactWrapper.ID];
                    taskWrapper = new TaskBaseWrapper(task);

                    if (task.CategoryID > 0)
                    {
                        taskWrapper.Category = taskCategories.First(x => x.ID == task.CategoryID);
                    }
                }

                result.Add(new ContactWithTaskWrapper
                {
                    Contact = contactWrapper,
                    Task = taskWrapper
                });
            }


            #region CanDelete for main contacts

            if (result.Count > 0)
            {
                var resultListCanDelete = contactDao.CanDelete(result.Select(item => item.Contact.ID).ToArray());
                foreach (var contactBaseWrapperQuick in result)
                {
                    contactBaseWrapperQuick.Contact.CanDelete = contactBaseWrapperQuick.Contact.CanEdit && resultListCanDelete[contactBaseWrapperQuick.Contact.ID];
                }
            }

            #endregion

            return result;
        }

        private IEnumerable<ContactWrapper> ToListContactWrapper(IReadOnlyList<Contact> itemList)
        {
            if (itemList.Count == 0) return new List<ContactWrapper>();

            var result = new List<ContactWrapper>();

            var personsIDs = new List<int>();
            var companyIDs = new List<int>();
            var contactIDs = new int[itemList.Count];

            var peopleCompanyIDs = new List<int>();
            var peopleCompanyList = new Dictionary<int, ContactBaseWrapper>();


            var contactDao = DaoFactory.ContactDao;


            for (var index = 0; index < itemList.Count; index++)
            {
                var contact = itemList[index];

                if (contact is Company)
                {
                    companyIDs.Add(contact.ID);
                }
                else
                {
                    var person = contact as Person;
                    if (person != null)
                    {
                        personsIDs.Add(person.ID);

                        if (person.CompanyID > 0)
                        {
                            peopleCompanyIDs.Add(person.CompanyID);
                        }
                    }
                }

                contactIDs[index] = itemList[index].ID;
            }

            if (peopleCompanyIDs.Count > 0)
            {
                var tmpList = contactDao.GetContacts(peopleCompanyIDs.ToArray()).ConvertAll(item => ToContactBaseWrapperQuick(item));
                var tmpListCanDelete = contactDao.CanDelete(tmpList.Select(item => item.ID).ToArray());

                foreach (var contactBaseWrapperQuick in tmpList)
                {
                    contactBaseWrapperQuick.CanDelete = contactBaseWrapperQuick.CanEdit && tmpListCanDelete[contactBaseWrapperQuick.ID];
                    peopleCompanyList.Add(contactBaseWrapperQuick.ID, contactBaseWrapperQuick);
                }
            }

            var companiesMembersCount = contactDao.GetMembersCount(companyIDs.Distinct().ToArray());

            var contactStatusIDs = itemList.Select(item => item.StatusID).Distinct().ToArray();
            var contactInfos = new Dictionary<int, List<ContactInfoWrapper>>();

            var haveLateTask = DaoFactory.TaskDao.HaveLateTask(contactIDs);
            var contactStatus = DaoFactory.ListItemDao
                                          .GetItems(contactStatusIDs)
                                          .ToDictionary(item => item.ID, item => new ContactStatusBaseWrapper(item));

            var personsCustomFields = DaoFactory.CustomFieldDao.GetEntityFields(EntityType.Person, personsIDs.ToArray());
            var companyCustomFields = DaoFactory.CustomFieldDao.GetEntityFields(EntityType.Company, companyIDs.ToArray());

            var customFields = personsCustomFields.Union(companyCustomFields)
                                                  .GroupBy(item => item.EntityID).ToDictionary(item => item.Key, item => item.Select(ToCustomFieldBaseWrapper));

            var addresses = new Dictionary<int, List<Address>>();
            var taskCount = DaoFactory.TaskDao.GetTasksCount(contactIDs);

            var contactTags = DaoFactory.TagDao.GetEntitiesTags(EntityType.Contact);

            DaoFactory.ContactInfoDao.GetAll(contactIDs).ForEach(
                item =>
                    {
                        if (item.InfoType == ContactInfoType.Address)
                        {
                            if (!addresses.ContainsKey(item.ContactID))
                                addresses.Add(item.ContactID, new List<Address> { new Address(item) });
                            else
                                addresses[item.ContactID].Add(new Address(item));
                        }
                        else
                        {
                            if (!contactInfos.ContainsKey(item.ContactID))
                                contactInfos.Add(item.ContactID, new List<ContactInfoWrapper> { new ContactInfoWrapper(item) });
                            else
                                contactInfos[item.ContactID].Add(new ContactInfoWrapper(item));
                        }
                    }
                );


            foreach (var contact in itemList)
            {
                ContactWrapper contactWrapper;

                var person = contact as Person;
                if (person != null)
                {
                    var people = person;

                    var peopleWrapper = PersonWrapper.ToPersonWrapperQuick(people);

                    if (people.CompanyID > 0 && peopleCompanyList.ContainsKey(people.CompanyID))
                    {
                        peopleWrapper.Company = peopleCompanyList[people.CompanyID];
                    }

                    contactWrapper = peopleWrapper;
                }
                else
                {
                    var company = contact as Company;
                    if (company != null)
                    {
                        contactWrapper = CompanyWrapper.ToCompanyWrapperQuick(company);

                        if (companiesMembersCount.ContainsKey(contactWrapper.ID))
                        {
                            ((CompanyWrapper)contactWrapper).PersonsCount = companiesMembersCount[contactWrapper.ID];
                        }
                    }
                    else
                    {
                        throw new ArgumentException();
                    }
                }

                if (contactTags.ContainsKey(contact.ID))
                {
                    contactWrapper.Tags = contactTags[contact.ID].OrderBy(x => x);
                }

                if (addresses.ContainsKey(contact.ID))
                {
                    contactWrapper.Addresses = addresses[contact.ID];
                }

                contactWrapper.CommonData = contactInfos.ContainsKey(contact.ID) ? contactInfos[contact.ID] : new List<ContactInfoWrapper>();

                if (contactStatus.ContainsKey(contact.StatusID))
                {
                    contactWrapper.ContactStatus = contactStatus[contact.StatusID];
                }

                contactWrapper.HaveLateTasks = haveLateTask.ContainsKey(contact.ID) && haveLateTask[contact.ID];

                contactWrapper.CustomFields = customFields.ContainsKey(contact.ID) ? customFields[contact.ID] : new List<CustomFieldBaseWrapper>();

                contactWrapper.TaskCount = taskCount.ContainsKey(contact.ID) ? taskCount[contact.ID] : 0;

                result.Add(contactWrapper);
            }

            #region CanDelete for main contacts

            if (result.Count > 0)
            {
                var resultListCanDelete = contactDao.CanDelete(result.Select(item => item.ID).ToArray());
                foreach (var contactBaseWrapperQuick in result)
                {
                    contactBaseWrapperQuick.CanDelete = contactBaseWrapperQuick.CanEdit && resultListCanDelete[contactBaseWrapperQuick.ID];
                }
            }

            #endregion

            return result;
        }


        private static ContactBaseWrapper ToContactBaseWrapper(Contact contact)
        {
            return contact == null ? null : new ContactBaseWrapper(contact);
        }

        private static ContactBaseWrapper ToContactBaseWrapperQuick(Contact contact)
        {
            return contact == null ? null : ContactBaseWrapper.ToContactBaseWrapperQuick(contact);
        }

        private ContactWrapper ToContactWrapper(Contact contact)
        {
            ContactWrapper result;

            var person = contact as Person;
            if (person != null)
            {
                var peopleWrapper = new PersonWrapper(person);
                if (person.CompanyID > 0)
                {
                    peopleWrapper.Company = ToContactBaseWrapper(DaoFactory.ContactDao.GetByID(person.CompanyID));
                }

                result = peopleWrapper;
            }
            else
            {
                var company = contact as Company;
                if (company != null)
                {
                    result = new CompanyWrapper(company);
                    ((CompanyWrapper)result).PersonsCount = DaoFactory.ContactDao.GetMembersCount(result.ID);
                }
                else throw new ArgumentException();
            }

            if (contact.StatusID > 0)
            {
                var listItem = DaoFactory.ListItemDao.GetByID(contact.StatusID);
                if (listItem == null) throw new ItemNotFoundException();

                result.ContactStatus = new ContactStatusBaseWrapper(listItem);
            }

            result.TaskCount = DaoFactory.TaskDao.GetTasksCount(contact.ID);
            result.HaveLateTasks = DaoFactory.TaskDao.HaveLateTask(contact.ID);

            var contactInfos = new List<ContactInfoWrapper>();
            var addresses = new List<Address>();

            var data = DaoFactory.ContactInfoDao.GetList(contact.ID, null, null, null);

            foreach (var contactInfo in data)
            {
                if (contactInfo.InfoType == ContactInfoType.Address)
                {
                    addresses.Add(new Address(contactInfo));
                }
                else
                {
                    contactInfos.Add(new ContactInfoWrapper(contactInfo));
                }
            }

            result.Addresses = addresses;
            result.CommonData = contactInfos;

            if (contact is Person)
            {
                result.CustomFields = DaoFactory.CustomFieldDao
                                                .GetEntityFields(EntityType.Person, contact.ID, false)
                                                .ConvertAll(item => new CustomFieldBaseWrapper(item)).ToSmartList();
            }
            else
            {
                result.CustomFields = DaoFactory.CustomFieldDao
                                                .GetEntityFields(EntityType.Company, contact.ID, false)
                                                .ConvertAll(item => new CustomFieldBaseWrapper(item)).ToSmartList();
            }

            return result;
        }

        private ContactBaseWithEmailWrapper ToContactBaseWithEmailWrapper(Contact contact)
        {
            if (contact == null) return null;

            var result = new ContactBaseWithEmailWrapper(contact);
            var primaryEmail = DaoFactory.ContactInfoDao.GetList(contact.ID, ContactInfoType.Email, null, true);
            if (primaryEmail == null || primaryEmail.Count == 0)
            {
                result.Email = null;
            }
            else
            {
                result.Email = new ContactInfoWrapper(primaryEmail.FirstOrDefault());
            }
            return result;
        }

        private ContactBaseWithPhoneWrapper ToContactBaseWithPhoneWrapper(Contact contact)
        {
            if (contact == null) return null;

            var result = new ContactBaseWithPhoneWrapper(contact);
            var primaryPhone = DaoFactory.ContactInfoDao.GetList(contact.ID, ContactInfoType.Phone, null, true);
            if (primaryPhone == null || primaryPhone.Count == 0)
            {
                result.Phone = null;
            }
            else
            {
                result.Phone = new ContactInfoWrapper(primaryPhone.FirstOrDefault());
            }
            return result;
        }

    }
}