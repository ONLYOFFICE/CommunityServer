namespace net.openstack.Providers.Rackspace.Objects.AutoScale
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This class represents the configuration for a scaling group in the <see cref="IAutoScaleService"/>.
    /// </summary>
    /// <remarks>
    /// The configuration options for the scaling group. The scaling group configuration specifies
    /// the basic elements of the Auto Scale configuration. It manages how many servers can
    /// participate in the scaling group. It specifies information related to load balancers.
    /// </remarks>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class GroupConfiguration : ExtensibleJsonObject
    {
        /// <summary>
        /// This is the backing field for the <see cref="Name"/> property.
        /// </summary>
        [JsonProperty("name")]
        private string _name;

        /// <summary>
        /// This is the backing field for the <see cref="Cooldown"/> property.
        /// </summary>
        [JsonProperty("cooldown")]
        private long? _cooldown;

        /// <summary>
        /// This is the backing field for the <see cref="MinEntities"/> property.
        /// </summary>
        [JsonProperty("minEntities")]
        private long? _minEntities;

        /// <summary>
        /// This is the backing field for the <see cref="MaxEntities"/> property.
        /// </summary>
        [JsonProperty("maxEntities", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private long? _maxEntities;

        /// <summary>
        /// This is the backing field for the <see cref="Metadata"/> property.
        /// </summary>
        [JsonProperty("metadata", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private JObject _metadata;

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupConfiguration"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected GroupConfiguration()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupConfiguration"/> class
        /// with the specified values.
        /// </summary>
        /// <param name="name">The name of the scaling group.</param>
        /// <param name="cooldown">The cooldown period of the scaling group.</param>
        /// <param name="minEntities">The minimum number of servers to include in the scaling group.</param>
        /// <param name="maxEntities">The maximum number of servers to include in the scaling group.</param>
        /// <param name="metadata">The metadata to associate with the scaling group.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="name"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="name"/> is empty.
        /// <para>-or-</para>
        /// <para>If <paramref name="maxEntities"/> is less than <paramref name="minEntities"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="cooldown"/> is less than <see cref="TimeSpan.Zero"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="minEntities"/> is less than 0.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="maxEntities"/> is less than 0.</para>
        /// </exception>
        public GroupConfiguration(string name, TimeSpan cooldown, int minEntities, int? maxEntities, JObject metadata)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name cannot be empty");
            if (cooldown < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException("cooldown");
            if (minEntities < 0)
                throw new ArgumentOutOfRangeException("minEntities");
            if (maxEntities < 0)
                throw new ArgumentOutOfRangeException("maxEntities");

            _name = name;
            _cooldown = (int)cooldown.TotalSeconds;
            _minEntities = minEntities;
            _maxEntities = maxEntities;
            _metadata = metadata;
        }

        /// <summary>
        /// Gets the name of the scaling group.
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
        }

        /// <summary>
        /// Gets the cooldown time of the scaling group, which specifies the time the group must wait
        /// after a scaling policy is triggered before another request for scaling is accepted.
        /// </summary>
        /// <remarks>
        /// Applies mainly to event-based policies.
        /// </remarks>
        public TimeSpan? Cooldown
        {
            get
            {
                if (_cooldown == null)
                    return null;

                return TimeSpan.FromSeconds(_cooldown.Value);
            }
        }

        /// <summary>
        /// Gets the minimum amount of entities that are allowed in this group. You cannot scale down
        /// below this value. Increasing this value can cause an immediate addition to the scaling
        /// group.
        /// </summary>
        public long? MinEntities
        {
            get
            {
                return _minEntities;
            }
        }

        /// <summary>
        /// Gets the maximum amount of entities that are allowed in this group. You cannot scale up
        /// above this value. Decreasing this value can cause an immediate reduction of the scaling
        /// group.
        /// </summary>
        public long? MaxEntities
        {
            get
            {
                return _maxEntities;
            }
        }

        /// <summary>
        /// Gets the metadata associated with the scaling group resource.
        /// </summary>
        public JObject Metadata
        {
            get
            {
                return _metadata;
            }
        }
    }
}
