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
using ASC.Api.Attributes;
using ASC.Api.CRM.Wrappers;
using ASC.Api.Collections;
using ASC.Api.Exceptions;
using ASC.CRM.Core;
using ASC.CRM.Core.Entities;
using ASC.Core;
using ASC.MessagingSystem;
using EnumExtension = ASC.Web.CRM.Classes.EnumExtension;
using ASC.Core.Users;
using ASC.ElasticSearch;

namespace ASC.Api.CRM
{
    public partial class CRMApi
    {
        /// <summary>
        ///   Close the case with the ID specified in the request
        /// </summary>
        /// <short>Close case</short> 
        /// <category>Cases</category>
        /// <param name="caseid" optional="false">Case ID</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <returns>
        ///   Case
        /// </returns>
        [Update(@"case/{caseid:[0-9]+}/close")]
        public CasesWrapper CloseCases(int caseid)
        {
            if (caseid <= 0) throw new ArgumentException();

            var cases = DaoFactory.CasesDao.CloseCases(caseid);
            if (cases == null) throw new ItemNotFoundException();

            MessageService.Send(Request, MessageAction.CaseClosed, MessageTarget.Create(cases.ID), cases.Title);

            return ToCasesWrapper(cases);
        }

        /// <summary>
        ///   Resume the case with the ID specified in the request
        /// </summary>
        /// <short>Resume case</short> 
        /// <category>Cases</category>
        /// <param name="caseid" optional="false">Case ID</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <returns>
        ///   Case
        /// </returns>
        [Update(@"case/{caseid:[0-9]+}/reopen")]
        public CasesWrapper ReOpenCases(int caseid)
        {
            if (caseid <= 0) throw new ArgumentException();

            var cases = DaoFactory.CasesDao.ReOpenCases(caseid);
            if (cases == null) throw new ItemNotFoundException();

            MessageService.Send(Request, MessageAction.CaseOpened, MessageTarget.Create(cases.ID), cases.Title);

            return ToCasesWrapper(cases);
        }

        /// <summary>
        ///    Creates the case with the parameters specified in the request
        /// </summary>
        /// <short>Create case</short> 
        /// <param name="title" optional="false">Case title</param>
        /// <param name="members" optional="true">Participants</param>
        /// <param name="customFieldList" optional="true">User field list</param>
        /// <param name="isPrivate" optional="true">Case privacy: private or not</param>
        /// <param name="accessList" optional="true">List of users with access to the case</param>
        /// <param name="isNotify" optional="true">Notify users in accessList about the case</param>
        /// <returns>Case</returns>
        /// <category>Cases</category>
        /// <exception cref="ArgumentException"></exception>
        /// <example>
        /// <![CDATA[
        /// 
        /// Data transfer in application/json format:
        /// 
        /// data: {
        ///    title: "Exhibition organization",
        ///    isPrivate: false,
        ///    customFieldList: [{1: "value for text custom field with id = 1"}]
        /// }
        /// 
        /// ]]>
        /// </example>
        [Create(@"case")]
        public CasesWrapper CreateCases(
            string title,
            IEnumerable<int> members,
            IEnumerable<ItemKeyValuePair<int, string>> customFieldList,
            bool isPrivate,
            IEnumerable<Guid> accessList,
            bool isNotify)
        {
            if (string.IsNullOrEmpty(title)) throw new ArgumentException();

            var casesID = DaoFactory.CasesDao.CreateCases(title);

            var cases = new Cases
                {
                    ID = casesID,
                    Title = title,
                    CreateBy = SecurityContext.CurrentAccount.ID,
                    CreateOn = DateTime.UtcNow
                };
            FactoryIndexer<Web.CRM.Core.Search.CasesWrapper>.IndexAsync(cases);
            SetAccessToCases(cases, isPrivate, accessList, isNotify, false);

            var membersList = members != null ? members.ToList() : new List<int>();
            if (membersList.Any())
            {
                var contacts = DaoFactory.ContactDao.GetContacts(membersList.ToArray()).Where(CRMSecurity.CanAccessTo).ToList();
                membersList = contacts.Select(m => m.ID).ToList();
                DaoFactory.CasesDao.SetMembers(cases.ID, membersList.ToArray());
            }

            if (customFieldList != null)
            {
                var existingCustomFieldList = DaoFactory.CustomFieldDao.GetFieldsDescription(EntityType.Case).Select(fd => fd.ID).ToList();
                foreach (var field in customFieldList)
                {
                    if (string.IsNullOrEmpty(field.Value) || !existingCustomFieldList.Contains(field.Key)) continue;
                    DaoFactory.CustomFieldDao.SetFieldValue(EntityType.Case, cases.ID, field.Key, field.Value);
                }
            }

            return ToCasesWrapper(DaoFactory.CasesDao.GetByID(casesID));
        }

