namespace net.openstack.Providers.Rackspace.Objects.Request
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using net.openstack.Core.Domain.Converters;
    using Newtonsoft.Json;

    /// <summary>
    /// This models the JSON request used for the Update Server request.
    /// </summary>
    /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/ServerUpdate.html">Update Server (OpenStack Compute API v2 and Extensions Reference)</seealso>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class UpdateServerRequest
    {
        /// <summary>
        /// Gets additional details about the updated server.
        /// </summary>
        [JsonProperty("server")]
        public ServerUpdateDetails Server { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateServerRequest"/> class
        /// with the specified name and access IP addresses.
        /// </summary>
        /// <param name="name">The new name for the server. If the value is <see langword="null"/>, the server name is not changed.</param>
        /// <param name="accessIPv4">The new IP v4 address for the server, or <see cref="IPAddress.None"/> to remove the configured IP v4 address for the server. If the value is <see langword="null"/>, the server's IP v4 address is not updated.</param>
        /// <param name="accessIPv6">The new IP v6 address for the server, or <see cref="IPAddress.None"/> to remove the configured IP v6 address for the server. If the value is <see langword="null"/>, the server's IP v6 address is not updated.</param>
        /// <exception cref="ArgumentException">
        /// <para>If <paramref name="accessIPv4"/> is not <see cref="IPAddress.None"/> and the <see cref="AddressFamily"/> of <paramref name="accessIPv4"/> is not <see cref="AddressFamily.InterNetwork"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="accessIPv6"/> is not <see cref="IPAddress.None"/> and the <see cref="AddressFamily"/> of <paramref name="accessIPv6"/> is not <see cref="AddressFamily.InterNetworkV6"/>.</para>
        /// </exception>
        public UpdateServerRequest(string name, IPAddress accessIPv4, IPAddress accessIPv6)
        {
            if (accessIPv4 != null && !IPAddress.None.Equals(accessIPv4) && accessIPv4.AddressFamily != AddressFamily.InterNetwork)
                throw new ArgumentException("The specified value for accessIPv4 is not an IP v4 address.", "accessIPv4");
            if (accessIPv6 != null && !IPAddress.None.Equals(accessIPv6) && accessIPv6.AddressFamily != AddressFamily.InterNetworkV6)
                throw new ArgumentException("The specified value for accessIPv6 is not an IP v6 address.", "accessIPv6");

            Server = new ServerUpdateDetails(name, accessIPv4, accessIPv6);
        }

        /// <summary>
        /// This models the JSON body containing details for the Update Server request.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        [JsonObject(MemberSerialization.OptIn)]
        public class ServerUpdateDetails
        {
            /// <summary>
            /// Gets the new name for the server, or <see langword="null"/> if the server's name should not be changed by the update.
            /// </summary>
            [JsonProperty("name", DefaultValueHandling = DefaultValueHandling.Include)]
            public string Name { get; private set; }

            /// <summary>
            /// Gets the IP v4 access address for the server, or <see langword="null"/> if the access address should not be changed by the update.
            /// </summary>
            [JsonProperty("accessIPv4", DefaultValueHandling = DefaultValueHandling.Include)]
            [JsonConverter(typeof(IPAddressNoneIsNullSimpleConverter))]
            public IPAddress AccessIPv4 { get; private set; }

            /// <summary>
            /// Gets the IP v6 access address for the server, or <see langword="null"/> if the access address should not be changed by the update.
            /// </summary>
            [JsonProperty("accessIPv6", DefaultValueHandling = DefaultValueHandling.Include)]
            [JsonConverter(typeof(IPAddressNoneIsNullSimpleConverter))]
            public IPAddress AccessIPv6 { get; private set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="ServerUpdateDetails"/> class
            /// with the specified name and access IP addresses.
            /// </summary>
            /// <param name="name">The new name for the server. If the value is <see langword="null"/>, the server name is not changed.</param>
            /// <param name="accessIPv4">The new IP v4 address for the server, or <see cref="IPAddress.None"/> to remove the configured IP v4 address for the server. If the value is <see langword="null"/>, the server's IP v4 address is not updated.</param>
            /// <param name="accessIPv6">The new IP v6 address for the server, or <see cref="IPAddress.None"/> to remove the configured IP v6 address for the server. If the value is <see langword="null"/>, the server's IP v6 address is not updated.</param>
            /// <exception cref="ArgumentException">
            /// <para>If <paramref name="accessIPv4"/> is not <see cref="IPAddress.None"/> and the <see cref="AddressFamily"/> of <paramref name="accessIPv4"/> is not <see cref="AddressFamily.InterNetwork"/>.</para>
            /// <para>-or-</para>
            /// <para>If <paramref name="accessIPv6"/> is not <see cref="IPAddress.None"/> and the <see cref="AddressFamily"/> of <paramref name="accessIPv6"/> is not <see cref="AddressFamily.InterNetworkV6"/>.</para>
            /// </exception>
            public ServerUpdateDetails(string name, IPAddress accessIPv4, IPAddress accessIPv6)
            {
                if (accessIPv4 != null && !IPAddress.None.Equals(accessIPv4) && accessIPv4.AddressFamily != AddressFamily.InterNetwork)
                    throw new ArgumentException("The specified value for accessIPv4 is not an IP v4 address.", "accessIPv4");
                if (accessIPv6 != null && !IPAddress.None.Equals(accessIPv6) && accessIPv6.AddressFamily != AddressFamily.InterNetworkV6)
                    throw new ArgumentException("The specified value for accessIPv6 is not an IP v6 address.", "accessIPv6");

                Name = name;
                AccessIPv4 = accessIPv4;
                AccessIPv6 = accessIPv6;
            }
        }
    }
}
