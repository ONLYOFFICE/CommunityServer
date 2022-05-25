namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// This class models the JSON representation of a request to update the properties
    /// of a <see cref="Notification"/> resource in the <see cref="IMonitoringService"/>.
    /// </summary>
    /// <seealso cref="IMonitoringService.UpdateNotificationAsync"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class UpdateNotificationConfiguration : NotificationConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateNotificationConfiguration"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected UpdateNotificationConfiguration()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateNotificationConfiguration"/> class
        /// with the specified properties.
        /// </summary>
        /// <param name="label">The friendly name of the notification. If this value is <see langword="null"/>, the existing value for the notification is not changed.</param>
        /// <param name="notificationTypeId">The notification type ID. This is obtained from <see cref="NotificationType.Id">NotificationType.Id</see>, or from the predefined values in <see cref="NotificationTypeId"/>. If this value is <see langword="null"/>, the existing value for the notification is not changed.</param>
        /// <param name="details">A <see cref="NotificationDetails"/> object containing the detailed configuration properties for the specified notification type. If this value is <see langword="null"/>, the existing value for the notification is not changed.</param>
        /// <param name="metadata">A collection of metadata to associate with the notification. If this value is <see langword="null"/>, the existing value for the notification is not changed.</param>
        /// <exception cref="ArgumentException">
        /// If <paramref name="label"/> is empty.
        /// <para>-or-</para>
        /// <para>If <paramref name="details"/> is non-<see langword="null"/> and <paramref name="notificationTypeId"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="details"/> does not support notifications of type <paramref name="notificationTypeId"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="metadata"/> contains any empty keys.</para>
        /// </exception>
        public UpdateNotificationConfiguration(string label = null, NotificationTypeId notificationTypeId = null, NotificationDetails details = null, IDictionary<string, string> metadata = null)
            : base(label, notificationTypeId, details, metadata)
        {
        }
    }
}
