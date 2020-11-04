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
using ASC.Api.Exceptions;
using ASC.CRM.Core;
using ASC.CRM.Core.Entities;
using ASC.Api.Collections;
using ASC.MessagingSystem;
using ASC.Specific;
using ASC.Api.CRM.Wrappers;
using System.Security;
using ASC.Web.CRM.Resources;

namespace ASC.Api.CRM
{
    public partial class CRMApi
    {
        /// <summary>
        ///  Returns the list of all tags associated with the entity with the ID and type specified in the request
        /// </summary>
        /// <param name="entityType" remark="Allowed values: contact,opportunity,case">Entity type</param>
        /// <param name="entityid">Entity ID</param>
        /// <short>Get entity tags</short> 
        /// <category>Tags</category>
        /// <returns>
        ///   Tag
        /// </returns>
        ///<exception cref="ArgumentException"></exception>
        [Read(@"{entityType:(contact|opportunity|case)}/tag/{entityid:[0-9]+}")]
        public IEnumerable<string> GetEntityTags(string entityType, int entityid)
        {
            if (string.IsNullOrEmpty(entityType) || entityid <= 0) throw new ArgumentException();

            var entityTypeObj = ToEntityType(entityType);

            switch (entityTypeObj) {
                case EntityType.Contact:
                case EntityType.Person:
                case EntityType.Company:
                    var contact = DaoFactory.ContactDao.GetByID(entityid);
                    if (contact == null || !CRMSecurity.CanAccessTo(contact))
                        throw new ItemNotFoundException();
                    break;
                case EntityType.Case:
                    var cases = DaoFactory.CasesDao.GetByID(entityid);
                    if (cases == null || !CRMSecurity.CanAccessTo(cases))
                        throw new ItemNotFoundException();
                    break;
                case EntityType.Opportunity:
                    var deal = DaoFactory.DealDao.GetByID(entityid);
                    if (deal == null || !CRMSecurity.CanAccessTo(deal))
                        throw new ItemNotFoundException();
                    break;
            }

            return DaoFactory.TagDao.GetEntityTags(entityTypeObj, entityid);
        }

        /// <summary>
        ///    Returns the list of all tags for the contact with the ID specified in the request
        /// </summary>
        /// <param name="contactid">Contact ID</param>
        /// <short>Get all contact tags</short> 
        /// <category>Tags</category>
        /// <returns>
        ///   List of contact tags
        /// </returns>
        ///<exception cref="ArgumentException"></exception>
        [Read(@"contact/{contactid:[0-9]+}/tag")]
        public IEnumerable<string> GetContactTags(int contactid)
        {
            if (contactid <= 0) throw new ArgumentException();
            var contact = DaoFactory.ContactDao.GetByID(contactid);
            if (contact == null || !CRMSecurity.CanAccessTo(contact))
                throw new ItemNotFoundException();
            return DaoFactory.TagDao.GetEntityTags(EntityType.Contact, contactid).ToItemList();
        }

        /// <summary>
        ///  Creates the tag for the selected entity with the tag name specified in the request
        /// </summary>
        /// <param name="entityType" remark="Allowed values: contact,opportunity,case">Entity type</param>
        /// <param name="tagName">Tag name</param>
        /// <short>Create tag</short> 
        /// <category>Tags</category>
        /// <returns>
        ///   Tag
        /// </returns>
        ///<exception cref="ArgumentException"></exception>
        [Create(@"{entityType:(contact|opportunity|case)}/tag")]
        public string CreateTag(string entityType, string tagName)
        {
            if (string.IsNullOrEmpty(tagName)) throw new ArgumentException();

            var entityTypeObj = ToEntityType(entityType);

            var messageAction = GetEntityTagCreatedAction(entityTypeObj);
            DaoFactory.TagDao.AddTag(entityTypeObj, tagName);

            MessageService.Send(Request, messageAction, tagName);

            return tagName;
        }

