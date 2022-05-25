namespace net.openstack.Providers.Rackspace.Objects.Databases
{
    using System;
    using net.openstack.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the unique identifier of a backup in the <see cref="IDatabaseService"/>.
    /// </summary>
    /// <seealso cref="Backup.Id"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonConverter(typeof(BackupId.Converter))]
    public sealed class BackupId : ResourceIdentifier<BackupId>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BackupId"/> class
        /// with the specified identifier value.
        /// </summary>
        /// <param name="id">The backup identifier value.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="id"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="id"/> is empty.</exception>
        public BackupId(string id)
            : base(id)
        {
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="BackupId"/>
        /// objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        private sealed class Converter : ConverterBase
        {
            /// <inheritdoc/>
            protected override BackupId FromValue(string id)
            {
                return new BackupId(id);
            }
        }
    }
}
