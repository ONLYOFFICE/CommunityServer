using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenStack.Serialization;

namespace OpenStack.Networking.v2.Layer3
{
    /// <summary>
    /// Represents the security group of the <see cref="NetworkingService"/> 
    /// </summary>
    [JsonConverterWithConstructor(typeof(RootWrapperConverter), "security_group")]
    public class SecurityGroup : IHaveExtraData, IServiceResource
    {
        /// <summary>
        /// The security group description
        /// </summary>
        [JsonProperty("description")]
        public string Description;

        /// <summary>
        /// The UUID of security group
        /// </summary>
        [JsonProperty("id")]
        public Identifier Id;

        /// <summary>
        /// The security group name
        /// </summary>
        [JsonProperty("name")]
        public string Name;

        /// <summary>
        /// A list of <see cref="SecurityGroup"/> objects.
        /// </summary>
        [JsonProperty("security_group_rules")]
        public IList<SecurityGroupRule> SecurityGroupRules;

        [JsonExtensionData]
        IDictionary<string, JToken> IHaveExtraData.Data { get; set; } = new Dictionary<string, JToken>();

        object IServiceResource.Owner { get; set; }
    }
}
