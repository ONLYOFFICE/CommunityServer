namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System;
    using System.Collections.Concurrent;
    using net.openstack.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the type of a check type in the monitoring service.
    /// </summary>
    /// <remarks>
    /// This class functions as a strongly-typed enumeration of known check type types,
    /// with added support for unknown types returned by a server extension.
    /// </remarks>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonConverter(typeof(CheckTypeType.Converter))]
    public sealed class CheckTypeType : ExtensibleEnum<CheckTypeType>
    {
        private static readonly ConcurrentDictionary<string, CheckTypeType> _types =
            new ConcurrentDictionary<string, CheckTypeType>(StringComparer.OrdinalIgnoreCase);
        private static readonly CheckTypeType _remote = FromName("remote");
        private static readonly CheckTypeType _agent = FromName("agent");

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckTypeType"/> class with the specified name.
        /// </summary>
        /// <inheritdoc/>
        private CheckTypeType(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Gets the <see cref="CheckTypeType"/> instance with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The unique <see cref="CheckTypeType"/> instance with the specified name.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="name"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="name"/> is empty.</exception>
        public static CheckTypeType FromName(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name cannot be empty");

            return _types.GetOrAdd(name, i => new CheckTypeType(i));
        }

        /// <summary>
        /// Gets a <see cref="CheckTypeType"/> representing a remote check type.
        /// </summary>
        public static CheckTypeType Remote
        {
            get
            {
                return _remote;
            }
        }

        /// <summary>
        /// Gets a <see cref="CheckTypeType"/> representing an agent check type.
        /// </summary>
        public static CheckTypeType Agent
        {
            get
            {
                return _agent;
            }
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="CheckTypeType"/>
        /// objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        /// <preliminary/>
        private sealed class Converter : ConverterBase
        {
            /// <inheritdoc/>
            protected override CheckTypeType FromName(string name)
            {
                return CheckTypeType.FromName(name);
            }
        }
    }
}