        /// <summary>
        ///   Updates the selected case with the parameters specified in the request
        /// </summary>
        /// <short>Update case</short> 
        /// <param name="caseid" optional="false">Case ID</param>
        /// <param name="title" optional="false">Case title</param>
        /// <param name="members" optional="true">Participants</param>
        /// <param name="customFieldList" optional="true">User field list</param>
        /// <param name="isPrivate" optional="true">Case privacy: private or not</param>
        /// <param name="accessList" optional="true">List of users with access to the case</param>
        /// <param name="isNotify" optional="true">Notify users in accessList about the case</param>
        /// <category>Cases</category>
        /// <returns>Case</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <example>
        /// <![CDATA[
        /// 
        /// Data transfer in application/json format:
        /// 
        /// data: {
        ///    caseid: 0,
        ///    title: "Exhibition organization",
        ///    isPrivate: false,
        ///    customFieldList: [{1: "value for text custom field with id = 1"}]
        /// }
        /// 
        /// ]]>
        /// </example>
        [Update(@"case/{caseid:[0-9]+}")]
        public CasesWrapper UpdateCases(
            int caseid,
            string title,
            IEnumerable<int> members,
            IEnumerable<ItemKeyValuePair<int, string>> customFieldList,
            bool isPrivate,
            IEnumerable<Guid> accessList,
            bool isNotify)
        {
            if ((caseid <= 0) || (string.IsNullOrEmpty(title))) throw new ArgumentException();

            var cases = DaoFactory.CasesDao.GetByID(caseid);
            if (cases == null) throw new ItemNotFoundException();

            cases.Title = title;

            DaoFactory.CasesDao.UpdateCases(cases);

            if (CRMSecurity.IsAdmin || cases.CreateBy == Core.SecurityContext.CurrentAccount.ID)
            {
                SetAccessToCases(cases, isPrivate, accessList, isNotify, false);
            }

            var membersList = members != null ? members.ToList() : new List<int>();
            if (membersList.Any())
            {
                var contacts = DaoFactory.ContactDao.GetContacts(membersList.ToArray()).Where(CRMSecurity.CanAccessTo).ToList();
                membersList = contacts.Select(m => m.ID).ToList();
                DaoFactory.CasesDao.SetMembers(cases.ID, membersList.ToArray());
            }

            if (customFieldList != null)
            {
                var existingCustomFieldList = DaoFactory.CustomFieldDao.GetFieldsDescription(EntityType.Case).Select(fd => fd.ID).ToList();
                foreach (var field in customFieldList)
                {
                    if (string.IsNullOrEmpty(field.Value) || !existingCustomFieldList.Contains(field.Key)) continue;
                    DaoFactory.CustomFieldDao.SetFieldValue(EntityType.Case, cases.ID, field.Key, field.Value);
                }
            }

            return ToCasesWrapper(cases);
        }

        /// <summary>
        ///   Sets access rights for the selected case with the parameters specified in the request
        /// </summary>
        /// <param name="caseid" optional="false">Case ID</param>
        /// <param name="isPrivate" optional="false">Case privacy: private or not</param>
        /// <param name="accessList" optional="false">List of users with access to the case</param>
        /// <short>Set rights to case</short> 
        /// <category>Cases</category>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <returns>
        ///   Case 
        /// </returns>
        [Update(@"case/{caseid:[0-9]+}/access")]
        public CasesWrapper SetAccessToCases(int caseid, bool isPrivate, IEnumerable<Guid> accessList)
        {
            if (caseid <= 0) throw new ArgumentException();

            var cases = DaoFactory.CasesDao.GetByID(caseid);
            if (cases == null) throw new ItemNotFoundException();

            if (!(CRMSecurity.IsAdmin || cases.CreateBy == Core.SecurityContext.CurrentAccount.ID)) throw CRMSecurity.CreateSecurityException();

            return SetAccessToCases(cases, isPrivate, accessList, false, true);
        }

