using System.Threading;
using System.Threading.Tasks;

namespace OpenStack.Authentication
{
    /// <summary>
    /// Provides authentication functionality to any service implementations.
    /// </summary>
    public interface IAuthenticationProvider
    {
        /// <summary>
        /// Gets the endpoints for the specified service.
        /// <para>
        /// Uses a region specific endpoint if available, otherwise returns the global endpoint.
        /// </para>
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <param name="region">The region.</param>
        /// <param name="useInternalUrl">if set to <c>true</c> [use internal URL].</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="UserAuthenticationException">
        /// The user does not have access to the service or it does not exist.
        /// or
        /// The user does not have access to the service endpoint in the specified region.
        /// </exception>
        /// <exception cref="RegionRequiredException">No region was specified and the {0} service does not provide a region-independent endpoint.</exception>
        /// <returns>The requested endpoint.</returns>
        Task<string> GetEndpoint(IServiceType serviceType, string region, bool useInternalUrl, CancellationToken cancellationToken);
        
        /// <summary>
        /// Gets an authentication token for the user.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>An authentication token.</returns>
        Task<string> GetToken(CancellationToken cancellationToken);
    }
}
