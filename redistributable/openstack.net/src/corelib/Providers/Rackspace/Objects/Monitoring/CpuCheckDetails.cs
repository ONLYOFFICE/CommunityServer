namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using Newtonsoft.Json;

    /// <summary>
    /// This class represents the detailed configuration parameters for a
    /// <see cref="CheckTypeId.AgentCpu"/> check.
    /// </summary>
    /// <seealso cref="CheckTypeId.AgentCpu"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class CpuCheckDetails : CheckDetails
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CpuCheckDetails"/> class.
        /// </summary>
        public CpuCheckDetails()
        {
        }

        /// <inheritdoc/>
        /// <remarks>
        /// This class only supports <see cref="CheckTypeId.AgentCpu"/> checks.
        /// </remarks>
        protected internal override bool SupportsCheckType(CheckTypeId checkTypeId)
        {
            return checkTypeId == CheckTypeId.AgentCpu;
        }
    }
}
