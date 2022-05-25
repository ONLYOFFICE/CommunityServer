using System;
using System.Collections.Generic;
using net.openstack.Core.Domain;
using net.openstack.Core.Exceptions;
using net.openstack.Core.Exceptions.Response;
using net.openstack.Providers.Rackspace.Exceptions;

namespace net.openstack.Core.Providers
{
    /// <summary>
    /// Represents a provider for the OpenStack Block Storage Service.
    /// </summary>
    /// <seealso href="http://docs.openstack.org/api/openstack-block-storage/2.0/content/">OpenStack Block Storage Service API v2 Reference</seealso>
    public interface IBlockStorageProvider
    {
        #region Volume

        /// <summary>
        /// Creates a new block storage volume.
        /// </summary>
        /// <param name="size">The size of the volume in GB.</param>
        /// <param name="displayDescription">A description of the volume.</param>
        /// <param name="displayName">The name of the volume.</param>
        /// <param name="snapshotId">The snapshot from which to create a volume. The value should be <see langword="null"/> or obtained from <see cref="Snapshot.Id">Snapshot.Id</see>.</param>
        /// <param name="volumeType">The type of volume to create. If not defined, then the default is used. The value should be <see langword="null"/> or obtained from <see cref="VolumeType.Id">VolumeType.Id</see>.</param>
        /// <param name="region">The region in which to execute this action. If not specified, the user's default region will be used.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns><see langword="true"/> if the request succeeded; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="size"/> is less than zero.</exception>
        /// <exception cref="InvalidVolumeSizeException">If <paramref name="size"/> is not valid for this provider.</exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="identity"/> type.
        /// <para>-or-</para>
        /// <para>If the specified <paramref name="volumeType"/> is not supported.</para>
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// <para>-or-</para>
        /// <para>If <paramref name="region"/> is <see langword="null"/> and no default region is available for the provider.</para>
        /// </exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso href="http://docs.openstack.org/api/openstack-block-storage/2.0/content/Create_Volume.html">Create Volume (OpenStack Block Storage Service API Reference)</seealso>
        Volume CreateVolume(int size, string displayDescription = null, string displayName = null, string snapshotId = null, string volumeType = null, string region = null, CloudIdentity identity = null);

        /// <summary>
        /// Gets a list of volumes.
        /// </summary>
        /// <param name="region">The region in which to execute this action. If not specified, the user's default region will be used.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns>A collection of <see cref="Volume"/> objects describing the volumes.</returns>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="identity"/> type.
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// <para>-or-</para>
        /// <para>If <paramref name="region"/> is <see langword="null"/> and no default region is available for the provider.</para>
        /// </exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso href="http://docs.openstack.org/api/openstack-block-storage/2.0/content/List_Summary_Volumes.html">List Volume Summaries (OpenStack Block Storage Service API Reference)</seealso>
        IEnumerable<Volume> ListVolumes(string region = null, CloudIdentity identity = null);

        /// <summary>
        /// View information about a single volume.
        /// </summary>
        /// <param name="volumeId">The ID of the volume. The value should be obtained from <see cref="Volume.Id">Volume.Id</see>.</param>
        /// <param name="region">The region in which to execute this action. If not specified, the user's default region will be used.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns>A <see cref="Volume"/> object containing the volume details.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="volumeId"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="volumeId"/> is empty.</exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="identity"/> type.
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// <para>-or-</para>
        /// <para>If <paramref name="region"/> is <see langword="null"/> and no default region is available for the provider.</para>
        /// </exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso href="http://docs.openstack.org/api/openstack-block-storage/2.0/content/Show_Volume.html">Show Volume (OpenStack Block Storage Service API Reference)</seealso>
        Volume ShowVolume(string volumeId, string region = null, CloudIdentity identity = null);

