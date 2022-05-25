using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenStack.Serialization;

namespace OpenStack.Compute.v2_1
{
    /// <summary>
    /// Set of account limits for the compute service.
    /// </summary>
    [JsonConverterWithConstructor(typeof(RootWrapperConverter), "limits")]
    public class ServiceLimits : IHaveExtraData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceLimits"/> class.
        /// </summary>
        public ServiceLimits()
        {
            RateLimits = new List<RateLimits>();
        }

        /// <summary>
        /// Fixed resource limits.
        /// </summary>
        [JsonProperty("absolute")]
        public ResourceLimits ResourceLimits { get; set; }

        /// <summary>
        /// Threshold limits for the compute service that are reset after a certain amount of time passes.
        /// </summary>
        [JsonProperty("rate")]
        public IList<RateLimits> RateLimits { get; set; }

        [JsonExtensionData]
        IDictionary<string, JToken> IHaveExtraData.Data { get; set; } = new Dictionary<string, JToken>();
    }

    /// <summary>
    /// Fixed resource limits for the compute service.
    /// </summary>
    public class ResourceLimits : IHaveExtraData
    {
        /// <summary>
        /// Maximum number of metadata items associated with a server.
        /// </summary>
        [JsonProperty("maxServerMeta")]
        public int? ServerMetadataMax { get; set; }

        /// <summary>
        /// The maximum number of file path/content pairs that can be supplied on server build.
        /// </summary>
        [JsonProperty("maxPersonality")]
        public int? PersonalityMax { get; set; }

        /// <summary>
        /// Maximum number of metadata items associated with an image.
        /// </summary>
        [JsonProperty("maxImageMeta")]
        public int? ImageMetadataMax { get; set; }

        /// <summary>
        /// The maximum size, in bytes, for each personality file.
        /// </summary>
        [JsonProperty("maxPersonalitySize")]
        public int? PersonalitySizeMax { get; set; }

        /// <summary>
        /// The maximum number of key pairs per server.
        /// </summary>
        [JsonProperty("maxTotalKeypairs")]
        public int? KeypairsMax { get; set; }

        /// <summary>
        /// The maximum number of security group rules per security group.
        /// </summary>
        [JsonProperty("maxSecurityGroupRules")]
        public int? SecurityGroupRulesMax { get; set; }

        /// <summary>
        /// The number of security group rules used.
        /// </summary>
        [JsonProperty("totalServerGroupsUsed")]
        public int? ServerGroupsUsed { get; set; }

        /// <summary>
        /// The maximun number of server groups per server.
        /// </summary>
        [JsonProperty("maxServerGroups")]
        public int? ServerGroupsMax { get; set; }

        /// <summary>
        /// The number of cores used.
        /// </summary>
        [JsonProperty("totalCoresUsed")]
        public int? CoresUsed { get; set; }

        /// <summary>
        /// The maximum number of cores.
        /// </summary>
        [JsonProperty("maxTotalCores")]
        public int? CoresMax { get; set; }

        /// <summary>
        /// The amount of RAM (MB) used.
        /// </summary>
        [JsonProperty("totalRAMUsed")]
        public int? MemoryUsed { get; set; }

        /// <summary>
        /// Maximum total amount of RAM (MB)
        /// </summary>
        [JsonProperty("maxTotalRAMSize")]
        public int? MemorySizeMax { get; set; }

        /// <summary>
        /// The number of server instances used.
        /// </summary>
        [JsonProperty("totalInstancesUsed")]
        public int? ServersUsed { get; set; }

        /// <summary>
        /// The maximum number of servers at any one time.
        /// </summary>
        [JsonProperty("maxTotalInstances")]
        public int? ServersMax { get; set; }

        /// <summary>
        /// The number of security groups used.
        /// </summary>
        [JsonProperty("totalSecurityGroupsUsed")]
        public int? SecurityGroupUsed { get; set; }

        /// <summary>
        /// The maximum number of security groups per server.
        /// </summary>
        [JsonProperty("maxSecurityGroups")]
        public int? SecurityGroupMax { get; set; }

        /// <summary>
        /// The number of floating IP addresses used.
        /// </summary>
        [JsonProperty("totalFloatingIpsUsed")]
        public int? FloatingIPUsed { get; set; }

        /// <summary>
        /// The maximum number of floating IP addresses.
        /// </summary>
        [JsonProperty("maxTotalFloatingIps")]
        public int? FloatingIPMax { get; set; }

        /// <summary>
        /// The maximum number of server group members per server group.
        /// </summary>
        [JsonProperty("maxServerGroupMembers")]
        public int? ServerGroupMemberMax { get; set; }
        
        /// <summary>
        /// Contains additional limits that may be defined by the cloud provider.
        /// </summary>
        [JsonExtensionData]
        public IDictionary<string, JToken> Data { get; set; } = new Dictionary<string, JToken>();
    }

    /// <summary>
    /// Threshold limits for the compute service that are reset after a certain amount of time passes.
    /// </summary>
    public class RateLimits
    {
        /// <summary>
        /// The service endpoint.
        /// </summary>
        [JsonProperty("uri")]
        public string Name { get; set; }

        /// <summary>
        /// The regular expression applied to the request URL.
        /// </summary>
        [JsonProperty("regex")]
        public Regex Regex { get; set; }

        /// <summary>
        /// The service endpoint rate limits.
        /// </summary>
        [JsonProperty("limit")]
        public IList<RateLimit> Limits { get; set; }
    }

    /// <summary>
    /// A rate limit for a service endpoint.
    /// </summary>
    public class RateLimit
    {
        /// <summary>
        /// The request verb, e.g. GET, POST
        /// </summary>
        [JsonProperty("verb")]
        public string HttpMethod { get; set; }

        /// <summary>
        /// The maximum number of requests allowed in the current time frame.
        /// </summary>
        [JsonProperty("value")]
        public int Maximum { get; set; }

        /// <summary>
        /// The number of remaining requests allowed in the current time frame.
        /// </summary>
        [JsonProperty("remaining")]
        public int Remaining { get; set; }

        /// <summary>
        /// The time frame unit of measurement.
        /// </summary>
        [JsonProperty("unit")]
        public string UnitOfMeasurement { get; set; }

        /// <summary>
        /// Specifies when the current time frame expires and the rate limits are reset.
        /// </summary>
        [JsonProperty("next-available")]
        public DateTimeOffset NextAvailable { get; set; }
    }
}