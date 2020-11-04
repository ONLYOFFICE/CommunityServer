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
