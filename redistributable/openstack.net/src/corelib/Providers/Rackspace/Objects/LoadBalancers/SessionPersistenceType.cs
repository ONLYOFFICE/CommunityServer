namespace net.openstack.Providers.Rackspace.Objects.LoadBalancers
{
    using System;
    using System.Collections.Concurrent;
    using net.openstack.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a type of session persistence in the load balancers service.
    /// </summary>
    /// <remarks>
    /// This class functions as a strongly-typed enumeration of known session persistence types,
    /// with added support for unknown types returned by a server extension.
    /// </remarks>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonConverter(typeof(SessionPersistenceType.Converter))]
    public sealed class SessionPersistenceType : ExtensibleEnum<SessionPersistenceType>
    {
        private static readonly ConcurrentDictionary<string, SessionPersistenceType> _types =
            new ConcurrentDictionary<string, SessionPersistenceType>(StringComparer.OrdinalIgnoreCase);
        private static readonly SessionPersistenceType _httpCookie = FromName("HTTP_COOKIE");
        private static readonly SessionPersistenceType _sourceAddress = FromName("SOURCE_IP");

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionPersistenceType"/> class with the specified name.
        /// </summary>
        /// <inheritdoc/>
        private SessionPersistenceType(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Gets the <see cref="SessionPersistenceType"/> instance with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The unique <see cref="SessionPersistenceType"/> instance with the specified name.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="name"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="name"/> is empty.</exception>
        public static SessionPersistenceType FromName(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name cannot be empty");

            return _types.GetOrAdd(name, i => new SessionPersistenceType(i));
        }

        /// <summary>
        /// Gets a <see cref="SessionPersistenceType"/> representing a session persistence mechanism that
        /// inserts an HTTP cookie and is used to determine the destination back-end node. This is supported
        /// for HTTP load balancing only.
        /// </summary>
        public static SessionPersistenceType HttpCookie
        {
            get
            {
                return _httpCookie;
            }
        }

        /// <summary>
        /// Gets a <see cref="SessionPersistenceType"/> representing a session persistence mechanism that
        /// will keep track of the source IP address that is mapped and is able to determine the destination
        /// back-end node. This is supported for HTTPS pass-through and non-HTTP load balancing only.
        /// </summary>
        public static SessionPersistenceType SourceAddress
        {
            get
            {
                return _sourceAddress;
            }
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="SessionPersistenceType"/>
        /// objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        /// <preliminary/>
        private sealed class Converter : ConverterBase
        {
            /// <inheritdoc/>
            protected override SessionPersistenceType FromName(string name)
            {
                return SessionPersistenceType.FromName(name);
            }
        }
    }
}
