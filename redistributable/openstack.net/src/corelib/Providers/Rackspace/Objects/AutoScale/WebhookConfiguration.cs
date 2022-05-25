using System.Collections.ObjectModel;

namespace net.openstack.Providers.Rackspace.Objects.AutoScale
{
    using System;
    using System.Collections.Generic;

    using Newtonsoft.Json;

    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This class models the base configuration information of a webhook resource
    /// in the <see cref="IAutoScaleService"/>.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class WebhookConfiguration : ExtensibleJsonObject
    {
        /// <summary>
        /// This is the backing field for the <see cref="Name"/> property.
        /// </summary>
        [JsonProperty("name", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private string _name;

        /// <summary>
        /// This is the backing field for the <see cref="Metadata"/> property.
        /// </summary>
        [JsonProperty("metadata", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private Dictionary<string, string> _metadata;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebhookConfiguration"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected WebhookConfiguration()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebhookConfiguration"/> class
        /// with the specified name and metadata.
        /// </summary>
        /// <param name="name">The webhook name.</param>
        /// <param name="metadata">The metadata to associate with the webhook.</param>
        /// <exception cref="ArgumentException">If <paramref name="name"/> is empty.</exception>
        protected WebhookConfiguration(string name, IDictionary<string, string> metadata)
        {
            if (name == string.Empty)
                throw new ArgumentException("name cannot be empty");

            _name = name;
            if (metadata != null)
                _metadata = new Dictionary<string, string>(metadata);
        }

        /// <summary>
        /// Gets the name of the webhook.
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
        }

        /// <summary>
        /// Gets a collection of metadata associated with the webhook.
        /// </summary>
        public ReadOnlyDictionary<string, string> Metadata
        {
            get
            {
                if (_metadata == null)
                    return null;

                return new ReadOnlyDictionary<string, string>(_metadata);
            }
        }
    }
}
