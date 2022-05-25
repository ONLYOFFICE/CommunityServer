using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using OpenStack.Networking.v2.Serialization;
using OpenStack.Synchronous.Extensions;

namespace OpenStack.Networking.v2.Layer3
{
    /// <summary>
    /// Exposes functionality from the Level3 networking extension
    /// </summary>
    /// <seealso href="http://developer.openstack.org/api-ref-networking-v2-ext.html#layer3-ext"/>
    public static class NetworkingService_Layer3_Extensions
    {
        #region Routers
        /// <inheritdoc cref="NetworkingApiBuilder.GetRouterAsync{T}" />
        public static Task<Router> GetRouterAsync(this NetworkingService service, Identifier routerId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return service._networkingApiBuilder.GetRouterAsync<Router>(routerId, cancellationToken);
        }

        /// <inheritdoc cref="NetworkingApiBuilder.CreateRouterAsync{T}" />
        public static Task<Router> CreateRouterAsync(this NetworkingService service, RouterCreateDefinition router, CancellationToken cancellationToken = default(CancellationToken))
        {
            return service._networkingApiBuilder.CreateRouterAsync<Router>(router, cancellationToken);
        }

        /// <inheritdoc cref="NetworkingApiBuilder.UpdateRouterAsync{T}" />
        public static Task<Router> UpdateRouterAsync(this NetworkingService service, RouterUpdateDefinition router, CancellationToken cancellationToken = default(CancellationToken))
        {
            return service._networkingApiBuilder.CreateRouterAsync<Router>(router, cancellationToken);
        }