        private CasesWrapper SetAccessToCases(Cases cases, bool isPrivate, IEnumerable<Guid> accessList, bool isNotify, bool isMessageServicSende)
        {
            var accessListLocal = accessList != null ? accessList.Distinct().ToList() : new List<Guid>();
            if (isPrivate && accessListLocal.Any())
            {
                if (isNotify)
                {
                    accessListLocal = accessListLocal.Where(u => u != SecurityContext.CurrentAccount.ID).ToList();
                    ASC.Web.CRM.Services.NotifyService.NotifyClient.Instance.SendAboutSetAccess(EntityType.Case, cases.ID, DaoFactory, accessListLocal.ToArray());
                }

                if (!accessListLocal.Contains(SecurityContext.CurrentAccount.ID))
                {
                    accessListLocal.Add(SecurityContext.CurrentAccount.ID);
                }

                CRMSecurity.SetAccessTo(cases, accessListLocal);
                if (isMessageServicSende)
                {
                    var users = GetUsersByIdList(accessListLocal);
                    MessageService.Send(Request, MessageAction.CaseRestrictedAccess, MessageTarget.Create(cases.ID), cases.Title, users.Select(x => x.DisplayUserName(false)));
                }
            }
            else
            {
                CRMSecurity.MakePublic(cases);
                if (isMessageServicSende)
                {
                    MessageService.Send(Request, MessageAction.CaseOpenedAccess, MessageTarget.Create(cases.ID), cases.Title);
                }
            }

            return ToCasesWrapper(cases);
        }

        /// <summary>
        ///   Sets access rights for other users to the list of cases with the IDs specified in the request
        /// </summary>
        /// <param name="casesid">Case ID list</param>
        /// <param name="isPrivate">Case privacy: private or not</param>
        /// <param name="accessList">List of users with access</param>
        /// <short>Set case access rights</short> 
        /// <category>Cases</category>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <returns>
        ///   Case list
        /// </returns>
        [Update(@"case/access")]
        public IEnumerable<CasesWrapper> SetAccessToBatchCases(IEnumerable<int> casesid, bool isPrivate, IEnumerable<Guid> accessList)
        {
            var result = new List<Cases>();

            var cases = DaoFactory.CasesDao.GetCases(casesid);

            if (!cases.Any()) return new List<CasesWrapper>();

            foreach (var c in cases)
            {
                if (c == null) throw new ItemNotFoundException();

                if (!(CRMSecurity.IsAdmin || c.CreateBy == Core.SecurityContext.CurrentAccount.ID)) continue;

                SetAccessToCases(c, isPrivate, accessList, false, true);
                result.Add(c);
            }

            return ToListCasesWrappers(result);
        }

        /// <summary>
        ///   Sets access rights for other users to the list of all cases matching the parameters specified in the request
        /// </summary>
        /// <param optional="true" name="contactid">Contact ID</param>
        /// <param optional="true" name="isClosed">Case status</param>
        /// <param optional="true" name="tags">Tags</param>
        /// <param name="isPrivate">Case privacy: private or not</param>
        /// <param name="accessList">List of users with access</param>
        /// <short>Set case access rights</short> 
        /// <category>Cases</category>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <returns>
        ///   Case list
        /// </returns>
        [Update(@"case/filter/access")]
        public IEnumerable<CasesWrapper> SetAccessToBatchCases(
            int contactid,
            bool? isClosed,
            IEnumerable<string> tags,
            bool isPrivate,
            IEnumerable<Guid> accessList
            )
        {
            var result = new List<Cases>();

            var caseses = DaoFactory.CasesDao.GetCases(_context.FilterValue, contactid, isClosed, tags, 0, 0, null);

            if (!caseses.Any()) return new List<CasesWrapper>();

            foreach (var casese in caseses)
            {
                if (casese == null) throw new ItemNotFoundException();

                if (!(CRMSecurity.IsAdmin || casese.CreateBy == Core.SecurityContext.CurrentAccount.ID)) continue;

                SetAccessToCases(casese, isPrivate, accessList, false, true);
                result.Add(casese);
            }

            return ToListCasesWrappers(result);
        }

