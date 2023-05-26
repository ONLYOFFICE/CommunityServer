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

using ASC.Api.Attributes;
using ASC.Api.Collections;
using ASC.Api.CRM.Wrappers;
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
        /// Returns a list of descriptions for all the existing custom fields.
        /// </summary>
        /// <param type="System.String, System" method="url" name="entityType" remark="Allowed values: contact, person, company, opportunity, case">Entity type</param>
        /// <short>Get custom fields</short> 
        /// <category>Custom fields</category>
        ///<returns type="ASC.Api.CRM.Wrappers.CustomFieldWrapper, ASC.Api.CRM">
        /// List of custom fields
        /// </returns>
        /// <path>api/2.0/crm/{entityType}/customfield/definitions</path>
        /// <httpMethod>GET</httpMethod>
        ///<exception cref="ArgumentException"></exception>
        ///<collection>list</collection>
        [Read(@"{entityType:(contact|person|company|opportunity|case)}/customfield/definitions")]
        public IEnumerable<CustomFieldWrapper> GetCustomFieldDefinitions(string entityType)
        {
            return DaoFactory.CustomFieldDao.GetFieldsDescription(ToEntityType(entityType)).ConvertAll(ToCustomFieldWrapper).ToSmartList();
        }

        /// <summary>
        /// Returns a list of all the custom fields for the entity type and ID specified in the request.
        /// </summary>
        /// <param type="System.String, System" method="url" name="entityType" remark="Allowed values: contact, person, company, opportunity, case">Entity type</param>
        /// <param type="System.Int32, System" method="url" name="entityid">Entity ID</param>
        /// <short>Get entity custom fields</short> 
        /// <category>Custom fields</category>
        /// <returns type="ASC.Api.CRM.Wrappers.CustomFieldBaseWrapper, ASC.Api.CRM">List of entity custom fields</returns>
        /// <path>api/2.0/crm/{entityType}/{entityid}/customfield</path>
        /// <httpMethod>GET</httpMethod>
        /// <collection>list</collection>
        [Read(@"{entityType:(contact|person|company|opportunity|case)}/{entityid:[0-9]+}/customfield")]
        public IEnumerable<CustomFieldBaseWrapper> GetCustomFieldForSubject(string entityType, int entityid)
        {
            return DaoFactory.CustomFieldDao.GetEntityFields(ToEntityType(entityType), entityid, false).ConvertAll(ToCustomFieldBaseWrapper).ToItemList();
        }

        /// <summary>
        /// Sets the selected custom field to the entity with type and ID specified in the request.
        /// </summary>
        /// <param type="System.String, System" method="url" name="entityType" remark="Allowed values: contact, person, company, opportunity, case">Entity type</param>
        /// <param type="System.Int32, System" method="url" name="entityid">Entity ID</param>
        /// <param type="System.Int32, System" method="url" name="fieldid">Field ID</param>
        /// <param type="System.String, System" name="fieldValue">Field value</param>
        /// <short>Set an entity custom field</short> 
        /// <category>Custom fields</category>
        /// <returns type="ASC.Api.CRM.Wrappers.CustomFieldBaseWrapper, ASC.Api.CRM">
        /// Custom field
        /// </returns>
        /// <path>api/2.0/crm/{entityType}/{entityid}/customfield/{fieldid}</path>
        /// <httpMethod>POST</httpMethod>
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
        /// Creates a new custom field with the parameters (entity type, field title, type, etc.) specified in the request.
        /// </summary>
        /// <param type="System.String, System" method="url" optional="false" name="entityType" remark="Allowed values: contact, person, company, opportunity, case">Entity type</param>
        /// <param type="System.String, System" optional="false" name="label">Field title</param>
        /// <param type="System.Int32, System" name="fieldType" 
        /// remark="Allowed values: 0 (TextField), 1 (TextArea), 2 (SelectBox), 3 (CheckBox), 4 (Heading), or 5 (Date)">
        ///  Custom field type
        /// </param>
        /// <param type="System.Int32, System" optional="true" name="position">Field position</param>
        /// <param type="System.String, System" optional="true" name="mask" remark="Sent in JSON format only" >Mask</param>
        /// <short>Create a custom field</short> 
        /// <category>Custom fields</category>
        /// <returns type="ASC.Api.CRM.Wrappers.CustomFieldWrapper, ASC.Api.CRM">
        /// Custom field
        /// </returns>
        ///<example>
        /// <![CDATA[
        /// 
        /// Data transfer in application/json format:
        /// 
        /// 1) Creation of the TextField custom field:
        /// 
        /// data: {
        ///    entityType: "contact",
        ///    label: "Sample TextField",
        ///    fieldType: 0,
        ///    position: 0,
        ///    mask: {"size":"40"}  // This is the TextField size. All other values are ignored.
        /// }
        /// 
        /// 
        /// 2) Creation of the TextArea custom field:
        /// 
        /// data: {
        ///    entityType: "contact",
        ///    label: "Sample TextArea",
        ///    fieldType: 1,
        ///    position: 1,
        ///    mask: '{"rows":"2","cols":"30"}' // This is the TextArea size. All other values are ignored.
        /// }
        /// 
        /// 
        /// 3) Creation of the SelectBox custom field:
        /// 
        /// data: {
        ///    entityType: "contact",
        ///    label: "Sample SelectBox",
        ///    fieldType: 2,
        ///    position: 0,
        ///    mask: ["1","2","3"]  // These are the SelectBox values.
        /// }
        /// 
        /// 
        /// 
        /// 4) Creation of the CheckBox custom field:
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
        /// 5) Creation of the Heading custom field:
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
        /// 6) Creation of the Date custom field:
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
        /// <path>api/2.0/crm/{entityType}/customfield</path>
        /// <httpMethod>POST</httpMethod>
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
        /// Updates the selected custom field with the parameters (entity type, field title, type, etc.) specified in the request.
        /// </summary>
        /// <param type="System.Int32, System" method="url" name="id">Custom field ID</param>
        /// <param type="System.String, System" method="url" name="entityType" remark="Allowed values: contact, person, company, opportunity, case">New entity type</param>
        /// <param type="System.String, System" optional="false" name="label">New field title</param>
        /// <param type="System.Int32, System" name="fieldType" 
        /// remark="Allowed values: 0 (TextField), 1 (TextArea), 2 (SelectBox), 3 (CheckBox), 4 (Heading), or 5 (Date)">
        ///  New custom field type
        /// </param>
        /// <param type="System.Int32, System" optional="true" name="position">New field position</param>
        /// <param type="System.String, System" optional="true" name="mask" remark="Sent in json format only" >New mask</param>
        /// <short>Update a custom field</short> 
        /// <category>Custom fields</category>
        /// <returns type="ASC.Api.CRM.Wrappers.CustomFieldWrapper, ASC.Api.CRM">
        /// Updated custom field
        /// </returns>
        ///<remarks>
        /// <![CDATA[
        ///  You can update field if there are no related elements. If such elements exist, only label and mask will be updated. Other parameters will be ignored.
        /// ]]>
        /// </remarks>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <path>api/2.0/crm/{entityType}/customfield/{id}</path>
        /// <httpMethod>PUT</httpMethod>
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
        /// Deletes a custom field with the ID specified in the request.
        /// </summary>
        /// <param type="System.String, System" method="url" name="entityType" remark="Allowed values: contact, person, company, opportunity, case">Entity type</param>
        /// <param type="System.Int32, System" method="url" name="fieldid">Field ID</param>
        /// <short>Delete a custom field</short> 
        /// <category>Custom fields</category>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <returns type="ASC.Api.CRM.Wrappers.CustomFieldWrapper, ASC.Api.CRM">
        /// Custom field
        /// </returns>
        /// <path>api/2.0/crm/{entityType}/customfield/{fieldid}</path>
        /// <httpMethod>DELETE</httpMethod>
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
        /// Updates the order of the custom fields with a list specified in the request.
        /// </summary>
        /// <param type="System.Collections.Generic.IEnumerable{System.Int32}, System.Collections.Generic" name="fieldids">List of custom field IDs</param>
        /// <param type="System.String, System" method="url" name="entityType" remark="Allowed values: contact, person, company, opportunity, case">Entity type</param>
        /// <short>Update the order of custom fields</short> 
        /// <category>Custom fields</category>
        /// <returns type="ASC.Api.CRM.Wrappers.CustomFieldBaseWrapper, ASC.Api.CRM">
        /// Custom fields in the new order
        /// </returns>
        /// <exception cref="SecurityException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <path>api/2.0/crm/{entityType}/customfield/reorder</path>
        /// <httpMethod>PUT</httpMethod>
        /// <collection>list</collection>
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