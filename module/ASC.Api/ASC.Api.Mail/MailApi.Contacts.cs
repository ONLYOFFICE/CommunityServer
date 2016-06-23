/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ASC.Api.Attributes;
using ASC.Api.Mail.DataContracts;
using ASC.Api.Mail.Extensions;
using ASC.Mail.Aggregator;
using ASC.Mail.Aggregator.Common;
using ASC.Mail.Aggregator.DbSchema;
using ASC.Mail.Aggregator.Filter;

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

            var scheme = HttpContext.Current == null ? Uri.UriSchemeHttp : HttpContext.Current.Request.GetUrlRewriter().Scheme;
            return MailBoxManager.SearchEmails(TenantId, Username, term, MailAutocompleteMaxCountPerSystem, scheme, MailAutocompleteTimeout);
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
        public IEnumerable<MailContactData> GetContacts(string search, int? contactType, int? pageSize, int fromIndex, string sortorder)
        {
            var filter = new MailContactsFilter
            {
                SearchFilter = search,
                Type = contactType,
                StartIndex = fromIndex,
                Count = pageSize.GetValueOrDefault(25),
                SortOrder = sortorder
            };

            var contacts = MailBoxManager.GetMailContacts(
                TenantId,
                Username,
                filter);

            int totalCount;

            if (contacts.Any() && contacts.Count() < pageSize)
            {
                totalCount = fromIndex + contacts.Count();
            }
            else
            {
                totalCount = MailBoxManager.GetMailContactsCount(TenantId,
                Username,
                filter);
            }
            _context.SetTotalCount(totalCount);

            return contacts.ToContactData();
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
            var contacts = MailBoxManager.GetContactsByContactInfo(TenantId, Username, infoType, data, isPrimary);

            return contacts.ToContactData();
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

            var contact = MailBoxManager.SaveMailContact(TenantId, Username, name, description, emails, phoneNumbers, ContactType.Personal);

            return contact.ToContactData();
        }

        /// <summary>
        ///    Removes selected mail contacts
        /// </summary>
        /// <param name="ids">List of mail contact ids</param>
        /// <returns>List of removed mail contact ids </returns>
        /// <short>Remove mail contact </short> 
        /// <category>Contacts</category>
        [Update(@"contacts/remove")]
        public IEnumerable<int> RemoveContact(List<int> ids)
        {
            if (!ids.Any())
                throw new ArgumentException(@"Empty ids collection", "ids");

            MailBoxManager.DeleteMailContacts(TenantId, Username, ids);

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

            var contact = MailBoxManager.UpdateMailContact(TenantId, Username, id, name, description, emails, phoneNumbers, ContactType.Personal);

            return contact.ToContactData();
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
        public IEnumerable<CrmContactEntity> GetLinkedCrmEntitiesInfo(int message_id)
        {
            if(message_id < 0)
                throw new ArgumentException(@"meesage_id must be positive integer", "message_id");

            return MailBoxManager.GetLinkedCrmEntitiesId(message_id, TenantId, Username);
        }
    }
}
