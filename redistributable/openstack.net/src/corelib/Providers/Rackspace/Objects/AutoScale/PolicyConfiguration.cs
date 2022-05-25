namespace net.openstack.Providers.Rackspace.Objects.AutoScale
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This class models the JSON representation of the basic configuration of a scaling policy
    /// in the <see cref="IAutoScaleService"/>.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class PolicyConfiguration : ExtensibleJsonObject
    {
        /// <summary>
        /// This is the backing field for the <see cref="Name"/> property.
        /// </summary>
        [JsonProperty("name")]
        private string _name;

        /// <summary>
        /// This is the backing field for the <see cref="DesiredCapacity"/> property.
        /// </summary>
        [JsonProperty("desiredCapacity", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private long? _desiredCapacity;

        /// <summary>
        /// This is the backing field for the <see cref="Cooldown"/> property.
        /// </summary>
        [JsonProperty("cooldown")]
        private long? _cooldown;

        /// <summary>
        /// This is the backing field for the <see cref="Change"/> property.
        /// </summary>
        [JsonProperty("change", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private long? _change;

        /// <summary>
        /// This is the backing field for the <see cref="ChangePercent"/> property.
        /// </summary>
        [JsonProperty("changePercent", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private double? _changePercent;

        /// <summary>
        /// This is the backing field for the <see cref="Arguments"/> property.
        /// </summary>
        [JsonProperty("args", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private JObject _args;

        /// <summary>
        /// This is the backing field for the <see cref="Type"/> property.
        /// </summary>
        [JsonProperty("type")]
        private PolicyType _type;

        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyConfiguration"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected PolicyConfiguration()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyConfiguration"/> class
        /// with the specified values.
        /// </summary>
        /// <remarks>
        /// To create scaling policy for one of the predefined policy types, use
        /// one of the following factory methods.
        /// <list type="bullet">
        /// <item><see cref="Capacity"/></item>
        /// <item><see cref="IncrementalChange"/></item>
        /// <item><see cref="PercentageChange"/></item>
        /// <item><see cref="PercentageChangeAtTime"/></item>
        /// </list>
        /// </remarks>
        /// <param name="name">The name of the scaling policy.</param>
        /// <param name="type">The scaling policy type.</param>
        /// <param name="desiredCapacity">The desired capacity of the scaling policy.</param>
        /// <param name="cooldown">The cooldown of the scaling policy.</param>
        /// <param name="change">The incremental change for the scaling policy.</param>
        /// <param name="changePercent">The percentage change for the scaling policy.</param>
        /// <param name="arguments">An object modeling the additional arguments to associate with the scaling policy.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="name"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="type"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">If <paramref name="name"/> is empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="desiredCapacity"/> is less than 0.
        /// <para>-or-</para>
        /// <para>If <paramref name="cooldown"/> is less than <see cref="TimeSpan.Zero"/>.</para>
        /// </exception>
        protected PolicyConfiguration(string name, PolicyType type, long? desiredCapacity, TimeSpan? cooldown, long? change, double? changePercent, object arguments)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (type == null)
                throw new ArgumentNullException("type");
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name cannot be empty");
            if (desiredCapacity < 0)
                throw new ArgumentOutOfRangeException("desiredCapacity");
            if (cooldown < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException("cooldown");

            _name = name;
            _type = type;
            _desiredCapacity = desiredCapacity;
            _cooldown = cooldown != null ? (long?)cooldown.Value.TotalSeconds : null;
            _change = change;
            _changePercent = changePercent;
            _args = arguments != null ? JObject.FromObject(arguments) : null;
        }

        /// <summary>
        /// Gets the name of the scaling policy.
        /// </summary>
        /// <remarks>
        /// The <see cref="Name"/> must be unique for each scaling policy.
        /// </remarks>
        public string Name
        {
            get
            {
                return _name;
            }
        }

        /// <summary>
        /// Gets the desired capacity of the scaling policy.
        /// </summary>
        /// <remarks>
        /// Specifies the final capacity that is desired by the scale up event. Note that this value
        /// is always rounded up. Use to specify a number of servers for the policy to implement - by
        /// either adding or removing servers as needed.
        /// </remarks>
        public long? DesiredCapacity
        {
            get
            {
                return _desiredCapacity;
            }
        }

        /// <summary>
        /// Gets the cooldown period of the scaling policy.
        /// </summary>
        /// <remarks>
        /// The cooldown period prohibits the execution of this specific policy until the configured
        /// cooldown time period has passed. Helps prevent an event triggering a policy and can help
        /// to ensure that servers can be added quickly (short cooldown) and removed gradually (long
        /// cooldown).
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
        /// Gets the incremental change for the scaling policy.
        /// </summary>
        /// <remarks>
        /// Specifies the number of entities to add or remove, for example "1" implies that 1 server
        /// needs to be added. Use to change the number of servers to a specific number. If a positive
        /// number is used, servers are added; if a negative number is used, servers are removed.
        /// </remarks>
        public long? Change
        {
            get
            {
                return _change;
            }
        }

        /// <summary>
        /// Gets the percentage change for the scaling policy.
        /// </summary>
        /// <remarks>
        /// Use to change the percentage of servers relative to the current number of servers. If a positive
        /// number is used, servers are added; if a negative number is used, servers are removed. The absolute
        /// change in the number of servers is always rounded up. For example, if -X% of the current number
        /// of servers translates to -0.5 or -0.25 or -0.75 servers, the actual number of servers that will
        /// be shut down is 1.
        /// </remarks>
        public double? ChangePercent
        {
            get
            {
                return _changePercent;
            }
        }

        /// <summary>
        /// Gets the arguments for the scaling policy.
        /// </summary>
        public JObject Arguments
        {
            get
            {
                return _args;
            }
        }

        /// <summary>
        /// Gets the scaling policy type.
        /// </summary>
        public PolicyType PolicyType
        {
            get
            {
                return _type;
            }
        }

        /// <summary>
        /// Creates a new <see cref="PolicyConfiguration"/> for a scaling policy that sets the
        /// desired capacity of a scaling group to a specific value.
        /// </summary>
        /// <param name="name">The name of scaling policy.</param>
        /// <param name="desiredCapacity">The desired capacity for the scaling group.</param>
        /// <param name="cooldown">The cooldown period for the scaling policy.</param>
        /// <returns>A <see cref="PolicyConfiguration"/> representing the desired scaling policy configuration.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="name"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="name"/> is empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="desiredCapacity"/> is less than 0.
        /// <para>-or-</para>
        /// <para>If <paramref name="cooldown"/> is less than <see cref="TimeSpan.Zero"/>.</para>
        /// </exception>
        public static PolicyConfiguration Capacity(string name, int desiredCapacity, TimeSpan cooldown)
        {
            return new PolicyConfiguration(name, PolicyType.Webhook, desiredCapacity, cooldown, null, null, null);
        }

        /// <summary>
        /// Creates a new <see cref="PolicyConfiguration"/> for a scaling policy that changes the
        /// desired capacity of a scaling group by a fixed amount.
        /// </summary>
        /// <param name="name">The name of scaling policy.</param>
        /// <param name="change">The change to apply to the desired capacity for the scaling group.</param>
        /// <param name="cooldown">The cooldown period for the scaling policy.</param>
        /// <returns>A <see cref="PolicyConfiguration"/> representing the desired scaling policy configuration.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="name"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="name"/> is empty.
        /// <para>-or-</para>
        /// <para>If <paramref name="change"/> is 0.</para>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="cooldown"/> is less than <see cref="TimeSpan.Zero"/>.</exception>
        public static PolicyConfiguration IncrementalChange(string name, int change, TimeSpan cooldown)
        {
            if (change == 0)
                throw new ArgumentException("change cannot be 0", "change");

            return new PolicyConfiguration(name, PolicyType.Webhook, null, cooldown, change, null, null);
        }

        /// <summary>
        /// Creates a new <see cref="PolicyConfiguration"/> for a scaling policy that changes the
        /// desired capacity of a scaling group by a percentage amount.
        /// </summary>
        /// <param name="name">The name of scaling policy.</param>
        /// <param name="changePercentage">The percentage change to apply to the desired capacity for the scaling group.</param>
        /// <param name="cooldown">The cooldown period for the scaling policy.</param>
        /// <returns>A <see cref="PolicyConfiguration"/> representing the desired scaling policy configuration.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="name"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="name"/> is empty.
        /// <para>-or-</para>
        /// <para>If <paramref name="changePercentage"/> is 0.</para>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="cooldown"/> is less than <see cref="TimeSpan.Zero"/>.</exception>
        public static PolicyConfiguration PercentageChange(string name, double changePercentage, TimeSpan cooldown)
        {
            if (changePercentage == 0)
                throw new ArgumentException("changePercentage cannot be 0", "changePercentage");

            return new PolicyConfiguration(name, PolicyType.Webhook, null, cooldown, null, changePercentage, null);
        }

        /// <summary>
        /// Creates a new <see cref="PolicyConfiguration"/> for a scaling policy that changes the
        /// desired capacity of a scaling group by a percentage amount at the specified time.
        /// </summary>
        /// <param name="name">The name of scaling policy.</param>
        /// <param name="changePercentage">The percentage change to apply to the desired capacity for the scaling group.</param>
        /// <param name="cooldown">The cooldown period for the scaling policy.</param>
        /// <param name="time">The time at which to apply the change.</param>
        /// <returns>A <see cref="PolicyConfiguration"/> representing the desired scaling policy configuration.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="name"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="name"/> is empty.
        /// <para>-or-</para>
        /// <para>If <paramref name="changePercentage"/> is 0.</para>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="cooldown"/> is less than <see cref="TimeSpan.Zero"/>.</exception>
        public static PolicyConfiguration PercentageChangeAtTime(string name, double changePercentage, TimeSpan cooldown, DateTimeOffset time)
        {
            if (changePercentage == 0)
                throw new ArgumentException("changePercentage cannot be 0", "changePercentage");

            const string timeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'";
            string serializedTime = time.ToUniversalTime().ToString(timeFormat);

            JObject arguments = new JObject(
                new JProperty("at", JValue.CreateString(serializedTime)));

            return new PolicyConfiguration(name, PolicyType.Schedule, null, cooldown, null, changePercentage, arguments);
        }
    }
}
