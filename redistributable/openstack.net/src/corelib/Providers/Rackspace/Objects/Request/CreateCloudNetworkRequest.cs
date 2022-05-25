namespace net.openstack.Providers.Rackspace.Objects.Request
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// This models the JSON request used for the Create Network request.
    /// </summary>
    /// <seealso href="http://docs.openstack.org/api/openstack-network/2.0/content/Create_Network.html">Create Network (OpenStack Networking API v2.0 Reference)</seealso>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class CreateCloudNetworkRequest
    {
        /// <summary>
        /// Gets additional details about the Create Network request.
        /// </summary>
        [JsonProperty("network")]
        public CreateCloudNetworksDetails Details { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateCloudNetworkRequest"/>
        /// class with the specified details.
        /// </summary>
        /// <param name="details">The details of the request.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="details"/> is <see langword="null"/>.</exception>
        public CreateCloudNetworkRequest(CreateCloudNetworksDetails details)
        {
            if (details == null)
                throw new ArgumentNullException("details");

            Details = details;
        }
    }
}
