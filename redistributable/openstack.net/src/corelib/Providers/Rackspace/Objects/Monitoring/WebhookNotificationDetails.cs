namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// This class extends <see cref="NotificationDetails"/> with properties specific to
    /// <see cref="NotificationTypeId.Webhook"/> notifications.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class WebhookNotificationDetails : NotificationDetails
    {
        /// <summary>
        /// This is the backing field for the <see cref="Url"/> property.
        /// </summary>
        [JsonProperty("url")]
        private string _url;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebhookNotificationDetails"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected WebhookNotificationDetails()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebhookNotificationDetails"/> class
        /// with the specified target URI.
        /// </summary>
        /// <param name="url">The URI of the webhook to notify.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="url"/> is <see langword="null"/>.</exception>
        public WebhookNotificationDetails(Uri url)
        {
            if (url == null)
                throw new ArgumentNullException("url");

            _url = url.ToString();
        }

        /// <summary>
        /// Gets the URI a POST request will be sent to for this notification.
        /// </summary>
        public Uri Url
        {
            get
            {
                if (_url == null)
                    return null;

                return new Uri(_url);
            }
        }

        /// <inheritdoc/>
        /// <remarks>
        /// This class only supports <see cref="NotificationTypeId.Webhook"/> notifications.
        /// </remarks>
        protected internal override bool SupportsNotificationType(NotificationTypeId notificationTypeId)
        {
            return notificationTypeId == NotificationTypeId.Webhook;
        }
    }
}
