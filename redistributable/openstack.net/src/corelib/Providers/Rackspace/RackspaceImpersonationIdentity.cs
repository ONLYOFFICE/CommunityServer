using net.openstack.Providers.Rackspace.Objects;

namespace net.openstack.Providers.Rackspace
{
    /// <summary>
    /// Represents a cloud identity that impersonates another <see cref="RackspaceCloudIdentity"/>.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    public class RackspaceImpersonationIdentity : RackspaceCloudIdentity
    {
        /// <summary>
        /// Gets or sets the <see cref="RackspaceCloudIdentity"/> of the user to impersonate.
        /// </summary>
        /// <remarks>
        /// The <see cref="RackspaceImpersonationIdentity"/> class represents <em>credentials</em>
        /// (as opposed to an <em>account</em>), so any changes made to this property value will
        /// not be reflected in the account.
        /// </remarks>
        public RackspaceCloudIdentity UserToImpersonate { get; set; }
    }
}
