namespace net.openstack.Providers.Rackspace.Objects.Dns
{
    using System.Collections.ObjectModel;
    using Newtonsoft.Json;
    using CancellationToken = System.Threading.CancellationToken;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This class models the rate limits in effect for the DNS service.
    /// </summary>
    /// <seealso cref="IDnsService.ListLimitsAsync(CancellationToken)"/>
    /// <seealso href="http://docs.rackspace.com/cdns/api/v1.0/cdns-devguide/content/List_All_Limits.html">List All Limits (Rackspace Cloud DNS Developer Guide - API v1.0)</seealso>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class DnsRateLimits : ExtensibleJsonObject
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value
        /// <summary>
        /// This is the backing field for the <see cref="Rate"/> property.
        /// </summary>
        [JsonProperty("rate")]
        private DnsRateLimitPattern[] _rate;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="DnsRateLimits"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected DnsRateLimits()
        {
        }

        /// <summary>
        /// Gets a collection of <see cref="DnsRateLimitPattern"/> objects describing the
        /// rate limits in effect for various DNS resources.
        /// </summary>
        /// <value>
        /// A collection of <see cref="DnsRateLimitPattern"/> objects describing DNS rate
        /// limits, or <see langword="null"/> if the JSON response from the server did not include
        /// this property.
        /// </value>
        public ReadOnlyCollection<DnsRateLimitPattern> Rate
        {
            get
            {
                if (_rate == null)
                    return null;

                return new ReadOnlyCollection<DnsRateLimitPattern>(_rate);
            }
        }
    }
}
