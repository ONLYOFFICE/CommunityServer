namespace net.openstack.Providers.Rackspace.Objects.AutoScale
{
    using System;
    using net.openstack.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the unique identifier of a webhook in the <see cref="IAutoScaleService"/>.
    /// </summary>
    /// <seealso cref="Webhook.Id"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonConverter(typeof(WebhookId.Converter))]
    public sealed class WebhookId : ResourceIdentifier<WebhookId>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebhookId"/> class
        /// with the specified identifier value.
        /// </summary>
        /// <param name="id">The scaling group identifier value.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="id"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="id"/> is empty.</exception>
        public WebhookId(string id)
            : base(id)
        {
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="WebhookId"/>
        /// objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        private sealed class Converter : ConverterBase
        {
            /// <inheritdoc/>
            protected override WebhookId FromValue(string id)
            {
                return new WebhookId(id);
            }
        }
    }
}
