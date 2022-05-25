namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using Newtonsoft.Json;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This class models the JSON representation of an evaluated alarm example in the <see cref="IMonitoringService"/>.
    /// </summary>
    /// <seealso cref="IMonitoringService.EvaluateAlarmExampleAsync"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class BoundAlarmExample : ExtensibleJsonObject
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value {value}
        /// <summary>
        /// This is the backing field for the <see cref="BoundCriteria"/> property.
        /// </summary>
        [JsonProperty("bound_criteria")]
        private string _boundCriteria;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="BoundAlarmExample"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected BoundAlarmExample()
        {
        }

        /// <summary>
        /// Gets the evaluated alarm example criteria.
        /// </summary>
        /// <value>
        /// The evaluated alarm example criteria, or <see langword="null"/> if the JSON
        /// response from the server did not include the underlying property.
        /// </value>
        public string BoundCriteria
        {
            get
            {
                return _boundCriteria;
            }
        }
    }
}
