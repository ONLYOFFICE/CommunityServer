using net.openstack.Core.Domain;

namespace net.openstack.Providers.Rackspace.Objects
{
    using System;

    /// <summary>
    /// Extends the <see cref="CloudIdentity"/> class by adding support for specifying
    /// a <see cref="Domain"/> for the identity.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    public class RackspaceCloudIdentity : CloudIdentity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RackspaceCloudIdentity"/> class
        /// with the default values.
        /// </summary>
        public RackspaceCloudIdentity()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RackspaceCloudIdentity"/> class
        /// from the given <see cref="CloudIdentity"/> instance.
        /// </summary>
        /// <param name="cloudIdentity">The generic cloud identity.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="cloudIdentity"/> is <see langword="null"/>.</exception>
        public RackspaceCloudIdentity(CloudIdentity cloudIdentity) : this()
        {
            if (cloudIdentity == null)
                throw new ArgumentNullException("cloudIdentity");

            this.Username = cloudIdentity.Username;
            this.Password = cloudIdentity.Password;
            this.APIKey = cloudIdentity.APIKey;

            RackspaceCloudIdentity raxIdentity = cloudIdentity as RackspaceCloudIdentity;
            if (raxIdentity != null)
                this.Domain = raxIdentity.Domain;
        }

        /// <summary>
        /// Gets or sets the <see cref="Domain"/> for this account.
        /// </summary>
        /// <remarks>
        /// The <see cref="RackspaceCloudIdentity"/> class represents <em>credentials</em> (as opposed
        /// to an <em>account</em>), so any changes made to this property value will not be
        /// reflected in the account.
        /// </remarks>
        public Domain Domain { get; set; }
    }
}
