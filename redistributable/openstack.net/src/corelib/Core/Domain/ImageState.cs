namespace net.openstack.Core.Domain
{
    using System;
    using System.Collections.Concurrent;
    using net.openstack.Core.Providers;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the state of a compute image.
    /// </summary>
    /// <remarks>
    /// This class functions as a strongly-typed enumeration of known image states,
    /// with added support for unknown states returned by an image extension.
    /// </remarks>
    /// <threadsafety static="true" instance="false"/>
    [JsonConverter(typeof(ImageState.Converter))]
    public sealed class ImageState : ExtensibleEnum<ImageState>
    {
        private static readonly ConcurrentDictionary<string, ImageState> _states =
            new ConcurrentDictionary<string, ImageState>(StringComparer.OrdinalIgnoreCase);
        private static readonly ImageState _active = FromName("ACTIVE");
        private static readonly ImageState _saving = FromName("SAVING");
        private static readonly ImageState _deleted = FromName("DELETED");
        private static readonly ImageState _error = FromName("ERROR");
        private static readonly ImageState _unknown = FromName("UNKNOWN");

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageState"/> class with the specified name.
        /// </summary>
        /// <inheritdoc/>
        private ImageState(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Gets the <see cref="ImageState"/> instance with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The unique <see cref="ImageState"/> instance with the specified name.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="name"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="name"/> is empty.</exception>
        public static ImageState FromName(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name cannot be empty");

            return _states.GetOrAdd(name, i => new ImageState(i));
        }

        /// <summary>
        /// Gets an <see cref="ImageState"/> representing an image which is active and ready to use.
        /// </summary>
        public static ImageState Active
        {
            get
            {
                return _active;
            }
        }

        /// <summary>
        /// Gets an <see cref="ImageState"/> representing an image currently being saved.
        /// </summary>
        public static ImageState Saving
        {
            get
            {
                return _saving;
            }
        }

        /// <summary>
        /// Gets an <see cref="ImageState"/> representing an image which has been deleted.
        /// </summary>
        /// <remarks>
        /// By default, the <see cref="IComputeProvider.ListImages"/> operation does not return
        /// images which have been deleted. To list deleted images, call
        /// <see cref="IComputeProvider.ListImages"/> specifying the <c>changesSince</c>
        /// parameter.
        /// </remarks>
        public static ImageState Deleted
        {
            get
            {
                return _deleted;
            }
        }

        /// <summary>
        /// Gets an <see cref="ImageState"/> representing an image which failed to perform
        /// an operation and is now in an error state.
        /// </summary>
        public static ImageState Error
        {
            get
            {
                return _error;
            }
        }

        /// <summary>
        /// Gets an <see cref="ImageState"/> for an image that is currently in an unknown state.
        /// </summary>
        public static ImageState Unknown
        {
            get
            {
                return _unknown;
            }
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="ImageState"/>
        /// objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        private sealed class Converter : ConverterBase
        {
            /// <inheritdoc/>
            protected override ImageState FromName(string name)
            {
                return ImageState.FromName(name);
            }
        }
    }
}
