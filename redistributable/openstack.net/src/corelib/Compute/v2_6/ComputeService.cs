using System.Threading;
using System.Threading.Tasks;
using OpenStack.Authentication;
using OpenStack.Compute.v2_6.Serialization;

namespace OpenStack.Compute.v2_6
{
    /// <inheritdoc />
    public class ComputeService
    {
        private readonly ComputeApi _computeApi;

        /// <summary />
        public ComputeService(IAuthenticationProvider authenticationProvider, string region, bool useInternalUrl = false)
        {
            _computeApi = new ComputeApi(ServiceType.Compute, authenticationProvider, region, useInternalUrl);
        }

        #region Servers

        /// <summary />
        public virtual async Task<IPage<ServerReference>> ListServerReferencesAsync(ServerListOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await _computeApi.ListServerSummariesAsync<ServerCollection>(options, cancellationToken);
        }

        /// <summary />
        public virtual Task<Console> GetConsoleAsync(Identifier serverId, ConsoleProtocol protocol, RemoteConsoleType type, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _computeApi.GetConsoleAsync<Console>(serverId, protocol, type, cancellationToken);
        }
        #endregion

        #region Keypairs

        /// <summary />
        public virtual Task<KeyPair> CreateKeyPairAsync(string name, KeyPairType? type = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var keyPair = new KeyPairDefinition(name)
            {
                Type = type
            };
            return _computeApi.CreateKeyPairAsync<KeyPair>(keyPair, cancellationToken);
        }

        #endregion
    }
}
