namespace net.openstack.Providers.Rackspace
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using net.openstack.Core;
    using net.openstack.Core.Domain;
    using net.openstack.Core.Providers;
    using net.openstack.Providers.Rackspace.Objects.Monitoring;
    using Newtonsoft.Json.Linq;
    using CancellationToken = System.Threading.CancellationToken;
    using HttpMethod = JSIStudios.SimpleRESTServices.Client.HttpMethod;
    using HttpResponseCodeValidator = net.openstack.Providers.Rackspace.Validators.HttpResponseCodeValidator;
    using IHttpResponseCodeValidator = net.openstack.Core.Validators.IHttpResponseCodeValidator;
    using IRestService = JSIStudios.SimpleRESTServices.Client.IRestService;
    using JsonRestServices = JSIStudios.SimpleRESTServices.Client.Json.JsonRestServices;

    /// <summary>
    /// Provides an implementation of <see cref="IMonitoringService"/> for operating
    /// with Rackspace's Cloud Monitoring product.
    /// </summary>
    /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/overview.html">Rackspace Cloud Monitoring Developer Guide - API v1.0</seealso>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    public class CloudMonitoringProvider : ProviderBase<IMonitoringService>, IMonitoringService
    {
        /// <summary>
        /// This field caches the base URI used for accessing the Cloud Monitoring service.
        /// </summary>
        /// <seealso cref="GetBaseUriAsync"/>
        private Uri _baseUri;

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudMonitoringProvider"/> class with
        /// the specified values.
        /// </summary>
        /// <param name="defaultIdentity">The default identity to use for calls that do not explicitly specify an identity. If this value is <see langword="null"/>, no default identity is available so all calls must specify an explicit identity.</param>
        /// <param name="defaultRegion">The default region to use for calls that do not explicitly specify a region. If this value is <see langword="null"/>, the default region for the user will be used; otherwise if the service uses region-specific endpoints all calls must specify an explicit region.</param>
        /// <param name="identityProvider">The identity provider to use for authenticating requests to this provider. If this value is <see langword="null"/>, a new instance of <see cref="CloudIdentityProvider"/> is created using <paramref name="defaultIdentity"/> as the default identity.</param>
        public CloudMonitoringProvider(CloudIdentity defaultIdentity, string defaultRegion, IIdentityProvider identityProvider)
            : base(defaultIdentity, defaultRegion, identityProvider, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudMonitoringProvider"/> class with
        /// the specified values.
        /// </summary>
        /// <param name="defaultIdentity">The default identity to use for calls that do not explicitly specify an identity. If this value is <see langword="null"/>, no default identity is available so all calls must specify an explicit identity.</param>
        /// <param name="defaultRegion">The default region to use for calls that do not explicitly specify a region. If this value is <see langword="null"/>, the default region for the user will be used; otherwise if the service uses region-specific endpoints all calls must specify an explicit region.</param>
        /// <param name="identityProvider">The identity provider to use for authenticating requests to this provider. If this value is <see langword="null"/>, a new instance of <see cref="CloudIdentityProvider"/> is created using <paramref name="defaultIdentity"/> as the default identity.</param>
        /// <param name="restService">The implementation of <see cref="IRestService"/> to use for executing synchronous REST requests. If this value is <see langword="null"/>, the provider will use a new instance of <see cref="JsonRestServices"/>.</param>
        /// <param name="httpStatusCodeValidator">The HTTP status code validator to use for synchronous REST requests. If this value is <see langword="null"/>, the provider will use <see cref="HttpResponseCodeValidator.Default"/>.</param>
        protected CloudMonitoringProvider(CloudIdentity defaultIdentity, string defaultRegion, IIdentityProvider identityProvider, IRestService restService, IHttpResponseCodeValidator httpStatusCodeValidator)
            : base(defaultIdentity, defaultRegion, identityProvider, restService, httpStatusCodeValidator)
        {
        }

        #region IMonitoringService Members

        /// <inheritdoc/>
        public Task<MonitoringAccount> GetAccountAsync(CancellationToken cancellationToken)
        {
            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/account");
            var parameters = new Dictionary<string, string>();

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<MonitoringAccount>> requestResource =
                GetResponseAsyncFunc<MonitoringAccount>(cancellationToken);

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource);
        }

        /// <inheritdoc/>
        public Task UpdateAccountAsync(MonitoringAccountId accountId, AccountConfiguration configuration, CancellationToken cancellationToken)
        {
            if (accountId == null)
                throw new ArgumentNullException("accountId");
            if (configuration == null)
                throw new ArgumentNullException("configuration");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/account");
            var parameters = new Dictionary<string, string>();

            Func<Task<Tuple<IdentityToken, Uri>>, Task<HttpWebRequest>> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.PUT, template, parameters, configuration);

            Func<Task<HttpWebRequest>, Task<string>> requestResource =
                GetResponseAsyncFunc(cancellationToken);

            return AuthenticateServiceAsync(cancellationToken)
                .Then(prepareRequest)
                .Then(requestResource);
        }

        /// <inheritdoc/>
        public Task<MonitoringLimits> GetLimitsAsync(CancellationToken cancellationToken)
        {
            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/limits");
            var parameters = new Dictionary<string, string>();

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<MonitoringLimits>> requestResource =
                GetResponseAsyncFunc<MonitoringLimits>(cancellationToken);

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource);
        }

        /// <inheritdoc/>
        public Task<ReadOnlyCollectionPage<Audit, AuditId>> ListAuditsAsync(AuditId marker, int? limit, DateTimeOffset? from, DateTimeOffset? to, CancellationToken cancellationToken)
        {
            if (limit < 0)
                throw new ArgumentOutOfRangeException("limit");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/audits?marker={marker}&limit={limit}&from={from}&to={to}");
            var parameters = new Dictionary<string, string>();
            if (marker != null)
                parameters.Add("marker", marker.Value);
            if (limit != null)
                parameters.Add("limit", limit.ToString());
            if (from != null)
                parameters.Add("from", from.Value.ToTimestamp().ToString());
            if (to != null)
                parameters.Add("to", to.Value.ToTimestamp().ToString());

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<JObject>> requestResource =
                GetResponseAsyncFunc<JObject>(cancellationToken);

            Func<Task<JObject>, ReadOnlyCollectionPage<Audit, AuditId>> resultSelector =
                task =>
                {
                    JObject result = task.Result;
                    if (result == null)
                        return null;

                    JToken valuesToken = result["values"];
                    if (valuesToken == null)
                        return null;

                    JToken metadataToken = result["metadata"];

                    Audit[] values = valuesToken.ToObject<Audit[]>();
                    Func<AuditId, CancellationToken, Task<ReadOnlyCollectionPage<Audit, AuditId>>> getNextPageAsync =
                        (nextMarker, nextCancellationToken) => ListAuditsAsync(nextMarker, limit, from, to, nextCancellationToken);
                    IDictionary<string, object> metadata = metadataToken != null ? metadataToken.ToObject<IDictionary<string, object>>() : null;
                    return new ReadOnlyCollectionPage<Audit, AuditId>(values, getNextPageAsync, metadata);
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task<EntityId> CreateEntityAsync(NewEntityConfiguration configuration, CancellationToken cancellationToken)
        {
            if (configuration == null)
                throw new ArgumentNullException("configuration");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/entities");
            var parameters = new Dictionary<string, string>();

            Func<Task<Tuple<IdentityToken, Uri>>, Task<HttpWebRequest>> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.POST, template, parameters, configuration);

            Func<Task<Tuple<HttpWebResponse, string>>, Task<EntityId>> parseResult =
                task =>
                {
                    UriTemplate.UriTemplate entityTemplate = new UriTemplate.UriTemplate("/entities/{entityId}");
                    string location = task.Result.Item1.Headers[HttpResponseHeader.Location];
                    UriTemplate.UriTemplateMatch match = entityTemplate.Match(_baseUri, new Uri(location));
                    return InternalTaskExtensions.CompletedTask(new EntityId(match.BoundVariables["entityId"]));
                };

            Func<Task<HttpWebRequest>, Task<EntityId>> requestResource =
                GetResponseAsyncFunc(cancellationToken, parseResult);

            return AuthenticateServiceAsync(cancellationToken)
                .Then(prepareRequest)
                .Then(requestResource);
        }

        /// <inheritdoc/>
        public Task<ReadOnlyCollectionPage<Entity, EntityId>> ListEntitiesAsync(EntityId marker, int? limit, CancellationToken cancellationToken)
        {
            if (limit < 0)
                throw new ArgumentOutOfRangeException("limit");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/entities?marker={marker}&limit={limit}");
            var parameters = new Dictionary<string, string>();
            if (marker != null)
                parameters.Add("marker", marker.Value);
            if (limit != null)
                parameters.Add("limit", limit.ToString());

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<JObject>> requestResource =
                GetResponseAsyncFunc<JObject>(cancellationToken);

            Func<Task<JObject>, ReadOnlyCollectionPage<Entity, EntityId>> resultSelector =
                task =>
                {
                    JObject result = task.Result;
                    if (result == null)
                        return null;

                    JToken valuesToken = result["values"];
                    if (valuesToken == null)
                        return null;

                    JToken metadataToken = result["metadata"];

                    Entity[] values = valuesToken.ToObject<Entity[]>();
                    Func<EntityId, CancellationToken, Task<ReadOnlyCollectionPage<Entity, EntityId>>> getNextPageAsync =
                        (nextMarker, nextCancellationToken) => ListEntitiesAsync(nextMarker, limit, nextCancellationToken);
                    IDictionary<string, object> metadata = metadataToken != null ? metadataToken.ToObject<IDictionary<string, object>>() : null;
                    return new ReadOnlyCollectionPage<Entity, EntityId>(values, getNextPageAsync, metadata);
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task<Entity> GetEntityAsync(EntityId entityId, CancellationToken cancellationToken)
        {
            if (entityId == null)
                throw new ArgumentNullException("entityId");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/entities/{entityId}");
            var parameters = new Dictionary<string, string> { { "entityId", entityId.Value } };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<Entity>> requestResource =
                GetResponseAsyncFunc<Entity>(cancellationToken);

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource);
        }

        /// <inheritdoc/>
        public Task UpdateEntityAsync(EntityId entityId, UpdateEntityConfiguration configuration, CancellationToken cancellationToken)
        {
            if (entityId == null)
                throw new ArgumentNullException("entityId");
            if (configuration == null)
                throw new ArgumentNullException("configuration");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/entities/{entityId}");
            var parameters = new Dictionary<string, string> { { "entityId", entityId.Value } };

            Func<Task<Tuple<IdentityToken, Uri>>, Task<HttpWebRequest>> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.PUT, template, parameters, configuration);

            Func<Task<HttpWebRequest>, Task<string>> requestResource =
                GetResponseAsyncFunc(cancellationToken);

            return AuthenticateServiceAsync(cancellationToken)
                .Then(prepareRequest)
                .Then(requestResource);
        }

        /// <inheritdoc/>
        public Task RemoveEntityAsync(EntityId entityId, CancellationToken cancellationToken)
        {
            if (entityId == null)
                throw new ArgumentNullException("entityId");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/entities/{entityId}");
            var parameters = new Dictionary<string, string> { { "entityId", entityId.Value } };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.DELETE, template, parameters);

            Func<Task<HttpWebRequest>, Task<string>> requestResource =
                GetResponseAsyncFunc(cancellationToken);

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource);
        }

        /// <inheritdoc/>
        public Task<CheckId> CreateCheckAsync(EntityId entityId, NewCheckConfiguration configuration, CancellationToken cancellationToken)
        {
            if (entityId == null)
                throw new ArgumentNullException("entityId");
            if (configuration == null)
                throw new ArgumentNullException("configuration");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/entities/{entityId}/checks");
            var parameters = new Dictionary<string, string> { { "entityId", entityId.Value } };

            Func<Task<Tuple<IdentityToken, Uri>>, Task<HttpWebRequest>> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.POST, template, parameters, configuration);

            Func<Task<Tuple<HttpWebResponse, string>>, Task<CheckId>> parseResult =
                task =>
                {
                    UriTemplate.UriTemplate entityTemplate = new UriTemplate.UriTemplate("/entities/{entityId}/checks/{checkId}");
                    string location = task.Result.Item1.Headers[HttpResponseHeader.Location];
                    UriTemplate.UriTemplateMatch match = entityTemplate.Match(_baseUri, new Uri(location));
                    return InternalTaskExtensions.CompletedTask(new CheckId(match.BoundVariables["checkId"]));
                };

            Func<Task<HttpWebRequest>, Task<CheckId>> requestResource =
                GetResponseAsyncFunc(cancellationToken, parseResult);

            return AuthenticateServiceAsync(cancellationToken)
                .Then(prepareRequest)
                .Then(requestResource);
        }

        /// <inheritdoc/>
        public Task<ReadOnlyCollection<CheckData>> TestCheckAsync(EntityId entityId, NewCheckConfiguration configuration, bool? debug, CancellationToken cancellationToken)
        {
            if (entityId == null)
                throw new ArgumentNullException("entityId");
            if (configuration == null)
                throw new ArgumentNullException("configuration");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/entities/{entityId}/test-check?debug={debug}");
            var parameters = new Dictionary<string, string> { { "entityId", entityId.Value } };
            if (debug != null)
                parameters.Add("debug", debug.ToString());

            Func<Task<Tuple<IdentityToken, Uri>>, Task<HttpWebRequest>> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.POST, template, parameters, configuration);

            Func<Task<HttpWebRequest>, Task<ReadOnlyCollection<CheckData>>> requestResource =
                GetResponseAsyncFunc<ReadOnlyCollection<CheckData>>(cancellationToken);

            return AuthenticateServiceAsync(cancellationToken)
                .Then(prepareRequest)
                .Then(requestResource);
        }

        /// <inheritdoc/>
        public Task<ReadOnlyCollection<CheckData>> TestExistingCheckAsync(EntityId entityId, CheckId checkId, CancellationToken cancellationToken)
        {
            if (entityId == null)
                throw new ArgumentNullException("entityId");
            if (checkId == null)
                throw new ArgumentNullException("checkId");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/entities/{entityId}/checks/{checkId}/test");
            var parameters = new Dictionary<string, string> { { "entityId", entityId.Value }, { "checkId", checkId.Value } };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.POST, template, parameters);

            Func<Task<HttpWebRequest>, Task<ReadOnlyCollection<CheckData>>> requestResource =
                GetResponseAsyncFunc<ReadOnlyCollection<CheckData>>(cancellationToken);

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource);
        }

        /// <inheritdoc/>
        public Task<ReadOnlyCollectionPage<Check, CheckId>> ListChecksAsync(EntityId entityId, CheckId marker, int? limit, CancellationToken cancellationToken)
        {
            if (entityId == null)
                throw new ArgumentNullException("entityId");
            if (limit < 0)
                throw new ArgumentOutOfRangeException("limit");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/entities/{entityId}/checks?marker={marker}&limit={limit}");
            var parameters = new Dictionary<string, string> { { "entityId", entityId.Value } };
            if (marker != null)
                parameters.Add("marker", marker.Value);
            if (limit != null)
                parameters.Add("limit", limit.ToString());

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<JObject>> requestResource =
                GetResponseAsyncFunc<JObject>(cancellationToken);

            Func<Task<JObject>, ReadOnlyCollectionPage<Check, CheckId>> resultSelector =
                task =>
                {
                    JObject result = task.Result;
                    if (result == null)
                        return null;

                    JToken valuesToken = result["values"];
                    if (valuesToken == null)
                        return null;

                    JToken metadataToken = result["metadata"];

                    Check[] values = valuesToken.ToObject<Check[]>();
                    Func<CheckId, CancellationToken, Task<ReadOnlyCollectionPage<Check, CheckId>>> getNextPageAsync =
                        (nextMarker, nextCancellationToken) => ListChecksAsync(entityId, nextMarker, limit, nextCancellationToken);
                    IDictionary<string, object> metadata = metadataToken != null ? metadataToken.ToObject<IDictionary<string, object>>() : null;
                    return new ReadOnlyCollectionPage<Check, CheckId>(values, getNextPageAsync, metadata);
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task<Check> GetCheckAsync(EntityId entityId, CheckId checkId, CancellationToken cancellationToken)
        {
            if (entityId == null)
                throw new ArgumentNullException("entityId");
            if (checkId == null)
                throw new ArgumentNullException("checkId");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/entities/{entityId}/checks/{checkId}");
            var parameters = new Dictionary<string, string> { { "entityId", entityId.Value }, { "checkId", checkId.Value } };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<Check>> requestResource =
                GetResponseAsyncFunc<Check>(cancellationToken);

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource);
        }

        /// <inheritdoc/>
        public Task UpdateCheckAsync(EntityId entityId, CheckId checkId, UpdateCheckConfiguration configuration, CancellationToken cancellationToken)
        {
            if (entityId == null)
                throw new ArgumentNullException("entityId");
            if (checkId == null)
                throw new ArgumentNullException("checkId");
            if (configuration == null)
                throw new ArgumentNullException("configuration");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/entities/{entityId}/checks/{checkId}");
            var parameters = new Dictionary<string, string> { { "entityId", entityId.Value }, { "checkId", checkId.Value } };

            Func<Task<Tuple<IdentityToken, Uri>>, Task<HttpWebRequest>> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.PUT, template, parameters, configuration);

            Func<Task<HttpWebRequest>, Task<string>> requestResource =
                GetResponseAsyncFunc(cancellationToken);

            return AuthenticateServiceAsync(cancellationToken)
                .Then(prepareRequest)
                .Then(requestResource);
        }

        /// <inheritdoc/>
        public Task RemoveCheckAsync(EntityId entityId, CheckId checkId, CancellationToken cancellationToken)
        {
            if (entityId == null)
                throw new ArgumentNullException("entityId");
            if (checkId == null)
                throw new ArgumentNullException("checkId");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/entities/{entityId}/checks/{checkId}");
            var parameters = new Dictionary<string, string> { { "entityId", entityId.Value }, { "checkId", checkId.Value } };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.DELETE, template, parameters);

            Func<Task<HttpWebRequest>, Task<string>> requestResource =
                GetResponseAsyncFunc(cancellationToken);

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource);
        }

        /// <inheritdoc/>
        public Task<ReadOnlyCollectionPage<CheckType, CheckTypeId>> ListCheckTypesAsync(CheckTypeId marker, int? limit, CancellationToken cancellationToken)
        {
            if (limit < 0)
                throw new ArgumentOutOfRangeException("limit");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/check_types?marker={marker}&limit={limit}");
            var parameters = new Dictionary<string, string>();
            if (marker != null)
                parameters.Add("marker", marker.Value);
            if (limit != null)
                parameters.Add("limit", limit.ToString());

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<JObject>> requestResource =
                GetResponseAsyncFunc<JObject>(cancellationToken);

            Func<Task<JObject>, ReadOnlyCollectionPage<CheckType, CheckTypeId>> resultSelector =
                task =>
                {
                    JObject result = task.Result;
                    if (result == null)
                        return null;

                    JToken valuesToken = result["values"];
                    if (valuesToken == null)
                        return null;

                    JToken metadataToken = result["metadata"];

                    CheckType[] values = valuesToken.ToObject<CheckType[]>();
                    Func<CheckTypeId, CancellationToken, Task<ReadOnlyCollectionPage<CheckType, CheckTypeId>>> getNextPageAsync =
                        (nextMarker, nextCancellationToken) => ListCheckTypesAsync(nextMarker, limit, nextCancellationToken);
                    IDictionary<string, object> metadata = metadataToken != null ? metadataToken.ToObject<IDictionary<string, object>>() : null;
                    return new ReadOnlyCollectionPage<CheckType, CheckTypeId>(values, getNextPageAsync, metadata);
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task<CheckType> GetCheckTypeAsync(CheckTypeId checkTypeId, CancellationToken cancellationToken)
        {
            if (checkTypeId == null)
                throw new ArgumentNullException("checkTypeId");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/check_types/{checkTypeId}");
            var parameters = new Dictionary<string, string> { { "checkTypeId", checkTypeId.Value } };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<CheckType>> requestResource =
                GetResponseAsyncFunc<CheckType>(cancellationToken);

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource);
        }

        /// <inheritdoc/>
        public Task<ReadOnlyCollectionPage<Metric, MetricName>> ListMetricsAsync(EntityId entityId, CheckId checkId, MetricName marker, int? limit, CancellationToken cancellationToken)
        {
            if (entityId == null)
                throw new ArgumentNullException("entityId");
            if (checkId == null)
                throw new ArgumentNullException("checkId");
            if (limit < 0)
                throw new ArgumentOutOfRangeException("limit");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/entities/{entityId}/checks/{checkId}/metrics?marker={marker}&limit={limit}");
            var parameters = new Dictionary<string, string> { { "entityId", entityId.Value }, { "checkId", checkId.Value } };
            if (marker != null)
                parameters.Add("marker", marker.Value);
            if (limit != null)
                parameters.Add("limit", limit.ToString());

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<JObject>> requestResource =
                GetResponseAsyncFunc<JObject>(cancellationToken);

            Func<Task<JObject>, ReadOnlyCollectionPage<Metric, MetricName>> resultSelector =
                task =>
                {
                    JObject result = task.Result;
                    if (result == null)
                        return null;

                    JToken valuesToken = result["values"];
                    if (valuesToken == null)
                        return null;

                    JToken metadataToken = result["metadata"];

                    Metric[] values = valuesToken.ToObject<Metric[]>();
                    Func<MetricName, CancellationToken, Task<ReadOnlyCollectionPage<Metric, MetricName>>> getNextPageAsync =
                        (nextMarker, nextCancellationToken) => ListMetricsAsync(entityId, checkId, nextMarker, limit, nextCancellationToken);
                    IDictionary<string, object> metadata = metadataToken != null ? metadataToken.ToObject<IDictionary<string, object>>() : null;
                    return new ReadOnlyCollectionPage<Metric, MetricName>(values, getNextPageAsync, metadata);
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task<ReadOnlyCollection<DataPoint>> GetDataPointsAsync(EntityId entityId, CheckId checkId, MetricName metricName, int? points, DataPointGranularity resolution, IEnumerable<DataPointStatistic> select, DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken)
        {
            if (entityId == null)
                throw new ArgumentNullException("entityId");
            if (checkId == null)
                throw new ArgumentNullException("checkId");
            if (metricName == null)
                throw new ArgumentNullException("metricName");
            if (points <= 0)
                throw new ArgumentOutOfRangeException("points");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/entities/{entityId}/checks/{checkId}/metrics/{metricName}/plot?points={points}&resolution={resolution}&SELECT={select}&from={from}&to={to}");
            var parameters = new Dictionary<string, string>
            {
                { "entityId", entityId.Value },
                { "checkId", checkId.Value },
                { "metricName", metricName.Value },
                { "from",from.ToTimestamp().ToString() },
                { "to", to.ToTimestamp().ToString() }
            };
            if (points != null)
                parameters.Add("points", points.ToString());
            if (resolution != null)
                parameters.Add("resolution", resolution.Name);
            if (select != null)
                parameters.Add("select", "select");

            Func<Uri, Uri> transform =
                uri =>
                {
                    UriBuilder builder = new UriBuilder(uri);
                    if (builder.Query != null && select != null)
                        builder.Query = builder.Query.Substring(1).Replace("SELECT=select", string.Join("&", select.Select(i => "select=" + i.Name).ToArray()));

                    return builder.Uri;
                };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters, transform);

            Func<Task<HttpWebRequest>, Task<JObject>> requestResource =
                GetResponseAsyncFunc<JObject>(cancellationToken);

            Func<Task<JObject>, ReadOnlyCollection<DataPoint>> resultSelector =
                task =>
                {
                    JObject result = task.Result;
                    if (result == null)
                        return null;

                    JToken valuesToken = result["values"];
                    if (valuesToken == null)
                        return null;

                    return valuesToken.ToObject<ReadOnlyCollection<DataPoint>>();
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task<AlarmId> CreateAlarmAsync(EntityId entityId, NewAlarmConfiguration configuration, CancellationToken cancellationToken)
        {
            if (entityId == null)
                throw new ArgumentNullException("entityId");
            if (configuration == null)
                throw new ArgumentNullException("configuration");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/entities/{entityId}/alarms");
            var parameters = new Dictionary<string, string> { { "entityId", entityId.Value } };

            Func<Task<Tuple<IdentityToken, Uri>>, Task<HttpWebRequest>> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.POST, template, parameters, configuration);

            Func<Task<Tuple<HttpWebResponse, string>>, Task<AlarmId>> parseResult =
                task =>
                {
                    UriTemplate.UriTemplate entityTemplate = new UriTemplate.UriTemplate("/entities/{entityId}/alarms/{alarmId}");
                    string location = task.Result.Item1.Headers[HttpResponseHeader.Location];
                    UriTemplate.UriTemplateMatch match = entityTemplate.Match(_baseUri, new Uri(location));
                    return InternalTaskExtensions.CompletedTask(new AlarmId(match.BoundVariables["alarmId"]));
                };

            Func<Task<HttpWebRequest>, Task<AlarmId>> requestResource =
                GetResponseAsyncFunc(cancellationToken, parseResult);

            return AuthenticateServiceAsync(cancellationToken)
                .Then(prepareRequest)
                .Then(requestResource);
        }

        /// <inheritdoc/>
        public Task<ReadOnlyCollection<AlarmData>> TestAlarmAsync(EntityId entityId, TestAlarmConfiguration configuration, CancellationToken cancellationToken)
        {
            if (entityId == null)
                throw new ArgumentNullException("entityId");
            if (configuration == null)
                throw new ArgumentNullException("configuration");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/entities/{entityId}/test-alarm");
            var parameters = new Dictionary<string, string> { { "entityId", entityId.Value } };

            Func<Task<Tuple<IdentityToken, Uri>>, Task<HttpWebRequest>> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.POST, template, parameters, configuration);

            Func<Task<HttpWebRequest>, Task<ReadOnlyCollection<AlarmData>>> requestResource =
                GetResponseAsyncFunc<ReadOnlyCollection<AlarmData>>(cancellationToken);

            return AuthenticateServiceAsync(cancellationToken)
                .Then(prepareRequest)
                .Then(requestResource);
        }

        /// <inheritdoc/>
        public Task<ReadOnlyCollectionPage<Alarm, AlarmId>> ListAlarmsAsync(EntityId entityId, AlarmId marker, int? limit, CancellationToken cancellationToken)
        {
            if (entityId == null)
                throw new ArgumentNullException("entityId");
            if (limit < 0)
                throw new ArgumentOutOfRangeException("limit");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/entities/{entityId}/alarms?marker={marker}&limit={limit}");
            var parameters = new Dictionary<string, string> { { "entityId", entityId.Value } };
            if (marker != null)
                parameters.Add("marker", marker.Value);
            if (limit != null)
                parameters.Add("limit", limit.ToString());

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<JObject>> requestResource =
                GetResponseAsyncFunc<JObject>(cancellationToken);

            Func<Task<JObject>, ReadOnlyCollectionPage<Alarm, AlarmId>> resultSelector =
                task =>
                {
                    JObject result = task.Result;
                    if (result == null)
                        return null;

                    JToken valuesToken = result["values"];
                    if (valuesToken == null)
                        return null;

                    JToken metadataToken = result["metadata"];

                    Alarm[] values = valuesToken.ToObject<Alarm[]>();
                    Func<AlarmId, CancellationToken, Task<ReadOnlyCollectionPage<Alarm, AlarmId>>> getNextPageAsync =
                        (nextMarker, nextCancellationToken) => ListAlarmsAsync(entityId, nextMarker, limit, nextCancellationToken);
                    IDictionary<string, object> metadata = metadataToken != null ? metadataToken.ToObject<IDictionary<string, object>>() : null;
                    return new ReadOnlyCollectionPage<Alarm, AlarmId>(values, getNextPageAsync, metadata);
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task<Alarm> GetAlarmAsync(EntityId entityId, AlarmId alarmId, CancellationToken cancellationToken)
        {
            if (entityId == null)
                throw new ArgumentNullException("entityId");
            if (alarmId == null)
                throw new ArgumentNullException("alarmId");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/entities/{entityId}/alarms/{alarmId}");
            var parameters = new Dictionary<string, string> { { "entityId", entityId.Value }, { "alarmId", alarmId.Value } };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<Alarm>> requestResource =
                GetResponseAsyncFunc<Alarm>(cancellationToken);

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource);
        }

        /// <inheritdoc/>
        public Task UpdateAlarmAsync(EntityId entityId, AlarmId alarmId, UpdateAlarmConfiguration configuration, CancellationToken cancellationToken)
        {
            if (entityId == null)
                throw new ArgumentNullException("entityId");
            if (alarmId == null)
                throw new ArgumentNullException("alarmId");
            if (configuration == null)
                throw new ArgumentNullException("configuration");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/entities/{entityId}/alarms/{alarmId}");
            var parameters = new Dictionary<string, string> { { "entityId", entityId.Value }, { "alarmId", alarmId.Value } };

            Func<Task<Tuple<IdentityToken, Uri>>, Task<HttpWebRequest>> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.PUT, template, parameters, configuration);

            Func<Task<HttpWebRequest>, Task<string>> requestResource =
                GetResponseAsyncFunc(cancellationToken);

            return AuthenticateServiceAsync(cancellationToken)
                .Then(prepareRequest)
                .Then(requestResource);
        }

        /// <inheritdoc/>
        public Task RemoveAlarmAsync(EntityId entityId, AlarmId alarmId, CancellationToken cancellationToken)
        {
            if (entityId == null)
                throw new ArgumentNullException("entityId");
            if (alarmId == null)
                throw new ArgumentNullException("alarmId");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/entities/{entityId}/alarms/{alarmId}");
            var parameters = new Dictionary<string, string> { { "entityId", entityId.Value }, { "alarmId", alarmId.Value } };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.DELETE, template, parameters);

            Func<Task<HttpWebRequest>, Task<string>> requestResource =
                GetResponseAsyncFunc(cancellationToken);

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource);
        }

        /// <inheritdoc/>
        public Task<NotificationPlanId> CreateNotificationPlanAsync(NewNotificationPlanConfiguration configuration, CancellationToken cancellationToken)
        {
            if (configuration == null)
                throw new ArgumentNullException("configuration");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/notification_plans");
            var parameters = new Dictionary<string, string>();

            Func<Task<Tuple<IdentityToken, Uri>>, Task<HttpWebRequest>> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.POST, template, parameters, configuration);

            Func<Task<Tuple<HttpWebResponse, string>>, Task<NotificationPlanId>> parseResult =
                task =>
                {
                    UriTemplate.UriTemplate entityTemplate = new UriTemplate.UriTemplate("/notification_plans/{notificationPlanId}");
                    string location = task.Result.Item1.Headers[HttpResponseHeader.Location];
                    UriTemplate.UriTemplateMatch match = entityTemplate.Match(_baseUri, new Uri(location));
                    return InternalTaskExtensions.CompletedTask(new NotificationPlanId(match.BoundVariables["notificationPlanId"]));
                };

            Func<Task<HttpWebRequest>, Task<NotificationPlanId>> requestResource =
                GetResponseAsyncFunc(cancellationToken, parseResult);

            return AuthenticateServiceAsync(cancellationToken)
                .Then(prepareRequest)
                .Then(requestResource);
        }

        /// <inheritdoc/>
        public Task<ReadOnlyCollectionPage<NotificationPlan, NotificationPlanId>> ListNotificationPlansAsync(NotificationPlanId marker, int? limit, CancellationToken cancellationToken)
        {
            if (limit < 0)
                throw new ArgumentOutOfRangeException("limit");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/notification_plans?marker={marker}&limit={limit}");
            var parameters = new Dictionary<string, string>();
            if (marker != null)
                parameters.Add("marker", marker.Value);
            if (limit != null)
                parameters.Add("limit", limit.ToString());

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<JObject>> requestResource =
                GetResponseAsyncFunc<JObject>(cancellationToken);

            Func<Task<JObject>, ReadOnlyCollectionPage<NotificationPlan, NotificationPlanId>> resultSelector =
                task =>
                {
                    JObject result = task.Result;
                    if (result == null)
                        return null;

                    JToken valuesToken = result["values"];
                    if (valuesToken == null)
                        return null;

                    JToken metadataToken = result["metadata"];

                    NotificationPlan[] values = valuesToken.ToObject<NotificationPlan[]>();
                    Func<NotificationPlanId, CancellationToken, Task<ReadOnlyCollectionPage<NotificationPlan, NotificationPlanId>>> getNextPageAsync =
                        (nextMarker, nextCancellationToken) => ListNotificationPlansAsync(nextMarker, limit, nextCancellationToken);
                    IDictionary<string, object> metadata = metadataToken != null ? metadataToken.ToObject<IDictionary<string, object>>() : null;
                    return new ReadOnlyCollectionPage<NotificationPlan, NotificationPlanId>(values, getNextPageAsync, metadata);
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task<NotificationPlan> GetNotificationPlanAsync(NotificationPlanId notificationPlanId, CancellationToken cancellationToken)
        {
            if (notificationPlanId == null)
                throw new ArgumentNullException("notificationPlanId");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/notification_plans/{notificationPlanId}");
            var parameters = new Dictionary<string, string> { { "notificationPlanId", notificationPlanId.Value } };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<NotificationPlan>> requestResource =
                GetResponseAsyncFunc<NotificationPlan>(cancellationToken);

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource);
        }

        /// <inheritdoc/>
        public Task UpdateNotificationPlanAsync(NotificationPlanId notificationPlanId, UpdateNotificationPlanConfiguration configuration, CancellationToken cancellationToken)
        {
            if (notificationPlanId == null)
                throw new ArgumentNullException("notificationPlanId");
            if (configuration == null)
                throw new ArgumentNullException("configuration");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/notification_plans/{notificationPlanId}");
            var parameters = new Dictionary<string, string> { { "notificationPlanId", notificationPlanId.Value } };

            Func<Task<Tuple<IdentityToken, Uri>>, Task<HttpWebRequest>> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.PUT, template, parameters, configuration);

            Func<Task<HttpWebRequest>, Task<string>> requestResource =
                GetResponseAsyncFunc(cancellationToken);

            return AuthenticateServiceAsync(cancellationToken)
                .Then(prepareRequest)
                .Then(requestResource);
        }

        /// <inheritdoc/>
        public Task RemoveNotificationPlanAsync(NotificationPlanId notificationPlanId, CancellationToken cancellationToken)
        {
            if (notificationPlanId == null)
                throw new ArgumentNullException("notificationPlanId");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/notification_plans/{notificationPlanId}");
            var parameters = new Dictionary<string, string> { { "notificationPlanId", notificationPlanId.Value } };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.DELETE, template, parameters);

            Func<Task<HttpWebRequest>, Task<string>> requestResource =
                GetResponseAsyncFunc(cancellationToken);

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource);
        }

        /// <inheritdoc/>
        public Task<ReadOnlyCollectionPage<MonitoringZone, MonitoringZoneId>> ListMonitoringZonesAsync(MonitoringZoneId marker, int? limit, CancellationToken cancellationToken)
        {
            if (limit < 0)
                throw new ArgumentOutOfRangeException("limit");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/monitoring_zones?marker={marker}&limit={limit}");
            var parameters = new Dictionary<string, string>();
            if (marker != null)
                parameters.Add("marker", marker.Value);
            if (limit != null)
                parameters.Add("limit", limit.ToString());

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<JObject>> requestResource =
                GetResponseAsyncFunc<JObject>(cancellationToken);

            Func<Task<JObject>, ReadOnlyCollectionPage<MonitoringZone, MonitoringZoneId>> resultSelector =
                task =>
                {
                    JObject result = task.Result;
                    if (result == null)
                        return null;

                    JToken valuesToken = result["values"];
                    if (valuesToken == null)
                        return null;

                    JToken metadataToken = result["metadata"];

                    MonitoringZone[] values = valuesToken.ToObject<MonitoringZone[]>();
                    Func<MonitoringZoneId, CancellationToken, Task<ReadOnlyCollectionPage<MonitoringZone, MonitoringZoneId>>> getNextPageAsync =
                        (nextMarker, nextCancellationToken) => ListMonitoringZonesAsync(nextMarker, limit, nextCancellationToken);
                    IDictionary<string, object> metadata = metadataToken != null ? metadataToken.ToObject<IDictionary<string, object>>() : null;
                    return new ReadOnlyCollectionPage<MonitoringZone, MonitoringZoneId>(values, getNextPageAsync, metadata);
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task<MonitoringZone> GetMonitoringZoneAsync(MonitoringZoneId monitoringZoneId, CancellationToken cancellationToken)
        {
            if (monitoringZoneId == null)
                throw new ArgumentNullException("monitoringZoneId");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/monitoring_zones/{monitoringZoneId}");
            var parameters = new Dictionary<string, string> { { "monitoringZoneId", monitoringZoneId.Value } };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<MonitoringZone>> requestResource =
                GetResponseAsyncFunc<MonitoringZone>(cancellationToken);

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource);
        }

        /// <inheritdoc/>
        public Task<TraceRoute> PerformTraceRouteFromMonitoringZoneAsync(MonitoringZoneId monitoringZoneId, TraceRouteConfiguration configuration, CancellationToken cancellationToken)
        {
            if (monitoringZoneId == null)
                throw new ArgumentNullException("monitoringZoneId");
            if (configuration == null)
                throw new ArgumentNullException("configuration");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/monitoring_zones/{monitoringZoneId}/traceroute");
            var parameters = new Dictionary<string, string> { { "monitoringZoneId", monitoringZoneId.Value } };

            Func<Task<Tuple<IdentityToken, Uri>>, Task<HttpWebRequest>> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.POST, template, parameters, configuration);

            Func<Task<HttpWebRequest>, Task<TraceRoute>> requestResource =
                GetResponseAsyncFunc<TraceRoute>(cancellationToken);

            return AuthenticateServiceAsync(cancellationToken)
                .Then(prepareRequest)
                .Then(requestResource);
        }

        /// <inheritdoc/>
        public Task<ReadOnlyCollection<CheckId>> DiscoverAlarmNotificationHistoryAsync(EntityId entityId, AlarmId alarmId, CancellationToken cancellationToken)
        {
            if (entityId == null)
                throw new ArgumentNullException("entityId");
            if (alarmId == null)
                throw new ArgumentNullException("alarmId");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/entities/{entityId}/alarms/{alarmId}/notification_history");
            var parameters = new Dictionary<string, string> { { "entityId", entityId.Value }, { "alarmId", alarmId.Value } };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<JObject>> requestResource =
                GetResponseAsyncFunc<JObject>(cancellationToken);

            Func<Task<JObject>, ReadOnlyCollection<CheckId>> resultSelector =
                task =>
                {
                    JObject result = task.Result;
                    if (result == null)
                        return null;

                    JToken checkIdsToken = result["check_ids"];
                    if (checkIdsToken == null)
                        return null;

                    return checkIdsToken.ToObject<ReadOnlyCollection<CheckId>>();
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task<ReadOnlyCollectionPage<AlarmNotificationHistoryItem, AlarmNotificationHistoryItemId>> ListAlarmNotificationHistoryAsync(EntityId entityId, AlarmId alarmId, CheckId checkId, AlarmNotificationHistoryItemId marker, int? limit, DateTimeOffset? from, DateTimeOffset? to, CancellationToken cancellationToken)
        {
            if (entityId == null)
                throw new ArgumentNullException("entityId");
            if (alarmId == null)
                throw new ArgumentNullException("alarmId");
            if (checkId == null)
                throw new ArgumentNullException("checkId");
            if (limit < 0)
                throw new ArgumentOutOfRangeException("limit");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/entities/{entityId}/alarms/{alarmId}/notification_history/{checkId}?marker={marker}&limit={limit}&from={from}&to={to}");
            var parameters = new Dictionary<string, string> { { "entityId", entityId.Value }, { "alarmId", alarmId.Value }, { "checkId", checkId.Value } };
            if (marker != null)
                parameters.Add("marker", marker.Value);
            if (limit != null)
                parameters.Add("limit", limit.ToString());
            if (from != null)
                parameters.Add("from", from.Value.ToTimestamp().ToString());
            if (to != null)
                parameters.Add("to", to.Value.ToTimestamp().ToString());

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<JObject>> requestResource =
                GetResponseAsyncFunc<JObject>(cancellationToken);

            Func<Task<JObject>, ReadOnlyCollectionPage<AlarmNotificationHistoryItem, AlarmNotificationHistoryItemId>> resultSelector =
                task =>
                {
                    JObject result = task.Result;
                    if (result == null)
                        return null;

                    JToken valuesToken = result["values"];
                    if (valuesToken == null)
                        return null;

                    JToken metadataToken = result["metadata"];

                    AlarmNotificationHistoryItem[] values = valuesToken.ToObject<AlarmNotificationHistoryItem[]>();
                    Func<AlarmNotificationHistoryItemId, CancellationToken, Task<ReadOnlyCollectionPage<AlarmNotificationHistoryItem, AlarmNotificationHistoryItemId>>> getNextPageAsync =
                        (nextMarker, nextCancellationToken) => ListAlarmNotificationHistoryAsync(entityId, alarmId, checkId, nextMarker, limit, from, to, nextCancellationToken);
                    IDictionary<string, object> metadata = metadataToken != null ? metadataToken.ToObject<IDictionary<string, object>>() : null;
                    return new ReadOnlyCollectionPage<AlarmNotificationHistoryItem, AlarmNotificationHistoryItemId>(values, getNextPageAsync, metadata);
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task<AlarmNotificationHistoryItem> GetAlarmNotificationHistoryAsync(EntityId entityId, AlarmId alarmId, CheckId checkId, AlarmNotificationHistoryItemId alarmNotificationHistoryItemId, CancellationToken cancellationToken)
        {
            if (entityId == null)
                throw new ArgumentNullException("entityId");
            if (alarmId == null)
                throw new ArgumentNullException("alarmId");
            if (checkId == null)
                throw new ArgumentNullException("checkId");
            if (alarmNotificationHistoryItemId == null)
                throw new ArgumentNullException("alarmNotificationHistoryItemId");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/entities/{entityId}/alarms/{alarmId}/notification_history/{checkId}/{uuid}");
            var parameters = new Dictionary<string, string> { { "entityId", entityId.Value }, { "alarmId", alarmId.Value }, { "checkId", checkId.Value }, { "uuid", alarmNotificationHistoryItemId.Value } };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<AlarmNotificationHistoryItem>> requestResource =
                GetResponseAsyncFunc<AlarmNotificationHistoryItem>(cancellationToken);

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource);
        }

        /// <inheritdoc/>
        public Task<NotificationId> CreateNotificationAsync(NewNotificationConfiguration configuration, CancellationToken cancellationToken)
        {
            if (configuration == null)
                throw new ArgumentNullException("configuration");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/notifications");
            var parameters = new Dictionary<string, string>();

            Func<Task<Tuple<IdentityToken, Uri>>, Task<HttpWebRequest>> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.POST, template, parameters, configuration);

            Func<Task<Tuple<HttpWebResponse, string>>, Task<NotificationId>> parseResult =
                task =>
                {
                    UriTemplate.UriTemplate entityTemplate = new UriTemplate.UriTemplate("/notifications/{notificationId}");
                    string location = task.Result.Item1.Headers[HttpResponseHeader.Location];
                    UriTemplate.UriTemplateMatch match = entityTemplate.Match(_baseUri, new Uri(location));
                    return InternalTaskExtensions.CompletedTask(new NotificationId(match.BoundVariables["notificationId"]));
                };

            Func<Task<HttpWebRequest>, Task<NotificationId>> requestResource =
                GetResponseAsyncFunc(cancellationToken, parseResult);

            return AuthenticateServiceAsync(cancellationToken)
                .Then(prepareRequest)
                .Then(requestResource);
        }

        /// <inheritdoc/>
        public Task<NotificationData> TestNotificationAsync(NewNotificationConfiguration configuration, CancellationToken cancellationToken)
        {
            if (configuration == null)
                throw new ArgumentNullException("configuration");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/test-notification");
            var parameters = new Dictionary<string, string>();

            Func<Task<Tuple<IdentityToken, Uri>>, Task<HttpWebRequest>> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.POST, template, parameters, configuration);

            Func<Task<HttpWebRequest>, Task<NotificationData>> requestResource =
                GetResponseAsyncFunc<NotificationData>(cancellationToken);

            return AuthenticateServiceAsync(cancellationToken)
                .Then(prepareRequest)
                .Then(requestResource);
        }

        /// <inheritdoc/>
        public Task<NotificationData> TestExistingNotificationAsync(NotificationId notificationId, CancellationToken cancellationToken)
        {
            if (notificationId == null)
                throw new ArgumentNullException("notificationId");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/notifications/{notificationId}/test");
            var parameters = new Dictionary<string, string> { { "notificationId", notificationId.Value } };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.POST, template, parameters);

            Func<Task<HttpWebRequest>, Task<NotificationData>> requestResource =
                GetResponseAsyncFunc<NotificationData>(cancellationToken);

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource);
        }

        /// <inheritdoc/>
        public Task<ReadOnlyCollectionPage<Notification, NotificationId>> ListNotificationsAsync(NotificationId marker, int? limit, CancellationToken cancellationToken)
        {
            if (limit < 0)
                throw new ArgumentOutOfRangeException("limit");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/notifications?marker={marker}&limit={limit}");
            var parameters = new Dictionary<string, string>();
            if (marker != null)
                parameters.Add("marker", marker.Value);
            if (limit != null)
                parameters.Add("limit", limit.ToString());

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<JObject>> requestResource =
                GetResponseAsyncFunc<JObject>(cancellationToken);

            Func<Task<JObject>, ReadOnlyCollectionPage<Notification, NotificationId>> resultSelector =
                task =>
                {
                    JObject result = task.Result;
                    if (result == null)
                        return null;

                    JToken valuesToken = result["values"];
                    if (valuesToken == null)
                        return null;

                    JToken metadataToken = result["metadata"];

                    Notification[] values = valuesToken.ToObject<Notification[]>();
                    Func<NotificationId, CancellationToken, Task<ReadOnlyCollectionPage<Notification, NotificationId>>> getNextPageAsync =
                        (nextMarker, nextCancellationToken) => ListNotificationsAsync(nextMarker, limit, nextCancellationToken);
                    IDictionary<string, object> metadata = metadataToken != null ? metadataToken.ToObject<IDictionary<string, object>>() : null;
                    return new ReadOnlyCollectionPage<Notification, NotificationId>(values, getNextPageAsync, metadata);
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task<Notification> GetNotificationAsync(NotificationId notificationId, CancellationToken cancellationToken)
        {
            if (notificationId == null)
                throw new ArgumentNullException("notificationId");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/notifications/{notificationId}");
            var parameters = new Dictionary<string, string> { { "notificationId", notificationId.Value } };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<Notification>> requestResource =
                GetResponseAsyncFunc<Notification>(cancellationToken);

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource);
        }

        /// <inheritdoc/>
        public Task UpdateNotificationAsync(NotificationId notificationId, UpdateNotificationConfiguration configuration, CancellationToken cancellationToken)
        {
            if (notificationId == null)
                throw new ArgumentNullException("notificationId");
            if (configuration == null)
                throw new ArgumentNullException("configuration");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/notifications/{notificationId}");
            var parameters = new Dictionary<string, string> { { "notificationId", notificationId.Value } };

            Func<Task<Tuple<IdentityToken, Uri>>, Task<HttpWebRequest>> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.PUT, template, parameters, configuration);

            Func<Task<HttpWebRequest>, Task<string>> requestResource =
                GetResponseAsyncFunc(cancellationToken);

            return AuthenticateServiceAsync(cancellationToken)
                .Then(prepareRequest)
                .Then(requestResource);
        }

        /// <inheritdoc/>
        public Task RemoveNotificationAsync(NotificationId notificationId, CancellationToken cancellationToken)
        {
            if (notificationId == null)
                throw new ArgumentNullException("notificationId");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/notifications/{notificationId}");
            var parameters = new Dictionary<string, string> { { "notificationId", notificationId.Value } };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.DELETE, template, parameters);

            Func<Task<HttpWebRequest>, Task<string>> requestResource =
                GetResponseAsyncFunc(cancellationToken);

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource);
        }

        /// <inheritdoc/>
        public Task<ReadOnlyCollectionPage<NotificationType, NotificationTypeId>> ListNotificationTypesAsync(NotificationTypeId marker, int? limit, CancellationToken cancellationToken)
        {
            if (limit < 0)
                throw new ArgumentOutOfRangeException("limit");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/notification_types?marker={marker}&limit={limit}");
            var parameters = new Dictionary<string, string>();
            if (marker != null)
                parameters.Add("marker", marker.Value);
            if (limit != null)
                parameters.Add("limit", limit.ToString());

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<JObject>> requestResource =
                GetResponseAsyncFunc<JObject>(cancellationToken);

            Func<Task<JObject>, ReadOnlyCollectionPage<NotificationType, NotificationTypeId>> resultSelector =
                task =>
                {
                    JObject result = task.Result;
                    if (result == null)
                        return null;

                    JToken valuesToken = result["values"];
                    if (valuesToken == null)
                        return null;

                    JToken metadataToken = result["metadata"];

                    NotificationType[] values = valuesToken.ToObject<NotificationType[]>();
                    Func<NotificationTypeId, CancellationToken, Task<ReadOnlyCollectionPage<NotificationType, NotificationTypeId>>> getNextPageAsync =
                        (nextMarker, nextCancellationToken) => ListNotificationTypesAsync(nextMarker, limit, nextCancellationToken);
                    IDictionary<string, object> metadata = metadataToken != null ? metadataToken.ToObject<IDictionary<string, object>>() : null;
                    return new ReadOnlyCollectionPage<NotificationType, NotificationTypeId>(values, getNextPageAsync, metadata);
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task<NotificationType> GetNotificationTypeAsync(NotificationTypeId notificationTypeId, CancellationToken cancellationToken)
        {
            if (notificationTypeId == null)
                throw new ArgumentNullException("notificationTypeId");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/notification_types/{notificationTypeId}");
            var parameters = new Dictionary<string, string> { { "notificationTypeId", notificationTypeId.Value } };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<NotificationType>> requestResource =
                GetResponseAsyncFunc<NotificationType>(cancellationToken);

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource);
        }

        /// <inheritdoc/>
        public Task<ReadOnlyCollectionPage<AlarmChangelog, AlarmChangelogId>> ListAlarmChangelogsAsync(AlarmChangelogId marker, int? limit, DateTimeOffset? from, DateTimeOffset? to, CancellationToken cancellationToken)
        {
            return ListAlarmChangelogsAsync(null, marker, limit, from, to, cancellationToken);
        }

        /// <inheritdoc/>
        public Task<ReadOnlyCollectionPage<AlarmChangelog, AlarmChangelogId>> ListAlarmChangelogsAsync(EntityId entityId, AlarmChangelogId marker, int? limit, DateTimeOffset? from, DateTimeOffset? to, CancellationToken cancellationToken)
        {
            if (limit < 0)
                throw new ArgumentOutOfRangeException("limit");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/changelogs/alarms?entityId={entityId}&marker={marker}&limit={limit}&from={from}&to={to}");
            var parameters = new Dictionary<string, string>();
            if (entityId != null)
                parameters.Add("entityId", entityId.Value);
            if (marker != null)
                parameters.Add("marker", marker.Value);
            if (limit != null)
                parameters.Add("limit", limit.ToString());
            if (from != null)
                parameters.Add("from", from.Value.ToTimestamp().ToString());
            if (to != null)
                parameters.Add("to", to.Value.ToTimestamp().ToString());

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<JObject>> requestResource =
                GetResponseAsyncFunc<JObject>(cancellationToken);

            Func<Task<JObject>, ReadOnlyCollectionPage<AlarmChangelog, AlarmChangelogId>> resultSelector =
                task =>
                {
                    JObject result = task.Result;
                    if (result == null)
                        return null;

                    JToken valuesToken = result["values"];
                    if (valuesToken == null)
                        return null;

                    JToken metadataToken = result["metadata"];

                    AlarmChangelog[] values = valuesToken.ToObject<AlarmChangelog[]>();
                    Func<AlarmChangelogId, CancellationToken, Task<ReadOnlyCollectionPage<AlarmChangelog, AlarmChangelogId>>> getNextPageAsync =
                        (nextMarker, nextCancellationToken) => ListAlarmChangelogsAsync(entityId, nextMarker, limit, from, to, nextCancellationToken);
                    IDictionary<string, object> metadata = metadataToken != null ? metadataToken.ToObject<IDictionary<string, object>>() : null;
                    return new ReadOnlyCollectionPage<AlarmChangelog, AlarmChangelogId>(values, getNextPageAsync, metadata);
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task<ReadOnlyCollectionPage<EntityOverview, EntityId>> ListEntityOverviewsAsync(EntityId marker, int? limit, CancellationToken cancellationToken)
        {
            IEnumerable<EntityId> entityIdFilter = null;
            return ListEntityOverviewsAsync(marker, limit, entityIdFilter, cancellationToken);
        }

        /// <inheritdoc/>
        public Task<ReadOnlyCollectionPage<EntityOverview, EntityId>> ListEntityOverviewsAsync(EntityId marker, int? limit, IEnumerable<EntityId> entityIdFilter, CancellationToken cancellationToken)
        {
            if (limit < 0)
                throw new ArgumentOutOfRangeException("limit");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/views/overview?ENTITYID={entityIdFilter}&marker={marker}&limit={limit}");
            var parameters = new Dictionary<string, string>();
            if (marker != null)
                parameters.Add("marker", marker.Value);
            if (limit != null)
                parameters.Add("limit", limit.ToString());
            if (entityIdFilter != null)
                parameters.Add("entityIdFilter", "entityIdFilter");

            Func<Uri, Uri> transform =
                uri =>
                {
                    UriBuilder builder = new UriBuilder(uri);
                    if (builder.Query != null && entityIdFilter != null)
                        builder.Query = builder.Query.Substring(1).Replace("ENTITYID=entityIdFilter", string.Join("&", entityIdFilter.Select(i => "entityId=" + i.Value).ToArray()));

                    return builder.Uri;
                };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters, transform);

            Func<Task<HttpWebRequest>, Task<JObject>> requestResource =
                GetResponseAsyncFunc<JObject>(cancellationToken);

            Func<Task<JObject>, ReadOnlyCollectionPage<EntityOverview, EntityId>> resultSelector =
                task =>
                {
                    JObject result = task.Result;
                    if (result == null)
                        return null;

                    JToken valuesToken = result["values"];
                    if (valuesToken == null)
                        return null;

                    JToken metadataToken = result["metadata"];

                    EntityOverview[] values = valuesToken.ToObject<EntityOverview[]>();
                    Func<EntityId, CancellationToken, Task<ReadOnlyCollectionPage<EntityOverview, EntityId>>> getNextPageAsync =
                        (nextMarker, nextCancellationToken) => ListEntityOverviewsAsync(nextMarker, limit, entityIdFilter, nextCancellationToken);
                    IDictionary<string, object> metadata = metadataToken != null ? metadataToken.ToObject<IDictionary<string, object>>() : null;
                    return new ReadOnlyCollectionPage<EntityOverview, EntityId>(values, getNextPageAsync, metadata);
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task<ReadOnlyCollectionPage<AlarmExample, AlarmExampleId>> ListAlarmExamplesAsync(AlarmExampleId marker, int? limit, CancellationToken cancellationToken)
        {
            if (limit < 0)
                throw new ArgumentOutOfRangeException("limit");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/alarm_examples?marker={marker}&limit={limit}");
            var parameters = new Dictionary<string, string>();
            if (marker != null)
                parameters.Add("marker", marker.Value);
            if (limit != null)
                parameters.Add("limit", limit.ToString());

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<JObject>> requestResource =
                GetResponseAsyncFunc<JObject>(cancellationToken);

            Func<Task<JObject>, ReadOnlyCollectionPage<AlarmExample, AlarmExampleId>> resultSelector =
                task =>
                {
                    JObject result = task.Result;
                    if (result == null)
                        return null;

                    JToken valuesToken = result["values"];
                    if (valuesToken == null)
                        return null;

                    JToken metadataToken = result["metadata"];

                    AlarmExample[] values = valuesToken.ToObject<AlarmExample[]>();
                    Func<AlarmExampleId, CancellationToken, Task<ReadOnlyCollectionPage<AlarmExample, AlarmExampleId>>> getNextPageAsync =
                        (nextMarker, nextCancellationToken) => ListAlarmExamplesAsync(nextMarker, limit, nextCancellationToken);
                    IDictionary<string, object> metadata = metadataToken != null ? metadataToken.ToObject<IDictionary<string, object>>() : null;
                    return new ReadOnlyCollectionPage<AlarmExample, AlarmExampleId>(values, getNextPageAsync, metadata);
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task<AlarmExample> GetAlarmExampleAsync(AlarmExampleId alarmExampleId, CancellationToken cancellationToken)
        {
            if (alarmExampleId == null)
                throw new ArgumentNullException("alarmExampleId");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/alarm_examples/{alarmExampleId}");
            var parameters = new Dictionary<string, string> { { "alarmExampleId", alarmExampleId.Value } };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<AlarmExample>> requestResource =
                GetResponseAsyncFunc<AlarmExample>(cancellationToken);

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource);
        }

        /// <inheritdoc/>
        public Task<BoundAlarmExample> EvaluateAlarmExampleAsync(AlarmExampleId alarmExampleId, IDictionary<string, object> exampleParameters, CancellationToken cancellationToken)
        {
            if (alarmExampleId == null)
                throw new ArgumentNullException("alarmExampleId");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/alarm_examples/{alarmExampleId}");
            var parameters = new Dictionary<string, string> { { "alarmExampleId", alarmExampleId.Value } };

            JObject body = new JObject(
                new JProperty("values", JObject.FromObject(exampleParameters)));

            Func<Task<Tuple<IdentityToken, Uri>>, Task<HttpWebRequest>> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.POST, template, parameters, body);

            Func<Task<HttpWebRequest>, Task<BoundAlarmExample>> requestResource =
                GetResponseAsyncFunc<BoundAlarmExample>(cancellationToken);

            return AuthenticateServiceAsync(cancellationToken)
                .Then(prepareRequest)
                .Then(requestResource);
        }

        /// <inheritdoc/>
        public Task<ReadOnlyCollectionPage<Agent, AgentId>> ListAgentsAsync(AgentId marker, int? limit, CancellationToken cancellationToken)
        {
            if (limit < 0)
                throw new ArgumentOutOfRangeException("limit");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/agents?marker={marker}&limit={limit}");
            var parameters = new Dictionary<string, string>();
            if (marker != null)
                parameters.Add("marker", marker.Value);
            if (limit != null)
                parameters.Add("limit", limit.ToString());

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<JObject>> requestResource =
                GetResponseAsyncFunc<JObject>(cancellationToken);

            Func<Task<JObject>, ReadOnlyCollectionPage<Agent, AgentId>> resultSelector =
                task =>
                {
                    JObject result = task.Result;
                    if (result == null)
                        return null;

                    JToken valuesToken = result["values"];
                    if (valuesToken == null)
                        return null;

                    JToken metadataToken = result["metadata"];

                    Agent[] values = valuesToken.ToObject<Agent[]>();
                    Func<AgentId, CancellationToken, Task<ReadOnlyCollectionPage<Agent, AgentId>>> getNextPageAsync =
                        (nextMarker, nextCancellationToken) => ListAgentsAsync(nextMarker, limit, nextCancellationToken);
                    IDictionary<string, object> metadata = metadataToken != null ? metadataToken.ToObject<IDictionary<string, object>>() : null;
                    return new ReadOnlyCollectionPage<Agent, AgentId>(values, getNextPageAsync, metadata);
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task<Agent> GetAgentAsync(AgentId agentId, CancellationToken cancellationToken)
        {
            if (agentId == null)
                throw new ArgumentNullException("agentId");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/agents/{agentId}");
            var parameters = new Dictionary<string, string> { { "agentId", agentId.Value } };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<Agent>> requestResource =
                GetResponseAsyncFunc<Agent>(cancellationToken);

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource);
        }

        /// <inheritdoc/>
        public Task<ReadOnlyCollectionPage<AgentConnection, AgentConnectionId>> ListAgentConnectionsAsync(AgentId agentId, AgentConnectionId marker, int? limit, CancellationToken cancellationToken)
        {
            if (agentId == null)
                throw new ArgumentNullException("agentId");
            if (limit < 0)
                throw new ArgumentOutOfRangeException("limit");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/agents/{agentId}/connections?marker={marker}&limit={limit}");
            var parameters = new Dictionary<string, string> { { "agentId", agentId.Value } };
            if (marker != null)
                parameters.Add("marker", marker.Value);
            if (limit != null)
                parameters.Add("limit", limit.ToString());

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<JObject>> requestResource =
                GetResponseAsyncFunc<JObject>(cancellationToken);

            Func<Task<JObject>, ReadOnlyCollectionPage<AgentConnection, AgentConnectionId>> resultSelector =
                task =>
                {
                    JObject result = task.Result;
                    if (result == null)
                        return null;

                    JToken valuesToken = result["values"];
                    if (valuesToken == null)
                        return null;

                    JToken metadataToken = result["metadata"];

                    AgentConnection[] values = valuesToken.ToObject<AgentConnection[]>();
                    Func<AgentConnectionId, CancellationToken, Task<ReadOnlyCollectionPage<AgentConnection, AgentConnectionId>>> getNextPageAsync =
                        (nextMarker, nextCancellationToken) => ListAgentConnectionsAsync(agentId, nextMarker, limit, nextCancellationToken);
                    IDictionary<string, object> metadata = metadataToken != null ? metadataToken.ToObject<IDictionary<string, object>>() : null;
                    return new ReadOnlyCollectionPage<AgentConnection, AgentConnectionId>(values, getNextPageAsync, metadata);
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task<AgentConnection> GetAgentConnectionAsync(AgentId agentId, AgentConnectionId agentConnectionId, CancellationToken cancellationToken)
        {
            if (agentId == null)
                throw new ArgumentNullException("agentId");
            if (agentConnectionId == null)
                throw new ArgumentNullException("agentConnectionId");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/agents/{agentId}/connections/{connId}");
            var parameters = new Dictionary<string, string> { { "agentId", agentId.Value }, { "connId", agentConnectionId.Value } };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<AgentConnection>> requestResource =
                GetResponseAsyncFunc<AgentConnection>(cancellationToken);

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource);
        }

        /// <inheritdoc/>
        public Task<AgentTokenId> CreateAgentTokenAsync(AgentTokenConfiguration configuration, CancellationToken cancellationToken)
        {
            if (configuration == null)
                throw new ArgumentNullException("configuration");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/agent_tokens");
            var parameters = new Dictionary<string, string>();

            Func<Task<Tuple<IdentityToken, Uri>>, Task<HttpWebRequest>> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.POST, template, parameters, configuration);

            Func<Task<Tuple<HttpWebResponse, string>>, Task<AgentTokenId>> parseResult =
                task =>
                {
                    UriTemplate.UriTemplate agentTokenTemplate = new UriTemplate.UriTemplate("/agent_tokens/{tokenId}");
                    string location = task.Result.Item1.Headers[HttpResponseHeader.Location];
                    UriTemplate.UriTemplateMatch match = agentTokenTemplate.Match(_baseUri, new Uri(location));
                    return InternalTaskExtensions.CompletedTask(new AgentTokenId(match.BoundVariables["tokenId"]));
                };

            Func<Task<HttpWebRequest>, Task<AgentTokenId>> requestResource =
                GetResponseAsyncFunc(cancellationToken, parseResult);

            return AuthenticateServiceAsync(cancellationToken)
                .Then(prepareRequest)
                .Then(requestResource);
        }

        /// <inheritdoc/>
        public Task<ReadOnlyCollectionPage<AgentToken, AgentTokenId>> ListAgentTokensAsync(AgentTokenId marker, int? limit, CancellationToken cancellationToken)
        {
            if (limit < 0)
                throw new ArgumentOutOfRangeException("limit");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/agent_tokens?marker={marker}&limit={limit}");
            var parameters = new Dictionary<string, string>();
            if (marker != null)
                parameters.Add("marker", marker.Value);
            if (limit != null)
                parameters.Add("limit", limit.ToString());

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<JObject>> requestResource =
                GetResponseAsyncFunc<JObject>(cancellationToken);

            Func<Task<JObject>, ReadOnlyCollectionPage<AgentToken, AgentTokenId>> resultSelector =
                task =>
                {
                    JObject result = task.Result;
                    if (result == null)
                        return null;

                    JToken valuesToken = result["values"];
                    if (valuesToken == null)
                        return null;

                    JToken metadataToken = result["metadata"];

                    AgentToken[] values = valuesToken.ToObject<AgentToken[]>();
                    Func<AgentTokenId, CancellationToken, Task<ReadOnlyCollectionPage<AgentToken, AgentTokenId>>> getNextPageAsync =
                        (nextMarker, nextCancellationToken) => ListAgentTokensAsync(nextMarker, limit, nextCancellationToken);
                    IDictionary<string, object> metadata = metadataToken != null ? metadataToken.ToObject<IDictionary<string, object>>() : null;
                    return new ReadOnlyCollectionPage<AgentToken, AgentTokenId>(values, getNextPageAsync, metadata);
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task<AgentToken> GetAgentTokenAsync(AgentTokenId agentTokenId, CancellationToken cancellationToken)
        {
            if (agentTokenId == null)
                throw new ArgumentNullException("agentTokenId");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/agent_tokens/{tokenId}");
            var parameters = new Dictionary<string, string> { { "tokenId", agentTokenId.Value } };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<AgentToken>> requestResource =
                GetResponseAsyncFunc<AgentToken>(cancellationToken);

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource);
        }

        /// <inheritdoc/>
        public Task UpdateAgentTokenAsync(AgentTokenId agentTokenId, AgentTokenConfiguration configuration, CancellationToken cancellationToken)
        {
            if (agentTokenId == null)
                throw new ArgumentNullException("agentTokenId");
            if (configuration == null)
                throw new ArgumentNullException("configuration");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/agent_tokens/{tokenId}");
            var parameters = new Dictionary<string, string> { { "tokenId", agentTokenId.Value } };

            Func<Task<Tuple<IdentityToken, Uri>>, Task<HttpWebRequest>> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.PUT, template, parameters, configuration);

            Func<Task<HttpWebRequest>, Task<string>> requestResource =
                GetResponseAsyncFunc(cancellationToken);

            return AuthenticateServiceAsync(cancellationToken)
                .Then(prepareRequest)
                .Then(requestResource);
        }

        /// <inheritdoc/>
        public Task RemoveAgentTokenAsync(AgentTokenId agentTokenId, CancellationToken cancellationToken)
        {
            if (agentTokenId == null)
                throw new ArgumentNullException("agentTokenId");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/agent_tokens/{tokenId}");
            var parameters = new Dictionary<string, string> { { "tokenId", agentTokenId.Value } };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.DELETE, template, parameters);

            Func<Task<HttpWebRequest>, Task<string>> requestResource =
                GetResponseAsyncFunc(cancellationToken);

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource);
        }

        /// <inheritdoc/>
        public Task<HostInformation<JToken>> GetAgentHostInformationAsync(AgentId agentId, HostInformationType hostInformation, CancellationToken cancellationToken)
        {
            if (agentId == null)
                throw new ArgumentNullException("agentId");
            if (hostInformation == null)
                throw new ArgumentNullException("hostInformation");

            return GetAgentHostInformationAsync<JToken>(agentId, hostInformation, cancellationToken);
        }

        /// <inheritdoc/>
        public Task<HostInformation<ReadOnlyCollection<CpuInformation>>> GetCpuInformationAsync(AgentId agentId, CancellationToken cancellationToken)
        {
            if (agentId == null)
                throw new ArgumentNullException("agentId");

            return GetAgentHostInformationAsync<ReadOnlyCollection<CpuInformation>>(agentId, HostInformationType.Cpus, cancellationToken);
        }

        /// <inheritdoc/>
        public Task<HostInformation<ReadOnlyCollection<DiskInformation>>> GetDiskInformationAsync(AgentId agentId, CancellationToken cancellationToken)
        {
            if (agentId == null)
                throw new ArgumentNullException("agentId");

            return GetAgentHostInformationAsync<ReadOnlyCollection<DiskInformation>>(agentId, HostInformationType.Disks, cancellationToken);
        }

        /// <inheritdoc/>
        public Task<HostInformation<ReadOnlyCollection<FilesystemInformation>>> GetFilesystemInformationAsync(AgentId agentId, CancellationToken cancellationToken)
        {
            if (agentId == null)
                throw new ArgumentNullException("agentId");

            return GetAgentHostInformationAsync<ReadOnlyCollection<FilesystemInformation>>(agentId, HostInformationType.Filesystems, cancellationToken);
        }

        /// <inheritdoc/>
        public Task<HostInformation<MemoryInformation>> GetMemoryInformationAsync(AgentId agentId, CancellationToken cancellationToken)
        {
            if (agentId == null)
                throw new ArgumentNullException("agentId");

            return GetAgentHostInformationAsync<MemoryInformation>(agentId, HostInformationType.Memory, cancellationToken);
        }

        /// <inheritdoc/>
        public Task<HostInformation<ReadOnlyCollection<NetworkInterfaceInformation>>> GetNetworkInterfaceInformationAsync(AgentId agentId, CancellationToken cancellationToken)
        {
            if (agentId == null)
                throw new ArgumentNullException("agentId");

            return GetAgentHostInformationAsync<ReadOnlyCollection<NetworkInterfaceInformation>>(agentId, HostInformationType.NetworkInterfaces, cancellationToken);
        }

        /// <inheritdoc/>
        public Task<HostInformation<ReadOnlyCollection<ProcessInformation>>> GetProcessInformationAsync(AgentId agentId, CancellationToken cancellationToken)
        {
            if (agentId == null)
                throw new ArgumentNullException("agentId");

            return GetAgentHostInformationAsync<ReadOnlyCollection<ProcessInformation>>(agentId, HostInformationType.Processes, cancellationToken);
        }

        /// <inheritdoc/>
        public Task<HostInformation<SystemInformation>> GetSystemInformationAsync(AgentId agentId, CancellationToken cancellationToken)
        {
            if (agentId == null)
                throw new ArgumentNullException("agentId");

            return GetAgentHostInformationAsync<SystemInformation>(agentId, HostInformationType.System, cancellationToken);
        }

        /// <inheritdoc/>
        public Task<HostInformation<ReadOnlyCollection<LoginInformation>>> GetLoginInformationAsync(AgentId agentId, CancellationToken cancellationToken)
        {
            if (agentId == null)
                throw new ArgumentNullException("agentId");

            return GetAgentHostInformationAsync<ReadOnlyCollection<LoginInformation>>(agentId, HostInformationType.Who, cancellationToken);
        }

        /// <inheritdoc/>
        public Task<ReadOnlyCollectionPage<CheckTarget, CheckTargetId>> ListAgentCheckTargetsAsync(EntityId entityId, CheckTypeId agentCheckType, CheckTargetId marker, int? limit, CancellationToken cancellationToken)
        {
            if (entityId == null)
                throw new ArgumentNullException("entityId");
            if (agentCheckType == null)
                throw new ArgumentNullException("agentCheckType");
            if (!agentCheckType.IsAgent)
                throw new ArgumentException(string.Format("The specified check type '{0}' is not an agent check.", agentCheckType), "agentCheckType");
            if (limit <= 0)
                throw new ArgumentOutOfRangeException("limit");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/entities/{entityId}/agent/check_types/{agentCheckType}/targets");
            var parameters = new Dictionary<string, string> { { "entityId", entityId.Value }, { "agentCheckType", agentCheckType.ToString() } };
            if (marker != null)
                parameters.Add("marker", marker.Value);
            if (limit != null)
                parameters.Add("limit", limit.ToString());

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<JObject>> requestResource =
                GetResponseAsyncFunc<JObject>(cancellationToken);

            Func<Task<JObject>, ReadOnlyCollectionPage<CheckTarget, CheckTargetId>> resultSelector =
                task =>
                {
                    JObject result = task.Result;
                    if (result == null)
                        return null;

                    JToken valuesToken = result["values"];
                    if (valuesToken == null)
                        return null;

                    JToken metadataToken = result["metadata"];

                    CheckTarget[] values = valuesToken.ToObject<CheckTarget[]>();
                    Func<CheckTargetId, CancellationToken, Task<ReadOnlyCollectionPage<CheckTarget, CheckTargetId>>> getNextPageAsync =
                        (nextMarker, nextCancellationToken) => ListAgentCheckTargetsAsync(entityId, agentCheckType, nextMarker, limit, nextCancellationToken);
                    IDictionary<string, object> metadata = metadataToken != null ? metadataToken.ToObject<IDictionary<string, object>>() : null;
                    return new ReadOnlyCollectionPage<CheckTarget, CheckTargetId>(values, getNextPageAsync, metadata);
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        #endregion

        /// <summary>
        /// Get generic agent host information.
        /// </summary>
        /// <typeparam name="T">The type of the object modeling the JSON representation of the host information reported by the agent.</typeparam>
        /// <param name="agentId">The agent ID. This is obtained from <see cref="Agent.Id">Agent.Id</see>.</param>
        /// <param name="hostInformation">The type of information to check.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. When
        /// the task completes successfully, the <see cref="Task{TResult}.Result"/>
        /// property will contain a <see cref="HostInformation{T}"/>
        /// object containing the host information.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="agentId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="hostInformation"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-agent-host_info.html">Agent Host Information (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        protected Task<HostInformation<T>> GetAgentHostInformationAsync<T>(AgentId agentId, HostInformationType hostInformation, CancellationToken cancellationToken)
        {
            if (agentId == null)
                throw new ArgumentNullException("agentId");
            if (hostInformation == null)
                throw new ArgumentNullException("hostInformation");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/agents/{agentId}/host_info/{hostInfo}");
            var parameters = new Dictionary<string, string> { { "agentId", agentId.Value }, { "hostInfo", hostInformation.ToString() } };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<HostInformation<T>>> requestResource =
                GetResponseAsyncFunc<HostInformation<T>>(cancellationToken);

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource);
        }

        /// <inheritdoc/>
        /// <remarks>
        /// This method returns a cached base address if one is available. If no cached address is
        /// available, <see cref="ProviderBase{TProvider}.GetServiceEndpoint"/> is called to obtain
        /// an <see cref="Endpoint"/> with the type <c>rax:monitor</c> and preferred type <c>cloudMonitoring</c>.
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
                    Endpoint endpoint = GetServiceEndpoint(null, "rax:monitor", "cloudMonitoring", null);
                    _baseUri = new Uri(endpoint.PublicURL);
                    return _baseUri;
                });
        }
    }
}
