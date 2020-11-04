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
using System.Security;
using System.Collections.Generic;
using System.Linq;
using ASC.Api.Attributes;
using ASC.Api.CRM.Wrappers;
using ASC.Api.Collections;
using ASC.Api.Exceptions;
using ASC.CRM.Core;
using ASC.CRM.Core.Entities;
using ASC.ElasticSearch;
using ASC.MessagingSystem;
using ASC.Web.CRM.Core.Search;

namespace ASC.Api.CRM
{
    public partial class CRMApi
    {
        /// <summary>
        ///    Returns the list of descriptions for all existing user fields
        /// </summary>
        /// <param name="entityType" remark="Allowed values: contact,person,company,opportunity,case">Type</param>
        /// <short>Get user field list</short> 
        /// <category>User fields</category>
        ///<returns>
        ///    User field list
        /// </returns>
        ///<exception cref="ArgumentException"></exception>
        [Read(@"{entityType:(contact|person|company|opportunity|case)}/customfield/definitions")]
        public IEnumerable<CustomFieldWrapper> GetCustomFieldDefinitions(string entityType)
        {
            return DaoFactory.CustomFieldDao.GetFieldsDescription(ToEntityType(entityType)).ConvertAll(ToCustomFieldWrapper).ToSmartList();
        }

        /// <summary>
        ///   Returns the list of all user field values using the entity type and entity ID specified in the request
        /// </summary>
        /// <param name="entityType" remark="Allowed values: contact,person,company,opportunity,case">Type</param>
        /// <param name="entityid">ID</param>
        /// <short>Get user field values</short> 
        /// <category>User fields</category>
        /// <returns></returns>
        [Read(@"{entityType:(contact|person|company|opportunity|case)}/{entityid:[0-9]+}/customfield")]
        public IEnumerable<CustomFieldBaseWrapper> GetCustomFieldForSubject(string entityType, int entityid)
        {
            return DaoFactory.CustomFieldDao.GetEnityFields(ToEntityType(entityType), entityid, false).ConvertAll(ToCustomFieldBaseWrapper).ToItemList();
        }

        /// <summary>
        ///    Sets the new user field value using the entity type, ID, field ID and value specified in the request
        /// </summary>
        /// <param name="entityType" remark="Allowed values: contact,person,company,opportunity,case">Type</param>
        /// <param name="entityid">ID</param>
        /// <param name="fieldid">Field ID</param>
        /// <param name="fieldValue">Field Value</param>
        /// <short>Set user field value</short> 
        /// <category>User fields</category>
        /// <returns>
        ///    User field
        /// </returns>
        [Create(@"{entityType:(contact|person|company|opportunity|case)}/{entityid:[0-9]+}/customfield/{fieldid:[0-9]+}")]
        public CustomFieldBaseWrapper SetEntityCustomFieldValue(string entityType, int entityid, int fieldid, string fieldValue)
        {
            var customField = DaoFactory.CustomFieldDao.GetFieldDescription(fieldid);

            var entityTypeStr = ToEntityType(entityType);

            customField.EntityID = entityid;
            customField.Value = fieldValue;

            DaoFactory.CustomFieldDao.SetFieldValue(entityTypeStr, entityid, fieldid, fieldValue);

            return ToCustomFieldBaseWrapper(customField);
        }

