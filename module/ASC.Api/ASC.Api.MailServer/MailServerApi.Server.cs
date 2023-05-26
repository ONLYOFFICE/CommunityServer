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
using System.Linq;

using ASC.Api.Attributes;
using ASC.Core;
using ASC.Mail.Data.Contracts;

using ASC.Web.Studio.PublicResources;

namespace ASC.Api.MailServer
{
    public partial class MailServerApi
    {
        /// <summary>
        /// Returns the mail server associated with the current tenant.
        /// </summary>
        /// <returns type="ASC.Mail.Data.Contracts.ServerData, ASC.Mail">Mail server data for the current tenant</returns>
        /// <short>Get the mail server</short> 
        /// <category>Servers</category>
        /// <path>api/2.0/mailserver/server</path>
        /// <httpMethod>GET</httpMethod>
        [Read(@"server")]
        public ServerData GetMailServer()
        {
            if (!IsEnableMailServer) throw new Exception(Resource.ErrorNotAllowedOption);
            return MailEngineFactory.ServerEngine.GetMailServer();
        }

        /// <summary>
        /// Returns full information on the mail server associated with the current tenant.
        /// </summary>
        /// <returns type="ASC.Mail.Data.Contracts.ServerFullData, ASC.Mail">Full mail server information for the current tenant</returns>
        /// <short>Get the mail server information</short> 
        /// <category>Servers</category>
        /// <path>api/2.0/mailserver/serverinfo/get</path>
        /// <httpMethod>GET</httpMethod>
        [Read(@"serverinfo/get")]
        public ServerFullData GetMailServerFullInfo()
        {
            var fullServerInfo = MailEngineFactory.ServerEngine.GetMailServerFullInfo();

            if (!CoreContext.Configuration.Standalone)
                return fullServerInfo;

            var commonDomain = fullServerInfo.Domains.FirstOrDefault(d => d.IsSharedDomain);
            if (commonDomain == null)
                return fullServerInfo;

            //Skip common domain
            fullServerInfo.Domains = fullServerInfo.Domains.Where(d => !d.IsSharedDomain).ToList();
            fullServerInfo.Mailboxes =
                fullServerInfo.Mailboxes.Where(m => m.Address.DomainId != commonDomain.Id).ToList();

            return fullServerInfo;
        }

        /// <summary>
        /// Returns or generates free DNS records.
        /// </summary>
        /// <returns type="ASC.Mail.Data.Contracts.ServerDomainDnsData, ASC.Mail">DNS records for the current tenant and user</returns>
        /// <short>Get or create free DNS records</short>
        /// <category>Servers</category>
        /// <path>api/2.0/mailserver/freedns/get</path>
        /// <httpMethod>GET</httpMethod>
        [Read(@"freedns/get")]
        public ServerDomainDnsData GetUnusedDnsRecords()
        {
            if (!IsEnableMailServer) throw new Exception(Resource.ErrorNotAllowedOption);
            return MailEngineFactory.ServerEngine.GetOrCreateUnusedDnsData();
        }
    }
}
