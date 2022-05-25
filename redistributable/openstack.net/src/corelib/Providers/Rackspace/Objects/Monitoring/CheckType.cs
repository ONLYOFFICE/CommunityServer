namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System.Collections.ObjectModel;
    using Newtonsoft.Json;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// Represents the type of a check in the monitoring service.
    /// </summary>
    /// <seealso cref="CheckConfiguration.CheckTypeId"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class CheckType : ExtensibleJsonObject
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value {value}
        /// <summary>
        /// This is the backing field for the <see cref="Id"/> property.
        /// </summary>
        [JsonProperty("id")]
        private CheckTypeId _id;

        /// <summary>
        /// This is the backing field for the <see cref="Type"/> property.
        /// </summary>
        [JsonProperty("type")]
        private CheckTypeType _type;

        /// <summary>
        /// This is the backing field for the <see cref="Fields"/> property.
        /// </summary>
        [JsonProperty("fields")]
        private NotificationTypeField[] _fields;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckType"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected CheckType()
        {
        }

        /// <summary>
        /// Gets the unique identifier of the check type.
        /// </summary>
        public CheckTypeId Id
        {
            get
            {
                return _id;
            }
        }

        /// <summary>
        /// Gets the type of check type.
        /// </summary>
        public CheckTypeType Type
        {
            get
            {
                return _type;
            }
        }

        /// <summary>
        /// Gets a collection of <see cref="NotificationTypeField"/> objects describing the
        /// configurable properties of checks of this type.
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
