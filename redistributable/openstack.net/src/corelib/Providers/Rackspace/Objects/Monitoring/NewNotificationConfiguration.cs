namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// This class models the JSON representation of a request to create or test a new
    /// <see cref="Notification"/> resource in the <see cref="IMonitoringService"/>.
    /// </summary>
    /// <seealso cref="IMonitoringService.CreateNotificationAsync"/>
    /// <seealso cref="IMonitoringService.TestNotificationAsync"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class NewNotificationConfiguration : NotificationConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NewNotificationConfiguration"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected NewNotificationConfiguration()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NewNotificationConfiguration"/> class
        /// with the specified properties.
        /// </summary>
        /// <param name="label">The friendly name of the notification.</param>
        /// <param name="notificationTypeId">The notification type ID. This is obtained from <see cref="NotificationType.Id">NotificationType.Id</see>, or from the predefined values in <see cref="NotificationTypeId"/>.</param>
        /// <param name="details">A <see cref="NotificationDetails"/> object containing the detailed configuration properties for the specified notification type.</param>
        /// <param name="metadata">A collection of metadata to associate with the notification. If the value is <see langword="null"/>, no custom metadata is associated with the notification.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="label"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="details"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="label"/> is empty.
        /// <para>-or-</para>
        /// <para>If <paramref name="details"/> is non-<see langword="null"/> and <paramref name="notificationTypeId"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="details"/> does not support notifications of type <paramref name="notificationTypeId"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="metadata"/> contains any empty keys.</para>
        /// </exception>
        public NewNotificationConfiguration(string label, NotificationTypeId notificationTypeId, NotificationDetails details, IDictionary<string, string> metadata = null)
            : base(label, notificationTypeId, details, metadata)
        {
            if (label == null)
                throw new ArgumentNullException("label");
            if (details == null)
                throw new ArgumentNullException("details");
        }
    }
}
