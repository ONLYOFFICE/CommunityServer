namespace net.openstack.Providers.Rackspace.Objects.Dns
{
    using System;
    using System.Collections.Concurrent;
    using net.openstack.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a format for domains exported by the DNS service.
    /// </summary>
    /// <remarks>
    /// This class functions as a strongly-typed enumeration of known serialization
    /// formats, with added support for unknown formats returned by a server extension.
    /// </remarks>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonConverter(typeof(SerializedDomainFormat.Converter))]
    public sealed class SerializedDomainFormat : ExtensibleEnum<SerializedDomainFormat>
    {
        private static readonly ConcurrentDictionary<string, SerializedDomainFormat> _types =
            new ConcurrentDictionary<string, SerializedDomainFormat>(StringComparer.OrdinalIgnoreCase);
        private static readonly SerializedDomainFormat _bind9 = FromName("BIND_9");

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializedDomainFormat"/> class with the specified name.
        /// </summary>
        /// <inheritdoc/>
        private SerializedDomainFormat(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Gets the <see cref="SerializedDomainFormat"/> instance with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The unique <see cref="SerializedDomainFormat"/> instance with the specified name.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="name"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="name"/> is empty.</exception>
        public static SerializedDomainFormat FromName(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name cannot be empty");

            return _types.GetOrAdd(name, i => new SerializedDomainFormat(i));
        }

        /// <summary>
        /// Gets a <see cref="SerializedDomainFormat"/> representing a <see href="http://en.wikipedia.org/wiki/BIND">BIND 9</see> serialized domain.
        /// </summary>
        public static SerializedDomainFormat Bind9
        {
            get
            {
                return _bind9;
            }
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="SerializedDomainFormat"/>
        /// objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        private sealed class Converter : ConverterBase
        {
            /// <inheritdoc/>
            protected override SerializedDomainFormat FromName(string name)
            {
                return SerializedDomainFormat.FromName(name);
            }
        }
    }
}