        /// <summary>
        ///    Returns the detailed information about the case with the ID specified in the request
        /// </summary>
        /// <short>Get case by ID</short> 
        /// <category>Cases</category>
        /// <param name="caseid">Case ID</param>
        ///<exception cref="ArgumentException"></exception>
        ///<exception cref="ItemNotFoundException"></exception>
        [Read(@"case/{caseid:[0-9]+}")]
        public CasesWrapper GetCaseByID(int caseid)
        {
            if (caseid <= 0) throw new ItemNotFoundException();

            var cases = DaoFactory.CasesDao.GetByID(caseid);
            if (cases == null || !CRMSecurity.CanAccessTo(cases)) throw new ItemNotFoundException();

            return ToCasesWrapper(cases);
        }

        /// <summary>
        ///     Returns the list of all cases matching the parameters specified in the request
        /// </summary>
        /// <short>Get case list</short> 
        /// <param optional="true" name="contactid">Contact ID</param>
        /// <param optional="true" name="isClosed">Case status</param>
        /// <param optional="true" name="tags">Tags</param>
        /// <category>Cases</category>
        /// <returns>
        ///    Case list
        /// </returns>
        [Read(@"case/filter")]
        public IEnumerable<CasesWrapper> GetCases(int contactid, bool? isClosed, IEnumerable<string> tags)
        {
            IEnumerable<CasesWrapper> result;
            SortedByType sortBy;
            OrderBy casesOrderBy;

            var searchString = _context.FilterValue;

            if (EnumExtension.TryParse(_context.SortBy, true, out sortBy))
            {
                casesOrderBy = new OrderBy(sortBy, !_context.SortDescending);
            }
            else if (string.IsNullOrEmpty(_context.SortBy))
            {
                casesOrderBy = new OrderBy(SortedByType.Title, true);
            }
            else
            {
                casesOrderBy = null;
            }

            var fromIndex = (int)_context.StartIndex;
            var count = (int)_context.Count;

            if (casesOrderBy != null)
            {
                result = ToListCasesWrappers(
                    DaoFactory
                        .CasesDao
                        .GetCases(
                            searchString,
                            contactid,
                            isClosed,
                            tags,
                            fromIndex,
                            count,
                            casesOrderBy)).ToList();

                _context.SetDataPaginated();
                _context.SetDataFiltered();
                _context.SetDataSorted();
            }
            else
            {
                result = ToListCasesWrappers(
                    DaoFactory
                        .CasesDao
                        .GetCases(
                            searchString, contactid, isClosed,
                            tags,
                            0,
                            0,
                            null)).ToList();
            }

            int totalCount;

            if (result.Count() < count)
            {
                totalCount = fromIndex + result.Count();
            }
            else
            {
                totalCount = DaoFactory.CasesDao.GetCasesCount(searchString, contactid, isClosed, tags);
            }

            _context.SetTotalCount(totalCount);
            return result.ToSmartList();
        }

        /// <summary>
        ///   Deletes the case with the ID specified in the request
        /// </summary>
        /// <short>Delete case</short> 
        /// <param name="caseid">Case ID</param>
        /// <category>Cases</category>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <returns>
        ///    Case
        /// </returns>
        [Delete(@"case/{caseid:[0-9]+}")]
        public CasesWrapper DeleteCase(int caseid)
        {
            if (caseid <= 0) throw new ArgumentException();

            var cases = DaoFactory.CasesDao.DeleteCases(caseid);
            if (cases == null) throw new ItemNotFoundException();

            FactoryIndexer<Web.CRM.Core.Search.CasesWrapper>.DeleteAsync(cases);

            MessageService.Send(Request, MessageAction.CaseDeleted, MessageTarget.Create(cases.ID), cases.Title);

            return ToCasesWrapper(cases);
        }

        /// <summary>
        ///   Deletes the group of cases with the IDs specified in the request
        /// </summary>
        /// <param name="casesids">Case ID list</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <short>Delete case group</short> 
        /// <category>Cases</category>
        /// <returns>
        ///   Case list
        /// </returns>
        [Update(@"case")]
        public IEnumerable<CasesWrapper> DeleteBatchCases(IEnumerable<int> casesids)
        {
            if (casesids == null) throw new ArgumentException();

            casesids = casesids.Distinct();
            var caseses = DaoFactory.CasesDao.DeleteBatchCases(casesids.ToArray());

            if (caseses == null || !caseses.Any()) return new List<CasesWrapper>();

            MessageService.Send(Request, MessageAction.CasesDeleted, MessageTarget.Create(casesids), caseses.Select(c => c.Title));

            return ToListCasesWrappers(caseses);
        }

