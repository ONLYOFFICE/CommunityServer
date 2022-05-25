namespace net.openstack.Providers.Rackspace.Objects.AutoScale
{
    using System.Collections.ObjectModel;
    using Newtonsoft.Json;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;
    using Link = net.openstack.Core.Domain.Link;
    using ServerBase = net.openstack.Core.Domain.ServerBase;
    using ServerId = net.openstack.Core.Domain.ServerId;

    /// <summary>
    /// Represents an active server which is part of a scaling group in the <see cref="IAutoScaleService"/>.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class ActiveServer : ExtensibleJsonObject
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value {value}
        /// <summary>
        /// This is the backing field for the <see cref="Id"/> property.
        /// </summary>
        [JsonProperty("id", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private ServerId _id;

        /// <summary>
        /// This is the backing field for the <see cref="Links"/> property.
        /// </summary>
        [JsonProperty("links", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private Link[] _links;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="ActiveServer"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected ActiveServer()
        {
        }

        /// <summary>
        /// Gets the ID of the active server which is part of the scaling group.
        /// </summary>
        /// <see cref="ServerBase.Id"/>
        public ServerId Id
        {
            get
            {
                return _id;
            }
        }

        /// <summary>
        /// Gets a collection of links to resources related to this active server resource.
        /// </summary>
        public ReadOnlyCollection<Link> Links
        {
            get
            {
                if (_links == null)
                    return null;

                return new ReadOnlyCollection<Link>(_links);
            }
        }
    }
}
