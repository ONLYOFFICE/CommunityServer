namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System.Collections.ObjectModel;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This class models the JSON representation of the result of sending a monitoring
    /// notification.
    /// </summary>
    /// <seealso cref="AlarmNotificationHistoryItem.Results"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class NotificationResult : ExtensibleJsonObject
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value {value}
        /// <summary>
        /// This is the backing field for the <see cref="NotificationId"/> property.
        /// </summary>
        [JsonProperty("notification_id")]
        private NotificationId _notificationId;

        /// <summary>
        /// This is the backing field for the <see cref="NotificationTypeId"/> property.
        /// </summary>
        [JsonProperty("notification_type")]
        private NotificationTypeId _notificationTypeId;

        /// <summary>
        /// This is the backing field for the <see cref="NotificationDetails"/> property.
        /// </summary>
        [JsonProperty("notification_details")]
        private JObject _notificationDetails;

        /// <summary>
        /// This is the backing field for the <see cref="InProgress"/> property.
        /// </summary>
        [JsonProperty("in_progress")]
        private bool? _inProgress;

        /// <summary>
        /// This is the backing field for the <see cref="Message"/> property.
        /// </summary>
        [JsonProperty("message")]
        private string _message;

        /// <summary>
        /// This is the backing field for the <see cref="Success"/> property.
        /// </summary>
        [JsonProperty("success")]
        private bool? _success;

        /// <summary>
        /// This is the backing field for the <see cref="Attempts"/> property.
        /// </summary>
        [JsonProperty("attempts")]
        private NotificationAttempt[] _attempts;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationResult"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected NotificationResult()
        {
        }

        /// <summary>
        /// Gets the ID of the notification which was sent as a result of this alarm
        /// change.
        /// </summary>
        public NotificationId NotificationId
        {
            get
            {
                return _notificationId;
            }
        }

        /// <summary>
        /// Gets the ID of the notification type of the notification which was sent.
        /// </summary>
        public NotificationTypeId NotificationTypeId
        {
            get
            {
                return _notificationTypeId;
            }
        }

        /// <summary>
        /// Gets the detailed configuration information for the notification which was sent.
        /// </summary>
        public NotificationDetails NotificationDetails
        {
            get
            {
                if (_notificationDetails == null)
                    return null;

                return NotificationDetails.FromJObject(NotificationTypeId, _notificationDetails);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the notification is currently in progress.
        /// </summary>
        public bool? InProgress
        {
            get
            {
                return _inProgress;
            }
        }

        /// <summary>
        /// Gets a message describing the result of the notification operation.
        /// </summary>
        public string Message
        {
            get
            {
                return _message;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the notification was successful.
        /// </summary>
        public bool? Success
        {
            get
            {
                return _success;
            }
        }

        /// <summary>
        /// Gets a collection of <see cref="NotificationAttempt"/> objects describing the
        /// results of individual attempts to sent the notification.
        /// </summary>
        public ReadOnlyCollection<NotificationAttempt> Attempts
        {
            get
            {
                if (_attempts == null)
                    return null;

                return new ReadOnlyCollection<NotificationAttempt>(_attempts);
            }
        }
    }
}
