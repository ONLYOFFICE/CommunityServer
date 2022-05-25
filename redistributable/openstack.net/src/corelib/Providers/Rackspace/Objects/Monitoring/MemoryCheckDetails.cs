namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using Newtonsoft.Json;

    /// <summary>
    /// This class represents the detailed configuration parameters for a
    /// <see cref="CheckTypeId.AgentMemory"/> check.
    /// </summary>
    /// <seealso cref="CheckTypeId.AgentMemory"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class MemoryCheckDetails : CheckDetails
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryCheckDetails"/> class.
        /// </summary>
        public MemoryCheckDetails()
        {
        }

        /// <inheritdoc/>
        /// <remarks>
        /// This class only supports <see cref="CheckTypeId.AgentMemory"/> checks.
        /// </remarks>
        protected internal override bool SupportsCheckType(CheckTypeId checkTypeId)
        {
            return checkTypeId == CheckTypeId.AgentMemory;
        }
    }
}
