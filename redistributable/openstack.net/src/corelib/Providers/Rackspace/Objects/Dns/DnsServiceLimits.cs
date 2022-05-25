namespace net.openstack.Providers.Rackspace.Objects.Dns
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    using Newtonsoft.Json;

    using CancellationToken = System.Threading.CancellationToken;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This class models the rate and absolute limits in effect for the DNS service.
    /// </summary>
    /// <seealso cref="IDnsService.ListLimitsAsync(CancellationToken)"/>
    /// <seealso href="http://docs.rackspace.com/cdns/api/v1.0/cdns-devguide/content/List_All_Limits.html">List All Limits (Rackspace Cloud DNS Developer Guide - API v1.0)</seealso>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class DnsServiceLimits : ExtensibleJsonObject
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value
        /// <summary>
        /// This is the backing field for the <see cref="RateLimits"/> property.
        /// </summary>
        [JsonProperty("rate")]
        private DnsRateLimitPattern[] _rate;

        /// <summary>
        /// This is the backing field for the <see cref="AbsoluteLimits"/> property.
        /// </summary>
        [JsonProperty("absolute")]
        private Dictionary<string, long> _absolute;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="DnsServiceLimits"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected DnsServiceLimits()
        {
        }

        /// <summary>
        /// Gets a collection of <see cref="DnsRateLimitPattern"/> objects describing the rate limits
        /// associated with resources in the DNS service.
        /// </summary>
        public ReadOnlyCollection<DnsRateLimitPattern> RateLimits
        {
            get
            {
                if (_rate == null)
                    return null;

                return new ReadOnlyCollection<DnsRateLimitPattern>(_rate);
            }
        }

        /// <summary>
        /// Gets a map of absolute limits in effect for the DNS service. The keys of the map are
        /// canonical names describing the limited DNS resource, and the values are the actual limits
        /// in effect for the service.
        /// </summary>
        public ReadOnlyDictionary<string, long> AbsoluteLimits
        {
            get
            {
                if (_absolute == null)
                    return null;

                return new ReadOnlyDictionary<string, long>(_absolute);
            }
        }
    }
}
