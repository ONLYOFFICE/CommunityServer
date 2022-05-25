namespace net.openstack.Core.Domain
{
    using System;
    using System.Collections.Concurrent;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents an image type.
    /// </summary>
    /// <remarks>
    /// This class functions as a strongly-typed enumeration of known image types,
    /// with added support for unknown types returned by a server extension.
    /// </remarks>
    /// <threadsafety static="true" instance="false"/>
    [JsonConverter(typeof(ImageType.Converter))]
    public sealed class ImageType : ExtensibleEnum<ImageType>
    {
        private static readonly ConcurrentDictionary<string, ImageType> _types =
            new ConcurrentDictionary<string, ImageType>(StringComparer.OrdinalIgnoreCase);
        private static readonly ImageType _base = FromName("BASE");
        private static readonly ImageType _snapshot = FromName("SNAPSHOT");

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageType"/> class with the specified name.
        /// </summary>
        /// <inheritdoc/>
        private ImageType(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Gets the <see cref="ImageType"/> instance with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The unique <see cref="ImageType"/> instance with the specified name.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="name"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="name"/> is empty.</exception>
        public static ImageType FromName(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name cannot be empty");

            return _types.GetOrAdd(name, i => new ImageType(i));
        }

        /// <summary>
        /// Gets an <see cref="ImageType"/> representing a base image.
        /// </summary>
        public static ImageType Base
        {
            get
            {
                return _base;
            }
        }

        /// <summary>
        /// Gets an <see cref="ImageType"/> representing an image created as a snapshot.
        /// </summary>
        public static ImageType Snapshot
        {
            get
            {
                return _snapshot;
            }
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="ImageType"/>
        /// objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        private sealed class Converter : ConverterBase
        {
            /// <inheritdoc/>
            protected override ImageType FromName(string name)
            {
                return ImageType.FromName(name);
            }
        }
    }
}
