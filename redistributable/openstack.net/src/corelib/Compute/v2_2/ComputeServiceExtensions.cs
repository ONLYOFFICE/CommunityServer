using OpenStack.Compute.v2_2;
using OpenStack.Synchronous.Extensions;

// ReSharper disable once CheckNamespace
namespace OpenStack.Synchronous
{
    /// <summary>
    /// Provides synchronous extention methods for a <see cref="ComputeService"/> instance.
    /// </summary>
    public static class ComputeServiceExtensions_v2_2
    {
        /// <inheritdoc cref="OpenStack.Compute.v2_1.ComputeService.GetVncConsoleAync" />
        public static RemoteConsole GetVncConsole(this ComputeService service, Identifier serverId, RemoteConsoleType type)
        {
            return service.GetVncConsoleAync(serverId, type).ForceSynchronous();
        }

        /// <inheritdoc cref="OpenStack.Compute.v2_1.ComputeService.ImportKeyPairAsync" />
        public static KeyPair ImportKeyPair(this ComputeService service, KeyPairDefinition keypair)
        {
            return service.ImportKeyPairAsync(keypair).ForceSynchronous();
        }
    }
}
