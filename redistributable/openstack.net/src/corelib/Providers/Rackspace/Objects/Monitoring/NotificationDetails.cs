namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This is the base class for classes modeling the detailed configuration parameters
    /// of various types of notifications.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class NotificationDetails : ExtensibleJsonObject
    {
        /// <summary>
        /// Deserializes a JSON object to a <see cref="NotificationDetails"/> instance of the proper type.
        /// </summary>
        /// <param name="notificationTypeId">The notification type ID.</param>
        /// <param name="obj">The JSON object representing the notification details.</param>
        /// <returns>A <see cref="NotificationDetails"/> object corresponding to the JSON object.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="notificationTypeId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="obj"/> is <see langword="null"/>.</para>
        /// </exception>
        public static NotificationDetails FromJObject(NotificationTypeId notificationTypeId, JObject obj)
        {
            if (notificationTypeId == null)
                throw new ArgumentNullException("notificationTypeId");
            if (obj == null)
                throw new ArgumentNullException("obj");

            if (notificationTypeId == NotificationTypeId.Webhook)
                return obj.ToObject<WebhookNotificationDetails>();
            else if (notificationTypeId == NotificationTypeId.Email)
                return obj.ToObject<EmailNotificationDetails>();
            else if (notificationTypeId == NotificationTypeId.PagerDuty)
                return obj.ToObject<PagerDutyNotificationDetails>();
            else
                return obj.ToObject<GenericNotificationDetails>();
        }

        /// <summary>
        /// Determines whether the current <see cref="NotificationDetails"/> object is compatible
        /// with notifications of a particular type.
        /// </summary>
        /// <param name="notificationTypeId">The notification type ID.</param>
        /// <returns><see langword="true"/> if the current <see cref="NotificationDetails"/> object is compatible with <paramref name="notificationTypeId"/>; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="notificationTypeId"/> is <see langword="null"/>.</exception>
        protected internal abstract bool SupportsNotificationType(NotificationTypeId notificationTypeId);
    }
}
