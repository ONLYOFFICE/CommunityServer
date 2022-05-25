namespace net.openstack.Providers.Rackspace.Objects.Databases
{
    using Newtonsoft.Json;

    /// <summary>
    /// This class models the JSON representation of a database user in the <see cref="IDatabaseService"/>.
    /// </summary>
    /// <seealso cref="IDatabaseService.ListDatabaseUsersAsync"/>
    /// <seealso cref="IDatabaseService.GetUserAsync"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class DatabaseUser : UserConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseUser"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected DatabaseUser()
        {
        }
    }
}
