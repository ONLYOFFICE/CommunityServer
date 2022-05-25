using OpenStack.Authentication;

namespace net.openstack.Providers.Hp
{
    using System;

    using net.openstack.Core.Caching;
    using net.openstack.Core.Domain;

    using Newtonsoft.Json.Linq;

    using CloudIdentityProvider = net.openstack.Providers.Rackspace.CloudIdentityProvider;
    using HttpMethod = JSIStudios.SimpleRESTServices.Client.HttpMethod;
    using IIdentityProvider = net.openstack.Core.Providers.IIdentityProvider;
    using IRestService = JSIStudios.SimpleRESTServices.Client.IRestService;
    using JsonRestServices = JSIStudios.SimpleRESTServices.Client.Json.JsonRestServices;

    /// <summary>
    /// Provides an implementation of <see cref="IIdentityProvider"/> for operating with
    /// HP's Cloud Identity product. This provider supports authentication using a username/password
    /// combination or an access key/secret key combinatiton, and supports scoped tokens
    /// when credentials are represented with <see cref="CloudIdentityWithProject"/>.
    /// </summary>
    /// <seealso href="http://docs.openstack.org/api/openstack-identity-service/2.0/content/">OpenStack Identity Service API v2.0 Reference</seealso>
    /// <seealso href="http://docs.hpcloud.com/api/identity/">HP Cloud v12.12 Identity Services API</seealso>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    public class HpIdentityProvider : CloudIdentityProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HpIdentityProvider"/> class
        /// with no default identity, the <see cref="PredefinedHpIdentityEndpoints.Default"/> base URL, and the default REST service
        /// implementation and token cache.
        /// </summary>
        public HpIdentityProvider()
            : this(PredefinedHpIdentityEndpoints.Default, null, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HpIdentityProvider"/> class
        /// with the specified default identity, the <see cref="PredefinedHpIdentityEndpoints.Default"/> base URL, and the default REST service
        /// implementation and token cache.
        /// </summary>
        /// <param name="defaultIdentity">The default identity to use for calls that do not explicitly specify an identity. If this value is <see langword="null"/>, no default identity is available so all calls must specify an explicit identity.</param>
        public HpIdentityProvider(CloudIdentity defaultIdentity)
            : this(PredefinedHpIdentityEndpoints.Default, defaultIdentity, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HpIdentityProvider"/> class
        /// with no default identity, the <see cref="PredefinedHpIdentityEndpoints.Default"/> base URL, and the default REST service
        /// implementation, and token cache.
        /// </summary>
        /// <param name="restService">The implementation of <see cref="IRestService"/> to use for executing REST requests. If this value is <see langword="null"/>, the provider will use a new instance of <see cref="JsonRestServices"/>.</param>
        /// <param name="tokenCache">The cache to use for caching user access tokens. If this value is <see langword="null"/>, the provider will use <see cref="UserAccessCache.Instance"/>.</param>
        public HpIdentityProvider(IRestService restService, ICache<UserAccess> tokenCache)
            : this(PredefinedHpIdentityEndpoints.Default, null, restService, tokenCache)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HpIdentityProvider"/> class
        /// using the <see cref="PredefinedHpIdentityEndpoints.Default"/> base URL and provided values.
        /// </summary>
        /// <param name="defaultIdentity">The default identity to use for calls that do not explicitly specify an identity. If this value is <see langword="null"/>, no default identity is available so all calls must specify an explicit identity.</param>
        /// <param name="restService">The implementation of <see cref="IRestService"/> to use for executing REST requests. If this value is <see langword="null"/>, the provider will use a new instance of <see cref="JsonRestServices"/>.</param>
        /// <param name="tokenCache">The cache to use for caching user access tokens. If this value is <see langword="null"/>, the provider will use <see cref="UserAccessCache.Instance"/>.</param>
        public HpIdentityProvider(CloudIdentity defaultIdentity, IRestService restService, ICache<UserAccess> tokenCache)
            : this(PredefinedHpIdentityEndpoints.Default, defaultIdentity, restService, tokenCache)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HpIdentityProvider"/> class
        /// with no default identity, the specified base URL, and the default REST service
        /// implementation and token cache.
        /// </summary>
        /// <param name="urlBase">The base URL for the cloud instance. Predefined URLs are available in <see cref="PredefinedHpIdentityEndpoints"/>.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="urlBase"/> is <see langword="null"/>.</exception>
        public HpIdentityProvider(Uri urlBase)
            : this(urlBase, null, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HpIdentityProvider"/> class
        /// with the specified default identity and base URL, and the default REST service
        /// implementation and token cache.
        /// </summary>
        /// <param name="urlBase">The base URL for the cloud instance. Predefined URLs are available in <see cref="PredefinedHpIdentityEndpoints"/>.</param>
        /// <param name="defaultIdentity">The default identity to use for calls that do not explicitly specify an identity. If this value is <see langword="null"/>, no default identity is available so all calls must specify an explicit identity.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="urlBase"/> is <see langword="null"/>.</exception>
        public HpIdentityProvider(Uri urlBase, CloudIdentity defaultIdentity)
            : this(urlBase, defaultIdentity, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HpIdentityProvider"/> class
        /// with no default identity, and the specified base URL, REST service
        /// implementation, and token cache.
        /// </summary>
        /// <param name="urlBase">The base URL for the cloud instance. Predefined URLs are available in <see cref="PredefinedHpIdentityEndpoints"/>.</param>
        /// <param name="restService">The implementation of <see cref="IRestService"/> to use for executing REST requests. If this value is <see langword="null"/>, the provider will use a new instance of <see cref="JsonRestServices"/>.</param>
        /// <param name="tokenCache">The cache to use for caching user access tokens. If this value is <see langword="null"/>, the provider will use <see cref="UserAccessCache.Instance"/>.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="urlBase"/> is <see langword="null"/>.</exception>
        public HpIdentityProvider(Uri urlBase, IRestService restService, ICache<UserAccess> tokenCache)
            : this(urlBase, null, restService, tokenCache)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HpIdentityProvider"/> class
        /// using the provided values.
        /// </summary>
        /// <param name="urlBase">The base URL for the cloud instance. Predefined URLs are available in <see cref="PredefinedHpIdentityEndpoints"/>.</param>
        /// <param name="defaultIdentity">The default identity to use for calls that do not explicitly specify an identity. If this value is <see langword="null"/>, no default identity is available so all calls must specify an explicit identity.</param>
        /// <param name="restService">The implementation of <see cref="IRestService"/> to use for executing REST requests. If this value is <see langword="null"/>, the provider will use a new instance of <see cref="JsonRestServices"/>.</param>
        /// <param name="tokenCache">The cache to use for caching user access tokens. If this value is <see langword="null"/>, the provider will use <see cref="UserAccessCache.Instance"/>.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="urlBase"/> is <see langword="null"/>.</exception>
        public HpIdentityProvider(Uri urlBase, CloudIdentity defaultIdentity, IRestService restService, ICache<UserAccess> tokenCache)
            : base(defaultIdentity, restService, tokenCache, urlBase)
        {
            if (urlBase == null)
                throw new ArgumentNullException("urlBase");
        }

        /// <inheritdoc/>
        public override UserAccess GetUserAccess(CloudIdentity identity, bool forceCacheRefresh = false)
        {
            if (identity == null)
                throw new ArgumentNullException("identity");

            CloudIdentityWithProject identityWithProject = identity as CloudIdentityWithProject;
            if (identityWithProject == null)
            {
                if (identity.GetType() != typeof(CloudIdentity))
                    throw new NotSupportedException(string.Format("{0} does not support credentials of type {1}", GetType().Name, identity.GetType().Name));
            }

            Func<UserAccess> refreshCallback =
                () =>
                {
                    JObject credentialsObject;
                    if (!string.IsNullOrEmpty(identity.APIKey))
                    {
                        credentialsObject = new JObject(
                            new JProperty("apiAccessKeyCredentials", new JObject(
                                new JProperty("accessKey", identity.APIKey),
                                new JProperty("secretKey", identity.Password))));
                    }
                    else
                    {
                        credentialsObject = new JObject(
                            new JProperty("passwordCredentials", new JObject(
                                new JProperty("username", identity.Username),
                                new JProperty("password", identity.Password))));
                    }

                    JObject authObject = new JObject(credentialsObject);
                    if (identityWithProject != null && identityWithProject.ProjectId != null)
                        authObject.Add("tenantId", JToken.FromObject(identityWithProject.ProjectId));
                    if (identityWithProject != null && !string.IsNullOrEmpty(identityWithProject.ProjectName))
                        authObject.Add("tenantName", JToken.FromObject(identityWithProject.ProjectName));

                    JObject requestBody = new JObject(
                        new JProperty("auth", authObject));

                    var response = ExecuteRESTRequest<JObject>(identity, new Uri(UrlBase, "/v2.0/tokens"), HttpMethod.POST, requestBody, isTokenRequest: true);
                    if (response == null || response.Data == null)
                        return null;

                    JToken userAccessObject = response.Data["access"];
                    if (userAccessObject == null)
                        return null;

                    UserAccess access = userAccessObject.ToObject<UserAccess>();
                    if (access == null || access.Token == null)
                        return null;

                    return access;
                };
            string key = string.Format("{0}:{1}/{2}/{3}/{4}", UrlBase, identityWithProject != null ? identityWithProject.ProjectId : null, identity.Username, identity.APIKey, identity.Password);
            var userAccess = TokenCache.Get(key, refreshCallback, forceCacheRefresh);

            return userAccess;
        }

        /// <inheritdoc />
        protected override string LookupServiceTypeKey(IServiceType serviceType)
        {
            if (ServiceType.ContentDeliveryNetwork.Equals(serviceType))
                return "hpext:cdn";
            return serviceType.Type;
        }
    }
}
