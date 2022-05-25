namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System;
    using net.openstack.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the unique identifier of an audit in the <see cref="IMonitoringService"/>.
    /// </summary>
    /// <seealso cref="Audit.Id"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonConverter(typeof(AuditId.Converter))]
    public sealed class AuditId : ResourceIdentifier<AuditId>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuditId"/> class
        /// with the specified identifier value.
        /// </summary>
        /// <param name="id">The audit identifier value.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="id"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="id"/> is empty.</exception>
        public AuditId(string id)
            : base(id)
        {
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="AuditId"/>
        /// objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        private sealed class Converter : ConverterBase
        {
            /// <inheritdoc/>
            protected override AuditId FromValue(string id)
            {
                return new AuditId(id);
            }
        }
    }
}
