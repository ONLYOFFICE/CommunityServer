/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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


using System.Collections.Generic;
using ASC.Api.Attributes;
using ASC.Mail.Data.Contracts;

// ReSharper disable InconsistentNaming

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
        public ServerDomainGroupData CreateMailGroup(string name, int domain_id, List<int> address_ids)
        {
            var group = MailEngineFactory.ServerMailgroupEngine.CreateMailGroup(name, domain_id, address_ids);
            return group;
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
        public ServerDomainGroupData AddMailGroupAddress(int mailgroup_id, int address_id)
        {
            var group = MailEngineFactory.ServerMailgroupEngine.AddMailGroupMember(mailgroup_id, address_id);
            return group;
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
            MailEngineFactory.ServerMailgroupEngine.RemoveMailGroupMember(mailgroup_id, address_id);
            return address_id;
        }

        /// <summary>
        ///    Returns list of group addresses associated with tenant
        /// </summary>
        /// <returns>List of MailGroupData for current tenant</returns>
        /// <short>Get mail group list</short>
        /// <category>MailGroup</category>
        [Read(@"groupaddress/get")]
        public List<ServerDomainGroupData> GetMailGroups()
        {
            var groups = MailEngineFactory.ServerMailgroupEngine.GetMailGroups();
            return groups;
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
            MailEngineFactory.ServerMailgroupEngine.RemoveMailGroup(id);
            return id;
        }
    }
}
