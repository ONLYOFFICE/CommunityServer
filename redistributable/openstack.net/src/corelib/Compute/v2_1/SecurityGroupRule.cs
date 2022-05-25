using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenStack.Compute.v2_1.Serialization;
using OpenStack.Serialization;

namespace OpenStack.Compute.v2_1
{
    /// <summary>
    /// A security group rule.
    /// </summary>
    [JsonConverterWithConstructor(typeof(RootWrapperConverter), "security_group_rule")]
    public class SecurityGroupRule : IHaveExtraData, IServiceResource
    {
        /// <summary>
        /// The rule identifier.
        /// </summary>
        [JsonProperty("id")]
        public Identifier Id { get; set; }

        /// <summary>
        /// The IP protocol.
        /// </summary>
        [JsonProperty("ip_protocol")]
        public IPProtocol Protocol { get; set; }

        /// <summary>
        /// The port at start of range.
        /// </summary>
        [JsonProperty("from_port")]
        public int FromPort { get; set; }

        /// <summary>
        /// The port at end of range.
        /// </summary>
        [JsonProperty("to_port")]
        public int ToPort { get; set; }

        /// <summary>
        /// The CIDR for address range.
        /// </summary>
        [JsonIgnore]
        public string CIDR
        {
            get { return _ipRange.CIDR; }
            set { _ipRange.CIDR = value; }
        }

        [JsonProperty("ip_range")]
        private CIDRWrapper _ipRange = new CIDRWrapper();

        /// <summary>
        /// The associated security group identifier.
        /// </summary>
        [JsonProperty("parent_group_id")]
        public Identifier GroupId { get; set; }

        /// <summary>
        /// The associated source group identifier.
        /// </summary>
        [JsonProperty("group_id")]
        public Identifier SourceGroupId { get; set; }

        /// <summary />
        [JsonExtensionData]
        IDictionary<string, JToken> IHaveExtraData.Data { get; set; } = new Dictionary<string, JToken>();

        object IServiceResource.Owner { get; set; }

        /// <inheritdoc cref="ComputeApi.DeleteSecurityGroupRuleAsync" />
        /// <exception cref="InvalidOperationException">When this instance was not constructed by the <see cref="ComputeService"/>, as it is missing the appropriate internal state to execute service calls.</exception>
        public Task DeleteAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var owner = this.GetOwnerOrThrow<ComputeApi>();
            return owner.DeleteSecurityGroupRuleAsync(Id, cancellationToken);
        }

        private class CIDRWrapper
        {
            [JsonProperty("cidr")]
            public string CIDR { get; set; }
        }
    }
}