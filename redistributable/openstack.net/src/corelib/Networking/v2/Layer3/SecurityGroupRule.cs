using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenStack.Serialization;

namespace OpenStack.Networking.v2.Layer3
{
    /// <summary>
    /// 
    /// </summary>
    [JsonConverterWithConstructor(typeof(RootWrapperConverter), "security_group_rule")]
    public class SecurityGroupRule : IHaveExtraData, IServiceResource
    {
        /// <summary>
        /// ingress or egress: the direction in which the security group rule is applied.
        /// For a compute instance, an ingress security group rule is applied to incoming (ingress) traffic for that instance. 
        /// An egress rule is applied to traffic leaving the instance.
        /// </summary>
        [JsonProperty("direction")]
        public TrafficDirection Direction;

        /// <summary>
        /// The internet protocol version. Addresses represented in CIDR must match the ingress or egress rules. 
        /// </summary>
        [JsonProperty("ethertype")]
        public IPVersion Ethertype;

        /// <summary>
        /// The UUID of the security group rule.
        /// </summary>
        [JsonProperty("id")]
        public Identifier Id;

        /// <summary>
        /// The minimum port number in the range that is matched by the security group rule.
        /// If the protocol is TCP or UDP, this value must be less than or equal to the port_range_max attribute value.
        /// If the protocol is ICMP, this value must be an ICMP type.
        /// </summary>
        [JsonProperty("port_range_min")]
        public int MinPort { get; set; }

        /// <summary>
        /// The maximum port number in the range that is matched by the security group rule. 
        /// The port_range_min attribute constrains the port_range_max attribute.
        /// If the protocol is ICMP, this value must be an ICMP type. 
        /// </summary>
        [JsonProperty("port_range_max")]
        public int MaxPort { get; set; }

        /// <summary>
        /// The protocol that is matched by the security group rule.
        /// </summary>
        [JsonProperty("protocol")]
        public IPProtocol Protocol;

        /// <summary>
        /// The remote group UUID to associate with this security group rule.
        /// You can specify either the remote_group_id or remote_ip_prefix attribute in the request body. 
        /// </summary>
        [JsonProperty("remote_group_id")]
        public Identifier RemoteGroupId;

        /// <summary>
        /// The remote IP prefix or CIDR to associate with this security group rule.
        /// You can specify either the remote_group_id or remote_ip_prefix attribute in the request body.
        /// This attribute value matches the IP prefix as the source IP address of the IP packet. 
        /// </summary>
        [JsonProperty("remote_ip_prefix")]
        public string RemoteCIDR;

        /// <summary>
        /// The UUId of security group 
        /// </summary>
        [JsonProperty("security_group_id")]
        public Identifier SecurityGroupId;

        [JsonExtensionData]
        IDictionary<string, JToken> IHaveExtraData.Data { get; set; } = new Dictionary<string, JToken>();

        object IServiceResource.Owner { get; set; }
    }
}
