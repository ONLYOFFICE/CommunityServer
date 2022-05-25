namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using Newtonsoft.Json;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This class models the JSON representation of an agent check target in the <see cref="IMonitoringService"/>.
    /// </summary>
    /// <remarks>
    /// Each agent check type gathers data for a related set of target devices on the server
    /// where the agent is installed. For example, <see cref="CheckTypeId.AgentNetwork"/>
    /// gathers data for network devices. The actual list of target devices is specific to
    /// the configuration of the host server. By focusing on specific targets, you can
    /// efficiently narrow the metric data that the agent gathers.
    /// </remarks>
    /// <seealso cref="IMonitoringService.ListAgentCheckTargetsAsync"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class CheckTarget : ExtensibleJsonObject
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value {value}
        /// <summary>
        /// This is the backing field for the <see cref="Id"/> property.
        /// </summary>
        [JsonProperty("id")]
        private CheckTargetId _id;

        /// <summary>
        /// This is the backing field for the <see cref="Label"/> property.
        /// </summary>
        [JsonProperty("label")]
        private string _label;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckTarget"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected CheckTarget()
        {
        }

        /// <summary>
        /// Gets the unique identifier for the agent check target.
        /// </summary>
        public CheckTargetId Id
        {
            get
            {
                return _id;
            }
        }

        /// <summary>
        /// Gets the label for the agent check target.
        /// </summary>
        public string Label
        {
            get
            {
                return _label;
            }
        }
    }
}
