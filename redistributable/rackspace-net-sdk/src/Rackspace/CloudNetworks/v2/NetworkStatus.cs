using System.Runtime.Serialization;
using Newtonsoft.Json;
using OpenStack.Serialization;

namespace Rackspace.CloudNetworks.v2
{
    /// <summary>
    /// <see cref="Network"/> Status
    /// </summary>
    [JsonConverter(typeof(TolerantEnumConverter))]
    public enum NetworkStatus
    {
        /// <summary>
        /// The network status is unknown.
        /// </summary>
        [EnumMember(Value = "UNKNOWN")]
        Unknown,

        /// <summary>
        /// The network is active.
        /// </summary>
        [EnumMember(Value = "ACTIVE")]
        Active
    }
}