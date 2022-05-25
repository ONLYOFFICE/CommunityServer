namespace net.openstack.Providers.Rackspace.Objects.Dns
{
    using System;
    using System.Collections.Concurrent;
    using net.openstack.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a category of limits which apply to the DNS service.
    /// </summary>
    /// <remarks>
    /// This class functions as a strongly-typed enumeration of known limit types,
    /// with added support for unknown types returned by a server extension.
    /// </remarks>
    /// <seealso href="http://docs.rackspace.com/cdns/api/v1.0/cdns-devguide/content/Rate_Limits-d1e1222.html">Rate Limits (Rackspace Cloud DNS Developer Guide - API v1.0)</seealso>
    /// <seealso href="http://docs.rackspace.com/cdns/api/v1.0/cdns-devguide/content/Domain_Limits.html">Domain Limits (Rackspace Cloud DNS Developer Guide - API v1.0)</seealso>
    /// <seealso href="http://docs.rackspace.com/cdns/api/v1.0/cdns-devguide/content/Record_Limits.html">Record Limits (Rackspace Cloud DNS Developer Guide - API v1.0)</seealso>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonConverter(typeof(LimitType.Converter))]
    public sealed class LimitType : ExtensibleEnum<LimitType>
    {
        private static readonly ConcurrentDictionary<string, LimitType> _types =
            new ConcurrentDictionary<string, LimitType>(StringComparer.OrdinalIgnoreCase);
        private static readonly LimitType _rate = FromName("RATE_LIMIT");
        private static readonly LimitType _domain = FromName("DOMAIN_LIMIT");
        private static readonly LimitType _domainRecord = FromName("DOMAIN_RECORD_LIMIT");

        /// <summary>
        /// Initializes a new instance of the <see cref="LimitType"/> class with the specified name.
        /// </summary>
        /// <inheritdoc/>
        private LimitType(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Gets the <see cref="LimitType"/> instance with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The unique <see cref="LimitType"/> instance with the specified name.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="name"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="name"/> is empty.</exception>
        public static LimitType FromName(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name cannot be empty");

            return _types.GetOrAdd(name, i => new LimitType(i));
        }

        /// <summary>
        /// Gets a <see cref="LimitType"/> representing rate limits in effect for the DNS service.
        /// </summary>
        public static LimitType Rate
        {
            get
            {
                return _rate;
            }
        }

        /// <summary>
        /// Gets a <see cref="LimitType"/> representing absolute limits in place for domains in the DNS service.
        /// </summary>
        public static LimitType Domain
        {
            get
            {
                return _domain;
            }
        }

        /// <summary>
        /// Gets a <see cref="LimitType"/> representing absolute limits in place for records in the DNS service.
        /// </summary>
        public static LimitType DomainRecord
        {
            get
            {
                return _domainRecord;
            }
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="LimitType"/>
        /// objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        /// <preliminary/>
        private sealed class Converter : ConverterBase
        {
            /// <inheritdoc/>
            protected override LimitType FromName(string name)
            {
                return LimitType.FromName(name);
            }
        }
    }
}
