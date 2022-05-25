using OpenStack.Authentication;

namespace OpenStack.Compute.v2_2
{
    /// <inheritdoc />
    /// <seealso href="https://github.com/openstack/nova/blob/master/nova/api/openstack/rest_api_version_history.rst#22">Compute Microversion 2.6</seealso>
    public class ComputeApi : v2_1.Serialization.ComputeApi
    {
        /// <summary />
        public ComputeApi(IServiceType serviceType, IAuthenticationProvider authenticationProvider, string region, bool useInternalUrl)
            : this(serviceType, authenticationProvider, region, useInternalUrl, "2.2")
        { }

        /// <summary />
        protected ComputeApi(IServiceType serviceType, IAuthenticationProvider authenticationProvider, string region, bool useInternalUrl, string microversion)
            : base(serviceType, authenticationProvider, region, useInternalUrl, microversion)
        { }
    }
}
