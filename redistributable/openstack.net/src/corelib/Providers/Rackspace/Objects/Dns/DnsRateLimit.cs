namespace net.openstack.Providers.Rackspace.Objects.Dns
{
    using System;
    using JSIStudios.SimpleRESTServices.Client;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using CancellationToken = System.Threading.CancellationToken;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This class models the detailed parameters of a specific single rate limit within
    /// the DNS service.
    /// </summary>
    /// <seealso cref="IDnsService.ListLimitsAsync(CancellationToken)"/>
    /// <seealso href="http://docs.rackspace.com/cdns/api/v1.0/cdns-devguide/content/List_All_Limits.html">List All Limits (Rackspace Cloud DNS Developer Guide - API v1.0)</seealso>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class DnsRateLimit : ExtensibleJsonObject
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value
        /// <summary>
        /// This is the backing field for the <see cref="Verb"/> property.
        /// </summary>
        [JsonProperty("verb")]
        [JsonConverter(typeof(StringEnumConverter))]
        private HttpMethod? _verb;

        /// <summary>
        /// This is the backing field for the <see cref="Unit"/> property.
        /// </summary>
        [JsonProperty("unit")]
        private DnsRateLimitUnit _unit;

        /// <summary>
        /// This is the backing field for the <see cref="Value"/> property.
        /// </summary>
        [JsonProperty("value")]
        private long? _value;

        /// <summary>
        /// This is the backing field for the <see cref="Remaining"/> property.
        /// </summary>
        [JsonProperty("remaining")]
        private long? _remaining;

        /// <summary>
        /// This is the backing field for the <see cref="NextAvailable"/> property.
        /// </summary>
        [JsonProperty("next-available")]
        private DateTimeOffset? _nextAvailable;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="DnsRateLimit"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected DnsRateLimit()
        {
        }

        /// <summary>
        /// Gets the HTTP method associated with the rate limit.
        /// </summary>
        /// <value>
        /// The specific HTTP method which is rate limited, or <see langword="null"/> if the JSON
        /// response from the server did not include this property.
        /// </value>
        public HttpMethod? Verb
        {
            get
            {
                return _verb;
            }
        }

        /// <summary>
        /// Gets the time unit this rate limit is measured in.
        /// </summary>
        /// <value>
        /// The time unit used for measuring this rate limit, or <see langword="null"/> if the JSON
        /// response from the server did not include this property.
        /// </value>
        public DnsRateLimitUnit Unit
        {
            get
            {
                return _unit;
            }
        }

        /// <summary>
        /// Gets the rate limit, in number of times an API call can be made using the
        /// <see cref="Verb"/> HTTP method in the measuring time <see cref="Unit"/>.
        /// </summary>
        /// <value>
        /// The rate limit value, or <see langword="null"/> if the JSON response from the server
        /// did not include this property.
        /// </value>
        public long? Value
        {
            get
            {
                return _value;
            }
        }

        /// <summary>
        /// Gets the number of calls remaining in the current time unit.
        /// </summary>
        /// <value>
        /// The number of calls remaining within the current rate limit time unit, or
        /// <see langword="null"/> if the JSON response from the server did not include this property.
        /// </value>
        public long? Remaining
        {
            get
            {
                return _remaining;
            }
        }

        /// <summary>
        /// Gets a timestamp indicating when the rate limit next resets.
        /// </summary>
        /// <value>
        /// The timestamp for when the rate limit next resets, or <see langword="null"/> if the
        /// JSON response from the server did not include this property.
        /// </value>
        public DateTimeOffset? NextAvailable
        {
            get
            {
                return _nextAvailable;
            }
        }
    }
}
