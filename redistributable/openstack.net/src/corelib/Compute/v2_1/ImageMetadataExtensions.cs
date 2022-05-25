using OpenStack.Compute.v2_1;
using OpenStack.Synchronous.Extensions;

// ReSharper disable once CheckNamespace
namespace OpenStack.Synchronous
{
    /// <summary>
    /// Provides synchronous extention methods for a <see cref="ImageMetadata"/> instance.
    /// </summary>
    public static class ImageMetadataExtensions_v2_1
    {
        /// <inheritdoc cref="ImageMetadata.CreateAsync"/>
        public static void Create(this ImageMetadata metadata, string key, string value)
        {
            metadata.CreateAsync(key, value).ForceSynchronous();
        }

        /// <inheritdoc cref="ImageMetadata.UpdateAsync"/>
        public static void Update(this ImageMetadata metadata, bool overwrite = false)
        {
            metadata.UpdateAsync(overwrite).ForceSynchronous();
        }

        /// <inheritdoc cref="ImageMetadata.DeleteAsync"/>
        public static void Delete(this ImageMetadata metadata, string key)
        {
            metadata.DeleteAsync(key).ForceSynchronous();
        }
    }
}
