using System;
using System.IO;
using System.Text;
using net.openstack.Core.Domain;
using net.openstack.Core.Providers;
using net.openstack.Providers.Rackspace;
using net.openstack.Providers.Rackspace.Objects;

namespace Net.OpenStack.Testing.Integration
{
    public class Bootstrapper
    {
        private static OpenstackNetSetings _settings;
        public static OpenstackNetSetings Settings
        {
            get
            {
                if(_settings == null)
                    Initialize();

                return _settings;
            }
        }

        public static void Initialize()
        {
            var homeDir = Environment.ExpandEnvironmentVariables("C:\\");

            var path = Path.Combine(homeDir, ".openstack_net");

            var contents = new StringBuilder();

            using(var stream = File.Open(path, FileMode.Open, FileAccess.Read))
            {
                using(var reader = new StreamReader(stream))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        if(!line.Trim().StartsWith("//"))
                            contents.Append(line);
                    }
                }
            }

            var appCredentials = Newtonsoft.Json.JsonConvert.DeserializeObject<OpenstackNetSetings>(contents.ToString());

            _settings = appCredentials;
        }

        public static IIdentityProvider CreateIdentityProvider()
        {
            return CreateIdentityProvider(Bootstrapper.Settings.TestIdentity);
        }

        public static IIdentityProvider CreateIdentityProvider(CloudIdentity identity)
        {
            var provider = new CloudIdentityProvider(identity);
            SetUserAgent(provider);
            return provider;
        }

        public static IComputeProvider CreateComputeProvider()
        {
            var provider = new CloudServersProvider(Bootstrapper.Settings.TestIdentity, Bootstrapper.Settings.DefaultRegion, CreateIdentityProvider(), null);
            SetUserAgent(provider);
            return provider;
        }

        public static INetworksProvider CreateNetworksProvider()
        {
            var provider = new CloudNetworksProvider(Bootstrapper.Settings.TestIdentity, Bootstrapper.Settings.DefaultRegion, CreateIdentityProvider(), null);
            SetUserAgent(provider);
            return provider;
        }

        public static IBlockStorageProvider CreateBlockStorageProvider()
        {
            var provider = new CloudBlockStorageProvider(Bootstrapper.Settings.TestIdentity, Bootstrapper.Settings.DefaultRegion, CreateIdentityProvider(), null);
            SetUserAgent(provider);
            return provider;
        }

        public static IObjectStorageProvider CreateObjectStorageProvider()
        {
            var provider = new CloudFilesProvider(Bootstrapper.Settings.TestIdentity, Bootstrapper.Settings.DefaultRegion, CreateIdentityProvider(), null);
            SetUserAgent(provider);
            return provider;
        }

        private static void SetUserAgent<T>(ProviderBase<T> provider)
            where T : class
        {
            provider.ApplicationUserAgent = "CI-BOT";
        }
    }

    public class OpenstackNetSetings
    {
        public ExtendedCloudIdentity TestIdentity { get; set; }

        public ExtendedCloudIdentity TestAdminIdentity { get; set; }

        public ExtendedRackspaceCloudIdentity TestDomainIdentity { get; set; }

        public string RackspaceExtendedIdentityUrl { get; set; }

        public string DefaultRegion
        {
            get;
            set;
        }
    }

    public class ExtendedCloudIdentity : CloudIdentity
    {
        public string TenantId { get; set; }

        public string Domain { get; set; }
    }

    public class ExtendedRackspaceCloudIdentity : RackspaceCloudIdentity
    {
        public string TenantId { get; set; }

        public ExtendedRackspaceCloudIdentity()
        {
            
        }

        public ExtendedRackspaceCloudIdentity(ExtendedCloudIdentity cloudIdentity)
        {
            this.APIKey = cloudIdentity.APIKey;
            this.Password = cloudIdentity.Password;
            this.Username = cloudIdentity.Username;
            this.TenantId = cloudIdentity.TenantId;
            this.Domain = string.IsNullOrEmpty(cloudIdentity.Domain) ? null : new Domain(cloudIdentity.Domain);
        }
    }
}
