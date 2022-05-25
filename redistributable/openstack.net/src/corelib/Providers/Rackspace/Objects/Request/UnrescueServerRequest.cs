namespace net.openstack.Providers.Rackspace.Objects.Request
{
    using Newtonsoft.Json;

    /// <summary>
    /// This models the JSON request used for the Unrescue Server request.
    /// </summary>
    /// <seealso href="http://docs.rackspace.com/servers/api/v2/cs-devguide/content/exit_rescue_mode.html">Unrescue Server (Rackspace Next Generation Cloud Servers Developer Guide - API v2)</seealso>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class UnrescueServerRequest
    {
#pragma warning disable 414 // The field 'fieldName' is assigned but its value is never used
        [JsonProperty("unrescue")]
        private string _command = "none";
#pragma warning restore 414
    }
}
