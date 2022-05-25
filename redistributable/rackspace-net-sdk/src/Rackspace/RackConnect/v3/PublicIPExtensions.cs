using System;
using OpenStack.Synchronous.Extensions;
using Rackspace.RackConnect.v3;

namespace Rackspace.Synchronous
{
    /// <summary>
    /// Provides synchronous extention methods for a <see cref="PublicIP"/> instance.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    public static class PublicIPExtensions
    {
        /// <inheritdoc cref="PublicIP.DeleteAsync"/>
        public static void Delete(this PublicIP publicIP)
        {
            publicIP.DeleteAsync().ForceSynchronous();
        }

        /// <inheritdoc cref="PublicIP.AssignAsync"/>
        public static void Assign(this PublicIP publicIP, string serverId)
        {
            publicIP.AssignAsync(serverId).ForceSynchronous();
        }

        /// <inheritdoc cref="PublicIP.UnassignAsync"/>
        public static void Unassign(this PublicIP publicIP)
        {
            publicIP.UnassignAsync().ForceSynchronous();
        }

        /// <inheritdoc cref="PublicIP.WaitUntilActiveAsync"/>
        public static void WaitUntilActive(this PublicIP publicIP, TimeSpan? refreshDelay = null, TimeSpan? timeout = null, IProgress<bool> progress = null)
        {
            publicIP.WaitUntilActiveAsync(refreshDelay, timeout, progress).ForceSynchronous();
        }

        /// <inheritdoc cref="PublicIP.WaitUntilDeletedAsync"/>
        public static void WaitUntilDeleted(this PublicIP publicIP, TimeSpan? refreshDelay = null, TimeSpan? timeout = null, IProgress<bool> progress = null)
        {
            publicIP.WaitUntilDeletedAsync(refreshDelay, timeout, progress).ForceSynchronous();
        }
    }
}