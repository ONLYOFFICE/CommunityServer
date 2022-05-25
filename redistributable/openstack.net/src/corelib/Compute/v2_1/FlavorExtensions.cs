using OpenStack.Compute.v2_1;
using OpenStack.Synchronous.Extensions;

// ReSharper disable once CheckNamespace
namespace OpenStack.Synchronous
{
    /// <summary>
    /// Provides synchronous extention methods for a <see cref="Flavor"/> instance.
    /// </summary>
    public static class FlavorExtensions_v2_1
    {
        /// <inheritdoc cref="FlavorReference.GetFlavorAsync"/>
        public static Flavor GetFlavor(this FlavorReference flavor)
        {
            return flavor.GetFlavorAsync().ForceSynchronous();
        }
    }
}