        /// <summary>
        ///  Returns the list of all tags associated with the entity type specified in the request
        /// </summary>
        /// <param name="entityType" remark="Allowed values: contact,opportunity,case">Entity type</param>
        /// <short>Get tags for entity type</short> 
        /// <category>Tags</category>
        /// <returns>
        ///   Tag
        /// </returns>
        ///<exception cref="ArgumentException"></exception>
        [Read(@"{entityType:(contact|opportunity|case)}/tag")]
        public IEnumerable<TagWrapper> GetAllTags(string entityType)
        {
            if (string.IsNullOrEmpty(entityType)) throw new ArgumentException();
            var entType = ToEntityType(entityType);

            var tagTitles = DaoFactory.TagDao.GetAllTags(entType).ToList();
            var relativeItemsCountArrayJSON = DaoFactory.TagDao.GetTagsLinkCount(entType).ToList();
            if (tagTitles.Count != relativeItemsCountArrayJSON.Count) throw new ArgumentException();

            var result = new List<TagWrapper>();
            for (var i = 0; i < tagTitles.Count; i++) {
                result.Add(new TagWrapper(tagTitles[i], relativeItemsCountArrayJSON[i]));
            }
            return result.OrderBy(x => x.Title.Trim(), StringComparer.OrdinalIgnoreCase).ToList();
        }

        /// <summary>
        ///    Adds a group of tags to the entity with the ID specified in the request
        /// </summary>
        /// <short>Add tag group to entity</short> 
        /// <category>Tags</category>
        /// <param name="entityType" remark="Allowed values: contact,opportunity,case">Tag type</param>
        /// <param name="entityid">Entity ID</param>
        /// <param name="tagName">Tag name</param>
        /// <exception cref="ArgumentException"></exception>
        /// <returns>
        ///    Tag
        /// </returns> 
        [Create(@"{entityType:(contact|opportunity|case)}/taglist")]
        public string AddTagToBatch(string entityType, IEnumerable<int> entityid, string tagName)
        {
            var ids = entityid.ToList();
            if (entityid == null || !ids.Any()) throw new ArgumentException();

            foreach (var entityID in ids)
            {
                AddTagTo(entityType, entityID, tagName);
            }
            return tagName;
        }

        /// <summary>
        ///    Adds the selected tag to the group of contacts with the parameters specified in the request
        /// </summary>
        /// <short>Add tag to contact group</short> 
        /// <category>Tags</category>
        /// <param optional="true" name="tags">Tag</param>
        /// <param optional="true" name="contactStage">Contact stage ID (warmth)</param>
        /// <param optional="true" name="contactType">Contact type ID</param>
        /// <param optional="true" name="contactListView" remark="Allowed values: Company, Person, WithOpportunity"></param>
        /// <param optional="true" name="fromDate">Start date</param>
        /// <param optional="true" name="toDate">End date</param>
        /// <param name="tagName">Tag name</param>
        /// <exception cref="ArgumentException"></exception>
        /// <returns>
        ///    Tag
        /// </returns> 
        [Create(@"contact/filter/taglist")]
        public string AddTagToBatchContacts(
            IEnumerable<string> tags,
            int contactStage,
            int contactType,
            ContactListViewType contactListView,
            ApiDateTime fromDate,
            ApiDateTime toDate,
            string tagName)
        {
            var contacts = DaoFactory
                .ContactDao
                .GetContacts(_context.FilterValue,
                             tags,
                             contactStage,
                             contactType,
                             contactListView,
                             fromDate,
                             toDate,
                             0,
                             0,
                             null).Where(CRMSecurity.CanEdit).ToList();

            foreach (var contact in contacts)
            {
                AddTagTo("contact", contact.ID, tagName);
            }

            return tagName;
        }

