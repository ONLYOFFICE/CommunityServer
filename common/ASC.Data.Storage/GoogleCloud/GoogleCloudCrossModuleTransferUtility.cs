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


#region Import

using ASC.Data.Storage.Configuration;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#endregion

namespace ASC.Data.Storage.GoogleCloud
{
    public class GoogleCloudCrossModuleTransferUtility : ICrossModuleTransferUtility
    {

        private readonly static ILog log = LogManager.GetLogger(typeof(GoogleCloudCrossModuleTransferUtility));
        private readonly string _srcTenant;
        private readonly string _destTenant;
        private readonly string _srcBucket;
        private readonly string _destBucket;
        private readonly string _jsonPath;
        private readonly string _region;
        private readonly ModuleConfigurationElement _srcModuleConfiguration;
        private readonly ModuleConfigurationElement _destModuleConfiguration;

        public GoogleCloudCrossModuleTransferUtility(string srcTenant,
                                            ModuleConfigurationElement srcModuleConfig,
                                            IDictionary<string, string> srcStorageConfig,
                                            string destTenant,
                                            ModuleConfigurationElement destModuleConfig,
                                            IDictionary<string, string> destStorageConfig)
        {
            _srcTenant = srcTenant;
            _destTenant = destTenant;
            _srcBucket = srcStorageConfig["bucket"];
            _destBucket = destStorageConfig["bucket"];
            _jsonPath = srcStorageConfig["jsonPath"];
            _region = destStorageConfig["region"];
            _srcModuleConfiguration = srcModuleConfig;
            _destModuleConfiguration = destModuleConfig;
        }

        private StorageClient GetStorage()
        {
            GoogleCredential credential = null;

            using (var jsonStream = new FileStream(_jsonPath, FileMode.Open,
                FileAccess.Read, FileShare.Read))
            {
                credential = GoogleCredential.FromStream(jsonStream);
            }

            return StorageClient.Create(credential);
        }


        public void MoveFile(string srcDomain, string srcPath, string destDomain, string destPath)
        {
            CopyFile(srcDomain, srcPath, destDomain, destPath);
            DeleteSrcFile(srcDomain, srcPath);
        }

        private void DeleteSrcFile(string domain, string path)
        {
            var key = GetKey(_srcTenant, _srcModuleConfiguration.Name, domain, path);

            var storage = GetStorage();

            storage.DeleteObject(_srcBucket, key);
        }


        public void CopyFile(string srcDomain, string srcPath, string destDomain, string destPath)
        {
            var srcKey = GetKey(_srcTenant, _srcModuleConfiguration.Name, srcDomain, srcPath);
            var destKey = GetKey(_destTenant, _destModuleConfiguration.Name, destDomain, destPath);
          
            var storage = GetStorage();

            try
            {               
                storage.CopyObject(_srcBucket, srcKey, _destBucket, destKey, new CopyObjectOptions
                {
                    DestinationPredefinedAcl = GetDestDomainAcl(destDomain)
                });
            }
            catch (Exception err)
            {
                log.ErrorFormat("sb {0}, db {1}, sk {2}, dk {3}, err {4}", _srcBucket, _destBucket, srcKey, destKey, err);
                
                throw;            
            }
        }

        private PredefinedObjectAcl GetGoogleCloudAcl(ACL acl)
        {
            switch (acl)
            {
                case ACL.Read:
                    return PredefinedObjectAcl.PublicRead;
                default:
                    return PredefinedObjectAcl.PublicRead;
            }
        }

        private PredefinedObjectAcl GetDestDomainAcl(string domain)
        {
            if (GetDomainExpire(_destModuleConfiguration, domain) != TimeSpan.Zero)
            {
                return PredefinedObjectAcl.Private;
            }

            var domainConfiguration = _destModuleConfiguration.Domains.GetDomainElement(domain);
            
            if (domainConfiguration == null)
            {
                return GetGoogleCloudAcl(_destModuleConfiguration.Acl);
            }
            
            return GetGoogleCloudAcl(domainConfiguration.Acl);
        
        }      

        private static TimeSpan GetDomainExpire(ModuleConfigurationElement moduleConfiguration, string domain)
        {
            var domainConfiguration = moduleConfiguration.Domains.GetDomainElement(domain);
        
            if (domainConfiguration == null || domainConfiguration.Expires == TimeSpan.Zero)
            {
                return moduleConfiguration.Expires;
            }
            
            return domainConfiguration.Expires;        
        }
        
        private static string GetKey(string tenantId, string module, string domain, string path)
        {
            path = path.TrimStart('\\').Trim('/').Replace('\\', '/');
            return string.Format("{0}/{1}/{2}/{3}", tenantId, module, domain, path).Replace("//", "/");
        }

    }
}    