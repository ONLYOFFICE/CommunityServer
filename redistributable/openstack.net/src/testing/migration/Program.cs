using System;
using net.openstack.Core.Domain;
using net.openstack.Core.Providers;
using net.openstack.Providers.Rackspace;
using OpenStack;
using OpenStack.Compute.v2_1;
using OpenStack.Networking.v2;
using OpenStack.Synchronous;

namespace migration
{
    class Program
    {
        static void Main(string[] args)
        {
            const string region = "RegionOne";

            // Configure OpenStack.NET
            OpenStackNet.Configure(options => options.DefaultTimeout=TimeSpan.FromSeconds(5));

            // Authenticate
            var identityUrl = new Uri("http://example.com");
            var user = new CloudIdentityWithProject();
            var identity = new OpenStackIdentityProvider(identityUrl, user);
            identity.Authenticate();

            // Use legacy and new providers
            var legacyNetworking = new CloudNetworksProvider(null, identity);
            legacyNetworking.ListNetworks();
            var networks = new NetworkingService(identity, region);
            networks.ListNetworks();

            var legacyCompute = new CloudServersProvider(null, identity);
            legacyCompute.ListServers();
            var compute = new ComputeService(identity, region);
            compute.ListServers();
        }
    }
}
