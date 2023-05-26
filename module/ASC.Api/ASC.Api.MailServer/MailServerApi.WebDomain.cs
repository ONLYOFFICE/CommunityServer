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

using ASC.Api.Attributes;
using ASC.Core;
using ASC.Mail.Core.Engine.Operations.Base;
using ASC.Mail.Data.Contracts;

using ASC.Web.Studio.PublicResources;

// ReSharper disable InconsistentNaming

namespace ASC.Api.MailServer
{
    ///<name>mailserver</name>
    public partial class MailServerApi
    {
        /// <summary>
        /// Returns a list of all the web domains associated with the current tenant.
        /// </summary>
        /// <returns type="ASC.Mail.Data.Contracts.ServerDomainData, ASC.Mail">List of web domains for the current tenant</returns>
        /// <short>Get web domains</short> 
        /// <category>Domains</category>
        /// <path>api/2.0/mailserver/domains/get</path>
        /// <httpMethod>GET</httpMethod>
        /// <collection>list</collection>
        [Read(@"domains/get")]
        public List<ServerDomainData> GetDomains()
        {
            if (!IsEnableMailServer) throw new Exception(Resource.ErrorNotAllowedOption);
            var listDomainData = MailEngineFactory.ServerDomainEngine.GetDomains();

            if (CoreContext.Configuration.Standalone)
            {
                //Skip common domain
                listDomainData = listDomainData.Where(d => !d.IsSharedDomain).ToList();
            }

            return listDomainData;
        }

        /// <summary>
        /// Returns the common web domain.
        /// </summary>
        /// <returns type="ASC.Mail.Data.Contracts.ServerDomainData, ASC.Mail">Common web domain</returns>
        /// <short>Get common web domain</short> 
        /// <category>Domains</category>
        /// <path>api/2.0/mailserver/domains/common</path>
        /// <httpMethod>GET</httpMethod>
        [Read(@"domains/common")]
        public ServerDomainData GetCommonDomain()
        {
            var commonDomain = MailEngineFactory.ServerDomainEngine.GetCommonDomain();
            return commonDomain;
        }

        /// <summary>
        /// Adds a web domain to the current tenant.
        /// </summary>
        /// <param type="System.String, System" name="name">Web domain name</param>
        /// <param type="System.Int32, System" name="id_dns">DNS ID</param>
        /// <returns type="ASC.Mail.Data.Contracts.ServerDomainData, ASC.Mail">Web domain data associated with the tenant</returns>
        /// <short>Add a domain to the mail server</short> 
        /// <category>Domains</category>
        /// <path>api/2.0/mailserver/domains/add</path>
        /// <httpMethod>POST</httpMethod>
        [Create(@"domains/add")]
        public ServerDomainData AddDomain(string name, int id_dns)
        {
            if (!IsEnableMailServer) throw new Exception(Resource.ErrorNotAllowedOption);
            var domain = MailEngineFactory.ServerDomainEngine.AddDomain(name, id_dns);
            return domain;
        }

        /// <summary>
        /// Deletes a web domain with the ID specified in the request from the mail server.
        /// </summary>
        /// <param type="System.Int32, System" method="url" name="id">Web domain ID</param>
        /// <returns type="ASC.Mail.Core.Engine.Operations.Base.MailOperationStatus, ASC.Mail">Operation status</returns>
        /// <short>Remove a domain from the mail server</short> 
        /// <category>Domains</category>
        /// <path>api/2.0/mailserver/domains/remove/{id}</path>
        /// <httpMethod>DELETE</httpMethod>
        [Delete(@"domains/remove/{id}")]
        public MailOperationStatus RemoveDomain(int id)
        {
            if (!IsEnableMailServer) throw new Exception(Resource.ErrorNotAllowedOption);
            var status = MailEngineFactory.ServerDomainEngine.RemoveDomain(id);
            return status;
        }

        /// <summary>
        /// Returns DNS records related to the domain with the ID specified in the request.
        /// </summary>
        /// <param type="System.Int32, System" method="url" name="id">Domain ID</param>
        /// <returns type="ASC.Mail.Data.Contracts.ServerDomainDnsData, ASC.Mail">DNS records associated with the domain</returns>
        /// <short>Get DNS records by domain ID</short>
        /// <category>Domains</category>
        /// <path>api/2.0/mailserver/domains/dns/get</path>
        /// <httpMethod>GET</httpMethod>
        [Read(@"domains/dns/get")]
        public ServerDomainDnsData GetDnsRecords(int id)
        {
            if (!IsEnableMailServer) throw new Exception(Resource.ErrorNotAllowedOption);
            var dns = MailEngineFactory.ServerDomainEngine.GetDnsData(id);
            return dns;
        }

        /// <summary>
        /// Checks if a web domain name specified in the request already exists or not.
        /// </summary>
        /// <param type="System.String, System" method="url" name="name">Web domain name</param>
        /// <returns>Boolean value: true - domain name exists, false - domain name does not exist</returns>
        /// <short>Check the domain name existence</short> 
        /// <category>Domains</category>
        /// <path>api/2.0/mailserver/domains/exists</path>
        /// <httpMethod>GET</httpMethod>
        [Read(@"domains/exists")]
        public bool IsDomainExists(string name)
        {
            if (!IsEnableMailServer) throw new Exception(Resource.ErrorNotAllowedOption);
            var isExists = MailEngineFactory.ServerDomainEngine.IsDomainExists(name);
            return isExists;
        }

        /// <summary>
        /// Checks if a web domain specified in the request belongs to the current user or not.
        /// </summary>
        /// <param type="System.String, System" method="url" name="name">Web domain name</param>
        /// <returns>Boolean value: true - current user is the domain owner, false - current user is not the domain owner</returns>
        /// <short>Check the domain ownership</short> 
        /// <category>Domains</category>
        /// <path>api/2.0/mailserver/domains/ownership/check</path>
        /// <httpMethod>GET</httpMethod>
        [Read(@"domains/ownership/check")]
        public bool CheckDomainOwnership(string name)
        {
            if (!IsEnableMailServer) throw new Exception(Resource.ErrorNotAllowedOption);
            var isOwnershipProven = MailEngineFactory.ServerEngine.CheckDomainOwnership(name);
            return isOwnershipProven;
        }
    }
}
