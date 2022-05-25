namespace net.openstack.Providers.Rackspace.Objects.Databases
{
    using System;
    using net.openstack.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the unique name of a database in the <see cref="IDatabaseService"/>.
    /// </summary>
    /// <seealso cref="Database.Name"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonConverter(typeof(DatabaseName.Converter))]
    public sealed class DatabaseName : ResourceIdentifier<DatabaseName>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseName"/> class
        /// with the specified name.
        /// </summary>
        /// <param name="id">The database name.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="id"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="id"/> is empty.</exception>
        public DatabaseName(string id)
            : base(id)
        {
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="DatabaseName"/>
        /// objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        private sealed class Converter : ConverterBase
        {
            /// <inheritdoc/>
            protected override DatabaseName FromValue(string id)
            {
                return new DatabaseName(id);
            }
        }
    }
}
