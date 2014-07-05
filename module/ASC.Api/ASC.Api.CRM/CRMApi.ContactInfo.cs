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
using System.Collections.Generic;
using System.Linq;
using ASC.Api.Attributes;
using ASC.Api.CRM.Wrappers;
using ASC.Api.Collections;
using ASC.Api.Exceptions;
using ASC.CRM.Core;
using ASC.CRM.Core.Entities;
using ASC.MessagingSystem;
using Newtonsoft.Json;

namespace ASC.Api.CRM
{
    public partial class CRMApi
    {
        /// <summary>
        ///   Returns the list of all available contact categories
        /// </summary>
        /// <param name="infoType">
        ///    Contact information type
        /// </param>
        /// <short>Get all categories</short> 
        /// <category>Contacts</category>
        /// <returns>
        ///   List of all available contact categories
        /// </returns>
        [Read(@"contact/data/{infoType}/category")]
        public IEnumerable<string> GetContactInfoCategory(ContactInfoType infoType)
        {
            return Enum.GetNames(ContactInfo.GetCategory(infoType)).ToItemList();
        }

        /// <summary>
        ///   Returns the list of all available contact information types
        /// </summary>
        /// <short>Get all contact info types</short> 
        /// <category>Contacts</category>
        /// <returns></returns>
        [Read(@"contact/data/infoType")]
        public IEnumerable<string> GetContactInfoType()
        {
            return Enum.GetNames(typeof(ContactInfoType)).ToItemList();
        }

        /// <summary>
        ///    Returns the detailed information for the contact
        /// </summary>
        /// <param name="contactid">Contact ID</param>
        /// <short>Get contact information</short> 
        /// <category>Contacts</category>
        /// <returns>
        ///   Contact information
        /// </returns>
        [Read(@"contact/{contactid:[0-9]+}/data")]
        public IEnumerable<ContactInfoWrapper> GetContactInfo(int contactid)
        {
            if (contactid <= 0) throw new ArgumentException();

            var contact = DaoFactory.GetContactDao().GetByID(contactid);
            if (contact == null || !CRMSecurity.CanAccessTo(contact)) throw new ItemNotFoundException();

            return DaoFactory.GetContactInfoDao().GetList(contactid, null, null, null)
                .OrderByDescending(info => info.ID)
                .ToList()
                .ConvertAll(ToContactInfoWrapper);
        }

        /// <summary>
        ///   Returns the detailed list of all information available for the contact with the ID specified in the request
        /// </summary>
        /// <param name="contactid">Contact ID</param>
        /// <param name="id">Contact information ID</param>
        /// <short>Get contact info</short> 
        /// <category>Contacts</category>
        /// <returns>Contact information</returns>
        ///<exception cref="ArgumentException"></exception>
        [Read(@"contact/{contactid:[0-9]+}/data/{id:[0-9]+}")]
        public ContactInfoWrapper GetContactInfoByID(int contactid, int id)
        {
            if (contactid <= 0 || id <= 0) throw new ArgumentException();

            var contact = DaoFactory.GetContactDao().GetByID(contactid);
            if (contact == null || !CRMSecurity.CanAccessTo(contact)) throw new ItemNotFoundException();

            var contactInfo = DaoFactory.GetContactInfoDao().GetByID(id);

            if (contactInfo.ContactID != contactid) throw new ArgumentException();

            return ToContactInfoWrapper(contactInfo);
        }

        /// <summary>
        ///    Adds the information with the parameters specified in the request to the contact with the selected ID
        /// </summary>
        ///<param name="contactid">Contact ID</param>
        ///<param name="infoType">Contact information type</param>
        ///<param name="data">Data</param>
        ///<param name="isPrimary">Contact importance: primary or not</param>
        ///<param   name="category">Category</param>
        ///<short> Add contact info</short> 
        ///<category>Contacts</category>
        /// <seealso cref="GetContactInfoType"/>
        /// <seealso cref="GetContactInfoCategory"/>
        /// <returns>
        ///    Contact information
        /// </returns> 
        ///<exception cref="ArgumentException"></exception>
        [Create(@"contact/{contactid:[0-9]+}/data")]
        public ContactInfoWrapper CreateContactInfo(int contactid, ContactInfoType infoType, string data, bool isPrimary, string category)
        {
            if (string.IsNullOrEmpty(data) || contactid <= 0) throw new ArgumentException();
            var contact = DaoFactory.GetContactDao().GetByID(contactid);
            if (contact == null || !CRMSecurity.CanEdit(contact)) throw new ItemNotFoundException();

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

            var contactInfoID = DaoFactory.GetContactInfoDao().Save(contactInfo);

            if (contactInfo.InfoType == ContactInfoType.Email)
            {
                var userIds = CRMSecurity.GetAccessSubjectTo(contact).Keys.ToList();
                var emails = new[] {contactInfo.Data};
                DaoFactory.GetContactInfoDao().UpdateMailAggregator(emails, userIds);
            }

            var messageAction = contact is Company ? MessageAction.CompanyUpdatedPrincipalInfo : MessageAction.PersonUpdatedPrincipalInfo;
            MessageService.Send(_context, messageAction, contact.GetTitle());

            var contactInfoWrapper = ToContactInfoWrapper(contactInfo);
            contactInfoWrapper.ID = contactInfoID;
            return contactInfoWrapper;
        }

