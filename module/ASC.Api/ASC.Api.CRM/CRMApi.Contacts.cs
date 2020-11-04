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
using ASC.Thrdparty;
using ASC.Thrdparty.Twitter;
using ASC.Web.CRM.Classes;
using ASC.Web.CRM.Classes.SocialMedia;
using ASC.Web.CRM.Core.Enums;
using ASC.Web.CRM.Resources;
using ASC.Web.CRM.SocialMedia;
using ASC.Web.Projects.Core;
using ASC.Web.Studio.Core;
using Autofac;
using Contact = ASC.CRM.Core.Entities.Contact;

namespace ASC.Api.CRM
{
    public partial class CRMApi
    {
        /// <summary>
        ///    Returns the detailed information about the contact with the ID specified in the request
        /// </summary>
        /// <param name="contactid">Contact ID</param>
        /// <returns>Contact</returns>
        /// <short>Get contact by ID</short> 
        /// <category>Contacts</category>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
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
        ///  Returns the contact list for the project with the ID specified in the request
        /// </summary>
        /// <short>
        ///  Get contacts by project ID
        /// </short>
        /// <param name="projectid">Project ID</param>
        /// <category>Contacts</category>
        /// <returns>
        ///     Contact list
        /// </returns>
        ///<exception cref="ArgumentException"></exception>
        [Read(@"contact/project/{projectid:[0-9]+}")]
        public IEnumerable<ContactWrapper> GetContactsByProjectID(int projectid)
        {
            if (projectid <= 0) throw new ArgumentException();

            var contacts = DaoFactory.ContactDao.GetContactsByProjectID(projectid);
            return ToListContactWrapper(contacts.ToList());
        }

        /// <summary>
        ///  Links the selected contact to the project with the ID specified in the request
        /// </summary>
        /// <param name="contactid">Contact ID</param>
        /// <param name="projectid">Project ID</param>
        /// <category>Contacts</category>
        /// <short>Link contact with project</short> 
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <returns>Contact Info</returns>
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

            DaoFactory.ContactDao.SetRelativeContactProject(new List<int> {contactid}, projectid);

            var messageAction = contact is Company ? MessageAction.ProjectLinkedCompany : MessageAction.ProjectLinkedPerson;
            MessageService.Send(Request, messageAction, MessageTarget.Create(contact.ID), project.Title, contact.GetTitle());

            return ToContactWrapper(contact);
        }

        /// <summary>
        ///  Links the selected contacts to the project with the ID specified in the request
        /// </summary>
        /// <param name="contactid">Contact IDs array</param>
        /// <param name="projectid">Project ID</param>
        /// <category>Contacts</category>
        /// <short>Link contact list with project</short> 
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <returns>
        ///    Contact list
        /// </returns>
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
        ///  Removes the link with the selected project from the contact with the ID specified in the request
        /// </summary>
        /// <param name="contactid">Contact ID</param>
        /// <param name="projectid">Project ID</param>
        /// <category>Contacts</category>
        /// <short>Remove contact from project</short> 
        /// <returns>
        ///    Contact info
        /// </returns>
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
        ///   Adds the selected opportunity to the contact with the ID specified in the request. The same as AddMemberToDeal
        /// </summary>
        /// <param name="opportunityid">Opportunity ID</param>
        /// <param name="contactid">Contact ID</param>
        /// <short>Add contact opportunity</short> 
        /// <category>Contacts</category>
        /// <exception cref="ArgumentException"></exception>
        /// <returns>
        ///    Opportunity
        /// </returns>
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
        ///   Deletes the selected opportunity from the contact with the ID specified in the request
        /// </summary>
        /// <param name="opportunityid">Opportunity ID</param>
        /// <param name="contactid">Contact ID</param>
        /// <short>Delete contact opportunity</short> 
        /// <category>Contacts</category>
        /// <exception cref="ArgumentException"></exception>
        /// <returns>
        ///    Opportunity
        /// </returns>
        [Delete(@"contact/{contactid:[0-9]+}/opportunity/{opportunityid:[0-9]+}")]
        public OpportunityWrapper DeleteDealFromContact(int contactid, int opportunityid)
        {
            if ((opportunityid <= 0) || (contactid <= 0)) throw new ArgumentException();

            var contact = DaoFactory.ContactDao.GetByID(contactid);
            if (contact == null || !CRMSecurity.CanAccessTo(contact)) throw new ItemNotFoundException();

            var opportunity = DaoFactory.DealDao.GetByID(opportunityid);
            if (opportunity == null || !CRMSecurity.CanAccessTo(opportunity)) throw new ItemNotFoundException();

            DaoFactory.DealDao.RemoveMember(opportunityid, contactid);

            return ToOpportunityWrapper(opportunity);
        }

