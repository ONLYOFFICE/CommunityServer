using Newtonsoft.Json;
using OpenStack.Serialization;

namespace OpenStack.Compute.v2_1
{
    /// <summary>
    /// Defines a set of fields to update on a server.
    /// </summary>
    [JsonConverterWithConstructor(typeof(RootWrapperConverter), "server")]
    public class ServerUpdateDefinition
    {
        private string _ipv4Address;
        private string _ipv6Address;

        /// <inheritdoc cref="ServerSummary.Name" />
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <inheritdoc cref="Server.IPv4Address" />
        [JsonProperty("accessIPv4")]
        public string IPv4Address
        {
            get { return _ipv4Address; }
            set
            {
                // Nova returns "" when the value isn't set, which is causing us to serialize this propety during updates, when we really shouldn't
                _ipv4Address = !string.IsNullOrEmpty(value) ? value : null;
            }
        }

        /// <inheritdoc cref="Server.IPv6Address" />
        [JsonProperty("accessIPv6")]
        public string IPv6Address
        {
            get { return _ipv6Address; }
            set
            {
                // Nova returns "" when the value isn't set, which is causing us to serialize this propety during updates, when we really shouldn't
                _ipv6Address = !string.IsNullOrEmpty(value) ? value : null;
            }
        }
    }
}