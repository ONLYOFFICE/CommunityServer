namespace net.openstack.Providers.Rackspace.Objects.Request
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// This models the JSON request used for the Reboot Server request.
    /// </summary>
    /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/Reboot_Server-d1e3371.html">Reboot Server (OpenStack Compute API v2 and Extensions Reference)</seealso>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class ServerRebootRequest
    {
        /// <summary>
        /// Gets additional details about the Reboot Server request.
        /// </summary>
        [JsonProperty("reboot")]
        public ServerRebootDetails Details { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerRebootRequest"/>
        /// class with the specified details.
        /// </summary>
        /// <param name="details">The details of the request.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="details"/> is <see langword="null"/>.</exception>
        public ServerRebootRequest(ServerRebootDetails details)
        {
            if (details == null)
                throw new ArgumentNullException("details");

            Details = details;
        }
    }
}
