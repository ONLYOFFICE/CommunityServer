using Newtonsoft.Json;

namespace OpenStack.Compute.v2_1
{
    /// <summary>
    /// Summary information for a server instance.
    /// </summary>
    public class ServerSummary : ServerReference
    {
        /// <summary>
        /// The server name.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}