        /// <summary>
        ///    Creates a new user field with the parameters (entity type, field title, type, etc.) specified in the request
        /// </summary>
        /// <param optional="false" name="entityType" remark="Allowed values: contact,person,company,opportunity,case">Entity type</param>
        /// <param optional="false" name="label">Field title</param>
        /// <param name="fieldType" 
        /// remark="Allowed values: TextField, TextArea, SelectBox, CheckBox, Heading or Date">
        ///   User field value
        /// </param>
        /// <param optional="true" name="position">Field position</param>
        /// <param optional="true" name="mask" remark="Sent in json format only" >Mask</param>
        /// <short>Create user field</short> 
        /// <category>User fields</category>
        /// <returns>
        ///    User field
        /// </returns>
        ///<example>
        /// <![CDATA[
        /// 
        /// Data transfer in application/json format:
        /// 
        /// 1) Creation of a user field of  TextField type
        /// 
        /// data: {
        ///    entityType: "contact",
        ///    label: "Sample TextField",
        ///    fieldType: 0,
        ///    position: 0,
        ///    mask: {"size":"40"}        - this is the text field size. All other values are ignored.
        /// }
        /// 
        /// 
        /// 2) Creation of a user field of TextArea type
        /// 
        /// data: {
        ///    entityType: "contact",
        ///    label: "Sample TextArea",
        ///    fieldType: 1,
        ///    position: 1,
        ///    mask: '{"rows":"2","cols":"30"}'        - this is the TextArea size. All other values are ignored.
        /// }
        /// 
        /// 
        /// 3) Creation of a user field of   SelectBox type
        /// 
        /// data: {
        ///    entityType: "contact",
        ///    label: "Sample SelectBox",
        ///    fieldType: 2,
        ///    position: 0,
        ///    mask: ["1","2","3"]   - SelectBox values.
        /// }
        /// 
        /// 
        /// 
        /// 4) Creation of a user field of  CheckBox type
        /// 
        /// data: {
        ///    entityType: "contact",
        ///    label: "Sample CheckBox",
        ///    fieldType: 3,
        ///    position: 0,
        ///    mask: ""     
        /// }
        /// 
        /// 
        /// 
        /// 5) Creation of a user field of   Heading type
        /// 
        /// data: {
        ///    entityType: "contact",
        ///    label: "Sample Heading",
        ///    fieldType: 4,
        ///    position: 0,
        ///    mask: "" 
        /// }
        /// 
        /// 
        /// 
        /// 6) Creation of a user field of   Date type
        /// 
        /// data: {
        ///    entityType: "contact",
        ///    label: "Sample Date",
        ///    fieldType: 5,
        ///    position: 0,
        ///    mask: "" 
        /// }
        /// 
        /// 
        /// ]]>
        /// </example>
        [Create(@"{entityType:(contact|person|company|opportunity|case)}/customfield")]
        public CustomFieldWrapper CreateCustomFieldValue(string entityType, string label, int fieldType, int position, string mask)
        {
            if (!(CRMSecurity.IsAdmin)) throw CRMSecurity.CreateSecurityException();
            var entityTypeObj = ToEntityType(entityType);
            var fieldID = DaoFactory.CustomFieldDao.CreateField(entityTypeObj, label, (CustomFieldType)fieldType, mask);
            var wrapper = DaoFactory.CustomFieldDao.GetFieldDescription(fieldID);

            var messageAction = GetCustomFieldCreatedAction(entityTypeObj);
            MessageService.Send(Request, messageAction, MessageTarget.Create(wrapper.ID), wrapper.Label);

            return ToCustomFieldWrapper(DaoFactory.CustomFieldDao.GetFieldDescription(fieldID));
        }

        /// <summary>
        ///    Updates the selected user field with the parameters (entity type, field title, type, etc.) specified in the request
        /// </summary>
        /// <param name="id">User field id</param>
        /// <param name="entityType" remark="Allowed values: contact,person,company,opportunity,case">Entity type</param>
        /// <param optional="false" name="label">Field title</param>
        /// <param name="fieldType" 
        /// remark="Allowed values: 0 (TextField),1 (TextArea),2 (SelectBox),3 (CheckBox),4 (Heading) or 5 (Date)">
        ///   User field value
        /// </param>
        /// <param optional="true" name="position">Field position</param>
        /// <param optional="true" name="mask" remark="Sent in json format only" >Mask</param>
        /// <short> Updates the selected user field</short> 
        /// <category>User fields</category>
        /// <returns>
        ///    User field
        /// </returns>
        ///<remarks>
        /// <![CDATA[
        ///  You can update field if there is no related elements. If such elements exist there will be updated only label and mask, other parameters will be ignored.
        /// ]]>
        /// </remarks>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        [Update(@"{entityType:(contact|person|company|opportunity|case)}/customfield/{id:[0-9]+}")]
        public CustomFieldWrapper UpdateCustomFieldValue(int id, string entityType, string label, int fieldType, int position, string mask)
        {
            if (id <= 0) throw new ArgumentException();
            if (!DaoFactory.CustomFieldDao.IsExist(id)) throw new ItemNotFoundException();

            var entityTypeObj = ToEntityType(entityType);

            var customField = new CustomField
                {
                    EntityType = entityTypeObj,
                    FieldType = (CustomFieldType)fieldType,
                    ID = id,
                    Mask = mask,
                    Label = label,
                    Position = position
                };

            DaoFactory.CustomFieldDao.EditItem(customField);

            customField = DaoFactory.CustomFieldDao.GetFieldDescription(id);

            var messageAction = GetCustomFieldUpdatedAction(entityTypeObj);
            MessageService.Send(Request, messageAction, MessageTarget.Create(customField.ID), customField.Label);

            return ToCustomFieldWrapper(customField);
        }

        /// <summary>
        ///    Deletes the user field with the ID specified in the request
        /// </summary>
        /// <param name="entityType" remark="Allowed values: contact,person,company,opportunity,case">Type</param>
        /// <param name="fieldid">Field ID</param>
        /// <short>Delete user field</short> 
        /// <category>User fields</category>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <returns>
        ///    User field
        /// </returns>
        [Delete(@"{entityType:(contact|person|company|opportunity|case)}/customfield/{fieldid:[0-9]+}")]
        public CustomFieldWrapper DeleteCustomField(string entityType, int fieldid)
        {
            if (!(CRMSecurity.IsAdmin)) throw CRMSecurity.CreateSecurityException();
            if (fieldid <= 0) throw new ArgumentException();

            var customField = DaoFactory.CustomFieldDao.GetFieldDescription(fieldid);
            if (customField == null) throw new ItemNotFoundException();

            var result = ToCustomFieldWrapper(customField);
            DaoFactory.CustomFieldDao.DeleteField(fieldid);

            FactoryIndexer<FieldsWrapper>.DeleteAsync(customField);

            var messageAction = GetCustomFieldDeletedAction(ToEntityType(entityType));
            MessageService.Send(Request, messageAction, MessageTarget.Create(customField.ID), result.Label);

            return result;
        }

