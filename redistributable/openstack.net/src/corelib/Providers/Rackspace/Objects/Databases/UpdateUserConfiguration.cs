namespace net.openstack.Providers.Rackspace.Objects.Databases
{
    using Newtonsoft.Json;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This class models the JSON representation of an operation to update properties
    /// of a <see cref="DatabaseUser"/> in the <see cref="IDatabaseService"/>.
    /// </summary>
    /// <seealso cref="IDatabaseService.UpdateUserAsync"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class UpdateUserConfiguration : ExtensibleJsonObject
    {
        /// <summary>
        /// This is one of the backing fields for the <see cref="UserName"/> property.
        /// </summary>
        [JsonProperty("name", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private string _name;

        /// <summary>
        /// This is the backing field for the <see cref="Password"/> property.
        /// </summary>
        [JsonProperty("password", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private string _password;

        /// <summary>
        /// This is one of the backing fields for the <see cref="UserName"/> property.
        /// </summary>
        [JsonProperty("host", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private string _host;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateUserConfiguration"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected UpdateUserConfiguration()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateUserConfiguration"/> class
        /// with the specified values.
        /// </summary>
        /// <param name="name">A <see cref="UserName"/> object providing the new name and host for the user. If this value is <see langword="null"/>, the name and host address for the database user is not changed.</param>
        /// <param name="password">The new password for the user. If this value is <see langword="null"/>, the existing password for the database user is not changed.</param>
        public UpdateUserConfiguration(UserName name, string password)
        {
            if (name != null)
            {
                _name = name.Name;
                _host = name.Host;
            }

            _password = password;
        }

        /// <summary>
        /// Gets a <see cref="UserName"/> object containing the updated username and host address for the database user.
        /// </summary>
        /// <value>
        /// A <see cref="UserName"/> object containing the new username and host address for the existing database user, or <see langword="null"/> if the username and password for the existing database user should not be changed.
        /// </value>
        public UserName UserName
        {
            get
            {
                if (_host == null)
                {
                    if (_name == null)
                        return null;

                    return new UserName(_name);
                }

                return new UserName(_name, _host);
            }
        }

        /// <summary>
        /// Gets the updated password for the database user.
        /// </summary>
        /// <value>
        /// The new password for the database user, or <see langword="null"/> if the password for the user should not be changed.
        /// </value>
        public string Password
        {
            get
            {
                return _password;
            }
        }
    }
}
