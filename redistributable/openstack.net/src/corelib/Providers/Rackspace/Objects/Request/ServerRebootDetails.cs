namespace net.openstack.Providers.Rackspace.Objects.Request
{
    using System;
    using net.openstack.Core.Domain;
    using Newtonsoft.Json;

    /// <summary>
    /// This models the JSON body containing details for the Reboot Server request.
    /// </summary>
    /// <seealso cref="ServerRebootRequest"/>
    /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/Reboot_Server-d1e3371.html">Reboot Server (OpenStack Compute API v2 and Extensions Reference)</seealso>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class ServerRebootDetails
    {
        /// <summary>
        /// Gets the type of reboot to perform.
        /// </summary>
        [JsonProperty("type")]
        public RebootType Type
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerRebootDetails"/>
        /// class with the specified reboot type.
        /// </summary>
        /// <param name="type">The type of reboot to perform. See <see cref="RebootType"/> for predefined values.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="type"/> is <see langword="null"/>.</exception>
        public ServerRebootDetails(RebootType type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            Type = type;
        }
    }
}