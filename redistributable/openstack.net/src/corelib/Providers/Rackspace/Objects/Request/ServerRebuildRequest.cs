namespace net.openstack.Providers.Rackspace.Objects.Request
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// This models the JSON request used for the Rebuild Server request.
    /// </summary>
    /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/Rebuild_Server-d1e3538.html">Rebuild Server (OpenStack Compute API v2 and Extensions Reference)</seealso>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class ServerRebuildRequest
    {
        /// <summary>
        /// Gets additional information about the Rebuild Server request.
        /// </summary>
        [JsonProperty("rebuild")]
        public ServerRebuildDetails Details { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerRebuildRequest"/>
        /// class with the specified details.
        /// </summary>
        /// <param name="details">The details of the request.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="details"/> is <see langword="null"/>.</exception>
        public ServerRebuildRequest(ServerRebuildDetails details)
        {
            if (details == null)
                throw new ArgumentNullException("details");

            Details = details;
        }
    }
}
