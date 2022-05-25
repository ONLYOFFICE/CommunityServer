namespace net.openstack.Providers.Rackspace.Objects
{
    using System;
    using Newtonsoft.Json;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// Represents the domain for a user account.
    /// </summary>
    /// <remarks>
    /// <note type="warning">
    /// This property is an undocumented Rackspace-specific extension to the OpenStack Identity Service.
    /// </note>
    /// </remarks>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    public class Domain : ExtensibleJsonObject
    {
        /// <summary>
        /// Gets the "name" property for the domain.
        /// <note type="warning">The value of this property is not defined. Do not use.</note>
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Domain"/> class with the specified domain.
        /// </summary>
        /// <param name="name">The domain name.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="name"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="name"/> is empty.</exception>
        public Domain(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name cannot be empty");

            Name = name;
        }
    }
}
