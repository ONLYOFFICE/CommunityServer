using System.Threading;
using System.Threading.Tasks;
using OpenStack.Compute.v2_1.Serialization;

namespace OpenStack.Compute.v2_1.Operator
{
    /// <summary>
    /// Provides operator extention methods for a <see cref="ComputeService"/> instance.
    /// </summary>
    public static class ComputeServiceExtensions_v2_1
    {
        #region Servers

        /// <inheritdoc cref="ComputeApi.EvacuateServerAsync" />
        public static Task EvacuateServerAsync(this ComputeService service, Identifier serverId, EvacuateServerRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            return service._computeApi.EvacuateServerAsync(serverId, request, cancellationToken);
        }

        #endregion

        #region Compute Service
        /// <inheritdoc cref="ComputeApi.GetCurrentQuotasAsync{T}" />
        public static Task<ServiceQuotas> GetCurrentQuotasAsync(this ComputeService service, CancellationToken cancellationToken = default(CancellationToken))
        {
            return service._computeApi.GetCurrentQuotasAsync<ServiceQuotas>(cancellationToken);
        }

        /// <inheritdoc cref="ComputeApi.GetDefaultQuotasAsync{T}" />
        public static Task<ServiceQuotas> GetDefaultQuotasAsync(this ComputeService service, CancellationToken cancellationToken = default(CancellationToken))
        {
            return service._computeApi.GetDefaultQuotasAsync<ServiceQuotas>(cancellationToken);
        }
        #endregion
    }
}