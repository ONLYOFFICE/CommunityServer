namespace net.openstack.Core.Domain
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the unique identifier of an image.
    /// </summary>
    /// <seealso cref="SimpleServerImage.Id"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonConverter(typeof(ImageId.Converter))]
    public sealed class ImageId : ResourceIdentifier<ImageId>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImageId"/> class
        /// with the specified identifier value.
        /// </summary>
        /// <param name="id">The image identifier value.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="id"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="id"/> is empty.</exception>
        public ImageId(string id)
            : base(id)
        {
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="ImageId"/>
        /// objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        private sealed class Converter : ConverterBase
        {
            /// <inheritdoc/>
            protected override ImageId FromValue(string id)
            {
                return new ImageId(id);
            }
        }
    }
}
