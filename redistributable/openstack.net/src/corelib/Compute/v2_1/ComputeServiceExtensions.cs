using System;
using System.Collections.Generic;
using OpenStack.Compute.v2_1;
using OpenStack.Images.v2;
using OpenStack.Synchronous.Extensions;

// ReSharper disable once CheckNamespace
namespace OpenStack.Synchronous
{
    /// <summary>
    /// Provides synchronous extention methods for a <see cref="ComputeService"/> instance.
    /// </summary>
    public static class ComputeServiceExtensions_v2_1
    {
        #region Servers
        /// <inheritdoc cref="ComputeService.GetServerAsync" />
        public static Server GetServer(this ComputeService service, Identifier serverId)
        {
            return service.GetServerAsync(serverId).ForceSynchronous();
        }

        /// <inheritdoc cref="ComputeService.GetServerAsync" />
        public static ServerMetadata GetServerMetadata(this ComputeService service, Identifier serverId)
        {
            return service.GetServerMetadataAsync(serverId).ForceSynchronous();
        }

        /// <inheritdoc cref="ComputeService.GetServerMetadataItemAsync" />
        public static string GetServerMetadataItem(this ComputeService service, Identifier serverId, string key)
        {
            return service.GetServerMetadataItemAsync(serverId, key).ForceSynchronous();
        }

        /// <inheritdoc cref="ComputeService.CreateServerAsync" />
        public static Server CreateServer(this ComputeService service, ServerCreateDefinition server)
        {
            return service.CreateServerAsync(server).ForceSynchronous();
        }

        /// <inheritdoc cref="ComputeService.CreateServerMetadataAsync" />
        public static void CreateServerMetadata(this ComputeService service, Identifier serverId, string key, string value)
        {
            service.CreateServerMetadataAsync(serverId, key, value).ForceSynchronous();
        }

        /// <inheritdoc cref="ComputeService.WaitForServerStatusAsync(Identifier,ServerStatus,TimeSpan?,TimeSpan?,IProgress{bool},System.Threading.CancellationToken)" />
        public static Server WaitForServerStatus(this ComputeService service, Identifier serverId, ServerStatus status, TimeSpan? refreshDelay = null, TimeSpan? timeout = null, IProgress<bool> progress = null)
        {
            return service.WaitForServerStatusAsync(serverId, status, refreshDelay, timeout, progress).ForceSynchronous();
        }

        /// <inheritdoc cref="ComputeService.WaitForServerStatusAsync(Identifier,IEnumerable{ServerStatus},TimeSpan?,TimeSpan?,IProgress{bool},System.Threading.CancellationToken)" />
        public static Server WaitForServerStatus(this ComputeService service, Identifier serverId, IEnumerable<ServerStatus> statuses, TimeSpan? refreshDelay = null, TimeSpan? timeout = null, IProgress<bool> progress = null)
        {
            return service.WaitForServerStatusAsync(serverId, statuses, refreshDelay, timeout, progress).ForceSynchronous();
        }

        /// <inheritdoc cref="ComputeService.ListServerSummariesAsync" />
        public static IPage<ServerSummary> ListServerSummaries(this ComputeService service, ServerListOptions options = null)
        {
            return service.ListServerSummariesAsync(options).ForceSynchronous();
        }

        /// <inheritdoc cref="ComputeService.ListServersAsync" />
        public static IPage<Server> ListServers(this ComputeService service, ServerListOptions options = null)
        {
            return service.ListServersAsync(options).ForceSynchronous();
        }

        /// <inheritdoc cref="ComputeService.UpdateServerAsync" />
        public static Server UpdateServer(this ComputeService service, Identifier serverid, ServerUpdateDefinition server)
        {
            return service.UpdateServerAsync(serverid, server).ForceSynchronous();
        }

        /// <inheritdoc cref="ComputeService.UpdateServerMetadataAsync" />
        public static ServerMetadata UpdateServerMetadata(this ComputeService service, Identifier serverId, ServerMetadata metadata, bool overwrite = false)
        {
            return service.UpdateServerMetadataAsync(serverId, metadata, overwrite).ForceSynchronous();
        }

        /// <inheritdoc cref="ComputeService.DeleteServerAsync" />
        public static void DeleteServer(this ComputeService service, Identifier serverId)
        {
            service.DeleteServerAsync(serverId).ForceSynchronous();
        }

        /// <inheritdoc cref="ComputeService.DeleteServerMetadataAsync" />
        public static void DeleteServerMetadata(this ComputeService service, Identifier serverId, string key)
        {
            service.DeleteServerMetadataAsync(serverId, key).ForceSynchronous();
        }

