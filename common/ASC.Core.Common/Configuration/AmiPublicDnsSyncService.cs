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


using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using ASC.Common.Module;

namespace ASC.Core.Configuration
{
    public class AmiPublicDnsSyncService : IServiceController
    {
        public void Start()
        {
            Synchronize();
        }

        public void Stop()
        {

        }

        public static void Synchronize()
        {
            if (CoreContext.Configuration.Standalone)
            {
                var tenants = CoreContext.TenantManager.GetTenants(false).Where(t => MappedDomainNotSettedByUser(t.MappedDomain));
                if (tenants.Any())
                {
                    var dnsname = GetAmiPublicDnsName();
                    foreach (var tenant in tenants.Where(t => !string.IsNullOrEmpty(dnsname) && t.MappedDomain != dnsname))
                    {
                        tenant.MappedDomain = dnsname;
                        CoreContext.TenantManager.SaveTenant(tenant);
                    }
                }
            }
        }

        private static bool MappedDomainNotSettedByUser(string domain)
        {
            return string.IsNullOrEmpty(domain) || Regex.IsMatch(domain, "^ec2.+\\.compute\\.amazonaws\\.com$", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
        }

        private static string GetAmiPublicDnsName()
        {
            try
            {
                var request = WebRequest.Create("http://169.254.169.254/latest/meta-data/public-hostname");
                request.Timeout = 5000;
                using (var responce = request.GetResponse())
                using (var stream = responce.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    throw;
                }
            }
            return null;
        }
    }
}
