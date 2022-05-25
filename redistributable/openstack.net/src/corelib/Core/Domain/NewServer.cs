namespace net.openstack.Core.Domain
{
    using System;
    using net.openstack.Core.Providers;
    using Newtonsoft.Json;

    /// <summary>
    /// Extends <see cref="ServerBase"/> with information for a newly-created server.
    /// </summary>
    /// <seealso cref="IComputeProvider.CreateServer"/>
    /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/CreateServers.html">Create Server (OpenStack Compute API v2 and Extensions Reference)</seealso>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    public class NewServer : ServerBase
    {
        /// <summary>
        /// Gets the disk configuration used for creating the server. If the value was
        /// not explicitly specified in the create request, the server inherits the value
        /// from the image it was created from.
        /// </summary>
        /// <remarks>
        /// <note>This property is defined by the Rackspace-specific Disk Configuration Extension to the OpenStack Compute API.</note>
        /// </remarks>
        /// <seealso href="http://docs.rackspace.com/servers/api/v2/cs-devguide/content/ch_extensions.html#diskconfig_attribute">Disk Configuration Extension (Rackspace Next Generation Cloud Servers Developer Guide - API v2)</seealso>
        [JsonProperty("OS-DCF:diskConfig")]
        public DiskConfiguration DiskConfig { get; private set; }

        /// <summary>
        /// Gets the administrator password for the newly created server.
        /// </summary>
        [JsonProperty("adminPass")]
        public string AdminPassword { get; private set; }

        /// <inheritdoc/>
        protected override void UpdateThis(ServerBase server)
        {
            if (server == null)
                throw new ArgumentNullException("server");

            base.UpdateThis(server);

            var details = server as NewServer;

            if (details == null)
                return;

            DiskConfig = details.DiskConfig;
            AdminPassword = details.AdminPassword;
        }
    }
}
