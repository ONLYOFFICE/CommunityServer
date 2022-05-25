namespace net.openstack.Providers.Rackspace.Objects.Databases
{
    using System;
    using System.Collections.Concurrent;
    using net.openstack.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the status of a database instance.
    /// </summary>
    /// <remarks>
    /// This class functions as a strongly-typed enumeration of known statuses,
    /// with added support for unknown statuses returned by a server extension.
    /// </remarks>
    /// <seealso cref="DatabaseInstance.Status"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonConverter(typeof(DatabaseInstanceStatus.Converter))]
    public sealed class DatabaseInstanceStatus : ExtensibleEnum<DatabaseInstanceStatus>
    {
        private static readonly ConcurrentDictionary<string, DatabaseInstanceStatus> _types =
            new ConcurrentDictionary<string, DatabaseInstanceStatus>(StringComparer.OrdinalIgnoreCase);
        private static readonly DatabaseInstanceStatus _build = FromName("BUILD");
        private static readonly DatabaseInstanceStatus _reboot = FromName("REBOOT");
        private static readonly DatabaseInstanceStatus _active = FromName("ACTIVE");
        private static readonly DatabaseInstanceStatus _backup = FromName("BACKUP");
        private static readonly DatabaseInstanceStatus _blocked = FromName("BLOCKED");
        private static readonly DatabaseInstanceStatus _resize = FromName("RESIZE");
        private static readonly DatabaseInstanceStatus _shutdown = FromName("SHUTDOWN");
        private static readonly DatabaseInstanceStatus _error = FromName("ERROR");

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseInstanceStatus"/> class with the specified name.
        /// </summary>
        /// <inheritdoc/>
        private DatabaseInstanceStatus(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Gets the <see cref="DatabaseInstanceStatus"/> instance with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The unique <see cref="DatabaseInstanceStatus"/> instance with the specified name.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="name"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="name"/> is empty.</exception>
        public static DatabaseInstanceStatus FromName(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name cannot be empty");

            return _types.GetOrAdd(name, i => new DatabaseInstanceStatus(i));
        }

        /// <summary>
        /// Gets a <see cref="DatabaseInstanceStatus"/> representing a database instance which is being provisioned.
        /// </summary>
        public static DatabaseInstanceStatus Build
        {
            get
            {
                return _build;
            }
        }

        /// <summary>
        /// Gets a <see cref="DatabaseInstanceStatus"/> representing a database instance which is rebooting.
        /// </summary>
        public static DatabaseInstanceStatus Reboot
        {
            get
            {
                return _reboot;
            }
        }

        /// <summary>
        /// Gets a <see cref="DatabaseInstanceStatus"/> representing a database instance which is online and available to take requests.
        /// </summary>
        public static DatabaseInstanceStatus Active
        {
            get
            {
                return _active;
            }
        }

        /// <summary>
        /// Gets a <see cref="DatabaseInstanceStatus"/> representing a database instance which is currently running a backup process.
        /// </summary>
        public static DatabaseInstanceStatus Backup
        {
            get
            {
                return _backup;
            }
        }

        /// <summary>
        /// Gets a <see cref="DatabaseInstanceStatus"/> representing a database instance which is currently unresponsive.
        /// </summary>
        public static DatabaseInstanceStatus Blocked
        {
            get
            {
                return _blocked;
            }
        }

        /// <summary>
        /// Gets a <see cref="DatabaseInstanceStatus"/> representing a database instance which is being resized.
        /// </summary>
        public static DatabaseInstanceStatus Resize
        {
            get
            {
                return _resize;
            }
        }

        /// <summary>
        /// Gets a <see cref="DatabaseInstanceStatus"/> representing a database instance which is terminating services,
        /// or the MySQL instance is shut down but not the actual server.
        /// </summary>
        public static DatabaseInstanceStatus Shutdown
        {
            get
            {
                return _shutdown;
            }
        }

        /// <summary>
        /// Gets a <see cref="DatabaseInstanceStatus"/> representing a database instance where the last operation failed due to an error.
        /// </summary>
        public static DatabaseInstanceStatus Error
        {
            get
            {
                return _error;
            }
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="DatabaseInstanceStatus"/>
        /// objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        /// <preliminary/>
        private sealed class Converter : ConverterBase
        {
            /// <inheritdoc/>
            protected override DatabaseInstanceStatus FromName(string name)
            {
                return DatabaseInstanceStatus.FromName(name);
            }
        }
    }
}
