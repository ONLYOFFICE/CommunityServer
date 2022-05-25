namespace net.openstack.Providers.Rackspace.Objects.Request
{
    using Newtonsoft.Json;

    /// <summary>
    /// This models the JSON request used for the Confirm Resized Server request.
    /// </summary>
    /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/Confirm_Resized_Server-d1e3868.html">Confirm Resized Server (OpenStack Compute API v2 and Extensions Reference)</seealso>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class ConfirmServerResizeRequest
    {
#pragma warning disable 414 // The field 'fieldName' is assigned but its value is never used
        [JsonProperty("confirmResize")]
        private string _command = "none";
#pragma warning restore 414
    }
}
