using System;
using System.Linq;
using System.Threading.Tasks;
using net.openstack.Core.Domain;
using net.openstack.Core.Providers;
using OpenStack.Compute.v2_1;

public class ComputeSample : ISample
{
    public async Task Run(string identityEndpoint, string username, string password, string project, string region)
    {
        // Configure authentication
        var user = new CloudIdentityWithProject
        {
            Username = username,
            Password = password,
            ProjectName = project
        };
        var identity = new OpenStackIdentityProvider(new Uri(identityEndpoint), user);
        var compute = new ComputeService(identity, region);

        Console.WriteLine("Looking up the tiny flavor...");
        var flavors = await compute.ListFlavorsAsync();
        var tinyFlavor = flavors.FirstOrDefault(x => x.Name.Contains("tiny"));
        if(tinyFlavor == null) throw new Exception("Unable to find a flavor with the 'tiny' in the name!");

        Console.WriteLine("Looking up the cirros image...");
        var images = await compute.ListImagesAsync(new ImageListOptions {Name = "cirros"});
        var cirrosImage = images.FirstOrDefault();
        if(cirrosImage == null) throw new Exception("Unable to find an image named 'cirros'");

        Console.WriteLine("Creating Sample server... ");
        var serverDefinition = new ServerCreateDefinition("sample", cirrosImage.Id, tinyFlavor.Id);
        var server = await compute.CreateServerAsync(serverDefinition);

        Console.WriteLine("Waiting for the sample server to come online...");
        await server.WaitUntilActiveAsync();

        Console.WriteLine("Taking a snaphot of the sample server...");
        var snapshot = await server.SnapshotAsync(new SnapshotServerRequest("sample-snapshot"));
        await snapshot.WaitUntilActiveAsync();

        Console.WriteLine();
        Console.WriteLine("Sample Server Information:");
        Console.WriteLine();
        Console.WriteLine($"Server Id: {server.Id}");
        Console.WriteLine($"Server Name: {server.Name}");
        Console.WriteLine($"Server Status: {server.Status}");
        Console.WriteLine($"Server Address: {server.IPv4Address}");
        Console.WriteLine();
        Console.WriteLine("Sample Snapshot Information:");
        Console.WriteLine();
        Console.WriteLine($"Image Id: {snapshot.Id}");
        Console.WriteLine($"Image Name: {snapshot.Name}");
        Console.WriteLine($"Image Status: {snapshot.Status}");
        Console.WriteLine($"Image Type: {snapshot.Type}");
        Console.WriteLine();

        Console.WriteLine("Deleting Sample Server...");
        await snapshot.DeleteAsync();
        await server.DeleteAsync();
    }

    public void PrintTasks()
    {
        Console.WriteLine("This sample will perform the following tasks:");
        Console.WriteLine("\t* Lookup a flavor with tiny in the name");
        Console.WriteLine("\t* Lookup an image named cirros");
        Console.WriteLine("\t* Create a server using cirros and the tiny flavor");
        Console.WriteLine("\t* Snapshot the server");
        Console.WriteLine("\t* Delete the snapshot and server");
    }

}