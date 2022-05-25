using System.Collections.ObjectModel;

namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System.Collections.Generic;

    using Newtonsoft.Json;

    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This class models the JSON representation of the resource and rate limits
    /// applied to the <see cref="IMonitoringService"/>.
    /// </summary>
    /// <seealso cref="IMonitoringService.GetLimitsAsync"/>
    /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-account.html#service-account-get-limits">Get Limits (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class MonitoringLimits : ExtensibleJsonObject
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value {value}
        /// <summary>
        /// This is the backing field for the <see cref="ResourceLimits"/> property.
        /// </summary>
        [JsonProperty("resource")]
        private IDictionary<string, int> _resourceLimits;

        /// <summary>
        /// This is the backing field for the <see cref="RateLimits"/> property.
        /// </summary>
        [JsonProperty("rate")]
        private IDictionary<string, RateLimit> _rateLimits;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="MonitoringLimits"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected MonitoringLimits()
        {
        }

        /// <summary>
        /// Gets a collection of absolute resource limits in effect for the monitoring service.
        /// </summary>
        public ReadOnlyDictionary<string, int> ResourceLimits
        {
            get
            {
                if (_resourceLimits == null)
                    return null;

                return new ReadOnlyDictionary<string, int>(_resourceLimits);
            }
        }

        /// <summary>
        /// Gets a collection of rate limits in effect for the monitoring service.
        /// </summary>
        public ReadOnlyDictionary<string, RateLimit> RateLimits
        {
            get
            {
                if (_rateLimits == null)
                    return null;

                return new ReadOnlyDictionary<string, RateLimit>(_rateLimits);
            }
        }

        /// <summary>
        /// This class models the JSON representation of the details of a rate limit in the <see cref="IMonitoringService"/>.
        /// </summary>
        /// <seealso cref="RateLimits"/>
        /// <threadsafety static="true" instance="false"/>
        /// <preliminary/>
        [JsonObject(MemberSerialization.OptIn)]
        public class RateLimit : ExtensibleJsonObject
        {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value {value}
            /// <summary>
            /// This is the backing field for the <see cref="Limit"/> property.
            /// </summary>
            [JsonProperty("limit")]
            private int? _limit;

            /// <summary>
            /// This is the backing field for the <see cref="Used"/> property.
            /// </summary>
            [JsonProperty("used")]
            private int? _used;

            /// <summary>
            /// This is the backing field for the <see cref="Window"/> property.
            /// </summary>
            [JsonProperty("window")]
            private string _window;
#pragma warning restore 649

            /// <summary>
            /// Initializes a new instance of the <see cref="RateLimit"/> class
            /// during JSON deserialization.
            /// </summary>
            [JsonConstructor]
            protected RateLimit()
            {
            }

            /// <summary>
            /// Gets the number of times the rate limited item is allowed to be used during a rate limit period.
            /// </summary>
            public int? Limit
            {
                get
                {
                    return _limit;
                }
            }

            /// <summary>
            /// Gets the number of times the rate limited item has been used during the current rate limiting period.
            /// </summary>
            public int? Used
            {
                get
                {
                    return _used;
                }
            }

            /// <summary>
            /// Gets a description of the period between resetting the rate limit.
            /// </summary>
            public string Window
            {
                get
                {
                    return _window;
                }
            }
        }
    }
}