        /// <inheritdoc cref="ComputeService.WaitUntilServerIsDeletedAsync" />
        public static void WaitUntilServerIsDeleted(this ComputeService service, Identifier serverId, TimeSpan? refreshDelay = null, TimeSpan? timeout = null, IProgress<bool> progress = null)
        {
            service.WaitUntilServerIsDeletedAsync(serverId, refreshDelay, timeout, progress).ForceSynchronous();
        }

        /// <inheritdoc cref="ComputeService.SnapshotServerAsync" />
        public static Image SnapshotServer(this ComputeService service, Identifier serverId, SnapshotServerRequest request)
        {
            return service.SnapshotServerAsync(serverId, request).ForceSynchronous();
        }

        /// <inheritdoc cref="ComputeService.StartServerAsync" />
        public static void StartServer(this ComputeService service, Identifier serverId)
        {
            service.StartServerAsync(serverId).ForceSynchronous();
        }

        /// <inheritdoc cref="ComputeService.StopServerAsync" />
        public static void StopServer(this ComputeService service, Identifier serverId)
        {
            service.StopServerAsync(serverId).ForceSynchronous();
        }

        /// <inheritdoc cref="ComputeService.RebootServerAsync" />
        public static void RebootServer(this ComputeService service, Identifier serverId, RebootServerRequest request = null)
        {
            service.RebootServerAsync(serverId, request).ForceSynchronous();
        }

        /// <inheritdoc cref="ComputeService.GetVncConsoleAync" />
        public static RemoteConsole GetVncConsole(this ComputeService service, Identifier serverId, RemoteConsoleType type)
        {
            return service.GetVncConsoleAync(serverId, type).ForceSynchronous();
        }

        /// <inheritdoc cref="ComputeService.GetSpiceConsoleAync" />
        public static RemoteConsole GetSpiceConsole(this ComputeService service, Identifier serverId)
        {
            return service.GetSpiceConsoleAync(serverId).ForceSynchronous();
        }

        /// <inheritdoc cref="ComputeService.GetSpiceConsoleAync" />
        public static RemoteConsole GetSerialConsole(this ComputeService service, Identifier serverId)
        {
            return service.GetSerialConsoleAync(serverId).ForceSynchronous();
        }

        /// <inheritdoc cref="ComputeService.GetRdpConsoleAsync" />
        public static RemoteConsole GetRdpConsole(this ComputeService service, Identifier serverId)
        {
            return service.GetRdpConsoleAsync(serverId).ForceSynchronous();
        }

        /// <inheritdoc cref="ComputeService.GetConsoleOutputAsync" />
        public static string GetConsoleOutput(this ComputeService service, Identifier serverId, int length = -1)
        {
            return service.GetConsoleOutputAsync(serverId, length).ForceSynchronous();
        }

        /// <inheritdoc cref="ComputeService.RescueServerAsync" />
        public static string RescueServer(this ComputeService service, Identifier serverId, RescueServerRequest request = null)
        {
            return service.RescueServerAsync(serverId, request).ForceSynchronous();
        }

        /// <inheritdoc cref="ComputeService.UnrescueServerAsync" />
        public static void UnrescueServer(this ComputeService service, Identifier serverId)
        {
            service.UnrescueServerAsync(serverId).ForceSynchronous();
        }

        /// <inheritdoc cref="ComputeService.ResizeServerAsync" />
        public static void ResizeServer(this ComputeService service, Identifier serverId, Identifier flavorId)
        {
            service.ResizeServerAsync(serverId, flavorId).ForceSynchronous();
        }

        /// <inheritdoc cref="ComputeService.ConfirmResizeServerAsync" />
        public static void ConfirmResizeServer(this ComputeService service, Identifier serverId)
        {
            service.ConfirmResizeServerAsync(serverId).ForceSynchronous();
        }

        /// <inheritdoc cref="ComputeService.CancelResizeServerAsync" />
        public static void CancelResizeServer(this ComputeService service, Identifier serverId)
        {
            service.CancelResizeServerAsync(serverId).ForceSynchronous();
        }

        /// <inheritdoc cref="ComputeService.AssociateFloatingIPAddressAsync" />
        public static void AssociateFloatingIPAddressAsync(this ComputeService service, Identifier serverId, AssociateFloatingIPRequest request)
        {
            service.AssociateFloatingIPAddressAsync(serverId, request).ForceSynchronous();
        }