        /// <summary>
        ///   Deletes the list of all cases matching the parameters specified in the request
        /// </summary>
        /// <param optional="true" name="contactid">Contact ID</param>
        /// <param optional="true" name="isClosed">Case status</param>
        /// <param optional="true" name="tags">Tags</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <short>Delete case group</short> 
        /// <category>Cases</category>
        /// <returns>
        ///   Case list
        /// </returns>
        [Delete(@"case/filter")]
        public IEnumerable<CasesWrapper> DeleteBatchCases(int contactid, bool? isClosed, IEnumerable<string> tags)
        {
            var caseses = DaoFactory.CasesDao.GetCases(_context.FilterValue, contactid, isClosed, tags, 0, 0, null);
            if (!caseses.Any()) return new List<CasesWrapper>();

            caseses = DaoFactory.CasesDao.DeleteBatchCases(caseses);

            MessageService.Send(Request, MessageAction.CasesDeleted, MessageTarget.Create(caseses.Select(c => c.ID)), caseses.Select(c => c.Title));

            return ToListCasesWrappers(caseses);
        }

        /// <summary>
        ///    Returns the list of all contacts associated with the case with the ID specified in the request
        /// </summary>
        /// <short>Get all case contacts</short> 
        /// <param name="caseid">Case ID</param>
        /// <category>Cases</category>
        /// <returns>Contact list</returns>
        ///<exception cref="ArgumentException"></exception>
        [Read(@"case/{caseid:[0-9]+}/contact")]
        public IEnumerable<ContactWrapper> GetCasesMembers(int caseid)
        {
            var contactIDs = DaoFactory.CasesDao.GetMembers(caseid);
            return contactIDs == null
                       ? new ItemList<ContactWrapper>()
                       : ToListContactWrapper(DaoFactory.ContactDao.GetContacts(contactIDs));
        }

        /// <summary>
        ///   Adds the selected contact to the case with the ID specified in the request
        /// </summary>
        /// <short>Add case contact</short> 
        /// <category>Cases</category>
        /// <param name="caseid">Case ID</param>
        /// <param name="contactid">Contact ID</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <returns>
        ///    Participant
        /// </returns>
        [Create(@"case/{caseid:[0-9]+}/contact")]
        public ContactWrapper AddMemberToCases(int caseid, int contactid)
        {
            if ((caseid <= 0) || (contactid <= 0)) throw new ArgumentException();

            var cases = DaoFactory.CasesDao.GetByID(caseid);
            if (cases == null || !CRMSecurity.CanAccessTo(cases)) throw new ItemNotFoundException();

            var contact = DaoFactory.ContactDao.GetByID(contactid);
            if (contact == null || !CRMSecurity.CanAccessTo(contact)) throw new ItemNotFoundException();

            DaoFactory.CasesDao.AddMember(caseid, contactid);

            var messageAction = contact is Company ? MessageAction.CaseLinkedCompany : MessageAction.CaseLinkedPerson;
            MessageService.Send(Request, messageAction, MessageTarget.Create(cases.ID), cases.Title, contact.GetTitle());

            return ToContactWrapper(contact);
        }

        /// <summary>
        ///   Delete the selected contact from the case with the ID specified in the request
        /// </summary>
        /// <short>Delete case contact</short> 
        /// <category>Cases</category>
        /// <param name="caseid">Case ID</param>
        /// <param name="contactid">Contact ID</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <returns>
        ///    Participant
        /// </returns>
        [Delete(@"case/{caseid:[0-9]+}/contact/{contactid:[0-9]+}")]
        public ContactWrapper DeleteMemberFromCases(int caseid, int contactid)
        {
            if ((caseid <= 0) || (contactid <= 0)) throw new ArgumentException();

            var cases = DaoFactory.CasesDao.GetByID(caseid);
            if (cases == null || !CRMSecurity.CanAccessTo(cases)) throw new ItemNotFoundException();

            var contact = DaoFactory.ContactDao.GetByID(contactid);
            if (contact == null || !CRMSecurity.CanAccessTo(contact)) throw new ItemNotFoundException();

            var result = ToContactWrapper(contact);

            DaoFactory.CasesDao.RemoveMember(caseid, contactid);

            var messageAction = contact is Company ? MessageAction.CaseUnlinkedCompany : MessageAction.CaseUnlinkedPerson;
            MessageService.Send(Request, messageAction, MessageTarget.Create(cases.ID), cases.Title, contact.GetTitle());

            return result;
        }

