using OpenStack.Synchronous.Extensions;

// ReSharper disable once CheckNamespace
namespace OpenStack.Networking.v2.Layer3.Synchronous
{
    /// <summary>
    /// Provides synchronous extention methods for a <see cref="FloatingIP"/> instance.
    /// </summary>
    public static class FloatingIPExtensions
    {
        /// <inheritdoc cref="FloatingIP.AssociateAsync" />
        public static void Associate(this FloatingIP floatingIP, Identifier portId)
        {
            floatingIP.AssociateAsync(portId).ForceSynchronous();
        }

        /// <inheritdoc cref="FloatingIP.DisassociateAsync" />
        public static void Disassociate(this FloatingIP floatingIP)
        {
            floatingIP.DisassociateAsync().ForceSynchronous();
        }
    }
}