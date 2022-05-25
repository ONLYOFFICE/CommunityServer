using System;
using System.Threading;
using System.Threading.Tasks;
using OpenStack.Compute.v2_1.Serialization;
using OpenStack.Serialization;

namespace OpenStack.Compute.v2_1.Operator
{
    /// <summary>
    /// Provides operator extention methods for a <see cref="Server"/> instance.
    /// </summary>
    public static class ServerExtensions
    {
        /// <inheritdoc cref="ComputeApi.EvacuateServerAsync" />
        /// <summary />
        /// <exception cref="InvalidOperationException">When this instance was not constructed by the <see cref="ComputeService"/>, as it is missing the appropriate internal state to execute service calls.</exception>
        public static async Task EvacuateAsync(this ServerReference server, EvacuateServerRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            var compute = server.GetOwnerOrThrow<ComputeApi>();
            await compute.EvacuateServerAsync(server.Id, request, cancellationToken).ConfigureAwait(false);
        }
    }
}
