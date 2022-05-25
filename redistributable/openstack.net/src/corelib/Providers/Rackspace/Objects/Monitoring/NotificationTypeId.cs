namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System;
    using net.openstack.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the unique identifier of a notification type in the <see cref="IMonitoringService"/>.
    /// </summary>
    /// <seealso cref="NotificationType.Id"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonConverter(typeof(NotificationTypeId.Converter))]
    public sealed class NotificationTypeId : ResourceIdentifier<NotificationTypeId>
    {
        /// <summary>
        /// This is the backing field for the <see cref="Webhook"/> property.
        /// </summary>
        private static readonly NotificationTypeId _webhook = new NotificationTypeId("webhook");

        /// <summary>
        /// This is the backing field for the <see cref="Email"/> property.
        /// </summary>
        private static readonly NotificationTypeId _email = new NotificationTypeId("email");

        /// <summary>
        /// This is the backing field for the <see cref="PagerDuty"/> property.
        /// </summary>
        private static readonly NotificationTypeId _pagerduty = new NotificationTypeId("pagerduty");

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationTypeId"/> class
        /// with the specified identifier value.
        /// </summary>
        /// <param name="id">The notification type identifier value.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="id"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="id"/> is empty.</exception>
        public NotificationTypeId(string id)
            : base(id)
        {
        }

        /// <summary>
        /// Gets a <see cref="NotificationTypeId"/> instance representing a webhook notification.
        /// </summary>
        public static NotificationTypeId Webhook
        {
            get
            {
                return _webhook;
            }
        }

        /// <summary>
        /// Gets a <see cref="NotificationTypeId"/> instance representing an email notification.
        /// </summary>
        public static NotificationTypeId Email
        {
            get
            {
                return _email;
            }
        }

        /// <summary>
        /// Gets a <see cref="NotificationTypeId"/> instance representing a PagerDuty notification.
        /// </summary>
        public static NotificationTypeId PagerDuty
        {
            get
            {
                return _pagerduty;
            }
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="NotificationTypeId"/>
        /// objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        private sealed class Converter : ConverterBase
        {
            /// <inheritdoc/>
            protected override NotificationTypeId FromValue(string id)
            {
                return new NotificationTypeId(id);
            }
        }
    }
}
