namespace net.openstack.Providers.Rackspace.Objects.Response
{
    using Newtonsoft.Json;

    /// <summary>
    /// This models the JSON response used for the Rescue Server request.
    /// </summary>
    /// <seealso href="http://docs.rackspace.com/servers/api/v2/cs-devguide/content/rescue_mode.html">Rescue Server (Rackspace Next Generation Cloud Servers Developer Guide - API v2)</seealso>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class RescueServerResponse
    {
        /// <summary>
        /// Gets the temporary administrator password assigned for use while the server
        /// is in rescue mode.
        /// </summary>
        [JsonProperty("adminPass")]
        public string AdminPassword { get; private set; }
    }
}
