namespace net.openstack.Providers.Rackspace.Objects.Request
{
    using Newtonsoft.Json;

    /// <summary>
    /// This models the JSON request used for the Rescue Server request.
    /// </summary>
    /// <seealso href="http://docs.rackspace.com/servers/api/v2/cs-devguide/content/rescue_mode.html">Rescue Server (Rackspace Next Generation Cloud Servers Developer Guide - API v2)</seealso>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class RescueServerRequest
    {
#pragma warning disable 414 // The field 'fieldName' is assigned but its value is never used
        [JsonProperty("rescue")]
        private string _command = "none";
#pragma warning restore 414
    }
}