        /// <summary>
        ///    Returns the list of all contacts in the CRM module matching the parameters specified in the request
        /// </summary>
        /// <param optional="true" name="tags">Tag</param>
        /// <param optional="true" name="contactStage">Contact stage ID (warmth)</param>
        /// <param optional="true" name="contactType">Contact type ID</param>
        /// <param optional="true" name="contactListView" remark="Allowed values: Company, Person, WithOpportunity"></param>
        /// <param optional="true" name="fromDate">Start date</param>
        /// <param optional="true" name="toDate">End date</param>
        /// <param optional="true" name="responsibleid">Responsible ID</param>
        /// <param optional="true" name="isShared">Responsible ID</param>
        /// <short>Get contact list</short> 
        /// <category>Contacts</category>
        /// <returns>
        ///    Contact list
        /// </returns>
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
        ///    Returns the list of the contacts for auto complete feature.
        /// </summary>
        /// <param name="term">String part of contact name, lastname or email.</param>
        /// <param name="maxCount">Max result count</param>
        /// <short>Search contact list</short> 
        /// <category>Contacts</category>
        /// <returns>
        ///    Contact list
        /// </returns>
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
        ///    Returns the list of all contacts in the CRM module matching the parameters specified in the request
        /// </summary>
        /// <param optional="true" name="tags">Tag</param>
        /// <param optional="true" name="contactStage">Contact stage ID (warmth)</param>
        /// <param optional="true" name="contactType">Contact type ID</param>
        /// <param optional="true" name="contactListView" remark="Allowed values: Company, Person, WithOpportunity"></param>
        /// <param optional="true" name="responsibleid">Responsible ID</param>
        /// <param optional="true" name="isShared">Responsible ID</param>
        /// <param optional="true" name="fromDate">Start date</param>
        /// <param optional="true" name="toDate">End date</param>
        /// <short>Get contact list</short> 
        /// <category>Contacts</category>
        /// <returns>
        ///    Contact list
        /// </returns>
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
        ///   Get the group of contacts with the IDs specified in the request
        /// </summary>
        /// <param name="contactids">Contact ID list</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <short>Get contact group</short> 
        /// <category>Contacts</category>
        /// <returns>
        ///   Contact list
        /// </returns>
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
        ///   Deletes the list of all contacts in the CRM module matching the parameters specified in the request
        /// </summary>
        /// <param optional="true" name="tags">Tag</param>
        /// <param optional="true" name="contactStage">Contact stage ID (warmth)</param>
        /// <param optional="true" name="contactType">Contact type ID</param>
        /// <param optional="true" name="contactListView" remark="Allowed values: Company, Person, WithOpportunity"></param>
        /// <param optional="true" name="fromDate">Start date</param>
        /// <param optional="true" name="toDate">End date</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <short>Delete the list of all contacts </short> 
        /// <category>Contacts</category>
        /// <returns>
        ///   Contact list
        /// </returns>
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
        ///    Returns the list of all the persons linked to the company with the ID specified in the request
        /// </summary>
        /// <param name="companyid">Company ID</param>
        /// <exception cref="ArgumentException"></exception>
        /// <short>Get company linked persons list</short> 
        /// <category>Contacts</category>
        /// <returns>
        ///   Linked persons
        /// </returns>
        [Read(@"contact/company/{companyid:[0-9]+}/person")]
        public IEnumerable<ContactWrapper> GetPeopleFromCompany(int companyid)
        {
            if (companyid <= 0) throw new ArgumentException();

            var company = DaoFactory.ContactDao.GetByID(companyid);
            if (company == null || !CRMSecurity.CanAccessTo(company)) throw new ItemNotFoundException();

            return ToListContactWrapper(DaoFactory.ContactDao.GetMembers(companyid).Where(CRMSecurity.CanAccessTo).ToList());
        }

