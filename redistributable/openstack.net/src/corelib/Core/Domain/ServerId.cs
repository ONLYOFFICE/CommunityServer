namespace net.openstack.Core.Domain
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the unique identifier of a server.
    /// </summary>
    /// <seealso cref="ServerBase.Id"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonConverter(typeof(ServerId.Converter))]
    public sealed class ServerId : ResourceIdentifier<ServerId>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServerId"/> class
        /// with the specified identifier value.
        /// </summary>
        /// <param name="id">The server identifier value.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="id"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="id"/> is empty.</exception>
        public ServerId(string id)
            : base(id)
        {
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="ServerId"/>
        /// objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        private sealed class Converter : ConverterBase
        {
            /// <inheritdoc/>
            protected override ServerId FromValue(string id)
            {
                return new ServerId(id);
            }
        }
    }
}
