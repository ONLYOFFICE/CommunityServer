using System;
using System.Collections.Generic;
using System.Extensions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenStack.BlockStorage.v2;
using OpenStack.Compute.v2_1.Serialization;
using OpenStack.Serialization;

namespace OpenStack.Compute.v2_1
{
    /// <summary>
    /// Represents a snapshot of a volume.
    /// </summary>
    [JsonConverterWithConstructor(typeof(RootWrapperConverter), "snapshot")]
    public class VolumeSnapshot : IServiceResource, IHaveExtraData
    {
        /// <summary>
        /// The volume snapshot identifier.
        /// </summary>
        [JsonProperty("id")]
        public Identifier Id { get; set; }

        /// <summary>
        /// The volume identifier.
        /// </summary>
        [JsonProperty("volumeId")]
        public Identifier VolumeId { get; set; }

        /// <summary>
        /// The snapshot name.
        /// </summary>
        [JsonProperty("displayName")]
        public string Name { get; set; }

        /// <summary>
        /// The snapshot description.
        /// </summary>
        [JsonProperty("displayDescription")]
        public string Description { get; set; }

        /// <summary>
        /// The snapshot size, in GB.
        /// </summary>
        [JsonProperty("size")]
        public int Size { get; set; }

        /// <summary>
        /// The snapshot status.
        /// </summary>
        [JsonProperty("status")]
        public SnapshotStatus Status { get; set; }

        /// <summary>
        /// The date and time when the resource was created.
        /// </summary>
        [JsonProperty("createdAt")]
        public DateTime Created { get; set; }

        [JsonExtensionData]
        IDictionary<string, JToken> IHaveExtraData.Data { get; set; } = new Dictionary<string, JToken>();

        object IServiceResource.Owner { get; set; }

        /// <summary>
        /// Waits the until the snapshot is available.
        /// </summary>
        /// <param name="refreshDelay">The refresh delay.</param>
        /// <param name="timeout">The timeout.</param>
        /// <param name="progress">The progress.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="InvalidOperationException">When the instance was not constructed by the <see cref="ComputeService" />, as it is missing the appropriate internal state to execute service calls.</exception>
        public Task WaitUntilAvailableAsync(TimeSpan? refreshDelay = null, TimeSpan? timeout = null, IProgress<bool> progress = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return WaitForStatusAsync(SnapshotStatus.Available, refreshDelay, timeout, progress, cancellationToken);
        }

        /// <inheritdoc cref="ComputeApi.WaitUntilVolumeIsDeletedAsync{TSnapshot,TStatus}" />
        /// <exception cref="InvalidOperationException">When the instance was not constructed by the <see cref="ComputeService"/>, as it is missing the appropriate internal state to execute service calls.</exception>
        public Task WaitUntilDeletedAsync(TimeSpan? refreshDelay = null, TimeSpan? timeout = null, IProgress<bool> progress = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var owner = this.GetOwnerOrThrow<ComputeApi>();
            return owner.WaitUntilVolumeSnapshotIsDeletedAsync<VolumeSnapshot, SnapshotStatus>(Id, refreshDelay, timeout, progress, cancellationToken);
        }

        /// <inheritdoc cref="ComputeApi.WaitForVolumeStatusAsync{TSnapshot,TStatus}(string,IEnumerable{TStatus},TimeSpan?,TimeSpan?,IProgress{bool},CancellationToken)" />
        /// <exception cref="InvalidOperationException">When the instance was not constructed by the <see cref="ComputeService"/>, as it is missing the appropriate internal state to execute service calls.</exception>
        public async Task WaitForStatusAsync(IEnumerable<SnapshotStatus> status, TimeSpan? refreshDelay = null, TimeSpan? timeout = null, IProgress<bool> progress = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var owner = this.GetOwnerOrThrow<ComputeApi>();
            var result = await owner.WaitForVolumeSnapshotStatusAsync<VolumeSnapshot, SnapshotStatus>(Id, status, refreshDelay, timeout, progress, cancellationToken).ConfigureAwait(false);
            result.CopyProperties(this);
        }

        /// <inheritdoc cref="ComputeApi.WaitForVolumeStatusAsync{TServer,TStatus}(string,TStatus,TimeSpan?,TimeSpan?,IProgress{bool},CancellationToken)" />
        /// <exception cref="InvalidOperationException">When the instance was not constructed by the <see cref="ComputeService"/>, as it is missing the appropriate internal state to execute service calls.</exception>
        public async Task WaitForStatusAsync(SnapshotStatus status, TimeSpan? refreshDelay = null, TimeSpan? timeout = null, IProgress<bool> progress = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var owner = this.GetOwnerOrThrow<ComputeApi>();
            var result = await owner.WaitForVolumeSnapshotStatusAsync<VolumeSnapshot, SnapshotStatus>(Id, status, refreshDelay, timeout, progress, cancellationToken).ConfigureAwait(false);
            result.CopyProperties(this);
        }

        /// <inheritdoc cref="ComputeApi.DeleteVolumeSnapshotAsync" />
        public Task DeleteAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var owner = this.GetOwnerOrThrow<ComputeApi>();
            return owner.DeleteVolumeSnapshotAsync(Id, cancellationToken);
        }
    }
}