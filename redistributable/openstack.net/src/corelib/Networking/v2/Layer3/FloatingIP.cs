using System;
using System.Collections.Generic;
using System.Extensions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenStack.Serialization;

namespace OpenStack.Networking.v2.Layer3
{
    /// <summary>
    /// An external IP address that you map to a port in an internal network
    /// </summary>
    [JsonConverterWithConstructor(typeof(RootWrapperConverter), "floatingip")]
    public class FloatingIP : IHaveExtraData, IServiceResource
    {
        /// <summary>
        /// The floating IP identifier.
        /// </summary>
        [JsonProperty("id")]
        public Identifier Id { get; set; }

        /// <summary>
        /// The associated router.
        /// </summary>
        [JsonProperty("router_id")]
        public Identifier RouterId { get; set; }

        /// <summary>
        /// The network associated with the floating IP. 
        /// </summary>
        [JsonProperty("floating_network_id")]
        public Identifier NetworkId { get; set; }

        /// <summary>
        /// The fixed IP address that is associated with the floating IP address. 
        /// </summary>
        [JsonProperty("fixed_ip_address")]
        public string FixedIPAddress { get; set; }

        /// <summary>
        /// The floating IP address.
        /// </summary>
        [JsonProperty("floating_ip_address")]
        public string FloatingIPAddress { get; set; }

        /// <summary>
        /// The associated port.
        /// </summary>
        [JsonProperty("port_id")]
        public Identifier PortId { get; set; }

        /// <summary>
        /// The status of the floating IP address.
        /// </summary>
        [JsonProperty("status")]
        public FloatingIPStatus Status { get; set; }

        [JsonExtensionData]
        IDictionary<string, JToken> IHaveExtraData.Data { get; set; } = new Dictionary<string, JToken>();

        object IServiceResource.Owner { get; set; }

        /// <inheritdoc cref="NetworkingApiBuilder.DeleteFloatingIPAsync"/>
        /// <exception cref="InvalidOperationException">Thrown when a resource as not constructed by the SDK.</exception>
        public Task DeleteAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var networking = this.GetOwnerOrThrow<NetworkingApiBuilder>();
            return networking.DeleteFloatingIPAsync(Id, cancellationToken);
        }

        /// <summary>
        /// Associates the floating IP withe the specified port.
        /// </summary>
        /// <param name="portId">The port identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="InvalidOperationException">Thrown when a resource as not constructed by the SDK.</exception>
        public async Task AssociateAsync(Identifier portId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var networking = this.GetOwnerOrThrow<NetworkingApiBuilder>();
            var request = new FloatingIPUpdateDefinition { PortId = portId };
            var result = await networking.UpdateFloatingIPAsync<FloatingIP>(Id, request, cancellationToken).ConfigureAwait(false);
            result.CopyProperties(this);
        }

        /// <summary>
        /// Frees the floating IP from any associations.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="InvalidOperationException">Thrown when a resource as not constructed by the SDK.</exception>
        public async Task DisassociateAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var networking = this.GetOwnerOrThrow<NetworkingApiBuilder>();
            var request = new FloatingIPUpdateDefinition { PortId = null };
            var result = await networking.UpdateFloatingIPAsync<FloatingIP>(Id, request, cancellationToken).ConfigureAwait(false);
            result.CopyProperties(this);
        }
    }
}