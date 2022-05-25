using System.Collections.ObjectModel;

namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System;
    using System.Collections.Generic;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This class models the JSON representation of the data produced by testing a monitoring check.
    /// </summary>
    /// <seealso cref="IMonitoringService.TestCheckAsync"/>
    /// <seealso cref="IMonitoringService.TestExistingCheckAsync"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class CheckData : ExtensibleJsonObject
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value {value}
        /// <summary>
        /// This is the backing field for the <see cref="Timestamp"/> property.
        /// </summary>
        [JsonProperty("timestamp")]
        private long? _timestamp;

        /// <summary>
        /// This is the backing field for the <see cref="MonitoringZoneId"/> property.
        /// </summary>
        [JsonProperty("monitoring_zone_id")]
        private MonitoringZoneId _monitoringZoneId;

        /// <summary>
        /// This is the backing field for the <see cref="Available"/> property.
        /// </summary>
        [JsonProperty("available")]
        private bool? _available;

        /// <summary>
        /// This is the backing field for the <see cref="Status"/> property.
        /// </summary>
        [JsonProperty("status")]
        private string _status;

        /// <summary>
        /// This is the backing field for the <see cref="Metrics"/> property.
        /// </summary>
        [JsonProperty("metrics")]
        private Dictionary<string, CheckMetric> _metrics;

        /// <summary>
        /// This is the backing field for the <see cref="DebugInfo"/> property.
        /// </summary>
        [JsonProperty("debug_info")]
        private DebugInformation _debugInfo;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckData"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected CheckData()
        {
        }

        /// <summary>
        /// Gets a timestamp indicating when the check was evaluated.
        /// </summary>
        public DateTimeOffset? Timestamp
        {
            get
            {
                return DateTimeOffsetExtensions.ToDateTimeOffset(_timestamp);
            }
        }

        /// <summary>
        /// Gets the ID of the monitoring zone which evaluated the check.
        /// </summary>
        public MonitoringZoneId MonitoringZoneId
        {
            get
            {
                return _monitoringZoneId;
            }
        }

        /// <summary>
        /// Gets a value indicating whether or not the target of the check was available.
        /// </summary>
        public bool? Available
        {
            get
            {
                return _available;
            }
        }

        /// <summary>
        /// Gets the a description of the status of the check.
        /// </summary>
        public string Status
        {
            get
            {
                return _status;
            }
        }

        /// <summary>
        /// Gets a collection of named metrics collected by the check. The keys of the collection
        /// are the names of metrics which are accessible in alarm criteria, and the values
        /// contain the actual metric information collected by the evaluation of the check.
        /// </summary>
        public ReadOnlyDictionary<string, CheckMetric> Metrics
        {
            get
            {
                if (_metrics == null)
                    return null;

                return new ReadOnlyDictionary<string, CheckMetric>(_metrics);
            }
        }

        /// <summary>
        /// Gets additional debug information about the evaluation of the check.
        /// </summary>
        public DebugInformation DebugInfo
        {
            get
            {
                return _debugInfo;
            }
        }

        /// <summary>
        /// This class models the JSON representation of metric information collected by a
        /// test evaluation of a monitoring check.
        /// </summary>
        /// <seealso cref="CheckData.Metrics"/>
        /// <threadsafety static="true" instance="false"/>
        /// <preliminary/>
        [JsonObject(MemberSerialization.OptIn)]
        public class CheckMetric : ExtensibleJsonObject
        {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value {value}
            /// <summary>
            /// This is the backing field for the <see cref="Type"/> property.
            /// </summary>
            [JsonProperty("type")]
            private CheckMetricType _type;

            /// <summary>
            /// This is the backing field for the <see cref="Data"/> property.
            /// </summary>
            [JsonProperty("data")]
            private string _data;

            /// <summary>
            /// This is the backing field for the <see cref="Unit"/> property.
            /// </summary>
            [JsonProperty("unit")]
            private string _unit;
#pragma warning restore 649

            /// <summary>
            /// Initializes a new instance of the <see cref="CheckMetric"/> class
            /// during JSON deserialization.
            /// </summary>
            [JsonConstructor]
            protected CheckMetric()
            {
            }

            /// <summary>
            /// Gets the type of data collected for this metric.
            /// </summary>
            public CheckMetricType Type
            {
                get
                {
                    return _type;
                }
            }

            /// <summary>
            /// Gets the actual data collected for this metric.
            /// </summary>
            public string Data
            {
                get
                {
                    return _data;
                }
            }

            /// <summary>
            /// Gets the name of the units for the collected data.
            /// </summary>
            public string Unit
            {
                get
                {
                    return _unit;
                }
            }
        }

        /// <summary>
        /// This class models the JSON representation of additional check-type-specific
        /// debugging information collected during the test evaluation of a monitoring
        /// check.
        /// </summary>
        /// <seealso cref="CheckData.DebugInfo"/>
        /// <threadsafety static="true" instance="false"/>
        /// <preliminary/>
        [JsonObject(MemberSerialization.OptIn)]
        public class DebugInformation : ExtensibleJsonObject
        {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value {value}
            /// <summary>
            /// This is the backing field for the <see cref="Data"/> property.
            /// </summary>
            [JsonExtensionData]
            private IDictionary<string, JToken> _data;
#pragma warning restore 649

            /// <summary>
            /// Initializes a new instance of the <see cref="DebugInformation"/> class
            /// during JSON deserialization.
            /// </summary>
            [JsonConstructor]
            protected DebugInformation()
            {
            }

            /// <summary>
            /// Gets a collection of additional check-type-specific debug information collected
            /// during the test evaluation of a monitoring check.
            /// </summary>
            public ReadOnlyDictionary<string, JToken> Data
            {
                get
                {
                    if (_data == null)
                        return null;

                    return new ReadOnlyDictionary<string, JToken>(_data);
                }
            }
        }
    }
}
