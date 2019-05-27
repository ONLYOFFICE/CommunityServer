/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
