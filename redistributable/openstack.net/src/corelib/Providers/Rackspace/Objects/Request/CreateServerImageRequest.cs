namespace net.openstack.Providers.Rackspace.Objects.Request
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// This models the JSON request used for the Create Image request.
    /// </summary>
    /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/Create_Image-d1e4655.html">Create Image (OpenStack Compute API v2 and Extensions Reference)</seealso>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class CreateServerImageRequest
    {
        /// <summary>
        /// Gets additional details about the Create Image request.
        /// </summary>
        [JsonProperty("createImage")]
        public CreateServerImageDetails Details { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateServerImageRequest"/>
        /// class with the specified details.
        /// </summary>
        /// <param name="details">The details of the request.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="details"/> is <see langword="null"/>.</exception>
        public CreateServerImageRequest(CreateServerImageDetails details)
        {
            if (details == null)
                throw new ArgumentNullException("details");

            Details = details;
        }
    }
}
