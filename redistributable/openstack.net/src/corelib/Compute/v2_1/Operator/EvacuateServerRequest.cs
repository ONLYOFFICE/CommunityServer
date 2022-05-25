using Newtonsoft.Json;
using OpenStack.Serialization;

namespace OpenStack.Compute.v2_1.Operator
{
    /// <summary>
    /// Evacuates a server from a failed host to a new one.
    /// </summary>
    [JsonConverterWithConstructor(typeof(RootWrapperConverter), "evacuate")]
    public class EvacuateServerRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EvacuateServerRequest"/> class.
        /// </summary>
        /// <param name="isServerOnSharedStorage">if set to <c>true</c> [is server on shared storage].</param>
        public EvacuateServerRequest(bool isServerOnSharedStorage)
        {
            IsServerOnSharedStorage = isServerOnSharedStorage;
        }

        /// <summary>
        /// The identifier of the host to which the server should be evacuated. 
        /// </summary>
        [JsonProperty("host")]
        public Identifier DestinationHostId { get; set; }

        /// <summary>
        /// An administrative password to access the evacuated instance. 
        /// </summary>
        [JsonProperty("adminPass")] // NOTE: The doc says admin_password but the API clearly wants adminPass, see https://bugs.launchpad.net/keystone/+bug/1526446
        public string AdminPassword { get; set; }

        /// <summary>
        /// Specify if the server is on shared storage.
        /// </summary>
        [JsonProperty("onSharedStorage", DefaultValueHandling = DefaultValueHandling.Include)] // NOTE: The doc says on_shared_storage but the API clearly wants onSharedStorage, see https://bugs.launchpad.net/keystone/+bug/1526446
        public bool IsServerOnSharedStorage { get; set; }
    }
}