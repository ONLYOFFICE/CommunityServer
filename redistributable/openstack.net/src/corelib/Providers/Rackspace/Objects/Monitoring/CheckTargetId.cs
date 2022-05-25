namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System;
    using net.openstack.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the unique identifier of a check target in the <see cref="IMonitoringService"/>.
    /// </summary>
    /// <seealso cref="CheckTarget.Id"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonConverter(typeof(CheckTargetId.Converter))]
    public sealed class CheckTargetId : ResourceIdentifier<CheckTargetId>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CheckTargetId"/> class
        /// with the specified identifier value.
        /// </summary>
        /// <param name="id">The check target identifier value.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="id"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="id"/> is empty.</exception>
        public CheckTargetId(string id)
            : base(id)
        {
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="CheckTargetId"/>
        /// objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        private sealed class Converter : ConverterBase
        {
            /// <inheritdoc/>
            protected override CheckTargetId FromValue(string id)
            {
                return new CheckTargetId(id);
            }
        }
    }
}
