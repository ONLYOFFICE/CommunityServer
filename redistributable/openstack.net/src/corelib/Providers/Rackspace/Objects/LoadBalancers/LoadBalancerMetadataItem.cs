namespace net.openstack.Providers.Rackspace.Objects.LoadBalancers
{
    using System;
    using Newtonsoft.Json;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This class represents a metadata item associated with a resource in the load balancer service.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class LoadBalancerMetadataItem : ExtensibleJsonObject
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value
        /// <summary>
        /// This is the backing field for the <see cref="Id"/> property.
        /// </summary>
        [JsonProperty("id", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private MetadataId _id;
#pragma warning restore 649

        /// <summary>
        /// This is the backing field for the <see cref="Key"/> property.
        /// </summary>
        [JsonProperty("key", DefaultValueHandling = DefaultValueHandling.Include)]
        private string _key;

        /// <summary>
        /// This is the backing field for the <see cref="Value"/> property.
        /// </summary>
        [JsonProperty("value", DefaultValueHandling = DefaultValueHandling.Include)]
        private string _value;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadBalancerMetadataItem"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected LoadBalancerMetadataItem()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadBalancerMetadataItem"/> class
        /// with the specified key and value.
        /// </summary>
        /// <param name="key">The metadata key.</param>
        /// <param name="value">The metadata value.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="key"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="value"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">If <paramref name="key"/> is empty.</exception>
        public LoadBalancerMetadataItem(string key, string value)
        {
            if (key == null)
                throw new ArgumentNullException("key");
            if (value == null)
                throw new ArgumentNullException("value");
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("key cannot be empty");

            _key = key;
            _value = value;
        }

        /// <summary>
        /// Gets the unique ID for the metadata item.
        /// </summary>
        /// <remarks>
        /// Metadata IDs in the load balancer service are only guaranteed to be unique within the
        /// context of the item they are associated with.
        /// </remarks>
        public MetadataId Id
        {
            get
            {
                return _id;
            }
        }

        /// <summary>
        /// Gets the key for this metadata item.
        /// </summary>
        public string Key
        {
            get
            {
                return _key;
            }
        }

        /// <summary>
        /// Gets the value for this metadata item.
        /// </summary>
        public string Value
        {
            get
            {
                return _value;
            }
        }
    }
}
