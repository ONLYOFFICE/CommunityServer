namespace net.openstack.Providers.Rackspace.Objects.Databases
{
    using System;
    using net.openstack.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the unique URI reference to a flavor in the <see cref="IDatabaseService"/>.
    /// </summary>
    /// <seealso cref="DatabaseFlavor.Href"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonConverter(typeof(FlavorRef.Converter))]
    public sealed class FlavorRef : ResourceIdentifier<FlavorRef>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FlavorRef"/> class
        /// with the specified identifier value.
        /// </summary>
        /// <param name="id">The URI reference for the flavor.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="id"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="id"/> is empty.</exception>
        public FlavorRef(string id)
            : base(id)
        {
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="FlavorRef"/>
        /// objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        private sealed class Converter : ConverterBase
        {
            /// <inheritdoc/>
            protected override FlavorRef FromValue(string id)
            {
                return new FlavorRef(id);
            }
        }
    }
}
