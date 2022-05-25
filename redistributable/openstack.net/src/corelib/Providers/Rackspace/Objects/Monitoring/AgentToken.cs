namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using Newtonsoft.Json;

    /// <summary>
    /// Agent tokens are used to authenticate monitoring agents to the monitoring
    /// service. Multiple agents can share a single token.
    /// </summary>
    /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-agent-tokens.html">Agent Token (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class AgentToken : AgentTokenConfiguration
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value {value}
        /// <summary>
        /// This is the backing field for the <see cref="Id"/> property.
        /// </summary>
        [JsonProperty("id")]
        private AgentTokenId _id;

        /// <summary>
        /// This is the backing field for the <see cref="Token"/> property.
        /// </summary>
        [JsonProperty("token")]
        private string _token;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentToken"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected AgentToken()
        {
        }

        /// <summary>
        /// Gets the unique identifier of the agent token.
        /// </summary>
        public AgentTokenId Id
        {
            get
            {
                return _id;
            }
        }

        /// <summary>
        /// Gets the value of the agent token.
        /// </summary>
        public string Token
        {
            get
            {
                return _token;
            }
        }
    }
}
