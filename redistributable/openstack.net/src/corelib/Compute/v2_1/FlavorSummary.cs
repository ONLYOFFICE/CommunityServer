using Newtonsoft.Json;

namespace OpenStack.Compute.v2_1
{
    /// <summary>
    /// Summary information for a flavor.
    /// </summary>
    public class FlavorSummary : FlavorReference
    {
        /// <summary>
        /// The flavor name.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }   
    }
}