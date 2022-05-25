using net.openstack.Core.Domain;
using net.openstack.Core.Providers;

namespace net.openstack.Providers.Rackspace
{
    internal interface IRackspaceProvider
    {
        /// <summary>
        /// The <see cref="IIdentityProvider"/> used by the provider.
        /// </summary>
        IIdentityProvider IdentityProvider { get; }

        /// <summary>
        /// The default identify used by the provider.
        /// </summary>
        CloudIdentity DefaultIdentity { get; }
    }
}