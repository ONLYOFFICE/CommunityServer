namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// This class represents the detailed configuration parameters for a
    /// <see cref="CheckTypeId.RemotePing"/> check.
    /// </summary>
    /// <seealso cref="CheckTypeId.RemotePing"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class PingCheckDetails : CheckDetails
    {
        /// <summary>
        /// This is the backing field for the <see cref="Count"/> property.
        /// </summary>
        [JsonProperty("count", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private int? _count;

        /// <summary>
        /// Initializes a new instance of the <see cref="PingCheckDetails"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected PingCheckDetails()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PingCheckDetails"/> class
        /// with the specified count.
        /// </summary>
        /// <param name="count">The number of pings to send within a single check. If this value is <see langword="null"/>, a provider-specific default value is used.</param>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="count"/> is less than or equal to 0.</exception>
        public PingCheckDetails(int? count = null)
        {
            if (count <= 0)
                throw new ArgumentOutOfRangeException("count");

            _count = count;
        }

        /// <summary>
        /// Gets the number of pings to send within a single check.
        /// </summary>
        public int? Count
        {
            get
            {
                return _count;
            }
        }

        /// <inheritdoc/>
        /// <remarks>
        /// This class only supports <see cref="CheckTypeId.RemotePing"/> checks.
        /// </remarks>
        protected internal override bool SupportsCheckType(CheckTypeId checkTypeId)
        {
            return checkTypeId == CheckTypeId.RemotePing;
        }
    }
}
