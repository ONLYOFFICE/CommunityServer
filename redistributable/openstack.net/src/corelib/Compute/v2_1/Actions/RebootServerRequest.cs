using Newtonsoft.Json;
using OpenStack.Serialization;

namespace OpenStack.Compute.v2_1
{
    /// <summary>
    /// Reboots a server.
    /// </summary>
    [JsonConverterWithConstructor(typeof (RootWrapperConverter), "reboot")]
    public class RebootServerRequest
    {
        /// <summary>
        /// The type of reboot to perform. The default, if unspecified, is a soft reboot.
        /// </summary>
        [JsonProperty("type")]
        public RebootType Type { get; set; } = RebootType.Soft;
    }
}