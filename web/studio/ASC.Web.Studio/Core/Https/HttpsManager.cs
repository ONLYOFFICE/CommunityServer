/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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


using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;
using System.Web.Hosting;
using ASC.Core;
using Microsoft.Web.Administration;

namespace ASC.Web.Studio.Core.Https
{
    public class HttpsManager
    {
        public static bool IsExistHttpsCertificate()
        {
            if (!CoreContext.Configuration.Standalone)
                throw new Exception("Functionality not available");

            using (var serverManager = new ServerManager())
            {
                if (serverManager.Sites.SelectMany(r => r.Bindings).Any(r => r.BindingInformation.Contains("*:443:")))
                {
                    return true;
                }
            }

            return false;
        }

        public static void UploadCertificate(string filePath, string password)
        {
            if (!CoreContext.Configuration.Standalone)
                throw new Exception("Functionality not available");

            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("filePath");

            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("password");

            var fileName = Path.GetFileName(filePath);
            var fileExt = Path.GetExtension(fileName);

            if (string.IsNullOrEmpty(fileExt))
                throw new ArgumentException("filePath");

            var store2 = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            var sp = new StorePermission(PermissionState.Unrestricted) {Flags = StorePermissionFlags.AllFlags};
            sp.Assert();
            store2.Open(OpenFlags.MaxAllowed);

            var cert = fileExt.Equals(".pfx", StringComparison.InvariantCultureIgnoreCase)
                            ? new X509Certificate2(filePath, password) {FriendlyName = fileName}
                            : new X509Certificate2(new X509Certificate(filePath));

            store2.Add(cert);
            store2.Close();

            UploadStandAloneCertificate(store2, cert);
            //UploadSaaSCertificate(store2, cert);
        }


        private static void UploadStandAloneCertificate(X509Store store2, X509Certificate cert)
        {
            using (var serverManager = new ServerManager())
            {
                const string newbindinginformation = "*:443:";

                foreach (var s in serverManager.Sites)
                {
                    var bindingIndex = -1;
                    foreach (var b in s.Bindings.Where(r => r.BindingInformation.Contains(newbindinginformation)))
                    {
                        bindingIndex = s.Bindings.IndexOf(b);
                    }

                    if (bindingIndex != -1)
                    {
                        s.Bindings.RemoveAt(bindingIndex);
                    }
                }

                var site = serverManager.Sites[HostingEnvironment.SiteName];

                AddHttpsBinding(site, newbindinginformation, cert.GetCertHash(), store2.Name);

                AddRewriteRules(serverManager, site);

                serverManager.CommitChanges();
            }
        }

        private static void UploadSaaSCertificate(X509Store store2, X509Certificate cert)
        {
            using (var serverManager = new ServerManager())
            {
                var siteName = cert.Issuer.Contains("CN=")
                                        ? cert.Issuer.Substring(cert.Issuer.IndexOf("CN=", StringComparison.Ordinal) + 3)
                                        : CoreContext.TenantManager.GetCurrentTenant().MappedDomain;

                var newbindinginformation = "*:443:" + siteName;

                var site = serverManager.Sites[siteName];
                if (site == null)
                {
                    site = serverManager.Sites.Add(siteName, "http", "*:80:" + siteName, HostingEnvironment.ApplicationPhysicalPath);
                    site.Applications[0].ApplicationPoolName = serverManager.Sites[HostingEnvironment.SiteName].Applications[0].ApplicationPoolName;
                }

                AddHttpsBinding(site, newbindinginformation, cert.GetCertHash(), store2.Name);

                serverManager.CommitChanges();
            }
        }

        private static void AddHttpsBinding(Site site, string newbindinginformation, byte[] certHash, string storeName)
        {
            var binding = site.Bindings.Add(newbindinginformation, certHash, storeName);
            binding.Protocol = "https";
            if (!string.IsNullOrEmpty(site.Bindings[0].Host))
                binding["sslFlags"] = 1;
        }

        private static void AddRewriteRules(ServerManager serverManager, Site site)
        {
            var config = serverManager.GetWebConfiguration(site.Name);

            var rulesSection = config.GetSection("system.webServer/rewrite/rules");

            var rulesCollection = rulesSection.GetCollection();
            //HTTP to HTTPS
            var ruleElement = rulesCollection.CreateElement("rule");
            ruleElement["name"] = @"HTTP to HTTPS";
            ruleElement["stopProcessing"] = true;

            var matchElement = ruleElement.GetChildElement("match");
            matchElement["url"] = @".*";

            var conditionsElement = ruleElement.GetChildElement("conditions");

            var conditionsCollection = conditionsElement.GetCollection();

            var addElement = conditionsCollection.CreateElement("add");
            addElement["input"] = @"{HTTPS}";
            addElement["pattern"] = @"off";
            conditionsCollection.Add(addElement);

            addElement = conditionsCollection.CreateElement("add");
            addElement["input"] = @"{REQUEST_URI}";
            addElement["pattern"] = @"products/files/services/wcfservice/service.svc.*";
            addElement["negate"] = @"true";
            conditionsCollection.Add(addElement);

            addElement = conditionsCollection.CreateElement("add");
            addElement["input"] = @"{HTTP_HOST}";
            addElement["pattern"] = @"localhost";
            addElement["negate"] = @"true";
            conditionsCollection.Add(addElement);

            var actionElement = ruleElement.GetChildElement("action");
            actionElement["type"] = @"Redirect";
            actionElement["url"] = @"https://{HTTP_HOST}{URL}";
            actionElement["appendQueryString"] = "true";
            actionElement["redirectType"] = @"Temporary";

            if (rulesCollection.Count(r => "HTTP to HTTPS".Equals(r.GetAttributeValue("name"))) <= 0)
            {
                rulesCollection.Add(ruleElement);
            }

            //WCF files HTTPS to HTTP
            ruleElement = rulesCollection.CreateElement("rule");
            ruleElement["name"] = @"WCF files HTTPS to HTTP";
            ruleElement["stopProcessing"] = true;

            matchElement = ruleElement.GetChildElement("match");
            matchElement["url"] = @"products/files/services/wcfservice/service.svc.*";

            conditionsElement = ruleElement.GetChildElement("conditions");

            conditionsCollection = conditionsElement.GetCollection();

            addElement = conditionsCollection.CreateElement("add");
            addElement["input"] = @"{HTTPS}";
            addElement["pattern"] = @"on";
            conditionsCollection.Add(addElement);

            actionElement = ruleElement.GetChildElement("action");
            actionElement["type"] = @"Rewrite";
            actionElement["url"] = @"http://{HTTP_HOST}{URL}";
            actionElement["appendQueryString"] = "true";

            if (rulesCollection.Count(r => "WCF files HTTPS to HTTP".Equals(r.GetAttributeValue("name"))) <= 0)
            {
                rulesCollection.Add(ruleElement);
            }
        }

    }
}