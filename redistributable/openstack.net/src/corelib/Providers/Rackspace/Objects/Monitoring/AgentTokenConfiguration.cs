namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using Newtonsoft.Json;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This class models the JSON representation of the basic properties of an Agent Token resource
    /// in the <see cref="IMonitoringService"/>.
    /// </summary>
    /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-agent-tokens.html">Agent Token (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class AgentTokenConfiguration : ExtensibleJsonObject
    {
        /// <summary>
        /// This is the backing field for the <see cref="Label"/> property.
        /// </summary>
        [JsonProperty("label")]
        private string _label;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentTokenConfiguration"/> class
        /// with no label.
        /// </summary>
        [JsonConstructor]
        public AgentTokenConfiguration()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentTokenConfiguration"/> class
        /// with the specified label.
        /// </summary>
        /// <param name="label">The token label.</param>
        public AgentTokenConfiguration(string label)
        {
            _label = label;
        }

        /// <summary>
        /// Gets the label for the agent token.
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
