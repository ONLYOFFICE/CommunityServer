namespace net.openstack.Providers.Rackspace.Objects.Dns
{
    using System;
    using System.Collections.Concurrent;
    using net.openstack.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the time unit used for measuring a rate limit in the DNS service.
    /// </summary>
    /// <remarks>
    /// This class functions as a strongly-typed enumeration of known time units,
    /// with added support for unknown formats returned by a server extension.
    /// </remarks>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonConverter(typeof(DnsRateLimitUnit.Converter))]
    public sealed class DnsRateLimitUnit : ExtensibleEnum<DnsRateLimitUnit>
    {
        private static readonly ConcurrentDictionary<string, DnsRateLimitUnit> _types =
            new ConcurrentDictionary<string, DnsRateLimitUnit>(StringComparer.OrdinalIgnoreCase);
        private static readonly DnsRateLimitUnit _second = FromName("SECOND");
        private static readonly DnsRateLimitUnit _minute = FromName("MINUTE");

        /// <summary>
        /// Initializes a new instance of the <see cref="DnsRateLimitUnit"/> class with the specified name.
        /// </summary>
        /// <inheritdoc/>
        private DnsRateLimitUnit(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Gets the <see cref="DnsRateLimitUnit"/> instance with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The unique <see cref="DnsRateLimitUnit"/> instance with the specified name.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="name"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="name"/> is empty.</exception>
        public static DnsRateLimitUnit FromName(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name cannot be empty");

            return _types.GetOrAdd(name, i => new DnsRateLimitUnit(i));
        }

        /// <summary>
        /// Gets a <see cref="DnsRateLimitUnit"/> representing a rate limit measured in seconds.
        /// </summary>
        public static DnsRateLimitUnit Second
        {
            get
            {
                return _second;
            }
        }

        /// <summary>
        /// Gets a <see cref="DnsRateLimitUnit"/> representing a rate limit measured in minutes.
        /// </summary>
        public static DnsRateLimitUnit Minute
        {
            get
            {
                return _minute;
            }
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="DnsRateLimitUnit"/>
        /// objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        /// <preliminary/>
        private sealed class Converter : ConverterBase
        {
            /// <inheritdoc/>
            protected override DnsRateLimitUnit FromName(string name)
            {
                return DnsRateLimitUnit.FromName(name);
            }
        }
    }
}