        /// <summary>
        /// Deletes a volume.
        /// </summary>
        /// <remarks>
        /// The deletion operation is performed asynchronously. After this call returns,
        /// <see cref="WaitForVolumeDeleted"/> may be called to wait until the volume
        /// is finally deleted.
        ///
        /// <note type="note">
        /// It is not currently possible to delete a volume once you have created a snapshot from it. Any snapshots will need to be deleted prior to deleting the volume.
        /// </note>
        /// </remarks>
        /// <param name="volumeId">The ID of the volume to delete. The value should be obtained from <see cref="Volume.Id">Volume.Id</see>.</param>
        /// <param name="region">The region in which to execute this action. If not specified, the user's default region will be used.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns><see langword="true"/> if the volume was successfully deleted; otherwise <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="volumeId"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="volumeId"/> is empty.</exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="identity"/> type.
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// <para>-or-</para>
        /// <para>If <paramref name="region"/> is <see langword="null"/> and no default region is available for the provider.</para>
        /// </exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso href="http://docs.openstack.org/api/openstack-block-storage/2.0/content/Delete_Volume.html">Delete Volume (OpenStack Block Storage Service API Reference)</seealso>
        bool DeleteVolume(string volumeId, string region = null, CloudIdentity identity = null);

        /// <summary>
        /// Get a list of volume types.
        /// </summary>
        /// <param name="region">The region in which to execute this action. If not specified, the user's default region will be used.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns>A collection of <see cref="VolumeType"/> objects containing the details of each volume type.</returns>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="identity"/> type.
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// <para>-or-</para>
        /// <para>If <paramref name="region"/> is <see langword="null"/> and no default region is available for the provider.</para>
        /// </exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso href="http://docs.openstack.org/api/openstack-block-storage/2.0/content/Volume_List_Types.html">List Volume Types (OpenStack Block Storage Service API Reference)</seealso>
        IEnumerable<VolumeType> ListVolumeTypes(string region = null, CloudIdentity identity = null);

        /// <summary>
        /// Get information about a volume type.
        /// </summary>
        /// <param name="volumeTypeId">The ID of the volume type. The value should be obtained from <see cref="VolumeType.Id">VolumeType.Id</see>.</param>
        /// <param name="region">The region in which to execute this action. If not specified, the user's default region will be used.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns>A <see cref="VolumeType"/> object containing the details of the volume type.</returns>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="identity"/> type.
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// <para>-or-</para>
        /// <para>If <paramref name="region"/> is <see langword="null"/> and no default region is available for the provider.</para>
        /// </exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso href="http://docs.openstack.org/api/openstack-block-storage/2.0/content/Volume_Show_Type.html">Show Volume Type (OpenStack Block Storage Service API Reference)</seealso>
        VolumeType DescribeVolumeType(string volumeTypeId, string region = null, CloudIdentity identity = null);

        /// <summary>
        /// Waits for a volume to be set to <see cref="VolumeState.Available"/> status.
        /// </summary>
        /// <remarks>
        /// This method can be used to ensure that a volume is correctly created prior to executing additional requests against it.
        /// </remarks>
        /// <param name="volumeId">The ID of the volume to poll. The value should be obtained from <see cref="Volume.Id">Volume.Id</see>.</param>
        /// <param name="refreshCount">The number of times to poll for the volume to become available.</param>
        /// <param name="refreshDelay">The refresh delay. If the value is <see langword="null"/>, the default value is 2.4 seconds.</param>
        /// <param name="region">The region in which to execute this action. If not specified, the user's default region will be used.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns>A <see cref="Volume"/> object containing the details of the volume, including the final <see cref="Volume.Status"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="volumeId"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="volumeId"/> is empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="refreshCount"/> is less than 0.
        /// <para>-or-</para>
        /// <para>If <paramref name="refreshDelay"/> is negative.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="identity"/> type.
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// <para>-or-</para>
        /// <para>If <paramref name="region"/> is <see langword="null"/> and no default region is available for the provider.</para>
        /// </exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        Volume WaitForVolumeAvailable(string volumeId, int refreshCount = 600, TimeSpan? refreshDelay = null, string region = null, CloudIdentity identity = null);

