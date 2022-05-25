namespace net.openstack.Providers.Rackspace.Objects.AutoScale
{
    using System.Collections.ObjectModel;
    using net.openstack.Core.Domain;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a scaling group in the <see cref="IAutoScaleService"/>.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class ScalingGroup : ScalingGroupConfiguration<Policy>
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value {value}
        /// <summary>
        /// This is the backing field for the <see cref="Id"/> property.
        /// </summary>
        [JsonProperty("id")]
        private ScalingGroupId _id;

        /// <summary>
        /// This is the backing field for the <see cref="Links"/> property.
        /// </summary>
        [JsonProperty("links")]
        private Link[] _links;

        /// <summary>
        /// This is the backing field for the <see cref="State"/> property.
        /// </summary>
        [JsonProperty("state")]
        private GroupState _state;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="ScalingGroup"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected ScalingGroup()
        {
        }

        /// <summary>
        /// Gets the unique identifier of the scaling group.
        /// </summary>
        public ScalingGroupId Id
        {
            get
            {
                return _id;
            }
        }

        /// <summary>
        /// Gets a collection of links to resources related to this scaling group resource.
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

        /// <summary>
        /// Gets a <see cref="GroupState"/> object describing the current state of the scaling group.
        /// </summary>
        public GroupState State
        {
            get
            {
                return _state;
            }
        }
    }
}
