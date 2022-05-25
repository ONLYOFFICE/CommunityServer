namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System;
    using Newtonsoft.Json;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This class models the JSON representation the record of an alarm state change
    /// in the entity overview.
    /// </summary>
    /// <seealso cref="EntityOverview.LatestAlarmStates"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class AlarmStateHistory : ExtensibleJsonObject
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value {value}
        /// <summary>
        /// This is the backing field for the <see cref="Timestamp"/> property.
        /// </summary>
        [JsonProperty("timestamp", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private long? _timestamp;

        /// <summary>
        /// This is the backing field for the <see cref="EntityId"/> property.
        /// </summary>
        [JsonProperty("entity_id", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private EntityId _entityId;

        /// <summary>
        /// This is the backing field for the <see cref="AlarmId"/> property.
        /// </summary>
        [JsonProperty("alarm_id", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private AlarmId _alarmId;

        /// <summary>
        /// This is the backing field for the <see cref="CheckId"/> property.
        /// </summary>
        [JsonProperty("check_id", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private CheckId _checkId;

        /// <summary>
        /// This is the backing field for the <see cref="Status"/> property.
        /// </summary>
        [JsonProperty("status")]
        private string _status;

        /// <summary>
        /// This is the backing field for the <see cref="State"/> property.
        /// </summary>
        [JsonProperty("state")]
        private AlarmState _state;

        /// <summary>
        /// This is the backing field for the <see cref="PreviousState"/> property.
        /// </summary>
        [JsonProperty("previous_state")]
        private AlarmState _previousState;

        /// <summary>
        /// This is the backing field for the <see cref="MonitoringZoneId"/> property.
        /// </summary>
        [JsonProperty("analyzed_by_monitoring_zone_id", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private MonitoringZoneId _analyzedByMonitoringZoneId;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="AlarmStateHistory"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected AlarmStateHistory()
        {
        }

        /// <summary>
        /// Gets a timestamp indicating when the alarm state changed.
        /// </summary>
        public DateTimeOffset? Timestamp
        {
            get
            {
                return DateTimeOffsetExtensions.ToDateTimeOffset(_timestamp);
            }
        }

        /// <summary>
        /// Gets the ID of the entity associated with the alarm.
        /// </summary>
        public EntityId EntityId
        {
            get
            {
                return _entityId;
            }
        }

        /// <summary>
        /// Gets the ID of the alarm which changed state.
        /// </summary>
        public AlarmId AlarmId
        {
            get
            {
                return _alarmId;
            }
        }

        /// <summary>
        /// Gets the ID of the check which triggered the alarm state change.
        /// </summary>
        public CheckId CheckId
        {
            get
            {
                return _checkId;
            }
        }

        /// <summary>
        /// Gets the status of the history item.
        /// </summary>
        public string Status
        {
            get
            {
                return _status;
            }
        }

        /// <summary>
        /// Gets the state of the alarm after this history item.
        /// </summary>
        public AlarmState State
        {
            get
            {
                return _state;
            }
        }

        /// <summary>
        /// Gets the state of the alarm before this history item.
        /// </summary>
        public AlarmState PreviousState
        {
            get
            {
                return _previousState;
            }
        }

        /// <summary>
        /// Gets the ID of the monitoring zone which analyzed the check that changed the alarm state.
        /// </summary>
        public MonitoringZoneId AnalyzedByMonitoringZoneId
        {
            get
            {
                return _analyzedByMonitoringZoneId;
            }
        }
    }
}
