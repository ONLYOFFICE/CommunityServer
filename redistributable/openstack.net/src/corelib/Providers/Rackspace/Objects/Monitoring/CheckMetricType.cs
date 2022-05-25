namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System;
    using System.Collections.Concurrent;
    using net.openstack.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a metric type in the monitoring service.
    /// </summary>
    /// <remarks>
    /// This class functions as a strongly-typed enumeration of known metric types,
    /// with added support for unknown types returned by a server extension.
    /// </remarks>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonConverter(typeof(CheckMetricType.Converter))]
    public sealed class CheckMetricType : ExtensibleEnum<CheckMetricType>
    {
        private static readonly ConcurrentDictionary<string, CheckMetricType> _types =
            new ConcurrentDictionary<string, CheckMetricType>(StringComparer.OrdinalIgnoreCase);
        private static readonly CheckMetricType _int32 = FromName("i");
        private static readonly CheckMetricType _int64 = FromName("l");
        private static readonly CheckMetricType _string = FromName("s");

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckMetricType"/> class with the specified name.
        /// </summary>
        /// <inheritdoc/>
        private CheckMetricType(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Gets the <see cref="CheckMetricType"/> instance with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The unique <see cref="CheckMetricType"/> instance with the specified name.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="name"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="name"/> is empty.</exception>
        public static CheckMetricType FromName(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name cannot be empty");

            return _types.GetOrAdd(name, i => new CheckMetricType(i));
        }

        /// <summary>
        /// Gets a <see cref="CheckMetricType"/> representing a 32-bit integer.
        /// </summary>
        public static CheckMetricType Int32
        {
            get
            {
                return _int32;
            }
        }

        /// <summary>
        /// Gets a <see cref="CheckMetricType"/> representing a 64-bit integer.
        /// </summary>
        public static CheckMetricType Int64
        {
            get
            {
                return _int64;
            }
        }

        /// <summary>
        /// Gets a <see cref="CheckMetricType"/> representing a string.
        /// </summary>
        public static CheckMetricType String
        {
            get
            {
                return _string;
            }
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="CheckMetricType"/>
        /// objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        /// <preliminary/>
        private sealed class Converter : ConverterBase
        {
            /// <inheritdoc/>
            protected override CheckMetricType FromName(string name)
            {
                return CheckMetricType.FromName(name);
            }
        }
    }
}