        /// <inheritdoc cref="NetworkingApiBuilder.ListRoutersAsync{T}" />
        public static async Task<IEnumerable<Router>> ListRoutersAsync(this NetworkingService service, RouterListOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await service._networkingApiBuilder.ListRoutersAsync<RouterCollection>(options, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc cref="NetworkingApiBuilder.DeleteRouterAsync" />
        public static Task DeleteRouterAsync(this NetworkingService service, Identifier routerId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return service._networkingApiBuilder.DeleteRouterAsync(routerId, cancellationToken);
        }

        /// <inheritdoc cref="NetworkingApiBuilder.AttachPortToRouterAsync" />
        public static Task AttachPortToRouterAsync(this NetworkingService service, Identifier routerId, Identifier portId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return service._networkingApiBuilder.AttachPortToRouterAsync(routerId, portId, cancellationToken);
        }

        /// <inheritdoc cref="NetworkingApiBuilder.AttachSubnetToRouterAsync" />
        public static async Task<Identifier> AttachSubnetToRouterAsync(this NetworkingService service, Identifier routerId, Identifier subnetId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await service._networkingApiBuilder.AttachSubnetToRouterAsync(routerId, subnetId, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc cref="NetworkingApiBuilder.DetachPortFromRouterAsync" />
        public static Task DetachPortFromRouterAsync(this NetworkingService service, Identifier routerId, Identifier portId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return service._networkingApiBuilder.DetachPortFromRouterAsync(routerId, portId, cancellationToken);
        }

        /// <inheritdoc cref="NetworkingApiBuilder.DetachSubnetFromRouterAsync" />
        public static Task DetachSubnetFromRouterAsync(this NetworkingService service, Identifier routerId, Identifier subnetId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return service._networkingApiBuilder.DetachSubnetFromRouterAsync(routerId, subnetId, cancellationToken);
        }
        #endregion

        #region Floating IPs
        /// <inheritdoc cref="NetworkingApiBuilder.GetFloatingIPAsync{T}" />
        public static Task<FloatingIP> GetFloatingIPAsync(this NetworkingService service, Identifier floatingIPId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return service._networkingApiBuilder.GetFloatingIPAsync<FloatingIP>(floatingIPId, cancellationToken);
        }

        /// <inheritdoc cref="NetworkingApiBuilder.CreateFloatingIPAsync{T}" />
        public static Task<FloatingIP> CreateFloatingIPAsync(this NetworkingService service, FloatingIPCreateDefinition floatingIP, CancellationToken cancellationToken = default(CancellationToken))
        {
            return service._networkingApiBuilder.CreateFloatingIPAsync<FloatingIP>(floatingIP, cancellationToken);
        }

        /// <inheritdoc cref="NetworkingApiBuilder.UpdateFloatingIPAsync{T}" />
        public static Task<FloatingIP> UpdateFloatingIPAsync(this NetworkingService service, FloatingIPUpdateDefinition floatingIP, CancellationToken cancellationToken = default(CancellationToken))
        {
            return service._networkingApiBuilder.CreateFloatingIPAsync<FloatingIP>(floatingIP, cancellationToken);
        }

        /// <inheritdoc cref="NetworkingApiBuilder.ListFloatingIPsAsync{T}" />
        public static async Task<IEnumerable<FloatingIP>> ListFloatingIPsAsync(this NetworkingService service, FloatingIPListOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await service._networkingApiBuilder.ListFloatingIPsAsync<FloatingIPCollection>(options, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc cref="NetworkingApiBuilder.DeleteFloatingIPAsync" />
        public static Task DeleteFloatingIPAsync(this NetworkingService service, Identifier floatingIPId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return service._networkingApiBuilder.DeleteFloatingIPAsync(floatingIPId, cancellationToken);
        }
        #endregion

        #region Security Groups
        /// <inheritdoc cref="NetworkingApiBuilder.ListSecurityGroupsAsync{T}" />
        public static async Task<IEnumerable<SecurityGroup>> ListSecurityGroupsAsync(this NetworkingService service, SecurityGroupListOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await service._networkingApiBuilder.ListSecurityGroupsAsync<SecurityGroupCollection>(options, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc cref="NetworkingApiBuilder.ListSecurityGroupRulesAsync{T}" />
        public static async Task<IEnumerable<SecurityGroupRule>> ListSecurityGroupRulesAsync(this NetworkingService service, SecurityGroupRuleListOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await service._networkingApiBuilder.ListSecurityGroupRulesAsync<SecurityGroupRuleCollection>(options, cancellationToken).ConfigureAwait(false);
        }
        #endregion
    }
}

namespace OpenStack.Networking.v2.Layer3.Synchronous
{
    /// <summary>
    /// Exposes synchronous extension methods for the Level3 networking extension
    /// </summary>
    /// <seealso href="http://developer.openstack.org/api-ref-networking-v2-ext.html#layer3-ext"/>
    public static class NetworkingService_Layer3_Synchronous_Extensions
    {
        #region Routers
        /// <inheritdoc cref="NetworkingService_Layer3_Extensions.GetRouterAsync" />
        public static Router GetRouter(this NetworkingService service, Identifier routerId)
        {
            return service._networkingApiBuilder.GetRouterAsync<Router>(routerId).ForceSynchronous();
        }

        /// <inheritdoc cref="NetworkingService_Layer3_Extensions.CreateRouterAsync" />
        public static Router CreateRouter(this NetworkingService service, RouterCreateDefinition router)
        {
            return service._networkingApiBuilder.CreateRouterAsync<Router>(router).ForceSynchronous();
        }

        /// <inheritdoc cref="NetworkingService_Layer3_Extensions.UpdateRouterAsync" />
        public static Router UpdateRouter(this NetworkingService service, RouterUpdateDefinition router)
        {
            return service._networkingApiBuilder.CreateRouterAsync<Router>(router).ForceSynchronous();
        }

        /// <inheritdoc cref="NetworkingService_Layer3_Extensions.ListRoutersAsync" />
        public static IEnumerable<Router> ListRouters(this NetworkingService service, RouterListOptions options = null)
        {
            return service._networkingApiBuilder.ListRoutersAsync<RouterCollection>(options).ForceSynchronous();
        }

        /// <inheritdoc cref="NetworkingService_Layer3_Extensions.DeleteRouterAsync" />
        public static void DeleteRouter(this NetworkingService service, Identifier routerId)
        {
            service._networkingApiBuilder.DeleteRouterAsync(routerId).ForceSynchronous();
        }

        /// <inheritdoc cref="NetworkingService_Layer3_Extensions.AttachPortToRouterAsync" />
        public static void AttachPortToRouter(this NetworkingService service, Identifier routerId, Identifier portId)
        {
            service._networkingApiBuilder.AttachPortToRouterAsync(routerId, portId).ForceSynchronous();
        }

        /// <inheritdoc cref="NetworkingService_Layer3_Extensions.AttachSubnetToRouterAsync" />
        public static Identifier AttachSubnetToRouter(this NetworkingService service, Identifier routerId, Identifier subnetId)
        {
            return service._networkingApiBuilder.AttachSubnetToRouterAsync(routerId, subnetId).ForceSynchronous();
        }

        /// <inheritdoc cref="NetworkingService_Layer3_Extensions.DetachPortFromRouterAsync" />
        public static void DetachPortFromRouter(this NetworkingService service, Identifier routerId, Identifier portId)
        {
            service._networkingApiBuilder.DetachPortFromRouterAsync(routerId, portId).ForceSynchronous();
        }

        /// <inheritdoc cref="NetworkingService_Layer3_Extensions.DetachSubnetFromRouterAsync" />
        public static void DetachSubnetFromRouter(this NetworkingService service, Identifier routerId, Identifier subnetId)
        {
            service._networkingApiBuilder.DetachSubnetFromRouterAsync(routerId, subnetId).ForceSynchronous();
        }
        #endregion

        #region Floating IPs
        /// <inheritdoc cref="NetworkingService_Layer3_Extensions.GetFloatingIPAsync" />
        public static FloatingIP GetFloatingIP(this NetworkingService service, Identifier floatingIPId)
        {
            return service._networkingApiBuilder.GetFloatingIPAsync<FloatingIP>(floatingIPId).ForceSynchronous();
        }

        /// <inheritdoc cref="NetworkingService_Layer3_Extensions.CreateFloatingIPAsync" />
        public static FloatingIP CreateFloatingIP(this NetworkingService service, FloatingIPCreateDefinition floatingIP)
        {
            return service._networkingApiBuilder.CreateFloatingIPAsync<FloatingIP>(floatingIP).ForceSynchronous();
        }

        /// <inheritdoc cref="NetworkingService_Layer3_Extensions.UpdateFloatingIPAsync" />
        public static FloatingIP UpdateFloatingIP(this NetworkingService service, FloatingIPUpdateDefinition floatingIP)
        {
            return service._networkingApiBuilder.CreateFloatingIPAsync<FloatingIP>(floatingIP).ForceSynchronous();
        }

        /// <inheritdoc cref="NetworkingService_Layer3_Extensions.ListFloatingIPsAsync" />
        public static IEnumerable<FloatingIP> ListFloatingIPs(this NetworkingService service, FloatingIPListOptions options = null)
        {
            return service._networkingApiBuilder.ListFloatingIPsAsync<FloatingIPCollection>(options).ForceSynchronous();
        }

        /// <inheritdoc cref="NetworkingService_Layer3_Extensions.DeleteFloatingIPAsync" />
        public static void DeleteFloatingIP(this NetworkingService service, Identifier floatingIPId)
        {
            service._networkingApiBuilder.DeleteFloatingIPAsync(floatingIPId).ForceSynchronous();
        }
        #endregion

        #region Security Groups
        /// <inheritdoc cref="NetworkingService_Layer3_Extensions.ListSecurityGroupsAsync" />
        public static IEnumerable<SecurityGroup> ListSecurityGroups(this NetworkingService service, SecurityGroupListOptions options = null)
        {
            return service.ListSecurityGroupsAsync(options).ForceSynchronous();
        }
        /// <inheritdoc cref="NetworkingService_Layer3_Extensions.ListSecurityGroupRulesAsync" />
        public static IEnumerable<SecurityGroupRule> ListSecurityGroupRules(this NetworkingService service, SecurityGroupRuleListOptions options = null)
        {
            return service.ListSecurityGroupRulesAsync(options).ForceSynchronous();
        }
        #endregion

    }
}
