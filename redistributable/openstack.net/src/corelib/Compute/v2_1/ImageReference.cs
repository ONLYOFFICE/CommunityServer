using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenStack.Compute.v2_1.Serialization;
using OpenStack.Images.v2;
using OpenStack.Serialization;

namespace OpenStack.Compute.v2_1
{
    /// <summary>
    /// Reference to an image.
    /// </summary>
    public class ImageReference : IHaveExtraData, IServiceResource
    {
        /// <summary>
        /// The image identifier.
        /// </summary>
        [JsonProperty("id")]
        public virtual Identifier Id { get; set; }

        /// <summary />
        [JsonExtensionData]
        IDictionary<string, JToken> IHaveExtraData.Data { get; set; } = new Dictionary<string, JToken>();

        object IServiceResource.Owner { get; set; }

        /// <inheritdoc cref="ComputeApi.GetImageAsync{T}" />
        /// <exception cref="InvalidOperationException">When the <see cref="ImageReference"/> instance was not constructed by the <see cref="ComputeService"/>, as it is missing the appropriate internal state to execute service calls.</exception>
        public Task<Image> GetImageAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var owner = this.GetOwnerOrThrow<ComputeApi>();
            return owner.GetImageAsync<Image>(Id, cancellationToken);
        }

        /// <inheritdoc cref="ComputeApi.GetImageMetadataAsync{T}" />
        /// <exception cref="InvalidOperationException">When the <see cref="ImageReference"/> instance was not constructed by the <see cref="ComputeService"/>, as it is missing the appropriate internal state to execute service calls.</exception>
        public Task<ImageMetadata> GetMetadataAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var owner = this.GetOwnerOrThrow<ComputeApi>();
            return owner.GetImageMetadataAsync<ImageMetadata>(Id, cancellationToken);
        }

        /// <inheritdoc cref="ComputeApi.GetImageMetadataItemAsync" />
        /// <exception cref="InvalidOperationException">When the <see cref="ImageReference"/> instance was not constructed by the <see cref="ComputeService"/>, as it is missing the appropriate internal state to execute service calls.</exception>
        public Task<string> GetMetadataItemAsync(string key, CancellationToken cancellationToken = default(CancellationToken))
        {
            var owner = this.GetOwnerOrThrow<ComputeApi>();
            return owner.GetImageMetadataItemAsync(Id, key, cancellationToken);
        }

        /// <inheritdoc cref="ComputeApi.DeleteImageAsync" />
        /// <exception cref="InvalidOperationException">When the <see cref="Server"/> instance was not constructed by the <see cref="ComputeService"/>, as it is missing the appropriate internal state to execute service calls.</exception>
        public virtual async Task DeleteAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var owner = this.GetOwnerOrThrow<ComputeApi>();
            await owner.DeleteImageAsync(Id, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc cref="ComputeApi.WaitUntilImageIsDeletedAsync{TImage,TStatus}" />
        /// <exception cref="InvalidOperationException">When this instance was not constructed by the <see cref="ComputeService"/>, as it is missing the appropriate internal state to execute service calls.</exception>
        public virtual async Task WaitUntilDeletedAsync(TimeSpan? refreshDelay = null, TimeSpan? timeout = null, IProgress<bool> progress = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var owner = this.GetOwnerOrThrow<ComputeApi>();
            await owner.WaitUntilImageIsDeletedAsync<Image, ImageStatus>(Id, null, refreshDelay, timeout, progress, cancellationToken).ConfigureAwait(false);
        }
    }
}