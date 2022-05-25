namespace net.openstack.Core.Domain
{
    using System.Diagnostics;
    using Newtonsoft.Json;

    /// <summary>
    /// A personality that a user assumes when performing a specific set of operations. A role
    /// includes a set of right and privileges. A user assuming that role inherits those rights
    /// and privileges.
    /// </summary>
    /// <remarks>
    /// In OpenStack Identity Service, a token that is issued to a user includes the list of
    /// roles that user can assume. Services that are being called by that user determine how
    /// they interpret the set of roles a user has and to which operations or resources each
    /// role grants access.
    /// </remarks>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    [DebuggerDisplay("{Name,nq} ({Id, nq})")]
    public class Role : ExtensibleJsonObject
    {
        /// <summary>
        /// Gets the unique identifier for the role.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; private set; }

        /// <summary>
        /// Gets the name of the role.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; private set; }

        /// <summary>
        /// Gets a description of the role, if one is provided.
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Role"/> class with
        /// the specified name and description.
        /// </summary>
        /// <param name="name">The name of the role.</param>
        /// <param name="description">The description of the role.</param>
        public Role(string name, string description)
        {
            Name = name;
            Description = description;
        }
    }
}
