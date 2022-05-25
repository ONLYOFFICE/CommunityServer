using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenStack.Serialization;

namespace OpenStack.Compute.v2_1
{
    /// <summary>
    /// Hints passed to the compute scheduler when creating a server.
    /// <para>These may be used by the scheduler to determine how and where the server instance is created.</para>
    /// <para>Custom hints can be set via the <see cref="Add"/> method.</para>
    /// </summary>
    /// <seealso href="http://docs.openstack.org/developer/nova/filter_scheduler.html"/>
    public class SchedulerHints : IHaveExtraData
    {
        /// <summary />
        public SchedulerHints()
        {
            DifferentHost = new List<string>();
            SameHost = new List<string>();
        }

        /// <summary>
        /// Specifies a custom filter by passing a scheduler hint in JSON format.
        /// </summary>
        /// <seealso href="http://docs.openstack.org/liberty/config-reference/content/section_compute-scheduler.html#jsonfilter"/>
        [JsonProperty("query")]
        public string Query { get; set; }

        /// <summary>
        /// Schedules the instance based on host IP subnet range. Specifies the CIDR that corresponds to the subnet.
        /// <para>Must also specify <see cref="BuildNearHostIP"/>.</para>
        /// </summary>
        [JsonProperty("build_near_host_ip")]
        public string BuildNearHostIP { get; set; }

        /// <summary>
        /// Schedules the instance based on host IP subnet range. Specifies the first IP address in the subnet.
        /// <para>Must also specify <see cref="BuildNearHostIP"/>.</para>
        /// </summary>
        [JsonProperty("cidr")]
        public string CIDR { get; set; }

        /// <summary>
        /// Schedules the instance on a different host from a set of instances.
        /// </summary>
        [JsonProperty("different_host")]
        public IList<string> DifferentHost { get; set; }

        /// <summary>
        /// Schedules the instance on the same host as another instance in a set of instances.
        /// </summary>
        [JsonProperty("same_host")]
        public IList<string> SameHost { get; set; }

        /// <summary>
        /// Specifies additional custom hints to pass to the scheduler.
        /// <para>Use the <see cref="Add"/> method instead of accessing this property directly.</para>
        /// </summary>
        [JsonExtensionData]
        public IDictionary<string, JToken> Data { get; set; } = new Dictionary<string, JToken>();

        /// <summary>
        /// Adds a custom scheduling hint.
        /// </summary>
        /// <param name="hint">The hint key.</param>
        /// <param name="value">The hint value.</param>
        public void Add(string hint, object value)
        {
            Data.Add(hint, JToken.FromObject(value));
        }
    }
}