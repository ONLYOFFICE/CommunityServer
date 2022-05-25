namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System;
    using net.openstack.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the unique identifier of a notification plan in the <see cref="IMonitoringService"/>.
    /// </summary>
    /// <seealso cref="NotificationPlan.Id"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonConverter(typeof(NotificationPlanId.Converter))]
    public sealed class NotificationPlanId : ResourceIdentifier<NotificationPlanId>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationPlanId"/> class
        /// with the specified identifier value.
        /// </summary>
        /// <param name="id">The notification plan identifier value.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="id"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="id"/> is empty.</exception>
        public NotificationPlanId(string id)
            : base(id)
        {
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="NotificationPlanId"/>
        /// objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        private sealed class Converter : ConverterBase
        {
            /// <inheritdoc/>
            protected override NotificationPlanId FromValue(string id)
            {
                return new NotificationPlanId(id);
            }
        }
    }
}
