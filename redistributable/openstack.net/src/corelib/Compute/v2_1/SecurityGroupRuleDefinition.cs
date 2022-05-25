using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenStack.Serialization;

namespace OpenStack.Compute.v2_1
{
    /// <summary>
    /// Defines a new security group rule.
    /// </summary>
    [JsonConverterWithConstructor(typeof(RootWrapperConverter), "security_group_rule")]
    public class SecurityGroupRuleDefinition : IHaveExtraData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityGroupRuleDefinition"/> class.
        /// </summary>
        /// <param name="protocol">The IP protocol.</param>
        /// <param name="fromPort">The source port.</param>
        /// <param name="toPort">To destination port.</param>
        /// <param name="cidr">The CIDR.</param>
        public SecurityGroupRuleDefinition(IPProtocol protocol, int fromPort, int toPort, string cidr)
        {
            Protocol = protocol;
            FromPort = fromPort;
            ToPort = toPort;
            CIDR = cidr;
        }

        /// <inheritdoc cref="SecurityGroupRule.Protocol"/>
        [JsonProperty("ip_protocol")]
        public IPProtocol Protocol { get; set; }

        /// <inheritdoc cref="SecurityGroupRule.FromPort"/>
        [JsonProperty("from_port")]
        public int FromPort { get; set; }

        /// <inheritdoc cref="SecurityGroupRule.ToPort"/>
        [JsonProperty("to_port")]
        public int ToPort { get; set; }

        /// <inheritdoc cref="SecurityGroupRule.CIDR"/>
        [JsonProperty("cidr")]
        public string CIDR { get; set; }

        /// <inheritdoc cref="SecurityGroupRule.GroupId"/>
        [JsonProperty("parent_group_id")]
        public Identifier GroupId { get; set; }

        /// <inheritdoc cref="SecurityGroupRule.SourceGroupId"/>
        [JsonProperty("group_id")]
        public Identifier SourceGroupId { get; set; }

        [JsonExtensionData]
        IDictionary<string, JToken> IHaveExtraData.Data { get; set; } = new Dictionary<string, JToken>();
    }
}