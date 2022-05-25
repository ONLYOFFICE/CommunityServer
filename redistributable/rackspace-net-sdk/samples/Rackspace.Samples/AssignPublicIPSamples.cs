using System;
using System.Linq;
using System.Threading.Tasks;
using net.openstack.Core.Domain;
using net.openstack.Providers.Rackspace;
using Rackspace.RackConnect.v3;

public class AssignPublicIPSamples : ISample
{
    public async Task Run(string username, string apiKey, string region)
    {
        // Configure authentication
        var identity = new CloudIdentity
        {
            Username = username,
            APIKey = apiKey
        };
        var identityService = new CloudIdentityProvider(identity);
        var result = identityService.Authenticate();

        var serverService = new CloudServersProvider(identity, region, null, null);
        var rackConnectService = new RackConnectService(identityService, region);

        // Create a cloud server on your hybrid network
        Console.WriteLine($"Looking up your RackConnect network in {region}...");
        var networks = await rackConnectService.ListNetworksAsync();
        var network = networks.FirstOrDefault();
        if (network == null)
            throw new Exception($"You do not have a Hybrid Cloud / RackConnect network configured in the {region} which is required to run this sample.");

        Console.WriteLine("Creating sample cloud server... ");
        // Ubuntu 14.04 LTS (Trusty Tahr) (PVHVM)
        const string ubuntuImageId = "09de0a66-3156-48b4-90a5-1cf25a905207";
        // 512MB Standard Instance
        const string standardFlavorId = "2";
        var requestedServer = serverService.CreateServer("sample", ubuntuImageId, standardFlavorId,
            networks: new string[] {network.Id});
        serverService.WaitForServerActive(requestedServer.Id);

        Console.WriteLine("Allocating a public IP address...");
        var ip = await rackConnectService.CreatePublicIPAsync(
            new PublicIPCreateDefinition {ShouldRetain = true});
        await ip.WaitUntilActiveAsync();
        Console.WriteLine($"Acquired {ip.PublicIPv4Address}!");

        Console.WriteLine("Assigning the public IP to the sample cloud server...");
        await ip.AssignAsync(requestedServer.Id);
        await ip.WaitUntilActiveAsync();

        Console.WriteLine("Deleting sample cloud server...");
        serverService.DeleteServer(requestedServer.Id);

        Console.WriteLine("Deallocating the public IP address...");
        await ip.DeleteAsync();
    }

    public void PrintTasks()
    {
        Console.WriteLine("This sample will perform the following tasks:");
        Console.WriteLine("\t* Locate a hybrid/RackConnect network");
        Console.WriteLine("\t* Create a cloud server on the network");
        Console.WriteLine("\t* Allocate a public IP on the network");
        Console.WriteLine("\t* Assign the public IP to the cloud server");
        Console.WriteLine("\t* Remove the public IP from the cloud server");
        Console.WriteLine("\t* Remove the cloud server");
    }

}