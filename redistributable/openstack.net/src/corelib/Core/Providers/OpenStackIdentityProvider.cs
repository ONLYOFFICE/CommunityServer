using Newtonsoft.Json;

using OpenStack;
using OpenStack.Authentication;

namespace net.openstack.Core.Providers
{
    using System;

    using net.openstack.Core.Caching;
    using net.openstack.Core.Domain;
    using net.openstack.Providers.Rackspace;

    using Newtonsoft.Json.Linq;

    using HttpMethod = JSIStudios.SimpleRESTServices.Client.HttpMethod;
    using IRestService = JSIStudios.SimpleRESTServices.Client.IRestService;
    using JsonRestServices = JSIStudios.SimpleRESTServices.Client.Json.JsonRestServices;

    /// <summary>
    /// This class extends the functionality of <see cref="CloudIdentityProvider"/> by
    /// supporting <see cref="CloudIdentityWithProject"/> identity objects. These
    /// identities may include values for the <c>tenantName</c> and <c>tenantId</c>
    /// properties, which may be required for authentication with certain
    /// OpenStack-compatible service providers.
    /// </summary>
    /// <threadsafety/>
    /// <preliminary/>
    public class OpenStackIdentityProvider : CloudIdentityProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OpenStackIdentityProvider"/> class
        /// with no default identity, the specified base URL, and the default REST service
        /// implementation and token cache.
        /// </summary>
        /// <param name="urlBase">The base URL for the cloud instance.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="urlBase"/> is <see langword="null"/>.</exception>
        public OpenStackIdentityProvider(Uri urlBase)
            : this(urlBase, null, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenStackIdentityProvider"/> class
        /// with the specified default identity and base URL, and the default REST service
        /// implementation and token cache.
        /// </summary>
        /// <param name="urlBase">The base URL for the cloud instance.</param>
        /// <param name="defaultIdentity">The default identity to use for calls that do not explicitly specify an identity. If this value is <see langword="null"/>, no default identity is available so all calls must specify an explicit identity.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="urlBase"/> is <see langword="null"/>.</exception>
        public OpenStackIdentityProvider(Uri urlBase, CloudIdentity defaultIdentity)
            : this(urlBase, defaultIdentity, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenStackIdentityProvider"/> class
        /// with no default identity, and the specified base URL, REST service
        /// implementation, and token cache.
        /// </summary>
        /// <param name="urlBase">The base URL for the cloud instance.</param>
        /// <param name="restService">The implementation of <see cref="IRestService"/> to use for executing REST requests. If this value is <see langword="null"/>, the provider will use a new instance of <see cref="JsonRestServices"/>.</param>
        /// <param name="tokenCache">The cache to use for caching user access tokens. If this value is <see langword="null"/>, the provider will use <see cref="UserAccessCache.Instance"/>.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="urlBase"/> is <see langword="null"/>.</exception>
        public OpenStackIdentityProvider(Uri urlBase, IRestService restService, ICache<UserAccess> tokenCache)
            : this(urlBase, null, restService, tokenCache)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenStackIdentityProvider"/> class
        /// using the provided values.
        /// </summary>
        /// <param name="urlBase">The base URL for the cloud instance.</param>
        /// <param name="defaultIdentity">The default identity to use for calls that do not explicitly specify an identity. If this value is <see langword="null"/>, no default identity is available so all calls must specify an explicit identity.</param>
        /// <param name="restService">The implementation of <see cref="IRestService"/> to use for executing REST requests. If this value is <see langword="null"/>, the provider will use a new instance of <see cref="JsonRestServices"/>.</param>
        /// <param name="tokenCache">The cache to use for caching user access tokens. If this value is <see langword="null"/>, the provider will use <see cref="UserAccessCache.Instance"/>.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="urlBase"/> is <see langword="null"/>.</exception>
        public OpenStackIdentityProvider(Uri urlBase, CloudIdentity defaultIdentity, IRestService restService, ICache<UserAccess> tokenCache)
            : base(defaultIdentity, restService, tokenCache, urlBase)
        {
            if (urlBase == null)
                throw new ArgumentNullException("urlBase");
        }

        /// <inheritdoc/>
        public override UserAccess GetUserAccess(CloudIdentity identity, bool forceCacheRefresh = false)
        {
            identity = identity ?? DefaultIdentity;

            CloudIdentityWithProject identityWithProject = identity as CloudIdentityWithProject;
            if (identityWithProject == null)
                return base.GetUserAccess(identityWithProject, forceCacheRefresh);

            if (string.IsNullOrEmpty(identityWithProject.Password))
                throw new NotSupportedException(string.Format("The {0} identity must specify a password.", typeof(CloudIdentityWithProject)));
            if (!string.IsNullOrEmpty(identityWithProject.APIKey))
                throw new NotSupportedException(string.Format("The {0} identity does not support API key authentication.", typeof(CloudIdentityWithProject)));

            Func<UserAccess> refreshCallback =
                () =>
                {
                    var projectId = identityWithProject.ProjectId != null ? JToken.FromObject(identityWithProject.ProjectId) : string.Empty;
                    JObject requestBody = new JObject(
                        new JProperty("auth", new JObject(
                            new JProperty("passwordCredentials", new JObject(
                                new JProperty("username", JValue.CreateString(identityWithProject.Username)),
                                new JProperty("password", JValue.CreateString(identityWithProject.Password)))),
                            new JProperty("tenantName", JToken.FromObject(identityWithProject.ProjectName)),
                            new JProperty("tenantId", projectId))));

                    var response = ExecuteRESTRequest<JObject>(identity, new Uri(UrlBase, "/v2.0/tokens"), HttpMethod.POST, requestBody, isTokenRequest: true);
                    if (response == null || response.Data == null)
                        return null;

                    // The defalut json serialization is helpfully formatting the expires date string. Use our custom serializer for this part to prevent chaos of timezone proportions.
                    var rawJson = response.Data["access"]?.ToString(Formatting.None);
                    if (rawJson == null)
                        return null;

                    UserAccess access = OpenStackNet.Deserialize<UserAccess>(rawJson);
                    if (access == null || access.Token == null)
                        return null;

                    return access;
                };
            string key = string.Format("{0}:{1}/{2}", UrlBase, identityWithProject.ProjectId, identityWithProject.Username);
            var userAccess = TokenCache.Get(key, refreshCallback, forceCacheRefresh);

            return userAccess;
        }

        /// <inheritdoc/>
        protected override string LookupServiceTypeKey(IServiceType serviceType)
        {
            return serviceType.Type;
        }
    }
}
