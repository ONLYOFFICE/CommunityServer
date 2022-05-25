namespace net.openstack.Providers.Rackspace.Objects.Databases
{
    using System;
    using net.openstack.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the unique identifier of a database instance in the <see cref="IDatabaseService"/>.
    /// </summary>
    /// <seealso cref="DatabaseInstance.Id"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonConverter(typeof(DatabaseInstanceId.Converter))]
    public sealed class DatabaseInstanceId : ResourceIdentifier<DatabaseInstanceId>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseInstanceId"/> class
        /// with the specified identifier value.
        /// </summary>
        /// <param name="id">The database instance identifier value.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="id"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="id"/> is empty.</exception>
        public DatabaseInstanceId(string id)
            : base(id)
        {
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="DatabaseInstanceId"/>
        /// objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        private sealed class Converter : ConverterBase
        {
            /// <inheritdoc/>
            protected override DatabaseInstanceId FromValue(string id)
            {
                return new DatabaseInstanceId(id);
            }
        }
    }
}
