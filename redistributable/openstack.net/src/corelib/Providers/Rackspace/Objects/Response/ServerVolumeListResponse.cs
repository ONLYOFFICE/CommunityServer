namespace net.openstack.Providers.Rackspace.Objects.Response
{
    using System.Collections.Generic;
    using net.openstack.Core.Domain;
    using Newtonsoft.Json;

    /// <summary>
    /// This models the JSON response used for the List Volume Attachments request.
    /// </summary>
    /// <seealso href="http://docs.rackspace.com/servers/api/v2/cs-devguide/content/List_Volume_Attachments.html">List Volume Attachments (Rackspace Next Generation Cloud Servers Developer Guide - API v2)</seealso>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class ServerVolumeListResponse
    {
        /// <summary>
        /// Gets a collection of information about the volume attachments.
        /// </summary>
        [JsonProperty("volumeAttachments")]
        public IEnumerable<ServerVolume> ServerVolumes { get; private set; }
    }
}
