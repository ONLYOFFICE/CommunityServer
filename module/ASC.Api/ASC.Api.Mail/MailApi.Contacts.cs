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
using System.Web;

using ASC.Api.Attributes;
using ASC.Mail;
using ASC.Mail.Core.Dao.Expressions.Contact;
using ASC.Mail.Core.Entities;
using ASC.Mail.Data.Contracts;
using ASC.Mail.Enums;
using ASC.Mail.Extensions;

// ReSharper disable InconsistentNaming

namespace ASC.Api.Mail
{
    public partial class MailApi
    {
        /// <summary>
        /// Searches for contacts by their names, last names, or emails.
        /// </summary>
        /// <param type="System.String, System" method="url" name="term">The string part of the contact name, last name, or email address</param>
        /// <returns>List of strings in the following format: "Name Last name" email</returns>
        /// <short>Search contacts</short> 
        /// <category>Contacts</category>
        /// <path>api/2.0/mail/emails/search</path>
        /// <httpMethod>GET</httpMethod>
        /// <collection>list</collection>
        /// <exception cref="ArgumentException">Exception happens when the parameters are invalid. Text description contains parameter name and text description.</exception>
        [Read(@"emails/search")]
        public IEnumerable<string> SearchEmails(string term)
        {
            if (string.IsNullOrEmpty(term))
                throw new ArgumentException(@"term parameter empty.", "term");

            var scheme = HttpContext.Current == null
                ? Uri.UriSchemeHttp
                : HttpContext.Current.Request.GetUrlRewriter().Scheme;

            return MailEngineFactory.ContactEngine.SearchEmails(TenantId, Username, term, MailAutocompleteMaxCountPerSystem,
                scheme, MailAutocompleteTimeout);
        }

        /// <summary>
        /// Returns a list of filtered mail contacts by the search query specified in the request.
        /// </summary>
        /// <param type="System.String, System" method="url" optional="true" name="search">Text to search in contact names or emails</param>
        /// <param type="System.Nullable{System.Int32}, System" method="url" optional="true" name="contactType">Contact type</param>
        /// <param type="System.Nullable{System.Int32}, System" method="url" optional="true" name="pageSize">Number of contacts on the page</param>
        /// <param type="System.Int32, System" method="url" optional="true" name="fromIndex">Page number</param>
        /// <param type="System.String, System" method="url" name="sortorder">Sort order by name. String parameter: "ascending" or "descending"</param>
        /// <returns type="ASC.Mail.Data.Contracts.MailContactData, ASC.Mail">List of filtered contacts</returns>
        /// <short>Get contacts by search query</short> 
        /// <category>Contacts</category>
        /// <path>api/2.0/mail/contacts</path>
        /// <httpMethod>GET</httpMethod>
        /// <collection>list</collection>
        [Read(@"contacts")]
        public IEnumerable<MailContactData> GetContacts(string search, int? contactType, int? pageSize, int fromIndex,
            string sortorder)
        {
            var exp = string.IsNullOrEmpty(search) && !contactType.HasValue
                ? new SimpleFilterContactsExp(TenantId, Username, sortorder == Defines.ASCENDING, fromIndex, pageSize)
                : new FullFilterContactsExp(TenantId, Username, search, contactType,
                    orderAsc: sortorder == Defines.ASCENDING,
                    startIndex: fromIndex, limit: pageSize);

            var contacts = MailEngineFactory.ContactEngine.GetContactCards(exp);

            int totalCount;

            if (contacts.Any() && contacts.Count() < pageSize)
            {
                totalCount = fromIndex + contacts.Count;
            }
            else
            {
                totalCount = MailEngineFactory.ContactEngine.GetContactCardsCount(exp);
            }

            _context.SetTotalCount(totalCount);

            return contacts.ToMailContactDataList();
        }

        /// <summary>
        /// Returns a list of mail contacts with the contact information specified in the request.
        /// </summary>
        /// <param type="ASC.Mail.Enums.ContactInfoType, ASC.Mail.Enums" method="url" optional="false" name="infoType">Information type</param>
        /// <param type="System.String, System" method="url" optional="false" name="data">Contact data</param>
        /// <param type="System.Nullable{System.Boolean}, System" method="url" optional="true" name="isPrimary">Contact importance: primary or not</param>
        /// <returns type="ASC.Mail.Data.Contracts.MailContactData, ASC.Mail">List of filtered contacts</returns>
        /// <short>Get contacts by contact information</short> 
        /// <category>Contacts</category>
        /// <path>api/2.0/mail/contacts/bycontactinfo</path>
        /// <httpMethod>GET</httpMethod>
        /// <collection>list</collection>
        [Read(@"contacts/bycontactinfo")]
        public IEnumerable<MailContactData> GetContactsByContactInfo(ContactInfoType infoType, String data, bool? isPrimary)
        {
            var exp = new FullFilterContactsExp(TenantId, Username, data, infoType: infoType, isPrimary: isPrimary);

            var contacts = MailEngineFactory.ContactEngine.GetContactCards(exp);

            return contacts.ToMailContactDataList();
        }

