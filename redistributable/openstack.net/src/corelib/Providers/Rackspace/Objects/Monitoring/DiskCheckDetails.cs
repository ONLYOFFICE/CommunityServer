namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// This class represents the detailed configuration parameters for a
    /// <see cref="CheckTypeId.AgentDisk"/> check.
    /// </summary>
    /// <seealso cref="CheckTypeId.AgentDisk"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class DiskCheckDetails : CheckDetails
    {
        /// <summary>
        /// This is the backing field for the <see cref="Target"/> property.
        /// </summary>
        [JsonProperty("target")]
        private string _target;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiskCheckDetails"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected DiskCheckDetails()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiskCheckDetails"/> class
        /// with the specified target.
        /// </summary>
        /// <param name="target">The disk to check.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="target"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="target"/> is empty.</exception>
        public DiskCheckDetails(string target)
        {
            if (target == null)
                throw new ArgumentNullException("target");
            if (string.IsNullOrEmpty(target))
                throw new ArgumentException("target cannot be empty");

            _target = target;
        }

        /// <summary>
        /// Gets the disk to check.
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
        /// This class only supports <see cref="CheckTypeId.AgentDisk"/> checks.
        /// </remarks>
        protected internal override bool SupportsCheckType(CheckTypeId checkTypeId)
        {
            return checkTypeId == CheckTypeId.AgentDisk;
        }
    }
}
