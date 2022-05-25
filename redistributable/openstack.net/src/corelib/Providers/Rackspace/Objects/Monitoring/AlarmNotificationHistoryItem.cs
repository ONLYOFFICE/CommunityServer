namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System;
    using System.Collections.ObjectModel;
    using Newtonsoft.Json;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This class models the JSON representation of an alarm notification history resource in the <see cref="IMonitoringService"/>.
    /// </summary>
    /// <remarks>
    /// The monitoring service keeps a record of notifications sent for each alarm.
    /// This history is further subdivided by the check on which the notification
    /// occurred. Every attempt to send a notification is recorded, making this
    /// history a valuable tool in diagnosing issues with unreceived notifications,
    /// in addition to offering a means of viewing the history of an alarm's
    /// statuses.
    /// </remarks>
    /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-alarm-notification-history.html">Alarm Notification History (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class AlarmNotificationHistoryItem : ExtensibleJsonObject
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value {value}
        /// <summary>
        /// This is the backing field for the <see cref="Id"/> property.
        /// </summary>
        [JsonProperty("id")]
        private AlarmNotificationHistoryItemId _id;

        /// <summary>
        /// This is the backing field for the <see cref="Timestamp"/> property.
        /// </summary>
        [JsonProperty("timestamp")]
        private long? _timestamp;

        /// <summary>
        /// This is the backing field for the <see cref="NotificationPlanId"/> property.
        /// </summary>
        [JsonProperty("notification_plan_id")]
        private NotificationPlanId _notificationPlanId;

        /// <summary>
        /// This is the backing field for the <see cref="TransactionId"/> property.
        /// </summary>
        [JsonProperty("transaction_id")]
        private TransactionId _transactionId;

        /// <summary>
        /// This is the backing field for the <see cref="Status"/> property.
        /// </summary>
        [JsonProperty("status")]
        private string _status;

        /// <summary>
        /// This is the backing field for the <see cref="State"/> property.
        /// </summary>
        [JsonProperty("state")]
        private AlarmState _state;

        /// <summary>
        /// This is the backing field for the <see cref="PreviousState"/> property.
        /// </summary>
        [JsonProperty("previous_state")]
        private AlarmState _previousState;

        /// <summary>
        /// This is the backing field for the <see cref="Results"/> property.
        /// </summary>
        [JsonProperty("notification_results")]
        private NotificationResult[] _results;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="AlarmNotificationHistoryItem"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected AlarmNotificationHistoryItem()
        {
        }

        /// <summary>
        /// Gets the unique identifier of the alarm notification history item.
        /// </summary>
        public AlarmNotificationHistoryItemId Id
        {
            get
            {
                return _id;
            }
        }

        /// <summary>
        /// Gets the timestamp associated with this history item.
        /// </summary>
        public DateTimeOffset? Timestamp
        {
            get
            {
                return DateTimeOffsetExtensions.ToDateTimeOffset(_timestamp);
            }
        }

        /// <summary>
        /// Gets the ID of the notification plan associated with the alarm.
        /// </summary>
        public NotificationPlanId NotificationPlanId
        {
            get
            {
                return _notificationPlanId;
            }
        }

        /// <summary>
        /// Gets the transaction ID for the history item.
        /// </summary>
        public TransactionId TransactionId
        {
            get
            {
                return _transactionId;
            }
        }

        /// <summary>
        /// Gets the status of the history item.
        /// </summary>
        public string Status
        {
            get
            {
                return _status;
            }
        }

        /// <summary>
        /// Gets the state of the alarm after this history item.
        /// </summary>
        public AlarmState State
        {
            get
            {
                return _state;
            }
        }

        /// <summary>
        /// Gets the state of the alarm before this history item.
        /// </summary>
        public AlarmState PreviousState
        {
            get
            {
                return _previousState;
            }
        }

        /// <summary>
        /// Gets a collection of <see cref="NotificationResult"/> objects describing
        /// the notifications sent by this history item.
        /// </summary>
        public ReadOnlyCollection<NotificationResult> Results
        {
            get
            {
                if (_results == null)
                    return null;

                return new ReadOnlyCollection<NotificationResult>(_results);
            }
        }
    }
}
