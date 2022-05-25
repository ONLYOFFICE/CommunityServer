using System;
using System.Collections.Generic;

internal class Program
{
    private static readonly Dictionary<int, ISample> _samples = new Dictionary<int, ISample>
    {
        {1, new CloudNetworkSamples()},
        {2, new AssignPublicIPSamples()}
    };

    private static void Main()
    {
        Console.Write("User: ");
        string username = Console.ReadLine();

        Console.Write("API Key: ");
        string apiKey = Console.ReadLine();

        Console.Write("Region: ");
        string region = Console.ReadLine();

        Console.WriteLine("Available Samples: ");
        Console.WriteLine("\t1. Cloud Networks");
        Console.WriteLine("\t2. RackConnect: Assign Public IP to Cloud Server");
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
                sample.Run(username, apiKey, region).Wait();
        }

        Console.WriteLine("Press any key to exit...");
        Console.ReadLine();
    }
}