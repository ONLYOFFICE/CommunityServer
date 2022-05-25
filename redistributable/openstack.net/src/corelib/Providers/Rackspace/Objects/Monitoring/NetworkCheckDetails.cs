namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// This class represents the detailed configuration parameters for a
    /// <see cref="CheckTypeId.AgentNetwork"/> check.
    /// </summary>
    /// <seealso cref="CheckTypeId.AgentNetwork"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class NetworkCheckDetails : CheckDetails
    {
        /// <summary>
        /// This is the backing field for the <see cref="Target"/> property.
        /// </summary>
        [JsonProperty("target")]
        private string _target;

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkCheckDetails"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected NetworkCheckDetails()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkCheckDetails"/> class
        /// with the specified target.
        /// </summary>
        /// <param name="target">The network device to check.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="target"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="target"/> is empty.</exception>
        public NetworkCheckDetails(string target)
        {
            if (target == null)
                throw new ArgumentNullException("target");
            if (string.IsNullOrEmpty(target))
                throw new ArgumentException("target cannot be empty");

            _target = target;
        }

        /// <summary>
        /// Gets the network device to check.
        /// </summary>
        public string Target
        {
            get
            {
                return _target;
            }
        }

        /// <inheritdoc/>
        /// <remarks>
        /// This class only supports <see cref="CheckTypeId.AgentNetwork"/> checks.
        /// </remarks>
        protected internal override bool SupportsCheckType(CheckTypeId checkTypeId)
        {
            return checkTypeId == CheckTypeId.AgentNetwork;
        }
    }
}
