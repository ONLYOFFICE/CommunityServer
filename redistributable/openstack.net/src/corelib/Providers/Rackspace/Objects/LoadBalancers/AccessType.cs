namespace net.openstack.Providers.Rackspace.Objects.LoadBalancers
{
    using System;
    using System.Collections.Concurrent;
    using net.openstack.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents an network access type in the load balancers service.
    /// </summary>
    /// <remarks>
    /// This class functions as a strongly-typed enumeration of known access types,
    /// with added support for unknown types returned by a server extension.
    /// </remarks>
    /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Manage_Access_Lists-d1e3187.html">Manage Access Lists (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonConverter(typeof(AccessType.Converter))]
    public sealed class AccessType : ExtensibleEnum<AccessType>
    {
        private static readonly ConcurrentDictionary<string, AccessType> _types =
            new ConcurrentDictionary<string, AccessType>(StringComparer.OrdinalIgnoreCase);
        private static readonly AccessType _allow = FromName("ALLOW");
        private static readonly AccessType _deny = FromName("DENY");

        /// <summary>
        /// Initializes a new instance of the <see cref="AccessType"/> class with the specified name.
        /// </summary>
        /// <inheritdoc/>
        private AccessType(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Gets the <see cref="AccessType"/> instance with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The unique <see cref="AccessType"/> instance with the specified name.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="name"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="name"/> is empty.</exception>
        public static AccessType FromName(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name cannot be empty");

            return _types.GetOrAdd(name, i => new AccessType(i));
        }

        /// <summary>
        /// Gets an <see cref="AccessType"/> representing an item to which traffic should be allowed.
        /// The <see cref="Allow"/> access type takes priority over the <see cref="Deny"/> access type.
        /// </summary>
        public static AccessType Allow
        {
            get
            {
                return _allow;
            }
        }

        /// <summary>
        /// Gets an <see cref="AccessType"/> representing an item to which traffic can be denied.
        /// </summary>
        public static AccessType Deny
        {
            get
            {
                return _deny;
            }
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="AccessType"/>
        /// objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        /// <preliminary/>
        private sealed class Converter : ConverterBase
        {
            /// <inheritdoc/>
            protected override AccessType FromName(string name)
            {
                return AccessType.FromName(name);
            }
        }
    }
}
