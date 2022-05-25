namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System;
    using net.openstack.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the unique identifier of a webhook token in the <see cref="IMonitoringService"/>.
    /// </summary>
    /// <seealso cref="AccountConfiguration.WebhookToken"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonConverter(typeof(WebhookToken.Converter))]
    public sealed class WebhookToken : ResourceIdentifier<WebhookToken>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebhookToken"/> class
        /// with the specified identifier value.
        /// </summary>
        /// <param name="id">The webhook token identifier value.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="id"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="id"/> is empty.</exception>
        public WebhookToken(string id)
            : base(id)
        {
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="WebhookToken"/>
        /// objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        private sealed class Converter : ConverterBase
        {
            /// <inheritdoc/>
            protected override WebhookToken FromValue(string id)
            {
                return new WebhookToken(id);
            }
        }
    }
}
