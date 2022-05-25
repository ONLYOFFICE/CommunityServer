using OpenStack.Synchronous.Extensions;

// ReSharper disable once CheckNamespace
namespace OpenStack.Networking.v2.Layer3.Synchronous
{
    /// <summary>
    /// Provides synchronous extention methods for a <see cref="FloatingIP"/> instance.
    /// </summary>
    public static class RouterExtensions
    {
        /// <inheritdoc cref="Router.DeleteAsync" />
        public static void Delete(this Router router)
        {
            router.DeleteAsync().ForceSynchronous();
        }

        /// <inheritdoc cref="Router.AttachPortAsync" />
        public static void AttachPort(this Router router, Identifier portId)
        {
            router.AttachPortAsync(portId).ForceSynchronous();
        }

        /// <inheritdoc cref="Router.AttachSubnetAsync" />
        public static Identifier AttachSubnet(this Router router, Identifier subnetId)
        {
            return router.AttachSubnetAsync(subnetId).ForceSynchronous();
        }

        /// <inheritdoc cref="Router.DetachPortAsync" />
        public static void DetachPort(this Router router, Identifier portId)
        {
            router.DetachPortAsync(portId).ForceSynchronous();
        }

        /// <inheritdoc cref="Router.DetachSubnetAsync" />
        public static void DetachSubnet(this Router router, Identifier subnetId)
        {
            router.DetachSubnetAsync(subnetId).ForceSynchronous();
        }
    }
}