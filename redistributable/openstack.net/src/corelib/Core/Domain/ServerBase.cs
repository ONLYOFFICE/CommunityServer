using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using net.openstack.Core.Exceptions;
using net.openstack.Core.Exceptions.Response;
using net.openstack.Core.Providers;
using Newtonsoft.Json;

namespace net.openstack.Core.Domain
{
    /// <summary>
    /// Provides basic information about a server.
    /// </summary>
    /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/Servers-d1e2073.html">Servers (OpenStack Compute API v2 and Extensions Reference - API v2)</seealso>
    /// <seealso href="http://docs.rackspace.com/servers/api/v2/cs-devguide/content/Servers-d1e2073.html">Servers (Rackspace Next Generation Cloud Servers Developer Guide  - API v2)</seealso>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class ServerBase : ProviderStateBase<IComputeProvider>
    {
        /// <summary>
        /// Gets the unique identifier for the server.
        /// <note type="warning">The value of this property is not defined by OpenStack, and may not be consistent across vendors.</note>
        /// </summary>
        [JsonProperty]
        public string Id { get; private set; }

        /// <summary>
        /// Gets a collection of links related to the current server.
        /// </summary>
        /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/LinksReferences.html">Links and References (OpenStack Compute API v2 and Extensions Reference - API v2)</seealso>
        [JsonProperty]
        public Link[] Links { get; private set; }

        /// <summary>
        /// Updates the current instance to match the information in <paramref name="server"/>.
        /// </summary>
        /// <remarks>
        /// <note type="implement">
        /// This method should be overridden in derived types to ensure all properties
        /// for the current instance are updated.
        /// </note>
        /// </remarks>
        /// <param name="server">The updated information for the current server.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="server"/> is <see langword="null"/>.</exception>
        protected virtual void UpdateThis(ServerBase server)
        {
            if (server == null)
                throw new ArgumentNullException("server");

            Id = server.Id;
            Links = server.Links;
        }

        /// <summary>
        /// Waits for the server to enter the <see cref="ServerState.Active"/> state.
        /// </summary>
        /// <remarks>
        /// When the method returns, the current instance is updated to reflect the state
        /// of the server at the end of the operation.
        ///
        /// <note type="caller">
        /// This is a blocking operation and will not return until the server enters the <see cref="ServerState.Active"/> state, an error state, or the retry count is exceeded.
        /// </note>
        /// </remarks>
        /// <param name="refreshCount">Number of times to poll the server's status.</param>
        /// <param name="refreshDelay">The time to wait between polling requests for the server status. If this value is <see langword="null"/>, the default is 2.4 seconds.</param>
        /// <param name="progressUpdatedCallback">A callback delegate to execute each time the <see cref="Server.Progress"/> value increases. If this value is <see langword="null"/>, progress updates are not reported.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="refreshCount"/> is less than 0.
        /// <para>-or-</para>
        /// <para>If <paramref name="refreshDelay"/> is negative.</para>
        /// </exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        public void WaitForActive(int refreshCount = 600, TimeSpan? refreshDelay = null, Action<int> progressUpdatedCallback = null)
        {
            if (refreshCount < 0)
                throw new ArgumentOutOfRangeException("refreshCount");
            if (refreshDelay < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException("refreshDelay");

            var details = Provider.WaitForServerActive(Id, refreshCount, refreshDelay ?? TimeSpan.FromMilliseconds(2400), progressUpdatedCallback, Region, Identity);
            UpdateThis(details);
        }

        /// <summary>
        /// Waits for the server to enter the <see cref="ServerState.Deleted"/> state.
        /// </summary>
        /// <remarks>
        /// <note type="caller">
        /// This is a blocking operation and will not return until the server enters the <see cref="ServerState.Deleted"/> state, an error state, or the retry count is exceeded.
        /// </note>
        /// </remarks>
        /// <param name="refreshCount">Number of times to poll the server's status.</param>
        /// <param name="refreshDelay">The time to wait between polling requests for the server status. If this value is <see langword="null"/>, the default is 2.4 seconds.</param>
        /// <param name="progressUpdatedCallback">A callback delegate to execute each time the <see cref="Server.Progress"/> value increases. If this value is <see langword="null"/>, progress updates are not reported.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="refreshCount"/> is less than 0.
        /// <para>-or-</para>
        /// <para>If <paramref name="refreshDelay"/> is negative.</para>
        /// </exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        public void WaitForDeleted(int refreshCount = 600, TimeSpan? refreshDelay = null, Action<int> progressUpdatedCallback = null)
        {
            if (refreshCount < 0)
                throw new ArgumentOutOfRangeException("refreshCount");
            if (refreshDelay < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException("refreshDelay");

            Provider.WaitForServerDeleted(Id, refreshCount, refreshDelay ?? TimeSpan.FromMilliseconds(2400), progressUpdatedCallback, Region, Identity);
        }

        /// <summary>
        /// Waits for the server to enter a specified state.
        /// </summary>
        /// <remarks>
        /// When the method returns, the current instance is updated to reflect the state
        /// of the server at the end of the operation.
        ///
        /// <note type="caller">
        /// This is a blocking operation and will not return until the server enters either an expected state, an error state, or the retry count is exceeded.
        /// </note>
        /// </remarks>
        /// <param name="expectedState">The expected state.</param>
        /// <param name="errorStates">The error state(s) in which to throw an exception if the server enters.</param>
        /// <param name="refreshCount">Number of times to poll the server's status.</param>
        /// <param name="refreshDelay">The time to wait between polling requests for the server status. If this value is <see langword="null"/>, the default is 2.4 seconds.</param>
        /// <param name="progressUpdatedCallback">A callback delegate to execute each time the <see cref="Server.Progress"/> value increases. If this value is <see langword="null"/>, progress updates are not reported.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="expectedState"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="errorStates"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="refreshCount"/> is less than 0.
        /// <para>-or-</para>
        /// <para>If <paramref name="refreshDelay"/> is negative.</para>
        /// </exception>
        /// <exception cref="ServerEnteredErrorStateException">If the method returned due to the server entering one of the <paramref name="errorStates"/>.</exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        public void WaitForState(ServerState expectedState, ServerState[] errorStates, int refreshCount = 600, TimeSpan? refreshDelay = null, Action<int> progressUpdatedCallback = null)
        {
            if (expectedState == null)
                throw new ArgumentNullException("expectedState");
            if (errorStates == null)
                throw new ArgumentNullException("errorStates");
            if (refreshCount < 0)
                throw new ArgumentOutOfRangeException("refreshCount");
            if (refreshDelay < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException("refreshDelay");

            var details = Provider.WaitForServerState(Id, expectedState, errorStates, refreshCount, refreshDelay ?? TimeSpan.FromMilliseconds(2400), progressUpdatedCallback, Region, Identity);
            UpdateThis(details);
        }

        /// <summary>
        /// Waits for the server to enter any one of a set of specified states.
        /// </summary>
        /// <remarks>
        /// When the method returns, the current instance is updated to reflect the state
        /// of the server at the end of the operation.
        ///
        /// <note type="caller">
        /// This is a blocking operation and will not return until the server enters either an expected state, an error state, or the retry count is exceeded.
        /// </note>
        /// </remarks>
        /// <param name="expectedStates">The expected state(s).</param>
        /// <param name="errorStates">The error state(s) in which to throw an exception if the server enters.</param>
        /// <param name="refreshCount">Number of times to poll the server's status.</param>
        /// <param name="refreshDelay">The time to wait between polling requests for the server status. If this value is <see langword="null"/>, the default is 2.4 seconds.</param>
        /// <param name="progressUpdatedCallback">A callback delegate to execute each time the <see cref="Server.Progress"/> value increases. If this value is <see langword="null"/>, progress updates are not reported.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="expectedStates"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="errorStates"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="expectedStates"/> is empty.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="refreshCount"/> is less than 0.
        /// <para>-or-</para>
        /// <para>If <paramref name="refreshDelay"/> is negative.</para>
        /// </exception>
        /// <exception cref="ServerEnteredErrorStateException">If the method returned due to the server entering one of the <paramref name="errorStates"/>.</exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        public void WaitForState(ServerState[] expectedStates, ServerState[] errorStates, int refreshCount = 600, TimeSpan? refreshDelay = null, Action<int> progressUpdatedCallback = null)
        {
            if (expectedStates == null)
                throw new ArgumentNullException("expectedStates");
            if (errorStates == null)
                throw new ArgumentNullException("errorStates");
            if (expectedStates.Length == 0)
                throw new ArgumentException("expectedStates cannot be empty");
            if (refreshCount < 0)
                throw new ArgumentOutOfRangeException("refreshCount");
            if (refreshDelay < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException("refreshDelay");

            var details = Provider.WaitForServerState(Id, expectedStates, errorStates, refreshCount, refreshDelay ?? TimeSpan.FromMilliseconds(2400), progressUpdatedCallback, Region, Identity);
            UpdateThis(details);
        }

        /// <summary>
        /// Initiates an asynchronous soft reboot operation on the specified server.
        /// </summary>
        /// <returns><see langword="true"/> if the reboot operation was successfully initiated; otherwise <see langword="false"/>.</returns>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support soft reboot operations.
        /// </exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso cref="RebootType.Soft"/>
        public bool SoftReboot()
        {
            return Provider.RebootServer(Id, RebootType.Soft, Region, Identity);
        }

        /// <summary>
        /// Initiates an asynchronous hard reboot operation on the specified server.
        /// </summary>
        /// <returns><see langword="true"/> if the reboot operation was successfully initiated; otherwise <see langword="false"/>.</returns>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support hard reboot operations.
        /// </exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso cref="RebootType.Hard"/>
        public bool HardReboot()
        {
            return Provider.RebootServer(Id, RebootType.Hard, Region, Identity);
        }

        /// <summary>
        /// Initiates an asynchronous rebuild of the server.
        /// </summary>
        /// <remarks>
        /// When the method returns, the current instance is updated to reflect the state
        /// of the server at the end of the operation.
        /// </remarks>
        /// <param name="name">The new name for the server. If the value is <see langword="null"/>, the server name is not changed.</param>
        /// <param name="imageId">The image to rebuild the server from. This is specified as an image ID (see <see cref="SimpleServerImage.Id"/>) or a full URL.</param>
        /// <param name="flavor">The new flavor for server. This is obtained from <see cref="Flavor.Id"/>.</param>
        /// <param name="adminPassword">The new admin password for the server.</param>
        /// <param name="accessIPv4">The new IP v4 address for the server, or <see cref="IPAddress.None"/> to remove the configured IP v4 address for the server. If the value is <see langword="null"/>, the server's IP v4 address is not updated.</param>
        /// <param name="accessIPv6">The new IP v6 address for the server, or <see cref="IPAddress.None"/> to remove the configured IP v6 address for the server. If the value is <see langword="null"/>, the server's IP v6 address is not updated.</param>
        /// <param name="metadata">The list of metadata to associate with the server. If the value is <see langword="null"/>, the metadata associated with the server is not changed during the rebuild operation.</param>
        /// <param name="diskConfig">The disk configuration. If the value is <see langword="null"/>, the default configuration for the specified image is used.</param>
        /// <param name="personality">The path and contents of a file to inject in the target file system during the rebuild operation. If the value is <see langword="null"/>, no file is injected.</param>
        /// <returns><see langword="true"/> if the rebuild operation was successfully initiated; otherwise <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="imageId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="flavor"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="adminPassword"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="name"/> is empty.
        /// <para>-or-</para>
        /// <para>If <paramref name="flavor"/> is empty.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="adminPassword"/> is empty.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="accessIPv4"/> is not <see cref="IPAddress.None"/> and the <see cref="AddressFamily"/> of <paramref name="accessIPv4"/> is not <see cref="AddressFamily.InterNetwork"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="accessIPv6"/> is not <see cref="IPAddress.None"/> and the <see cref="AddressFamily"/> of <paramref name="accessIPv6"/> is not <see cref="AddressFamily.InterNetworkV6"/>.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="diskConfig"/>.
        /// </exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        public bool Rebuild(string name, string imageId, string flavor, string adminPassword, IPAddress accessIPv4 = null, IPAddress accessIPv6 = null, Metadata metadata = null, DiskConfiguration diskConfig = null, Personality personality = null)
        {
            if (imageId == null)
                throw new ArgumentNullException("imageId");
            if (flavor == null)
                throw new ArgumentNullException("flavor");
            if (string.IsNullOrEmpty(imageId))
                throw new ArgumentException("imageId cannot be empty");
            if (string.IsNullOrEmpty(flavor))
                throw new ArgumentException("flavor cannot be empty");
            if (accessIPv4 != null && !IPAddress.None.Equals(accessIPv4) && accessIPv4.AddressFamily != AddressFamily.InterNetwork)
                throw new ArgumentException("The specified value for accessIPv4 is not an IP v4 address.", "accessIPv4");
            if (accessIPv6 != null && !IPAddress.None.Equals(accessIPv6) && accessIPv6.AddressFamily != AddressFamily.InterNetworkV6)
                throw new ArgumentException("The specified value for accessIPv6 is not an IP v6 address.", "accessIPv6");
            if (diskConfig != null && diskConfig != DiskConfiguration.Auto && diskConfig != DiskConfiguration.Manual)
                throw new NotSupportedException("The specified disk configuration is not supported.");

            var details = Provider.RebuildServer(Id, name, imageId, flavor, adminPassword, accessIPv4, accessIPv6, metadata, diskConfig, personality, Region, Identity);

            if (details == null)
                return false;

            UpdateThis(details);

            return true;
        }

        /// <summary>
        /// Initiates an asynchronous resize of the server. A server resize is performed by
        /// specifying a new <see cref="Flavor"/> for the server.
        /// </summary>
        /// <remarks>
        /// Following a resize operation, the original server is not immediately removed. After testing
        /// if the resulting server is operating successfully, a call should be made to <see cref="ConfirmResize"/>
        /// to keep the resized server, or to <see cref="RevertResize"/> to revert to the original server.
        /// If 24 hours pass and neither of these methods is called, the server will be automatically confirmed.
        /// </remarks>
        /// <param name="name">The new name for the resized server.</param>
        /// <param name="flavor">The new flavor. This is obtained from <see cref="Flavor.Id">Flavor.Id</see>.</param>
        /// <param name="diskConfig">The disk configuration. If the value is <see langword="null"/>, the default configuration for the specified image is used.</param>
        /// <returns><see langword="true"/> if the resize operation is successfully started; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="name"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="flavor"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="name"/> is empty.
        /// <para>-or-</para>
        /// <para>If <paramref name="flavor"/> is empty.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="diskConfig"/>.
        /// </exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        public bool Resize(string name, string flavor, DiskConfiguration diskConfig = null)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (flavor == null)
                throw new ArgumentNullException("flavor");
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name cannot be empty");
            if (string.IsNullOrEmpty(flavor))
                throw new ArgumentException("flavor cannot be empty");
            if (diskConfig != null && diskConfig != DiskConfiguration.Auto && diskConfig != DiskConfiguration.Manual)
                throw new NotSupportedException("The specified disk configuration is not supported.");

            return Provider.ResizeServer(Id, name, flavor, diskConfig, Region, Identity);
        }

        /// <summary>
        /// Confirms a completed asynchronous server resize action.
        /// </summary>
        /// <remarks>
        /// If a server resize operation is not manually confirmed or reverted within 24 hours,
        /// the operation is automatically confirmed.
        /// </remarks>
        /// <returns><see langword="true"/> if the resize operation was confirmed; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        public bool ConfirmResize()
        {
            return Provider.ConfirmServerResize(Id, Region, Identity);
        }

        /// <summary>
        /// Cancels and reverts a server resize action.
        /// </summary>
        /// <remarks>
        /// If a server resize operation is not manually confirmed or reverted within 24 hours,
        /// the operation is automatically confirmed.
        /// </remarks>
        /// <returns><see langword="true"/> if the resize operation was reverted; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        public bool RevertResize()
        {
            return Provider.RevertServerResize(Id, Region, Identity);
        }

        /// <summary>
        /// Places the server in rescue mode.
        /// </summary>
        /// <remarks>
        /// This operation is completed asynchronously. To wait for the server to enter rescue mode,
        /// call <see cref="O:net.openstack.Core.Domain.ServerBase.WaitForState"/> with the state <see cref="ServerState.Rescue"/>.
        ///
        /// <note>
        /// The provider may limit the duration of rescue mode, after which the rescue image is destroyed
        /// and the server attempts to reboot. Rescue mode may be explicitly exited at any time by
        /// calling <see cref="UnRescue"/>.
        /// </note>
        /// </remarks>
        /// <returns>The root password assigned for use during rescue mode.</returns>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        public string Rescue()
        {
            return Provider.RescueServer(Id, Region, Identity);
        }

        /// <summary>
        /// Takes the server out of rescue mode.
        /// </summary>
        /// <remarks>
        /// This operation is completed asynchronously. To wait for the server to exit rescue mode,
        /// call <see cref="WaitForActive"/>.
        ///
        /// <note>
        /// The provider may limit the duration of rescue mode, after which the rescue image is destroyed
        /// and the server attempts to reboot. Rescue mode may be explicitly exited at any time by
        /// calling <see cref="UnRescue"/>.
        /// </note>
        /// </remarks>
        /// <returns><see langword="true"/> if the server exited rescue mode; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        public bool UnRescue()
        {
            return Provider.UnRescueServer(Id, Region, Identity);
        }

        /// <summary>
        /// Creates a new snapshot image for a specified server at its current state.
        /// </summary>
        /// <remarks>
        /// The server snapshot process is completed asynchronously. To wait for the image
        /// to be completed, you may call <see cref="IComputeProvider.WaitForImageActive"/>.
        /// </remarks>
        /// <param name="imageName">Name of the new image.</param>
        /// <param name="metadata">The metadata to associate to the new image.</param>
        /// <returns><see langword="true"/> if the image creation process was successfully started; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="imageName"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="imageName"/> is empty.</exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        public bool CreateSnapshot(string imageName, Metadata metadata = null)
        {
            return Provider.CreateImage(Id, imageName, metadata, Region, Identity);
        }

        /// <summary>
        /// Gets the detailed information for the server.
        /// </summary>
        /// <returns>A <see cref="Server"/> object containing the details for the server.</returns>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        public Server GetDetails()
        {
            return Provider.GetDetails(Id, Region, Identity);
        }

        /// <summary>
        /// Lists the volume attachments for the server.
        /// </summary>
        /// <returns>A collection of <see cref="ServerVolume"/> objects describing the volumes attached to the server.</returns>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        public IEnumerable<ServerVolume> ListVolumes()
        {
            return Provider.ListServerVolumes(Id, Region, Identity);
        }

        /// <summary>
        /// Attaches a volume to the server.
        /// </summary>
        /// <param name="volumeId">The volume ID. This is obtained from <see cref="Volume.Id"/>.</param>
        /// <param name="storageDevice">The name of the device, such as <localUri>/dev/xvdb</localUri>. If the value is <see langword="null"/>, an automatically generated device name will be used.</param>
        /// <returns>A <see cref="ServerVolume"/> object containing the details about the volume.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="volumeId"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="volumeId"/> is empty.</exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        public ServerVolume AttachVolume(string volumeId, string storageDevice)
        {
            return Provider.AttachServerVolume(Id, volumeId, storageDevice, Region, Identity);
        }

        /// <summary>
        /// Detaches the specified volume from the server.
        /// </summary>
        /// <param name="volumeId">The volume attachment ID. This is obtained from <see cref="ServerVolume.Id">ServerVolume.Id</see>.</param>
        /// <returns><see langword="true"/> if the volume was successfully detached; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="volumeId"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="volumeId"/> is empty.</exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        public bool DetachVolume(string volumeId)
        {
            return Provider.DetachServerVolume(Id, volumeId, Region, Identity);
        }

        /// <summary>
        /// Updates the current instance to match the values in the <see cref="Server"/>
        /// instance returned from a call to <see cref="GetDetails()"/>.
        /// </summary>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        public void Refresh()
        {
            var details = this.GetDetails();

            UpdateThis(details);
        }
    }
}
