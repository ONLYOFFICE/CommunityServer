namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System;
    using net.openstack.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the unique identifier of a monitoring account in the <see cref="IMonitoringService"/>.
    /// </summary>
    /// <seealso cref="MonitoringAccount.Id"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonConverter(typeof(MonitoringAccountId.Converter))]
    public sealed class MonitoringAccountId : ResourceIdentifier<MonitoringAccountId>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MonitoringAccountId"/> class
        /// with the specified identifier value.
        /// </summary>
        /// <param name="id">The monitoring account identifier value.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="id"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="id"/> is empty.</exception>
        public MonitoringAccountId(string id)
            : base(id)
        {
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="MonitoringAccountId"/>
        /// objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        private sealed class Converter : ConverterBase
        {
            /// <inheritdoc/>
            protected override MonitoringAccountId FromValue(string id)
            {
                return new MonitoringAccountId(id);
            }
        }
    }
}
