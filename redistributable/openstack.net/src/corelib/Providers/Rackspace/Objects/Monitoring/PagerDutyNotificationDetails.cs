namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// This class extends <see cref="NotificationDetails"/> with properties specific to
    /// <see cref="NotificationTypeId.PagerDuty"/> notifications.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class PagerDutyNotificationDetails : NotificationDetails
    {
        /// <summary>
        /// This is the backing field for the <see cref="ServiceKey"/> property.
        /// </summary>
        [JsonProperty("service_key")]
        private string _serviceKey;

        /// <summary>
        /// Initializes a new instance of the <see cref="PagerDutyNotificationDetails"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected PagerDutyNotificationDetails()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PagerDutyNotificationDetails"/> class
        /// with the specified service key.
        /// </summary>
        /// <param name="serviceKey">The PagerDuty service key to use for sending notifications.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="serviceKey"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="serviceKey"/> is empty.</exception>
        public PagerDutyNotificationDetails(string serviceKey)
        {
            if (serviceKey == null)
                throw new ArgumentNullException("serviceKey");
            if (string.IsNullOrEmpty(serviceKey))
                throw new ArgumentException("serviceKey cannot be empty");

            _serviceKey = serviceKey;
        }

        /// <summary>
        /// Gets the PagerDuty service key to use for sending notifications.
        /// </summary>
        public string ServiceKey
        {
            get
            {
                return _serviceKey;
            }
        }

        /// <inheritdoc/>
        /// <remarks>
        /// This class only supports <see cref="NotificationTypeId.PagerDuty"/> notifications.
        /// </remarks>
        protected internal override bool SupportsNotificationType(NotificationTypeId notificationTypeId)
        {
            return notificationTypeId == NotificationTypeId.PagerDuty;
        }
    }
}
