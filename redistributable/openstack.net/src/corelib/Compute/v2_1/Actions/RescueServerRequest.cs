using Newtonsoft.Json;
using OpenStack.Serialization;

namespace OpenStack.Compute.v2_1
{
    /// <summary>
    /// Puts a server in rescue mode and changes its status to RESCUE.
    /// </summary>
    [JsonConverterWithConstructor(typeof(RootWrapperConverter), "rescue")]
    public class RescueServerRequest
    {
        /// <summary>
        /// The password for the rescued instance. If you omit this parameter, the operation generates a new password.
        /// </summary>
        [JsonProperty("adminPass")]
        public string AdminPassword { get; set; }

        /// <summary>
        /// The image identifier to use to rescue your server instance.
        /// </summary>
        [JsonProperty("rescue_image_ref")]
        public Identifier ImageId { get; set; }
    }
}