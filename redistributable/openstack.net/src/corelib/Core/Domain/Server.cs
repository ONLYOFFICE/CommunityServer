namespace net.openstack.Core.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using net.openstack.Core.Exceptions.Response;
    using net.openstack.Core.Providers;
    using Newtonsoft.Json;

    /// <summary>
    /// Extends <see cref="SimpleServer"/> with detailed information about a server.
    /// </summary>
    /// <seealso cref="IComputeProvider.GetDetails"/>
    /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/Get_Server_Details-d1e2623.html">Get Server Details (OpenStack Compute API v2 and Extensions Reference)</seealso>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    public class Server : SimpleServer
    {
        private SimpleServerImage _image;

        /// <summary>
        /// Gets the disk configuration used for creating, rebuilding, or resizing the server.
        /// If the value was not explicitly specified in the create, rebuild, or resize request,
        /// the server inherits the value from the image it was created from.
        /// </summary>
        /// <remarks>
        /// <note>This property is defined by the Rackspace-specific Disk Configuration Extension to the OpenStack Compute API.</note>
        /// </remarks>
        /// <seealso href="http://docs.rackspace.com/servers/api/v2/cs-devguide/content/ch_extensions.html#diskconfig_attribute">Disk Configuration Extension (Rackspace Next Generation Cloud Servers Developer Guide - API v2)</seealso>
        [JsonProperty("OS-DCF:diskConfig")]
        public DiskConfiguration DiskConfig { get; private set; }

        /// <summary>
        /// Gets the power state for the server.
        /// </summary>
        /// <remarks>
        /// <note>This property is defined by the Rackspace-specific Extended Status Extension to the OpenStack Compute API.</note>
        /// </remarks>
        /// <seealso href="http://docs.rackspace.com/servers/api/v2/cs-devguide/content/ch_extensions.html#power_state">OS-EXT-STS:power_state (Rackspace Next Generation Cloud Servers Developer Guide - API v2)</seealso>
        [JsonProperty("OS-EXT-STS:power_state")]
        public PowerState PowerState { get; private set; }

        /// <summary>
        /// Gets the task state for the server.
        /// </summary>
        /// <remarks>
        /// <note>This property is defined by the Rackspace-specific Extended Status Extension to the OpenStack Compute API.</note>
        /// </remarks>
        /// <seealso href="http://docs.rackspace.com/servers/api/v2/cs-devguide/content/ch_extensions.html#task_state">OS-EXT-STS:task_state (Rackspace Next Generation Cloud Servers Developer Guide - API v2)</seealso>
        [JsonProperty("OS-EXT-STS:task_state")]
        public TaskState TaskState { get; private set; }

        /// <summary>
        /// Gets the virtual machine (VM) state for the server.
        /// </summary>
        /// <remarks>
        /// <note>This property is defined by the Rackspace-specific Extended Status Extension to the OpenStack Compute API.</note>
        /// </remarks>
        /// <seealso href="http://docs.rackspace.com/servers/api/v2/cs-devguide/content/ch_extensions.html#vm_state">OS-EXT-STS:vm_state (Rackspace Next Generation Cloud Servers Developer Guide - API v2)</seealso>
        [JsonProperty("OS-EXT-STS:vm_state")]
        public VirtualMachineState VMState { get; private set; }

        /// <summary>
        /// Gets the public IP version 4 access address.
        /// <note type="warning">The value of this property is not defined by OpenStack, and may not be consistent across vendors.</note>
        /// </summary>
        [JsonProperty("accessIPv4")]
        public string AccessIPv4 { get; private set; }

        /// <summary>
        /// Gets the public IP version 6 access address.
        /// <note type="warning">The value of this property is not defined by OpenStack, and may not be consistent across vendors.</note>
        /// </summary>
        [JsonProperty("accessIPv6")]
        public string AccessIPv6 { get; private set; }

        /// <summary>
        /// Gets the user ID for the server.
        /// <note type="warning">The value of this property is not defined by OpenStack, and may not be consistent across vendors.</note>
        /// </summary>
        [JsonProperty("user_id")]
        public string UserId { get; private set; }

        /// <summary>
        /// Gets basic information about the image the server was created from.
        /// <note type="warning">The value of this property is not defined by OpenStack, and may not be consistent across vendors.</note>
        /// </summary>
        [JsonProperty("image")]
        public SimpleServerImage Image
        {
            get
            {
                // this is handled in the getter because the Provider/Region/Identity
                // properties of the current instance might not be set at the point
                // this property is set
                if (_image != null)
                {
                    _image.Provider = Provider;
                    _image.Region = Region;
                    _image.Identity = Identity;
                }

                return _image;
            }

            private set
            {
                _image = value;
            }
        }

        /// <summary>
        /// Gets the server status.
        /// <note type="warning">The value of this property is not defined by OpenStack, and may not be consistent across vendors.</note>
        /// </summary>
        [JsonProperty("status")]
        public ServerState Status
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets basic information about the flavor for the server.
        /// <note type="warning">The value of this property is not defined by OpenStack, and may not be consistent across vendors.</note>
        /// </summary>
        [JsonProperty("flavor")]
        public Flavor Flavor { get; private set; }

        /// <summary>
        /// Gets the public and private IP addresses for the server.
        /// <note type="warning">The value of this property is not defined by OpenStack, and may not be consistent across vendors.</note>
        /// </summary>
        [JsonProperty("addresses")]
        public ServerAddresses Addresses { get; private set; }

        /// <summary>
        /// Gets the time stamp for the creation date.
        /// <note type="warning">The value of this property is not defined by OpenStack, and may not be consistent across vendors.</note>
        /// </summary>
        [JsonProperty("created")]
        public DateTimeOffset Created { get; private set; }

        /// <summary>
        /// Gets the host ID for the server.
        /// </summary>
        /// <remarks>
        /// The compute provisioning algorithm has an anti-affinity property that attempts
        /// to spread customer VMs across hosts. Under certain situations, VMs from the
        /// same customer might be placed on the same host. The Host ID represents the host
        /// your server runs on and can be used to determine this scenario if it is relevant
        /// to your application.
        ///
        /// <para><see cref="HostId"/> is unique <em>per account</em> and is not globally unique.</para>
        /// </remarks>
        [JsonProperty("hostId")]
        public string HostId { get; private set; }

        /// <summary>
        /// Gets the build completion progress, as a percentage.
        /// <note type="warning">The value of this property is not defined by OpenStack, and may not be consistent across vendors.</note>
        /// </summary>
        /// <value>A percentage from 0 to 100 (inclusive) representing the build completion progress.</value>
        [JsonProperty("progress")]
        public int Progress { get; private set; }

        /// <summary>
        /// Gets the tenant ID of the server.
        /// <note type="warning">The value of this property is not defined by OpenStack, and may not be consistent across vendors.</note>
        /// </summary>
        /// <seealso cref="Tenant.Id"/>
        [JsonProperty("tenant_id")]
        public string TenantId { get; private set; }

        /// <summary>
        /// Gets the time stamp for the last update.
        /// <note type="warning">The value of this property is not defined by OpenStack, and may not be consistent across vendors.</note>
        /// </summary>
        [JsonProperty("updated")]
        public DateTimeOffset Updated { get; private set; }

        /// <inheritdoc/>
        protected override void UpdateThis(ServerBase server)
        {
            if (server == null)
                throw new ArgumentNullException("server");

            base.UpdateThis(server);

            var details = server as Server;

            if (details == null)
                return;

            DiskConfig = details.DiskConfig;
            PowerState = details.PowerState;
            TaskState = details.TaskState;
            VMState = details.VMState;
            AccessIPv4 = details.AccessIPv4;
            AccessIPv6 = details.AccessIPv6;
            UserId = details.UserId;
            Image = details.Image;
            Status = details.Status;
            Flavor = details.Flavor;
            Addresses = details.Addresses;
            Created = details.Created;
            HostId = details.HostId;
            Progress = details.Progress;
            TenantId = details.TenantId;
            Updated = details.Updated;
        }

        /// <summary>
        /// Lists the volume attachments for the server.
        /// </summary>
        /// <returns>A collection of <see cref="ServerVolume"/> objects describing the volumes attached to the server.</returns>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso cref="IComputeProvider.ListServerVolumes"/>
        /// <seealso href="http://docs.rackspace.com/servers/api/v2/cs-devguide/content/List_Volume_Attachments.html">List Volume Attachments (Rackspace Next Generation Cloud Servers Developer Guide - API v2)</seealso>
        public IEnumerable<ServerVolume> GetVolumes()
        {
            return Provider.ListServerVolumes(Id, Region, Identity);
        }

        /// <summary>
        /// Gets the metadata associated with the server.
        /// </summary>
        /// <returns>A <see cref="Metadata"/> object containing the metadata associated with the server.</returns>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso cref="IComputeProvider.ListServerMetadata"/>
        /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/List_Metadata-d1e5089.html">List Metadata (OpenStack Compute API v2 and Extensions Reference)</seealso>
        public Metadata GetMetadata()
        {
            return Provider.ListServerMetadata(Id, Region, Identity);
        }

        /// <summary>
        /// Sets the metadata associated with the server, replacing any existing metadata.
        /// </summary>
        /// <param name="metadata">The metadata to associate with the server.</param>
        /// <returns><see langword="true"/> if the metadata for the server was successfully updated; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="metadata"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="metadata"/> contains any values with empty keys.</exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso cref="IComputeProvider.SetServerMetadata"/>
        /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/Create_or_Replace_Metadata-d1e5358.html">Set Metadata (OpenStack Compute API v2 and Extensions Reference)</seealso>
        public bool SetMetadata(Metadata metadata)
        {
            return Provider.SetServerMetadata(Id, metadata, Region, Identity);
        }

        /// <summary>
        /// Updates the metadata for the server.
        /// </summary>
        /// <remarks>
        /// For each item in <paramref name="metadata"/>, if the key exists, the value is updated; otherwise, the item is added.
        /// </remarks>
        /// <param name="metadata">The server metadata to update.</param>
        /// <returns><see langword="true"/> if the metadata for the server was successfully updated; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="metadata"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="metadata"/> contains any values with empty keys.</exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso cref="IComputeProvider.UpdateServerMetadata"/>
        /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/Update_Metadata-d1e5208.html">Update Metadata (OpenStack Compute API v2 and Extensions Reference)</seealso>
        public bool AddMetadata(Metadata metadata)
        {
            return Provider.UpdateServerMetadata(Id, metadata, Region, Identity);
        }

        /// <summary>
        /// Adds or updates the value for the specified metadata item.
        /// </summary>
        /// <param name="key">The metadata key.</param>
        /// <param name="value">The new value for the metadata item.</param>
        /// <returns><see langword="true"/> if the metadata for the server was successfully updated; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="key"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="value"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">If <paramref name="key"/> is empty.</exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso cref="IComputeProvider.SetServerMetadataItem"/>
        /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/Create_or_Update_a_Metadata_Item-d1e5633.html">Set Metadata Item (OpenStack Compute API v2 and Extensions Reference)</seealso>
        public bool AddMetadata(string key, string value)
        {
            return Provider.SetServerMetadataItem(Id, key, value, Region, Identity);
        }

        /// <summary>
        /// Updates the metadata for the server.
        /// </summary>
        /// <remarks>
        /// For each item in <paramref name="metadata"/>, if the key exists, the value is updated; otherwise, the item is added.
        /// </remarks>
        /// <param name="metadata">The server metadata to update.</param>
        /// <returns><see langword="true"/> if the metadata for the server was successfully updated; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="metadata"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="metadata"/> contains any values with empty keys.</exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso cref="IComputeProvider.UpdateServerMetadata"/>
        /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/Update_Metadata-d1e5208.html">Update Metadata (OpenStack Compute API v2 and Extensions Reference)</seealso>
        public bool UpdateMetadata(Metadata metadata)
        {
            return Provider.UpdateServerMetadata(Id, metadata, Region, Identity);
        }

        /// <summary>
        /// Deletes the specified metadata items from the server.
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
        /// <seealso cref="IComputeProvider.DeleteServerMetadataItem"/>
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
        /// Deletes the specified metadata item from the server.
        /// </summary>
        /// <param name="key">The metadata key.</param>
        /// <returns><see langword="true"/> if the metadata item was removed; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="key"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="key"/> is empty.</exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso cref="IComputeProvider.DeleteServerMetadataItem"/>
        /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/Delete_Metadata_Item-d1e5790.html">Delete Metadata Item (OpenStack Compute API v2 and Extensions Reference)</seealso>
        public bool DeleteMetadataItem(string key)
        {
            return Provider.DeleteServerMetadataItem(Id, key, Region, Identity);
        }

        /// <summary>
        /// Sets the value for the specified metadata item. If the key already exists, it is updated; otherwise, a new metadata item is added.
        /// </summary>
        /// <param name="key">The metadata key.</param>
        /// <param name="value">The new value for the metadata item.</param>
        /// <returns><see langword="true"/> if the metadata for the server was successfully updated; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="key"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="value"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">If <paramref name="key"/> is empty.</exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso cref="IComputeProvider.SetServerMetadataItem"/>
        /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/Create_or_Update_a_Metadata_Item-d1e5633.html">Set Metadata Item (OpenStack Compute API v2 and Extensions Reference)</seealso>
        public bool UpdateMetadataItem(string key, string value)
        {
            return Provider.SetServerMetadataItem(Id, key, value, Region, Identity);
        }

        /// <summary>
        /// Lists all networks and server addresses associated with a server.
        /// </summary>
        /// <returns>A <see cref="ServerAddresses"/> object containing the list of network addresses for the server.</returns>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso cref="IComputeProvider.ListAddresses"/>
        /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/List_Addresses-d1e3014.html">List Addresses (OpenStack Compute API v2 and Extensions Reference)</seealso>
        public ServerAddresses ListAddresses()
        {
            return Provider.ListAddresses(Id, Region, Identity);
        }

        /// <summary>
        /// Lists addresses for the server associated with the specified network.
        /// </summary>
        /// <param name="networkLabel">The network label. This is obtained from <see cref="CloudNetwork.Label">CloudNetwork.Label</see>.</param>
        /// <returns>A collection of <see cref="IPAddress"/> containing the network addresses associated with the server on the specified network.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="networkLabel"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="networkLabel"/> is empty.</exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso cref="IComputeProvider.ListAddressesByNetwork"/>
        /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/List_Addresses_by_Network-d1e3118.html">List Addresses by Network (OpenStack Compute API v2 and Extensions Reference)</seealso>
        public IEnumerable<IPAddress> ListAddressesByNetwork(string networkLabel)
        {
            if (networkLabel == null)
                throw new ArgumentNullException("networkLabel");
            if (string.IsNullOrEmpty(networkLabel))
                throw new ArgumentException("networkLabel cannot be empty");

            return Provider.ListAddressesByNetwork(Id, networkLabel, Region, Identity);
        }

        /// <summary>
        /// Creates a new snapshot image for the server at its current state.
        /// </summary>
        /// <remarks>
        /// The server snapshot process is completed asynchronously. To wait for the image
        /// to be completed, you may call <see cref="SimpleServerImage.WaitForActive"/> on the
        /// returned image.
        /// </remarks>
        /// <param name="imageName">Name of the new image.</param>
        /// <param name="metadata">The metadata to associate to the new image.</param>
        /// <returns>A <see cref="ServerImage"/> object containing the details of the new image if the image creation process was successfully started; otherwise, <see langword="null"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="imageName"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="imageName"/> is empty.</exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/Create_Image-d1e4655.html">Create Image (OpenStack Compute API v2 and Extensions Reference)</seealso>
        public ServerImage Snapshot(string imageName, Metadata metadata = null)
        {
            if (!Provider.CreateImage(Id, imageName, metadata, Region, Identity))
                return null;

            return Provider.ListImagesWithDetails(Id, imageName, imageType: ImageType.Snapshot)
                        .OrderByDescending(i => i.Created)
                        .FirstOrDefault();
        }

        /// <summary>
        /// Marks the server for asynchronous deletion.
        /// </summary>
        /// <remarks>
        /// The server deletion operation is completed asynchronously. The <see cref="ServerBase.WaitForDeleted"/>
        /// method may be used to block execution until the server is finally deleted.
        /// </remarks>
        /// <returns><see langword="true"/> if the server was successfully marked for deletion; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso cref="IComputeProvider.DeleteServer"/>
        /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/Delete_Server-d1e2883.html">Delete Server (OpenStack Compute API v2 and Extensions Reference)</seealso>
        public bool Delete()
        {
            return Provider.DeleteServer(Id, Region, Identity);
        }

        /// <summary>
        /// Lists the virtual interfaces for the server.
        /// </summary>
        /// <returns>A collection of <see cref="VirtualInterface"/> objects describing the virtual interfaces for the server.</returns>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso cref="IComputeProvider.ListVirtualInterfaces"/>
        /// <seealso href="http://docs.rackspace.com/networks/api/v2/cn-devguide/content/list_virt_interfaces.html">List Virtual Interfaces (Rackspace Cloud Networks Developer Guide - OpenStack Networking API v2)</seealso>
        public IEnumerable<VirtualInterface> ListVirtualInterfaces()
        {
            return Provider.ListVirtualInterfaces(Id, Region, Identity);
        }

        /// <summary>
        /// Creates a virtual interface for the specified network and attaches the network to the server.
        /// </summary>
        /// <param name="networkId">The network ID. This is obtained from <see cref="CloudNetwork.Id">CloudNetwork.Id</see>.</param>
        /// <returns>A <see cref="VirtualInterface"/> object containing the details of the newly-created virtual network.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="networkId"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="networkId"/> is empty.</exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso cref="IComputeProvider.CreateVirtualInterface"/>
        /// <seealso href="http://docs.rackspace.com/networks/api/v2/cn-devguide/content/api_create_virtual_interface.html">Create Virtual Interface (Rackspace Cloud Networks Developer Guide - OpenStack Networking API v2)</seealso>
        public VirtualInterface CreateVirtualInterface(string networkId)
        {
            return Provider.CreateVirtualInterface(Id, networkId, Region, Identity);
        }

        /// <summary>
        /// Deletes the specified virtual interface from the server.
        /// </summary>
        /// <param name="virtualInterfaceId">The virtual interface ID. This is obtained from <see cref="VirtualInterface.Id">VirtualInterface.Id</see>.</param>
        /// <returns><see langword="true"/> if the virtual interface was successfully removed from the server; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="virtualInterfaceId"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="virtualInterfaceId"/> is empty.</exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso cref="IComputeProvider.DeleteVirtualInterface"/>
        /// <seealso href="http://docs.rackspace.com/networks/api/v2/cn-devguide/content/delete_virt_interface_api.html">Delete Virtual Interface (Rackspace Cloud Networks Developer Guide - OpenStack Networking API v2)</seealso>
        public bool DeleteVirtualInterface(string virtualInterfaceId)
        {
            return Provider.DeleteVirtualInterface(Id, virtualInterfaceId, Region, Identity);
        }
    }
}
