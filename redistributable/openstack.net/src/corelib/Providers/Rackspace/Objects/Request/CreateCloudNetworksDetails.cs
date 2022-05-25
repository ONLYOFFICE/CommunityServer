namespace net.openstack.Providers.Rackspace.Objects.Request
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// This models the JSON body containing details for the Create Network request.
    /// </summary>
    /// <seealso cref="CreateCloudNetworkRequest"/>
    /// <seealso href="http://docs.openstack.org/api/openstack-network/2.0/content/Create_Network.html">Create Network (OpenStack Networking API v2.0 Reference)</seealso>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class CreateCloudNetworksDetails
    {
        /// <summary>
        /// Gets the CIDR for the network.
        /// </summary>
        [JsonProperty("cidr")]
        public string Cidr { get; private set; }

        /// <summary>
        /// Gets the name of the network.
        /// </summary>
        [JsonProperty("label")]
        public string Label { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateCloudNetworksDetails"/>
        /// class with the specified CIDR and name.
        /// </summary>
        /// <param name="cidr">The IP block from which to allocate the network. For example, <c>172.16.0.0/24</c> or <c>2001:DB8::/64</c>.</param>
        /// <param name="label">The name of the new network. For example, <c>my_new_network</c>.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="cidr"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="label"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="cidr"/> is empty.
        /// <para>-or-</para>
        /// <para>If <paramref name="label"/> is empty.</para>
        /// </exception>
        public CreateCloudNetworksDetails(string cidr, string label)
        {
            if (cidr == null)
                throw new ArgumentNullException("cidr");
            if (label == null)
                throw new ArgumentNullException("label");
            if (string.IsNullOrEmpty(cidr))
                throw new ArgumentException("cidr cannot be empty");
            if (string.IsNullOrEmpty(label))
                throw new ArgumentException("label cannot be empty");

            Cidr = cidr;
            Label = label;
        }
    }
}
