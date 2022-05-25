namespace net.openstack.Providers.Rackspace.Objects.Request
{
    using System;
    using net.openstack.Core.Domain;
    using Newtonsoft.Json;

    /// <summary>
    /// This models the JSON request used for the Set Metadata Item request.
    /// </summary>
    /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/Create_or_Update_a_Metadata_Item-d1e5633.html">Set Metadata Item (OpenStack Compute API v2 and Extensions Reference)</seealso>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class UpdateMetadataItemRequest
    {
        /// <summary>
        /// Gets the metadata item to associate with the server or image.
        /// </summary>
        /// <remarks>
        /// The value is never <see langword="null"/> and always contains exactly one entry.
        /// </remarks>
        [JsonProperty("meta")]
        public Metadata Metadata { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateMetadataItemRequest"/>
        /// class with the specified key and value.
        /// </summary>
        /// <param name="key">The metadata key.</param>
        /// <param name="value">The new value for the metadata item.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="key"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="value"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">If <paramref name="key"/> is empty.</exception>
        public UpdateMetadataItemRequest(string key, string value)
        {
            if (key == null)
                throw new ArgumentNullException("key");
            if (value == null)
                throw new ArgumentNullException("value");
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("key cannot be empty");

            Metadata = new Metadata() { { key, value } };
        }
    }
}