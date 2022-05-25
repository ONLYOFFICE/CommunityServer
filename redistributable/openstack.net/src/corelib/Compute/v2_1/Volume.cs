using System;
using System.Collections.Generic;
using System.Extensions;
using System.Linq;
using System.Runtime.Serialization;
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
    /// Represents a volume which can be attached to a server.
    /// </summary>
    [JsonConverterWithConstructor(typeof(RootWrapperConverter), "volume")]
    public class Volume : IServiceResource, IHaveExtraData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Volume"/> class.
        /// </summary>
        public Volume()
        {
            Attachments = new List<ServerVolume>();    
            Metadata = new Dictionary<string, string>();
        }

        /// <summary>
        /// The volume identifier.
        /// </summary>
        [JsonProperty("id")]
        public Identifier Id { get; set; }

        /// <summary>
        /// The volume name.
        /// </summary>
        [JsonProperty("displayName")]
        public string Name { get; set; }

        /// <summary>
        /// The volume status.
        /// </summary>
        [JsonProperty("status")]
        public VolumeStatus Status { get; set; }

        /// <summary>
        /// The volume description.
        /// </summary>
        [JsonProperty("displayDescription")]
        public string Description { get; set; }

        /// <summary>
        /// The size of the volume, in gigabytes (GB).
        /// </summary>
        [JsonProperty("size")]
        public int Size { get; set; }

        /// <summary>
        /// The unique identifier for a volume type.
        /// </summary>
        [JsonProperty("volumeType")]
        public Identifier VolumeTypeId { get; set; }

        /// <summary>
        /// One or more metadata key and value pairs to associate with the volume.
        /// </summary>
        [JsonProperty("metadata")]
        public IDictionary<string, string> Metadata { get; set; }

        /// <summary>
        /// The availability zone.
        /// </summary>
        [JsonProperty("availabilityZone")]
        public string AvailabilityZone { get; set; }

        /// <summary>
        /// The snapshot from which to create a volume.
        /// </summary>
        [JsonProperty("snapshotId")]
        public Identifier SourceSnapshotId { get; set; }

        /// <summary>
        /// A list of servers to which this volume is attached.
        /// </summary>
        [JsonProperty("attachments")]
        public IList<ServerVolume> Attachments { get; set; }

        /// <summary>
        /// The date and time when the resource was created.
        /// </summary>
        [JsonProperty("createdAt")]
        public DateTime Created { get; set; }

        [JsonExtensionData]
        IDictionary<string, JToken> IHaveExtraData.Data { get; set; } = new Dictionary<string, JToken>();

        object IServiceResource.Owner { get; set; }

        /// <summary>
        /// Waits the until the volume is available.
        /// </summary>
        /// <param name="refreshDelay">The refresh delay.</param>
        /// <param name="timeout">The timeout.</param>
        /// <param name="progress">The progress.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="InvalidOperationException">When the instance was not constructed by the <see cref="ComputeService" />, as it is missing the appropriate internal state to execute service calls.</exception>
        public Task WaitUntilAvailableAsync(TimeSpan? refreshDelay = null, TimeSpan? timeout = null, IProgress<bool> progress = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return WaitForStatusAsync(VolumeStatus.Available, refreshDelay, timeout, progress, cancellationToken);
        }

        /// <inheritdoc cref="ComputeApi.WaitUntilVolumeIsDeletedAsync{TVolume,TStatus}" />
        /// <exception cref="InvalidOperationException">When the instance was not constructed by the <see cref="ComputeService"/>, as it is missing the appropriate internal state to execute service calls.</exception>
        public Task WaitUntilDeletedAsync(TimeSpan? refreshDelay = null, TimeSpan? timeout = null, IProgress<bool> progress = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var owner = this.GetOwnerOrThrow<ComputeApi>();
            return owner.WaitUntilVolumeIsDeletedAsync<Volume, VolumeStatus>(Id, refreshDelay, timeout, progress, cancellationToken);
        }

        /// <inheritdoc cref="ComputeApi.WaitForVolumeStatusAsync{TVolume,TStatus}(string,IEnumerable{TStatus},TimeSpan?,TimeSpan?,IProgress{bool},CancellationToken)" />
        /// <exception cref="InvalidOperationException">When the instance was not constructed by the <see cref="ComputeService"/>, as it is missing the appropriate internal state to execute service calls.</exception>
        public async Task WaitForStatusAsync(IEnumerable<VolumeStatus> status, TimeSpan? refreshDelay = null, TimeSpan? timeout = null, IProgress<bool> progress = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var owner = this.GetOwnerOrThrow<ComputeApi>();
            var result = await owner.WaitForVolumeStatusAsync<Volume, VolumeStatus>(Id, status, refreshDelay, timeout, progress, cancellationToken).ConfigureAwait(false);
            result.CopyProperties(this);
        }

        /// <inheritdoc cref="ComputeApi.WaitForVolumeStatusAsync{TVolume,TStatus}(string,TStatus,TimeSpan?,TimeSpan?,IProgress{bool},CancellationToken)" />
        /// <exception cref="InvalidOperationException">When the instance was not constructed by the <see cref="ComputeService"/>, as it is missing the appropriate internal state to execute service calls.</exception>
        public async Task WaitForStatusAsync(VolumeStatus status, TimeSpan? refreshDelay = null, TimeSpan? timeout = null, IProgress<bool> progress = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var owner = this.GetOwnerOrThrow<ComputeApi>();
            var result = await owner.WaitForVolumeStatusAsync<Volume, VolumeStatus>(Id, status, refreshDelay, timeout, progress, cancellationToken).ConfigureAwait(false);
            result.CopyProperties(this);
        }

        /// <inheritdoc cref="ComputeApi.SnapshotVolumeAsync{T}" />
        public Task<VolumeSnapshot> SnapshotAsync(VolumeSnapshotDefinition snapshot = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if(snapshot == null)
                snapshot = new VolumeSnapshotDefinition();

            snapshot.VolumeId = Id;

            var owner = this.GetOwnerOrThrow<ComputeApi>();
            return owner.SnapshotVolumeAsync<VolumeSnapshot>(snapshot, cancellationToken);
        }

        /// <inheritdoc cref="ComputeApi.DeleteVolumeAsync" />
        public Task DeleteAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var owner = this.GetOwnerOrThrow<ComputeApi>();
            return owner.DeleteVolumeAsync(Id, cancellationToken);
        }

        [OnDeserialized]
        private void OnDeserialize(StreamingContext context)
        {
            // Cleanup after an issue in Nova where an empty attachment is always returned.
            var emptyAttachment = Attachments.FirstOrDefault(x => x.Id == null);
            if (emptyAttachment != null)
            {
                Attachments.Remove(emptyAttachment);
            }
        }
    }
}