        /// <inheritdoc cref="ComputeService.DisassociateFloatingIPAsync" />
        public static void DisassociateFloatingIPAsync(this ComputeService service, Identifier serverId, string floatingIPAddress)
        {
            service.DisassociateFloatingIPAsync(serverId, floatingIPAddress).ForceSynchronous();
        }

        /// <inheritdoc cref="ComputeService.ListServerActionSummariesAsync" />
        public static IEnumerable<ServerActionSummary> ListServerActions(this ComputeService service, Identifier serverId)
        {
            return service.ListServerActionSummariesAsync(serverId).ForceSynchronous();
        }

        /// <inheritdoc cref="ComputeService.GetServerActionAsync" />
        public static ServerAction GetServerAction(this ComputeService service, Identifier serverId, Identifier actionId)
        {
            return service.GetServerActionAsync(serverId, actionId).ForceSynchronous();
        }
        #endregion

        #region Flavors
        /// <inheritdoc cref="ComputeService.GetFlavorAsync" />
        public static Flavor GetFlavor(this ComputeService service, Identifier flavorId)
        {
            return service.GetFlavorAsync(flavorId).ForceSynchronous();
        }

        /// <inheritdoc cref="ComputeService.ListFlavorSummariesAsync" />
        public static IEnumerable<FlavorSummary> ListFlavorSummaries(this ComputeService service)
        {
            return service.ListFlavorSummariesAsync().ForceSynchronous();
        }

        /// <inheritdoc cref="ComputeService.ListFlavorsAsync" />
        public static IEnumerable<Flavor> ListFlavors(this ComputeService service)
        {
            return service.ListFlavorsAsync().ForceSynchronous();
        }
        #endregion

        #region Images
        /// <inheritdoc cref="ComputeService.GetImageAsync" />
        public static Image GetImage(this ComputeService service, Identifier imageId)
        {
            return service.GetImageAsync(imageId).ForceSynchronous();
        }

        /// <inheritdoc cref="ComputeService.GetImageAsync" />
        public static ImageMetadata GetImageMetadata(this ComputeService service, Identifier imageId)
        {
            return service.GetImageMetadataAsync(imageId).ForceSynchronous();
        }

        /// <inheritdoc cref="ComputeService.GetImageMetadataItemAsync" />
        public static string GetImageMetadataItem(this ComputeService service, Identifier imageId, string key)
        {
            return service.GetImageMetadataItemAsync(imageId, key).ForceSynchronous();
        }

        /// <inheritdoc cref="ComputeService.WaitForImageStatusAsync" />
        public static void WaitForImageStatus(this ComputeService service, Identifier imageId, ImageStatus status, TimeSpan? refreshDelay = null, TimeSpan? timeout = null, IProgress<bool> progress = null)
        {
            service.WaitForImageStatusAsync(imageId, status, refreshDelay, timeout, progress).ForceSynchronous();
        }

        /// <inheritdoc cref="ComputeService.CreateImageMetadataAsync" />
        public static void CreateImageMetadata(this ComputeService service, Identifier imageId, string key, string value)
        {
            service.CreateImageMetadataAsync(imageId, key, value).ForceSynchronous();
        }

        /// <inheritdoc cref="ComputeService.ListImageSummariesAsync" />
        public static IPage<ImageSummary> ListImageSummaries(this ComputeService service, ImageListOptions options = null)
        {
            return service.ListImageSummariesAsync(options).ForceSynchronous();
        }

        /// <inheritdoc cref="ComputeService.ListImagesAsync" />
        public static IPage<Image> ListImages(this ComputeService service, ImageListOptions options = null)
        {
            return service.ListImagesAsync(options).ForceSynchronous();
        }

        /// <inheritdoc cref="ComputeService.UpdateImageMetadataAsync" />
        public static ImageMetadata UpdateImageMetadata(this ComputeService service, Identifier imageId, ImageMetadata metadata, bool overwrite = false)
        {
            return service.UpdateImageMetadataAsync(imageId, metadata, overwrite).ForceSynchronous();
        }
        
        /// <inheritdoc cref="ComputeService.DeleteImageAsync" />
        public static void DeleteImage(this ComputeService service, Identifier imageId)
        {
            service.DeleteImageAsync(imageId).ForceSynchronous();
        }

        /// <inheritdoc cref="ComputeService.DeleteImageMetadataAsync" />
        public static void DeleteImageMetadata(this ComputeService service, Identifier imageId, string key)
        {
            service.DeleteImageMetadataAsync(imageId, key).ForceSynchronous();
        }

