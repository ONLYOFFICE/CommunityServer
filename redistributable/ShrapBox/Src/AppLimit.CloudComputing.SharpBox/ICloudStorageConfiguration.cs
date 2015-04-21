using System.Net;
using System;

namespace AppLimit.CloudComputing.SharpBox
{
    /// <summary>
    /// This interface has to be implemented from storage providers to support 
    /// access configuration information. Consumers of this library has to create 
    /// an instance of a provider specific implementation to build up a connection 
    /// to the CloudStorage
    /// </summary>
    public interface ICloudStorageConfiguration
    {
        /// <summary>
        /// Contains the url of the specific service which will be used from 
        /// storage service provider
        /// </summary>
        Uri ServiceLocator { get; }

        /// <summary>
        /// If true this value indicates the all ssl connection are valid. This means also ssl connection
        /// with an invalid certificate will be accepted.
        /// </summary>
        bool TrustUnsecureSSLConnections { get; }

        /// <summary>
        /// Contains the limits of a specific cloud storage connection
        /// </summary>
        CloudStorageLimits Limits { get; }
    }
}
