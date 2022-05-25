using OpenStack.Compute.v2_6;
using OpenStack.Synchronous.Extensions;

// ReSharper disable once CheckNamespace
namespace OpenStack.Synchronous
{
    /// <summary>
    /// Provides synchronous extention methods for a <see cref="ComputeService"/> instance.
    /// </summary>
    public static class ComputeServiceExtensions_v2_6
    {
        /// <inheritdoc cref="ComputeService.GetConsoleAsync" />
        public static Console GetConsole(this ComputeService service, Identifier serverId, ConsoleProtocol protocol, RemoteConsoleType type)
        {
            return service.GetConsoleAsync(serverId, protocol, type).ForceSynchronous();
        }
    }
}
