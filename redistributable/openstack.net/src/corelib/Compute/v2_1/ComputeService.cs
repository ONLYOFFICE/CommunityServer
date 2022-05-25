using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OpenStack.Authentication;
using OpenStack.Compute.v2_1.Serialization;
using OpenStack.Images.v2;
using OpenStack.Serialization;

namespace OpenStack.Compute.v2_1
{
    /// <summary>
    /// The OpenStack Compute Service.
    /// </summary>
    /// <seealso href="http://developer.openstack.org/api-ref-compute-v2.1.html">OpenStack Compute API v2.1 Overview</seealso>
    public class ComputeService
    {
        /// <summary />
        internal readonly ComputeApi _computeApi;

        /// <summary>
        /// Initializes a new instance of the <see cref="ComputeService"/> class.
        /// </summary>
        /// <param name="authenticationProvider">The authentication provider.</param>
        /// <param name="region">The region.</param>
        /// <param name="useInternalUrl">if set to <c>true</c> uses the internal URLs specified in the ServiceCatalog, otherwise the public URLs are used.</param>
        public ComputeService(IAuthenticationProvider authenticationProvider, string region, bool useInternalUrl = false)
        {
            _computeApi = new ComputeApi(ServiceType.Compute, authenticationProvider, region, useInternalUrl);
        }

        #region Servers

        /// <inheritdoc cref="ComputeApi.GetServerAsync{T}" />
        public Task<Server> GetServerAsync(Identifier serverId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _computeApi.GetServerAsync<Server>(serverId, cancellationToken);
        }

        /// <inheritdoc cref="ComputeApi.GetServerMetadataAsync{T}" />
        public Task<ServerMetadata> GetServerMetadataAsync(Identifier serverId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _computeApi.GetServerMetadataAsync<ServerMetadata>(serverId, cancellationToken);
        }

        /// <inheritdoc cref="ComputeApi.GetServerMetadataItemAsync" />
        public Task<string> GetServerMetadataItemAsync(Identifier serverId, string key, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _computeApi.GetServerMetadataItemAsync(serverId, key, cancellationToken);
        }

        /// <inheritdoc cref="ComputeApi.CreateServerAsync{T}" />
        public Task<Server> CreateServerAsync(ServerCreateDefinition server, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _computeApi.CreateServerAsync<Server>(server, cancellationToken);
        }

        /// <inheritdoc cref="ComputeApi.CreateServerMetadataAsync" />
        public Task CreateServerMetadataAsync(Identifier serverId, string key, string value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _computeApi.CreateServerMetadataAsync(serverId, key, value, cancellationToken);
        }

        /// <inheritdoc cref="ComputeApi.WaitForServerStatusAsync{TServer,TStatus}(string,TStatus,TimeSpan?,TimeSpan?,IProgress{bool},CancellationToken)" />
        public Task<Server> WaitForServerStatusAsync(Identifier serverId, ServerStatus status, TimeSpan? refreshDelay = null, TimeSpan? timeout = null, IProgress<bool> progress = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _computeApi.WaitForServerStatusAsync<Server, ServerStatus>(serverId, status, refreshDelay, timeout, progress, cancellationToken);
        }

        /// <inheritdoc cref="ComputeApi.WaitForServerStatusAsync{TServer,TStatus}(string, IEnumerable{TStatus},TimeSpan?,TimeSpan?,IProgress{bool},CancellationToken)" />
        public Task<Server> WaitForServerStatusAsync(Identifier serverId, IEnumerable<ServerStatus> statuses, TimeSpan? refreshDelay = null, TimeSpan? timeout = null, IProgress<bool> progress = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _computeApi.WaitForServerStatusAsync<Server, ServerStatus>(serverId, statuses, refreshDelay, timeout, progress, cancellationToken);
        }

