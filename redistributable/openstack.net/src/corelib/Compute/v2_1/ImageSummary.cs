using Newtonsoft.Json;

namespace OpenStack.Compute.v2_1
{
    /// <summary>
    /// Summary information for an image.
    /// </summary>
    public class ImageSummary : ImageReference
    {
        /// <summary>
        /// The image name.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}