        /// <summary>
        ///    Returns the list of 30 cases in the CRM module with prefix
        /// </summary>
        /// <param optional="true" name="prefix"></param>
        /// <param optional="true" name="contactID"></param>
        /// <category>Cases</category>
        /// <returns>
        ///    Cases list
        /// </returns>
        /// <visible>false</visible>
        [Read(@"case/byprefix")]
        public IEnumerable<CasesWrapper> GetCasesByPrefix(string prefix, int contactID)
        {
            var result = new List<CasesWrapper>();

            if (contactID > 0)
            {
                var findedCases = DaoFactory.CasesDao.GetCases(string.Empty, contactID, null, null, 0, 0, null);

                foreach (var item in findedCases)
                {
                    if (item.Title.IndexOf(prefix, StringComparison.Ordinal) != -1)
                    {
                        result.Add(ToCasesWrapper(item));
                    }
                }

                _context.SetTotalCount(findedCases.Count);
            }
            else
            {
                const int maxItemCount = 30;
                var findedCases = DaoFactory.CasesDao.GetCasesByPrefix(prefix, 0, maxItemCount);

                foreach (var item in findedCases)
                {
                    result.Add(ToCasesWrapper(item));
                }
            }

            return result;
        }

        private IEnumerable<CasesWrapper> ToListCasesWrappers(ICollection<Cases> items)
        {
            if (items == null || items.Count == 0) return new List<CasesWrapper>();

            var result = new List<CasesWrapper>();

            var contactIDs = new List<int>();
            var casesIDs = items.Select(item => item.ID).ToArray();

            var customFields = DaoFactory.CustomFieldDao
                                         .GetEnityFields(EntityType.Case, casesIDs)
                                         .GroupBy(item => item.EntityID)
                                         .ToDictionary(item => item.Key, item => item.Select(ToCustomFieldBaseWrapper));

            var casesMembers = DaoFactory.CasesDao.GetMembers(casesIDs);

            foreach (var value in casesMembers.Values)
            {
                contactIDs.AddRange(value);
            }

            var contacts = DaoFactory
                .ContactDao
                .GetContacts(contactIDs.Distinct().ToArray())
                .ToDictionary(item => item.ID, ToContactBaseWrapper);

            foreach (var cases in items)
            {
                var casesWrapper = new CasesWrapper(cases)
                    {
                        CustomFields = customFields.ContainsKey(cases.ID)
                                           ? customFields[cases.ID]
                                           : new List<CustomFieldBaseWrapper>(),
                        Members = casesMembers.ContainsKey(cases.ID)
                                      ? casesMembers[cases.ID].Where(contacts.ContainsKey).Select(item => contacts[item])
                                      : new List<ContactBaseWrapper>()
                    };

                result.Add(casesWrapper);
            }

            return result;
        }

        private CasesWrapper ToCasesWrapper(Cases cases)
        {
            var casesWrapper = new CasesWrapper(cases)
                {
                    CustomFields = DaoFactory
                        .CustomFieldDao
                        .GetEnityFields(EntityType.Case, cases.ID, false)
                        .ConvertAll(item => new CustomFieldBaseWrapper(item))
                        .ToSmartList(),
                    Members = new List<ContactBaseWrapper>()
                };

            var memberIDs = DaoFactory.CasesDao.GetMembers(cases.ID);
            var membersList = DaoFactory.ContactDao.GetContacts(memberIDs);

            var membersWrapperList = new List<ContactBaseWrapper>();

            foreach (var member in membersList)
            {
                if (member == null) continue;
                membersWrapperList.Add(ToContactBaseWrapper(member));
            }

            casesWrapper.Members = membersWrapperList;
            return casesWrapper;
        }

        private IEnumerable<UserInfo> GetUsersByIdList(IEnumerable<Guid> ids)
        {
            return CoreContext.UserManager.GetUsers().Where(x => ids.Contains(x.ID));
        }
    }
}