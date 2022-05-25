namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using Newtonsoft.Json;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This class models the JSON representation of the detailed information
    /// describing an attempt to send a notification.
    /// </summary>
    /// <seealso cref="NotificationResult.Attempts"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class NotificationAttempt : ExtensibleJsonObject
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value {value}
        /// <summary>
        /// This is the backing field for the <see cref="Message"/> property.
        /// </summary>
        [JsonProperty("message")]
        private string _message;

        /// <summary>
        /// This is the backing field for the <see cref="From"/> property.
        /// </summary>
        [JsonProperty("from")]
        private string _from;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationAttempt"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected NotificationAttempt()
        {
        }

        /// <summary>
        /// Gets a message describing the result of the notification attempt.
        /// </summary>
        public string Message
        {
            get
            {
                return _message;
            }
        }

        /// <summary>
        /// Gets the name of the system responsible for the notification attempt.
        /// </summary>
        public string From
        {
            get
            {
                return _from;
            }
        }
    }
}
