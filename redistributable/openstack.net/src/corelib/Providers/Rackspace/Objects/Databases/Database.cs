namespace net.openstack.Providers.Rackspace.Objects.Databases
{
    using Newtonsoft.Json;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This class models the JSON representation of a database resource in the <see cref="IDatabaseService"/>.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class Database : ExtensibleJsonObject
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value {value}
        /// <summary>
        /// This is the backing field for the <see cref="Name"/> property.
        /// </summary>
        [JsonProperty("name")]
        private DatabaseName _name;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="Database"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected Database()
        {
        }

        /// <summary>
        /// Gets the name of the database.
        /// </summary>
        public DatabaseName Name
        {
            get
            {
                return _name;
            }
        }
    }
}
