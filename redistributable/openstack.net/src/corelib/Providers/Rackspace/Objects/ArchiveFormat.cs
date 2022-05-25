namespace net.openstack.Providers.Rackspace.Objects
{
    using System;
    using System.Collections.Concurrent;
    using net.openstack.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a archive format for the Cloud Files Extract Archive operation.
    /// </summary>
    /// <remarks>
    /// This class functions as a strongly-typed enumeration of known archive types,
    /// with added support for unknown types returned or supported by a server extension.
    /// </remarks>
    /// <seealso cref="CloudFilesProvider.ExtractArchive"/>
    /// <seealso cref="CloudFilesProvider.ExtractArchiveFromFile"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonConverter(typeof(ArchiveFormat.Converter))]
    public sealed class ArchiveFormat : ExtensibleEnum<ArchiveFormat>
    {
        private static readonly ConcurrentDictionary<string, ArchiveFormat> _types =
            new ConcurrentDictionary<string, ArchiveFormat>(StringComparer.OrdinalIgnoreCase);
        private static readonly ArchiveFormat _tar = FromName("tar");
        private static readonly ArchiveFormat _tarGz = FromName("tar.gz");
        private static readonly ArchiveFormat _tarBz2 = FromName("tar.bz2");

        /// <summary>
        /// Initializes a new instance of the <see cref="ArchiveFormat"/> class with the specified name.
        /// </summary>
        /// <inheritdoc/>
        private ArchiveFormat(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Gets the <see cref="ArchiveFormat"/> instance with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The unique <see cref="ArchiveFormat"/> instance with the specified name.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="name"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="name"/> is empty.</exception>
        public static ArchiveFormat FromName(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name cannot be empty");

            return _types.GetOrAdd(name, i => new ArchiveFormat(i));
        }

        /// <summary>
        /// Gets an <see cref="ArchiveFormat"/> representing <c>tar</c> files.
        /// </summary>
        public static ArchiveFormat Tar
        {
            get
            {
                return _tar;
            }
        }

        /// <summary>
        /// Gets an <see cref="ArchiveFormat"/> representing <c>tar.gz</c> files.
        /// </summary>
        public static ArchiveFormat TarGz
        {
            get
            {
                return _tarGz;
            }
        }

        /// <summary>
        /// Gets an <see cref="ArchiveFormat"/> representing <c>tar.bz2</c> files.
        /// </summary>
        public static ArchiveFormat TarBz2
        {
            get
            {
                return _tarBz2;
            }
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="ArchiveFormat"/>
        /// objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        /// <preliminary/>
        private sealed class Converter : ConverterBase
        {
            /// <inheritdoc/>
            protected override ArchiveFormat FromName(string name)
            {
                return ArchiveFormat.FromName(name);
            }
        }
    }
}