        /// <summary>
        ///    Adds the selected tag to the group of opportunities with the parameters specified in the request
        /// </summary>
        /// <short>Add tag to opportunity group</short> 
        /// <category>Tags</category>
        /// <param optional="true" name="responsibleid">Opportunity responsible</param>
        /// <param optional="true" name="opportunityStagesid">Opportunity stage ID</param>
        /// <param optional="true" name="tags">Tags</param>
        /// <param optional="true" name="contactid">Contact ID</param>
        /// <param optional="true" name="contactAlsoIsParticipant">Participation status: take into account opportunities where the contact is a participant or not</param>
        /// <param optional="true" name="fromDate">Start date</param>
        /// <param optional="true" name="toDate">End date</param>
        /// <param optional="true" name="stageType" remark="Allowed values: {Open, ClosedAndWon, ClosedAndLost}">Opportunity stage type</param>
        /// <param name="tagName">Tag name</param>
        /// <exception cref="ArgumentException"></exception>
        /// <returns>
        ///    Tag
        /// </returns> 
        [Create(@"opportunity/filter/taglist")]
        public string AddTagToBatchDeals(
            Guid responsibleid,
            int opportunityStagesid,
            IEnumerable<string> tags,
            int contactid,
            DealMilestoneStatus? stageType,
            bool? contactAlsoIsParticipant,
            ApiDateTime fromDate,
            ApiDateTime toDate,
            string tagName)
        {
            var deals = DaoFactory
                .DealDao
                .GetDeals(
                    _context.FilterValue,
                    responsibleid,
                    opportunityStagesid,
                    tags,
                    contactid,
                    stageType,
                    contactAlsoIsParticipant,
                    fromDate, toDate, 0, 0, null).Where(CRMSecurity.CanAccessTo).ToList();

            foreach (var deal in deals)
            {
                AddTagTo("opportunity", deal.ID, tagName);
            }
            return tagName;
        }

        /// <summary>
        ///    Adds the selected tag to the group of cases with the parameters specified in the request
        /// </summary>
        /// <short>Add tag to case group</short> 
        /// <category>Tags</category>
        /// <param optional="true" name="contactid">Contact ID</param>
        /// <param optional="true" name="isClosed">Case status</param>
        /// <param optional="true" name="tags">Tags</param>
        /// <param name="tagName">Tag name</param>
        /// <exception cref="ArgumentException"></exception>
        /// <returns>
        ///    Tag
        /// </returns> 
        [Create(@"case/filter/taglist")]
        public string AddTagToBatchCases(int contactid, bool? isClosed, IEnumerable<string> tags, string tagName)
        {
            var caseses = DaoFactory.CasesDao.GetCases(_context.FilterValue, contactid, isClosed, tags, 0, 0, null)
                .Where(CRMSecurity.CanAccessTo).ToList();

            if (!caseses.Any()) return tagName;

            foreach (var casese in caseses)
            {
                AddTagTo("case", casese.ID, tagName);
            }

            return tagName;
        }

        /// <summary>
        ///  Deletes all the unused tags from the entities with the type specified in the request
        /// </summary>
        /// <short>Delete unused tags</short> 
        /// <category>Tags</category>
        /// <param name="entityType" remark="Allowed values: contact,opportunity,case">Entity type</param>
        /// <returns>Tags</returns>
        [Delete(@"{entityType:(contact|opportunity|case)}/tag/unused")]
        public IEnumerable<string> DeleteUnusedTag(string entityType)
        {
            if (!CRMSecurity.IsAdmin) throw CRMSecurity.CreateSecurityException();

            var entityTypeObj = ToEntityType(entityType);

            var result = DaoFactory.TagDao.GetUnusedTags(entityTypeObj);

            DaoFactory.TagDao.DeleteUnusedTags(entityTypeObj);

            return new List<string>(result);
        }

        /// <summary>
        ///  Adds the selected tag to the entity with the type and ID specified in the request
        /// </summary>
        /// <short>Add tag</short> 
        /// <category>Tags</category>
        /// <param name="entityType" remark="Allowed values: contact,opportunity,case">Entity type</param>
        /// <param name="entityid">Entity ID</param>
        /// <param name="tagName">Tag name</param>
        /// <exception cref="ArgumentException"></exception>
        /// <returns>
        ///   Tag
        /// </returns> 
        [Create(@"{entityType:(contact|opportunity|case)}/{entityid:[0-9]+}/tag")]
        public string AddTagTo(string entityType, int entityid, string tagName)
        {
            if (entityid <= 0 || string.IsNullOrEmpty(tagName)) throw new ArgumentException();

            var entityTypeObj = ToEntityType(entityType);
            DomainObject entityObj;
            var entityTitle = GetEntityTitle(entityTypeObj, entityid, true, out entityObj);

            if (entityTypeObj == EntityType.Contact && !CRMSecurity.CanEdit(entityObj as Contact)) throw CRMSecurity.CreateSecurityException();

            DaoFactory.TagDao.AddTagToEntity(entityTypeObj, entityid, tagName);

            var messageAction = GetTagCreatedAction(entityTypeObj, entityid);
            MessageService.Send(Request, messageAction, MessageTarget.Create(entityid), entityTitle, tagName);

            return tagName;
        }