        /// <inheritdoc cref="ComputeApi.ListServerSummariesAsync{TPage}" />
        public async Task<IPage<ServerSummary>> ListServerSummariesAsync(ServerListOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await _computeApi.ListServerSummariesAsync<ServerSummaryCollection>(options, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc cref="ComputeApi.ListServersAsync{TPage}(IQueryStringBuilder,CancellationToken)" />
        public async Task<IPage<Server>> ListServersAsync(ServerListOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await _computeApi.ListServersAsync<ServerCollection>(options, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc cref="ComputeApi.UpdateServerAsync{T}" />
        public Task<Server> UpdateServerAsync(Identifier serverid, ServerUpdateDefinition server, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _computeApi.UpdateServerAsync<Server>(serverid, server, cancellationToken);
        }

        /// <inheritdoc cref="ComputeApi.UpdateServerMetadataAsync{T}" />
        public Task<ServerMetadata> UpdateServerMetadataAsync(Identifier serverId, ServerMetadata metadata, bool overwrite = false, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _computeApi.UpdateServerMetadataAsync<ServerMetadata>(serverId, metadata, overwrite, cancellationToken);
        }
        
        /// <inheritdoc cref="ComputeApi.DeleteServerAsync" />
        public Task DeleteServerAsync(Identifier serverId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _computeApi.DeleteServerAsync(serverId, cancellationToken);
        }

        /// <inheritdoc cref="ComputeApi.DeleteServerMetadataAsync" />
        public Task DeleteServerMetadataAsync(Identifier serverId, string key, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _computeApi.DeleteServerMetadataAsync(serverId, key, cancellationToken);
        }

        /// <inheritdoc cref="ComputeApi.WaitUntilServerIsDeletedAsync{TServer,TStatus}" />
        public Task WaitUntilServerIsDeletedAsync(Identifier serverId, TimeSpan? refreshDelay = null, TimeSpan? timeout = null, IProgress<bool> progress = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _computeApi.WaitUntilServerIsDeletedAsync<Server, ServerStatus>(serverId, null, refreshDelay, timeout, progress, cancellationToken);
        }

        /// <inheritdoc cref="ComputeApi.SnapshotServerAsync{T}" />
        public Task<Image> SnapshotServerAsync(Identifier serverId, SnapshotServerRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _computeApi.SnapshotServerAsync<Image>(serverId, request, cancellationToken);
        }

        /// <inheritdoc cref="ComputeApi.StartServerAsync" />
        public Task StartServerAsync(Identifier serverId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _computeApi.StartServerAsync(serverId, cancellationToken);
        }

        /// <inheritdoc cref="ComputeApi.StopServerAsync" />
        public Task StopServerAsync(Identifier serverId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _computeApi.StopServerAsync(serverId, cancellationToken);
        }

        /// <inheritdoc cref="ComputeApi.SuspendServerAsync" />
        public Task SuspendServerAsync(Identifier serverId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _computeApi.SuspendServerAsync(serverId, cancellationToken);
        }

        /// <inheritdoc cref="ComputeApi.ResumeServerAsync" />
        public Task ResumeServerAsync(Identifier serverId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _computeApi.ResumeServerAsync(serverId, cancellationToken);
        }

        /// <inheritdoc cref="ComputeApi.RebootServerAsync{TRequest}" />
        public Task RebootServerAsync(Identifier serverId, RebootServerRequest request = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _computeApi.RebootServerAsync(serverId, request, cancellationToken);
        }

        /// <inheritdoc cref="ComputeApi.GetVncConsoleAsync{T}" />
        public virtual Task<RemoteConsole> GetVncConsoleAync(Identifier serverId, RemoteConsoleType type, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _computeApi.GetVncConsoleAsync<RemoteConsole>(serverId, type, cancellationToken);
        }

        /// <inheritdoc cref="ComputeApi.GetSpiceConsoleAsync{T}" />
        public virtual Task<RemoteConsole> GetSpiceConsoleAync(Identifier serverId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _computeApi.GetSpiceConsoleAsync<RemoteConsole>(serverId, RemoteConsoleType.SpiceHtml5, cancellationToken);
        }

        /// <inheritdoc cref="ComputeApi.GetSerialConsoleAsync{T}" />
        public virtual Task<RemoteConsole> GetSerialConsoleAync(Identifier serverId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _computeApi.GetSerialConsoleAsync<RemoteConsole>(serverId, RemoteConsoleType.Serial, cancellationToken);
        }

        /// <inheritdoc cref="ComputeApi.GetRdpConsoleAsync{T}" />
        public virtual Task<RemoteConsole> GetRdpConsoleAsync(Identifier serverId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _computeApi.GetRdpConsoleAsync<RemoteConsole>(serverId, RemoteConsoleType.RdpHtml5, cancellationToken);
        }

        /// <inheritdoc cref="ComputeApi.GetConsoleOutputAsync" />
        public virtual Task<string> GetConsoleOutputAsync(Identifier serverId, int length = -1, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _computeApi.GetConsoleOutputAsync(serverId, length, cancellationToken);
        }

        /// <inheritdoc cref="ComputeApi.RescueServerAsync" />
        public virtual Task<string> RescueServerAsync(Identifier serverId, RescueServerRequest request = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _computeApi.RescueServerAsync(serverId, request, cancellationToken);
        }

        /// <inheritdoc cref="ComputeApi.UnrescueServerAsync" />
        public virtual Task UnrescueServerAsync(Identifier serverId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _computeApi.UnrescueServerAsync(serverId, cancellationToken);
        }

        /// <inheritdoc cref="ComputeApi.ResizeServerAsync" />
        public virtual Task ResizeServerAsync(Identifier serverId, Identifier flavorId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _computeApi.ResizeServerAsync(serverId, flavorId, cancellationToken);
        }

        /// <inheritdoc cref="ComputeApi.ConfirmResizeServerAsync" />
        public virtual Task ConfirmResizeServerAsync(Identifier serverId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _computeApi.ConfirmResizeServerAsync(serverId, cancellationToken);
        }

        /// <inheritdoc cref="ComputeApi.CancelResizeServerAsync" />
        public virtual Task CancelResizeServerAsync(Identifier serverId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _computeApi.CancelResizeServerAsync(serverId, cancellationToken);
        }

        /// <inheritdoc cref="ComputeApi.AssociateFloatingIPAsync" />
        public virtual Task AssociateFloatingIPAddressAsync(Identifier serverId, AssociateFloatingIPRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _computeApi.AssociateFloatingIPAsync(serverId, request, cancellationToken);
        }

        /// <inheritdoc cref="ComputeApi.DisassociateFloatingIPAsync" />
        public virtual Task DisassociateFloatingIPAsync(Identifier serverId, string floatingIPAddress, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _computeApi.DisassociateFloatingIPAsync(serverId, floatingIPAddress, cancellationToken);
        }

        /// <inheritdoc cref="ComputeApi.ListServerActionSummariesAsync{T}" />
        public virtual async Task<IEnumerable<ServerActionSummary>> ListServerActionSummariesAsync(Identifier serverId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await _computeApi.ListServerActionSummariesAsync<ServerActionSummaryCollection>(serverId, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc cref="ComputeApi.ListServerActionSummariesAsync{T}" />
        public virtual Task<ServerAction> GetServerActionAsync(Identifier serverId, Identifier actionId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _computeApi.GetServerActionAsync<ServerAction>(serverId, actionId, cancellationToken);
        }
        #endregion

        #region Flavors

        /// <inheritdoc cref="ComputeApi.GetFlavorAsync{T}" />
        public Task<Flavor> GetFlavorAsync(Identifier flavorId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _computeApi.GetFlavorAsync<Flavor>(flavorId, cancellationToken);
        }

        /// <inheritdoc cref="ComputeApi.ListFlavorSummariesAsync{T}" />
        public async Task<IPage<FlavorSummary>> ListFlavorSummariesAsync(FlavorListOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await _computeApi.ListFlavorSummariesAsync<FlavorSummaryCollection>(options, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc cref="ComputeApi.ListFlavorsAsync{T}" />
        public async Task<IPage<Flavor>> ListFlavorsAsync(FlavorListOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await _computeApi.ListFlavorsAsync<FlavorCollection>(options, cancellationToken).ConfigureAwait(false);
        }

        #endregion

        #region Images

        /// <inheritdoc cref="ComputeApi.GetImageAsync{T}" />
        public Task<Image> GetImageAsync(Identifier imageId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _computeApi.GetImageAsync<Image>(imageId, cancellationToken);
        }

        /// <inheritdoc cref="ComputeApi.GetImageMetadataAsync{T}" />
        public Task<ImageMetadata> GetImageMetadataAsync(Identifier imageId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _computeApi.GetImageMetadataAsync<ImageMetadata>(imageId, cancellationToken);
        }

        /// <inheritdoc cref="ComputeApi.GetImageMetadataItemAsync" />
        public Task<string> GetImageMetadataItemAsync(Identifier imageId, string key, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _computeApi.GetImageMetadataItemAsync(imageId, key, cancellationToken);
        }

        /// <inheritdoc cref="ComputeApi.WaitForImageStatusAsync{TImage,TStatus}" />
        public Task<Image> WaitForImageStatusAsync(Identifier imageId, ImageStatus status, TimeSpan? refreshDelay = null, TimeSpan? timeout = null, IProgress<bool> progress = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _computeApi.WaitForImageStatusAsync<Image, ImageStatus>(imageId, status, refreshDelay, timeout, progress, cancellationToken);
        }

        /// <inheritdoc cref="ComputeApi.CreateImageMetadataAsync" />
        public Task CreateImageMetadataAsync(Identifier imageId, string key, string value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _computeApi.CreateImageMetadataAsync(imageId, key, value, cancellationToken);
        }

        /// <inheritdoc cref="ComputeApi.ListImageSummariesAsync{TPage}" />
        public async Task<IPage<ImageSummary>> ListImageSummariesAsync(ImageListOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await _computeApi.ListImageSummariesAsync<ImageSummaryCollection>(options, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc cref="ComputeApi.ListImagesAsync{TPage}" />
        public async Task<IPage<Image>> ListImagesAsync(ImageListOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await _computeApi.ListImagesAsync<ImageCollection>(options, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc cref="ComputeApi.UpdateImageMetadataAsync{T}" />
        public Task<ImageMetadata> UpdateImageMetadataAsync(Identifier imageId, ImageMetadata metadata, bool overwrite = false, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _computeApi.UpdateImageMetadataAsync<ImageMetadata>(imageId, metadata, overwrite, cancellationToken);
        }
        
        /// <inheritdoc cref="ComputeApi.DeleteImageAsync" />
        public Task DeleteImageAsync(Identifier imageId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _computeApi.DeleteImageAsync(imageId, cancellationToken);
        }

        /// <inheritdoc cref="ComputeApi.DeleteImageMetadataAsync" />
        public Task DeleteImageMetadataAsync(Identifier imageId, string key, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _computeApi.DeleteImageMetadataAsync(imageId, key, cancellationToken);
        }

        /// <inheritdoc cref="ComputeApi.WaitUntilImageIsDeletedAsync{TImage,TStatus}" />
        public Task WaitUntilImageIsDeletedAsync(Identifier imageId, TimeSpan? refreshDelay = null, TimeSpan? timeout = null, IProgress<bool> progress = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _computeApi.WaitUntilImageIsDeletedAsync<Image, ImageStatus>(imageId, null, refreshDelay, timeout, progress, cancellationToken);
        }
        #endregion

        #region IP Addresses

        /// <inheritdoc cref="ComputeApi.GetServerAddressAsync{T}" />
        public Task<IList<ServerAddress>> GetServerAddressAsync(Identifier serverId, string key, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _computeApi.GetServerAddressAsync<ServerAddress>(serverId, key, cancellationToken);
        }

        /// <inheritdoc cref="ComputeApi.ListServerAddressesAsync{T}" />
        public async Task<IDictionary<string, IList<ServerAddress>>> ListServerAddressesAsync(Identifier serverId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await _computeApi.ListServerAddressesAsync<ServerAddressCollection>(serverId, cancellationToken).ConfigureAwait(false);
        }

        #endregion

        #region Server Volumes
        /// <inheritdoc cref="ComputeApi.GetServerVolumeAsync{T}" />
        public Task<ServerVolume> GetServerVolumeAsync(Identifier serverId, Identifier volumeId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _computeApi.GetServerVolumeAsync<ServerVolume>(serverId, volumeId, cancellationToken);
        }

        /// <inheritdoc cref="ComputeApi.ListServerVolumesAsync{T}" />
        public async Task<IEnumerable<ServerVolume>> ListServerVolumesAsync(Identifier serverId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await _computeApi.ListServerVolumesAsync<ServerVolumeCollection>(serverId, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc cref="ComputeApi.AttachVolumeAsync{T}" />
        public Task<ServerVolume> AttachVolumeAsync(Identifier serverId, ServerVolumeDefinition volume, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _computeApi.AttachVolumeAsync<ServerVolume>(serverId, volume, cancellationToken);
        }

        /// <inheritdoc cref="ComputeApi.DetachVolumeAsync" />
        public Task DetachVolumeAsync(Identifier serverId, Identifier volumeId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _computeApi.DetachVolumeAsync(serverId, volumeId, cancellationToken);
        }
        #endregion

        #region Keypairs

        /// <inheritdoc cref="ComputeApi.GetKeyPairAsync{T}" />
        public virtual Task<KeyPair> GetKeyPairAsync(string keypairName, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _computeApi.GetKeyPairAsync<KeyPair>(keypairName, cancellationToken);
        }

        /// <summary>
        /// Creates a new key pair.
        /// </summary>
        /// <param name="request">The key pair request.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The response includes the generated private key.</returns>
        public virtual Task<KeyPairResponse> CreateKeyPairAsync(KeyPairRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _computeApi.CreateKeyPairAsync<KeyPairResponse>(request, cancellationToken);
        }

        /// <summary>
        /// Imports a key pair.
        /// </summary>
        /// <param name="keypair">The keypair.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public virtual Task<KeyPairSummary> ImportKeyPairAsync(KeyPairDefinition keypair, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _computeApi.CreateKeyPairAsync<KeyPairSummary>(keypair, cancellationToken);
        }

        /// <inheritdoc cref="ComputeApi.ListKeyPairsAsync{T}" />
        public virtual async Task<IEnumerable<KeyPairSummary>> ListKeyPairsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return await _computeApi.ListKeyPairsAsync<KeyPairSummaryCollection>(cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc cref="ComputeApi.DeleteKeyPairAsync"/>
        public virtual Task DeleteKeyPairAsync(string keypairName, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _computeApi.DeleteKeyPairAsync(keypairName, cancellationToken);
        }
        #endregion

        #region Security Groups

        /// <inheritdoc cref="ComputeApi.GetSecurityGroupAsync{T}" />
        public Task<SecurityGroup> GetSecurityGroupAsync(Identifier securityGroupId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _computeApi.GetSecurityGroupAsync<SecurityGroup>(securityGroupId, cancellationToken);
        }

        /// <inheritdoc cref="ComputeApi.CreateSecurityGroupAsync{T}" />
        public Task<SecurityGroup> CreateSecurityGroupAsync(SecurityGroupDefinition securityGroup, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _computeApi.CreateSecurityGroupAsync<SecurityGroup>(securityGroup, cancellationToken);
        }

        /// <inheritdoc cref="ComputeApi.ListSecurityGroupsAsync{T}" />
        public async Task<IEnumerable<SecurityGroup>> ListSecurityGroupsAsync(Identifier serverId = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await _computeApi.ListSecurityGroupsAsync<SecurityGroupCollection>(serverId, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc cref="ComputeApi.UpdateSecurityGroupAsync{T}" />
        public Task<SecurityGroup> UpdateSecurityGroupAsync(Identifier securityGroupId, SecurityGroupDefinition securityGroup, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _computeApi.UpdateSecurityGroupAsync<SecurityGroup>(securityGroupId, securityGroup, cancellationToken);
        }

        /// <inheritdoc cref="ComputeApi.DeleteSecurityGroupAsync" />
        public Task DeleteSecurityGroupAsync(Identifier securityGroupId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _computeApi.DeleteSecurityGroupAsync(securityGroupId, cancellationToken);
        }

        #endregion

        #region Sever Groups

        /// <inheritdoc cref="ComputeApi.GetServerGroupAsync{T}" />
        public Task<ServerGroup> GetServerGroupAsync(Identifier serverGroupId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _computeApi.GetServerGroupAsync<ServerGroup>(serverGroupId, cancellationToken);
        }

        /// <inheritdoc cref="ComputeApi.CreateServerGroupAsync{T}" />
        public Task<ServerGroup> CreateServerGroupAsync(ServerGroupDefinition serverGroup, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _computeApi.CreateServerGroupAsync<ServerGroup>(serverGroup, cancellationToken);
        }

        /// <inheritdoc cref="ComputeApi.ListServerGroupsAsync{T}" />
        public async Task<IEnumerable<ServerGroup>> ListServerGroupsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return await _computeApi.ListServerGroupsAsync<ServerGroupCollection>(cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc cref="ComputeApi.DeleteServerGroupAsync" />
        public Task DeleteServerGroupAsync(Identifier serverGroupId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _computeApi.DeleteServerGroupAsync(serverGroupId, cancellationToken);
        }

        #endregion

        #region Volumes

        /// <inheritdoc cref="ComputeApi.GetVolumeAsync{T}" />
        public Task<Volume> GetVolumeAsync(Identifier volumeId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _computeApi.GetVolumeAsync<Volume>(volumeId, cancellationToken);
        }

        /// <inheritdoc cref="ComputeApi.GetVolumeSnapshotAsync{T}" />
        public Task<VolumeSnapshot> GetVolumeSnapshotAsync(Identifier snapshotId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _computeApi.GetVolumeSnapshotAsync<VolumeSnapshot>(snapshotId, cancellationToken);
        }

        /// <inheritdoc cref="ComputeApi.CreateVolumeAsync{T}" />
        public Task<Volume> CreateVolumeAsync(VolumeDefinition volume, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _computeApi.CreateVolumeAsync<Volume>(volume, cancellationToken);
        }

        /// <inheritdoc cref="ComputeApi.SnapshotVolumeAsync{T}" />
        public Task<VolumeSnapshot> SnapshotVolumeAsync(VolumeSnapshotDefinition snapshot, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _computeApi.SnapshotVolumeAsync<VolumeSnapshot>(snapshot, cancellationToken);
        }

        /// <inheritdoc cref="ComputeApi.ListVolumesAsync{T}" />
        public async Task<IEnumerable<Volume>> ListVolumesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return await _computeApi.ListVolumesAsync<VolumeCollection>(cancellationToken).ConfigureAwait(false);
        }
        
        /// <inheritdoc cref="ComputeApi.ListVolumeSnapshotsAsync{T}" />
        public async Task<IEnumerable<VolumeSnapshot>> ListVolumeSnapshotsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return await _computeApi.ListVolumeSnapshotsAsync<VolumeSnapshotCollection>(cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc cref="ComputeApi.DeleteVolumeAsync" />
        public Task DeleteVolumeAsync(Identifier volumeId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _computeApi.DeleteVolumeAsync(volumeId, cancellationToken);
        }

        /// <inheritdoc cref="ComputeApi.DeleteVolumeSnapshotAsync" />
        public Task DeleteVolumeSnapshotAsync(Identifier snapshotId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _computeApi.DeleteVolumeSnapshotAsync(snapshotId, cancellationToken);
        }

        #endregion

        #region Compute Service
        /// <inheritdoc cref="ComputeApi.GetLimitsAsync{T}" />
        public Task<ServiceLimits> GetLimitsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return _computeApi.GetLimitsAsync<ServiceLimits>(cancellationToken);
        }
        #endregion
    }
}
