using System;
using System.Linq;
using net.openstack.Core.Domain;
using net.openstack.Core.Providers;
using net.openstack.Providers.Rackspace;
using OpenStack;
using OpenStack.ContentDeliveryNetworks.v1;
using OpenStack.Synchronous;

namespace CSharpCodeSamples
{
    public class ContentDeliveryNetworkExample
    {
        public async void FindAServiceAysnc()
        {
            var identity = new CloudIdentity {Username = "{username}", APIKey = "{api-key}"};
            IIdentityProvider identityProvider = new CloudIdentityProvider(identity);
            var service = new ContentDeliveryNetworkService(identityProvider, "DFW");

            IPage<Service> currentPage = await service.ListServicesAsync();
            
            Service myService;
            do
            {
                myService = currentPage.FirstOrDefault(x => x.Name == "MyService");
                if (myService != null)
                    break;

                currentPage = await currentPage.GetNextPageAsync();
            } while (currentPage.Any());

            if (myService == null)
            {
                Console.Error.WriteLine("Could not find MyService!");
                return;
            }

            Console.WriteLine("MyService: {0}", myService.Status);
        }

        public void FindAService()
        {
            var identity = new CloudIdentity { Username = "{username}", APIKey = "{api-key}" };
            IIdentityProvider identityProvider = new CloudIdentityProvider(identity);
            var service = new ContentDeliveryNetworkService(identityProvider, "DFW");

            IPage<Service> currentPage = service.ListServices();

            Service myService;
            do
            {
                myService = currentPage.FirstOrDefault(x => x.Name == "MyService");
                if (myService != null)
                    break;

                currentPage = currentPage.GetNextPage();
            } while (currentPage.Any());

            if (myService == null)
            {
                Console.Error.WriteLine("Could not find MyService!");
                return;
            }

            Console.WriteLine("MyService: {0}", myService.Status);
        }
    }
}
