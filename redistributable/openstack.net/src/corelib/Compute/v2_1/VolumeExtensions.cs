using System;
using System.Collections.Generic;
using OpenStack.BlockStorage.v2;
using OpenStack.Compute.v2_1;
using OpenStack.Synchronous.Extensions;


// ReSharper disable once CheckNamespace
namespace OpenStack.Synchronous
{
    /// <summary>
    /// Provides synchronous extention methods for a <see cref="Volume"/> instance.
    /// </summary>
    public static class VolumeExtensions_v2_1
    {
        /// <inheritdoc cref="Volume.SnapshotAsync"/>
        public static VolumeSnapshot Snapshot(this Volume volume, VolumeSnapshotDefinition snapshot = null)
        {
            return volume.SnapshotAsync(snapshot).ForceSynchronous();
        }

        /// <inheritdoc cref="Volume.DeleteAsync"/>
        public static void Delete(this Volume volume)
        {
            volume.DeleteAsync().ForceSynchronous();
        }

        /// <inheritdoc cref="Volume.WaitForStatusAsync(VolumeStatus,TimeSpan?,TimeSpan?,IProgress{bool},System.Threading.CancellationToken)"/>
        public static void WaitForStatus(this Volume volume, VolumeStatus status, TimeSpan? refreshDelay = null, TimeSpan? timeout = null, IProgress<bool> progress = null)
        {
            volume.WaitForStatusAsync(status, refreshDelay, timeout, progress).ForceSynchronous();
        }

        /// <inheritdoc cref="Volume.WaitForStatusAsync(IEnumerable{VolumeStatus},TimeSpan?,TimeSpan?,IProgress{bool},System.Threading.CancellationToken)"/>
        public static void WaitForStatus(this Volume volume, IEnumerable<VolumeStatus> status, TimeSpan? refreshDelay = null, TimeSpan? timeout = null, IProgress<bool> progress = null)
        {
            volume.WaitForStatusAsync(status, refreshDelay, timeout, progress).ForceSynchronous();
        }

        /// <inheritdoc cref="Volume.WaitUntilAvailableAsync"/>
        public static void WaitUntilAvailable(this Volume volume, TimeSpan? refreshDelay = null, TimeSpan? timeout = null, IProgress<bool> progress = null)
        {
            volume.WaitUntilAvailableAsync(refreshDelay, timeout, progress).ForceSynchronous();
        }

        /// <inheritdoc cref="Volume.WaitUntilDeletedAsync"/>
        public static void WaitUntilDeleted(this Volume volume, TimeSpan? refreshDelay = null, TimeSpan? timeout = null, IProgress<bool> progress = null)
        {
            volume.WaitUntilDeletedAsync(refreshDelay, timeout, progress).ForceSynchronous();
        }
    }
}
