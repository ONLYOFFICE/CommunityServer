namespace net.openstack.Providers.Rackspace.Objects.Request
{
    using System;
    using net.openstack.Core.Domain;
    using Newtonsoft.Json;

    /// <summary>
    /// This models the JSON request used for the Set Metadata and Update Metadata requests.
    /// </summary>
    /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/Create_or_Replace_Metadata-d1e5358.html">Set Metadata (OpenStack Compute API v2 and Extensions Reference)</seealso>
    /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/Update_Metadata-d1e5208.html">Update Metadata (OpenStack Compute API v2 and Extensions Reference)</seealso>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class UpdateMetadataRequest
    {
        /// <summary>
        /// Gets the metadata.
        /// </summary>
        [JsonProperty("metadata")]
        public Metadata Metadata { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateMetadataRequest"/> class
        /// with the given metadata.
        /// </summary>
        /// <param name="metadata">The metadata.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="metadata"/> is <see langword="null"/>.</exception>
        public UpdateMetadataRequest(Metadata metadata)
        {
            if (metadata == null)
                throw new ArgumentNullException("metadata");

            Metadata = metadata;
        }
    }
}
