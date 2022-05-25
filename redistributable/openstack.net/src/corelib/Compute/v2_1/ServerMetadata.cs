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
    /// Key value pairs associated with a server instance.
    /// </summary>
    [JsonConverterWithConstructor(typeof (RootWrapperConverter), "metadata")]
    public class ServerMetadata : Dictionary<string, string>, IHaveExtraData, IChildResource
    {
        /// <summary>
        /// The associated server.
        /// </summary>
        [JsonIgnore]
        protected ServerReference Server { get; set; }

        [JsonExtensionData]
        IDictionary<string, JToken> IHaveExtraData.Data { get; set; } = new Dictionary<string, JToken>();

        object IServiceResource.Owner { get; set; }

        /// <summary />
        protected internal void SetParent(ServerReference parent)
        {
            Server = parent;
        }

        void IChildResource.SetParent(string parentId)
        {
            SetParent(new ServerReference {Id = parentId});
        }

        void IChildResource.SetParent(object parent)
        {
            SetParent((ServerReference)parent);
        }

        /// <summary />
        protected void AssertParentIsSet([CallerMemberName]string callerName = "")
        {
            if (Server != null)
                return;

            throw new InvalidOperationException(string.Format($"{callerName} can only be used on instances which were constructed by the ComputeService. Use ComputeService.{callerName} instead."));
        }

        /// <inheritdoc cref="ComputeApi.CreateServerMetadataAsync" />
        public async Task CreateAsync(string key, string value, CancellationToken cancellationToken = default(CancellationToken))
        {
            AssertParentIsSet();
            var compute = this.GetOwnerOrThrow<ComputeApi>();
            await compute.CreateServerMetadataAsync(Server.Id, key, value, cancellationToken).ConfigureAwait(false);
            this[key] = value;
        }

        /// <inheritdoc cref="ComputeApi.UpdateServerMetadataAsync{T}" />
        public async Task UpdateAsync(bool overwrite = false, CancellationToken cancellationToken = default(CancellationToken))
        {
            AssertParentIsSet();
            var compute = this.GetOwnerOrThrow<ComputeApi>();
            var results = await compute.UpdateServerMetadataAsync<ServerMetadata>(Server.Id, this, overwrite, cancellationToken).ConfigureAwait(false);
            Clear();
            foreach (var result in results)
            {
                Add(result.Key, result.Value);
            }
        }

        /// <inheritdoc cref="ComputeApi.DeleteServerMetadataAsync" />
        public async Task DeleteAsync(string key, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!ContainsKey(key))
                return;

            AssertParentIsSet();
            var compute = this.GetOwnerOrThrow<ComputeApi>();
            await compute.DeleteServerMetadataAsync(Server.Id, key, cancellationToken).ConfigureAwait(false);
            Remove(key);
        }
    }
}