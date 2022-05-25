namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using Newtonsoft.Json;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This class models the JSON representation of the results of testing a notification.
    /// </summary>
    /// <seealso cref="IMonitoringService.TestNotificationAsync"/>
    /// <seealso cref="IMonitoringService.TestExistingNotificationAsync"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class NotificationData : ExtensibleJsonObject
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value {value}
        /// <summary>
        /// This is the backing field for the <see cref="Status"/> property.
        /// </summary>
        [JsonProperty("status")]
        private string _status;

        /// <summary>
        /// This is the backing field for the <see cref="Message"/> property.
        /// </summary>
        [JsonProperty("message")]
        private string _message;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationData"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected NotificationData()
        {
        }

        /// <summary>
        /// Gets the result of testing the notification.
        /// </summary>
        public string Status
        {
            get
            {
                return _status;
            }
        }

        /// <summary>
        /// Gets a message describing the result of testing the notification.
        /// </summary>
        public string Message
        {
            get
            {
                return _message;
            }
        }
    }
}
