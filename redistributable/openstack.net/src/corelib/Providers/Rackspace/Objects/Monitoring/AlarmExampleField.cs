namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using Newtonsoft.Json;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This class models the JSON representation of a field in an Alarm Example <see cref="AlarmExample.Criteria"/> template.
    /// </summary>
    /// <seealso cref="AlarmExample.Fields"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class AlarmExampleField : ExtensibleJsonObject
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
        /// This is the backing field for the <see cref="Type"/> property.
        /// </summary>
        [JsonProperty("type")]
        private string _type;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="AlarmExampleField"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected AlarmExampleField()
        {
        }

        /// <summary>
        /// Gets the field name.
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
        /// Gets the type of data stored in the field.
        /// </summary>
        public string Type
        {
            get
            {
                return _type;
            }
        }
    }
}
