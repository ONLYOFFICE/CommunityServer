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
using ASC.Api.Attributes;
using ASC.Api.MailServer.DataContracts;
using ASC.Api.MailServer.Extensions;
using ASC.Mail.Aggregator.Common;
using ASC.Mail.Server.Utils;
using System.Security;
using ASC.Web.Studio.Core;

namespace ASC.Api.MailServer
{
    public partial class MailServerApi
    {
        /// <summary>
        ///    Create group address
        /// </summary>
        /// <param name="name"></param>
        /// <param name="domain_id"></param>
        /// <param name="address_ids"></param>
        /// <returns>MailGroupData associated with tenant</returns>
        /// <short>Create mail group address</short>
        /// <category>MailGroup</category>
        [Create(@"groupaddress/add")]
        public MailGroupData CreateMailGroup(string name, int domain_id, List<int> address_ids)
        {
            if (!IsAdmin)
                throw new SecurityException("Need admin privileges.");

            if (string.IsNullOrEmpty(name))
                throw new ArgumentException(@"Invalid mailgroup name.", "name");

            if (domain_id < 0)
                throw new ArgumentException(@"Invalid domain id.", "domain_id");

            if (name.Length > 64)
                throw new ArgumentException(@"Local part of mailgroup exceed limitation of 64 characters.", "name");

            if (!Parser.IsEmailLocalPartValid(name))
                throw new ArgumentException(@"Incorrect group name.", "name");

            if (!address_ids.Any())
                throw new ArgumentException(@"Empty collection of address_ids.", "address_ids");

            var domain = MailServer.GetWebDomain(domain_id, MailServerFactory);

            if (domain.Tenant == Defines.SHARED_TENANT_ID)
                throw new InvalidOperationException("Creating mail group is not allowed for shared domain.");

            var mailgroupName = name.ToLowerInvariant();

            var mailgroup = MailServer.CreateMailGroup(mailgroupName, domain, address_ids, MailServerFactory);
            MailBoxManager.CachedAccounts.ClearAll();

            return mailgroup.ToMailGroupData();
        }

        /// <summary>
        ///    Add addresses to group
        /// </summary>
        /// <param name="mailgroup_id">id of group address</param>
        /// <param name="address_id"></param>
        /// <returns>MailGroupData associated with tenant</returns>
        /// <short>Add group's addresses</short> 
        /// <category>MailGroup</category>
        [Update(@"groupaddress/address/add")]
        public MailGroupData AddMailGroupAddress(int mailgroup_id, int address_id)
        {
            if (!IsAdmin)
                throw new SecurityException("Need admin privileges.");

            if (address_id < 0)
                throw new ArgumentException(@"Invalid address id.", "address_id");

            if (mailgroup_id < 0)
                throw new ArgumentException(@"Invalid mailgroup id.", "mailgroup_id");

            var mailgroup = MailServer.GetMailGroup(mailgroup_id, MailServerFactory);

            if (mailgroup == null)
                throw new ArgumentException("Mailgroup not exists");

            mailgroup.AddMember(address_id, MailServerFactory);
            MailBoxManager.CachedAccounts.ClearAll();

            return mailgroup.ToMailGroupData();
        }

        /// <summary>
        ///    Remove address from group
        /// </summary>
        /// <param name="mailgroup_id">id of group address</param>
        /// <param name="address_id"></param>
        /// <returns>id of group address</returns>
        /// <short>Remove group's address</short>
        /// <category>MailGroup</category>
        [Delete(@"groupaddress/addresses/remove")]
        public int RemoveMailGroupAddress(int mailgroup_id, int address_id)
        {
            if (!IsAdmin)
                throw new SecurityException("Need admin privileges.");

            if (address_id < 0)
                throw new ArgumentException(@"Invalid address id.", "address_id");

            if (mailgroup_id < 0)
                throw new ArgumentException(@"Invalid mailgroup id.", "mailgroup_id");
            
            var mailgroup = MailServer.GetMailGroup(mailgroup_id, MailServerFactory);

            if (mailgroup == null)
                throw new ArgumentException("Mailgroup not exists");

            mailgroup.RemoveMember(address_id);
            MailBoxManager.CachedAccounts.ClearAll();

            return address_id;
        }

        /// <summary>
        ///    Returns list of group addresses associated with tenant
        /// </summary>
        /// <returns>List of MailGroupData for current tenant</returns>
        /// <short>Get mail group list</short>
        /// <category>MailGroup</category>
        [Read(@"groupaddress/get")]
        public List<MailGroupData> GetMailGroups()
        {
            if (!IsAdmin)
                throw new SecurityException("Need admin privileges.");

            var groups = MailServer.GetMailGroups(MailServerFactory);

            return groups
                .Select(group => group.ToMailGroupData())
                .ToList();
        }

        /// <summary>
        ///    Deletes the selected group address
        /// </summary>
        /// <param name="id">id of group address</param>
        /// <returns>id of group address</returns>
        /// <short>Remove group address from mail server</short> 
        /// <category>MailGroup</category>
        [Delete(@"groupaddress/remove/{id}")]
        public int RemoveMailGroup(int id)
        {
            if (!IsAdmin)
                throw new SecurityException("Need admin privileges.");

            if (id < 0)
                throw new ArgumentException(@"Invalid mailgroup id.", "id");

            MailServer.DeleteMailGroup(id, MailServerFactory);
            MailBoxManager.CachedAccounts.ClearAll();

            return id;
        }
    }
}
