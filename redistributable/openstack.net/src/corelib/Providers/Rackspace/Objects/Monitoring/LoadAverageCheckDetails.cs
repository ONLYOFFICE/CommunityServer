namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using Newtonsoft.Json;

    /// <summary>
    /// This class represents the detailed configuration parameters for a
    /// <see cref="CheckTypeId.AgentLoadAverage"/> check.
    /// </summary>
    /// <seealso cref="CheckTypeId.AgentLoadAverage"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class LoadAverageCheckDetails : CheckDetails
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoadAverageCheckDetails"/> class.
        /// </summary>
        public LoadAverageCheckDetails()
        {
        }

        /// <inheritdoc/>
        /// <remarks>
        /// This class only supports <see cref="CheckTypeId.AgentLoadAverage"/> checks.
        /// </remarks>
        protected internal override bool SupportsCheckType(CheckTypeId checkTypeId)
        {
            return checkTypeId == CheckTypeId.AgentLoadAverage;
        }
    }
}
