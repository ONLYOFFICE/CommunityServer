namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System;
    using net.openstack.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the unique identifier of an entity in the <see cref="IMonitoringService"/>.
    /// </summary>
    /// <seealso cref="Entity.Id"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonConverter(typeof(EntityId.Converter))]
    public sealed class EntityId : ResourceIdentifier<EntityId>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityId"/> class
        /// with the specified identifier value.
        /// </summary>
        /// <param name="id">The entity identifier value.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="id"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="id"/> is empty.</exception>
        public EntityId(string id)
            : base(id)
        {
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="EntityId"/>
        /// objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        private sealed class Converter : ConverterBase
        {
            /// <inheritdoc/>
            protected override EntityId FromValue(string id)
            {
                return new EntityId(id);
            }
        }
    }
}
