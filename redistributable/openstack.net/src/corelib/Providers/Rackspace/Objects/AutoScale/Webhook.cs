namespace net.openstack.Providers.Rackspace.Objects.AutoScale
{
    using System.Collections.ObjectModel;
    using net.openstack.Core.Domain;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a webhook resource in the <see cref="IAutoScaleService"/>.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class Webhook : WebhookConfiguration
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value {value}
        /// <summary>
        /// This is the backing field for the <see cref="Id"/> property.
        /// </summary>
        [JsonProperty("id")]
        private WebhookId _id;

        /// <summary>
        /// This is the backing field for the <see cref="Links"/> property.
        /// </summary>
        [JsonProperty("links")]
        private Link[] _links;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="Webhook"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected Webhook()
        {
        }

        /// <summary>
        /// Gets the unique identifier of the webhook.
        /// </summary>
        public WebhookId Id
        {
            get
            {
                return _id;
            }
        }

        /// <summary>
        /// Gets a collection of links to resources related to the webhook resource.
        /// </summary>
        public ReadOnlyCollection<Link> Links
        {
            get
            {
                if (_links == null)
                    return null;

                return new ReadOnlyCollection<Link>(_links);
            }
        }
    }
}