        /// <summary>
        ///   Adds the selected tag to the entity (company or person) specified in the request and to all related contacts
        /// </summary>
        /// <short>Add tag</short> 
        /// <param name="entityType" remark="Allowed values: company,person">Entity type</param>
        /// <param name="entityid">Entity ID</param>
        /// <param name="tagName">Tag name</param>
        /// <category>Tags</category>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <exception cref="Exception"></exception>
        /// <returns>
        ///   Tag
        /// </returns>
        [Create(@"{entityType:(company|person)}/{entityid:[0-9]+}/tag/group")]
        public string AddContactTagToGroup(string entityType, int entityid, string tagName)
        {
            if (entityid <= 0 || string.IsNullOrEmpty(tagName)) throw new ArgumentException();

            var entityTypeObj = ToEntityType(entityType);
            if (entityTypeObj != EntityType.Company && entityTypeObj != EntityType.Person) throw new ArgumentException();

            var contactInst = DaoFactory.ContactDao.GetByID(entityid);
            if (contactInst == null || !CRMSecurity.CanAccessTo(contactInst)) throw new ItemNotFoundException();

            if (contactInst is Person && entityTypeObj == EntityType.Company) throw new Exception(CRMErrorsResource.ContactIsNotCompany);
            if (contactInst is Company && entityTypeObj == EntityType.Person) throw new Exception(CRMErrorsResource.ContactIsNotPerson);


            var contactIDsToAddTag = new List<int>();


            if (contactInst is Company)
            {
                contactIDsToAddTag.Add(contactInst.ID);

                var members = DaoFactory.ContactDao.GetMembersIDsAndShareType(contactInst.ID);

                foreach (var m in members)
                {
                    if (CRMSecurity.CanAccessTo(m.Key, EntityType.Person, m.Value, 0))
                    {
                        contactIDsToAddTag.Add(m.Key);
                    }
                }
            }
            else
            {
                var CompanyID = ((Person)contactInst).CompanyID;
                if (CompanyID != 0)
                {
                    var cnt = DaoFactory.ContactDao.GetByID(CompanyID);
                    if (cnt != null && cnt is Company && CRMSecurity.CanAccessTo(cnt))
                    {
                        contactIDsToAddTag.Add(CompanyID);

                        var members = DaoFactory.ContactDao.GetMembersIDsAndShareType(CompanyID);

                        foreach (var m in members)
                        {
                            if (CRMSecurity.CanAccessTo(m.Key, EntityType.Person, m.Value, 0))
                            {
                                contactIDsToAddTag.Add(m.Key);
                            }
                        }
                    }
                    else
                    {
                        contactIDsToAddTag.Add(contactInst.ID);
                    }
                }
                else
                {
                    contactIDsToAddTag.Add(contactInst.ID);
                }
            }

            DaoFactory.TagDao.AddTagToContacts(contactIDsToAddTag.ToArray(), tagName);


            var entityTitle = contactInst.GetTitle();
            var messageActions = GetTagCreatedGroupAction(entityTypeObj);
            foreach (var messageAction in messageActions)
            {
                MessageService.Send(Request, messageAction, MessageTarget.Create(contactInst.ID), entityTitle, tagName);
            }

            return tagName;
        }

