using OpenStack.Compute.v2_1;
using OpenStack.Synchronous.Extensions;

namespace OpenStack.Synchronous
{
    /// <summary>
    /// Provides synchronous extention methods for a <see cref="ServerVolume"/> instance.
    /// </summary>
    public static class ServerVolumeExtensions
    {
        /// <inheritdoc cref="ServerVolumeReference.GetServerVolumeAsync" />
        public static ServerVolume GetServerVolume(this ServerVolumeReference volume)
        {
            return volume.GetServerVolumeAsync().ForceSynchronous();
        }

        /// <inheritdoc cref="ServerVolumeReference.DetachAsync" />
        public static void Detach(this ServerVolumeReference volume)
        {
            volume.DetachAsync().ForceSynchronous();
        }
    }
}