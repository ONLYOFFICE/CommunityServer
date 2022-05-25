namespace net.openstack.Providers.Rackspace.Objects.AutoScale
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using DiskConfiguration = net.openstack.Core.Domain.DiskConfiguration;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;
    using Flavor = net.openstack.Core.Domain.Flavor;
    using FlavorId = net.openstack.Core.Domain.FlavorId;
    using ImageId = net.openstack.Core.Domain.ImageId;
    using Personality = net.openstack.Core.Domain.Personality;
    using SimpleServerImage = net.openstack.Core.Domain.SimpleServerImage;

    /// <summary>
    /// This class models the JSON representation of the arguments for creating new servers
    /// as part of the launch configuration in the <see cref="IAutoScaleService"/>.
    /// </summary>
    /// <seealso cref="ServerLaunchArguments.Server"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class ServerArgument : ExtensibleJsonObject
    {
        /// <summary>
        /// This is the backing field for the <see cref="FlavorId"/> property.
        /// </summary>
        [JsonProperty("flavorRef", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private FlavorId _flavorId;

        /// <summary>
        /// This is the backing field for the <see cref="ImageId"/> property.
        /// </summary>
        [JsonProperty("imageRef", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private ImageId _imageId;

        /// <summary>
        /// This is the backing field for the <see cref="Name"/> property.
        /// </summary>
        [JsonProperty("name", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private string _name;

        /// <summary>
        /// This is the backing field for the <see cref="DiskConfiguration"/> property.
        /// </summary>
        [JsonProperty("OS-DCF:diskConfig", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private DiskConfiguration _diskConfiguration;

        /// <summary>
        /// This is the backing field for the <see cref="Metadata"/> property.
        /// </summary>
        [JsonProperty("metadata", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private JObject _metadata;

        /// <summary>
        /// This is the backing field for the <see cref="Networks"/> property.
        /// </summary>
        [JsonProperty("networks", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private ServerNetworkArgument[] _networks;

        /// <summary>
        /// This is the backing field for the <see cref="Personality"/> property.
        /// </summary>
        [JsonProperty("personality", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private Personality[] _personality;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerArgument"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected ServerArgument()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerArgument"/> class
        /// with the specified values.
        /// </summary>
        /// <param name="flavorId">The ID of the flavor to use when creating new servers. See <see cref="Flavor.Id"/>.</param>
        /// <param name="imageId">The ID of the image to use when creating new servers. See <see cref="SimpleServerImage.Id"/>.</param>
        /// <param name="name">The prefix to use when assigning names to new servers.</param>
        /// <param name="diskConfiguration">The disk configuration to use for new servers.</param>
        /// <param name="metadata">The metadata to associate with the server argument.</param>
        /// <param name="networks">A collection of <see cref="ServerNetworkArgument"/> objects describing the networks to initially connect newly created servers to.</param>
        /// <param name="personality">A collection of <see cref="Personality"/> objects describing the personality for new server instances.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="flavorId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="imageId"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="networks"/> contains any <see langword="null"/> values.
        /// <para>-or-</para>
        /// <para>If <paramref name="personality"/> contains any <see langword="null"/> values.</para>
        /// </exception>
        public ServerArgument(FlavorId flavorId, ImageId imageId, string name, DiskConfiguration diskConfiguration, JObject metadata, IEnumerable<ServerNetworkArgument> networks, IEnumerable<Personality> personality)
        {
            if (flavorId == null)
                throw new ArgumentNullException("flavorId");
            if (imageId == null)
                throw new ArgumentNullException("imageId");

            _flavorId = flavorId;
            _imageId = imageId;
            _name = name;
            _diskConfiguration = diskConfiguration;
            _metadata = metadata;

            if (networks != null)
            {
                _networks = networks.ToArray();
                if (_networks.Contains(null))
                    throw new ArgumentException("networks cannot contain any null values", "networks");
            }

            if (personality != null)
            {
                _personality = personality.ToArray();
                if (_personality.Contains(null))
                    throw new ArgumentException("personality cannot contain any null values", "personality");
            }
        }

        /// <summary>
        /// Gets the ID of the flavor to use when creating new servers.
        /// </summary>
        public FlavorId FlavorId
        {
            get
            {
                return _flavorId;
            }
        }

        /// <summary>
        /// Gets the ID of the image to use when creating new servers.
        /// </summary>
        public ImageId ImageId
        {
            get
            {
                return _imageId;
            }
        }

        /// <summary>
        /// Gets the prefix to use for the server name when creating new servers.
        /// </summary>
        /// <remarks>
        /// The final name assigned to servers created by the Auto Scale service will
        /// be a combination of this value and a unique string generated by the Auto Scale
        /// service.
        /// </remarks>
        public string Name
        {
            get
            {
                return _name;
            }
        }

        /// <summary>
        /// Gets the disk configuration, which specifies how new servers are partitioned.
        /// </summary>
        public DiskConfiguration DiskConfiguration
        {
            get
            {
                return _diskConfiguration;
            }
        }

        /// <summary>
        /// Gets the metadata associated with the server argument resource.
        /// </summary>
        public JObject Metadata
        {
            get
            {
                return _metadata;
            }
        }

        /// <summary>
        /// Gets a collection of <see cref="ServerNetworkArgument"/> objects describing the networks
        /// to initially connect newly created servers to.
        /// </summary>
        public ReadOnlyCollection<ServerNetworkArgument> Networks
        {
            get
            {
                if (_networks == null)
                    return null;

                return new ReadOnlyCollection<ServerNetworkArgument>(_networks);
            }
        }

        /// <summary>
        /// Gets a collection of <see cref="Personality"/> objects describing the personality
        /// for new server instances.
        /// </summary>
        public ReadOnlyCollection<Personality> Personality
        {
            get
            {
                if (_personality == null)
                    return null;

                return new ReadOnlyCollection<Personality>(_personality);
            }
        }
    }
}
