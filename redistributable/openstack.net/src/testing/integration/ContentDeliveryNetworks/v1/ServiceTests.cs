using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Marvin.JsonPatch;
using Newtonsoft.Json;
using OpenStack.Synchronous;
using Xunit;
using Xunit.Abstractions;

namespace OpenStack.ContentDeliveryNetworks.v1
{
    [Trait("ci","false")]
    public class ServiceTests : IDisposable
    {
        private readonly ContentDeliveryNetworkService _cdnService;

        public ServiceTests(ITestOutputHelper testLog)
        {
            var testOutput = new XunitTraceListener(testLog);
            OpenStackNet.Tracing.Http.Listeners.Add(testOutput);
            Trace.Listeners.Add(testOutput);

            var authenticationProvider = TestIdentityProvider.GetIdentityProvider();
            _cdnService = new ContentDeliveryNetworkService(authenticationProvider, "DFW");
        }

        public void Dispose()
        {
            Trace.Listeners.Clear();
            OpenStackNet.Tracing.Http.Listeners.Clear();
        }

        [Fact]
        public async Task CreateAServiceOnAkamai_UsingDefaults()
        {
            Trace.WriteLine("Looking for a CDN flavor provided by Akamai...");
            var flavors = await _cdnService.ListFlavorsAsync();
            var flavor = flavors.FirstOrDefault(x => x.Providers.Any(p => string.Equals(p.Name, "Akamai", StringComparison.OrdinalIgnoreCase)));
            Assert.NotNull(flavor);
            var akamaiFlavor = flavor.Id;
            Trace.WriteLine(string.Format("Found the {0} flavor", akamaiFlavor));

            Trace.WriteLine("Creating a CDN service using defaults for anything I can omit...");
            var domains = new[] {new ServiceDomain("mirror.example.com")};
            var origins = new[] {new ServiceOrigin("example.com")};
            var serviceDefinition = new ServiceDefinition("ci-test", akamaiFlavor, domains, origins);
            var serviceId = await _cdnService.CreateServiceAsync(serviceDefinition);
            Trace.WriteLine(string.Format("Service was created: {0}", serviceId));

            try
            {
                Trace.WriteLine("Waiting for the service to be deployed...");
                var service = await _cdnService.WaitForServiceDeployedAsync(serviceId, progress: new Progress<bool>(x => Trace.WriteLine("...")));

                Trace.WriteLine("Verifying service matches the requested definition...");
                Assert.Equal("ci-test", service.Name);
                Assert.Equal(serviceDefinition.FlavorId, service.FlavorId);

                Assert.Equal(serviceDefinition.Origins.Count, service.Origins.Count());
                Assert.Equal(serviceDefinition.Origins.First().Origin, service.Origins.First().Origin);

                Assert.Equal(serviceDefinition.Domains.Count, service.Domains.Count());
                Assert.Equal(serviceDefinition.Domains.First().Domain, service.Domains.First().Domain);

                Trace.WriteLine("Updating the service...");
                var patch = new JsonPatchDocument<ServiceDefinition>();
                patch.Replace(x => x.Name, "ci-test2");
                var intranetOnly = new ServiceRestriction("intranet", new[] {new ServiceRestrictionRule("intranet", "intranet.example.com")});
                patch.Add(x => x.Restrictions, intranetOnly, 0);
                await _cdnService.UpdateServiceAsync(serviceId, patch);

                Trace.WriteLine("Waiting for the service changes to be deployed...");
                service = await _cdnService.WaitForServiceDeployedAsync(serviceId, progress: new Progress<bool>(x => Trace.WriteLine("...")));

                Trace.WriteLine("Verifying service matches updated definition...");
                Assert.Equal("ci-test2", service.Name);
                Assert.Equal(JsonConvert.SerializeObject(intranetOnly), JsonConvert.SerializeObject(service.Restrictions.First()));

                Trace.WriteLine("Purging all assets on service");
                await _cdnService.PurgeCachedAssetsAsync(serviceId);
            }
            finally
            {
                Trace.WriteLine("Cleaning up any test data...");

                Trace.WriteLine("Removing the service...");
                _cdnService.DeleteService(serviceId);
                _cdnService.WaitForServiceDeleted(serviceId);
                Trace.WriteLine("The service was cleaned up sucessfully.");
            }
        }

        [Fact]
        public async Task FindServiceOnAPage()
        {
            var serviceIds = new List<string>();
            try
            {
                var create1 = CreateService("ci-test1", "mirror.example1.com", "example1.com").ContinueWith(t => serviceIds.Add(t.Result));
                var create2 = CreateService("ci-test2", "mirror.example2.com", "example2.com").ContinueWith(t => serviceIds.Add(t.Result));
                var create3 = CreateService("ci-test3", "mirror.example3.com", "example3.com").ContinueWith(t => serviceIds.Add(t.Result));

                await Task.WhenAll(create1, create2, create3);

                var currentPage = await _cdnService.ListServicesAsync(pageSize: 1);
                while (currentPage.Any())
                {
                    if (currentPage.Any(x => x.Name == "ci-test3"))
                    {
                        Trace.WriteLine("Found the desired service");
                        break;
                    }

                    currentPage = await currentPage.GetNextPageAsync();
                }
            }
            finally
            {
                Trace.WriteLine("Cleaning up any test data...");

                Trace.WriteLine("Removing the services...");
                var deletes = serviceIds.Select(serviceId => _cdnService
                    .DeleteServiceAsync(serviceId)
                    .ContinueWith(t => _cdnService.WaitForServiceDeletedAsync(serviceId))
                    .ContinueWith(t => Trace.WriteLine(string.Format("Service was deleted: {0}", serviceId))))
                    .ToArray();

                Task.WaitAll(deletes);
                Trace.WriteLine("The services were cleaned up sucessfully.");

            }
        }

        private async Task<string> CreateService(string name, string domain, string origin)
        {
            var flavors = await _cdnService.ListFlavorsAsync();
            var flavor = flavors.First();

            Trace.WriteLine(string.Format("Creating CDN Service: {0} for {1} originating from {2}", name, domain, origin));
            var domains = new[] { new ServiceDomain(domain) };
            var origins = new[] { new ServiceOrigin(origin) };
            var serviceDefinition = new ServiceDefinition(name, flavor.Id, domains, origins);
            var serviceId = await _cdnService.CreateServiceAsync(serviceDefinition);
            Trace.WriteLine("Service was created: {0}", serviceId);
            return serviceId;
        }
    }
}