        /// <summary>
        ///   Deletes the selected tag from the entity with the type specified in the request
        /// </summary>
        /// <short>Delete tag</short> 
        /// <param name="entityType" remark="Allowed values: contact,opportunity,case">Entity type</param>
        /// <param name="tagName">Tag name</param>
        /// <category>Tags</category>
        /// <exception cref="ArgumentException"></exception>
        ///<exception cref="ItemNotFoundException"></exception>
        /// <returns>
        ///   Tag
        /// </returns>
        [Delete(@"{entityType:(contact|opportunity|case)}/tag")]
        public string DeleteTag(string entityType, string tagName)
        {
            if (string.IsNullOrEmpty(entityType) || string.IsNullOrEmpty(tagName)) throw new ArgumentException();


            var entityTypeObj = ToEntityType(entityType);

            if (!DaoFactory.TagDao.IsExist(entityTypeObj, tagName)) throw new ItemNotFoundException();

            DaoFactory.TagDao.DeleteTag(entityTypeObj, tagName);

            var messageAction = GetEntityTagDeletedAction(entityTypeObj);
            MessageService.Send(Request, messageAction, tagName);

            return tagName;
        }

        /// <summary>
        ///  Deletes the selected tag from the entity with the type and ID specified in the request
        /// </summary>
        /// <short>Remove tag</short> 
        /// <category>Tags</category>
        /// <param name="entityType" remark="Allowed values: contact,opportunity,case">Entity type</param>
        /// <param name="entityid">Entity ID</param>
        /// <param name="tagName">Tag name</param>
        /// <exception cref="ArgumentException"></exception>
        /// <returns>
        ///   Tag
        /// </returns> 
        [Delete(@"{entityType:(contact|opportunity|case)}/{entityid:[0-9]+}/tag")]
        public string DeleteTagFrom(string entityType, int entityid, string tagName)
        {
            if (string.IsNullOrEmpty(entityType) || entityid <= 0 || string.IsNullOrEmpty(tagName)) throw new ArgumentException();

            var entityTypeObj = ToEntityType(entityType);
            DomainObject entityObj;
            var entityTitle = GetEntityTitle(entityTypeObj, entityid, true, out entityObj);

            if (entityTypeObj == EntityType.Contact && !CRMSecurity.CanEdit(entityObj as Contact)) throw CRMSecurity.CreateSecurityException();

            if (!DaoFactory.TagDao.IsExist(entityTypeObj, tagName)) throw new ItemNotFoundException();

            DaoFactory.TagDao.DeleteTagFromEntity(entityTypeObj, entityid, tagName);

            var messageAction = GetTagDeletedAction(entityTypeObj, entityid);
            MessageService.Send(Request, messageAction, MessageTarget.Create(entityid), entityTitle, tagName);

            return tagName;
        }

        /// <summary>
        ///   Deletes the selected tag from the entity (company or person) specified in the request and from all related contacts
        /// </summary>
        /// <short>Delete tag</short> 
        /// <param name="entityType" remark="Allowed values: company,person">Entity type</param>
        /// <param name="entityid">Entity ID</param>
        /// <param name="tagName">Tag name</param>
        /// <category>Tags</category>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <exception cref="Exception"></exception>
        /// <returns>
        ///   Tag
        /// </returns>
        [Delete(@"{entityType:(company|person)}/{entityid:[0-9]+}/tag/group")]
        public string DeleteContactTagFromGroup(string entityType, int entityid, string tagName)
        {
            if (entityid <= 0 || string.IsNullOrEmpty(tagName)) throw new ArgumentException();

            var entityTypeObj = ToEntityType(entityType);
            if (entityTypeObj != EntityType.Company && entityTypeObj != EntityType.Person) throw new ArgumentException();

            var contactInst = DaoFactory.ContactDao.GetByID(entityid);
            if (contactInst == null) throw new ItemNotFoundException();

            if (contactInst is Person && entityTypeObj == EntityType.Company) throw new Exception(CRMErrorsResource.ContactIsNotCompany);
            if (contactInst is Company && entityTypeObj == EntityType.Person) throw new Exception(CRMErrorsResource.ContactIsNotPerson);



            var contactIDsForDeleteTag = new List<int>();

            if (contactInst is Company)
            {
                contactIDsForDeleteTag.Add(contactInst.ID);

                var members = DaoFactory.ContactDao.GetMembersIDsAndShareType(contactInst.ID);

                foreach (var m in members)
                {
                    if (CRMSecurity.CanAccessTo(m.Key, EntityType.Person, m.Value, 0))
                    {
                        contactIDsForDeleteTag.Add(m.Key);
                    }
                }
            }
            else
            {
                var CompanyID = ((Person)contactInst).CompanyID;
                if (CompanyID != 0)
                {
                    var cnt = DaoFactory.ContactDao.GetByID(CompanyID);
                    if (cnt != null && cnt is Company && CRMSecurity.CanAccessTo(cnt))
                    {
                        contactIDsForDeleteTag.Add(CompanyID);

                        var members = DaoFactory.ContactDao.GetMembersIDsAndShareType(CompanyID);

                        foreach (var m in members)
                        {
                            if (CRMSecurity.CanAccessTo(m.Key, EntityType.Person, m.Value, 0))
                            {
                                contactIDsForDeleteTag.Add(m.Key);
                            }
                        }
                    }
                    else
                    {
                        contactIDsForDeleteTag.Add(contactInst.ID);
                    }
                }
                else
                {
                    contactIDsForDeleteTag.Add(contactInst.ID);
                }
            }

            DaoFactory.TagDao.DeleteTagFromContacts(contactIDsForDeleteTag.ToArray(), tagName);

            return tagName;
        }


