using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenStack.Serialization;

namespace OpenStack.Compute.v2_1.Operator
{
    /// <summary>
    /// Compute service quotas.
    /// </summary>
    [JsonConverterWithConstructor(typeof (RootWrapperConverter), "quota_set")]
    public class ServiceQuotas : IHaveExtraData
    {
        /// <summary>
        /// Number of content bytes allowed per injected file.
        /// </summary>
        [JsonProperty("injected_file_content_bytes")]
        public int InjectedFileContentSize { get; set; }

        /// <summary>
        /// Number of metadata items allowed per instance.
        /// </summary>
        [JsonProperty("metadata_items")]
        public int MetadataItems { get; set; }

        /// <summary>
        /// Number of members per server group.
        /// </summary>
        [JsonProperty("server_group_members")]
        public int ServerGroupMembers { get; set; }

        /// <summary>
        ///  Number of server groups per tenant.
        /// </summary>
        [JsonProperty("server_groups")]
        public int ServerGroups { get; set; }

        /// <summary>
        /// Megabytes of instance ram allowed per tenant.
        /// </summary>
        [JsonProperty("ram")]
        public int MemorySize { get; set; }

        /// <summary>
        /// Number of floating IP addresses allowed per tenant.
        /// </summary>
        [JsonProperty("floating_ips")]
        public int FloatingIPs { get; set; }

        /// <summary>
        /// Number of key pairs allowed per user.
        /// </summary>
        [JsonProperty("key_pairs")]
        public int KeyPairs { get; set; }

        /// <summary>
        /// The quota set identifier.
        /// </summary>
        [JsonProperty("id")]
        public Identifier Id { get; set; }

        /// <summary>
        /// Number of instances allowed per tenant.
        /// </summary>
        [JsonProperty("instances")]
        public int Instances { get; set; }

        /// <summary>
        /// Number of rules per security group.
        /// </summary>
        [JsonProperty("security_group_rules")]
        public int SecurityGroupRules { get; set; }

        /// <summary>
        /// Number of injected files allowed per tenant.
        /// </summary>
        [JsonProperty("injected_files")]
        public int InjectedFiles { get; set; }

        /// <summary>
        /// Number of instance cores (VCPUs) allowed per tenant.
        /// </summary>
        [JsonProperty("cores")]
        public int Cores { get; set; }

        /// <summary>
        /// Number of fixed IP addresses allowed per tenant. This number must be equal to or greater than the number of allowed instances.
        /// </summary>
        [JsonProperty("fixed_ips")]
        public int FixedIPs { get; set; }

        /// <summary>
        /// Number of content bytes allowed per injected file.
        /// </summary>
        [JsonProperty("injected_file_path_bytes")]
        public int InjectedFilePathSize { get; set; }

        /// <summary>
        /// Number of security groups per tenant.
        /// </summary>
        [JsonProperty("security_groups")]
        public int SecurityGroups { get; set; }

        /// <summary />
        [JsonExtensionData]
        IDictionary<string, JToken> IHaveExtraData.Data { get; set; } = new Dictionary<string, JToken>();
    }
}