        /// <summary>
        ///  Creates contact information with the parameters specified in the request for the contact with the selected ID
        /// </summary>
        ///<short>Group contact info</short> 
        /// <param name="contactid">Contact ID</param>
        /// <param name="items">Contact information</param>
        /// <category>Contacts</category>
        /// <exception cref="ArgumentException"></exception>
        /// <returns>
        ///   Contact information
        /// </returns>
        /// <visible>false</visible>
        [Create(@"contact/{contactid:[0-9]+}/batch")]
        public IEnumerable<ContactInfoWrapper> CreateBatchContactInfo(int contactid, IEnumerable<ContactInfoWrapper> items)
        {
            if (contactid <= 0) throw new ArgumentException();

            var contact = DaoFactory.GetContactDao().GetByID(contactid);
            if (contact == null || !CRMSecurity.CanEdit(contact)) throw new ItemNotFoundException();

            var itemsList = items.ToList();
            var contactInfoList = itemsList.Select(FromContactInfoWrapper).ToList();

            foreach (var contactInfo in contactInfoList)
            {
                contactInfo.ContactID = contactid;
            }

            var ids = DaoFactory.GetContactInfoDao().SaveList(contactInfoList);

            for (var index = 0; index < itemsList.Count; index++)
            {
                var infoWrapper = itemsList[index];
                infoWrapper.ID = ids[index];
            }
            return itemsList;
        }

        /// <summary>
        ///   Updates the information with the parameters specified in the request for the contact with the selected ID
        /// </summary>
        ///<param name="id">Contact information record ID</param>
        ///<param name="contactid">Contact ID</param>
        ///<param optional="true" name="infoType">Contact information type</param>
        ///<param name="data">Data</param>
        ///<param optional="true" name="isPrimary">Contact importance: primary or not</param>
        ///<param optional="true" name="category">Contact information category</param>
        ///<short>Update contact info</short> 
        ///<category>Contacts</category>
        ///<exception cref="ArgumentException"></exception>
        /// <returns>
        ///   Contact information
        /// </returns>
        [Update(@"contact/{contactid:[0-9]+}/data/{id:[0-9]+}")]
        public ContactInfoWrapper UpdateContactInfo(int id, int contactid, ContactInfoType? infoType, string data, bool? isPrimary, string category)
        {
            if (id <= 0 || string.IsNullOrEmpty(data) || contactid <= 0) throw new ArgumentException();

            var contact = DaoFactory.GetContactDao().GetByID(contactid);
            if (contact == null || !CRMSecurity.CanEdit(contact)) throw new ItemNotFoundException();

            var contactInfo = DaoFactory.GetContactInfoDao().GetByID(id);

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

            DaoFactory.GetContactInfoDao().Update(contactInfo);

            if (contactInfo.InfoType == ContactInfoType.Email)
            {
                var userIds = CRMSecurity.GetAccessSubjectTo(contact).Keys.ToList();
                var emails = new[] {contactInfo.Data};
                DaoFactory.GetContactInfoDao().UpdateMailAggregator(emails, userIds);
            }

            var messageAction = contact is Company ? MessageAction.CompanyUpdatedPrincipalInfo : MessageAction.PersonUpdatedPrincipalInfo;
            MessageService.Send(_context, messageAction, contact.GetTitle());

            var contactInfoWrapper = ToContactInfoWrapper(contactInfo);
            return contactInfoWrapper;
        }


        /// <summary>
        ///  Updates contact information with the parameters specified in the request for the contact with the selected ID
        /// </summary>
        ///<short>Group contact info update</short> 
        ///<param name="contactid">Contact ID</param>
        ///<param name="items">Contact information</param>
        ///<category>Contacts</category>
        ///<exception cref="ArgumentException"></exception>
        /// <returns>
        ///   Contact information
        /// </returns>
        /// <visible>false</visible>
        [Update(@"contact/{contactid:[0-9]+}/batch")]
        public IEnumerable<ContactInfoWrapper> UpdateBatchContactInfo(int contactid, IEnumerable<ContactInfoWrapper> items)
        {
            if (contactid <= 0) throw new ArgumentException();

            var contact = DaoFactory.GetContactDao().GetByID(contactid);
            if (contact == null || !CRMSecurity.CanEdit(contact)) throw new ItemNotFoundException();

            items = items ?? new List<ContactInfoWrapper>();
            var itemsList = items.ToList();
            var contactInfoList = itemsList.Select(FromContactInfoWrapper).ToList();

            foreach (var contactInfo in contactInfoList)
            {
                contactInfo.ContactID = contactid;
            }

            DaoFactory.GetContactInfoDao().DeleteByContact(contactid);
            var ids = DaoFactory.GetContactInfoDao().SaveList(contactInfoList);

            for (var index = 0; index < itemsList.Count; index++)
            {
                var infoWrapper = itemsList[index];
                infoWrapper.ID = ids[index];
            }
            return itemsList;
        }

