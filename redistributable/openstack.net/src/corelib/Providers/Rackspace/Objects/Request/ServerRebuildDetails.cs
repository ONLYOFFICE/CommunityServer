namespace net.openstack.Providers.Rackspace.Objects.Request
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Sockets;
    using net.openstack.Core.Domain;
    using net.openstack.Core.Domain.Converters;
    using Newtonsoft.Json;

    /// <summary>
    /// This models the JSON body containing details for the Rebuild Server request.
    /// </summary>
    /// <seealso cref="ServerRebuildRequest"/>
    /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/Rebuild_Server-d1e3538.html">Rebuild Server (OpenStack Compute API v2 and Extensions Reference)</seealso>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class ServerRebuildDetails
    {
        /// <summary>
        /// Gets the new name for the server.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; private set; }

        /// <summary>
        /// Gets the image to rebuild the server from. This is specified as an image ID (see <see cref="SimpleServerImage.Id"/>) or a full URL.
        /// </summary>
        [JsonProperty("imageRef")]
        public string ImageName { get; private set; }

        /// <summary>
        /// The new flavor for server. This is obtained from <see cref="net.openstack.Core.Domain.Flavor.Id"/>.
        /// </summary>
        [JsonProperty("flavorRef")]
        public string Flavor { get; private set; }

        /// <summary>
        /// The disk configuration. If the value is <see langword="null"/>, the default configuration for the specified image is used.
        /// </summary>
        [JsonProperty("OS-DCF:diskConfig", DefaultValueHandling = DefaultValueHandling.Include)]
        public DiskConfiguration DiskConfig { get; private set; }

        /// <summary>
        /// Gets the new admin password for the server.
        /// </summary>
        [JsonProperty("adminPass")]
        public string AdminPassword { get; private set; }

        /// <summary>
        /// Gets the list of metadata to associate with the server. If the value is <see langword="null"/>, the metadata associated with the server is not changed during the rebuild operation.
        /// </summary>
        [JsonProperty("metadata", DefaultValueHandling = DefaultValueHandling.Include)]
        public Dictionary<string, string> Metadata { get; private set; }

        /// <summary>
        /// The path and contents of a file to inject in the target file system during the rebuild operation. If the value is <see langword="null"/>, no file is injected.
        /// </summary>
        [JsonProperty("personality", DefaultValueHandling = DefaultValueHandling.Include)]
        public Personality Personality { get; private set; }

        /// <summary>
        /// The new IP v4 address for the server. If the value is <see langword="null"/>, the server's IP v4 address is not updated.
        /// </summary>
        [JsonProperty("accessIPv4", DefaultValueHandling = DefaultValueHandling.Include)]
        [JsonConverter(typeof(IPAddressNoneIsNullSimpleConverter))]
        public IPAddress AccessIPv4 { get; private set; }

        /// <summary>
        /// The new IP v6 address for the server. If the value is <see langword="null"/>, the server's IP v6 address is not updated.
        /// </summary>
        [JsonProperty("accessIPv6", DefaultValueHandling = DefaultValueHandling.Include)]
        [JsonConverter(typeof(IPAddressNoneIsNullSimpleConverter))]
        public IPAddress AccessIPv6 { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerRebuildDetails"/> class with the specified details.
        /// </summary>
        /// <param name="name">The new name for the server. If the value is <see langword="null"/>, the server name is not changed.</param>
        /// <param name="imageName">The image to rebuild the server from. This is specified as an image ID (see <see cref="SimpleServerImage.Id"/>) or a full URL.</param>
        /// <param name="flavor">The new flavor for server. This is obtained from <see cref="net.openstack.Core.Domain.Flavor.Id"/>.</param>
        /// <param name="adminPassword">The new admin password for the server.</param>
        /// <param name="accessIPv4">The new IP v4 address for the server, or <see cref="IPAddress.None"/> to remove the configured IP v4 address for the server. If the value is <see langword="null"/>, the server's IP v4 address is not updated.</param>
        /// <param name="accessIPv6">The new IP v6 address for the server, or <see cref="IPAddress.None"/> to remove the configured IP v6 address for the server. If the value is <see langword="null"/>, the server's IP v6 address is not updated.</param>
        /// <param name="metadata">The list of metadata to associate with the server. If the value is <see langword="null"/>, the metadata associated with the server is not changed during the rebuild operation.</param>
        /// <param name="diskConfig">The disk configuration. If the value is <see langword="null"/>, the default configuration for the specified image is used.</param>
        /// <param name="personality">The path and contents of a file to inject in the target file system during the rebuild operation. If the value is <see langword="null"/>, no file is injected.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="imageName"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="flavor"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="adminPassword"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="imageName"/> is empty.
        /// <para>-or-</para>
        /// <para>If <paramref name="flavor"/> is empty.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="adminPassword"/> is empty.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="accessIPv4"/> is not <see cref="IPAddress.None"/> and the <see cref="AddressFamily"/> of <paramref name="accessIPv4"/> is not <see cref="AddressFamily.InterNetwork"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="accessIPv6"/> is not <see cref="IPAddress.None"/> and the <see cref="AddressFamily"/> of <paramref name="accessIPv6"/> is not <see cref="AddressFamily.InterNetworkV6"/>.</para>
        /// </exception>
        public ServerRebuildDetails(string name, string imageName, string flavor, string adminPassword, IPAddress accessIPv4, IPAddress accessIPv6, Metadata metadata, DiskConfiguration diskConfig, Personality personality)
        {
            if (imageName == null)
                throw new ArgumentNullException("imageName");
            if (flavor == null)
                throw new ArgumentNullException("flavor");
            if (adminPassword == null)
                throw new ArgumentNullException("adminPassword");
            if (string.IsNullOrEmpty(imageName))
                throw new ArgumentException("imageName cannot be empty");
            if (string.IsNullOrEmpty(flavor))
                throw new ArgumentException("flavor cannot be empty");
            if (string.IsNullOrEmpty(adminPassword))
                throw new ArgumentException("adminPassword cannot be empty");
            if (accessIPv4 != null && !IPAddress.None.Equals(accessIPv4) && accessIPv4.AddressFamily != AddressFamily.InterNetwork)
                throw new ArgumentException("The specified value for accessIPv4 is not an IP v4 address.", "accessIPv4");
            if (accessIPv6 != null && !IPAddress.None.Equals(accessIPv6) && accessIPv6.AddressFamily != AddressFamily.InterNetworkV6)
                throw new ArgumentException("The specified value for accessIPv6 is not an IP v6 address.", "accessIPv6");

            Name = name;
            ImageName = imageName;
            Flavor = flavor;
            AdminPassword = adminPassword;
            AccessIPv4 = accessIPv4;
            AccessIPv6 = accessIPv6;
            Metadata = metadata;
            DiskConfig = diskConfig;
            Personality = personality;
        }
    }
}
