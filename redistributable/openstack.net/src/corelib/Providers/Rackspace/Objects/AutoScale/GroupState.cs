namespace net.openstack.Providers.Rackspace.Objects.AutoScale
{
    using System.Collections.ObjectModel;
    using Newtonsoft.Json;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;
    using ServerState = net.openstack.Core.Domain.ServerState;

    /// <summary>
    /// This class represents the current state of a scaling group in the <see cref="IAutoScaleService"/>.
    /// </summary>
    /// <seealso cref="ScalingGroup.State"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class GroupState : ExtensibleJsonObject
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value {value}
        /// <summary>
        /// This is the backing field for the <see cref="Name"/> property.
        /// </summary>
        [JsonProperty("name")]
        private string _name;

        /// <summary>
        /// This is the backing field for the <see cref="Paused"/> property.
        /// </summary>
        [JsonProperty("paused")]
        private bool? _paused;

        /// <summary>
        /// This is the backing field for the <see cref="ActiveCapacity"/> property.
        /// </summary>
        [JsonProperty("activeCapacity")]
        private long? _activeCapacity;

        /// <summary>
        /// This is the backing field for the <see cref="DesiredCapacity"/> property.
        /// </summary>
        [JsonProperty("desiredCapacity")]
        private long? _desiredCapacity;

        /// <summary>
        /// This is the backing field for the <see cref="PendingCapacity"/> property.
        /// </summary>
        [JsonProperty("pendingCapacity")]
        private long? _pendingCapacity;

        /// <summary>
        /// This is the backing field for the <see cref="Active"/> property.
        /// </summary>
        [JsonProperty("active")]
        private ActiveServer[] _active;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupState"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected GroupState()
        {
        }

        /// <summary>
        /// Gets the name of the Auto Scale group.
        /// </summary>
        /// <seealso cref="GroupConfiguration.Name"/>
        public string Name
        {
            get
            {
                return _name;
            }
        }

        /// <summary>
        /// Gets a value indicating whether execution of the scaling policies in the Auto Scale
        /// group is currently suspended.
        /// </summary>
        /// <remarks>
        /// If this value is <see langword="true"/>, the group will not scale up or down. All
        /// policy execution calls will be ignored while this value is set to <see langword="true"/>.
        /// </remarks>
        public bool? Paused
        {
            get
            {
                return _paused;
            }
        }

        /// <summary>
        /// Gets the number of servers in the group which completed the build process
        /// and are now considered active.
        /// </summary>
        public long? ActiveCapacity
        {
            get
            {
                return _activeCapacity;
            }
        }

        /// <summary>
        /// Gets the desired number of resources in the scaling group. This property is
        /// the sum of the <see cref="ActiveCapacity"/> and <see cref="PendingCapacity"/>.
        /// </summary>
        public long? DesiredCapacity
        {
            get
            {
                return _desiredCapacity;
            }
        }

        /// <summary>
        /// Gets the number of servers currently in the <see cref="ServerState.Build"/> state.
        /// </summary>
        public long? PendingCapacity
        {
            get
            {
                return _pendingCapacity;
            }
        }

        /// <summary>
        /// Gets a collection of <see cref="ActiveServer"/> objects describing the servers
        /// in the scaling group.
        /// </summary>
        public ReadOnlyCollection<ActiveServer> Active
        {
            get
            {
                if (_active == null)
                    return null;

                return new ReadOnlyCollection<ActiveServer>(_active);
            }
        }
    }
}
