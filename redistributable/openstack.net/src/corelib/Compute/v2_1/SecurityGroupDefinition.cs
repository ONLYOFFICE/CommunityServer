using Newtonsoft.Json;
using OpenStack.Serialization;

namespace OpenStack.Compute.v2_1
{
    /// <summary>
    /// Defines a new security group.
    /// </summary>
    [JsonConverterWithConstructor(typeof(RootWrapperConverter), "security_group")]
    public class SecurityGroupDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityGroupDefinition"/> class.
        /// </summary>
        /// <param name="name">The security group name.</param>
        /// <param name="description">The security group description.</param>
        public SecurityGroupDefinition(string name, string description)
        {
            Name = name;
            Description = description;
        }

        /// <inheritdoc cref="SecurityGroupReference.Name"/>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <inheritdoc cref="SecurityGroup.Description"/>
        [JsonProperty("description")]
        public string Description { get; set; }
    }
}