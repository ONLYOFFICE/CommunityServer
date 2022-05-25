namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System;
    using Newtonsoft.Json;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This class models the JSON representation of agent host information reported by various API calls in the <see cref="IMonitoringService"/>.
    /// </summary>
    /// <typeparam name="T">The type modeling the JSON representation of the host information reported by the agent.</typeparam>
    /// <seealso cref="IMonitoringService.GetAgentHostInformationAsync"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class HostInformation<T> : ExtensibleJsonObject
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value {value}
        /// <summary>
        /// This is the backing field for the <see cref="Timestamp"/> property.
        /// </summary>
        [JsonProperty("timestamp")]
        private long? _timestamp;

        /// <summary>
        /// This is the backing field for the <see cref="Info"/> property.
        /// </summary>
        [JsonProperty("info")]
        private T _info;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="HostInformation{T}"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected HostInformation()
        {
        }

        /// <summary>
        /// Gets a timestamp indicating when the agent reported the information.
        /// </summary>
        public DateTimeOffset? Timestamp
        {
            get
            {
                return DateTimeOffsetExtensions.ToDateTimeOffset(_timestamp);
            }
        }

        /// <summary>
        /// Gets the information reported by the agent.
        /// </summary>
        public T Info
        {
            get
            {
                return _info;
            }
        }
    }
}
