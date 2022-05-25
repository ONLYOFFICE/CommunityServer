using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenStack.Networking.v2.Serialization;
using OpenStack.Serialization;

namespace OpenStack.Networking.v2.Layer3
{
    /// <summary>
    /// A logical entity for forwarding packets across internal subnets and NATting them on external networks through an appropriate external gateway.
    /// </summary>
    [JsonConverterWithConstructor(typeof(RootWrapperConverter), "router")]
    public class Router : IHaveExtraData, IServiceResource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Router"/> class.
        /// </summary>
        public Router()
        {
            Routes = new List<HostRoute>();
        }

        /// <summary>
        /// The router identifier.
        /// </summary>
        [JsonProperty("id")]
        public Identifier Id { get; set; }

        /// <summary>
        /// The router name. 
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// The network status. 
        /// </summary>
        [JsonProperty("status")]
        public RouterStatus Status { get; set; }

        /// <summary>
        /// The external gateway connection information.
        /// </summary>
        [JsonProperty("external_gateway_info")]
        public ExternalGateway ExternalGateway { get; set; }

        /// <summary>
        /// The administrative state of the router.
        /// </summary>
        [JsonProperty("admin_state_up")]
        public bool IsUp { get; set; }

        /// <summary>
        /// The extra routes configuration for L3 router.
        /// </summary>
        [JsonProperty("routes")]
        public IList<HostRoute> Routes { get; set; }

        [JsonExtensionData]
        IDictionary<string, JToken> IHaveExtraData.Data { get; set; } = new Dictionary<string, JToken>();

        object IServiceResource.Owner { get; set; }

        /// <summary>
        /// Deletes the router and any attached interfaces.
        /// </summary>
        /// <inheritdoc cref="NetworkingApiBuilder.DeleteRouterAsync"/>
        /// <exception cref="InvalidOperationException">Thrown when a resource as not constructed by the SDK.</exception>
        public async Task DeleteAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var networking = this.GetOwnerOrThrow<NetworkingApiBuilder>();

            // Remove attached interfaces, these aren't really tracked by the end-user but will prevent deleting the router
            var filterByRouter = new PortListOptions {DeviceId = Id};
            var attachedPorts = await networking.ListPortsAsync<PortCollection>(filterByRouter, cancellationToken);
            Task.WaitAll(attachedPorts.Select(port => DetachPortAsync(port.Id, cancellationToken)).ToArray());

            await networking.DeleteRouterAsync(Id, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc cref="NetworkingApiBuilder.AttachPortToRouterAsync"/>
        /// <exception cref="InvalidOperationException">Thrown when a resource as not constructed by the SDK.</exception>
        public Task AttachPortAsync(Identifier portId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var networking = this.GetOwnerOrThrow<NetworkingApiBuilder>();
            return networking.AttachPortToRouterAsync(Id, portId, cancellationToken);
        }

        /// <inheritdoc cref="NetworkingApiBuilder.AttachSubnetToRouterAsync"/>
        /// <exception cref="InvalidOperationException">Thrown when a resource as not constructed by the SDK.</exception>
        public async Task<Identifier> AttachSubnetAsync(Identifier subnetId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var networking = this.GetOwnerOrThrow<NetworkingApiBuilder>();
            return await networking.AttachSubnetToRouterAsync(Id, subnetId, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc cref="NetworkingApiBuilder.DetachPortFromRouterAsync"/>
        /// <exception cref="InvalidOperationException">Thrown when a resource as not constructed by the SDK.</exception>
        public Task DetachPortAsync(Identifier portId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var networking = this.GetOwnerOrThrow<NetworkingApiBuilder>();
            return networking.DetachPortFromRouterAsync(Id, portId, cancellationToken);
        }

        /// <inheritdoc cref="NetworkingApiBuilder.DetachSubnetFromRouterAsync"/>
        /// <exception cref="InvalidOperationException">Thrown when a resource as not constructed by the SDK.</exception>
        public Task DetachSubnetAsync(Identifier subnetId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var networking = this.GetOwnerOrThrow<NetworkingApiBuilder>();
            return networking.DetachSubnetFromRouterAsync(Id, subnetId, cancellationToken);
        }
    }
}