namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System;
    using Newtonsoft.Json;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This class models the JSON representation of the data produced by testing a monitoring alarm.
    /// </summary>
    /// <seealso cref="IMonitoringService.TestAlarmAsync"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class AlarmData : ExtensibleJsonObject
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value {value}
        /// <summary>
        /// This is the backing field for the <see cref="Timestamp"/> property.
        /// </summary>
        [JsonProperty("timestamp")]
        private long? _timestamp;

        /// <summary>
        /// This is the backing field for the <see cref="State"/> property.
        /// </summary>
        [JsonProperty("state")]
        private AlarmState _state;

        /// <summary>
        /// This is the backing field for the <see cref="Status"/> property.
        /// </summary>
        [JsonProperty("status")]
        private string _status;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="AlarmData"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected AlarmData()
        {
        }

        /// <summary>
        /// Gets a timestamp indicating when this alarm data occurred.
        /// </summary>
        public DateTimeOffset? Timestamp
        {
            get
            {
                return DateTimeOffsetExtensions.ToDateTimeOffset(_timestamp);
            }
        }

        /// <summary>
        /// Gets the state of the alarm.
        /// </summary>
        public AlarmState State
        {
            get
            {
                return _state;
            }
        }

        /// <summary>
        /// Gets the status of the alarm.
        /// </summary>
        public string Status
        {
            get
            {
                return _status;
            }
        }
    }
}
