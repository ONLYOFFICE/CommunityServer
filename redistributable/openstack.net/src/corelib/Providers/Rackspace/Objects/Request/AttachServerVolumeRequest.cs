namespace net.openstack.Providers.Rackspace.Objects.Request
{
    using System;
    using net.openstack.Core.Domain;
    using Newtonsoft.Json;

    /// <summary>
    /// This models the JSON request used for the Attach Volume to Server request.
    /// </summary>
    /// <seealso href="http://docs.rackspace.com/servers/api/v2/cs-devguide/content/Attach_Volume_to_Server.html">Attach Volume to Server (Rackspace Next Generation Cloud Servers Developer Guide - API v2)</seealso>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class AttachServerVolumeRequest
    {
        /// <summary>
        /// Gets additional information about the volume to attach.
        /// </summary>
        [JsonProperty("volumeAttachment")]
        public AttachServerVolumeData ServerVolumeData { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AttachServerVolumeRequest"/> class
        /// with the given device name and volume ID.
        /// </summary>
        /// <param name="device">
        /// The name of the device, such as <localUri>/dev/xvdb</localUri>. If the value
        /// is <see langword="null"/>, an automatically generated device name will be used.
        /// </param>
        /// <param name="volumeId">The volume ID. This is obtained from <see cref="Volume.Id"/>.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="volumeId"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="volumeId"/> is empty.</exception>
        public AttachServerVolumeRequest(string device, string volumeId)
        {
            if (volumeId == null)
                throw new ArgumentNullException("volumeId");
            if (string.IsNullOrEmpty(volumeId))
                throw new ArgumentException("volumeId cannot be empty");

            ServerVolumeData = new AttachServerVolumeData(device, volumeId);
        }

        /// <summary>
        /// This models the JSON body containing the details of the Attach Volume to Server request.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        [JsonObject(MemberSerialization.OptIn)]
        public class AttachServerVolumeData
        {
            /// <summary>
            /// Gets the name of the device, such as <localUri>/dev/xvdb</localUri>.
            /// If the value is <see langword="null"/>, the server automatically assigns a device
            /// name.
            /// </summary>
            [JsonProperty("device")]
            public string Device { get; private set; }

            /// <summary>
            /// Gets the ID of the volume to attach to the server instance.
            /// </summary>
            [JsonProperty("volumeId")]
            public string VolumeId { get; private set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="AttachServerVolumeData"/> class
            /// with the given device name and volume ID.
            /// </summary>
            /// <param name="device">
            /// The name of the device, such as <localUri>/dev/xvdb</localUri>. If the value
            /// is <see langword="null"/>, an automatically generated device name will be used.
            /// </param>
            /// <param name="volumeId">The volume ID. This is obtained from <see cref="Volume.Id"/>.</param>
            /// <exception cref="ArgumentNullException">If <paramref name="volumeId"/> is <see langword="null"/>.</exception>
            /// <exception cref="ArgumentException">If <paramref name="volumeId"/> is empty.</exception>
            public AttachServerVolumeData(string device, string volumeId)
            {
                if (volumeId == null)
                    throw new ArgumentNullException("volumeId");
                if (string.IsNullOrEmpty(volumeId))
                    throw new ArgumentException("volumeId cannot be empty");

                Device = device;
                VolumeId = volumeId;
            }
        }
    }
}