        /// <summary>
        ///   Adds the selected person to the company with the ID specified in the request
        /// </summary>
        /// <param optional="true"  name="companyid">Company ID</param>
        /// <param optional="true" name="personid">Person ID</param>
        /// <short>Add person to company</short> 
        /// <category>Contacts</category>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <returns>
        ///    Person
        /// </returns>
        [Create(@"contact/company/{companyid:[0-9]+}/person")]
        public PersonWrapper AddPeopleToCompany(int companyid, int personid)
        {
            if ((companyid <= 0) || (personid <= 0)) throw new ArgumentException();

            var company = DaoFactory.ContactDao.GetByID(companyid);
            var person = DaoFactory.ContactDao.GetByID(personid);

            if (person == null || company == null || !CRMSecurity.CanAccessTo(person) || !CRMSecurity.CanAccessTo(company)) throw new ItemNotFoundException();

            DaoFactory.ContactDao.AddMember(personid, companyid);
            MessageService.Send(Request, MessageAction.CompanyLinkedPerson, MessageTarget.Create(new[] { company.ID, person.ID }), company.GetTitle(), person.GetTitle());

            return (PersonWrapper)ToContactWrapper(person);
        }

        /// <summary>
        ///   Deletes the selected person from the company with the ID specified in the request
        /// </summary>
        /// <param optional="true"  name="companyid">Company ID</param>
        /// <param optional="true" name="personid">Person ID</param>
        /// <short>Delete person from company</short> 
        /// <category>Contacts</category>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <returns>
        ///    Person
        /// </returns>
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
        ///    Creates the person with the parameters (first name, last name, description, etc.) specified in the request
        /// </summary>
        /// <param name="firstName">First name</param>
        /// <param name="lastName">Last name</param>
        /// <param optional="true"  name="jobTitle">Post</param>
        /// <param optional="true" name="companyId">Company ID</param>
        /// <param optional="true" name="about">Person description text</param>
        /// <param name="shareType">Person privacy: 0 - not shared, 1 - shared for read/write, 2 - shared for read only</param>
        /// <param optional="true" name="managerList">List of managers for the person</param>
        /// <param optional="true" name="customFieldList">User field list</param>
        /// <param optional="true" name="photo">Contact photo (upload using multipart/form-data)</param>
        /// <short>Create person</short> 
        /// <category>Contacts</category>
        /// <returns>Person</returns>
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
            if (companyId > 0) {
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
        ///    Changes the photo for the contact with the ID specified in the request
        /// </summary>
        /// <param name="contactid">Contact ID</param>
        /// <param name="photo">Contact photo (upload using multipart/form-data)</param>
        /// <short> Change contact photo</short> 
        /// <category>Contacts</category>
        /// <exception cref="ArgumentException"></exception>
        /// <returns>
        ///    Path to contact photo
        /// </returns>
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
        ///    Changes the photo for the contact with the ID specified in the request
        /// </summary>
        /// <param name="contactid">Contact ID</param>
        /// <param name="photourl">contact photo url</param>
        /// <short> Change contact photo</short> 
        /// <category>Contacts</category>
        /// <exception cref="ArgumentException"></exception>
        /// <returns>
        ///    Path to contact photo
        /// </returns>
        [Update(@"contact/{contactid:[0-9]+}/changephotobyurl")]
        public string ChangeContactPhoto(int contactid, string photourl)
        {
            if (contactid <= 0 || string.IsNullOrEmpty(photourl)) throw new ArgumentException();

            var contact = DaoFactory.ContactDao.GetByID(contactid);
            if (contact == null || !CRMSecurity.CanAccessTo(contact)) throw new ItemNotFoundException();

            return ContactPhotoManager.UploadPhoto(photourl, contactid, false).Url;
        }

        /// <summary>
        ///    Merge two selected contacts
        /// </summary>
        /// <param name="fromcontactid">the first contact ID for merge</param>
        /// <param name="tocontactid">the second contact ID for merge</param>
        /// <short>Merge contacts</short> 
        /// <category>Contacts</category>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <exception cref="SecurityException"></exception>
        /// <returns>
        ///    Contact
        /// </returns>
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
        ///    Updates the selected person with the parameters (first name, last name, description, etc.) specified in the request
        /// </summary>
        /// <param name="personid">Person ID</param>
        /// <param name="firstName">First name</param>
        /// <param name="lastName">Last name</param>
        /// <param optional="true"  name="jobTitle">Post</param>
        /// <param optional="true" name="companyId">Company ID</param>
        /// <param optional="true" name="about">Person description text</param>
        /// <param name="shareType">Person privacy: 0 - not shared, 1 - shared for read/write, 2 - shared for read only</param>
        /// <param optional="true" name="managerList">List of persons managers</param>
        /// <param optional="true" name="customFieldList">User field list</param>
        /// <param optional="true" name="photo">Contact photo (upload using multipart/form-data)</param>
        /// <short>Update person</short> 
        /// <category>Contacts</category>
        /// <returns>Person</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
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
        ///    Creates the company with the parameters specified in the request
        /// </summary>
        /// <param  name="companyName">Company name</param>
        /// <param optional="true" name="about">Company description text</param>
        /// <param optional="true" name="personList">Linked person list</param>
        /// <param name="shareType">Company privacy: 0 - not shared, 1 - shared for read/write, 2 - shared for read only</param>
        /// <param optional="true" name="managerList">List of managers for the company</param>
        /// <param optional="true" name="customFieldList">User field list</param>
        /// <param optional="true" name="photo">Contact photo (upload using multipart/form-data)</param>
        /// <short>Create company</short> 
        /// <category>Contacts</category>
        /// <returns>Company</returns>
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
                    var person = DaoFactory.ContactDao.GetByID(personID);
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
        ///    Quickly creates the list of companies
        /// </summary>
        /// <short>
        ///    Quick company list creation
        /// </short>
        /// <param name="companyName">Company name</param>
        /// <category>Contacts</category>
        /// <returns>Contact list</returns>
        /// <exception cref="ArgumentException"></exception>
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

            var selectedManagers = new List<Guid> {Core.SecurityContext.CurrentAccount.ID};

            foreach (var ct in contacts)
            {
                CRMSecurity.SetAccessTo(ct, selectedManagers);
            }

            return contacts.ConvertAll(ToContactBaseWrapper);
        }

