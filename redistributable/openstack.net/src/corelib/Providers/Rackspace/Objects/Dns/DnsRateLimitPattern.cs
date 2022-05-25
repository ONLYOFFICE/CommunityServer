namespace net.openstack.Providers.Rackspace.Objects.Dns
{
    using System.Collections.ObjectModel;
    using Newtonsoft.Json;
    using CancellationToken = System.Threading.CancellationToken;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This class describes the rate limits in effect for resource URIs matching
    /// a specific pattern.
    /// </summary>
    /// <seealso cref="IDnsService.ListLimitsAsync(CancellationToken)"/>
    /// <seealso href="http://docs.rackspace.com/cdns/api/v1.0/cdns-devguide/content/List_All_Limits.html">List All Limits (Rackspace Cloud DNS Developer Guide - API v1.0)</seealso>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class DnsRateLimitPattern : ExtensibleJsonObject
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value
        /// <summary>
        /// This is the backing field for the <see cref="Uri"/> property.
        /// </summary>
        [JsonProperty("uri")]
        private string _uri;

        /// <summary>
        /// This is the backing field for the <see cref="RegularExpression"/> property.
        /// </summary>
        [JsonProperty("regex")]
        private string _regex;

        /// <summary>
        /// This is the backing field for the <see cref="Limit"/> property.
        /// </summary>
        [JsonProperty("limit")]
        private DnsRateLimit[] _limit;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="DnsRateLimitPattern"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected DnsRateLimitPattern()
        {
        }

        /// <summary>
        /// Gets a display representation of the URIs which are limited by this pattern.
        /// </summary>
        /// <value>
        /// A display representation of the URIs which are limited by this pattern, or
        /// <see langword="null"/> if the JSON response from the server did not include this property.
        /// </value>
        public string Uri
        {
            get
            {
                return _uri;
            }
        }

        /// <summary>
        /// Gets a regular expression pattern intended to match the URIs which are limited by this pattern.
        /// </summary>
        /// <value>
        /// A regular expression pattern intended to match the URIs which are limited by this pattern, or
        /// <see langword="null"/> if the JSON response from the server did not include this property.
        /// </value>
        public string RegularExpression
        {
            get
            {
                return _regex;
            }
        }

        /// <summary>
        /// Gets a collection of <see cref="DnsRateLimit"/> objects describing the specific limits in
        /// effect for the resources at URIs described by this pattern.
        /// </summary>
        /// <value>
        /// A collection of <see cref="DnsRateLimit"/> objects describing the specific limits, or
        /// <see langword="null"/> if the JSON response from the server did not include this property.
        /// </value>
        public ReadOnlyCollection<DnsRateLimit> Limit
        {
            get
            {
                if (_limit == null)
                    return null;

                return new ReadOnlyCollection<DnsRateLimit>(_limit);
            }
        }
    }
}
