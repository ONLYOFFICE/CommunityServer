namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System;
    using System.Collections.Concurrent;
    using net.openstack.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a data granularity in the monitoring service.
    /// </summary>
    /// <remarks>
    /// This class functions as a strongly-typed enumeration of known granularities,
    /// with added support for unknown values supported by a server extension.
    /// </remarks>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonConverter(typeof(DataPointGranularity.Converter))]
    public sealed class DataPointGranularity : ExtensibleEnum<DataPointGranularity>
    {
        private static readonly ConcurrentDictionary<string, DataPointGranularity> _types =
            new ConcurrentDictionary<string, DataPointGranularity>(StringComparer.OrdinalIgnoreCase);
        private static readonly DataPointGranularity _full = FromName("FULL");
        private static readonly DataPointGranularity _min5 = FromName("MIN5");
        private static readonly DataPointGranularity _min20 = FromName("MIN20");
        private static readonly DataPointGranularity _min60 = FromName("MIN60");
        private static readonly DataPointGranularity _min240 = FromName("MIN240");
        private static readonly DataPointGranularity _min1440 = FromName("MIN1440");

        /// <summary>
        /// Initializes a new instance of the <see cref="DataPointGranularity"/> class with the specified name.
        /// </summary>
        /// <inheritdoc/>
        private DataPointGranularity(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Gets the <see cref="DataPointGranularity"/> instance with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The unique <see cref="DataPointGranularity"/> instance with the specified name.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="name"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="name"/> is empty.</exception>
        public static DataPointGranularity FromName(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name cannot be empty");

            return _types.GetOrAdd(name, i => new DataPointGranularity(i));
        }

        /// <summary>
        /// Gets a <see cref="DataPointGranularity"/> representing full resolution data.
        /// </summary>
        public static DataPointGranularity Full
        {
            get
            {
                return _full;
            }
        }

        /// <summary>
        /// Gets a <see cref="DataPointGranularity"/> representing data roll-ups computed at 5-minute intervals.
        /// </summary>
        public static DataPointGranularity Min5
        {
            get
            {
                return _min5;
            }
        }

        /// <summary>
        /// Gets a <see cref="DataPointGranularity"/> representing data roll-ups computed at 20-minute intervals.
        /// </summary>
        public static DataPointGranularity Min20
        {
            get
            {
                return _min20;
            }
        }

        /// <summary>
        /// Gets a <see cref="DataPointGranularity"/> representing data roll-ups computed at 60-minute intervals.
        /// </summary>
        public static DataPointGranularity Min60
        {
            get
            {
                return _min60;
            }
        }

        /// <summary>
        /// Gets a <see cref="DataPointGranularity"/> representing data roll-ups computed at 240-minute intervals.
        /// </summary>
        public static DataPointGranularity Min240
        {
            get
            {
                return _min240;
            }
        }

        /// <summary>
        /// Gets a <see cref="DataPointGranularity"/> representing data roll-ups computed at 1440-minute intervals.
        /// </summary>
        public static DataPointGranularity Min1440
        {
            get
            {
                return _min1440;
            }
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="DataPointGranularity"/>
        /// objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        /// <preliminary/>
        private sealed class Converter : ConverterBase
        {
            /// <inheritdoc/>
            protected override DataPointGranularity FromName(string name)
            {
                return DataPointGranularity.FromName(name);
            }
        }
    }
}
