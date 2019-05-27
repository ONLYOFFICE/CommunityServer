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
