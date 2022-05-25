namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using Newtonsoft.Json;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This class represents a single field associated with a notification type in the <see cref="IMonitoringService"/>.
    /// </summary>
    /// <seealso cref="NotificationType.Fields"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class NotificationTypeField : ExtensibleJsonObject
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value {value}
        /// <summary>
        /// This is the backing field for the <see cref="Name"/> property.
        /// </summary>
        [JsonProperty("name")]
        private string _name;

        /// <summary>
        /// This is the backing field for the <see cref="Description"/> property.
        /// </summary>
        [JsonProperty("description")]
        private string _description;

        /// <summary>
        /// This is the backing field for the <see cref="Optional"/> property.
        /// </summary>
        [JsonProperty("optional")]
        private bool? _optional;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationTypeField"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected NotificationTypeField()
        {
        }

        /// <summary>
        /// Gets the name of the field.
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
        }

        /// <summary>
        /// Gets a description of the field.
        /// </summary>
        public string Description
        {
            get
            {
                return _description;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the field value is optional when creating a notification of the associated type
        /// </summary>
        public bool? Optional
        {
            get
            {
                return _optional;
            }
        }
    }
}
