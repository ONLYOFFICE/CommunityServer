namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System.Collections.ObjectModel;
    using Newtonsoft.Json;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This class models the JSON representation of a notification type in the <see cref="IMonitoringService"/>.
    /// </summary>
    /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-notification-types-crud.html">Notification Types (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class NotificationType : ExtensibleJsonObject
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value {value}
        /// <summary>
        /// This is the backing field for the <see cref="Id"/> property.
        /// </summary>
        [JsonProperty("id")]
        private NotificationTypeId _id;

        /// <summary>
        /// This is the backing field for the <see cref="Fields"/> property.
        /// </summary>
        [JsonProperty("fields", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private NotificationTypeField[] _fields;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationType"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected NotificationType()
        {
        }

        /// <summary>
        /// Gets the unique identifier for the notification type.
        /// </summary>
        public NotificationTypeId Id
        {
            get
            {
                return _id;
            }
        }

        /// <summary>
        /// Gets a collection of <see cref="NotificationTypeField"/> objects describing the fields
        /// of this notification type.
        /// </summary>
        public ReadOnlyCollection<NotificationTypeField> Fields
        {
            get
            {
                if (_fields == null)
                    return null;

                return new ReadOnlyCollection<NotificationTypeField>(_fields);
            }
        }
    }
}
