using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using net.openstack.Providers.Rackspace;
using net.openstack.Core.Domain;
using log4net;
using ASC.Data.Storage.Configuration;
using MimeMapping = ASC.Common.Web.MimeMapping;

namespace ASC.Data.Storage.RackspaceCloud
{
    public class RackspaceCloudCrossModuleTransferUtility : ICrossModuleTransferUtility
    {
        private readonly ModuleConfigurationElement _srcModuleConfiguration;
        private readonly ModuleConfigurationElement _destModuleConfiguration;
        private readonly static ILog log = LogManager.GetLogger(typeof(RackspaceCloudCrossModuleTransferUtility));
        private readonly string _srcTenant;
        private readonly string _destTenant;
        private readonly string _srcContainer;
        private readonly string _destContainer;
        private readonly string _username;
        private readonly string _apiKey;

        public RackspaceCloudCrossModuleTransferUtility(String srcTenant,
                                                        ModuleConfigurationElement srcModuleConfig,
                                                        IDictionary<string, string> srcStorageConfig,
                                                        String destTenant,
                                                        ModuleConfigurationElement destModuleConfig,
                                                        IDictionary<string, string> destStorageConfig)
        {
            _srcTenant = srcTenant;
            _destTenant = destTenant;
            _srcContainer = srcStorageConfig["container"];
            _destContainer = destStorageConfig["container"];

            _apiKey = srcStorageConfig["apiKey"];
            _username = srcStorageConfig["username"];

            _srcModuleConfiguration = srcModuleConfig;
            _destModuleConfiguration = destModuleConfig;
        }


        public void MoveFile(string srcDomain, string srcPath, string destDomain, string destPath)
        {
            CopyFile(srcDomain, srcPath, destDomain, destPath);
            DeleteSrcFile(srcDomain, srcPath);
        }

        public void CopyFile(string srcDomain, string srcPath, string destDomain, string destPath)
        {
            var srcKey = GetKey(_srcTenant, _srcModuleConfiguration.Name, srcDomain, srcPath);
            var destKey = GetKey(_destTenant, _destModuleConfiguration.Name, destDomain, destPath);
            try
            {

                var client = GetClient();

                client.CopyObject(_srcContainer, srcKey, _destContainer, destKey);

            }
            catch (Exception err)
            {
                log.ErrorFormat("sb {0}, db {1}, sk {2}, dk {3}, err {4}", _srcContainer, _destContainer, srcKey, destKey, err);
                throw;
            }

        }

        private CloudFilesProvider GetClient()
        {
            CloudIdentity cloudIdentity = new CloudIdentity()
            {
                Username = _username,
                APIKey = _apiKey
            };

            return new CloudFilesProvider(cloudIdentity);
        }


        private void DeleteSrcFile(string domain, string path)
        {
            var key = GetKey(_srcTenant, _srcModuleConfiguration.Name, domain, path);

            var client = GetClient();

            client.DeleteObject(_srcContainer, key);
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