        /// <summary>
        /// Waits for a volume to be deleted.
        /// </summary>
        /// <remarks>
        /// This method can be used to ensure that a volume is completely removed.
        /// </remarks>
        /// <param name="volumeId">The ID of the volume to poll. The value should be obtained from <see cref="Volume.Id">Volume.Id</see>.</param>
        /// <param name="refreshCount">The number of times to poll for the volume to be deleted.</param>
        /// <param name="refreshDelay">The refresh delay. If the value is <see langword="null"/>, the default value is 10 seconds.</param>
        /// <param name="region">The region in which to execute this action. If not specified, the user's default region will be used.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns><see langword="true"/> if the volume was successfully deleted; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="volumeId"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="volumeId"/> is empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="refreshCount"/> is less than 0.
        /// <para>-or-</para>
        /// <para>If <paramref name="refreshDelay"/> is negative.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="identity"/> type.
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// <para>-or-</para>
        /// <para>If <paramref name="region"/> is <see langword="null"/> and no default region is available for the provider.</para>
        /// </exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        bool WaitForVolumeDeleted(string volumeId, int refreshCount = 360, TimeSpan? refreshDelay = null, string region = null, CloudIdentity identity = null);

        /// <summary>
        /// Waits for a volume to be set to be set to a particular <see cref="VolumeState"/>.
        /// </summary>
        /// <remarks>
        /// This method can be used to ensure that a volume is in an intended state prior to
        /// executing additional requests against it.
        /// </remarks>
        /// <param name="volumeId">The ID of the volume to poll. The value should be obtained from <see cref="Volume.Id">Volume.Id</see>.</param>
        /// <param name="expectedState">The expected state for the volume.</param>
        /// <param name="errorStates">The error state(s) in which to stop polling once reached.</param>
        /// <param name="refreshCount">The number of times to poll the volume.</param>
        /// <param name="refreshDelay">The refresh delay. If the value is <see langword="null"/>, the default value is 10 seconds.</param>
        /// <param name="region">The region in which to execute this action. If not specified, the user's default region will be used.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns>A <see cref="Volume"/> object containing the details of the volume, including the final <see cref="Volume.Status"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="volumeId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="expectedState"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="errorStates"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="volumeId"/> is empty.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="refreshCount"/> is less than 0.
        /// <para>-or-</para>
        /// <para>If <paramref name="refreshDelay"/> is negative.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="identity"/> type.
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// <para>-or-</para>
        /// <para>If <paramref name="region"/> is <see langword="null"/> and no default region is available for the provider.</para>
        /// </exception>
        /// <exception cref="VolumeEnteredErrorStateException">If the method returned due to the volume entering one of the <paramref name="errorStates"/>.</exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso href="http://docs.rackspace.com/cbs/api/v1.0/cbs-devguide/content/volume_status.html">Volume Status (Rackspace Cloud Block Storage Developer Guide - API V1.0)</seealso>
        Volume WaitForVolumeState(string volumeId, VolumeState expectedState, VolumeState[] errorStates, int refreshCount = 600, TimeSpan? refreshDelay = null, string region = null, CloudIdentity identity = null);

        #endregion

        #region Snapshot

        /// <summary>
        /// Creates a new snapshot.
        /// </summary>
        /// <remarks>
        /// The snapshot operation is performed asynchronously. After this call returns,
        /// <see cref="WaitForSnapshotAvailable"/> may be called to wait until the snapshot
        /// process is complete and the snapshot is available.
        ///
        /// <para>Creating a snapshot makes a point-in-time copy of the volume.
        /// All writes to the volume should be flushed before creating the snapshot, either by un-mounting any file systems on the volume, or by detaching the volume before creating the snapshot.
        /// Snapshots are incremental, so each time you create a new snapshot, you are appending the incremental changes for the new snapshot to the previous one.
        /// The previous snapshot is still available. Note that you can create a new volume from the snapshot if desired.</para>
        /// </remarks>
        /// <param name="volumeId">The ID of the volume to snapshot. The value should be obtained from <see cref="Volume.Id">Volume.Id</see>.</param>
        /// <param name="force">If <see langword="true"/>, the snapshot is created even if the volume is currently attached.</param>
        /// <param name="displayName">Name of the snapshot.</param>
        /// <param name="displayDescription">Description of snapshot.</param>
        /// <param name="region">The region in which to execute this action. If not specified, the user's default region will be used.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns>A <see cref="Snapshot"/> object containing the details about the snapshot.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="volumeId"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="volumeId"/> is empty.</exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="identity"/> type.
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// <para>-or-</para>
        /// <para>If <paramref name="region"/> is <see langword="null"/> and no default region is available for the provider.</para>
        /// </exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso href="http://docs.openstack.org/api/openstack-block-storage/2.0/content/Create_Snapshot.html">Create Snapshot (OpenStack Block Storage Service API Reference)</seealso>
        Snapshot CreateSnapshot(string volumeId, bool force = false, string displayName = "None", string displayDescription = null, string region = null, CloudIdentity identity = null);

