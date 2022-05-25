namespace net.openstack.Providers.Rackspace.Objects.Databases
{
    using Newtonsoft.Json;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This class models the JSON representation of a root user for a database instance.
    /// </summary>
    /// <seealso cref="IDatabaseService.EnableRootUserAsync"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class RootUser : ExtensibleJsonObject
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value {value}
        /// <summary>
        /// This is the backing field for the <see cref="Name"/> property.
        /// </summary>
        [JsonProperty("name")]
        private string _name;

        /// <summary>
        /// This is the backing field for the <see cref="Password"/> property.
        /// </summary>
        [JsonProperty("password")]
        private string _password;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="RootUser"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected RootUser()
        {
        }

        /// <summary>
        /// Gets the username of the database instance root user.
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
        }

        /// <summary>
        /// Gets the password for the database instance root user.
        /// </summary>
        public string Password
        {
            get
            {
                return _password;
            }
        }
    }
}
