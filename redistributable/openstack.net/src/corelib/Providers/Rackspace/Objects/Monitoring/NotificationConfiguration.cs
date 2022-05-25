using System.Collections.ObjectModel;

namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System;
    using System.Collections.Generic;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This class models the configurable properties of the JSON representation of
    /// a notification resource in the <see cref="IMonitoringService"/>.
    /// </summary>
    /// <seealso cref="Notification"/>
    /// <seealso cref="NewNotificationConfiguration"/>
    /// <seealso cref="UpdateNotificationConfiguration"/>
    /// <seealso cref="IMonitoringService.CreateNotificationAsync"/>
    /// <see href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-notifications.html">Notifications (Rackspace Cloud Monitoring Developer Guide - API v1.0)</see>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class NotificationConfiguration : ExtensibleJsonObject
    {
        /// <summary>
        /// This is the backing field for the <see cref="Label"/> property.
        /// </summary>
        [JsonProperty("label", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private string _label;

        /// <summary>
        /// This is the backing field for the <see cref="Type"/> property.
        /// </summary>
        [JsonProperty("type", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private NotificationTypeId _type;

        /// <summary>
        /// This is the backing field for the <see cref="Details"/> property.
        /// </summary>
        [JsonProperty("details", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private JObject _details;

        /// <summary>
        /// This is the backing field for the <see cref="Metadata"/> property.
        /// </summary>
        [JsonProperty("metadata", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private IDictionary<string, string> _metadata;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationConfiguration"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected NotificationConfiguration()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationConfiguration"/> class
        /// with the specified properties.
        /// </summary>
        /// <param name="label">The friendly name of the notification. If this value is <see langword="null"/>, the underlying property will be omitted from the JSON representation of the object.</param>
        /// <param name="notificationTypeId">The notification type ID. This is obtained from <see cref="NotificationType.Id">NotificationType.Id</see>, or from the predefined values in <see cref="NotificationTypeId"/>. If this value is <see langword="null"/>, the underlying property will be omitted from the JSON representation of the object.</param>
        /// <param name="details">A <see cref="NotificationDetails"/> object containing the detailed configuration properties for the specified notification type. If this value is <see langword="null"/>, the underlying property will be omitted from the JSON representation of the object.</param>
        /// <param name="metadata">A collection of metadata to associate with the notification. If the value is <see langword="null"/>, no custom metadata is associated with the notification. If this value is <see langword="null"/>, the underlying property will be omitted from the JSON representation of the object.</param>
        /// <exception cref="ArgumentException">
        /// If <paramref name="label"/> is empty.
        /// <para>-or-</para>
        /// <para>If <paramref name="details"/> is non-<see langword="null"/> and <paramref name="notificationTypeId"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="details"/> does not support notifications of type <paramref name="notificationTypeId"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="metadata"/> contains any empty keys.</para>
        /// </exception>
        protected NotificationConfiguration(string label, NotificationTypeId notificationTypeId, NotificationDetails details, IDictionary<string, string> metadata)
        {
            if (label == string.Empty)
                throw new ArgumentException("label cannot be empty");
            if (details != null && notificationTypeId == null)
                throw new ArgumentException("notificationTypeId must be specified if details is specified", "notificationTypeId");
            if (details != null && !details.SupportsNotificationType(notificationTypeId))
                throw new ArgumentException(string.Format("The notification details object does not support '{0}' notifications.", notificationTypeId), "details");
            if (metadata != null && metadata.ContainsKey(string.Empty))
                throw new ArgumentException("metadata cannot contain any empty keys", "metadata");

            _label = label;
            _type = notificationTypeId;
            _details = details != null ? JObject.FromObject(details) : null;
            _metadata = metadata;
        }

        /// <summary>
        /// Gets the friendly name of the notification.
        /// </summary>
        public string Label
        {
            get
            {
                return _label;
            }
        }

        /// <summary>
        /// Gets the ID of the notification type to send.
        /// </summary>
        public NotificationTypeId Type
        {
            get
            {
                return _type;
            }
        }

        /// <summary>
        /// Gets the detailed configuration properties for the notification.
        /// </summary>
        public NotificationDetails Details
        {
            get
            {
                if (_details == null)
                    return null;

                return NotificationDetails.FromJObject(Type, _details);
            }
        }

        /// <summary>
        /// Gets a collection of metadata associated with the notification.
        /// </summary>
        public ReadOnlyDictionary<string, string> Metadata
        {
            get
            {
                if (_metadata == null)
                    return null;

                return new ReadOnlyDictionary<string, string>(_metadata);
            }
        }
    }
}
