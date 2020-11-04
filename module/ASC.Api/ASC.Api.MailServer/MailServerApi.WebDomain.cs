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
using System.Linq;
using ASC.Api.Attributes;
using ASC.Core;
using ASC.Mail.Core.Engine.Operations.Base;
using ASC.Mail.Data.Contracts;

// ReSharper disable InconsistentNaming

namespace ASC.Api.MailServer
{
    public partial class MailServerApi
    {
        /// <summary>
        ///    Returns list of the web domains associated with tenant
        /// </summary>
        /// <returns>List of WebDomainData for current tenant</returns>
        /// <short>Get tenant web domain list</short> 
        /// <category>Domains</category>
        [Read(@"domains/get")]
        public List<ServerDomainData> GetDomains()
        {
            var listDomainData = MailEngineFactory.ServerDomainEngine.GetDomains();

            if (CoreContext.Configuration.Standalone)
            {
                //Skip common domain
               listDomainData = listDomainData.Where(d => !d.IsSharedDomain).ToList();
            }

            return listDomainData;
        }

        /// <summary>
        ///    Returns the common web domain
        /// </summary>
        /// <returns>WebDomainData for common web domain</returns>
        /// <short>Get common web domain</short> 
        /// <category>Domains</category>
        [Read(@"domains/common")]
        public ServerDomainData GetCommonDomain()
        {
            var commonDomain = MailEngineFactory.ServerDomainEngine.GetCommonDomain();
            return commonDomain;
        }

        /// <summary>
        ///    Associate a web domain with tenant
        /// </summary>
        /// <param name="name">web domain name</param>
        /// <param name="id_dns"></param>
        /// <returns>WebDomainData associated with tenant</returns>
        /// <short>Add domain to mail server</short> 
        /// <category>Domains</category>
        [Create(@"domains/add")]
        public ServerDomainData AddDomain(string name, int id_dns)
        {
            var domain = MailEngineFactory.ServerDomainEngine.AddDomain(name, id_dns);
            return domain;
        }

        /// <summary>
        ///    Deletes the selected web domain
        /// </summary>
        /// <param name="id">id of web domain</param>
        /// <returns>MailOperationResult object</returns>
        /// <short>Remove domain from mail server</short> 
        /// <category>Domains</category>
        [Delete(@"domains/remove/{id}")]
        public MailOperationStatus RemoveDomain(int id)
        {
            var status = MailEngineFactory.ServerDomainEngine.RemoveDomain(id);
            return status;
        }

        /// <summary>
        ///    Returns dns records associated with domain
        /// </summary>
        /// <param name="id">id of domain</param>
        /// <returns>Dns records associated with domain</returns>
        /// <short>Returns dns records</short>
        /// <category>DnsRecords</category>
        [Read(@"domains/dns/get")]
        public ServerDomainDnsData GetDnsRecords(int id)
        {
            var dns = MailEngineFactory.ServerDomainEngine.GetDnsData(id);
            return dns;
        }

        /// <summary>
        ///    Check web domain name existance
        /// </summary>
        /// <param name="name">web domain name</param>
        /// <returns>True if domain name already exists.</returns>
        /// <short>Is domain name exists.</short> 
        /// <category>Domains</category>
        [Read(@"domains/exists")]
        public bool IsDomainExists(string name)
        {
            var isExists = MailEngineFactory.ServerDomainEngine.IsDomainExists(name);
            return isExists;
        }

        /// <summary>
        ///    Check web domain name ownership over txt record in dns
        /// </summary>
        /// <param name="name">web domain name</param>
        /// <returns>True if user is owner of this domain.</returns>
        /// <short>Check domain ownership.</short> 
        /// <category>Domains</category>
        [Read(@"domains/ownership/check")]
        public bool CheckDomainOwnership(string name)
        {
            var isOwnershipProven = MailEngineFactory.ServerEngine.CheckDomainOwnership(name);
            return isOwnershipProven;
        }
    }
}
