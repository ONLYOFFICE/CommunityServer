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

using System.Collections.Generic;
using System.Linq;
using ASC.Api.Attributes;
using ASC.Api.MailServer.DataContracts;
using ASC.Api.MailServer.Extensions;

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
        public TenantServerData GetMailServer()
        {
            var dns = GetUnusedDnsRecords();

            return new TenantServerData
                           {
                               Id = MailServer.Id,
                               Dns = dns,
                               ServerLimits = new TenantServerLimitsData
                                   {
                                       MailboxMaxCountPerUser = MailboxPerUserLimit
                                   }
                           };
        }

        /// <summary>
        ///    Returns ServerData for mail server associated with tenant
        /// </summary>
        /// <returns>ServerData for current tenant.</returns>
        /// <short>Get mail server</short> 
        /// <category>Servers</category>
        [Read(@"serverinfo/get")]
        public FullServerData GetMailServerFullInfo()
        {
            var full_server_info = new FullServerData();
            var mailboxes = new List<MailboxData>();
            var mailgroups = new List<MailGroupData>();

            var server = GetMailServer();
            var domains = GetDomains();

            if (domains.Any())
            {
                mailboxes = GetMailboxes();

                if (mailboxes.Any())
                    mailgroups = GetMailGroups();
            }

            full_server_info.Server = server;
            full_server_info.Domains = domains;
            full_server_info.Mailboxes = mailboxes;
            full_server_info.Mailgroups = mailgroups;

            return full_server_info;
        }

        /// <summary>
        ///    Get or generate free to any domain DNS records
        /// </summary>
        /// <returns>DNS records for current tenant and user.</returns>
        /// <short>Get free DNS records</short>
        /// <category>DnsRecords</category>
        [Read(@"freedns/get")]
        public DnsData GetUnusedDnsRecords()
        {
            return MailServer.GetFreeDnsRecords(MailServerFactory).ToDnsData();
        }
    }
}
