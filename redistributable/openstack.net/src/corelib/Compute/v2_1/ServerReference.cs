using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenStack.Compute.v2_1.Serialization;
using OpenStack.Serialization;

namespace OpenStack.Compute.v2_1
{
    /// <summary>
    /// Reference to a server instance.
    /// </summary>
    public class ServerReference : IHaveExtraData, IServiceResource
    {
        /// <summary>
        /// The server identifier.
        /// </summary>
        [JsonProperty("id")]
        public Identifier Id { get; set; }

        [JsonExtensionData]
        IDictionary<string, JToken> IHaveExtraData.Data { get; set; } = new Dictionary<string, JToken>();

        object IServiceResource.Owner { get; set; }

        /// <inheritdoc cref="ComputeApi.GetServerAsync{T}" />
        public async Task<Server> GetServerAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var compute = this.GetOwnerOrThrow<ComputeApi>();
            return await compute.GetServerAsync<Server>(Id, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc cref="ComputeApi.GetServerAddressAsync{T}" />
        /// <summary />
        public async Task<IList<ServerAddress>> GetAddressAsync(string key, CancellationToken cancellationToken = default(CancellationToken))
        {
            var compute = this.GetOwnerOrThrow<ComputeApi>();
            return await compute.GetServerAddressAsync<ServerAddress>(Id, key, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc cref="ComputeApi.GetServerMetadataAsync{T}" />
        /// <exception cref="InvalidOperationException">When this instance was not constructed by the <see cref="ComputeService"/>, as it is missing the appropriate internal state to execute service calls.</exception>
        public Task<ServerMetadata> GetMetadataAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var owner = this.GetOwnerOrThrow<ComputeApi>();
            return owner.GetServerMetadataAsync<ServerMetadata>(Id, cancellationToken);
        }

        /// <inheritdoc cref="ComputeApi.GetServerMetadataItemAsync" />
        /// <exception cref="InvalidOperationException">When this instance was not constructed by the <see cref="ComputeService"/>, as it is missing the appropriate internal state to execute service calls.</exception>
        public Task<string> GetMetadataItemAsync(string key, CancellationToken cancellationToken = default(CancellationToken))
        {
            var owner = this.GetOwnerOrThrow<ComputeApi>();
            return owner.GetServerMetadataItemAsync(Id, key, cancellationToken);
        }

        /// <inheritdoc cref="ComputeApi.ListServerAddressesAsync{T}" />
        public async Task<IDictionary<string, IList<ServerAddress>>> ListAddressesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var compute = this.GetOwnerOrThrow<ComputeApi>();
            return await compute.ListServerAddressesAsync<ServerAddressCollection>(Id, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc cref="ComputeApi.DeleteServerAsync" />
        /// <exception cref="InvalidOperationException">When this instance was not constructed by the <see cref="ComputeService"/>, as it is missing the appropriate internal state to execute service calls.</exception>
        public Task DeleteAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var owner = this.GetOwnerOrThrow<ComputeApi>();
            return owner.DeleteServerAsync(Id, cancellationToken);
        }

        /// <inheritdoc cref="ComputeApi.WaitUntilServerIsDeletedAsync{TServer,TStatus}" />
        /// <exception cref="InvalidOperationException">When the <see cref="Server"/> instance was not constructed by the <see cref="ComputeService"/>, as it is missing the appropriate internal state to execute service calls.</exception>
        public virtual async Task WaitUntilDeletedAsync(TimeSpan? refreshDelay = null, TimeSpan? timeout = null, IProgress<bool> progress = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var owner = this.GetOwnerOrThrow<ComputeApi>();
            await owner.WaitUntilServerIsDeletedAsync<Server, ServerStatus>(Id, null, refreshDelay, timeout, progress, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc cref="ComputeApi.SnapshotServerAsync{T}" />
        /// <exception cref="InvalidOperationException">When this instance was not constructed by the <see cref="ComputeService"/>, as it is missing the appropriate internal state to execute service calls.</exception>
        public async Task<Image> SnapshotAsync(SnapshotServerRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            var compute = this.GetOwnerOrThrow<ComputeApi>();
            return await compute.SnapshotServerAsync<Image>(Id, request, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc cref="ComputeApi.StartServerAsync" />
        /// <exception cref="InvalidOperationException">When this instance was not constructed by the <see cref="ComputeService"/>, as it is missing the appropriate internal state to execute service calls.</exception>
        public async Task StartAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var compute = this.GetOwnerOrThrow<ComputeApi>();
            await compute.StartServerAsync(Id, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc cref="ComputeApi.StopServerAsync" />
        /// <exception cref="InvalidOperationException">When this instance was not constructed by the <see cref="ComputeService"/>, as it is missing the appropriate internal state to execute service calls.</exception>
        public async Task StopAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var compute = this.GetOwnerOrThrow<ComputeApi>();
            await compute.StopServerAsync(Id, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc cref="ComputeApi.SuspendServerAsync" />
        /// <exception cref="InvalidOperationException">When this instance was not constructed by the <see cref="ComputeService"/>, as it is missing the appropriate internal state to execute service calls.</exception>
        public async Task SuspendAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var compute = this.GetOwnerOrThrow<ComputeApi>();
            await compute.SuspendServerAsync(Id, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc cref="ComputeApi.ResumeServerAsync" />
        /// <exception cref="InvalidOperationException">When this instance was not constructed by the <see cref="ComputeService"/>, as it is missing the appropriate internal state to execute service calls.</exception>
        public async Task ResumeAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var compute = this.GetOwnerOrThrow<ComputeApi>();
            await compute.ResumeServerAsync(Id, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc cref="ComputeApi.RebootServerAsync{T}" />
        /// <exception cref="InvalidOperationException">When this instance was not constructed by the <see cref="ComputeService"/>, as it is missing the appropriate internal state to execute service calls.</exception>
        public async Task RebootAsync(RebootServerRequest request = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var compute = this.GetOwnerOrThrow<ComputeApi>();
            await compute.RebootServerAsync(Id, request, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc cref="ComputeApi.ListServerVolumesAsync{T}" />
        /// <exception cref="InvalidOperationException">When this instance was not constructed by the <see cref="ComputeService"/>, as it is missing the appropriate internal state to execute service calls.</exception>
        public async Task<IEnumerable<ServerVolume>> ListVolumesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var compute = this.GetOwnerOrThrow<ComputeApi>();
            return await compute.ListServerVolumesAsync<ServerVolumeCollection>(Id, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc cref="ComputeApi.GetVncConsoleAsync{T}" />
        /// <exception cref="InvalidOperationException">When this instance was not constructed by the <see cref="ComputeService"/>, as it is missing the appropriate internal state to execute service calls.</exception>
        public async Task<RemoteConsole> GetVncConsoleAsync(RemoteConsoleType type, CancellationToken cancellationToken = default(CancellationToken))
        {
            var compute = this.GetOwnerOrThrow<ComputeApi>();
            return await compute.GetVncConsoleAsync<RemoteConsole>(Id, type, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc cref="ComputeApi.GetSpiceConsoleAsync{T}" />
        /// <exception cref="InvalidOperationException">When this instance was not constructed by the <see cref="ComputeService"/>, as it is missing the appropriate internal state to execute service calls.</exception>
        public async Task<RemoteConsole> GetSpiceConsoleAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var compute = this.GetOwnerOrThrow<ComputeApi>();
            return await compute.GetSpiceConsoleAsync<RemoteConsole>(Id,RemoteConsoleType.SpiceHtml5, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc cref="ComputeApi.GetSerialConsoleAsync{T}" />
        /// <exception cref="InvalidOperationException">When this instance was not constructed by the <see cref="ComputeService"/>, as it is missing the appropriate internal state to execute service calls.</exception>
        public async Task<RemoteConsole> GetSerialConsoleAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var compute = this.GetOwnerOrThrow<ComputeApi>();
            return await compute.GetSerialConsoleAsync<RemoteConsole>(Id, RemoteConsoleType.Serial, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc cref="ComputeApi.GetRdpConsoleAsync{T}" />
        /// <exception cref="InvalidOperationException">When this instance was not constructed by the <see cref="ComputeService"/>, as it is missing the appropriate internal state to execute service calls.</exception>
        public async Task<RemoteConsole> GetRdpConsoleAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var compute = this.GetOwnerOrThrow<ComputeApi>();
            return await compute.GetRdpConsoleAsync<RemoteConsole>(Id, RemoteConsoleType.RdpHtml5, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc cref="ComputeApi.GetConsoleOutputAsync" />
        /// <exception cref="InvalidOperationException">When this instance was not constructed by the <see cref="ComputeService"/>, as it is missing the appropriate internal state to execute service calls.</exception>
        public async Task<string> GetConsoleOutputAsync(int length = -1, CancellationToken cancellationToken = default(CancellationToken))
        {
            var compute = this.GetOwnerOrThrow<ComputeApi>();
            return await compute.GetConsoleOutputAsync(Id, length, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc cref="ComputeApi.RescueServerAsync" />
        /// <exception cref="InvalidOperationException">When this instance was not constructed by the <see cref="ComputeService"/>, as it is missing the appropriate internal state to execute service calls.</exception>
        public async Task<string> RescueAsync(RescueServerRequest request = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var compute = this.GetOwnerOrThrow<ComputeApi>();
            return await compute.RescueServerAsync(Id, request, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc cref="ComputeApi.UnrescueServerAsync" />
        /// <exception cref="InvalidOperationException">When this instance was not constructed by the <see cref="ComputeService"/>, as it is missing the appropriate internal state to execute service calls.</exception>
        public Task UnrescueAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var compute = this.GetOwnerOrThrow<ComputeApi>();
            return compute.UnrescueServerAsync(Id, cancellationToken);
        }

        /// <inheritdoc cref="ComputeApi.ResizeServerAsync" />
        /// <exception cref="InvalidOperationException">When this instance was not constructed by the <see cref="ComputeService"/>, as it is missing the appropriate internal state to execute service calls.</exception>
        public Task ResizeAsync(Identifier flavorId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var compute = this.GetOwnerOrThrow<ComputeApi>();
            return compute.ResizeServerAsync(Id, flavorId, cancellationToken);
        }

        /// <inheritdoc cref="ComputeApi.ConfirmResizeServerAsync" />
        /// <exception cref="InvalidOperationException">When this instance was not constructed by the <see cref="ComputeService"/>, as it is missing the appropriate internal state to execute service calls.</exception>
        public Task ConfirmResizeAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var compute = this.GetOwnerOrThrow<ComputeApi>();
            return compute.ConfirmResizeServerAsync(Id, cancellationToken);
        }

        /// <inheritdoc cref="ComputeApi.CancelResizeServerAsync" />
        /// <exception cref="InvalidOperationException">When this instance was not constructed by the <see cref="ComputeService"/>, as it is missing the appropriate internal state to execute service calls.</exception>
        public Task CancelResizeAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var compute = this.GetOwnerOrThrow<ComputeApi>();
            return compute.CancelResizeServerAsync(Id, cancellationToken);
        }

        /// <inheritdoc cref="ComputeApi.ListServerActionSummariesAsync{T}" />
        /// <exception cref="InvalidOperationException">When this instance was not constructed by the <see cref="ComputeService"/>, as it is missing the appropriate internal state to execute service calls.</exception>
        public async Task<IEnumerable<ServerActionSummary>> ListActionSummariesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var compute = this.GetOwnerOrThrow<ComputeApi>();
            return await compute.ListServerActionSummariesAsync<ServerActionSummaryCollection>(Id, cancellationToken).ConfigureAwait(false);
        }
    }
}