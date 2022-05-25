/*
 *
 * (c) Copyright Ascensio System Limited 2010-2021
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
    public partial class CRMApi
    {
        /// <summary>
        /// Returns a list of all the available contact categories of the specified information type.
        /// </summary>
        /// <param name="infoType">Contact information type</param>
        /// <short>Get contact categories by information type</short> 
        /// <category>Contacts</category>
        /// <returns>
        /// List of contact categories
        /// </returns>
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
        [Read(@"contact/data/infoType")]
        public IEnumerable<string> GetContactInfoType()
        {
            return Enum.GetNames(typeof(ContactInfoType)).ToItemList();
        }

        /// <summary>
        /// Returns the detailed information on the contact with the ID specified in the request.
        /// </summary>
        /// <param name="contactid">Contact ID</param>
        /// <short>Get contact information</short> 
        /// <category>Contacts</category>
        /// <returns>
        /// Contact information
        /// </returns>
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
        /// <param name="contactid">Contact ID</param>
        /// <param name="id">Contact information ID</param>
        /// <short>Get contact information by ID</short> 
        /// <category>Contacts</category>
        /// <returns>Contact information</returns>
        ///<exception cref="ArgumentException"></exception>
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
        ///<param name="contactid">Contact ID</param>
        ///<param name="infoType">Contact information type</param>
        ///<param name="data">New data</param>
        ///<param name="isPrimary">Contact information importance: primary or not</param>
        ///<param name="category">Contact information category</param>
        ///<short>Add contact information</short> 
        ///<category>Contacts</category>
        /// <seealso cref="GetContactInfoType"/>
        /// <seealso cref="GetContactInfoCategory"/>
        /// <returns>
        /// Contact information
        /// </returns> 
        ///<exception cref="ArgumentException"></exception>
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
        /// <param name="contactid">Contact ID</param>
        /// <param name="address">Address data</param>
        /// <short>Add contact address information</short> 
        /// <category>Contacts</category>
        /// <seealso cref="GetContactInfoType"/>
        /// <seealso cref="GetContactInfoCategory"/>
        /// <returns>
        /// Contact information
        /// </returns> 
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
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
        ///  Creates contact information (add new information to the existing list) with the parameters specified in the request for the contact with the selected ID.
        /// </summary>
        ///<short>Add new contact information</short> 
        /// <param name="contactid">Contact ID</param>
        /// <param name="items">Contact information</param>
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
        ///<param name="id">Contact information record ID</param>
        ///<param name="contactid">Contact ID</param>
        ///<param optional="true" name="infoType">New contact information type</param>
        ///<param name="data">New data</param>
        ///<param optional="true" name="isPrimary">New contact information importance: primary or not</param>
        ///<param optional="true" name="category">New contact information category</param>
        ///<short>Update contact information</short> 
        ///<category>Contacts</category>
        ///<exception cref="ArgumentException"></exception>
        /// <returns>
        /// Updated contact information
        /// </returns>
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
        /// <param name="id">Contact information record ID</param>
        /// <param name="contactid">Contact ID</param>
        /// <param name="address">New address data</param>
        /// <short>Update contact address information</short> 
        /// <category>Contacts</category>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <returns>
        /// Contact information with the updated address
        /// </returns>
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
        ///  Updates contact information (delete the existing information and add a new list) with the parameters specified in the request for the contact with the selected ID.
        /// </summary>
        ///<short>Update contact information</short> 
        ///<param name="contactid">Contact ID</param>
        ///<param name="items">New contact information</param>
        /// <![CDATA[
        ///  Items have the following format:
        ///  [{infoType : 1, category : 1, categoryName : 'work', data : "myemail@email.com", isPrimary : true}, {infoType : 0, category : 0, categoryName : 'home', data : "+8999111999111", isPrimary : true}]
        /// ]]>
        ///<category>Contacts</category>
        ///<exception cref="ArgumentException"></exception>
        /// <returns>
        /// Updated contact information
        /// </returns>
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
        /// <param name="contactid">Contact ID</param>
        /// <param name="infoType">Contact information type</param>
        /// <short>Get contact information by type</short> 
        /// <category>Contacts</category>
        /// <returns>
        /// Contact information
        /// </returns>
        [Read(@"contact/{contactid:[0-9]+}/data/{infoType}")]
        public IEnumerable<string> GetContactInfo(int contactid, ContactInfoType infoType)
        {
            if (contactid <= 0) throw new ArgumentException();

            var contact = DaoFactory.ContactDao.GetByID(contactid);
            if (contact == null || !CRMSecurity.CanAccessTo(contact)) throw new ItemNotFoundException();

            return DaoFactory.ContactInfoDao.GetListData(contactid, infoType);
        }


        /// <summary>
        /// Deletes the selected information for the contact with the ID specified in the request.
        /// </summary>
        /// <param name="contactid">Contact ID</param>
        /// <param name="id">Contact information record ID</param>
        /// <short>Delete contact information</short> 
        /// <category>Contacts</category>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <returns>
        /// Contact information
        /// </returns>
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