namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// This class extends <see cref="NotificationDetails"/> with properties specific to
    /// <see cref="NotificationTypeId.Email"/> notifications.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class EmailNotificationDetails : NotificationDetails
    {
        /// <summary>
        /// This is the backing field for the <see cref="Address"/> property.
        /// </summary>
        [JsonProperty("address")]
        private string _address;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailNotificationDetails"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected EmailNotificationDetails()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailNotificationDetails"/> class
        /// with the specified email address.
        /// </summary>
        /// <param name="address">The email address that should be notified.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="address"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="address"/> is empty.</exception>
        public EmailNotificationDetails(string address)
        {
            if (address == null)
                throw new ArgumentNullException("address");
            if (string.IsNullOrEmpty(address))
                throw new ArgumentException("address cannot be empty");

            _address = address;
        }

        /// <summary>
        /// Gets the email address notifications will be sent to.
        /// </summary>
        public string Address
        {
            get
            {
                return _address;
            }
        }

        /// <inheritdoc/>
        /// <remarks>
        /// This class only supports <see cref="NotificationTypeId.Email"/> notifications.
        /// </remarks>
        protected internal override bool SupportsNotificationType(NotificationTypeId notificationTypeId)
        {
            return notificationTypeId == NotificationTypeId.Email;
        }
    }
}