        /// <summary>
        ///    Quickly creates the list of persons with the first and last names specified in the request
        /// </summary>
        /// <short>
        ///    Quick person list creation
        /// </short>
        /// <param name="data">Pairs: user first name, user last name</param>
        /// <remarks>
        /// <![CDATA[
        ///  data has format
        ///  [{key: 'First name 1', value: 'Last name 1'},{key: 'First name 2', value: 'Last name 2'}]
        /// ]]>
        /// </remarks>
        /// <category>Contacts</category>
        /// <returns>Contact list</returns>
        /// <exception cref="ArgumentException"></exception>
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

            var selectedManagers = new List<Guid> {Core.SecurityContext.CurrentAccount.ID};

            foreach (var ct in contacts)
            {
                CRMSecurity.SetAccessTo(ct, selectedManagers);
            }

            MessageService.Send(Request, MessageAction.PersonsCreated, MessageTarget.Create(contacts.Select(x => x.ID)), contacts.Select(x => x.GetTitle()));

            return contacts.ConvertAll(ToContactBaseWrapper);
        }

        /// <summary>
        ///    Updates the selected company with the parameters specified in the request
        /// </summary>
        /// <param name="companyid">Company ID</param>
        /// <param  name="companyName">Company name</param>
        /// <param optional="true" name="about">Company description text</param>
        /// <param name="shareType">Company privacy: 0 - not shared, 1 - shared for read/write, 2 - shared for read only</param>
        /// <param optional="true" name="managerList">List of company managers</param>
        /// <param optional="true" name="customFieldList">User field list</param>
        /// <short>Update company</short> 
        /// <category>Contacts</category>
        /// <exception cref="ArgumentException"></exception>
        /// <returns>
        ///   Company
        /// </returns>
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

            var managerListLocal = managerList != null ? managerList.ToList(): new List<Guid>();
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
        ///    Updates the selected contact status
        /// </summary>
        /// <param name="contactid">Contact ID</param>
        /// <param  name="contactStatusid">Contact status ID</param>
        /// <short>Update status in contact by id</short> 
        /// <category>Contacts</category>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <returns>
        ///   Company
        /// </returns>
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

            dao.UpdateContactStatus(new List<int>{companyInst.ID}, contactStatusid);
            companyInst.StatusID = contactStatusid;

            var messageAction = companyInst is Company ? MessageAction.CompanyUpdatedTemperatureLevel : MessageAction.PersonUpdatedTemperatureLevel;
            MessageService.Send(Request, messageAction, MessageTarget.Create(companyInst.ID), companyInst.GetTitle());

            return ToContactWrapper(companyInst);
        }

        /// <summary>
        ///    Updates status of the selected company and all its participants
        /// </summary>
        /// <param name="companyid">Company ID</param>
        /// <param  name="contactStatusid">Contact status ID</param>
        /// <short>Update company and participants status</short> 
        /// <category>Contacts</category>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <returns>
        ///   Company
        /// </returns>
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
        ///    Updates status of the selected person, related company and all its participants
        /// </summary>
        /// <param name="personid">Person ID</param>
        /// <param  name="contactStatusid">Contact status ID</param>
        /// <short>Update person, related company and participants status</short> 
        /// <category>Contacts</category>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <returns>
        ///   Person
        /// </returns>
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
        ///   Get access rights to the contact with the ID specified in the request
        /// </summary>
        /// <short>Get contact access rights</short> 
        /// <category>Contacts</category>
        ///<exception cref="ArgumentException"></exception>
        ///<exception cref="ItemNotFoundException"></exception>
        ///<exception cref="SecurityException"></exception>
        /// <returns>User list</returns>
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
        ///   Sets access rights for other users to the contact with the ID specified in the request
        /// </summary>
        /// <param name="contactid">Contact ID</param>
        /// <param name="isShared">Contact privacy: private or not</param>
        /// <param name="managerList">List of managers</param>
        /// <short>Set contact access rights</short> 
        /// <category>Contacts</category>
        ///<exception cref="ArgumentException"></exception>
        ///<exception cref="SecurityException"></exception>
        ///<exception cref="ItemNotFoundException"></exception>
        /// <returns>
        ///   Contact
        /// </returns>
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
        ///   Sets access rights for other users to the list of contacts with the IDs specified in the request
        /// </summary>
        /// <param name="contactid">Contact ID list</param>
        /// <param name="isShared">Company privacy: shared or not</param>
        /// <param name="managerList">List of managers</param>
        /// <short>Set contact access rights</short> 
        /// <category>Contacts</category>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <returns>
        ///   Contact list
        /// </returns>
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
        ///   Sets access rights for the selected user to the list of contacts with the parameters specified in the request
        /// </summary>
        /// <param name="isPrivate">Contact privacy: private or not</param>
        /// <param name="managerList">List of managers</param>
        /// <param optional="true" name="tags">Tag</param>
        /// <param optional="true" name="contactStage">Contact stage ID (warmth)</param>
        /// <param optional="true" name="contactType">Contact type ID</param>
        /// <param optional="true" name="contactListView" remark="Allowed values: Company, Person, WithOpportunity"></param>
        /// <param optional="true" name="fromDate">Start date</param>
        /// <param optional="true" name="toDate">End date</param>
        /// <short>Set contact access rights</short> 
        /// <category>Contacts</category>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <returns>
        ///   Contact list
        /// </returns>
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
        ///     Deletes the contact with the ID specified in the request from the portal
        /// </summary>
        /// <short>Delete contact</short> 
        /// <category>Contacts</category>
        /// <param name="contactid">Contact ID</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <returns>
        ///   Contact
        /// </returns>
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
        ///   Deletes the group of contacts with the IDs specified in the request
        /// </summary>
        /// <param name="contactids">Contact ID list</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <short>Delete contact group</short> 
        /// <category>Contacts</category>
        /// <returns>
        ///   Contact list
        /// </returns>
        [Update(@"contact")]
        public IEnumerable<ContactBaseWrapper> DeleteBatchContacts(IEnumerable<int> contactids)
        {
            if (contactids == null) throw new ArgumentException();

            var contacts = DaoFactory.ContactDao.DeleteBatchContact(contactids.ToArray());
            MessageService.Send(Request, MessageAction.ContactsDeleted, MessageTarget.Create(contactids), contacts.Select(c => c.GetTitle()));

            return contacts.Select(ToContactBaseWrapper);
        }

        /// <summary>
        ///    Returns the list of 30 contacts in the CRM module with prefix
        /// </summary>
        /// <param optional="true" name="prefix"></param>
        /// <param optional="false" name="searchType" remark="Allowed values: -1 (Any), 0 (Company), 1 (Persons), 2 (PersonsWithoutCompany), 3 (CompaniesAndPersonsWithoutCompany)">searchType</param>
        /// <param optional="true" name="entityType"></param>
        /// <param optional="true" name="entityID"></param>
        /// <category>Contacts</category>
        /// <returns>
        ///    Contact list
        /// </returns>
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
        ///    Returns the list contacts in the CRM module with contact information
        /// </summary>
        /// <param optional="false" name="infoType">Contact information type</param>
        /// <param optional="false" name="data">Data</param>
        /// <param optional="true" name="category">Category</param>
        /// <param optional="true" name="isPrimary">Contact importance: primary or not</param>
        /// <category>Contacts</category>
        /// <returns>
        ///    Contact list
        /// </returns>
        [Read(@"contact/bycontactinfo")]
        public IEnumerable<ContactWrapper> GetContactsByContactInfo(ContactInfoType? infoType, String data, int? category, bool? isPrimary)
        {
            if (!infoType.HasValue) throw new ArgumentException();

            var ids = DaoFactory.ContactDao.GetContactIDsByContactInfo(infoType.Value, data, category, isPrimary);

            var result = DaoFactory.ContactDao.GetContacts(ids.ToArray()).ConvertAll(ToContactWrapper);

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contactid"></param>
        /// <param name="count"></param>
        /// <category>Contacts</category>
        /// <returns></returns>
        [Read(@"contact/{contactid:[0-9]+}/tweets")]
        public List<Message> GetUserTweets(int contactid, int count)
        {
            var MessageCount = 10;
            var twitterAccounts = DaoFactory.ContactInfoDao.GetList(contactid, ContactInfoType.Twitter, null, null);

            if (twitterAccounts.Count == 0)
                throw new ResourceNotFoundException(
                    Newtonsoft.Json.JsonConvert.SerializeObject(
                                        new
                                        {
                                            message = "",
                                            description = CRMSocialMediaResource.SocialMediaAccountNotFoundTwitter
                                        }
                    ));

            var apiInfo = TwitterApiHelper.GetTwitterApiInfoForCurrentUser();
            TwitterDataProvider twitterProvider = new TwitterDataProvider(apiInfo);

            List<Message> messages = new List<Message>();

            foreach (var twitterAccount in twitterAccounts)
            {
                try
                {
                    messages.AddRange(twitterProvider.GetUserTweets(twitterAccount.ID, twitterAccount.Data, MessageCount));
                }
                catch (ResourceNotFoundException ex)
                {
                    throw new ResourceNotFoundException(
                        Newtonsoft.Json.JsonConvert.SerializeObject(
                                            new
                                            {
                                                message = ex.Message,
                                                description = String.Format("{0}: {1}", CRMSocialMediaResource.ErrorUnknownTwitterAccount, twitterAccount.Data)
                                            }
                        ));
                }
                catch (Exception ex)
                {
                    throw new Exception(
                        Newtonsoft.Json.JsonConvert.SerializeObject(
                                            new
                                            {
                                                message = ex.Message,
                                                description = String.Format("{0}: {1}", CRMSocialMediaResource.ErrorUnknownTwitterAccount, twitterAccount.Data)
                                            }
                        ));
                }
            }


            return messages.OrderByDescending(m => m.PostedOn).Take(MessageCount).ToList();

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="searchText"></param>
        /// <category>Contacts</category>
        /// <returns></returns>
        [Read(@"contact/twitterprofile")]
        public List<TwitterUserInfo> FindTwitterProfiles(string searchText)
        {
            try
            {
                TwitterApiInfo apiInfo = TwitterApiHelper.GetTwitterApiInfoForCurrentUser();
                if (apiInfo == null)
                    throw new SocialMediaAccountNotFound(CRMSocialMediaResource.SocialMediaAccountNotFoundTwitter);

                TwitterDataProvider provider = new TwitterDataProvider(apiInfo);
                List<TwitterUserInfo> users = provider.FindUsers(searchText);
                /*List<TwitterUserInfo> users = new List<TwitterUserInfo>();
                users.Add(new TwitterUserInfo { Description = "I'm a cool user", SmallImageUrl = "http://localhost/TeamLab/products/crm/data/0/photos/00/00/10/contact_10_50_50.jpg", UserName = "User", ScreenName = "user", UserID = 1 });
                users.Add(new TwitterUserInfo { Description = "I'm a cool user", SmallImageUrl = "http://localhost/TeamLab/products/crm/data/0/photos/00/00/10/contact_10_50_50.jpg", UserName = "User", ScreenName = "user", UserID = 1 });
                users.Add(new TwitterUserInfo { Description = "I'm a cool user", SmallImageUrl = "http://localhost/TeamLab/products/crm/data/0/photos/00/00/10/contact_10_50_50.jpg", UserName = "User", ScreenName = "user", UserID = 1 });*/
                return users;
            }
            catch (Exception ex) {
                throw new SocialMediaUI(DaoFactory).ProcessError(ex, "ASC.Api.CRM.CRMApi.FindTwitterProfiles");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contactId"></param>
        /// <param name="contactType"></param>
        /// <param name="uploadOnly"></param>
        /// <category>Contacts</category>
        /// <returns></returns>
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
        /// 
        /// </summary>
        /// <param name="contactId"></param>
        /// <category>Contacts</category>
        /// <returns></returns>
        [Read(@"contact/{contactid:[0-9]+}/socialmediaavatar")]
        public List<SocialMediaImageDescription> GetContactSMImages(int contactId)
        {
            return new SocialMediaUI(DaoFactory).GetContactSMImages(contactId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="socialNetworks"></param>
        /// <category>Contacts</category>
        /// <returns></returns>
        [Create(@"contact/socialmediaavatar")]
        public List<SocialMediaImageDescription> GetContactSMImagesByNetworks(List<ContactInfoWrapper> socialNetworks)
        {
            if (socialNetworks == null || socialNetworks.Count == 0){
                return new List<SocialMediaImageDescription>();
            }
            var twitter = new List<String>();

            foreach (var sn in socialNetworks) {
                if (sn.InfoType == ContactInfoType.Twitter) twitter.Add(sn.Data);
            }

            return new SocialMediaUI(DaoFactory).GetContactSMImages(twitter);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contactId"></param>
        /// <param name="socialNetwork"></param>
        /// <param name="userIdentity"></param>
        /// <param name="uploadOnly"></param>
        /// <param name="tmpDirName" visible="false"></param>
        /// <category>Contacts</category>
        /// <returns></returns>
        [Update(@"contact/{contactid:[0-9]+}/avatar")]
        public ContactPhotoManager.PhotoData UploadUserAvatarFromSocialNetwork(int contactId, SocialNetworks socialNetwork, string userIdentity, bool uploadOnly, string tmpDirName)
        {
            if (socialNetwork != SocialNetworks.Twitter)
                throw new ArgumentException();

            if (contactId != 0)
            {
                var contact = DaoFactory.ContactDao.GetByID(contactId);
                if (contact == null || !CRMSecurity.CanAccessTo(contact)) throw new ItemNotFoundException();

                if (!CRMSecurity.CanEdit(contact)) throw CRMSecurity.CreateSecurityException();
            }

            if (socialNetwork == SocialNetworks.Twitter)
            {
                var provider = new TwitterDataProvider(TwitterApiHelper.GetTwitterApiInfoForCurrentUser());
                var imageUrl = provider.GetUrlOfUserImage(userIdentity, TwitterDataProvider.ImageSize.Original);
                return UploadAvatar(contactId, imageUrl, uploadOnly, tmpDirName, false);
            }

            return null;
        }

        /// <visible>false</visible>
        [Create(@"contact/mailsmtp/send")]
        public IProgressItem SendMailSMTPToContacts(List<int> fileIDs, List<int> contactIds, String subject, String body, bool storeInHistory)
        {
            if (contactIds == null || contactIds.Count == 0 || String.IsNullOrEmpty(body)) throw new ArgumentException();

            var contacts = DaoFactory.ContactDao.GetContacts(contactIds.ToArray());
            MessageService.Send(Request, MessageAction.CrmSmtpMailSent, MessageTarget.Create(contactIds), contacts.Select(c => c.GetTitle()));

            return MailSender.Start(fileIDs, contactIds, subject, body, storeInHistory);
        }

        /// <visible>false</visible>
        [Create(@"contact/mailsmtp/preview")]
        public string GetMailSMTPToContactsPreview(string template, int contactId)
        {
            if (contactId == 0 || String.IsNullOrEmpty(template)) throw new ArgumentException();

            var manager = new MailTemplateManager(DaoFactory);

            return manager.Apply(template, contactId);
        }

        /// <visible>false</visible>
        [Read(@"contact/mailsmtp/status")]
        public IProgressItem GetMailSMTPToContactsStatus()
        {
            return MailSender.GetStatus();
        }

        /// <visible>false</visible>
        [Update(@"contact/mailsmtp/cancel")]
        public IProgressItem CancelMailSMTPToContacts()
        {
            var progressItem = MailSender.GetStatus();
            MailSender.Cancel();
            return progressItem;
        }

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
        

        private static ContactPhotoManager.PhotoData UploadAvatar(int contactID, string imageUrl, bool uploadOnly, string tmpDirName, bool checkFormat = true)
        {
            if (contactID != 0)
            {
                return ContactPhotoManager.UploadPhoto(imageUrl, contactID, uploadOnly, checkFormat);
            }

            if (string.IsNullOrEmpty(tmpDirName) || tmpDirName == "null") tmpDirName = null;
            return ContactPhotoManager.UploadPhotoToTemp(imageUrl, tmpDirName, checkFormat);
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
                                contactInfos.Add(item.ContactID, new List<ContactInfoWrapper> {new ContactInfoWrapper(item)});
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

                foreach (var contactBaseWrapperQuick in tmpList) {
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

            var personsCustomFields = DaoFactory.CustomFieldDao.GetEnityFields(EntityType.Person, personsIDs.ToArray());
            var companyCustomFields = DaoFactory.CustomFieldDao.GetEnityFields(EntityType.Company, companyIDs.ToArray());

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
                                contactInfos.Add(item.ContactID, new List<ContactInfoWrapper> {new ContactInfoWrapper(item)});
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
                                                .GetEnityFields(EntityType.Person, contact.ID, false)
                                                .ConvertAll(item => new CustomFieldBaseWrapper(item)).ToSmartList();
            }
            else
            {
                result.CustomFields = DaoFactory.CustomFieldDao
                                                .GetEnityFields(EntityType.Company, contact.ID, false)
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