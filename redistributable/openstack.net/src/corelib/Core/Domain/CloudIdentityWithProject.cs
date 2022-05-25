namespace net.openstack.Core.Domain
{
    /// <summary>
    /// This class extends <see cref="CloudIdentity"/> with the addition of the
    /// <see cref="ProjectId"/> and <see cref="ProjectName"/> properties, which
    /// may be required for authentication with certain Identity Service providers.
    /// </summary>
    /// <threadsafety/>
    /// <preliminary/>
    public class CloudIdentityWithProject : CloudIdentity
    {
        /// <summary>
        /// Gets or sets the project ID for this identity.
        /// </summary>
        /// <value>
        /// The project ID for this identity. The value may be <see langword="null"/> if the particular
        /// provider supports authenticating without a project ID.
        /// </value>
        public ProjectId ProjectId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the project name for this identity.
        /// </summary>
        /// <value>
        /// The project name for this identity. The value may be <see langword="null"/> if the particular
        /// provider supports authenticating without a project name.
        /// </value>
        public string ProjectName
        {
            get;
            set;
        }
    }
}
