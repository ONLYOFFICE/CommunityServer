namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System;
    using System.Collections.Concurrent;
    using net.openstack.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a statistic type in the monitoring service.
    /// </summary>
    /// <remarks>
    /// This class functions as a strongly-typed enumeration of known statistic types,
    /// with added support for unknown types supported by a server extension.
    /// </remarks>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonConverter(typeof(DataPointStatistic.Converter))]
    public sealed class DataPointStatistic : ExtensibleEnum<DataPointStatistic>
    {
        private static readonly ConcurrentDictionary<string, DataPointStatistic> _types =
            new ConcurrentDictionary<string, DataPointStatistic>(StringComparer.OrdinalIgnoreCase);
        private static readonly DataPointStatistic _numPoints = FromName("numPoints");
        private static readonly DataPointStatistic _average = FromName("average");
        private static readonly DataPointStatistic _variance = FromName("variance");
        private static readonly DataPointStatistic _min = FromName("min");
        private static readonly DataPointStatistic _max = FromName("max");

        /// <summary>
        /// Initializes a new instance of the <see cref="DataPointStatistic"/> class with the specified name.
        /// </summary>
        /// <inheritdoc/>
        private DataPointStatistic(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Gets the <see cref="DataPointStatistic"/> instance with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The unique <see cref="DataPointStatistic"/> instance with the specified name.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="name"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="name"/> is empty.</exception>
        public static DataPointStatistic FromName(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name cannot be empty");

            return _types.GetOrAdd(name, i => new DataPointStatistic(i));
        }

        /// <summary>
        /// Gets a <see cref="DataPointStatistic"/> representing the size of a set of data points.
        /// </summary>
        public static DataPointStatistic NumPoints
        {
            get
            {
                return _numPoints;
            }
        }

        /// <summary>
        /// Gets a <see cref="DataPointStatistic"/> representing the average of a set of data points.
        /// </summary>
        public static DataPointStatistic Average
        {
            get
            {
                return _average;
            }
        }

        /// <summary>
        /// Gets a <see cref="DataPointStatistic"/> representing the variance of a set of data points.
        /// </summary>
        public static DataPointStatistic Variance
        {
            get
            {
                return _variance;
            }
        }

        /// <summary>
        /// Gets a <see cref="DataPointStatistic"/> representing the minimum of a set of data points.
        /// </summary>
        public static DataPointStatistic Min
        {
            get
            {
                return _min;
            }
        }

        /// <summary>
        /// Gets a <see cref="DataPointStatistic"/> representing the maximum of a set of data points.
        /// </summary>
        public static DataPointStatistic Max
        {
            get
            {
                return _max;
            }
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="DataPointStatistic"/>
        /// objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        /// <preliminary/>
        private sealed class Converter : ConverterBase
        {
            /// <inheritdoc/>
            protected override DataPointStatistic FromName(string name)
            {
                return DataPointStatistic.FromName(name);
            }
        }
    }
}
