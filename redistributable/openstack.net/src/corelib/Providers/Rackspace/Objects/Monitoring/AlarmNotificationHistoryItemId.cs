namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System;
    using net.openstack.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the unique identifier of an alarm notification history item in the <see cref="IMonitoringService"/>.
    /// </summary>
    /// <seealso cref="AlarmNotificationHistoryItem.Id"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonConverter(typeof(AlarmNotificationHistoryItemId.Converter))]
    public sealed class AlarmNotificationHistoryItemId : ResourceIdentifier<AlarmNotificationHistoryItemId>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AlarmNotificationHistoryItemId"/> class
        /// with the specified identifier value.
        /// </summary>
        /// <param name="id">The alarm notification history item identifier value.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="id"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="id"/> is empty.</exception>
        public AlarmNotificationHistoryItemId(string id)
            : base(id)
        {
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="AlarmNotificationHistoryItemId"/>
        /// objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        private sealed class Converter : ConverterBase
        {
            /// <inheritdoc/>
            protected override AlarmNotificationHistoryItemId FromValue(string id)
            {
                return new AlarmNotificationHistoryItemId(id);
            }
        }
    }
}
