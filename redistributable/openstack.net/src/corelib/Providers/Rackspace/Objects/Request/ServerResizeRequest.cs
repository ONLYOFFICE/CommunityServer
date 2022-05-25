namespace net.openstack.Providers.Rackspace.Objects.Request
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// This models the JSON request used for the Resize Server request.
    /// </summary>
    /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/Resize_Server-d1e3707.html">Resize Server (OpenStack Compute API v2 and Extensions Reference)</seealso>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class ServerResizeRequest
    {
        /// <summary>
        /// Gets additional information about the Resize Server request.
        /// </summary>
        [JsonProperty("resize")]
        public ServerResizeDetails Details { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerResizeRequest"/>
        /// class with the specified details.
        /// </summary>
        /// <param name="details">The details of the request.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="details"/> is <see langword="null"/>.</exception>
        public ServerResizeRequest(ServerResizeDetails details)
        {
            if (details == null)
                throw new ArgumentNullException("details");

            Details = details;
        }
    }
}