        /// <summary>
        /// Get a list of snapshots.
        /// </summary>
        /// <param name="region">The region in which to execute this action. If not specified, the user's default region will be used.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns>A collection of <see cref="Snapshot"/> objects containing the details of each snapshot.</returns>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="identity"/> type.
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// <para>-or-</para>
        /// <para>If <paramref name="region"/> is <see langword="null"/> and no default region is available for the provider.</para>
        /// </exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso href="http://docs.openstack.org/api/openstack-block-storage/2.0/content/List_Snapshots.html">List Snapshot Summaries (OpenStack Block Storage Service API Reference)</seealso>
        IEnumerable<Snapshot> ListSnapshots(string region = null, CloudIdentity identity = null);

        /// <summary>
        /// View all information about a single snapshot.
        /// </summary>
        /// <param name="snapshotId">The ID of the snapshot. The value should be obtained from <see cref="Snapshot.Id">Snapshot.Id</see>.</param>
        /// <param name="region">The region in which to execute this action. If not specified, the user's default region will be used.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns>A <see cref="Snapshot"/> object containing the snapshot details.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="snapshotId"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="snapshotId"/> is empty.</exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="identity"/> type.
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// <para>-or-</para>
        /// <para>If <paramref name="region"/> is <see langword="null"/> and no default region is available for the provider.</para>
        /// </exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso href="http://docs.openstack.org/api/openstack-block-storage/2.0/content/Show_Snapshot.html">Show Snapshot (OpenStack Block Storage Service API Reference)</seealso>
        Snapshot ShowSnapshot(string snapshotId, string region = null, CloudIdentity identity = null);

        /// <summary>
        /// Marks a snapshot for deletion.
        /// </summary>
        /// <remarks>
        /// The deletion operation is performed asynchronously. After this call returns,
        /// <see cref="WaitForSnapshotDeleted"/> may be called to wait until the snapshot
        /// is finally deleted.
        /// </remarks>
        /// <param name="snapshotId">The ID of the snapshot. The value should be obtained from <see cref="Snapshot.Id">Snapshot.Id</see>.</param>
        /// <param name="region">The region in which to execute this action. If not specified, the user's default region will be used.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns><see langword="true"/> if the snapshot was successfully marked for deletion; otherwise <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="snapshotId"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="snapshotId"/> is empty.</exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="identity"/> type.
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// <para>-or-</para>
        /// <para>If <paramref name="region"/> is <see langword="null"/> and no default region is available for the provider.</para>
        /// </exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso href="http://docs.openstack.org/api/openstack-block-storage/2.0/content/Delete_Snapshot.html">Delete Snapshot (OpenStack Block Storage Service API Reference)</seealso>
        bool DeleteSnapshot(string snapshotId, string region = null, CloudIdentity identity = null);