        /// <summary>
        ///    Updates user fields order
        /// </summary>
        /// <param name="fieldids">User field ID list</param>
        /// <param name="entityType" remark="Allowed values: contact,person,company,opportunity,case">Entity type</param>
        /// <category>User fields</category>
        /// <returns>
        ///    User fields
        /// </returns>
        /// <exception cref="SecurityException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        [Update(@"{entityType:(contact|person|company|opportunity|case)}/customfield/reorder")]
        public IEnumerable<CustomFieldBaseWrapper> UpdateCustomFieldsOrder(IEnumerable<int> fieldids, string entityType)
        {
            if (fieldids == null) throw new ArgumentException();
            if (!(CRMSecurity.IsAdmin)) throw CRMSecurity.CreateSecurityException();

            var customFields = new List<CustomField>();
            foreach (var id in fieldids)
            {
                if (!DaoFactory.CustomFieldDao.IsExist(id)) throw new ItemNotFoundException();
                customFields.Add(DaoFactory.CustomFieldDao.GetFieldDescription(id));
            }

            DaoFactory.CustomFieldDao.ReorderFields(fieldids.ToArray());

            var messageAction = GetCustomFieldsUpdatedOrderAction(ToEntityType(entityType));
            MessageService.Send(Request, messageAction, MessageTarget.Create(fieldids), customFields.Select(x => x.Label));

            return customFields.Select(ToCustomFieldBaseWrapper);
        }

        private static CustomFieldBaseWrapper ToCustomFieldBaseWrapper(CustomField customField)
        {
            return new CustomFieldBaseWrapper(customField);
        }

        private CustomFieldWrapper ToCustomFieldWrapper(CustomField customField)
        {
            var result = new CustomFieldWrapper(customField)
                {
                    RelativeItemsCount = DaoFactory.CustomFieldDao.GetContactLinkCount(customField.EntityType, customField.ID)
                };
            return result;
        }

        private static MessageAction GetCustomFieldCreatedAction(EntityType entityType)
        {
            switch (entityType)
            {
                case EntityType.Contact:
                    return MessageAction.ContactUserFieldCreated;
                case EntityType.Person:
                    return MessageAction.PersonUserFieldCreated;
                case EntityType.Company:
                    return MessageAction.CompanyUserFieldCreated;
                case EntityType.Opportunity:
                    return MessageAction.OpportunityUserFieldCreated;
                case EntityType.Case:
                    return MessageAction.CaseUserFieldCreated;
                default:
                    throw new ArgumentException("Invalid entityType: " + entityType);
            }
        }

        private static MessageAction GetCustomFieldUpdatedAction(EntityType entityType)
        {
            switch (entityType)
            {
                case EntityType.Contact:
                    return MessageAction.ContactUserFieldUpdated;
                case EntityType.Person:
                    return MessageAction.PersonUserFieldUpdated;
                case EntityType.Company:
                    return MessageAction.CompanyUserFieldUpdated;
                case EntityType.Opportunity:
                    return MessageAction.OpportunityUserFieldUpdated;
                case EntityType.Case:
                    return MessageAction.CaseUserFieldUpdated;
                default:
                    throw new ArgumentException("Invalid entityType: " + entityType);
            }
        }

        private static MessageAction GetCustomFieldDeletedAction(EntityType entityType)
        {
            switch (entityType)
            {
                case EntityType.Contact:
                    return MessageAction.ContactUserFieldDeleted;
                case EntityType.Person:
                    return MessageAction.PersonUserFieldDeleted;
                case EntityType.Company:
                    return MessageAction.CompanyUserFieldDeleted;
                case EntityType.Opportunity:
                    return MessageAction.OpportunityUserFieldDeleted;
                case EntityType.Case:
                    return MessageAction.CaseUserFieldDeleted;
                default:
                    throw new ArgumentException("Invalid entityType: " + entityType);
            }
        }

        private static MessageAction GetCustomFieldsUpdatedOrderAction(EntityType entityType)
        {
            switch (entityType)
            {
                case EntityType.Contact:
                    return MessageAction.ContactUserFieldsUpdatedOrder;
                case EntityType.Person:
                    return MessageAction.PersonUserFieldsUpdatedOrder;
                case EntityType.Company:
                    return MessageAction.CompanyUserFieldsUpdatedOrder;
                case EntityType.Opportunity:
                    return MessageAction.OpportunityUserFieldsUpdatedOrder;
                case EntityType.Case:
                    return MessageAction.CaseUserFieldsUpdatedOrder;
                default:
                    throw new ArgumentException("Invalid entityType: " + entityType);
            }
        }
    }
}