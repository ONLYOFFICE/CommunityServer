using OpenStack.Compute.v2_1;
using OpenStack.Synchronous.Extensions;

// ReSharper disable once CheckNamespace
namespace OpenStack.Synchronous
{
    /// <summary>
    /// Provides synchronous extention methods for a <see cref="ServerMetadata"/> instance.
    /// </summary>
    public static class ServerMetadataExtensions_v2_1
    {
        /// <inheritdoc cref="ServerMetadata.CreateAsync"/>
        public static void Create(this ServerMetadata metadata, string key, string value)
        {
            metadata.CreateAsync(key, value).ForceSynchronous();
        }

        /// <inheritdoc cref="ServerMetadata.UpdateAsync"/>
        public static void Update(this ServerMetadata metadata, bool overwrite = false)
        {
            metadata.UpdateAsync(overwrite).ForceSynchronous();
        }

        /// <inheritdoc cref="ServerMetadata.DeleteAsync"/>
        public static void Delete(this ServerMetadata metadata, string key)
        {
            metadata.DeleteAsync(key).ForceSynchronous();
        }
    }
}
