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

using ASC.Api.Attributes;
using ASC.Mail.Data.Contracts;

using ASC.Web.Studio.PublicResources;

// ReSharper disable InconsistentNaming

namespace ASC.Api.MailServer
{
    public partial class MailServerApi
    {
        /// <summary>
        /// Creates a mail group with the parameters specified in the request.
        /// </summary>
        /// <param type="System.String, System" name="name">Sender name</param>
        /// <param type="System.Int32, System" name="domain_id">Domain ID</param>
        /// <param type="System.Collections.Generic.List{System.Int32}, System.Collections.Generic" name="address_ids">List of address IDs</param>
        /// <returns type="ASC.Mail.Data.Contracts.ServerDomainGroupData, ASC.Mail">Mail group data associated with the tenant</returns>
        /// <short>Create a mail group</short>
        /// <category>Mail groups</category>
        /// <path>api/2.0/mailserver/groupaddress/add</path>
        /// <httpMethod>POST</httpMethod>
        [Create(@"groupaddress/add")]
        public ServerDomainGroupData CreateMailGroup(string name, int domain_id, List<int> address_ids)
        {
            if (!IsEnableMailServer) throw new Exception(Resource.ErrorNotAllowedOption);
            var group = MailEngineFactory.ServerMailgroupEngine.CreateMailGroup(name, domain_id, address_ids);
            return group;
        }

        /// <summary>
        /// Adds an address with the ID specified in the request to the mail group.
        /// </summary>
        /// <param type="System.Int32, System" name="mailgroup_id">Mail group ID</param>
        /// <param type="System.Int32, System" name="address_id">Address ID</param>
        /// <returns type="ASC.Mail.Data.Contracts.ServerDomainGroupData, ASC.Mail">Mail group data associated with the tenant</returns>
        /// <short>Add an address to the mail group</short> 
        /// <category>Mail groups</category>
        /// <path>api/2.0/mailserver/groupaddress/address/add</path>
        /// <httpMethod>PUT</httpMethod>
        [Update(@"groupaddress/address/add")]
        public ServerDomainGroupData AddMailGroupAddress(int mailgroup_id, int address_id)
        {
            if (!IsEnableMailServer) throw new Exception(Resource.ErrorNotAllowedOption);
            var group = MailEngineFactory.ServerMailgroupEngine.AddMailGroupMember(mailgroup_id, address_id);
            return group;
        }

        /// <summary>
        /// Removes an address with the ID specified in the request from the mail group.
        /// </summary>
        /// <param type="System.Int32, System" name="mailgroup_id">Mail group ID</param>
        /// <param type="System.Int32, System" name="address_id">Address ID</param>
        /// <returns>Mail group ID</returns>
        /// <short>Remove an address from the mail group</short>
        /// <category>Mail groups</category>
        /// <path>api/2.0/mailserver/groupaddress/addresses/remove</path>
        /// <httpMethod>DELETE</httpMethod>
        [Delete(@"groupaddress/addresses/remove")]
        public int RemoveMailGroupAddress(int mailgroup_id, int address_id)
        {
            if (!IsEnableMailServer) throw new Exception(Resource.ErrorNotAllowedOption);
            MailEngineFactory.ServerMailgroupEngine.RemoveMailGroupMember(mailgroup_id, address_id);
            return address_id;
        }

        /// <summary>
        /// Returns a list of mail groups associated with the tenant.
        /// </summary>
        /// <returns type="ASC.Mail.Data.Contracts.ServerDomainGroupData, ASC.Mail">List of mail group data for the current tenant</returns>
        /// <short>Get mail groups</short>
        /// <category>Mail groups</category>
        /// <path>api/2.0/mailserver/groupaddress/get</path>
        /// <httpMethod>GET</httpMethod>
        /// <collection>list</collection>
        [Read(@"groupaddress/get")]
        public List<ServerDomainGroupData> GetMailGroups()
        {
            if (!IsEnableMailServer) throw new Exception(Resource.ErrorNotAllowedOption);
            var groups = MailEngineFactory.ServerMailgroupEngine.GetMailGroups();
            return groups;
        }

        /// <summary>
        /// Deletes a mail group with the ID specified in the request.
        /// </summary>
        /// <param type="System.Int32, System" method="url" name="id">Mail group ID</param>
        /// <returns>Mail group ID</returns>
        /// <short>Remove a mail group</short> 
        /// <category>Mail groups</category>
        /// <path>api/2.0/mailserver/groupaddress/remove/{id}</path>
        /// <httpMethod>DELETE</httpMethod>
        [Delete(@"groupaddress/remove/{id}")]
        public int RemoveMailGroup(int id)
        {
            if (!IsEnableMailServer) throw new Exception(Resource.ErrorNotAllowedOption);
            MailEngineFactory.ServerMailgroupEngine.RemoveMailGroup(id);
            return id;
        }
    }
}
