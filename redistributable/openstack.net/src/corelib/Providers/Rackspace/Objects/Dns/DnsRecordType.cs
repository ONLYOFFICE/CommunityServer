namespace net.openstack.Providers.Rackspace.Objects.Dns
{
    using System;
    using System.Collections.Concurrent;
    using net.openstack.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a DNS record type.
    /// </summary>
    /// <remarks>
    /// This class functions as a strongly-typed enumeration of known record types,
    /// with added support for unknown types returned by a server extension.
    /// </remarks>
    /// <seealso href="http://docs.rackspace.com/cdns/api/v1.0/cdns-devguide/content/supported_record_types.html">Supported Record Types (Rackspace Cloud DNS Developer Guide - API v1.0)</seealso>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonConverter(typeof(DnsRecordType.Converter))]
    public sealed class DnsRecordType : ExtensibleEnum<DnsRecordType>
    {
        private static readonly ConcurrentDictionary<string, DnsRecordType> _types =
            new ConcurrentDictionary<string, DnsRecordType>(StringComparer.OrdinalIgnoreCase);
        private static readonly DnsRecordType _a = FromName("A");
        private static readonly DnsRecordType _aaaa = FromName("AAAA");
        private static readonly DnsRecordType _cname = FromName("CNAME");
        private static readonly DnsRecordType _mx = FromName("MX");
        private static readonly DnsRecordType _ns = FromName("NS");
        private static readonly DnsRecordType _ptr = FromName("PTR");
        private static readonly DnsRecordType _srv = FromName("SRV");
        private static readonly DnsRecordType _txt = FromName("TXT");

        /// <summary>
        /// Initializes a new instance of the <see cref="DnsRecordType"/> class with the specified name.
        /// </summary>
        /// <inheritdoc/>
        private DnsRecordType(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Gets the <see cref="DnsRecordType"/> instance with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The unique <see cref="DnsRecordType"/> instance with the specified name.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="name"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="name"/> is empty.</exception>
        public static DnsRecordType FromName(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name cannot be empty");

            return _types.GetOrAdd(name, i => new DnsRecordType(i));
        }

        /// <summary>
        /// Gets a <see cref="DnsRecordType"/> representing an address record.
        /// </summary>
        public static DnsRecordType A
        {
            get
            {
                return _a;
            }
        }

        /// <summary>
        /// Gets a <see cref="DnsRecordType"/> representing an IPv6 address record.
        /// </summary>
        public static DnsRecordType Aaaa
        {
            get
            {
                return _aaaa;
            }
        }

        /// <summary>
        /// Gets a <see cref="DnsRecordType"/> representing a canonical name record.
        /// </summary>
        public static DnsRecordType Cname
        {
            get
            {
                return _cname;
            }
        }

        /// <summary>
        /// Gets a <see cref="DnsRecordType"/> representing mail exchange record.
        /// </summary>
        public static DnsRecordType Mx
        {
            get
            {
                return _mx;
            }
        }

        /// <summary>
        /// Gets a <see cref="DnsRecordType"/> representing a name server record.
        /// </summary>
        public static DnsRecordType Ns
        {
            get
            {
                return _ns;
            }
        }

        /// <summary>
        /// Gets a <see cref="DnsRecordType"/> representing a pointer record.
        /// </summary>
        public static DnsRecordType Ptr
        {
            get
            {
                return _ptr;
            }
        }

        /// <summary>
        /// Gets a <see cref="DnsRecordType"/> representing a service locator.
        /// </summary>
        public static DnsRecordType Srv
        {
            get
            {
                return _srv;
            }
        }

        /// <summary>
        /// Gets a <see cref="DnsRecordType"/> representing a text record.
        /// </summary>
        public static DnsRecordType Txt
        {
            get
            {
                return _txt;
            }
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="DnsRecordType"/>
        /// objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        /// <preliminary/>
        private sealed class Converter : ConverterBase
        {
            /// <inheritdoc/>
            protected override DnsRecordType FromName(string name)
            {
                return DnsRecordType.FromName(name);
            }
        }
    }
}