        private static MessageAction GetEntityTagCreatedAction(EntityType entityType)
        {
            switch (entityType)
            {
                case EntityType.Contact:
                    return MessageAction.ContactsCreatedTag;
                case EntityType.Opportunity:
                    return MessageAction.OpportunitiesCreatedTag;
                case EntityType.Case:
                    return MessageAction.CasesCreatedTag;
                default:
                    throw new ArgumentException("Invalid entityType: " + entityType);
            }
        }

        private static MessageAction GetEntityTagDeletedAction(EntityType entityType)
        {
            switch (entityType)
            {
                case EntityType.Contact:
                    return MessageAction.ContactsDeletedTag;
                case EntityType.Opportunity:
                    return MessageAction.OpportunitiesDeletedTag;
                case EntityType.Case:
                    return MessageAction.CasesDeletedTag;
                default:
                    throw new ArgumentException("Invalid entityType: " + entityType);
            }
        }

        private MessageAction GetTagCreatedAction(EntityType entityType, int entityId)
        {
            switch (entityType)
            {
                case EntityType.Contact:
                    var entity = DaoFactory.ContactDao.GetByID(entityId);
                    return entity is Company ? MessageAction.CompanyCreatedTag : MessageAction.PersonCreatedTag;
                case EntityType.Company:
                    return MessageAction.CompanyCreatedTag;
                case EntityType.Person:
                    return MessageAction.PersonCreatedTag;
                case EntityType.Opportunity:
                    return MessageAction.OpportunityCreatedTag;
                case EntityType.Case:
                    return MessageAction.CaseCreatedTag;
                default:
                    throw new ArgumentException("Invalid entityType: " + entityType);
            }
        }

        private static IEnumerable<MessageAction> GetTagCreatedGroupAction(EntityType entityType)
        {
            switch (entityType)
            {
                case EntityType.Company:
                    return new List<MessageAction> {MessageAction.CompanyCreatedTag, MessageAction.CompanyCreatedPersonsTag};
                case EntityType.Person:
                    return new List<MessageAction> {MessageAction.PersonCreatedTag, MessageAction.PersonCreatedCompanyTag};
                default:
                    throw new ArgumentException("Invalid entityType: " + entityType);
            }
        }

        private MessageAction GetTagDeletedAction(EntityType entityType, int entityId)
        {
            switch (entityType)
            {
                case EntityType.Contact:
                    var entity = DaoFactory.ContactDao.GetByID(entityId);
                    return entity is Company ? MessageAction.CompanyDeletedTag : MessageAction.PersonDeletedTag;
                case EntityType.Company:
                    return MessageAction.CompanyDeletedTag;
                case EntityType.Person:
                    return MessageAction.PersonDeletedTag;
                case EntityType.Opportunity:
                    return MessageAction.OpportunityDeletedTag;
                case EntityType.Case:
                    return MessageAction.CaseDeletedTag;
                default:
                    throw new ArgumentException("Invalid entityType: " + entityType);
            }
        }
    }
}