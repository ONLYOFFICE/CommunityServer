namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System;
    using Newtonsoft.Json;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This class models the JSON representation of a raw or rolled-up data point
    /// reported by <see cref="IMonitoringService.GetDataPointsAsync"/>.
    /// </summary>
    /// <seealso cref="IMonitoringService.GetDataPointsAsync"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class DataPoint : ExtensibleJsonObject
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value {value}
        /// <summary>
        /// This is the backing field for the <see cref="NumPoints"/> property.
        /// </summary>
        [JsonProperty("numPoints")]
        private long? _numPoints;

        /// <summary>
        /// This is the backing field for the <see cref="Average"/> property.
        /// </summary>
        [JsonProperty("average")]
        private double? _average;

        /// <summary>
        /// This is the backing field for the <see cref="Variance"/> property.
        /// </summary>
        [JsonProperty("variance")]
        private double? _variance;

        /// <summary>
        /// This is the backing field for the <see cref="Min"/> property.
        /// </summary>
        [JsonProperty("min")]
        private double? _min;

        /// <summary>
        /// This is the backing field for the <see cref="Max"/> property.
        /// </summary>
        [JsonProperty("max")]
        private double? _max;

        /// <summary>
        /// This is the backing field for the <see cref="Timestamp"/> property.
        /// </summary>
        [JsonProperty("timestamp")]
        private long? _timestamp;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="DataPoint"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected DataPoint()
        {
        }

        /// <summary>
        /// Gets the number of raw data points represented by this <see cref="DataPoint"/> instance.
        /// </summary>
        /// <seealso cref="DataPointStatistic.NumPoints"/>
        public long? NumPoints
        {
            get
            {
                return _numPoints;
            }
        }

        /// <summary>
        /// Gets the average value of the data points represented by this <see cref="DataPoint"/> instance.
        /// </summary>
        /// <seealso cref="DataPointStatistic.Average"/>
        public double? Average
        {
            get
            {
                return _average;
            }
        }

        /// <summary>
        /// Gets the variance of the data points represented by this <see cref="DataPoint"/> instance.
        /// </summary>
        /// <seealso cref="DataPointStatistic.Variance"/>
        public double? Variance
        {
            get
            {
                return _variance;
            }
        }

        /// <summary>
        /// Gets the minimum value of the data points represented by this <see cref="DataPoint"/> instance.
        /// </summary>
        /// <seealso cref="DataPointStatistic.Min"/>
        public double? Min
        {
            get
            {
                return _min;
            }
        }

        /// <summary>
        /// Gets the maximum value of the data points represented by this <see cref="DataPoint"/> instance.
        /// </summary>
        /// <seealso cref="DataPointStatistic.Max"/>
        public double? Max
        {
            get
            {
                return _max;
            }
        }

        /// <summary>
        /// Gets a timestamp indicating when the data represented by this <see cref="DataPoint"/> instance was recorded.
        /// </summary>
        public DateTimeOffset? Timestamp
        {
            get
            {
                return DateTimeOffsetExtensions.ToDateTimeOffset(_timestamp);
            }
        }
    }
}
