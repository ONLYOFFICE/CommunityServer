namespace net.openstack.Providers.Rackspace
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using net.openstack.Core;
    using net.openstack.Core.Collections;
    using net.openstack.Core.Domain;
    using net.openstack.Core.Providers;
    using net.openstack.Providers.Rackspace.Objects.AutoScale;
    using Newtonsoft.Json.Linq;
    using CancellationToken = System.Threading.CancellationToken;
    using HttpMethod = JSIStudios.SimpleRESTServices.Client.HttpMethod;
    using HttpResponseCodeValidator = net.openstack.Providers.Rackspace.Validators.HttpResponseCodeValidator;
    using IHttpResponseCodeValidator = net.openstack.Core.Validators.IHttpResponseCodeValidator;
    using InternalTaskExtensions = net.openstack.Core.InternalTaskExtensions;
    using IRestService = JSIStudios.SimpleRESTServices.Client.IRestService;
    using JsonRestServices = JSIStudios.SimpleRESTServices.Client.Json.JsonRestServices;

    /// <summary>
    /// Provides an implementation of <see cref="IAutoScaleService"/> for operating
    /// with Rackspace's Auto Scale product.
    /// </summary>
    /// <seealso href="http://docs.rackspace.com/cas/api/v1.0/autoscale-devguide/content/index.html">Rackspace Auto Scale Developer Guide - API v1.0</seealso>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    public class CloudAutoScaleProvider : ProviderBase<IAutoScaleService>, IAutoScaleService
    {
        /// <summary>
        /// This field caches the base URI used for accessing the Auto Scale service.
        /// </summary>
        /// <seealso cref="GetBaseUriAsync"/>
        private Uri _baseUri;

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudAutoScaleProvider"/> class with
        /// the specified values.
        /// </summary>
        /// <param name="defaultIdentity">The default identity to use for calls that do not explicitly specify an identity. If this value is <see langword="null"/>, no default identity is available so all calls must specify an explicit identity.</param>
        /// <param name="defaultRegion">The default region to use for calls that do not explicitly specify a region. If this value is <see langword="null"/>, the default region for the user will be used; otherwise if the service uses region-specific endpoints all calls must specify an explicit region.</param>
        /// <param name="identityProvider">The identity provider to use for authenticating requests to this provider. If this value is <see langword="null"/>, a new instance of <see cref="CloudIdentityProvider"/> is created using <paramref name="defaultIdentity"/> as the default identity.</param>
        public CloudAutoScaleProvider(CloudIdentity defaultIdentity, string defaultRegion, IIdentityProvider identityProvider)
            : base(defaultIdentity, defaultRegion, identityProvider, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudAutoScaleProvider"/> class with
        /// the specified values.
        /// </summary>
        /// <param name="defaultIdentity">The default identity to use for calls that do not explicitly specify an identity. If this value is <see langword="null"/>, no default identity is available so all calls must specify an explicit identity.</param>
        /// <param name="defaultRegion">The default region to use for calls that do not explicitly specify a region. If this value is <see langword="null"/>, the default region for the user will be used; otherwise if the service uses region-specific endpoints all calls must specify an explicit region.</param>
        /// <param name="identityProvider">The identity provider to use for authenticating requests to this provider. If this value is <see langword="null"/>, a new instance of <see cref="CloudIdentityProvider"/> is created using <paramref name="defaultIdentity"/> as the default identity.</param>
        /// <param name="restService">The implementation of <see cref="IRestService"/> to use for executing synchronous REST requests. If this value is <see langword="null"/>, the provider will use a new instance of <see cref="JsonRestServices"/>.</param>
        /// <param name="httpStatusCodeValidator">The HTTP status code validator to use for synchronous REST requests. If this value is <see langword="null"/>, the provider will use <see cref="HttpResponseCodeValidator.Default"/>.</param>
        protected CloudAutoScaleProvider(CloudIdentity defaultIdentity, string defaultRegion, IIdentityProvider identityProvider, IRestService restService, IHttpResponseCodeValidator httpStatusCodeValidator)
            : base(defaultIdentity, defaultRegion, identityProvider, restService, httpStatusCodeValidator)
        {
        }

        #region IAutoScaleService Members

        /// <inheritdoc/>
        public Task<ReadOnlyCollectionPage<ScalingGroup>> ListScalingGroupsAsync(ScalingGroupId marker, int? limit, CancellationToken cancellationToken)
        {
            if (limit <= 0)
                throw new ArgumentOutOfRangeException("limit");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/groups/?marker={marker}&limit={limit}");
            var parameters = new Dictionary<string, string>();
            if (marker != null)
                parameters.Add("marker", marker.Value);
            if (limit != null)
                parameters.Add("limit", limit.ToString());

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<JObject>> requestResource =
                GetResponseAsyncFunc<JObject>(cancellationToken);

            Func<Task<JObject>, ReadOnlyCollectionPage<ScalingGroup>> resultSelector =
                task =>
                {
                    JObject result = task.Result;
                    if (result == null)
                        return null;

                    JToken valuesToken = result["groups"];
                    if (valuesToken == null)
                        return null;

                    JToken linksToken = result["groups_links"];
                    Link[] links = linksToken != null ? linksToken.ToObject<Link[]>() : null;

                    ScalingGroup[] values = valuesToken.ToObject<ScalingGroup[]>();

                    ScalingGroupId nextMarker = values.Any() && (links == null || links.Any(i => string.Equals(i.Rel, "next", StringComparison.OrdinalIgnoreCase))) ? values.Last().Id : null;
                    Func<CancellationToken, Task<ReadOnlyCollectionPage<ScalingGroup>>> getNextPageAsync = null;
                    if (nextMarker != null)
                        getNextPageAsync = nextCancellationToken => ListScalingGroupsAsync(nextMarker, limit, cancellationToken);

                    return new BasicReadOnlyCollectionPage<ScalingGroup>(values, getNextPageAsync);
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task<ScalingGroup> CreateGroupAsync(ScalingGroupConfiguration configuration, CancellationToken cancellationToken)
        {
            if (configuration == null)
                throw new ArgumentNullException("configuration");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/groups");
            var parameters = new Dictionary<string, string>();

            Func<Task<Tuple<IdentityToken, Uri>>, Task<HttpWebRequest>> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.POST, template, parameters, configuration);

            Func<Task<HttpWebRequest>, Task<JObject>> requestResource =
                GetResponseAsyncFunc<JObject>(cancellationToken);

            Func<Task<JObject>, ScalingGroup> resultSelector =
                task =>
                {
                    JObject result = task.Result;
                    if (result == null)
                        return null;

                    JToken valueToken = result["group"];
                    if (valueToken == null)
                        return null;

                    return valueToken.ToObject<ScalingGroup>();
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Then(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task<ScalingGroup> GetGroupAsync(ScalingGroupId groupId, CancellationToken cancellationToken)
        {
            if (groupId == null)
                throw new ArgumentNullException("groupId");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/groups/{groupId}");
            var parameters = new Dictionary<string, string> { { "groupId", groupId.Value } };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<JObject>> requestResource =
                GetResponseAsyncFunc<JObject>(cancellationToken);

            Func<Task<JObject>, ScalingGroup> resultSelector =
                task =>
                {
                    JObject result = task.Result;
                    if (result == null)
                        return null;

                    JToken valueToken = result["group"];
                    if (valueToken == null)
                        return null;

                    return valueToken.ToObject<ScalingGroup>();
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task DeleteGroupAsync(ScalingGroupId groupId, bool? force, CancellationToken cancellationToken)
        {
            if (groupId == null)
                throw new ArgumentNullException("groupId");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/groups/{groupId}?force={force}");
            var parameters = new Dictionary<string, string> { { "groupId", groupId.Value } };
            if (force ?? false)
                parameters.Add("force", force.ToString().ToLowerInvariant());

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.DELETE, template, parameters);

            Func<Task<HttpWebRequest>, Task<string>> requestResource =
                GetResponseAsyncFunc(cancellationToken);

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource);
        }

        /// <inheritdoc/>
        public Task<GroupState> GetGroupStateAsync(ScalingGroupId groupId, CancellationToken cancellationToken)
        {
            if (groupId == null)
                throw new ArgumentNullException("groupId");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/groups/{groupId}/state");
            var parameters = new Dictionary<string, string> { { "groupId", groupId.Value } };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<JObject>> requestResource =
                GetResponseAsyncFunc<JObject>(cancellationToken);

            Func<Task<JObject>, GroupState> resultSelector =
                task =>
                {
                    JObject result = task.Result;
                    if (result == null)
                        return null;

                    JToken valueToken = result["group"];
                    if (valueToken == null)
                        return null;

                    return valueToken.ToObject<GroupState>();
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task PauseGroupAsync(ScalingGroupId groupId, CancellationToken cancellationToken)
        {
            if (groupId == null)
                throw new ArgumentNullException("groupId");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/groups/{groupId}/pause");
            var parameters = new Dictionary<string, string> { { "groupId", groupId.Value } };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.POST, template, parameters);

            Func<Task<HttpWebRequest>, Task<string>> requestResource =
                GetResponseAsyncFunc(cancellationToken);

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource);
        }

        /// <inheritdoc/>
        public Task ResumeGroupAsync(ScalingGroupId groupId, CancellationToken cancellationToken)
        {
            if (groupId == null)
                throw new ArgumentNullException("groupId");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/groups/{groupId}/resume");
            var parameters = new Dictionary<string, string> { { "groupId", groupId.Value } };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.POST, template, parameters);

            Func<Task<HttpWebRequest>, Task<string>> requestResource =
                GetResponseAsyncFunc(cancellationToken);

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource);
        }

        /// <inheritdoc/>
        public Task<GroupConfiguration> GetGroupConfigurationAsync(ScalingGroupId groupId, CancellationToken cancellationToken)
        {
            if (groupId == null)
                throw new ArgumentNullException("groupId");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/groups/{groupId}/config");
            var parameters = new Dictionary<string, string> { { "groupId", groupId.Value } };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<JObject>> requestResource =
                GetResponseAsyncFunc<JObject>(cancellationToken);

            Func<Task<JObject>, GroupConfiguration> resultSelector =
                task =>
                {
                    JObject result = task.Result;
                    if (result == null)
                        return null;

                    JToken valueToken = result["groupConfiguration"];
                    if (valueToken == null)
                        return null;

                    return valueToken.ToObject<GroupConfiguration>();
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task SetGroupConfigurationAsync(ScalingGroupId groupId, GroupConfiguration configuration, CancellationToken cancellationToken)
        {
            if (groupId == null)
                throw new ArgumentNullException("groupId");
            if (configuration == null)
                throw new ArgumentNullException("configuration");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/groups/{groupId}/config");
            var parameters = new Dictionary<string, string> { { "groupId", groupId.Value } };

            Func<Task<Tuple<IdentityToken, Uri>>, Task<HttpWebRequest>> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.PUT, template, parameters, configuration);

            Func<Task<HttpWebRequest>, Task<string>> requestResource =
                GetResponseAsyncFunc(cancellationToken);

            return AuthenticateServiceAsync(cancellationToken)
                .Then(prepareRequest)
                .Then(requestResource);
        }

        /// <inheritdoc/>
        public Task<LaunchConfiguration> GetLaunchConfigurationAsync(ScalingGroupId groupId, CancellationToken cancellationToken)
        {
            if (groupId == null)
                throw new ArgumentNullException("groupId");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/groups/{groupId}/launch");
            var parameters = new Dictionary<string, string> { { "groupId", groupId.Value } };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<JObject>> requestResource =
                GetResponseAsyncFunc<JObject>(cancellationToken);

            Func<Task<JObject>, LaunchConfiguration> resultSelector =
                task =>
                {
                    JObject result = task.Result;
                    if (result == null)
                        return null;

                    JObject valueToken = result["launchConfiguration"] as JObject;
                    if (valueToken == null)
                        return null;

                    return LaunchConfiguration.FromJObject(valueToken);
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task SetLaunchConfigurationAsync(ScalingGroupId groupId, LaunchConfiguration configuration, CancellationToken cancellationToken)
        {
            if (groupId == null)
                throw new ArgumentNullException("groupId");
            if (configuration == null)
                throw new ArgumentNullException("configuration");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/groups/{groupId}/launch");
            var parameters = new Dictionary<string, string> { { "groupId", groupId.Value } };

            Func<Task<Tuple<IdentityToken, Uri>>, Task<HttpWebRequest>> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.PUT, template, parameters, configuration);

            Func<Task<HttpWebRequest>, Task<string>> requestResource =
                GetResponseAsyncFunc(cancellationToken);

            return AuthenticateServiceAsync(cancellationToken)
                .Then(prepareRequest)
                .Then(requestResource);
        }

        /// <inheritdoc/>
        public Task<ReadOnlyCollectionPage<Policy>> ListPoliciesAsync(ScalingGroupId groupId, PolicyId marker, int? limit, CancellationToken cancellationToken)
        {
            if (groupId == null)
                throw new ArgumentNullException("groupId");
            if (limit <= 0)
                throw new ArgumentOutOfRangeException("limit");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/groups/{groupId}/policies/?marker={marker}&limit={limit}");
            var parameters = new Dictionary<string, string>() { { "groupId", groupId.Value } };
            if (marker != null)
                parameters.Add("marker", marker.Value);
            if (limit != null)
                parameters.Add("limit", limit.ToString());

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<JObject>> requestResource =
                GetResponseAsyncFunc<JObject>(cancellationToken);

            Func<Task<JObject>, ReadOnlyCollectionPage<Policy>> resultSelector =
                task =>
                {
                    JObject result = task.Result;
                    if (result == null)
                        return null;

                    JToken valuesToken = result["policies"];
                    if (valuesToken == null)
                        return null;

                    JToken linksToken = result["policies_links"];
                    Link[] links = linksToken != null ? linksToken.ToObject<Link[]>() : null;

                    Policy[] values = valuesToken.ToObject<Policy[]>();

                    PolicyId nextMarker = values.Any() && (links == null || links.Any(i => string.Equals(i.Rel, "next", StringComparison.OrdinalIgnoreCase))) ? values.Last().Id : null;
                    Func<CancellationToken, Task<ReadOnlyCollectionPage<Policy>>> getNextPageAsync = null;
                    if (nextMarker != null)
                        getNextPageAsync = nextCancellationToken => ListPoliciesAsync(groupId, nextMarker, limit, cancellationToken);

                    return new BasicReadOnlyCollectionPage<Policy>(values, getNextPageAsync);
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task<Policy> CreatePolicyAsync(ScalingGroupId groupId, PolicyConfiguration configuration, CancellationToken cancellationToken)
        {
            if (groupId == null)
                throw new ArgumentNullException("groupId");
            if (configuration == null)
                throw new ArgumentNullException("configuration");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/groups/{groupId}/policies");
            var parameters = new Dictionary<string, string> { { "groupId", groupId.Value } };

            Func<Task<Tuple<IdentityToken, Uri>>, Task<HttpWebRequest>> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.POST, template, parameters, new[] { configuration });

            Func<Task<HttpWebRequest>, Task<JObject>> requestResource =
                GetResponseAsyncFunc<JObject>(cancellationToken);

            Func<Task<JObject>, Policy> resultSelector =
                task =>
                {
                    JObject result = task.Result;
                    if (result == null)
                        return null;

                    JToken valueToken = result["policies"];
                    if (valueToken == null)
                        return null;

                    Policy[] policies = valueToken.ToObject<Policy[]>();
                    return policies[0];
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Then(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task<Policy> GetPolicyAsync(ScalingGroupId groupId, PolicyId policyId, CancellationToken cancellationToken)
        {
            if (groupId == null)
                throw new ArgumentNullException("groupId");
            if (policyId == null)
                throw new ArgumentNullException("policyId");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/groups/{groupId}/policies/{policyId}");
            var parameters = new Dictionary<string, string> { { "groupId", groupId.Value }, { "policyId", policyId.Value } };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<JObject>> requestResource =
                GetResponseAsyncFunc<JObject>(cancellationToken);

            Func<Task<JObject>, Policy> resultSelector =
                task =>
                {
                    JObject result = task.Result;
                    if (result == null)
                        return null;

                    JToken valueToken = result["policy"];
                    if (valueToken == null)
                        return null;

                    return valueToken.ToObject<Policy>();
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task SetPolicyAsync(ScalingGroupId groupId, PolicyId policyId, PolicyConfiguration configuration, CancellationToken cancellationToken)
        {
            if (groupId == null)
                throw new ArgumentNullException("groupId");
            if (policyId == null)
                throw new ArgumentNullException("policyId");
            if (configuration == null)
                throw new ArgumentNullException("configuration");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/groups/{groupId}/policies/{policyId}");
            var parameters = new Dictionary<string, string> { { "groupId", groupId.Value }, { "policyId", policyId.Value } };

            Func<Task<Tuple<IdentityToken, Uri>>, Task<HttpWebRequest>> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.PUT, template, parameters, configuration);

            Func<Task<HttpWebRequest>, Task<string>> requestResource =
                GetResponseAsyncFunc(cancellationToken);

            return AuthenticateServiceAsync(cancellationToken)
                .Then(prepareRequest)
                .Then(requestResource);
        }

        /// <inheritdoc/>
        public Task DeletePolicyAsync(ScalingGroupId groupId, PolicyId policyId, CancellationToken cancellationToken)
        {
            if (groupId == null)
                throw new ArgumentNullException("groupId");
            if (policyId == null)
                throw new ArgumentNullException("policyId");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/groups/{groupId}/policies/{policyId}");
            var parameters = new Dictionary<string, string> { { "groupId", groupId.Value }, { "policyId", policyId.Value } };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.DELETE, template, parameters);

            Func<Task<HttpWebRequest>, Task<string>> requestResource =
                GetResponseAsyncFunc(cancellationToken);

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource);
        }

        /// <inheritdoc/>
        public Task ExecutePolicyAsync(ScalingGroupId groupId, PolicyId policyId, CancellationToken cancellationToken)
        {
            if (groupId == null)
                throw new ArgumentNullException("groupId");
            if (policyId == null)
                throw new ArgumentNullException("policyId");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/groups/{groupId}/policies/{policyId}/execute");
            var parameters = new Dictionary<string, string> { { "groupId", groupId.Value }, { "policyId", policyId.Value } };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.POST, template, parameters);

            Func<Task<HttpWebRequest>, Task<string>> requestResource =
                GetResponseAsyncFunc(cancellationToken);

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource);
        }

        /// <inheritdoc/>
        public Task<ReadOnlyCollectionPage<Webhook>> ListWebhooksAsync(ScalingGroupId groupId, PolicyId policyId, WebhookId marker, int? limit, CancellationToken cancellationToken)
        {
            if (groupId == null)
                throw new ArgumentNullException("groupId");
            if (policyId == null)
                throw new ArgumentNullException("policyId");
            if (limit <= 0)
                throw new ArgumentOutOfRangeException("limit");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/groups/{groupId}/policies/{policyId}/webhooks/?marker={marker}&limit={limit}");
            var parameters = new Dictionary<string, string>() { { "groupId", groupId.Value }, { "policyId", policyId.Value } };
            if (marker != null)
                parameters.Add("marker", marker.Value);
            if (limit != null)
                parameters.Add("limit", limit.ToString());

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<JObject>> requestResource =
                GetResponseAsyncFunc<JObject>(cancellationToken);

            Func<Task<JObject>, ReadOnlyCollectionPage<Webhook>> resultSelector =
                task =>
                {
                    JObject result = task.Result;
                    if (result == null)
                        return null;

                    JToken valuesToken = result["webhooks"];
                    if (valuesToken == null)
                        return null;

                    JToken linksToken = result["webhooks_links"];
                    Link[] links = linksToken != null ? linksToken.ToObject<Link[]>() : null;

                    Webhook[] values = valuesToken.ToObject<Webhook[]>();

                    WebhookId nextMarker = values.Any() && (links == null || links.Any(i => string.Equals(i.Rel, "next", StringComparison.OrdinalIgnoreCase))) ? values.Last().Id : null;
                    Func<CancellationToken, Task<ReadOnlyCollectionPage<Webhook>>> getNextPageAsync = null;
                    if (nextMarker != null)
                        getNextPageAsync = nextCancellationToken => ListWebhooksAsync(groupId, policyId, nextMarker, limit, cancellationToken);

                    return new BasicReadOnlyCollectionPage<Webhook>(values, getNextPageAsync);
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task<Webhook> CreateWebhookAsync(ScalingGroupId groupId, PolicyId policyId, NewWebhookConfiguration configuration, CancellationToken cancellationToken)
        {
            if (groupId == null)
                throw new ArgumentNullException("groupId");
            if (policyId == null)
                throw new ArgumentNullException("policyId");
            if (configuration == null)
                throw new ArgumentNullException("configuration");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/groups/{groupId}/policies/{policyId}/webhooks");
            var parameters = new Dictionary<string, string> { { "groupId", groupId.Value }, { "policyId", policyId.Value } };

            Func<Task<Tuple<IdentityToken, Uri>>, Task<HttpWebRequest>> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.POST, template, parameters, new[] { configuration });

            Func<Task<HttpWebRequest>, Task<JObject>> requestResource =
                GetResponseAsyncFunc<JObject>(cancellationToken);

            Func<Task<JObject>, Webhook> resultSelector =
                task =>
                {
                    JObject result = task.Result;
                    if (result == null)
                        return null;

                    JToken valueToken = result["webhooks"];
                    if (valueToken == null)
                        return null;

                    Webhook[] webhooks = valueToken.ToObject<Webhook[]>();
                    return webhooks[0];
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Then(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task<ReadOnlyCollection<Webhook>> CreateWebhookRangeAsync(ScalingGroupId groupId, PolicyId policyId, IEnumerable<NewWebhookConfiguration> configurations, CancellationToken cancellationToken)
        {
            if (groupId == null)
                throw new ArgumentNullException("groupId");
            if (policyId == null)
                throw new ArgumentNullException("policyId");
            if (configurations == null)
                throw new ArgumentNullException("configurations");

            NewWebhookConfiguration[] configurationsArray = configurations.ToArray();
            if (configurationsArray.Contains(null))
                throw new ArgumentException("configurations cannot contain any null values", "configurations");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/groups/{groupId}/policies/{policyId}/webhooks");
            var parameters = new Dictionary<string, string> { { "groupId", groupId.Value }, { "policyId", policyId.Value } };

            Func<Task<Tuple<IdentityToken, Uri>>, Task<HttpWebRequest>> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.POST, template, parameters, configurations);

            Func<Task<HttpWebRequest>, Task<JObject>> requestResource =
                GetResponseAsyncFunc<JObject>(cancellationToken);

            Func<Task<JObject>, ReadOnlyCollection<Webhook>> resultSelector =
                task =>
                {
                    JObject result = task.Result;
                    if (result == null)
                        return null;

                    JToken valueToken = result["webhooks"];
                    if (valueToken == null)
                        return null;

                    ReadOnlyCollection<Webhook> webhooks = valueToken.ToObject<ReadOnlyCollection<Webhook>>();
                    return webhooks;
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Then(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task<Webhook> GetWebhookAsync(ScalingGroupId groupId, PolicyId policyId, WebhookId webhookId, CancellationToken cancellationToken)
        {
            if (groupId == null)
                throw new ArgumentNullException("groupId");
            if (policyId == null)
                throw new ArgumentNullException("policyId");
            if (webhookId == null)
                throw new ArgumentNullException("webhookId");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/groups/{groupId}/policies/{policyId}/webhooks/{webhookId}");
            var parameters = new Dictionary<string, string> { { "groupId", groupId.Value }, { "policyId", policyId.Value }, { "webhookId", webhookId.Value } };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<JObject>> requestResource =
                GetResponseAsyncFunc<JObject>(cancellationToken);

            Func<Task<JObject>, Webhook> resultSelector =
                task =>
                {
                    JObject result = task.Result;
                    if (result == null)
                        return null;

                    JToken valueToken = result["webhook"];
                    if (valueToken == null)
                        return null;

                    return valueToken.ToObject<Webhook>();
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task UpdateWebhookAsync(ScalingGroupId groupId, PolicyId policyId, WebhookId webhookId, UpdateWebhookConfiguration configuration, CancellationToken cancellationToken)
        {
            if (groupId == null)
                throw new ArgumentNullException("groupId");
            if (policyId == null)
                throw new ArgumentNullException("policyId");
            if (webhookId == null)
                throw new ArgumentNullException("webhookId");
            if (configuration == null)
                throw new ArgumentNullException("configuration");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/groups/{groupId}/policies/{policyId}/webhooks/{webhookId}");
            var parameters = new Dictionary<string, string> { { "groupId", groupId.Value }, { "policyId", policyId.Value }, { "webhookId", webhookId.Value } };

            Func<Task<Tuple<IdentityToken, Uri>>, Task<HttpWebRequest>> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.PUT, template, parameters, configuration);

            Func<Task<HttpWebRequest>, Task<string>> requestResource =
                GetResponseAsyncFunc(cancellationToken);

            return AuthenticateServiceAsync(cancellationToken)
                .Then(prepareRequest)
                .Then(requestResource);
        }

        /// <inheritdoc/>
        public Task DeleteWebhookAsync(ScalingGroupId groupId, PolicyId policyId, WebhookId webhookId, CancellationToken cancellationToken)
        {
            if (groupId == null)
                throw new ArgumentNullException("groupId");
            if (policyId == null)
                throw new ArgumentNullException("policyId");
            if (webhookId == null)
                throw new ArgumentNullException("webhookId");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/groups/{groupId}/policies/{policyId}/webhooks/{webhookId}");
            var parameters = new Dictionary<string, string> { { "groupId", groupId.Value }, { "policyId", policyId.Value }, { "webhookId", webhookId.Value } };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.DELETE, template, parameters);

            Func<Task<HttpWebRequest>, Task<string>> requestResource =
                GetResponseAsyncFunc(cancellationToken);

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource);
        }

        #endregion

        /// <inheritdoc/>
        /// <remarks>
        /// This method returns a cached base address if one is available. If no cached address is
        /// available, <see cref="ProviderBase{TProvider}.GetServiceEndpoint"/> is called to obtain
        /// an <see cref="Endpoint"/> with the type <c>rax:autoscale</c> and preferred name <c>autoscale</c>.
        /// </remarks>
        protected override Task<Uri> GetBaseUriAsync(CancellationToken cancellationToken)
        {
            if (_baseUri != null)
            {
                return InternalTaskExtensions.CompletedTask(_baseUri);
            }

            return Task.Factory.StartNew(
                () =>
                {
                    Endpoint endpoint = GetServiceEndpoint(null, "rax:autoscale", "autoscale", null);
                    _baseUri = new Uri(endpoint.PublicURL);
                    return _baseUri;
                });
        }
    }
}
