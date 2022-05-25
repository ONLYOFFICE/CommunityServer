using System;
using System.Collections.Generic;

internal class Program
{
    private static readonly Dictionary<int, ISample> _samples = new Dictionary<int, ISample>
    {
        {1, new NetworkingSample()},
        {2, new ComputeSample()},
    };

    private static void Main()
    {
        Console.Write("Identity Endpoint (e.g. http://104.130.30.68:5000/v2.0): ");
        string identityEndpoint = Console.ReadLine();

        Console.Write("User: ");
        string username = Console.ReadLine();

        Console.Write("Password: ");
        string password = Console.ReadLine();

        Console.Write("Project Name: ");
        string project = Console.ReadLine();

        Console.Write("Region [RegionOne]: ");
        string region = Console.ReadLine();
        if (string.IsNullOrEmpty(region))
            region = "RegionOne";

        Console.WriteLine("Available Samples: ");
        Console.WriteLine("\t1. Networking");
        Console.WriteLine("\t2. Compute");
        Console.WriteLine();

        Console.Write("Enter the example number to execute: ");
        string requestedSample = Console.ReadLine();
        int sampleId;
        if (!(int.TryParse(requestedSample, out sampleId) && _samples.ContainsKey(sampleId)))
        {
            Console.WriteLine("Invalid sample requested. Exiting.");
        }
        else
        {
            ISample sample = _samples[sampleId];

            Console.WriteLine();
            sample.PrintTasks();
            Console.WriteLine();

            Console.Write("Do you want to proceed? [y/N]: ");
            var shouldProceed = Console.ReadLine();
            if (shouldProceed.ToLower() == "y")
                sample.Run(identityEndpoint, username, password, project, region).Wait();
        }

        Console.WriteLine("Press any key to exit...");
        Console.ReadLine();
    }
}