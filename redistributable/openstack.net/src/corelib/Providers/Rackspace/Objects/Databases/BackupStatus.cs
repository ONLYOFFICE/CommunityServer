namespace net.openstack.Providers.Rackspace.Objects.Databases
{
    using System;
    using System.Collections.Concurrent;
    using net.openstack.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the status of a database backup.
    /// </summary>
    /// <remarks>
    /// This class functions as a strongly-typed enumeration of known statuses,
    /// with added support for unknown statuses returned by a server extension.
    /// </remarks>
    /// <seealso cref="Backup.Status"/>
    /// <seealso href="http://docs.rackspace.com/cdb/api/v1.0/cdb-devguide/content/backups.html">Backups (Rackspace Cloud Databases Developer Guide - API v1.0)</seealso>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonConverter(typeof(BackupStatus.Converter))]
    public sealed class BackupStatus : ExtensibleEnum<BackupStatus>
    {
        private static readonly ConcurrentDictionary<string, BackupStatus> _types =
            new ConcurrentDictionary<string, BackupStatus>(StringComparer.OrdinalIgnoreCase);
        private static readonly BackupStatus _new = FromName("NEW");
        private static readonly BackupStatus _building = FromName("BUILDING");
        private static readonly BackupStatus _completed = FromName("COMPLETED");
        private static readonly BackupStatus _failed = FromName("FAILED");
        private static readonly BackupStatus _deleteFailed = FromName("DELETE_FAILED");

        /// <summary>
        /// Initializes a new instance of the <see cref="BackupStatus"/> class with the specified name.
        /// </summary>
        /// <inheritdoc/>
        private BackupStatus(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Gets the <see cref="BackupStatus"/> instance with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The unique <see cref="BackupStatus"/> instance with the specified name.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="name"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="name"/> is empty.</exception>
        public static BackupStatus FromName(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name cannot be empty");

            return _types.GetOrAdd(name, i => new BackupStatus(i));
        }

        /// <summary>
        /// Gets a <see cref="BackupStatus"/> representing a backup task that is created but not yet running.
        /// </summary>
        public static BackupStatus New
        {
            get
            {
                return _new;
            }
        }

        /// <summary>
        /// Gets a <see cref="BackupStatus"/> representing a backup task that is currently running.
        /// </summary>
        public static BackupStatus Building
        {
            get
            {
                return _building;
            }
        }

        /// <summary>
        /// Gets a <see cref="BackupStatus"/> representing a backup task which completed successfully.
        /// </summary>
        public static BackupStatus Completed
        {
            get
            {
                return _completed;
            }
        }

        /// <summary>
        /// Gets a <see cref="BackupStatus"/> representing a backup task which failed to complete successfully.
        /// </summary>
        public static BackupStatus Failed
        {
            get
            {
                return _failed;
            }
        }

        /// <summary>
        /// Gets a <see cref="BackupStatus"/> representing a backup task which failed to delete Cloud Files objects.
        /// </summary>
        public static BackupStatus DeleteFailed
        {
            get
            {
                return _deleteFailed;
            }
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="BackupStatus"/>
        /// objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        /// <preliminary/>
        private sealed class Converter : ConverterBase
        {
            /// <inheritdoc/>
            protected override BackupStatus FromName(string name)
            {
                return BackupStatus.FromName(name);
            }
        }
    }
}