        /// <summary>
        ///    Returns the detailed information for the contact with the selected ID by the information type specified in the request
        /// </summary>
        /// <param name="contactid">Contact ID</param>
        /// <param name="infoType">Contact information type</param>
        /// <short>Get contact information by type</short> 
        /// <category>Contacts</category>
        /// <returns>
        ///   Contact information
        /// </returns>
        [Read(@"contact/{contactid:[0-9]+}/data/{infoType}")]
        public IEnumerable<string> GetContactInfo(int contactid, ContactInfoType infoType)
        {
            if (contactid <= 0) throw new ArgumentException();

            var contact = DaoFactory.GetContactDao().GetByID(contactid);
            if (contact == null || !CRMSecurity.CanAccessTo(contact)) throw new ItemNotFoundException();

            return DaoFactory.GetContactInfoDao().GetListData(contactid, infoType);
        }

        /// <summary>
        ///   Adds the address with the parameters specified in the request to the selected contact
        /// </summary>
        /// <param name="contactid">Contact ID</param>
        /// <param name="address">Address</param>
        /// <param name="isPrimary">Address type: primary or not</param>
        /// <param name="category">Category</param>
        ///<short>Add contact address</short> 
        ///<category>Contacts</category>
        /// <returns>
        ///   Address
        /// </returns>
        [Create(@"contact/{contactid:[0-9]+}/data/address/{category}")]
        public int AddAddress(int contactid, Address address, bool isPrimary, string category)
        {
            return CreateContactInfo(contactid, ContactInfoType.Address, JsonConvert.SerializeObject(address), isPrimary, category).ID;
        }

        /// <summary>
        ///   Updates the address with the parameters specified in the request for the selected contact
        /// </summary>
        ///<param name="id">Contact information record ID</param>
        /// <param name="contactid">Contact ID</param>
        /// <param name="address">Address</param>
        /// <param name="isPrimary">Address type: primary or not</param>
        /// <param name="category">Category</param>
        /// <short>Update contact address</short> 
        /// <category>Contacts</category>
        /// <returns>
        ///   Address
        /// </returns>
        [Update(@"contact/{contactid:[0-9]+}/data/address/{id:[0-9]+}")]
        public Address UpdateAddress(int id, int contactid, Address address, bool isPrimary, string category)
        {
            UpdateContactInfo(id, contactid, ContactInfoType.Address, JsonConvert.SerializeObject(address), isPrimary, category);
            return address;
        }

        /// <summary>
        ///   Deletes the address for the contact with the ID specified in the request
        /// </summary>
        /// <param name="contactid">Contact ID</param>
        /// <param name="id">Contact information record ID</param>
        /// <short>Delete contact address</short> 
        /// <category>Contacts</category>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <returns>
        ///   Address
        /// </returns>
        [Delete(@"contact/{contactid:[0-9]+}/data/address/{id:[0-9]+}")]
        public Address DeleteAddress(int contactid, int id)
        {
            if (id <= 0 || contactid <= 0) throw new ArgumentException();

            var contact = DaoFactory.GetContactDao().GetByID(contactid);
            if (contact == null || !CRMSecurity.CanEdit(contact)) throw new ItemNotFoundException();

            var contactInfo = DaoFactory.GetContactInfoDao().GetByID(id);
            if (contactInfo == null) throw new ItemNotFoundException();

            if (contactInfo.InfoType != ContactInfoType.Address) throw new ArgumentException();

            DeleteContactInfo(contactid, id);

            return new Address(contactInfo);
        }

        /// <summary>
        ///   Deletes the contact information for the contact with the ID specified in the request
        /// </summary>
        /// <param name="contactid">Contact ID</param>
        /// <param name="id">Contact information record ID</param>
        /// <short>Delete contact info</short> 
        /// <category>Contacts</category>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <returns>
        ///   Contact information
        /// </returns>
        [Delete(@"contact/{contactid:[0-9]+}/data/{id:[0-9]+}")]
        public ContactInfoWrapper DeleteContactInfo(int contactid, int id)
        {
            if (id <= 0 || contactid <= 0) throw new ArgumentException();

            var contact = DaoFactory.GetContactDao().GetByID(contactid);
            if (contact == null || !CRMSecurity.CanEdit(contact)) throw new ItemNotFoundException();

            var contactInfo = DaoFactory.GetContactInfoDao().GetByID(id);
            if (contactInfo == null) throw new ItemNotFoundException();

            var wrapper = ToContactInfoWrapper(contactInfo);

            DaoFactory.GetContactInfoDao().Delete(id);

            var messageAction = contact is Company ? MessageAction.CompanyUpdatedPrincipalInfo : MessageAction.PersonUpdatedPrincipalInfo;
            MessageService.Send(_context, messageAction, contact.GetTitle());

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