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
