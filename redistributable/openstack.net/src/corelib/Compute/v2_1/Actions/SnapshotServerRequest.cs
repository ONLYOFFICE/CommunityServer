using System.Collections.Generic;
using Newtonsoft.Json;
using OpenStack.Serialization;

namespace OpenStack.Compute.v2_1
{
    /// <summary>
    /// Snapshots a server.
    /// </summary>
    [JsonConverterWithConstructor(typeof (RootWrapperConverter), "createImage")]
    public class SnapshotServerRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SnapshotServerRequest"/> class.
        /// </summary>
        /// <param name="name">The snapshot name.</param>
        public SnapshotServerRequest(string name)
        {
            Name = name;
            Metadata = new Dictionary<string, string>();
        }

        /// <summary>
        /// The snapshot name.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Additional snapshot image metadata.
        /// </summary>
        [JsonProperty("metadata")]
        public IDictionary<string, string> Metadata { get; set; }
    }
}
