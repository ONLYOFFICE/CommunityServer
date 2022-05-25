namespace net.openstack.Providers.Rackspace.Objects.Request
{
    using System;
    using net.openstack.Core.Domain;
    using Newtonsoft.Json;

    /// <summary>
    /// This models the JSON body containing details for the Resize Server request.
    /// </summary>
    /// <seealso cref="ServerResizeRequest"/>
    /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/Resize_Server-d1e3707.html">Resize Server (OpenStack Compute API v2 and Extensions Reference)</seealso>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class ServerResizeDetails
    {
        /// <summary>
        /// Gets the new name for the resized server.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; private set; }

        /// <summary>
        /// Gets the ID of the new flavor.
        /// </summary>
        /// <seealso cref="net.openstack.Core.Domain.Flavor.Id"/>
        [JsonProperty("flavorRef")]
        public string Flavor { get; private set; }

        /// <summary>
        /// The disk configuration. If the value is <see langword="null"/>, the default configuration for the image is used.
        /// </summary>
        [JsonProperty("OS-DCF:diskConfig", DefaultValueHandling = DefaultValueHandling.Include)]
        public DiskConfiguration DiskConfig { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerResizeDetails"/> class with the specified details.
        /// </summary>
        /// <param name="name">The new name for the resized server.</param>
        /// <param name="flavor">The new flavor. This is obtained from <see cref="net.openstack.Core.Domain.Flavor.Id">Flavor.Id</see>.</param>
        /// <param name="diskConfig">The disk configuration. If the value is <see langword="null"/>, the default configuration for the specified image is used.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="name"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="flavor"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="name"/> is empty.
        /// <para>-or-</para>
        /// <para>If <paramref name="flavor"/> is empty.</para>
        /// </exception>
        public ServerResizeDetails(string name, string flavor, DiskConfiguration diskConfig)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (flavor == null)
                throw new ArgumentNullException("flavor");
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("serverName cannot be empty");
            if (string.IsNullOrEmpty(flavor))
                throw new ArgumentException("flavor cannot be empty");

            Name = name;
            Flavor = flavor;
            DiskConfig = diskConfig;
        }
    }
}
