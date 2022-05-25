using System.Threading;
using System.Threading.Tasks;

using OpenStack.Authentication;
using OpenStack.Compute.v2_2.Serialization;

namespace OpenStack.Compute.v2_2
{
    /// <summary />
    public class ComputeService
    {
        private readonly ComputeApi _computeApi;

        /// <summary />
        public ComputeService(IAuthenticationProvider authenticationProvider, string region, bool useInternalUrl)
        {
            _computeApi = new ComputeApi(ServiceType.Compute, authenticationProvider, region, useInternalUrl);
        }

        #region Servers
        /// <inheritdoc cref="v2_1.Serialization.ComputeApi.ListServerSummariesAsync{TPage}" />
        public virtual async Task<IPage<ServerReference>> ListServerReferencesAsync(ServerListOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await _computeApi.ListServerSummariesAsync<ServerCollection>(options, cancellationToken);
        }

        /// <summary />
        public virtual Task<RemoteConsole> GetVncConsoleAync(Identifier serverId, RemoteConsoleType type, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _computeApi.GetVncConsoleAsync<RemoteConsole>(serverId, type, cancellationToken);
        }
        #endregion

        #region Keypairs

        /// <summary />
        public virtual Task<KeyPair> ImportKeyPairAsync(KeyPairDefinition keypair, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _computeApi.CreateKeyPairAsync<KeyPair>(keypair, cancellationToken);
        }

        #endregion
    }
}
