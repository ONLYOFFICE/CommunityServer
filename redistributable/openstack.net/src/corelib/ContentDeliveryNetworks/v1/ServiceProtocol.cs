using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OpenStack.ContentDeliveryNetworks.v1
{
    /// <summary>
    /// Protocol used to access the assets on a domain.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ServiceProtocol
    {
        /// <summary />
        [EnumMember(Value = "http")]
        HTTP,

        /// <summary />
        [EnumMember(Value = "https")]
        HTTPS
    }
}