        /// <summary>
        /// Creates a mail contact with the parameters specified in the request.
        /// </summary>
        /// <param type="System.String, System" name="name">Contact name</param>
        /// <param type="System.String, System" name="description">Contact description</param>
        /// <param type="System.Collections.Generic.List{System.String}, System.Collections.Generic" name="emails">List of contact emails</param>
        /// <param type="System.Collections.Generic.List{System.String}, System.Collections.Generic" name="phoneNumbers">List of contact phone numbers</param>
        /// <returns type="ASC.Mail.Data.Contracts.MailContactData, ASC.Mail">Information about created contact</returns>
        /// <short>Create a mail contact</short>
        /// <category>Contacts</category>
        /// <path>api/2.0/mail/contact/add</path>
        /// <httpMethod>POST</httpMethod>
        [Create(@"contact/add")]
        public MailContactData CreateContact(string name, string description, List<string> emails, List<string> phoneNumbers)
        {
            if (!emails.Any())
                throw new ArgumentException(@"Invalid list of emails.", "emails");

            var contactCard = new ContactCard(0, TenantId, Username, name, description, ContactType.Personal, emails,
                phoneNumbers);

            var newContact = MailEngineFactory.ContactEngine.SaveContactCard(contactCard);

            return newContact.ToMailContactData();
        }

        /// <summary>
        /// Removes the mail contacts with the IDs specified in the request.
        /// </summary>
        /// <param type="System.Collections.Generic.List{System.Int32}, System.Collections.Generic" name="ids">List of mail contact IDs</param>
        /// <returns>List of removed mail contact IDs </returns>
        /// <short>Remove mail contacts</short> 
        /// <category>Contacts</category>
        /// <path>api/2.0/mail/contacts/remove</path>
        /// <httpMethod>PUT</httpMethod>
        /// <collection>list</collection>
        [Update(@"contacts/remove")]
        public IEnumerable<int> RemoveContacts(List<int> ids)
        {
            if (!ids.Any())
                throw new ArgumentException(@"Empty ids collection", "ids");

            MailEngineFactory.ContactEngine.RemoveContacts(ids);

            return ids;
        }

        /// <summary>
        /// Updates a mail contact with the ID specified in the request.
        /// </summary>
        /// <param type="System.Int32, System" name="id">Mail contact ID</param>
        /// <param type="System.String, System" name="name">New contact name</param>
        /// <param type="System.String, System" name="description">New contact description</param>
        /// <param type="System.Collections.Generic.List{System.String}, System.Collections.Generic" name="emails">New list of contact emails</param>
        /// <param type="System.Collections.Generic.List{System.String}, System.Collections.Generic" name="phoneNumbers">New list of contact phone numbers</param>
        /// <returns type="ASC.Mail.Data.Contracts.MailContactData, ASC.Mail">Information about updated contact</returns>
        /// <short>Update a mail contact</short>
        /// <category>Contacts</category>
        /// <path>api/2.0/mail/contact/update</path>
        /// <httpMethod>PUT</httpMethod>
        [Update(@"contact/update")]
        public MailContactData UpdateContact(int id, string name, string description, List<string> emails, List<string> phoneNumbers)
        {
            if (id < 0)
                throw new ArgumentException(@"Invalid contact id.", "id");

            if (!emails.Any())
                throw new ArgumentException(@"Invalid list of emails.", "emails");

            var contactCard = new ContactCard(id, TenantId, Username, name, description, ContactType.Personal, emails, phoneNumbers);

            var contact = MailEngineFactory.ContactEngine.UpdateContactCard(contactCard);

            return contact.ToMailContactData();
        }

        /// <summary>
        /// Returns a list of the CRM entities (contact, case, or opportunity) linked with a conversation.
        /// </summary>
        /// <param type="System.Int32, System" method="url" name="message_id">Message ID. It may be ID of any message included in the conversation</param>
        /// <returns type="ASC.Mail.Data.Contracts.CrmContactData, ASC.Mail">List of entity information: {entity_id, entity_type, avatar_link, title}</returns>
        /// <short>Get the linked CRM entities</short> 
        /// <category>Contacts</category>
        /// <path>api/2.0/mail/crm/linked/entities</path>
        /// <httpMethod>GET</httpMethod>
        /// <collection>list</collection>
        ///<exception cref="ArgumentException">An exception occurs when the parameters are invalid. The text description contains the parameter name and the text description.</exception>
        [Read(@"crm/linked/entities")]
        public IEnumerable<CrmContactData> GetLinkedCrmEntitiesInfo(int message_id)
        {
            if (message_id < 0)
                throw new ArgumentException(@"meesage_id must be positive integer", "message_id");

            return MailEngineFactory.CrmLinkEngine.GetLinkedCrmEntitiesId(message_id);
        }
    }
}
