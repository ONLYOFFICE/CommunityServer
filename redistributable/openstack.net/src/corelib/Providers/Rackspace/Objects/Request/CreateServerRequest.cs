namespace net.openstack.Providers.Rackspace.Objects.Request
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using net.openstack.Core.Domain;
    using Newtonsoft.Json;

    /// <summary>
    /// This models the JSON request used for the Create Server request.
    /// </summary>
    /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/CreateServers.html">Create Server (OpenStack Compute API v2 and Extensions Reference)</seealso>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class CreateServerRequest
    {
        /// <summary>
        /// Gets additional details about the Create Server request.
        /// </summary>
        [JsonProperty("server")]
        public CreateServerDetails Details { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateServerRequest"/> class
        /// with the specified details.
        /// </summary>
        /// <param name="name">Name of the new server.</param>
        /// <param name="imageName">The image to use for the new server instance. This is
        /// specified as an image ID (see <see cref="SimpleServerImage.Id"/>) or a full URL.</param>
        /// <param name="flavor">The flavor to use for the new server instance. This
        /// is specified as a flavor ID (see <see cref="Flavor.Id"/>) or a full URL.</param>
        /// <param name="diskConfig">The disk configuration. If the value is <see langword="null"/>, the default configuration for the specified image is used.</param>
        /// <param name="metadata">The metadata to associate with the server.</param>
        /// <param name="personality">A collection of <see cref="Personality"/> objects describing the paths and contents of files to inject in the target file system during the creation process. If the value is <see langword="null"/>, no files are injected.</param>
        /// <param name="accessIPv4">The behavior of this value is unspecified. Do not use.</param>
        /// <param name="accessIPv6">The behavior of this value is unspecified. Do not use.</param>
        /// <param name="networks">A collection of identifiers for networks to initially connect to the server. These are obtained from <see cref="CloudNetwork.Id">CloudNetwork.Id</see></param>
        public CreateServerRequest(string name, string imageName, string flavor, DiskConfiguration diskConfig, Dictionary<string, string> metadata, string accessIPv4, string accessIPv6, IEnumerable<string> networks, IEnumerable<Personality> personality)
        {
            Details = new CreateServerDetails(name, imageName, flavor, diskConfig, metadata, accessIPv4, accessIPv6, networks, personality);
        }

        /// <summary>
        /// This models the JSON body containing details for a Create Server request.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        [JsonObject(MemberSerialization.OptIn)]
        public class CreateServerDetails
        {
            /// <summary>
            /// Gets the name of the new server to create.
            /// </summary>
            [JsonProperty("name")]
            public string Name { get; private set; }

            /// <summary>
            /// Gets the image to use for the new server instance. This is
            /// specified as an image ID (see <see cref="SimpleServerImage.Id"/>) or a full URL.
            /// </summary>
            [JsonProperty("imageRef")]
            public string ImageName { get; private set; }

            /// <summary>
            /// Gets the flavor to use for the new server instance. This
            /// is specified as a flavor ID (see <see cref="net.openstack.Core.Domain.Flavor.Id"/>) or a full URL.
            /// </summary>
            [JsonProperty("flavorRef")]
            public string Flavor { get; private set; }

            /// <summary>
            /// Gets the disk configuration. If the value is <see langword="null"/>, the default configuration for the specified image is used.
            /// </summary>
            [JsonProperty("OS-DCF:diskConfig")]
            public DiskConfiguration DiskConfig { get; private set; }

            /// <summary>
            /// Gets the metadata to associate with the server.
            /// </summary>
            [JsonProperty("metadata", DefaultValueHandling = DefaultValueHandling.Include)]
            public Dictionary<string, string> Metadata { get; private set; }

            /// <summary>
            /// The behavior of this value is unspecified. Do not use.
            /// </summary>
            [JsonProperty("accessIPv4", DefaultValueHandling = DefaultValueHandling.Include)]
            public string AccessIPv4 { get; private set; }

            /// <summary>
            /// The behavior of this value is unspecified. Do not use.
            /// </summary>
            [JsonProperty("accessIPv6", DefaultValueHandling = DefaultValueHandling.Include)]
            public string AccessIPv6 { get; private set; }

            /// <summary>
            /// Gets a collection of information about networks to initially connect to the server.
            /// </summary>
            [JsonProperty("networks", DefaultValueHandling = DefaultValueHandling.Include)]
            public NewServerNetwork[] Networks { get; private set; }

            /// <summary>
            /// Gets a collection of <see cref="Personality"/> objects describing the paths and
            /// contents of files to inject in the target file system during the creation process.
            /// If the value is <see langword="null"/>, no files are injected.
            /// </summary>
            [JsonProperty("personality", DefaultValueHandling = DefaultValueHandling.Include)]
            public Personality[] Personality { get; private set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="CreateServerDetails"/> class
            /// with the specified details.
            /// </summary>
            /// <param name="name">Name of the new server.</param>
            /// <param name="imageName">The image to use for the new server instance. This is
            /// specified as an image ID (see <see cref="SimpleServerImage.Id"/>) or a full URL.</param>
            /// <param name="flavor">The flavor to use for the new server instance. This
            /// is specified as a flavor ID (see <see cref="net.openstack.Core.Domain.Flavor.Id"/>) or a full URL.</param>
            /// <param name="diskConfig">The disk configuration. If the value is <see langword="null"/>, the default configuration for the specified image is used.</param>
            /// <param name="metadata">The metadata to associate with the server.</param>
            /// <param name="personality">A collection of <see cref="Personality"/> objects describing the paths and contents of files to inject in the target file system during the creation process. If the value is <see langword="null"/>, no files are injected.</param>
            /// <param name="accessIPv4">The behavior of this value is unspecified. Do not use.</param>
            /// <param name="accessIPv6">The behavior of this value is unspecified. Do not use.</param>
            /// <param name="networks">A collection of identifiers for networks to initially connect to the server. These are obtained from <see cref="CloudNetwork.Id">CloudNetwork.Id</see></param>
            public CreateServerDetails(string name, string imageName, string flavor, DiskConfiguration diskConfig, Dictionary<string, string> metadata, string accessIPv4, string accessIPv6, IEnumerable<string> networks, IEnumerable<Personality> personality)
            {
                Name = name;
                ImageName = imageName;
                Flavor = flavor;
                DiskConfig = diskConfig;
                Metadata = metadata;
                AccessIPv4 = accessIPv4;
                AccessIPv6 = accessIPv6;
                Networks = networks.Select(i => new NewServerNetwork(i)).ToArray();
                Personality = personality != null ? personality.ToArray() : null;
            }

            /// <summary>
            /// This models the JSON body containing details for a connected network
            /// within the Create Server request.
            /// </summary>
            /// <threadsafety static="true" instance="false"/>
            [JsonObject(MemberSerialization.OptIn)]
            public class NewServerNetwork
            {
                /// <summary>
                /// Gets the ID of the network.
                /// </summary>
                /// <seealso cref="CloudNetwork.Id"/>
                [JsonProperty("uuid")]
                public string NetworkId { get; private set; }

                /// <summary>
                /// Initializes a new instance of the <see cref="NewServerNetwork"/> class
                /// with the specified ID.
                /// </summary>
                /// <param name="networkId">The network ID. This is obtained from <see cref="CloudNetwork.Id">CloudNetwork.Id</see>.</param>
                public NewServerNetwork(string networkId)
                {
                    if (networkId == null)
                        throw new ArgumentNullException("networkId");

                    NetworkId = networkId;
                }
            }
        }
    }
}
