namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using Newtonsoft.Json;

    /// <summary>
    /// This class represents an account in the <see cref="IMonitoringService"/>.
    /// </summary>
    /// <remarks>
    /// An account contains attributes describing a customer's account. This description
    /// contains mostly read only data; however, a few properties can be modified with the API.
    /// </remarks>
    /// <see href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-account.html">Account (Rackspace Cloud Monitoring Developer Guide - API v1.0)</see>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class MonitoringAccount : AccountConfiguration
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value {value}
        /// <summary>
        /// This is the backing field for the <see cref="Id"/> property.
        /// </summary>
        [JsonProperty("id")]
        private MonitoringAccountId _id;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="MonitoringAccount"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected MonitoringAccount()
        {
        }

        /// <summary>
        /// Gets the unique identifier for the monitoring account.
        /// </summary>
        public MonitoringAccountId Id
        {
            get
            {
                return _id;
            }
        }
    }
}
