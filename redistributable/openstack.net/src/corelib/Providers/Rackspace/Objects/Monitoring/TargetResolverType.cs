namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System;
    using System.Collections.Concurrent;
    using net.openstack.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a target resolver type in the monitoring service.
    /// </summary>
    /// <remarks>
    /// This class functions as a strongly-typed enumeration of known target resolver types,
    /// with added support for unknown types supported by a server extension.
    /// </remarks>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonConverter(typeof(TargetResolverType.Converter))]
    public sealed class TargetResolverType : ExtensibleEnum<TargetResolverType>
    {
        private static readonly ConcurrentDictionary<string, TargetResolverType> _types =
            new ConcurrentDictionary<string, TargetResolverType>(StringComparer.OrdinalIgnoreCase);
        private static readonly TargetResolverType _ipv4 = FromName("IPv4");
        private static readonly TargetResolverType _ipv6 = FromName("IPv6");

        /// <summary>
        /// Initializes a new instance of the <see cref="TargetResolverType"/> class with the specified name.
        /// </summary>
        /// <inheritdoc/>
        private TargetResolverType(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Gets the <see cref="TargetResolverType"/> instance with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The unique <see cref="TargetResolverType"/> instance with the specified name.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="name"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="name"/> is empty.</exception>
        public static TargetResolverType FromName(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name cannot be empty");

            return _types.GetOrAdd(name, i => new TargetResolverType(i));
        }

        /// <summary>
        /// Gets a <see cref="TargetResolverType"/> representing a resolver that resolves hostnames to IP V4 addresses.
        /// </summary>
        public static TargetResolverType IPv4
        {
            get
            {
                return _ipv4;
            }
        }

        /// <summary>
        /// Gets a <see cref="TargetResolverType"/> representing a resolver that resolves hostnames to IP V6 addresses.
        /// </summary>
        public static TargetResolverType IPv6
        {
            get
            {
                return _ipv6;
            }
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="TargetResolverType"/>
        /// objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        /// <preliminary/>
        private sealed class Converter : ConverterBase
        {
            /// <inheritdoc/>
            protected override TargetResolverType FromName(string name)
            {
                return TargetResolverType.FromName(name);
            }
        }
    }
}
