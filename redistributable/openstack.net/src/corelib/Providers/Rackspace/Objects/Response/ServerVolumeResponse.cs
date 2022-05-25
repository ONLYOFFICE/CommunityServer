namespace net.openstack.Providers.Rackspace.Objects.Response
{
    using net.openstack.Core.Domain;
    using Newtonsoft.Json;

    /// <summary>
    /// This models the JSON response used for the Attach Volume to Server and Get Volume Attachment Details requests.
    /// </summary>
    /// <seealso href="http://docs.rackspace.com/servers/api/v2/cs-devguide/content/Attach_Volume_to_Server.html">Attach Volume to Server (Rackspace Next Generation Cloud Servers Developer Guide - API v2)</seealso>
    /// <seealso href="http://docs.rackspace.com/servers/api/v2/cs-devguide/content/Get_Volume_Attachment_Details.html">Get Volume Attachment Details (Rackspace Next Generation Cloud Servers Developer Guide - API v2)</seealso>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class ServerVolumeResponse
    {
        /// <summary>
        /// Gets information about the volume attachment.
        /// </summary>
        [JsonProperty("volumeAttachment")]
        public ServerVolume ServerVolume { get; private set; }
    }
}
