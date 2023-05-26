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

using ASC.Api.Attributes;
using ASC.Api.Collections;
using ASC.Api.CRM.Wrappers;
using ASC.Api.Exceptions;
using ASC.CRM.Core;
using ASC.CRM.Core.Entities;
using ASC.ElasticSearch;
using ASC.MessagingSystem;
using ASC.Web.CRM.Classes;
using ASC.Web.CRM.Core.Search;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ASC.Api.CRM
{
    ///<name>crm</name>
    public partial class CRMApi
    {
        /// <summary>
        /// Returns a list of all the available contact categories of the specified information type.
        /// </summary>
        /// <param type="ASC.CRM.Core.ContactInfoType, ASC.CRM.Core" method="url" name="infoType">Contact information type</param>
        /// <short>Get contact categories by information type</short> 
        /// <category>Contacts</category>
        /// <returns>
        /// List of contact categories
        /// </returns>
        /// <path>api/2.0/crm/contact/data/{infoType}/category</path>
        /// <httpMethod>GET</httpMethod>
        /// <collection>list</collection>
        [Read(@"contact/data/{infoType}/category")]
        public IEnumerable<string> GetContactInfoCategory(ContactInfoType infoType)
        {
            return Enum.GetNames(ContactInfo.GetCategory(infoType)).ToItemList();
        }

        /// <summary>
        /// Returns a list of all the available contact information types.
        /// </summary>
        /// <short>Get contact information types</short> 
        /// <category>Contacts</category>
        /// <returns>List of all the contact information types</returns>
        /// <collection>list</collection>
        /// <path>api/2.0/crm/contact/data/infoType</path>
        /// <httpMethod>GET</httpMethod>
        [Read(@"contact/data/infoType")]
        public IEnumerable<string> GetContactInfoType()
        {
            return Enum.GetNames(typeof(ContactInfoType)).ToItemList();
        }

        /// <summary>
        /// Returns the detailed information on the contact with the ID specified in the request.
        /// </summary>
        /// <param type="System.Int32, System" method="url" name="contactid">Contact ID</param>
        /// <short>Get contact information</short> 
        /// <category>Contacts</category>
        /// <returns type="ASC.Api.CRM.Wrappers.ContactInfoWrapper, ASC.Api.CRM">
        /// Contact information
        /// </returns>
        /// <path>api/2.0/crm/contact/{contactid}/data</path>
        /// <httpMethod>GET</httpMethod>
        /// <collection>list</collection>
        [Read(@"contact/{contactid:[0-9]+}/data")]
        public IEnumerable<ContactInfoWrapper> GetContactInfo(int contactid)
        {
            if (contactid <= 0) throw new ArgumentException();

            var contact = DaoFactory.ContactDao.GetByID(contactid);
            if (contact == null || !CRMSecurity.CanAccessTo(contact)) throw new ItemNotFoundException();

            return DaoFactory.ContactInfoDao.GetList(contactid, null, null, null)
                .OrderByDescending(info => info.ID)
                .ToList()
                .ConvertAll(ToContactInfoWrapper);
        }

        /// <summary>
        /// Returns the detailed contact information with the ID specified in the request.
        /// </summary>
        /// <param type="System.Int32, System" method="url" name="contactid">Contact ID</param>
        /// <param type="System.Int32, System" method="url" name="id">Contact information ID</param>
        /// <short>Get contact information by ID</short> 
        /// <category>Contacts</category>
        /// <returns type="ASC.Api.CRM.Wrappers.ContactInfoWrapper, ASC.Api.CRM">Contact information</returns>
        ///<exception cref="ArgumentException"></exception>
        ///<path>api/2.0/crm/contact/{contactid}/data/{id}</path>
        ///<httpMethod>GET</httpMethod>
        [Read(@"contact/{contactid:[0-9]+}/data/{id:[0-9]+}")]
        public ContactInfoWrapper GetContactInfoByID(int contactid, int id)
        {
            if (contactid <= 0 || id <= 0) throw new ArgumentException();

            var contact = DaoFactory.ContactDao.GetByID(contactid);
            if (contact == null || !CRMSecurity.CanAccessTo(contact)) throw new ItemNotFoundException();

            var contactInfo = DaoFactory.ContactInfoDao.GetByID(id);

            if (contactInfo == null || contactInfo.ContactID != contactid) throw new ArgumentException();

            return ToContactInfoWrapper(contactInfo);
        }

        /// <summary>
        /// Adds the information with the parameters specified in the request to the contact with the selected ID.
        /// </summary>
        ///<param type="System.Int32, System" method="url" name="contactid">Contact ID</param>
        ///<param type="ASC.CRM.Core.ContactInfoType, ASC.CRM.Core" name="infoType">Contact information type</param>
        ///<param type="System.String, System" name="data">New data</param>
        ///<param type="System.Boolean, System" name="isPrimary">Contact information importance: primary or not</param>
        ///<param type="System.String, System" name="category">Contact information category</param>
        ///<short>Add contact information</short> 
        ///<category>Contacts</category>
        /// <seealso cref="GetContactInfoType"/>
        /// <seealso cref="GetContactInfoCategory"/>
        /// <returns type="ASC.Api.CRM.Wrappers.ContactInfoWrapper,  ASC.Api.CRM">
        /// Contact information
        /// </returns> 
        ///<exception cref="ArgumentException"></exception>
        ///<path>api/2.0/crm/contact/{contactid}/data</path>
        ///<httpMethod>POST</httpMethod>
        [Create(@"contact/{contactid:[0-9]+}/data")]
        public ContactInfoWrapper CreateContactInfo(int contactid, ContactInfoType infoType, string data, bool isPrimary, string category)
        {
            if (string.IsNullOrEmpty(data) || contactid <= 0) throw new ArgumentException();
            var contact = DaoFactory.ContactDao.GetByID(contactid);
            if (contact == null) throw new ItemNotFoundException();

            if (infoType == ContactInfoType.Twitter)
            {
                if (!CRMSecurity.CanAccessTo(contact)) throw new ItemNotFoundException();
            }
            else
            {
                if (!CRMSecurity.CanEdit(contact)) throw new ItemNotFoundException();
            }

            var categoryType = ContactInfo.GetCategory(infoType);
            if (!Enum.IsDefined(categoryType, category)) throw new ArgumentException();


            var contactInfo = new ContactInfo
            {
                Data = data,
                InfoType = infoType,
                ContactID = contactid,
                IsPrimary = isPrimary,
                Category = (int)Enum.Parse(categoryType, category)
            };

            if (contactInfo.InfoType == ContactInfoType.Address)
            {
                Address res;
                if (!Address.TryParse(contactInfo, out res))
                    throw new ArgumentException();
            }

            var contactInfoID = DaoFactory.ContactInfoDao.Save(contactInfo);

            var messageAction = contact is Company ? MessageAction.CompanyUpdatedPrincipalInfo : MessageAction.PersonUpdatedPrincipalInfo;
            MessageService.Send(Request, messageAction, MessageTarget.Create(contact.ID), contact.GetTitle());

            var contactInfoWrapper = ToContactInfoWrapper(contactInfo);
            contactInfoWrapper.ID = contactInfoID;
            return contactInfoWrapper;
        }

        /// <summary>
        /// Adds the address information to the contact with the ID specified in the request.
        /// </summary>
        /// <param type="System.Int32, System" method="url" name="contactid">Contact ID</param>
        /// <param type="ASC.Api.CRM.Wrappers.Address, ASC.Api.CRM.Wrappers" name="address">Address data</param>
        /// <short>Add contact address information</short> 
        /// <category>Contacts</category>
        /// <seealso cref="GetContactInfoType"/>
        /// <seealso cref="GetContactInfoCategory"/>
        /// <returns type="ASC.Api.CRM.Wrappers.ContactInfoWrapper, ASC.Api.CRM">
        /// Contact information
        /// </returns> 
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <path>api/2.0/crm/contact/{contactid}/addressdata</path>
        /// <httpMethod>POST</httpMethod>
        [Create(@"contact/{contactid:[0-9]+}/addressdata")]
        public ContactInfoWrapper CreateContactInfoAddress(int contactid, Address address)
        {
            if (contactid <= 0) throw new ArgumentException("Invalid value", "contactid");

            var contact = DaoFactory.ContactDao.GetByID(contactid);

            if (contact == null || !CRMSecurity.CanEdit(contact)) throw new ItemNotFoundException();

            if (address == null) throw new ArgumentException("Value cannot be null", "address");
            if (address.City == null) address.City = "";
            if (address.Country == null) address.Country = "";
            if (address.State == null) address.State = "";
            if (address.Street == null) address.Street = "";
            if (address.Zip == null) address.Zip = "";

            if (!Enum.IsDefined(typeof(AddressCategory), address.Category)) throw new ArgumentException("Value does not fall within the expected range.", "address.Category");

            address.CategoryName = ((AddressCategory)address.Category).ToLocalizedString();

            var settings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                },
                Formatting = Formatting.Indented
            };

            var contactInfo = new ContactInfo
            {
                InfoType = ContactInfoType.Address,
                ContactID = contactid,
                IsPrimary = address.IsPrimary,
                Category = address.Category,
                Data = JsonConvert.SerializeObject(address, settings)
            };

            contactInfo.ID = DaoFactory.ContactInfoDao.Save(contactInfo);

            var messageAction = contact is Company ? MessageAction.CompanyUpdatedPrincipalInfo : MessageAction.PersonUpdatedPrincipalInfo;
            MessageService.Send(Request, messageAction, MessageTarget.Create(contact.ID), contact.GetTitle());

            return ToContactInfoWrapper(contactInfo);
        }

        /// <summary>
        /// Adds a list of contact information items with the parameters specified in the request for the contact with the selected ID.
        /// </summary>
        ///<short>Add contact information items</short> 
        /// <param type="System.Int32, System" name="contactid">Contact ID</param>
        /// <param type="System.Collections.Generic.IEnumerable{ASC.Api.CRM.Wrappers.ContactInfoWrapper}, System.Collections.Generic" name="items">Contact information</param>
        /// <remarks>
        /// <![CDATA[
        ///  Items have the following format:
        ///  [{infoType : 1, category : 1, categoryName : 'work', data : "myemail@email.com", isPrimary : true}, {infoType : 0, category : 0, categoryName : 'home', data : "+8999111999111", isPrimary : true}]
        /// ]]>
        /// </remarks>
        /// <category>Contacts</category>
        /// <exception cref="ArgumentException"></exception>
        /// <returns>
        /// Contact information
        /// </returns>
        /// <path>api/2.0/crm/contact/{contactid}/batch</path>
        /// <httpMethod>POST</httpMethod>
        /// <collection>list</collection>
        /// <visible>false</visible>
        [Create(@"contact/{contactid:[0-9]+}/batch")]
        public IEnumerable<ContactInfoWrapper> CreateBatchContactInfo(int contactid, IEnumerable<ContactInfoWrapper> items)
        {
            if (contactid <= 0) throw new ArgumentException();

            var contact = DaoFactory.ContactDao.GetByID(contactid);
            if (contact == null || !CRMSecurity.CanEdit(contact)) throw new ItemNotFoundException();

            var itemsList = items != null ? items.ToList() : new List<ContactInfoWrapper>();
            var contactInfoList = itemsList.Select(FromContactInfoWrapper).ToList();

            foreach (var contactInfo in contactInfoList)
            {
                if (contactInfo.InfoType == ContactInfoType.Address)
                {
                    Address res;
                    if (!Address.TryParse(contactInfo, out res))
                        throw new ArgumentException();
                }
                contactInfo.ContactID = contactid;
            }

            var ids = DaoFactory.ContactInfoDao.SaveList(contactInfoList, contact);

            for (var index = 0; index < itemsList.Count; index++)
            {
                var infoWrapper = itemsList[index];
                infoWrapper.ID = ids[index];
            }
            return itemsList;
        }

        /// <summary>
        /// Updates the contact information with the parameters specified in the request.
        /// </summary>
        ///<param type="System.Int32, System" method="url" name="id">Contact information record ID</param>
        ///<param type="System.Int32, System" method="url" name="contactid">Contact ID</param>
        ///<param type="System.Nullable{ASC.CRM.Core.ContactInfoType}, System" optional="true" name="infoType">New contact information type</param>
        ///<param type="System.String, System" name="data">New data</param>
        ///<param type="System.Nullable{System.Boolean}, System" optional="true" name="isPrimary">New contact information importance: primary or not</param>
        ///<param type="System.String, System" optional="true" name="category">New contact information category</param>
        ///<short>Update contact information</short> 
        ///<category>Contacts</category>
        ///<exception cref="ArgumentException"></exception>
        /// <returns type="ASC.Api.CRM.Wrappers.ContactInfoWrapper, ASC.Api.CRM">
        /// Updated contact information
        /// </returns>
        /// <path>api/2.0/crm/contact/{contactid}/data/{id}</path>
        /// <httpMethod>PUT</httpMethod>
        [Update(@"contact/{contactid:[0-9]+}/data/{id:[0-9]+}")]
        public ContactInfoWrapper UpdateContactInfo(int id, int contactid, ContactInfoType? infoType, string data, bool? isPrimary, string category)
        {
            if (id <= 0 || string.IsNullOrEmpty(data) || contactid <= 0) throw new ArgumentException();

            var contact = DaoFactory.ContactDao.GetByID(contactid);
            if (contact == null || !CRMSecurity.CanEdit(contact)) throw new ItemNotFoundException();

            var contactInfo = DaoFactory.ContactInfoDao.GetByID(id);

            if (infoType != null)
            {
                var categoryType = ContactInfo.GetCategory(infoType.Value);

                if (!string.IsNullOrEmpty(category) && Enum.IsDefined(categoryType, category))
                {
                    contactInfo.Category = (int)Enum.Parse(categoryType, category);
                }

                contactInfo.InfoType = infoType.Value;
            }

            contactInfo.ContactID = contactid;

            if (isPrimary != null)
            {
                contactInfo.IsPrimary = isPrimary.Value;
            }

            contactInfo.Data = data;

            if (contactInfo.InfoType == ContactInfoType.Address)
            {
                Address res;
                if (!Address.TryParse(contactInfo, out res))
                    throw new ArgumentException();
            }

            DaoFactory.ContactInfoDao.Update(contactInfo);

            var messageAction = contact is Company ? MessageAction.CompanyUpdatedPrincipalInfo : MessageAction.PersonUpdatedPrincipalInfo;
            MessageService.Send(Request, messageAction, MessageTarget.Create(contact.ID), contact.GetTitle());

            var contactInfoWrapper = ToContactInfoWrapper(contactInfo);
            return contactInfoWrapper;
        }

        /// <summary>
        /// Updates the contact address information with the parameter specified in the request.
        /// </summary>
        /// <param type="System.Int32, System" method="url" name="id">Contact information record ID</param>
        /// <param type="System.Int32, System" method="url" name="contactid">Contact ID</param>
        /// <param type="ASC.Api.CRM.Wrappers.Address, ASC.Api.CRM.Wrappers" name="address">New address data</param>
        /// <short>Update contact address information</short> 
        /// <category>Contacts</category>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <returns type="ASC.Api.CRM.Wrappers.ContactInfoWrapper, ASC.Api.CRM">
        /// Contact information with the updated address
        /// </returns>
        /// <path>api/2.0/crm/contact/{contactid}/addressdata/{id}</path>
        /// <httpMethod>PUT</httpMethod>
        [Update(@"contact/{contactid:[0-9]+}/addressdata/{id:[0-9]+}")]
        public ContactInfoWrapper UpdateContactInfoAddress(int id, int contactid, Address address)
        {
            if (id <= 0) throw new ArgumentException("Invalid value", "id");

            var contactInfo = DaoFactory.ContactInfoDao.GetByID(id);

            if (contactInfo == null || contactInfo.InfoType != ContactInfoType.Address) throw new ItemNotFoundException();

            if (contactid <= 0) throw new ArgumentException("Invalid value", "contactid");

            var contact = DaoFactory.ContactDao.GetByID(contactid);

            if (contact == null || !CRMSecurity.CanEdit(contact) || contactInfo.ContactID != contactid) throw new ItemNotFoundException();

            if (address == null) throw new ArgumentException("Value cannot be null", "address");

            if (!Enum.IsDefined(typeof(AddressCategory), address.Category)) throw new ArgumentException("Value does not fall within the expected range.", "address.Category");

            address.CategoryName = ((AddressCategory)address.Category).ToLocalizedString();

            var settings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                },
                Formatting = Formatting.Indented
            };

            contactInfo.IsPrimary = address.IsPrimary;
            contactInfo.Category = address.Category;
            contactInfo.Data = JsonConvert.SerializeObject(address, settings);

            DaoFactory.ContactInfoDao.Update(contactInfo);

            var messageAction = contact is Company ? MessageAction.CompanyUpdatedPrincipalInfo : MessageAction.PersonUpdatedPrincipalInfo;
            MessageService.Send(Request, messageAction, MessageTarget.Create(contact.ID), contact.GetTitle());

            return ToContactInfoWrapper(contactInfo);
        }

        /// <summary>
        /// Updates the contact information items with the parameters specified in the request for the contact with the selected ID.
        /// </summary>
        ///<short>Update contact information items</short> 
        ///<param type="System.Int32, System" name="contactid">Contact ID</param>
        ///<param type="System.Collections.Generic.IEnumerable{ASC.Api.CRM.Wrappers.ContactInfoWrapper}, System.Collections.Generic" name="items">New contact information</param>
        /// <![CDATA[
        ///  Items have the following format:
        ///  [{infoType : 1, category : 1, categoryName : 'work', data : "myemail@email.com", isPrimary : true}, {infoType : 0, category : 0, categoryName : 'home', data : "+8999111999111", isPrimary : true}]
        /// ]]>
        ///<category>Contacts</category>
        ///<exception cref="ArgumentException"></exception>
        /// <returns>
        /// Updated contact information
        /// </returns>
        /// <path>api/2.0/crm/contact/{contactid}/batch</path>
        /// <httpMethod>PUT</httpMethod>
        /// <collection>list</collection>
        /// <visible>false</visible>
        [Update(@"contact/{contactid:[0-9]+}/batch")]
        public IEnumerable<ContactInfoWrapper> UpdateBatchContactInfo(int contactid, IEnumerable<ContactInfoWrapper> items)
        {
            if (contactid <= 0) throw new ArgumentException();

            var contact = DaoFactory.ContactDao.GetByID(contactid);
            if (contact == null || !CRMSecurity.CanEdit(contact)) throw new ItemNotFoundException();

            var itemsList = items != null ? items.ToList() : new List<ContactInfoWrapper>();
            var contactInfoList = itemsList.Select(FromContactInfoWrapper).ToList();

            foreach (var contactInfo in contactInfoList)
            {
                if (contactInfo.InfoType == ContactInfoType.Address)
                {
                    Address res;
                    if (!Address.TryParse(contactInfo, out res))
                        throw new ArgumentException();
                }
                contactInfo.ContactID = contactid;
            }

            DaoFactory.ContactInfoDao.DeleteByContact(contactid);
            var ids = DaoFactory.ContactInfoDao.SaveList(contactInfoList, contact);

            for (var index = 0; index < itemsList.Count; index++)
            {
                var infoWrapper = itemsList[index];
                infoWrapper.ID = ids[index];
            }
            return itemsList;
        }

        /// <summary>
        /// Returns the detailed contact information by the information type specified in the request.
        /// </summary>
        /// <param type="System.Int32, System" method="url" name="contactid">Contact ID</param>
        /// <param type="ASC.CRM.Core.ContactInfoType, ASC.CRM.Core" method="url" name="infoType">Contact information type</param>
        /// <short>Get contact information by its type</short> 
        /// <category>Contacts</category>
        /// <returns>
        /// Contact information
        /// </returns>
        /// <path>api/2.0/crm/contact/{contactid}/data/{infoType}</path>
        /// <httpMethod>GET</httpMethod>
        /// <collection>list</collection>
        [Read(@"contact/{contactid:[0-9]+}/data/{infoType}")]
        public IEnumerable<string> GetContactInfo(int contactid, ContactInfoType infoType)
        {
            if (contactid <= 0) throw new ArgumentException();

            var contact = DaoFactory.ContactDao.GetByID(contactid);
            if (contact == null || !CRMSecurity.CanAccessTo(contact)) throw new ItemNotFoundException();

            return DaoFactory.ContactInfoDao.GetListData(contactid, infoType);
        }


        /// <summary>
        /// Deletes the selected information from the contact with the ID specified in the request.
        /// </summary>
        /// <param type="System.Int32, System" method="url" name="contactid">Contact ID</param>
        /// <param type="System.Int32, System" method="url" name="id">Contact information record ID</param>
        /// <short>Delete contact information</short> 
        /// <category>Contacts</category>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <returns type="ASC.Api.CRM.Wrappers.ContactInfoWrapper, ASC.Api.CRM">
        /// Contact information
        /// </returns>
        /// <path>api/2.0/crm/contact/{contactid}/data/{id}</path>
        /// <httpMethod>DELETE</httpMethod>
        [Delete(@"contact/{contactid:[0-9]+}/data/{id:[0-9]+}")]
        public ContactInfoWrapper DeleteContactInfo(int contactid, int id)
        {
            if (id <= 0 || contactid <= 0) throw new ArgumentException();

            var contact = DaoFactory.ContactDao.GetByID(contactid);
            if (contact == null || !CRMSecurity.CanEdit(contact)) throw new ItemNotFoundException();

            var contactInfo = DaoFactory.ContactInfoDao.GetByID(id);
            if (contactInfo == null) throw new ItemNotFoundException();

            var wrapper = ToContactInfoWrapper(contactInfo);

            DaoFactory.ContactInfoDao.Delete(id);

            var messageAction = contact is Company ? MessageAction.CompanyUpdatedPrincipalInfo : MessageAction.PersonUpdatedPrincipalInfo;
            MessageService.Send(Request, messageAction, MessageTarget.Create(contact.ID), contact.GetTitle());

            if (contactInfo.InfoType == ContactInfoType.Email)
            {
                FactoryIndexer<EmailWrapper>.DeleteAsync(EmailWrapper.ToEmailWrapper(contact, new List<ContactInfo> { contactInfo }));
            }
            FactoryIndexer<InfoWrapper>.DeleteAsync(contactInfo);

            return wrapper;
        }


        private static ContactInfoWrapper ToContactInfoWrapper(ContactInfo contactInfo)
        {
            return new ContactInfoWrapper(contactInfo);
        }

        private static ContactInfo FromContactInfoWrapper(ContactInfoWrapper contactInfoWrapper)
        {
            return new ContactInfo
            {
                ID = contactInfoWrapper.ID,
                Category = contactInfoWrapper.Category,
                Data = contactInfoWrapper.Data,
                InfoType = contactInfoWrapper.InfoType,
                IsPrimary = contactInfoWrapper.IsPrimary
            };
        }
    }
}