namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System;
    using net.openstack.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the unique identifier of a check in the <see cref="IMonitoringService"/>.
    /// </summary>
    /// <seealso cref="Check.Id"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonConverter(typeof(CheckId.Converter))]
    public sealed class CheckId : ResourceIdentifier<CheckId>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CheckId"/> class
        /// with the specified identifier value.
        /// </summary>
        /// <param name="id">The check identifier value.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="id"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="id"/> is empty.</exception>
        public CheckId(string id)
            : base(id)
        {
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="CheckId"/>
        /// objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        private sealed class Converter : ConverterBase
        {
            /// <inheritdoc/>
            protected override CheckId FromValue(string id)
            {
                return new CheckId(id);
            }
        }
    }
}
