using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenStack.Compute.v2_1.Serialization;
using OpenStack.Serialization;

namespace OpenStack.Compute.v2_1
{
    /// <summary>
    /// Metadata key pairs containing information about the image.
    /// </summary>
    [JsonConverterWithConstructor(typeof (RootWrapperConverter), "metadata")]
    public class ImageMetadata : Dictionary<string, string>, IHaveExtraData, IChildResource
    {
        /// <summary>
        /// The associated image.
        /// </summary>
        [JsonIgnore]
        protected ImageReference Image { get; set; }

        /// <summary />
        [JsonExtensionData]
        IDictionary<string, JToken> IHaveExtraData.Data { get; set; } = new Dictionary<string, JToken>();

        object IServiceResource.Owner { get; set; }

        /// <summary />
        protected internal void SetParent(ImageReference parent)
        {
            Image = parent;
        }

        void IChildResource.SetParent(string parentId)
        {
            SetParent(new ImageReference { Id = parentId});
        }

        void IChildResource.SetParent(object parent)
        {
            SetParent((ImageReference)parent);
        }

        /// <summary />
        protected void AssertParentIsSet([CallerMemberName]string callerName = "")
        {
            if (Image != null)
                return;

            throw new InvalidOperationException(string.Format($"{callerName} can only be used on instances which were constructed by the ComputeService. Use ComputeService.{callerName} instead."));
        }

        /// <inheritdoc cref="ComputeApi.CreateImageMetadataAsync" />
        public async Task CreateAsync(string key, string value, CancellationToken cancellationToken = default(CancellationToken))
        {
            AssertParentIsSet();
            var compute = this.GetOwnerOrThrow<ComputeApi>();
            await compute.CreateImageMetadataAsync(Image.Id, key, value, cancellationToken).ConfigureAwait(false);
            this[key] = value;
        }

        /// <inheritdoc cref="ComputeApi.UpdateImageMetadataAsync{T}" />
        public async Task UpdateAsync(bool overwrite = false, CancellationToken cancellationToken = default(CancellationToken))
        {
            AssertParentIsSet();
            var compute = this.GetOwnerOrThrow<ComputeApi>();
            var results = await compute.UpdateImageMetadataAsync<ImageMetadata>(Image.Id, this, overwrite, cancellationToken).ConfigureAwait(false);
            Clear();
            foreach (var result in results)
            {
                Add(result.Key, result.Value);
            }
        }

        /// <inheritdoc cref="ComputeApi.DeleteImageMetadataAsync" />
        public async Task DeleteAsync(string key, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!ContainsKey(key))
                return;

            AssertParentIsSet();
            var compute = this.GetOwnerOrThrow<ComputeApi>();
            await compute.DeleteImageMetadataAsync(Image.Id, key, cancellationToken).ConfigureAwait(false);
            Remove(key);
        }
    }
}