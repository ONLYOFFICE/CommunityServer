using System;
using System.Linq;
using net.openstack.Core.Domain;
using net.openstack.Core.Providers;
using OpenStack.Authentication;

namespace OpenStack.Core
{
    /// <summary>
    /// Provides methods to assist legacy <see cref="IIdentityProvider"/> implementations to implement the new <see cref="IAuthenticationProvider"/> interface which acts as a shim between the old provider model and the new service model.
    /// </summary>
    internal static class LegacyAuthenticationProviderHelper
    {
        /// <summary>
        /// Gets the endpoint for the specified identity and region.
        /// </summary>
        /// <param name="serviceType">The provider specific service type to look for in the service catalog</param>
        /// <param name="userAccess">The user access instance.</param>
        /// <param name="defaultIdentity">The cloud identity.</param>
        /// <param name="region">The endpoint region.</param>
        /// <param name="useInternalUrl">if set to <c>true</c> [use internal URL].</param>
        /// <returns></returns>
        /// <exception cref="OpenStack.UserAuthenticationException">
        /// The user does not have access to the service or it does not exist.
        /// or
        /// The user does not have access to the service endpoint in the specified region.
        /// </exception>
        /// <exception cref="RegionRequiredException">No region was specified and the service does not provide a region-independent endpoint.</exception>
        public static string GetEndpoint(string serviceType, UserAccess userAccess, CloudIdentity defaultIdentity, string region, bool useInternalUrl)
        {
            ServiceCatalog service = userAccess.ServiceCatalog.FirstOrDefault(sc => string.Equals(sc.Type, serviceType, StringComparison.OrdinalIgnoreCase));
            if (service == null)
                throw new UserAuthenticationException("The user does not have access to the {0} service or it does not exist.", serviceType);

            Endpoint endpoint = service.Endpoints
                .Where(e => string.Equals(e.Region, region, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(e => e.Region)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(region) && endpoint == null)
                throw new RegionRequiredException("No region was specified and the {0} service does not provide a region-independent endpoint.", serviceType);

            if (endpoint == null)
                throw new UserAuthenticationException("The user does not have access to the {0} endpoint in the {1} region.", serviceType, region);

            return useInternalUrl ? endpoint.InternalURL : endpoint.PublicURL;
        }
    }
}
