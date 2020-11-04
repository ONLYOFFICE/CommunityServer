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


using System.Linq;
using ASC.Api.Attributes;
using ASC.Core;
using ASC.Mail.Data.Contracts;

namespace ASC.Api.MailServer
{
    public partial class MailServerApi
    {
        /// <summary>
        ///    Returns ServerData for mail server associated with tenant
        /// </summary>
        /// <returns>ServerData for current tenant.</returns>
        /// <short>Get mail server</short> 
        /// <category>Servers</category>
        [Read(@"server")]
        public ServerData GetMailServer()
        {
            return MailEngineFactory.ServerEngine.GetMailServer();
        }

        /// <summary>
        ///    Returns ServerData for mail server associated with tenant
        /// </summary>
        /// <returns>ServerData for current tenant.</returns>
        /// <short>Get mail server</short> 
        /// <category>Servers</category>
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
        ///    Get or generate free to any domain DNS records
        /// </summary>
        /// <returns>DNS records for current tenant and user.</returns>
        /// <short>Get free DNS records</short>
        /// <category>DnsRecords</category>
        [Read(@"freedns/get")]
        public ServerDomainDnsData GetUnusedDnsRecords()
        {
            return MailEngineFactory.ServerEngine.GetOrCreateUnusedDnsData();
        }
    }
}
