/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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
using ASC.Api.Attributes;
using ASC.Core;
using ASC.Mail.Aggregator;
using ASC.Mail.Aggregator.Dal.DbSchema;
using ASC.Web.Core;

namespace ASC.Api.Mail
{
    public partial class MailApi
    {
        /// <summary>
        ///    Returns the list of the contacts for auto complete feature.
        /// </summary>
        /// <param name="term">string part of contact name, lastname or email.</param>
        /// <returns>Strings list.  Strings format: "Name Lastname"</returns>
        /// <short>Get contact list for auto complete</short> 
        /// <category>Contacts</category>
        ///<exception cref="ArgumentException">Exception happens when in parameters is invalid. Text description contains parameter name and text description.</exception>
        [Read(@"contacts")]
        public IEnumerable<string> GetContacts(string term)
        {
            if (string.IsNullOrEmpty(term))
                throw new ArgumentException(@"term parameter empty.", "term");

            var equality = new ContactEqualityComparer();
            var contacts = MailBoxManager.SearchMailContacts(TenantId, Username, term).Distinct(equality).ToList();
            if (WebItemSecurity.IsAvailableForUser(WebItemManager.CRMProductID.ToString(), SecurityContext.CurrentAccount.ID))
                contacts = contacts.Concat(MailBoxManager.SearchCrmContacts(TenantId, Username, term)).Distinct(equality).ToList();
            if (WebItemSecurity.IsAvailableForUser(WebItemManager.PeopleProductID.ToString(), SecurityContext.CurrentAccount.ID))
                contacts = contacts.Concat(MailBoxManager.SearchTeamLabContacts(TenantId, term)).Distinct(equality).ToList();
            return contacts;
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
