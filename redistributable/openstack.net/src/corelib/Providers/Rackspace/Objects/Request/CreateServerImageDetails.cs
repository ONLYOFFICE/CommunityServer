namespace net.openstack.Providers.Rackspace.Objects.Request
{
    using System;
    using net.openstack.Core.Domain;
    using Newtonsoft.Json;

    /// <summary>
    /// This models the JSON body containing details for the Create Image request.
    /// </summary>
    /// <seealso cref="CreateServerImageRequest"/>
    /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/Create_Image-d1e4655.html">Create Image (OpenStack Compute API v2 and Extensions Reference)</seealso>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class CreateServerImageDetails
    {
        /// <summary>
        /// Gets the name of the image to create.
        /// </summary>
        [JsonProperty("name")]
        public string ImageName { get; private set; }

        /// <summary>
        /// Gets the metadata to associate with the image.
        /// </summary>
        [JsonProperty("metadata", DefaultValueHandling = DefaultValueHandling.Include)]
        public Metadata Metadata { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateServerImageDetails"/> class
        /// with the specified name and metadata.
        /// </summary>
        /// <param name="imageName">Name of the new image.</param>
        /// <param name="metadata">The metadata to associate to the new image.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="imageName"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="imageName"/> is empty.</exception>
        public CreateServerImageDetails(string imageName, Metadata metadata)
        {
            if (imageName == null)
                throw new ArgumentNullException("imageName");
            if (string.IsNullOrEmpty(imageName))
                throw new ArgumentException("imageName cannot be empty");

            ImageName = imageName;
            Metadata = metadata;
        }
    }
}