        /// <inheritdoc cref="ComputeService.WaitUntilImageIsDeletedAsync" />
        public static void WaitUntilImageIsDeleted(this ComputeService service, Identifier imageId, TimeSpan? refreshDelay = null, TimeSpan? timeout = null, IProgress<bool> progress = null)
        {
            service.WaitUntilImageIsDeletedAsync(imageId, refreshDelay, timeout, progress).ForceSynchronous();
        }
        #endregion

        #region IP Addresses

        /// <inheritdoc cref="ComputeService.GetServerAddressAsync" />
        public static IList<ServerAddress> GetServerAddress(this ComputeService service, Identifier serverId, string key)
        {
            return service.GetServerAddressAsync(serverId, key).ForceSynchronous();
        }

        /// <inheritdoc cref="ComputeService.ListServerAddressesAsync" />
        public static IDictionary<string, IList<ServerAddress>> ListServerAddresses(this ComputeService service, Identifier serverId)
        {
            return service.ListServerAddressesAsync(serverId).ForceSynchronous();
        }

        #endregion

        #region Server Volumes
        /// <inheritdoc cref="ComputeService.GetServerVolumeAsync" />
        public static ServerVolume GetServerVolume(this ComputeService service, Identifier serverId, Identifier volumeId)
        {
            return service.GetServerVolumeAsync(serverId, volumeId).ForceSynchronous();
        }

        /// <inheritdoc cref="ComputeService.ListServerVolumesAsync" />
        public static IEnumerable<ServerVolume> ListServerVolumes(this ComputeService service, Identifier serverId)
        {
            return service.ListServerVolumesAsync(serverId).ForceSynchronous();
        }

        /// <inheritdoc cref="ComputeService.AttachVolumeAsync" />
        public static void AttachVolume(this ComputeService service, Identifier serverId, ServerVolumeDefinition volume)
        {
            service.AttachVolumeAsync(serverId, volume).ForceSynchronous();
        }

        /// <inheritdoc cref="ComputeService.DetachVolumeAsync" />
        public static void DetachVolume(this ComputeService service, Identifier serverId, Identifier volumeId)
        {
            service.DetachVolumeAsync(serverId, volumeId).ForceSynchronous();
        }
        #endregion

        #region KeyPairs
        /// <inheritdoc cref="ComputeService.GetKeyPairAsync" />
        public static KeyPair GetKeyPair(this ComputeService service, string keypairName)
        {
            return service.GetKeyPairAsync(keypairName).ForceSynchronous();
        }

        /// <inheritdoc cref="ComputeService.CreateKeyPairAsync" />
        public static KeyPairResponse CreateKeyPair(this ComputeService service, KeyPairRequest request)
        {
            return service.CreateKeyPairAsync(request).ForceSynchronous();
        }

        /// <inheritdoc cref="ComputeService.ImportKeyPairAsync" />
        public static KeyPairSummary ImportKeyPair(this ComputeService service, KeyPairDefinition keyPair)
        {
            return service.ImportKeyPairAsync(keyPair).ForceSynchronous();
        }

        /// <inheritdoc cref="ComputeService.ListKeyPairsAsync" />
        public static IEnumerable<KeyPairSummary> ListKeyPairs(this ComputeService service)
        {
            return service.ListKeyPairsAsync().ForceSynchronous();
        }

        /// <inheritdoc cref="ComputeService.DeleteKeyPairAsync" />
        public static void DeleteKeyPair(this ComputeService service, string keypairName)
        {
            service.DeleteKeyPairAsync(keypairName).ForceSynchronous();
        }

        #endregion

        #region Security Groups

        /// <inheritdoc cref="ComputeService.GetSecurityGroupAsync" />
        public static SecurityGroup GetSecurityGroup(this ComputeService service, Identifier securityGroupId)
        {
            return service.GetSecurityGroupAsync(securityGroupId).ForceSynchronous();
        }

        /// <inheritdoc cref="ComputeService.CreateSecurityGroupAsync" />
        public static SecurityGroup CreateSecurityGroup(this ComputeService service, SecurityGroupDefinition securityGroup)
        {
            return service.CreateSecurityGroupAsync(securityGroup).ForceSynchronous();
        }

        /// <inheritdoc cref="ComputeService.ListSecurityGroupsAsync" />
        public static IEnumerable<SecurityGroup> ListSecurityGroups(this ComputeService service, Identifier serverId = null)
        {
            return service.ListSecurityGroupsAsync(serverId).ForceSynchronous();
        }

