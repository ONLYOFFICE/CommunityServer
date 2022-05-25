namespace net.openstack.Providers.Rackspace.Objects.Databases
{
    using System;
    using net.openstack.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the unique identifier of a flavor in the <see cref="IDatabaseService"/>.
    /// </summary>
    /// <seealso cref="DatabaseFlavor.Id"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonConverter(typeof(FlavorId.Converter))]
    public sealed class FlavorId : ResourceIdentifier<FlavorId>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FlavorId"/> class
        /// with the specified identifier value.
        /// </summary>
        /// <param name="id">The flavor identifier value.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="id"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="id"/> is empty.</exception>
        public FlavorId(string id)
            : base(id)
        {
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="FlavorId"/>
        /// objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        private sealed class Converter : ConverterBase
        {
            /// <inheritdoc/>
            protected override FlavorId FromValue(string id)
            {
                return new FlavorId(id);
            }
        }
    }
}
