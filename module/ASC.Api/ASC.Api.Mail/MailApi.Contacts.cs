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
        ///    Returns the list of the contacts for auto complete feature.
        /// </summary>
        /// <param name="term">string part of contact name, lastname or email.</param>
        /// <returns>Strings list.  Strings format: "Name Lastname" email</returns>
        /// <short>Get contact list for auto complete</short> 
        /// <category>Contacts</category>
        /// <exception cref="ArgumentException">Exception happens when in parameters is invalid. Text description contains parameter name and text description.</exception>
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
        ///    Returns lists of mail contacts.
        /// </summary>
        /// <param optional="true" name="search">Text to search in contacts name or emails.</param>
        /// <param optional="true" name="contactType">Type of contacts</param>
        /// <param optional="true" name="pageSize">Count of contacts on page</param>
        /// <param optional="true" name="fromIndex">Page number</param> 
        /// <param name="sortorder">Sort order by name. String parameter: "ascending" - ascended, "descending" - descended.</param> 
        /// <returns>List of filtered contacts</returns>
        /// <short>Gets filtered contacts</short> 
        /// <category>Contacts</category>
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
        ///   Returns lists of mail contacts with contact information
        /// </summary>
        /// <param optional="false" name="infoType">infoType</param>
        /// <param optional="false" name="data">data</param>
        /// <param optional="true" name="isPrimary">isPrimary</param>
        /// <returns>List of filtered contacts</returns>
        /// <short>Gets filtered contacts</short> 
        /// <category>Contacts</category>
        [Read(@"contacts/bycontactinfo")]
        public IEnumerable<MailContactData> GetContactsByContactInfo(ContactInfoType infoType, String data, bool? isPrimary)
        {
            var exp = new FullFilterContactsExp(TenantId, Username, data, infoType: infoType, isPrimary: isPrimary);

            var contacts = MailEngineFactory.ContactEngine.GetContactCards(exp);

            return contacts.ToMailContactDataList();
        }

        /// <summary>
        ///    Create mail contact
        /// </summary>
        /// <param name="name">Contact's name</param>
        /// <param name="description">Description of contact</param>
        /// <param name="emails">List of emails</param>
        /// <param name="phoneNumbers">List of phone numbers</param>
        /// <returns>Information about created contact </returns>
        /// <short>Create mail contact</short>
        /// <category>Contacts</category>
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
        ///    Removes selected mail contacts
        /// </summary>
        /// <param name="ids">List of mail contact ids</param>
        /// <returns>List of removed mail contact ids </returns>
        /// <short>Remove mail contact </short> 
        /// <category>Contacts</category>
        [Update(@"contacts/remove")]
        public IEnumerable<int> RemoveContacts(List<int> ids)
        {
            if (!ids.Any())
                throw new ArgumentException(@"Empty ids collection", "ids");

            MailEngineFactory.ContactEngine.RemoveContacts(ids);

            return ids;
        }

        /// <summary>
        ///    Updates the existing mail contact
        /// </summary>
        /// <param name="id">id of mail contact</param>
        /// <param name="name">Contact's name</param>
        /// <param name="description">Description of contact</param>
        /// <param name="emails">List of emails</param>
        /// <param name="phoneNumbers">List of phone numbers</param>
        /// <returns>Information about updated contact </returns>
        /// <short>Update mail contact</short>
        /// <category>Contacts</category>
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
        ///    Returns list of crm entities linked with chain. Entity: contact, case or opportunity.
        /// </summary>
        /// <param name="message_id">Id of message included in the chain. It may be id any of messages included in the chain.</param>
        /// <returns>List of structures: {entity_id, entity_type, avatar_link, title}</returns>
        /// <short>Get crm linked entities</short> 
        /// <category>Contacts</category>
        ///<exception cref="ArgumentException">Exception happens when in parameters is invalid. Text description contains parameter name and text description.</exception>
        [Read(@"crm/linked/entities")]
        public IEnumerable<CrmContactData> GetLinkedCrmEntitiesInfo(int message_id)
        {
            if(message_id < 0)
                throw new ArgumentException(@"meesage_id must be positive integer", "message_id");

            return MailEngineFactory.CrmLinkEngine.GetLinkedCrmEntitiesId(message_id);
        }
    }
}
