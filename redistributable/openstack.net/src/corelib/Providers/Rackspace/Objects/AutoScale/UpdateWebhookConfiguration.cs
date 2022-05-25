namespace net.openstack.Providers.Rackspace.Objects.AutoScale
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// This class models the JSON representation of a request to update the properties
    /// of a <see cref="Webhook"/> resource in the <see cref="IAutoScaleService"/>.
    /// </summary>
    /// <seealso cref="IAutoScaleService.UpdateWebhookAsync"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class UpdateWebhookConfiguration : WebhookConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateWebhookConfiguration"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected UpdateWebhookConfiguration()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateWebhookConfiguration"/> class
        /// with the specified name.
        /// </summary>
        /// <param name="name">The name of the webhook.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="name"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="name"/> is empty.</exception>
        public UpdateWebhookConfiguration(string name)
            : base(name, null)
        {
            if (name == null)
                throw new ArgumentNullException("name");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateWebhookConfiguration"/> class
        /// with the specified metadata.
        /// </summary>
        /// <param name="metadata">A collection of metadata to associate with the webhook resource.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="metadata"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="metadata"/> contains any empty keys.</exception>
        public UpdateWebhookConfiguration(IDictionary<string, string> metadata)
            : base(null, metadata)
        {
            if (metadata == null)
                throw new ArgumentNullException("metadata");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateWebhookConfiguration"/> class
        /// with the specified name and metadata.
        /// </summary>
        /// <param name="name">The name of the webhook.</param>
        /// <param name="metadata">A collection of metadata to associate with the webhook resource.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="name"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="metadata"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="name"/> is empty.
        /// <para>-or-</para>
        /// <para>If <paramref name="metadata"/> contains any empty keys.</para>
        /// </exception>
        public UpdateWebhookConfiguration(string name, IDictionary<string, string> metadata)
            : base(name, metadata)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (metadata == null)
                throw new ArgumentNullException("metadata");
        }
    }
}