        /// <inheritdoc cref="ComputeService.UpdateSecurityGroupAsync" />
        public static SecurityGroup UpdateSecurityGroup(this ComputeService service, Identifier securityGroupid, SecurityGroupDefinition securityGroup)
        {
            return service.UpdateSecurityGroupAsync(securityGroupid, securityGroup).ForceSynchronous();
        }

        /// <inheritdoc cref="ComputeService.DeleteSecurityGroupAsync" />
        public static void DeleteSecurityGroup(this ComputeService service, Identifier securityGroupId)
        {
            service.DeleteSecurityGroupAsync(securityGroupId).ForceSynchronous();
        }

        #endregion

        #region Server Groups

        /// <inheritdoc cref="ComputeService.GetServerGroupAsync" />
        public static ServerGroup GetServerGroup(this ComputeService service, Identifier severGroupId)
        {
            return service.GetServerGroupAsync(severGroupId).ForceSynchronous();
        }

        /// <inheritdoc cref="ComputeService.CreateServerGroupAsync" />
        public static ServerGroup CreateServerGroup(this ComputeService service, ServerGroupDefinition serverGroup)
        {
            return service.CreateServerGroupAsync(serverGroup).ForceSynchronous();
        }

        /// <inheritdoc cref="ComputeService.ListServerGroupsAsync" />
        public static IEnumerable<ServerGroup> ListServerGroups(this ComputeService service)
        {
            return service.ListServerGroupsAsync().ForceSynchronous();
        }

        /// <inheritdoc cref="ComputeService.DeleteServerGroupAsync" />
        public static void DeleteServerGroup(this ComputeService service, Identifier serverGroupId)
        {
            service.DeleteServerGroupAsync(serverGroupId).ForceSynchronous();
        }

        #endregion

        #region Volumes

        /// <inheritdoc cref="ComputeService.GetVolumeAsync" />
        public static Volume GetVolume(this ComputeService service, Identifier volumeId)
        {
            return service.GetVolumeAsync(volumeId).ForceSynchronous();
        }

        /// <inheritdoc cref="ComputeService.GetVolumeSnapshotAsync" />
        public static VolumeSnapshot GetVolumeSnapshot(this ComputeService service, Identifier snapshotId)
        {
            return service.GetVolumeSnapshotAsync(snapshotId).ForceSynchronous();
        }

        /// <inheritdoc cref="ComputeService.CreateVolumeAsync" />
        public static Volume CreateVolume(this ComputeService service, VolumeDefinition volume)
        {
            return service.CreateVolumeAsync(volume).ForceSynchronous();
        }

        /// <inheritdoc cref="ComputeService.SnapshotVolumeAsync" />
        public static VolumeSnapshot SnapshotVolume(this ComputeService service, VolumeSnapshotDefinition snapshot)
        {
            return service.SnapshotVolumeAsync(snapshot).ForceSynchronous();
        }

        /// <inheritdoc cref="ComputeService.ListVolumesAsync" />
        public static IEnumerable<Volume> ListVolumes(this ComputeService service)
        {
            return service.ListVolumesAsync().ForceSynchronous();
        }

        ///// <inheritdoc cref="ComputeService.ListVolumeTypesAsync" />
        //public static IEnumerable<VolumeType> ListVolumeTypes(this ComputeService service)
        //{
        //    return service.ListVolumeTypesAsync().ForceSynchronous();
        //}

        /// <inheritdoc cref="ComputeService.ListVolumeSnapshotsAsync" />
        public static IEnumerable<VolumeSnapshot> ListVolumeSnapshots(this ComputeService service)
        {
            return service.ListVolumeSnapshotsAsync().ForceSynchronous();
        }

        /// <inheritdoc cref="ComputeService.DeleteVolumeAsync" />
        public static void DeleteVolume(this ComputeService service, Identifier volumeId)
        {
            service.DeleteVolumeAsync(volumeId).ForceSynchronous();
        }

        /// <inheritdoc cref="ComputeService.DeleteVolumeSnapshotAsync" />
        public static void DeleteVolumeSnapshot(this ComputeService service, Identifier snapshotId)
        {
            service.DeleteVolumeSnapshotAsync(snapshotId).ForceSynchronous();
        }

        #endregion

        #region Compute Service
        /// <inheritdoc cref="ComputeService.GetLimitsAsync" />
        public static ServiceLimits GetLimits(this ComputeService service)
        {
            return service.GetLimitsAsync().ForceSynchronous();
        }
        #endregion
    }
}
