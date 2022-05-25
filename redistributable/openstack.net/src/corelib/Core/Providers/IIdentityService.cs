namespace net.openstack.Core.Providers
{
    using System;
    using System.Threading.Tasks;
    using net.openstack.Core.Domain;
    using net.openstack.Core.Exceptions.Response;
    using CancellationToken = System.Threading.CancellationToken;

    /// <summary>
    /// Represents a provider for asynchronous operations on the OpenStack Identity Service.
    /// </summary>
    /// <seealso href="http://docs.openstack.org/api/openstack-identity-service/2.0/content/">OpenStack Identity Service API v2.0 Reference</seealso>
    /// <preliminary/>
    public interface IIdentityService
    {
        /// <summary>
        /// Authenticates the user for the specified identity.
        /// </summary>
        /// <remarks>
        /// This method always authenticates to the server, even if an unexpired, cached <see cref="UserAccess"/>
        /// is available for the specified identity.
        /// </remarks>
        /// <param name="identity">The identity of the user to authenticate.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>A <see cref="Task"/> object representing the asynchronous operation. When the task completes successfully, the <see cref="Task{TResult}.Result"/> property will contain a <see cref="UserAccess"/> object containing the authentication token, service catalog and user data.</returns>
        /// <exception cref="NotSupportedException">If the provider does not support the given <paramref name="identity"/> type.</exception>
        /// <exception cref="InvalidOperationException">If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.</exception>
        /// <exception cref="ResponseException">If the authentication request failed.</exception>
        /// <seealso href="http://docs.openstack.org/api/openstack-identity-service/2.0/content/POST_authenticate_v2.0_tokens_.html">Authenticate (OpenStack Identity Service API v2.0 Reference)</seealso>
        Task<UserAccess> AuthenticateAsync(CloudIdentity identity, CancellationToken cancellationToken);

        /// <summary>
        /// Gets the user access details, authenticating with the server if necessary.
        /// </summary>
        /// <remarks>
        /// If <paramref name="forceCacheRefresh"/> is <see langword="false"/> and a cached <see cref="UserAccess"/>
        /// is available for the specified <paramref name="identity"/>, this method may return the cached
        /// value without performing an authentication against the server. If <paramref name="forceCacheRefresh"/>
        /// is <see langword="true"/>, this method is equivalent to <see cref="AuthenticateAsync"/>.
        /// </remarks>
        /// <param name="identity">The identity of the user to authenticate.</param>
        /// <param name="forceCacheRefresh">If <see langword="true"/>, the user is always authenticated against the server; otherwise a cached <see cref="IdentityToken"/> may be returned.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>A <see cref="Task"/> object representing the asynchronous operation. When the task completes successfully, the <see cref="Task{TResult}.Result"/> property will contain a <see cref="UserAccess"/> object containing the authentication token, service catalog and user data.</returns>
        /// <exception cref="NotSupportedException">If the provider does not support the given <paramref name="identity"/> type.</exception>
        /// <exception cref="InvalidOperationException">If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.</exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso href="http://docs.openstack.org/api/openstack-identity-service/2.0/content/POST_authenticate_v2.0_tokens_.html">Authenticate (OpenStack Identity Service API v2.0 Reference)</seealso>
        Task<UserAccess> GetUserAccessAsync(CloudIdentity identity, bool forceCacheRefresh, CancellationToken cancellationToken);

        /// <summary>
        /// Gets the authentication token for the specified identity. If necessary, the identity is authenticated
        /// on the server to obtain a token.
        /// </summary>
        /// <remarks>
        /// If a cached <see cref="IdentityToken"/> is available for the specified <paramref name="identity"/>,
        /// this method may return the cached value without performing an authentication against the server.
        /// </remarks>
        /// <param name="identity">The identity of the user to authenticate.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>A <see cref="Task"/> object representing the asynchronous operation. When the task completes successfully, the <see cref="Task{TResult}.Result"/> property will contain the user's authentication token.</returns>
        /// <exception cref="NotSupportedException">If the provider does not support the given <paramref name="identity"/> type.</exception>
        /// <exception cref="InvalidOperationException">If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.</exception>
        /// <exception cref="ResponseException">If the authentication request failed.</exception>
        /// <seealso href="http://docs.openstack.org/api/openstack-identity-service/2.0/content/POST_authenticate_v2.0_tokens_.html">Authenticate (OpenStack Identity Service API v2.0 Reference)</seealso>
        Task<IdentityToken> GetTokenAsync(CloudIdentity identity, CancellationToken cancellationToken);
    }
}
