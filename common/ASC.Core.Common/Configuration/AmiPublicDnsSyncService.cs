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
                var tenants = CoreContext.TenantManager.GetTenants().Where(t => MappedDomainNotSettedByUser(t.MappedDomain));
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
