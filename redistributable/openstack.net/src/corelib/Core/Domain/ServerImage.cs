namespace net.openstack.Core.Domain
{
    using System;
    using net.openstack.Core.Exceptions.Response;
    using net.openstack.Core.Providers;
    using Newtonsoft.Json;

    /// <summary>
    /// Extends <see cref="SimpleServerImage"/> with detailed information about an image.
    /// </summary>
    /// <seealso cref="IComputeProvider.ListImagesWithDetails"/>
    /// <seealso cref="IComputeProvider.GetImage"/>
    /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/Get_Image_Details-d1e4848.html">Get Image Details (OpenStack Compute API v2 and Extensions Reference)</seealso>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    public class ServerImage : SimpleServerImage
    {
        /// <summary>
        /// This is the backing field for the <see cref="Server"/> property.
        /// </summary>
        private SimpleServer _server;

        /// <summary>
        /// Gets the default disk configuration used when creating, rebuilding, or resizing servers
        /// with the image. For images created from servers, the value is inherited from the server.
        /// </summary>
        /// <remarks>
        /// <note>This property is defined by the Rackspace-specific Disk Configuration Extension to the OpenStack Compute API.</note>
        /// </remarks>
        /// <seealso href="http://docs.rackspace.com/servers/api/v2/cs-devguide/content/ch_extensions.html#diskconfig_attribute">Disk Configuration Extension (Rackspace Next Generation Cloud Servers Developer Guide - API v2)</seealso>
        [JsonProperty("OS-DCF:diskConfig")]
        public DiskConfiguration DiskConfig { get; private set; }

        /// <summary>
        /// Gets the "status" property of the image.
        /// <note type="warning">The value of this property is not defined. Do not use.</note>
        /// </summary>
        [JsonProperty("status")]
        public ImageState Status
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the "created" property of the image.
        /// <note type="warning">The value of this property is not defined. Do not use.</note>
        /// </summary>
        [JsonProperty("created")]
        public DateTimeOffset Created { get; private set; }

        /// <summary>
        /// Gets the image completion progress, as a percentage.
        /// <note type="warning">The value of this property is not defined. Do not use.</note>
        /// </summary>
        /// <value>A percentage from 0 to 100 (inclusive) representing the image completion progress.</value>
        [JsonProperty("progress")]
        public int Progress { get; private set; }

        /// <summary>
        /// Gets the "updated" property of the image.
        /// <note type="warning">The value of this property is not defined. Do not use.</note>
        /// </summary>
        [JsonProperty("updated")]
        public DateTimeOffset Updated { get; private set; }

        /// <summary>
        /// Gets the minimum disk requirements needed to create a server with the image.
        /// <note type="warning">The value of this property is not defined. Do not use.</note>
        /// </summary>
        /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/List_Images-d1e4435.html">List Images (OpenStack Compute API v2 and Extensions Reference)</seealso>
        [JsonProperty("minDisk")]
        public int MinDisk { get; private set; }

        /// <summary>
        /// Gets the "server" property of the image.
        /// <note type="warning">The value of this property is not defined. Do not use.</note>
        /// </summary>
        [JsonProperty("server")]
        public SimpleServer Server
        {
            get
            {
                // this is handled in the getter because the Provider/Region/Identity
                // properties of the current instance might not be set at the point
                // this property is set
                if (_server != null)
                {
                    _server.Provider = Provider;
                    _server.Region = Region;
                    _server.Identity = Identity;
                }

                return _server;
            }

            private set
            {
                _server = value;
            }
        }

        /// <summary>
        /// Gets the minimum RAM requirements needed to create a server with the image.
        /// <note type="warning">The value of this property is not defined. Do not use.</note>
        /// </summary>
        /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/List_Images-d1e4435.html">List Images (OpenStack Compute API v2 and Extensions Reference)</seealso>
        [JsonProperty("minRam")]
        public int MinRAM { get; private set; }

        /// <inheritdoc/>
        protected override void UpdateThis(SimpleServerImage serverImage)
        {
            if (serverImage == null)
                throw new ArgumentNullException("serverImage");

            base.UpdateThis(serverImage);

            var details = serverImage as ServerImage;

            if (details == null)
                return;

            DiskConfig = details.DiskConfig;
            Status = details.Status;
            Created = details.Created;
            Progress = details.Progress;
            Updated = details.Updated;
            MinDisk = details.MinDisk;
            MinRAM = details.MinRAM;
        }

        /// <summary>
        /// Gets the metadata associated with the specified image.
        /// </summary>
        /// <returns>A <see cref="Metadata"/> object containing the metadata associated with the image.</returns>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso cref="IComputeProvider.ListImageMetadata"/>
        /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/List_Metadata-d1e5089.html">List Metadata (OpenStack Compute API v2 and Extensions Reference)</seealso>
        public Metadata GetMetadata()
        {
            return Provider.ListImageMetadata(Id, Region, Identity);
        }

        /// <summary>
        /// Sets the metadata associated with the specified image, replacing any existing metadata.
        /// </summary>
        /// <param name="metadata">The metadata to associate with the image.</param>
        /// <returns><see langword="true"/> if the metadata for the image was successfully updated; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="metadata"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="metadata"/> contains any values with empty keys.</exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso cref="IComputeProvider.SetImageMetadata"/>
        /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/Create_or_Replace_Metadata-d1e5358.html">Set Metadata (OpenStack Compute API v2 and Extensions Reference)</seealso>
        public bool SetMetadata(Metadata metadata)
        {
            if (metadata == null)
                throw new ArgumentNullException("metadata");

            return Provider.SetImageMetadata(Id, metadata, Region, Identity);
        }

        /// <summary>
        /// Updates the metadata for the specified image.
        /// </summary>
        /// <remarks>
        /// For each item in <paramref name="metadata"/>, if the key exists, the value is updated; otherwise, the item is added.
        /// </remarks>
        /// <param name="metadata">The image metadata to update.</param>
        /// <returns><see langword="true"/> if the metadata for the image was successfully updated; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="metadata"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="metadata"/> contains any values with empty keys.</exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso cref="IComputeProvider.UpdateImageMetadata"/>
        /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/Update_Metadata-d1e5208.html">Update Metadata (OpenStack Compute API v2 and Extensions Reference)</seealso>
        public bool AddMetadata(Metadata metadata)
        {
            if (metadata == null)
                throw new ArgumentNullException("metadata");

            return Provider.UpdateImageMetadata(Id, metadata, Region, Identity);
        }

        /// <summary>
        /// Sets the value for the specified metadata item. If the key already exists, it is updated; otherwise, a new metadata item is added.
        /// </summary>
        /// <param name="key">The metadata key.</param>
        /// <param name="value">The new value for the metadata item.</param>
        /// <returns><see langword="true"/> if the metadata for the image was successfully updated; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="key"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="value"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">If <paramref name="key"/> is empty.</exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso cref="IComputeProvider.SetImageMetadataItem"/>
        /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/Create_or_Update_a_Metadata_Item-d1e5633.html">Set Metadata Item (OpenStack Compute API v2 and Extensions Reference)</seealso>
        public bool AddMetadata(string key, string value)
        {
            if (key == null)
                throw new ArgumentNullException("key");
            if (value == null)
                throw new ArgumentNullException("value");
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("key cannot be empty");

            return Provider.SetImageMetadataItem(Id, key, value, Region, Identity);
        }

        /// <summary>
        /// Updates the metadata for the specified image.
        /// </summary>
        /// <remarks>
        /// For each item in <paramref name="metadata"/>, if the key exists, the value is updated; otherwise, the item is added.
        /// </remarks>
        /// <param name="metadata">The image metadata to update.</param>
        /// <returns><see langword="true"/> if the metadata for the image was successfully updated; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="metadata"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="metadata"/> contains any values with empty keys.</exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso cref="IComputeProvider.UpdateImageMetadata"/>
        /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/Update_Metadata-d1e5208.html">Update Metadata (OpenStack Compute API v2 and Extensions Reference)</seealso>
        public bool UpdateMetadata(Metadata metadata)
        {
            if (metadata == null)
                throw new ArgumentNullException("metadata");

            return Provider.UpdateImageMetadata(Id, metadata, Region, Identity);
        }

        /// <summary>
        /// Deletes the specified metadata items from the image.
        /// </summary>
        /// <remarks>
        /// <note>
        /// This method ignores the values in <paramref name="metadata"/>. Metadata items are
        /// removed whether or not their current values match those in <paramref name="metadata"/>.
        /// </note>
        /// </remarks>
        /// <param name="metadata">A collection of metadata items to delete.</param>
        /// <returns><see langword="true"/> if all of the metadata item were removed; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="metadata"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="metadata"/> contains a null or empty key.</exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso cref="IComputeProvider.DeleteImageMetadataItem"/>
        /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/Delete_Metadata_Item-d1e5790.html">Delete Metadata Item (OpenStack Compute API v2 and Extensions Reference)</seealso>
        public bool DeleteMetadata(Metadata metadata)
        {
            if (metadata == null)
                throw new ArgumentNullException("metadata");

            bool success = true;
            foreach (var item in metadata)
            {
                if (string.IsNullOrEmpty(item.Key))
                    throw new ArgumentException("metadata cannot contain any empty keys");

                success &= DeleteMetadataItem(item.Key);
            }

            return success;
        }

        /// <summary>
        /// Deletes the specified metadata item from the image.
        /// </summary>
        /// <param name="key">The metadata key.</param>
        /// <returns><see langword="true"/> if the metadata item was removed; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="key"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="key"/> is empty.</exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso cref="IComputeProvider.DeleteImageMetadataItem"/>
        /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/Delete_Metadata_Item-d1e5790.html">Delete Metadata Item (OpenStack Compute API v2 and Extensions Reference)</seealso>
        public bool DeleteMetadataItem(string key)
        {
            if (key == null)
                throw new ArgumentNullException("key");
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("key cannot be empty");

            return Provider.DeleteImageMetadataItem(Id, key, Region, Identity);
        }

        /// <summary>
        /// Sets the value for the specified metadata item. If the key already exists, it is updated; otherwise, a new metadata item is added.
        /// </summary>
        /// <param name="key">The metadata key.</param>
        /// <param name="value">The new value for the metadata item.</param>
        /// <returns><see langword="true"/> if the metadata for the image was successfully updated; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="key"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="value"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">If <paramref name="key"/> is empty.</exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso cref="IComputeProvider.SetImageMetadataItem"/>
        /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/Create_or_Update_a_Metadata_Item-d1e5633.html">Set Metadata Item (OpenStack Compute API v2 and Extensions Reference)</seealso>
        public bool UpdateMetadataItem(string key, string value)
        {
            if (key == null)
                throw new ArgumentNullException("key");
            if (value == null)
                throw new ArgumentNullException("value");
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("key cannot be empty");

            return Provider.SetImageMetadataItem(Id, key, value, Region, Identity);
        }
    }
}