        /// <summary>
        /// Waits for a snapshot to be set to <see cref="SnapshotState.Available"/> status.
        /// </summary>
        /// <remarks>
        /// This method can be used to ensure that a snapshot is correctly created prior to executing additional requests against it.
        /// </remarks>
        /// <param name="snapshotId">The ID of the snapshot to poll. The value should be obtained from <see cref="Snapshot.Id">Snapshot.Id</see>.</param>
        /// <param name="refreshCount">The number of times to poll for the snapshot to become available.</param>
        /// <param name="refreshDelay">The refresh delay. If the value is <see langword="null"/>, the default value is 10 seconds.</param>
        /// <param name="region">The region in which to execute this action. If not specified, the user's default region will be used.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns>A <see cref="Snapshot"/> object containing the snapshot details, including the final <see cref="Snapshot.Status"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="snapshotId"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="snapshotId"/> is empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="refreshCount"/> is less than 0.
        /// <para>-or-</para>
        /// <para>If <paramref name="refreshDelay"/> is negative.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="identity"/> type.
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// <para>-or-</para>
        /// <para>If <paramref name="region"/> is <see langword="null"/> and no default region is available for the provider.</para>
        /// </exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        Snapshot WaitForSnapshotAvailable(string snapshotId, int refreshCount = 360, TimeSpan? refreshDelay = null, string region = null, CloudIdentity identity = null);

        /// <summary>
        /// Waits for a snapshot to be deleted.
        /// </summary>
        /// <remarks>
        /// This method can be used to ensure that a snapshot is completely removed.
        /// </remarks>
        /// <param name="snapshotId">The ID of the snapshot to poll. The value should be obtained from <see cref="Snapshot.Id">Snapshot.Id</see>.</param>
        /// <param name="refreshCount">The number of times to poll for the snapshot to be deleted.</param>
        /// <param name="refreshDelay">The refresh delay. If the value is <see langword="null"/>, the default value is 10 seconds.</param>
        /// <param name="region">The region in which to execute this action. If not specified, the user's default region will be used.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns><see langword="true"/> if the snapshot was successfully deleted; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="snapshotId"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="snapshotId"/> is empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="refreshCount"/> is less than 0.
        /// <para>-or-</para>
        /// <para>If <paramref name="refreshDelay"/> is negative.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="identity"/> type.
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// <para>-or-</para>
        /// <para>If <paramref name="region"/> is <see langword="null"/> and no default region is available for the provider.</para>
        /// </exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        bool WaitForSnapshotDeleted(string snapshotId, int refreshCount = 180, TimeSpan? refreshDelay = null, string region = null, CloudIdentity identity = null);

        /// <summary>
        /// Waits for a snapshot to be set to be set to a particular status.
        /// </summary>
        /// <remarks>
        /// This method can be used to ensure that a snapshot is in an intended state prior to
        /// executing additional requests against it.
        /// </remarks>
        /// <param name="snapshotId">The ID of the snapshot to poll. The value should be obtained from <see cref="Snapshot.Id">Snapshot.Id</see>.</param>
        /// <param name="expectedState">The expected state for the snapshot.</param>
        /// <param name="errorStates">The error state(s) in which to stop polling once reached.</param>
        /// <param name="refreshCount">The number of times to poll the snapshot.</param>
        /// <param name="refreshDelay">The refresh delay. If the value is <see langword="null"/>, the default value is 10 seconds.</param>
        /// <param name="region">The region in which to execute this action. If not specified, the user's default region will be used.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns>A <see cref="Snapshot"/> object containing the snapshot details, including the final <see cref="Snapshot.Status"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="snapshotId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="expectedState"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="errorStates"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="snapshotId"/> is empty.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="refreshCount"/> is less than 0.
        /// <para>-or-</para>
        /// <para>If <paramref name="refreshDelay"/> is negative.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="identity"/> type.
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// <para>-or-</para>
        /// <para>If <paramref name="region"/> is <see langword="null"/> and no default region is available for the provider.</para>
        /// </exception>
        /// <exception cref="SnapshotEnteredErrorStateException">If the method returned due to the snapshot entering one of the <paramref name="errorStates"/>.</exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        Snapshot WaitForSnapshotState(string snapshotId, SnapshotState expectedState, SnapshotState[] errorStates, int refreshCount = 60, TimeSpan? refreshDelay = null, string region = null, CloudIdentity identity = null);

        #endregion
    }
}
