namespace net.openstack.Core.Domain
{
    using System;
    using System.Collections.Concurrent;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the state of a block storage volume.
    /// </summary>
    /// <remarks>
    /// This class functions as a strongly-typed enumeration of known volume states,
    /// with added support for unknown states returned by a server extension.
    /// </remarks>
    /// <threadsafety static="true" instance="false"/>
    [JsonConverter(typeof(VolumeState.Converter))]
    public sealed class VolumeState : ExtensibleEnum<VolumeState>
    {
        private static readonly ConcurrentDictionary<string, VolumeState> _states =
            new ConcurrentDictionary<string, VolumeState>(StringComparer.OrdinalIgnoreCase);
        private static readonly VolumeState _creating = FromName("creating");
        private static readonly VolumeState _available = FromName("available");
        private static readonly VolumeState _attaching = FromName("attaching");
        private static readonly VolumeState _inUse = FromName("in-use");
        private static readonly VolumeState _deleting = FromName("deleting");
        private static readonly VolumeState _error = FromName("error");
        private static readonly VolumeState _errorDeleting = FromName("error_deleting");
        private static readonly VolumeState _backingUp = FromName("backing-up");
        private static readonly VolumeState _restoringBackup = FromName("restoring-backup");
        private static readonly VolumeState _errorRestoring = FromName("error_restoring");

        /// <summary>
        /// Initializes a new instance of the <see cref="VolumeState"/> class with the specified name.
        /// </summary>
        /// <inheritdoc/>
        private VolumeState(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Gets the <see cref="VolumeState"/> instance with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The unique <see cref="VolumeState"/> instance with the specified name.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="name"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="name"/> is empty.</exception>
        public static VolumeState FromName(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name cannot be empty");

            return _states.GetOrAdd(name, i => new VolumeState(i));
        }

        /// <summary>
        /// Gets a <see cref="VolumeState"/> indicating the volume is being created.
        /// </summary>
        public static VolumeState Creating
        {
            get
            {
                return _creating;
            }
        }

        /// <summary>
        /// Gets a <see cref="VolumeState"/> indicating the volume is ready to be attached to an instance.
        /// </summary>
        public static VolumeState Available
        {
            get
            {
                return _available;
            }
        }

        /// <summary>
        /// Gets a <see cref="VolumeState"/> indicating the volume is attaching to an instance.
        /// </summary>
        public static VolumeState Attaching
        {
            get
            {
                return _attaching;
            }
        }

        /// <summary>
        /// Gets a <see cref="VolumeState"/> indicating the volume is attached to an instance.
        /// </summary>
        public static VolumeState InUse
        {
            get
            {
                return _inUse;
            }
        }

        /// <summary>
        /// Gets a <see cref="VolumeState"/> indicating the volume is being deleted.
        /// </summary>
        public static VolumeState Deleting
        {
            get
            {
                return _deleting;
            }
        }

        /// <summary>
        /// Gets a <see cref="VolumeState"/> indicating an error occurred during volume creation.
        /// </summary>
        public static VolumeState Error
        {
            get
            {
                return _error;
            }
        }

        /// <summary>
        /// Gets a <see cref="VolumeState"/> indicating an error occurred during volume deletion.
        /// </summary>
        public static VolumeState ErrorDeleting
        {
            get
            {
                return _errorDeleting;
            }
        }

        /// <summary>
        /// Gets a <see cref="VolumeState"/> indicating the volume is being backed-up.
        /// </summary>
        public static VolumeState BackingUp
        {
            get
            {
                return _backingUp;
            }
        }

        /// <summary>
        /// Gets a <see cref="VolumeState"/> indicating a backup is being restored to the volume.
        /// </summary>
        public static VolumeState RestoringBackup
        {
            get
            {
                return _restoringBackup;
            }
        }

        /// <summary>
        /// Gets a <see cref="VolumeState"/> indicating an error occurred during backup restoration to a volume.
        /// </summary>
        public static VolumeState ErrorRestoring
        {
            get
            {
                return _errorRestoring;
            }
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="VolumeState"/>
        /// objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        private sealed class Converter : ConverterBase
        {
            /// <inheritdoc/>
            protected override VolumeState FromName(string name)
            {
                return VolumeState.FromName(name);
            }
        }
    }
}
