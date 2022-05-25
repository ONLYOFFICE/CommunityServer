namespace net.openstack.Providers.Rackspace.Objects.AutoScale
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// This class models the JSON representation of a request to create a new
    /// <see cref="Webhook"/> resource in the <see cref="IAutoScaleService"/>.
    /// </summary>
    /// <seealso cref="IAutoScaleService.CreateWebhookAsync"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class NewWebhookConfiguration : WebhookConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NewWebhookConfiguration"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected NewWebhookConfiguration()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NewWebhookConfiguration"/> class
        /// with the specified name.
        /// </summary>
        /// <param name="name">The webhook name.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="name"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="name"/> is empty.</exception>
        public NewWebhookConfiguration(string name)
            : base(name, null)
        {
            if (name == null)
                throw new ArgumentNullException("name");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NewWebhookConfiguration"/> class
        /// with the specified name and metadata.
        /// </summary>
        /// <param name="name">The webhook name.</param>
        /// <param name="metadata">The metadata to associate with the webhook.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="name"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="name"/> is empty.</exception>
        public NewWebhookConfiguration(string name, IDictionary<string, string> metadata)
            : base(name, metadata)
        {
            if (name == null)
                throw new ArgumentNullException("name");
        }
    }
}
