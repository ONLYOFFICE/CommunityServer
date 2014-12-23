/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using System;
using System.Collections.Generic;
using System.Linq;
using ASC.Api.Attributes;
using ASC.Api.MailServer.DataContracts;
using ASC.Api.MailServer.Extensions;
using ASC.Mail.Server.Utils;

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
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Invalid mailgroup name.", "name");

            if (domain_id < 0)
                throw new ArgumentException("Invalid domain id.", "domain_id");

            if (name.Length > 64)
                throw new ArgumentException("Local part of mailgroup exceed limitation of 64 characters.", "name");

            if (!Parser.IsEmailLocalPartValid(name))
                throw new ArgumentException("Incorrect group name.", "name");

            if (!address_ids.Any())
                throw new ArgumentException("Empty collection of address_ids.", "address_ids");

            var domain = MailServer.GetWebDomain(domain_id, MailServerFactory);

            var mailgroup_name = name.ToLowerInvariant();

            var mailgroup = MailServer.CreateMailGroup(mailgroup_name, domain, address_ids, MailServerFactory);

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
            if (address_id < 0)
                throw new ArgumentException("Invalid address id.", "address_id");

            if (mailgroup_id < 0)
                throw new ArgumentException("Invalid mailgroup id.", "mailgroup_id");
            
            var mailgroup = MailServer.GetMailGroup(mailgroup_id, MailServerFactory);

            mailgroup.AddMember(address_id, MailServerFactory);

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
            if (address_id < 0)
                throw new ArgumentException("Invalid address id.", "address_id");

            if (mailgroup_id < 0)
                throw new ArgumentException("Invalid mailgroup id.", "mailgroup_id");
            
            var mailgroup = MailServer.GetMailGroup(mailgroup_id, MailServerFactory);

            mailgroup.RemoveMember(address_id);

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
            if (id < 0)
                throw new ArgumentException("Invalid mailgroup id.", "id");

            MailServer.DeleteMailGroup(id, MailServerFactory);

            return id;
        }
    }
}
