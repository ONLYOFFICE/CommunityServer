using System;
using System.Collections.Generic;

using OpenStack.Synchronous.Extensions;

using Rackspace.RackConnect.v3;

// ReSharper disable once CheckNamespace
namespace Rackspace.Synchronous
{
    /// <summary>
    /// Provides synchronous extention methods for the <see cref="RackConnectService"/>.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    public static class RackConnectServiceExtensions
    {
        #region Networks
        /// <inheritdoc cref="RackConnectService.ListNetworksAsync"/>
        public static IEnumerable<NetworkReference> ListNetworks(this RackConnectService rackConnectService)
        {
            return rackConnectService.ListNetworksAsync().ForceSynchronous();
        }

        /// <inheritdoc cref="RackConnectService.GetNetworkAsync"/>
        public static NetworkReference GetNetwork(this RackConnectService rackConnectService, Identifier networkId)
        {
            return rackConnectService.GetNetworkAsync(networkId).ForceSynchronous();
        }
        #endregion

        #region Public IPs
        /// <inheritdoc cref="RackConnectService.ListPublicIPsAsync"/>
        public static IEnumerable<PublicIP> ListPublicIPs(this RackConnectService rackConnectService, ListPublicIPsFilter filter = null)
        {
            return rackConnectService.ListPublicIPsAsync(filter).ForceSynchronous();
        }

        /// <inheritdoc cref="RackConnectService.GetPublicIPAsync"/>
        public static PublicIP GetPublicIP(this RackConnectService rackConnectService, Identifier publicIPId)
        {
            return rackConnectService.GetPublicIPAsync(publicIPId).ForceSynchronous();
        }

        /// <inheritdoc cref="RackConnectService.CreatePublicIPAsync"/>
        public static PublicIP CreatePublicIP(this RackConnectService rackConnectService, PublicIPCreateDefinition definition)
        {
            return rackConnectService.CreatePublicIPAsync(definition).ForceSynchronous();
        }

        /// <inheritdoc cref="RackConnectService.WaitUntilPublicIPIsActiveAsync"/>
        public static PublicIP WaitUntilPublicIPIsActive(this RackConnectService rackConnectService, Identifier publicIPId, TimeSpan? refreshDelay = null, TimeSpan? timeout = null, IProgress<bool> progress = null)
        {
            return rackConnectService.WaitUntilPublicIPIsActiveAsync(publicIPId, refreshDelay, timeout, progress).ForceSynchronous();
        }

        /// <inheritdoc cref="RackConnectService.WaitUntilPublicIPIsDeletedAsync"/>
        public static void WaitUntilPublicIPIsDeleted(this RackConnectService rackConnectService, Identifier publicIPId, TimeSpan? refreshDelay = null, TimeSpan? timeout = null, IProgress<bool> progress = null)
        {
            rackConnectService.WaitUntilPublicIPIsDeletedAsync(publicIPId, refreshDelay, timeout, progress).ForceSynchronous();
        }

        /// <inheritdoc cref="RackConnectService.UpdatePublicIPAsync"/>
        public static PublicIP UpdatePublicIP(this RackConnectService rackConnectService, Identifier publicIPId, PublicIPUpdateDefinition definition)
        {
            return rackConnectService.UpdatePublicIPAsync(publicIPId, definition).ForceSynchronous();
        }

        /// <inheritdoc cref="RackConnectService.DeletePublicIPAsync"/>
        public static void DeletePublicIP(this RackConnectService rackConnectService, Identifier publicIPId)
        {
            rackConnectService.DeletePublicIPAsync(publicIPId).ForceSynchronous();
        }

        #endregion
    }
}