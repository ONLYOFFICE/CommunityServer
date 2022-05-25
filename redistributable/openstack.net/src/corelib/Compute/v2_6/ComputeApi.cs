using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Extensions;
using Flurl.Http;
using OpenStack.Authentication;

namespace OpenStack.Compute.v2_6
{
    /// <inheritdoc />
    /// <seealso href="https://github.com/openstack/nova/blob/master/nova/api/openstack/rest_api_version_history.rst#26">Compute Microversion 2.6</seealso>
    public class ComputeApi : v2_2.ComputeApi
    {
        /// <inheritdoc />
        public ComputeApi(IServiceType serviceType, IAuthenticationProvider authenticationProvider, string region, bool useInternalUrl)
            : this(serviceType, authenticationProvider, region, useInternalUrl, "2.6")
        { }

        /// <inheritdoc />
        protected ComputeApi(IServiceType serviceType, IAuthenticationProvider authenticationProvider, string region, bool useInternalUrl, string microversion)
            : base(serviceType, authenticationProvider, region, useInternalUrl, microversion)
        { }

#pragma warning disable 809
        /// <inheritdoc />
        /// <exception cref="NotSupportedException"></exception>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("GetVncConsole was deprecated in v2.6. Use GetConsole instead.", true)]
        public override Task<T> GetVncConsoleAsync<T>(string serverId, object type, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotSupportedException("GetVncConsole was deprecated in v2.6. Use GetConsole instead.");
        }
#pragma warning restore 809

        /// <summary />
        public virtual Task<T> GetConsoleAsync<T>(string serverId, object protocol, object type, CancellationToken cancellationToken = default(CancellationToken))
        {
            return BuildGetConsoleRequest(serverId, protocol, type, cancellationToken).SendAsync().ReceiveJson<T>();
        }

        /// <summary />
        public virtual async Task<PreparedRequest> BuildGetConsoleRequest(string serverId, object protocol, object type, CancellationToken cancellationToken = default(CancellationToken))
        {
            var body = new { remote_console = new { protocol, type } };

            PreparedRequest request = await Endpoint.PrepareRequest($"servers/{serverId}/remote-consoles", cancellationToken);
            return request.PreparePostJson(body, cancellationToken);
        }
    }
}
