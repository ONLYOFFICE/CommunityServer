namespace net.openstack.Providers.Rackspace
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using JSIStudios.SimpleRESTServices.Client;
    using net.openstack.Core;
    using net.openstack.Core.Collections;
    using net.openstack.Core.Domain;
    using net.openstack.Core.Providers;
    using net.openstack.Providers.Rackspace.Objects.Dns;
    using Newtonsoft.Json.Linq;
    using CancellationToken = System.Threading.CancellationToken;
    using HttpResponseCodeValidator = net.openstack.Providers.Rackspace.Validators.HttpResponseCodeValidator;
    using IHttpResponseCodeValidator = net.openstack.Core.Validators.IHttpResponseCodeValidator;
    using JsonRestServices = JSIStudios.SimpleRESTServices.Client.Json.JsonRestServices;

    /// <summary>
    /// Provides an implementation of <see cref="IDnsService"/> for operating
    /// with Rackspace's Cloud DNS product.
    /// </summary>
    /// <seealso href="http://docs.rackspace.com/cdns/api/v1.0/cdns-devguide/content/index.html">Rackspace Cloud DNS Developer Guide - API v1.0</seealso>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    public class CloudDnsProvider : ProviderBase<IDnsService>, IDnsService
    {
        /// <summary>
        /// Specifies whether the <see cref="Endpoint.PublicURL"/> or <see cref="Endpoint.InternalURL"/>
        /// should be used for accessing the Cloud DNS API.
        /// </summary>
        private readonly bool _internalUrl;

        /// <summary>
        /// This field caches the base URI used for accessing the Cloud DNS service.
        /// </summary>
        /// <seealso cref="GetBaseUriAsync"/>
        private Uri _baseUri;

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudDnsProvider"/> class with
        /// the specified values.
        /// </summary>
        /// <param name="defaultIdentity">The default identity to use for calls that do not explicitly specify an identity. If this value is <see langword="null"/>, no default identity is available so all calls must specify an explicit identity.</param>
        /// <param name="defaultRegion">The default region to use for calls that do not explicitly specify a region. If this value is <see langword="null"/>, the default region for the user will be used; otherwise if the service uses region-specific endpoints all calls must specify an explicit region.</param>
        /// <param name="internalUrl"><see langword="true"/> to use the endpoint's <see cref="Endpoint.InternalURL"/>; otherwise <see langword="false"/> to use the endpoint's <see cref="Endpoint.PublicURL"/>.</param>
        /// <param name="identityProvider">The identity provider to use for authenticating requests to this provider. If this value is <see langword="null"/>, a new instance of <see cref="CloudIdentityProvider"/> is created using <paramref name="defaultIdentity"/> as the default identity.</param>
        public CloudDnsProvider(CloudIdentity defaultIdentity, string defaultRegion, bool internalUrl, IIdentityProvider identityProvider)
            : this(defaultIdentity, defaultRegion, internalUrl, identityProvider, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudDnsProvider"/> class with
        /// the specified values.
        /// </summary>
        /// <param name="defaultIdentity">The default identity to use for calls that do not explicitly specify an identity. If this value is <see langword="null"/>, no default identity is available so all calls must specify an explicit identity.</param>
        /// <param name="defaultRegion">The default region to use for calls that do not explicitly specify a region. If this value is <see langword="null"/>, the default region for the user will be used; otherwise if the service uses region-specific endpoints all calls must specify an explicit region.</param>
        /// <param name="internalUrl"><see langword="true"/> to use the endpoint's <see cref="Endpoint.InternalURL"/>; otherwise <see langword="false"/> to use the endpoint's <see cref="Endpoint.PublicURL"/>.</param>
        /// <param name="identityProvider">The identity provider to use for authenticating requests to this provider. If this value is <see langword="null"/>, a new instance of <see cref="CloudIdentityProvider"/> is created using <paramref name="defaultIdentity"/> as the default identity.</param>
        /// <param name="restService">The implementation of <see cref="IRestService"/> to use for executing synchronous REST requests. If this value is <see langword="null"/>, the provider will use a new instance of <see cref="JsonRestServices"/>.</param>
        /// <param name="httpStatusCodeValidator">The HTTP status code validator to use for synchronous REST requests. If this value is <see langword="null"/>, the provider will use <see cref="HttpResponseCodeValidator.Default"/>.</param>
        protected CloudDnsProvider(CloudIdentity defaultIdentity, string defaultRegion, bool internalUrl, IIdentityProvider identityProvider, IRestService restService, IHttpResponseCodeValidator httpStatusCodeValidator)
            : base(defaultIdentity, defaultRegion, identityProvider, restService, httpStatusCodeValidator)
        {
            _internalUrl = internalUrl;
        }

        #region IDnsService Members

        /// <inheritdoc/>
        public Task<DnsServiceLimits> ListLimitsAsync(CancellationToken cancellationToken)
        {
            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/limits");
            var parameters = new Dictionary<string, string>();

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<JObject>> requestResource =
                GetResponseAsyncFunc<JObject>(cancellationToken);

            Func<Task<JObject>, DnsServiceLimits> resultSelector =
                task =>
                {
                    JObject result = task.Result;
                    if (result == null)
                        return null;

                    JToken limits = result["limits"];
                    if (limits == null)
                        return null;

                    return limits.ToObject<DnsServiceLimits>();
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task<ReadOnlyCollection<LimitType>> ListLimitTypesAsync(CancellationToken cancellationToken)
        {
            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/limits/types");
            var parameters = new Dictionary<string, string>();

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<JObject>> requestResource =
                GetResponseAsyncFunc<JObject>(cancellationToken);

            Func<Task<JObject>, ReadOnlyCollection<LimitType>> resultSelector =
                task =>
                {
                    JObject result = task.Result;
                    if (result == null)
                        return null;

                    JToken limitTypes = result["limitTypes"];
                    if (limitTypes == null)
                        return null;

                    return limitTypes.ToObject<ReadOnlyCollection<LimitType>>();
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task<DnsServiceLimits> ListLimitsAsync(LimitType type, CancellationToken cancellationToken)
        {
            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/limits/{type}");
            var parameters = new Dictionary<string, string>
                {
                    { "type", type.Name.ToLowerInvariant() }
                };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<JObject>> requestResource =
                GetResponseAsyncFunc<JObject>(cancellationToken);

            Func<Task<JObject>, DnsServiceLimits> resultSelector =
                task =>
                {
                    JObject result = task.Result;
                    if (result == null)
                        return null;

                    JToken limits = result["limits"];
                    if (limits == null)
                        return null;

                    return limits.ToObject<DnsServiceLimits>();
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task<DnsJob> GetJobStatusAsync(DnsJob job, bool showDetails, CancellationToken cancellationToken)
        {
            if (job == null)
                throw new ArgumentNullException("job");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/status/{jobId}?showDetails={showDetails}");
            var parameters = new Dictionary<string, string>
                {
                    { "jobId", job.Id.Value },
                    { "showDetails", showDetails ? "true" : "false" },
                };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<JObject>> requestResource =
                GetResponseAsyncFunc<JObject>(cancellationToken);

            Func<Task<JObject>, DnsJob> resultSelector =
                task =>
                {
                    JObject result = task.Result;
                    if (result == null)
                        return null;

                    return result.ToObject<DnsJob>();
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task<DnsJob<TResponse>> GetJobStatusAsync<TResponse>(DnsJob<TResponse> job, bool showDetails, CancellationToken cancellationToken)
        {
            if (job == null)
                throw new ArgumentNullException("job");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/status/{jobId}?showDetails={showDetails}");
            var parameters = new Dictionary<string, string>
                {
                    { "jobId", job.Id.Value },
                    { "showDetails", showDetails ? "true" : "false" },
                };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<JObject>> requestResource =
                GetResponseAsyncFunc<JObject>(cancellationToken);

            Func<Task<JObject>, DnsJob<TResponse>> resultSelector =
                task =>
                {
                    JObject result = task.Result;
                    if (result == null)
                        return null;

                    return result.ToObject<DnsJob<TResponse>>();
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task<Tuple<ReadOnlyCollectionPage<DnsDomain>, int?>> ListDomainsAsync(string domainName, int? offset, int? limit, CancellationToken cancellationToken)
        {
            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/domains/?name={name}&offset={offset}&limit={limit}");
            var parameters = new Dictionary<string, string>();
            if (domainName != null)
                parameters.Add("name", domainName);
            if (offset != null)
                parameters.Add("offset", offset.ToString());
            if (limit != null)
                parameters.Add("limit", limit.ToString());

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<JObject>> requestResource =
                GetResponseAsyncFunc<JObject>(cancellationToken);

            Func<Task<JObject>, Tuple<ReadOnlyCollectionPage<DnsDomain>, int?>> resultSelector =
                task =>
                {
                    JObject result = task.Result;
                    if (result == null)
                        return null;

                    JToken domains = result["domains"];
                    if (domains == null)
                        return null;

                    int? totalEntries = null;
                    JToken totalEntriesToken = result["totalEntries"];
                    if (totalEntriesToken != null)
                        totalEntries = totalEntriesToken.ToObject<int>();

                    DnsDomain[] currentPage = domains.ToObject<DnsDomain[]>();
                    if (currentPage == null || currentPage.Length == 0)
                        return Tuple.Create(ReadOnlyCollectionPage<DnsDomain>.Empty, totalEntries);
                    else if (!HasNextLink(result))
                        return Tuple.Create((ReadOnlyCollectionPage<DnsDomain>)new BasicReadOnlyCollectionPage<DnsDomain>(currentPage, null), totalEntries);

                    int nextOffset = (offset ?? 0) + currentPage.Length;
                    Func<CancellationToken, Task<ReadOnlyCollectionPage<DnsDomain>>> getNextPageAsync =
                        nextCancellationToken =>
                        {
                            return ListDomainsAsync(domainName, nextOffset, limit, nextCancellationToken)
                                .Select(i => i.Result.Item1);
                        };
                    ReadOnlyCollectionPage<DnsDomain> page = new BasicReadOnlyCollectionPage<DnsDomain>(currentPage, getNextPageAsync);
                    return Tuple.Create(page, totalEntries);
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task<DnsDomain> ListDomainDetailsAsync(DomainId domainId, bool showRecords, bool showSubdomains, CancellationToken cancellationToken)
        {
            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/domains/{domainId}?showRecords={showRecords}&showSubdomains={showSubdomains}");
            var parameters = new Dictionary<string, string>()
            {
                { "domainId", domainId.Value },
                { "showRecords", showRecords ? "true" : "false" },
                { "showSubdomains", showSubdomains ? "true" : "false" },
            };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<JObject>> requestResource =
                GetResponseAsyncFunc<JObject>(cancellationToken);

            Func<Task<JObject>, DnsDomain> resultSelector =
                task =>
                {
                    JObject result = task.Result;
                    if (result == null)
                        return null;

                    return result.ToObject<DnsDomain>();
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task<DnsDomainChanges> ListDomainChangesAsync(DomainId domainId, DateTimeOffset? since, CancellationToken cancellationToken)
        {
            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/domains/{domainId}/changes?since={since}");
            var parameters = new Dictionary<string, string>()
                {
                    { "domainId", domainId.Value },
                };
            if (since.HasValue)
                parameters.Add("since", since.Value.ToString("G"));

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<JObject>> requestResource =
                GetResponseAsyncFunc<JObject>(cancellationToken);

            Func<Task<JObject>, DnsDomainChanges> resultSelector =
                task =>
                {
                    JObject result = task.Result;
                    if (result == null)
                        return null;

                    return result.ToObject<DnsDomainChanges>();
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task<DnsJob<ExportedDomain>> ExportDomainAsync(DomainId domainId, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<DnsJob<ExportedDomain>> progress)
        {
            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/domains/{domainId}/export");
            var parameters = new Dictionary<string, string>()
                {
                    { "domainId", domainId.Value },
                };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<JObject>> requestResource =
                GetResponseAsyncFunc<JObject>(cancellationToken);

            Func<Task<JObject>, DnsJob<ExportedDomain>> resultSelector =
                task =>
                {
                    JObject result = task.Result;
                    if (result == null)
                        return null;

                    DnsJob<ExportedDomain> job = task.Result.ToObject<DnsJob<ExportedDomain>>();
                    if (completionOption == AsyncCompletionOption.RequestCompleted)
                        job = WaitForJobAsync(job, true, cancellationToken, progress).Result;

                    return job;
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task<DnsJob<DnsDomains>> CreateDomainsAsync(DnsConfiguration configuration, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<DnsJob<DnsDomains>> progress)
        {
            if (configuration == null)
                throw new ArgumentNullException("configuration");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/domains");
            var parameters = new Dictionary<string, string>();

            Func<Task<Tuple<IdentityToken, Uri>>, Task<HttpWebRequest>> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.POST, template, parameters, configuration);

            Func<Task<HttpWebRequest>, Task<JObject>> requestResource =
                GetResponseAsyncFunc<JObject>(cancellationToken);

            Func<Task<JObject>, DnsJob<DnsDomains>> resultSelector =
                task =>
                {
                    JObject result = task.Result;
                    if (result == null)
                        return null;

                    DnsJob<DnsDomains> job = task.Result.ToObject<DnsJob<DnsDomains>>();
                    if (completionOption == AsyncCompletionOption.RequestCompleted)
                        job = WaitForJobAsync(job, true, cancellationToken, progress).Result;

                    return job;
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Then(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task<DnsJob> UpdateDomainsAsync(DnsUpdateConfiguration configuration, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<DnsJob> progress)
        {
            if (configuration == null)
                throw new ArgumentNullException("configuration");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/domains");
            var parameters = new Dictionary<string, string>();

            Func<Task<Tuple<IdentityToken, Uri>>, Task<HttpWebRequest>> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.PUT, template, parameters, configuration);

            Func<Task<HttpWebRequest>, Task<JObject>> requestResource =
                GetResponseAsyncFunc<JObject>(cancellationToken);

            Func<Task<JObject>, Task<DnsJob>> resultSelector =
                task =>
                {
                    JObject result = task.Result;
                    if (result == null)
                        return null;

                    DnsJob job = task.Result.ToObject<DnsJob>();
                    if (completionOption == AsyncCompletionOption.RequestCompleted)
                        return WaitForJobAsync(job, true, cancellationToken, progress);

                    return InternalTaskExtensions.CompletedTask(job);
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Then(prepareRequest)
                .Then(requestResource)
                .Then(resultSelector);
        }

        /// <inheritdoc/>
        public Task<DnsJob<DnsDomains>> CloneDomainAsync(DomainId domainId, string cloneName, bool? cloneSubdomains, bool? modifyRecordData, bool? modifyEmailAddress, bool? modifyComment, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<DnsJob<DnsDomains>> progress)
        {
            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/domains/{domainId}/clone?cloneName={cloneName}&cloneSubdomains={cloneSubdomains}&modifyRecordData={modifyRecordData}&modifyEmailAddress={modifyEmailAddress}&modifyComment={modifyComment}");
            var parameters = new Dictionary<string, string> { { "domainId", domainId.Value }, { "cloneName", cloneName } };
            if (cloneSubdomains != null)
                parameters.Add("cloneSubdomains", cloneSubdomains.Value ? "true" : "false");
            if (modifyRecordData != null)
                parameters.Add("modifyRecordData", modifyRecordData.Value ? "true" : "false");
            if (modifyEmailAddress != null)
                parameters.Add("modifyEmailAddress", modifyEmailAddress.Value ? "true" : "false");
            if (modifyComment != null)
                parameters.Add("modifyComment", modifyComment.Value ? "true" : "false");

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.POST, template, parameters);

            Func<Task<HttpWebRequest>, Task<JObject>> requestResource =
                GetResponseAsyncFunc<JObject>(cancellationToken);

            Func<Task<JObject>, DnsJob<DnsDomains>> resultSelector =
                task =>
                {
                    JObject result = task.Result;
                    if (result == null)
                        return null;

                    DnsJob<DnsDomains> job = task.Result.ToObject<DnsJob<DnsDomains>>();
                    if (completionOption == AsyncCompletionOption.RequestCompleted)
                        job = WaitForJobAsync(job, true, cancellationToken, progress).Result;

                    return job;
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task<DnsJob<DnsDomains>> ImportDomainAsync(IEnumerable<SerializedDomain> serializedDomains, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<DnsJob<DnsDomains>> progress)
        {
            if (serializedDomains == null)
                throw new ArgumentNullException("serializedDomains");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/domains/import");
            var parameters = new Dictionary<string, string>();

            JObject request = new JObject(new JProperty("domains", JArray.FromObject(serializedDomains)));
            Func<Task<Tuple<IdentityToken, Uri>>, Task<HttpWebRequest>> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.POST, template, parameters, request);

            Func<Task<HttpWebRequest>, Task<JObject>> requestResource =
                GetResponseAsyncFunc<JObject>(cancellationToken);

            Func<Task<JObject>, DnsJob<DnsDomains>> resultSelector =
                task =>
                {
                    JObject result = task.Result;
                    if (result == null)
                        return null;

                    DnsJob<DnsDomains> job = task.Result.ToObject<DnsJob<DnsDomains>>();
                    if (completionOption == AsyncCompletionOption.RequestCompleted)
                        job = WaitForJobAsync(job, true, cancellationToken, progress).Result;

                    return job;
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Then(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task<DnsJob> RemoveDomainsAsync(IEnumerable<DomainId> domainIds, bool deleteSubdomains, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<DnsJob> progress)
        {
            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/domains?deleteSubdomains={deleteSubdomains}");
            var parameters = new Dictionary<string, string>()
            {
                { "deleteSubdomains", deleteSubdomains ? "true" : "false" },
            };

            Func<Uri, Uri> transform =
                uri =>
                {
                    UriBuilder builder = new UriBuilder(uri);
                    builder.Query = builder.Query.TrimStart('?') + string.Concat(domainIds.Select(domainId => "&id=" + domainId.Value));
                    return builder.Uri;
                };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.DELETE, template, parameters, transform);

            Func<Task<HttpWebRequest>, Task<JObject>> requestResource =
                GetResponseAsyncFunc<JObject>(cancellationToken);

            Func<Task<JObject>, Task<DnsJob>> resultSelector =
                task =>
                {
                    JObject result = task.Result;
                    if (result == null)
                        return null;

                    DnsJob job = task.Result.ToObject<DnsJob>();
                    if (completionOption == AsyncCompletionOption.RequestCompleted)
                        return WaitForJobAsync(job, true, cancellationToken, progress);

                    return InternalTaskExtensions.CompletedTask(job);
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Then(resultSelector);
        }

        /// <inheritdoc/>
        public Task<Tuple<ReadOnlyCollectionPage<DnsSubdomain>, int?>> ListSubdomainsAsync(DomainId domainId, int? offset, int? limit, CancellationToken cancellationToken)
        {
            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/domains/{domainId}/subdomains?offset={offset}&limit={limit}");
            var parameters = new Dictionary<string, string>
                {
                    { "domainId", domainId.Value }
                };
            if (offset != null)
                parameters.Add("offset", offset.ToString());
            if (limit != null)
                parameters.Add("limit", limit.ToString());

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<JObject>> requestResource =
                GetResponseAsyncFunc<JObject>(cancellationToken);

            Func<Task<JObject>, Tuple<ReadOnlyCollectionPage<DnsSubdomain>, int?>> resultSelector =
                task =>
                {
                    JObject result = task.Result;
                    if (result == null)
                        return null;

                    JToken domains = result["domains"];
                    if (domains == null)
                        return null;

                    int? totalEntries = null;
                    JToken totalEntriesToken = result["totalEntries"];
                    if (totalEntriesToken != null)
                        totalEntries = totalEntriesToken.ToObject<int>();

                    DnsSubdomain[] currentPage = domains.ToObject<DnsSubdomain[]>();
                    if (currentPage == null || currentPage.Length == 0)
                        return Tuple.Create(ReadOnlyCollectionPage<DnsSubdomain>.Empty, totalEntries);
                    else if (!HasNextLink(result))
                        return Tuple.Create((ReadOnlyCollectionPage<DnsSubdomain>)new BasicReadOnlyCollectionPage<DnsSubdomain>(currentPage, null), totalEntries);

                    int nextOffset = (offset ?? 0) + currentPage.Length;
                    Func<CancellationToken, Task<ReadOnlyCollectionPage<DnsSubdomain>>> getNextPageAsync =
                        nextCancellationToken =>
                        {
                            return ListSubdomainsAsync(domainId, nextOffset, limit, nextCancellationToken)
                                .Select(i => i.Result.Item1);
                        };
                    ReadOnlyCollectionPage<DnsSubdomain> page = new BasicReadOnlyCollectionPage<DnsSubdomain>(currentPage, getNextPageAsync);
                    return Tuple.Create(page, totalEntries);
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task<Tuple<ReadOnlyCollectionPage<DnsRecord>, int?>> ListRecordsAsync(DomainId domainId, DnsRecordType recordType, string recordName, string recordData, int? offset, int? limit, CancellationToken cancellationToken)
        {
            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/domains/{domainId}/records?type={recordType}&name={recordName}&data={recordData}&offset={offset}&limit={limit}");
            var parameters = new Dictionary<string, string>
                {
                    { "domainId", domainId.Value }
                };
            if (recordType != null)
                parameters.Add("recordType", recordType.Name);
            if (recordName != null)
                parameters.Add("recordName", recordName);
            if (recordData != null)
                parameters.Add("recordData", recordData);
            if (offset != null)
                parameters.Add("offset", offset.ToString());
            if (limit != null)
                parameters.Add("limit", limit.ToString());

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<JObject>> requestResource =
                GetResponseAsyncFunc<JObject>(cancellationToken);

            Func<Task<JObject>, Tuple<ReadOnlyCollectionPage<DnsRecord>, int?>> resultSelector =
                task =>
                {
                    JObject result = task.Result;
                    if (result == null)
                        return null;

                    JToken records = result["records"];
                    if (records == null)
                        return null;

                    int? totalEntries = null;
                    JToken totalEntriesToken = result["totalEntries"];
                    if (totalEntriesToken != null)
                        totalEntries = totalEntriesToken.ToObject<int>();

                    DnsRecord[] currentPage = records.ToObject<DnsRecord[]>();
                    if (currentPage == null || currentPage.Length == 0)
                        return Tuple.Create(ReadOnlyCollectionPage<DnsRecord>.Empty, totalEntries);
                    else if (!HasNextLink(result))
                        return Tuple.Create((ReadOnlyCollectionPage<DnsRecord>)new BasicReadOnlyCollectionPage<DnsRecord>(currentPage, null), totalEntries);

                    int nextOffset = (offset ?? 0) + currentPage.Length;
                    Func<CancellationToken, Task<ReadOnlyCollectionPage<DnsRecord>>> getNextPageAsync =
                        nextCancellationToken =>
                        {
                            return ListRecordsAsync(domainId, recordType, recordName, recordData, nextOffset, limit, nextCancellationToken)
                                .Select(i => i.Result.Item1);
                        };
                    ReadOnlyCollectionPage<DnsRecord> page = new BasicReadOnlyCollectionPage<DnsRecord>(currentPage, getNextPageAsync);
                    return Tuple.Create(page, totalEntries);
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task<DnsRecord> ListRecordDetailsAsync(DomainId domainId, RecordId recordId, CancellationToken cancellationToken)
        {
            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/domains/{domainId}/records/{recordId}");
            var parameters = new Dictionary<string, string>
                {
                    { "domainId", domainId.Value },
                    { "recordId", recordId.Value }
                };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<JObject>> requestResource =
                GetResponseAsyncFunc<JObject>(cancellationToken);

            Func<Task<JObject>, DnsRecord> resultSelector =
                task =>
                {
                    JObject result = task.Result;
                    if (result == null)
                        return null;

                    return result.ToObject<DnsRecord>();
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task<DnsJob<DnsRecordsList>> AddRecordsAsync(DomainId domainId, IEnumerable<DnsDomainRecordConfiguration> recordConfigurations, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<DnsJob<DnsRecordsList>> progress)
        {
            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/domains/{domainId}/records");
            var parameters = new Dictionary<string, string>()
                {
                    { "domainId", domainId.Value }
                };

            JObject request = new JObject(new JProperty("records", JArray.FromObject(recordConfigurations)));
            Func<Task<Tuple<IdentityToken, Uri>>, Task<HttpWebRequest>> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.POST, template, parameters, request);

            Func<Task<HttpWebRequest>, Task<JObject>> requestResource =
                GetResponseAsyncFunc<JObject>(cancellationToken);

            Func<Task<JObject>, DnsJob<DnsRecordsList>> resultSelector =
                task =>
                {
                    JObject result = task.Result;
                    if (result == null)
                        return null;

                    DnsJob<DnsRecordsList> job = task.Result.ToObject<DnsJob<DnsRecordsList>>();
                    if (completionOption == AsyncCompletionOption.RequestCompleted)
                        job = WaitForJobAsync(job, true, cancellationToken, progress).Result;

                    return job;
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Then(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task<DnsJob> UpdateRecordsAsync(DomainId domainId, IEnumerable<DnsDomainRecordUpdateConfiguration> recordConfigurations, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<DnsJob> progress)
        {
            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/domains/{domainId}/records");
            var parameters = new Dictionary<string, string>()
                {
                    { "domainId", domainId.Value }
                };

            JObject request = new JObject(new JProperty("records", JArray.FromObject(recordConfigurations)));
            Func<Task<Tuple<IdentityToken, Uri>>, Task<HttpWebRequest>> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.PUT, template, parameters, request);

            Func<Task<HttpWebRequest>, Task<JObject>> requestResource =
                GetResponseAsyncFunc<JObject>(cancellationToken);

            Func<Task<JObject>, Task<DnsJob>> resultSelector =
                task =>
                {
                    JObject result = task.Result;
                    if (result == null)
                        return null;

                    DnsJob job = task.Result.ToObject<DnsJob>();
                    if (completionOption == AsyncCompletionOption.RequestCompleted)
                        return WaitForJobAsync(job, true, cancellationToken, progress);

                    return InternalTaskExtensions.CompletedTask(job);
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Then(prepareRequest)
                .Then(requestResource)
                .Then(resultSelector);
        }

        /// <inheritdoc/>
        public Task<DnsJob> RemoveRecordsAsync(DomainId domainId, IEnumerable<RecordId> recordIds, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<DnsJob> progress)
        {
            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/domains/{domainId}/records");
            var parameters = new Dictionary<string, string>()
            {
                { "domainId", domainId.Value },
            };

            Func<Uri, Uri> transform =
                uri =>
                {
                    UriBuilder builder = new UriBuilder(uri);
                    builder.Query = builder.Query.TrimStart('?') + string.Concat(recordIds.Select(recordId => "&id=" + recordId.Value));
                    return builder.Uri;
                };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.DELETE, template, parameters, transform);

            Func<Task<HttpWebRequest>, Task<JObject>> requestResource =
                GetResponseAsyncFunc<JObject>(cancellationToken);

            Func<Task<JObject>, Task<DnsJob>> resultSelector =
                task =>
                {
                    JObject result = task.Result;
                    if (result == null)
                        return null;

                    DnsJob job = task.Result.ToObject<DnsJob>();
                    if (completionOption == AsyncCompletionOption.RequestCompleted)
                        return WaitForJobAsync(job, true, cancellationToken, progress);

                    return InternalTaskExtensions.CompletedTask(job);
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Then(resultSelector);
        }

        /// <inheritdoc/>
        public Task<Tuple<ReadOnlyCollectionPage<DnsRecord>, int?>> ListPtrRecordsAsync(string serviceName, Uri deviceResourceUri, int? offset, int? limit, CancellationToken cancellationToken)
        {
            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/rdns/{serviceName}?href={deviceResourceUri}&offset={offset}&limit={limit}");
            var parameters = new Dictionary<string, string>
                {
                    { "serviceName", serviceName },
                    { "deviceResourceUri", deviceResourceUri.AbsoluteUri },
                };
            if (offset != null)
                parameters.Add("offset", offset.ToString());
            if (limit != null)
                parameters.Add("limit", limit.ToString());

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<JObject>> requestResource =
                GetResponseAsyncFunc<JObject>(cancellationToken);

            Func<Task<JObject>, Tuple<ReadOnlyCollectionPage<DnsRecord>, int?>> resultSelector =
                task =>
                {
                    JObject result = task.Result;
                    if (result == null)
                        return null;

                    JToken records = result["records"];
                    if (records == null)
                        return null;

                    int? totalEntries = null;
                    JToken totalEntriesToken = result["totalEntries"];
                    if (totalEntriesToken != null)
                        totalEntries = totalEntriesToken.ToObject<int>();

                    DnsRecord[] currentPage = records.ToObject<DnsRecord[]>();
                    if (currentPage == null || currentPage.Length == 0)
                        return Tuple.Create(ReadOnlyCollectionPage<DnsRecord>.Empty, totalEntries);
                    else if (!HasNextLink(result))
                        return Tuple.Create((ReadOnlyCollectionPage<DnsRecord>)new BasicReadOnlyCollectionPage<DnsRecord>(currentPage, null), totalEntries);

                    int nextOffset = (offset ?? 0) + currentPage.Length;
                    Func<CancellationToken, Task<ReadOnlyCollectionPage<DnsRecord>>> getNextPageAsync =
                        nextCancellationToken =>
                        {
                            return ListPtrRecordsAsync(serviceName, deviceResourceUri, nextOffset, limit, nextCancellationToken)
                                .Select(i => i.Result.Item1);
                        };
                    ReadOnlyCollectionPage<DnsRecord> page = new BasicReadOnlyCollectionPage<DnsRecord>(currentPage, getNextPageAsync);
                    return Tuple.Create(page, totalEntries);
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task<DnsRecord> ListPtrRecordDetailsAsync(string serviceName, Uri deviceResourceUri, RecordId recordId, CancellationToken cancellationToken)
        {
            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/rdns/{serviceName}/{recordId}?href={deviceResourceUri}");
            var parameters = new Dictionary<string, string>
                {
                    { "serviceName", serviceName },
                    { "deviceResourceUri", deviceResourceUri.AbsoluteUri },
                    { "recordId", recordId.Value },
                };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<JObject>> requestResource =
                GetResponseAsyncFunc<JObject>(cancellationToken);

            Func<Task<JObject>, DnsRecord> resultSelector =
                task =>
                {
                    JObject result = task.Result;
                    if (result == null)
                        return null;

                    return result.ToObject<DnsRecord>();
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task<DnsJob<DnsRecordsList>> AddPtrRecordsAsync(string serviceName, Uri deviceResourceUri, IEnumerable<DnsDomainRecordConfiguration> recordConfigurations, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<DnsJob<DnsRecordsList>> progress)
        {
            if (serviceName == null)
                throw new ArgumentNullException("serviceName");
            if (deviceResourceUri == null)
                throw new ArgumentNullException("deviceResourceUri");
            if (string.IsNullOrEmpty(serviceName))
                throw new ArgumentException("serviceName cannot be empty");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/rdns");
            var parameters = new Dictionary<string, string>();

            JObject request = new JObject(
                new JProperty("link", new JObject(
                    new JProperty("href", new JValue(deviceResourceUri.AbsoluteUri)),
                    new JProperty("rel", new JValue(serviceName)),
                    new JProperty("content", new JValue(string.Empty)))),
                new JProperty("recordsList", new JObject(
                    new JProperty("records", JArray.FromObject(recordConfigurations)))));
            Func<Task<Tuple<IdentityToken, Uri>>, Task<HttpWebRequest>> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.POST, template, parameters, request);

            Func<Task<HttpWebRequest>, Task<JObject>> requestResource =
                GetResponseAsyncFunc<JObject>(cancellationToken);

            Func<Task<JObject>, DnsJob<DnsRecordsList>> resultSelector =
                task =>
                {
                    JObject result = task.Result;
                    if (result == null)
                        return null;

                    DnsJob<DnsRecordsList> job = task.Result.ToObject<DnsJob<DnsRecordsList>>();
                    if (completionOption == AsyncCompletionOption.RequestCompleted)
                        job = WaitForJobAsync(job, true, cancellationToken, progress).Result;

                    return job;
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Then(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task<DnsJob> UpdatePtrRecordsAsync(string serviceName, Uri deviceResourceUri, IEnumerable<DnsDomainRecordUpdateConfiguration> recordConfigurations, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<DnsJob> progress)
        {
            if (serviceName == null)
                throw new ArgumentNullException("serviceName");
            if (deviceResourceUri == null)
                throw new ArgumentNullException("deviceResourceUri");
            if (string.IsNullOrEmpty(serviceName))
                throw new ArgumentException("serviceName cannot be empty");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/rdns");
            var parameters = new Dictionary<string, string>();

            JObject request = new JObject(
                new JProperty("link", new JObject(
                    new JProperty("href", new JValue(deviceResourceUri.AbsoluteUri)),
                    new JProperty("rel", new JValue(serviceName)),
                    new JProperty("content", new JValue(string.Empty)))),
                new JProperty("recordsList", new JObject(
                    new JProperty("records", JArray.FromObject(recordConfigurations)))));
            Func<Task<Tuple<IdentityToken, Uri>>, Task<HttpWebRequest>> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.PUT, template, parameters, request);

            Func<Task<HttpWebRequest>, Task<JObject>> requestResource =
                GetResponseAsyncFunc<JObject>(cancellationToken);

            Func<Task<JObject>, Task<DnsJob>> resultSelector =
                task =>
                {
                    JObject result = task.Result;
                    if (result == null)
                        return null;

                    DnsJob job = task.Result.ToObject<DnsJob>();
                    if (completionOption == AsyncCompletionOption.RequestCompleted)
                        return WaitForJobAsync(job, true, cancellationToken, progress);

                    return InternalTaskExtensions.CompletedTask(job);
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Then(prepareRequest)
                .Then(requestResource)
                .Then(resultSelector);
        }

        /// <inheritdoc/>
        public Task<DnsJob> RemovePtrRecordsAsync(string serviceName, Uri deviceResourceUri, IPAddress ipAddress, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<DnsJob> progress)
        {
            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/rdns/{serviceName}?href={deviceResourceUri}&ip={ipAddress}");
            var parameters = new Dictionary<string, string>()
                {
                    { "serviceName", serviceName },
                    { "deviceResourceUri", deviceResourceUri.AbsoluteUri },
                };
            if (ipAddress != null)
                parameters.Add("ipAddress", ipAddress.ToString());

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.DELETE, template, parameters);

            Func<Task<HttpWebRequest>, Task<JObject>> requestResource =
                GetResponseAsyncFunc<JObject>(cancellationToken);

            Func<Task<JObject>, Task<DnsJob>> resultSelector =
                task =>
                {
                    JObject result = task.Result;
                    if (result == null)
                        return null;

                    DnsJob job = task.Result.ToObject<DnsJob>();
                    if (completionOption == AsyncCompletionOption.RequestCompleted)
                        return WaitForJobAsync(job, true, cancellationToken, progress);

                    return InternalTaskExtensions.CompletedTask(job);
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Then(resultSelector);
        }

        #endregion

        /// <summary>
        /// Waits for an asynchronous server job to complete.
        /// </summary>
        /// <param name="job">The <see cref="DnsJob"/> to wait for.</param>
        /// <param name="showDetails"><see langword="true"/> to include detailed information about the job; otherwise, <see langword="false"/>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <param name="progress">An optional callback object to receive progress notifications. If this is <see langword="null"/>, no progress notifications are sent.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. When the task completes successfully,
        /// the <see cref="Task{TResult}.Result"/> property will return a <see cref="DnsJob"/> object
        /// describing the asynchronous server operation. The job will additionally be in one of the following
        /// states.
        ///
        /// <list type="bullet">
        /// <item><see cref="DnsJobStatus.Completed"/></item>
        /// <item><see cref="DnsJobStatus.Error"/>: In this case the <see cref="DnsJob.Error"/> property provides
        /// additional information about the error which occurred during the asynchronous server operation.</item>
        /// </list>
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="job"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cdns/api/v1.0/cdns-devguide/content/sync_asynch_responses.html">Synchronous and Asynchronous Responses (Rackspace Cloud DNS Developer Guide - API v1.0)</seealso>
        protected Task<DnsJob> WaitForJobAsync(DnsJob job, bool showDetails, CancellationToken cancellationToken, IProgress<DnsJob> progress)
        {
            if (job == null)
                throw new ArgumentNullException("job");

            TaskCompletionSource<DnsJob> taskCompletionSource = new TaskCompletionSource<DnsJob>();
            Func<Task<DnsJob>> pollJob = () => PollJobStateAsync(job, showDetails, cancellationToken, progress);

            IEnumerator<TimeSpan> backoffPolicy = BackoffPolicy.GetBackoffIntervals().GetEnumerator();
            Func<Task<DnsJob>> moveNext =
                () =>
                {
                    if (!backoffPolicy.MoveNext())
                        throw new OperationCanceledException();

                    if (backoffPolicy.Current == TimeSpan.Zero)
                    {
                        return pollJob();
                    }
                    else
                    {
                        return Task.Factory.StartNewDelayed((int)backoffPolicy.Current.TotalMilliseconds, cancellationToken)
                            .Then(task => pollJob());
                    }
                };

            Task<DnsJob> currentTask = moveNext();
            Action<Task<DnsJob>> continuation = null;
            continuation =
                previousTask =>
                {
                    if (previousTask.Status != TaskStatus.RanToCompletion)
                    {
                        taskCompletionSource.SetFromTask(previousTask);
                        return;
                    }

                    DnsJob result = previousTask.Result;
                    if (result == null || result.Status == DnsJobStatus.Completed || result.Status == DnsJobStatus.Error)
                    {
                        // finished waiting
                        taskCompletionSource.SetResult(result);
                        return;
                    }

                    // reschedule
                    currentTask = moveNext();
                    // use ContinueWith since the continuation handles cancellation and faulted antecedent tasks
                    currentTask.ContinueWith(continuation, TaskContinuationOptions.ExecuteSynchronously);
                };
            // use ContinueWith since the continuation handles cancellation and faulted antecedent tasks
            currentTask.ContinueWith(continuation, TaskContinuationOptions.ExecuteSynchronously);

            return taskCompletionSource.Task;
        }

        /// <summary>
        /// Waits for an asynchronous server job with a strongly-typed result to complete.
        /// </summary>
        /// <typeparam name="TResult">The class modeling the JSON result of the asynchronous operation.</typeparam>
        /// <param name="job">The <see cref="DnsJob{TResponse}"/> to wait for.</param>
        /// <param name="showDetails"><see langword="true"/> to include detailed information about the job; otherwise, <see langword="false"/>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <param name="progress">An optional callback object to receive progress notifications. If this is <see langword="null"/>, no progress notifications are sent.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. When the task completes successfully,
        /// the <see cref="Task{TResult}.Result"/> property will return a <see cref="DnsJob{TResponse}"/> object
        /// describing the asynchronous server operation. The job will additionally be in one of the following
        /// states.
        ///
        /// <list type="bullet">
        /// <item><see cref="DnsJobStatus.Completed"/>: In this case the <see cref="DnsJob{TResponse}.Response"/>
        /// property provides the strongly-typed <typeparamref name="TResult"/> object which is the result of the
        /// operation.</item>
        /// <item><see cref="DnsJobStatus.Error"/>: In this case the <see cref="DnsJob.Error"/> property provides
        /// additional information about the error which occurred during the asynchronous server operation.</item>
        /// </list>
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="job"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cdns/api/v1.0/cdns-devguide/content/sync_asynch_responses.html">Synchronous and Asynchronous Responses (Rackspace Cloud DNS Developer Guide - API v1.0)</seealso>
        protected Task<DnsJob<TResult>> WaitForJobAsync<TResult>(DnsJob<TResult> job, bool showDetails, CancellationToken cancellationToken, IProgress<DnsJob<TResult>> progress)
        {
            if (job == null)
                throw new ArgumentNullException("job");

            TaskCompletionSource<DnsJob<TResult>> taskCompletionSource = new TaskCompletionSource<DnsJob<TResult>>();
            Func<Task<DnsJob<TResult>>> pollJob = () => PollJobStateAsync(job, showDetails, cancellationToken, progress);

            IEnumerator<TimeSpan> backoffPolicy = BackoffPolicy.GetBackoffIntervals().GetEnumerator();
            Func<Task<DnsJob<TResult>>> moveNext =
                () =>
                {
                    if (!backoffPolicy.MoveNext())
                        throw new OperationCanceledException();

                    if (backoffPolicy.Current == TimeSpan.Zero)
                    {
                        return pollJob();
                    }
                    else
                    {
                        return Task.Factory.StartNewDelayed((int)backoffPolicy.Current.TotalMilliseconds, cancellationToken)
                            .Then(task => pollJob());
                    }
                };

            Task<DnsJob<TResult>> currentTask = moveNext();
            Action<Task<DnsJob<TResult>>> continuation = null;
            continuation =
                previousTask =>
                {
                    if (previousTask.Status != TaskStatus.RanToCompletion)
                    {
                        taskCompletionSource.SetFromTask(previousTask);
                        return;
                    }

                    DnsJob<TResult> result = previousTask.Result;
                    if (result == null || result.Status == DnsJobStatus.Completed || result.Status == DnsJobStatus.Error)
                    {
                        // finished waiting
                        taskCompletionSource.SetResult(result);
                        return;
                    }

                    // reschedule
                    currentTask = moveNext();
                    // use ContinueWith since the continuation handles cancellation and faulted antecedent tasks
                    currentTask.ContinueWith(continuation, TaskContinuationOptions.ExecuteSynchronously);
                };
            // use ContinueWith since the continuation handles cancellation and faulted antecedent tasks
            currentTask.ContinueWith(continuation, TaskContinuationOptions.ExecuteSynchronously);

            return taskCompletionSource.Task;
        }

        /// <summary>
        /// Determines if the JSON representation of a single page of a paginated collection includes a "next" link.
        /// </summary>
        /// <param name="result">The JSON object representing a page of a paginated collection.</param>
        /// <returns><see langword="true"/> if the paginated collection result includes a "next" link; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="result"/> is <see langword="null"/>.</exception>
        protected virtual bool HasNextLink(JObject result)
        {
            if (result == null)
                throw new ArgumentNullException("result");

            JToken linksToken = result["links"];
            if (linksToken == null)
                return false;

            Link[] links = linksToken.ToObject<Link[]>();
            return links.Any(i => string.Equals(i.Rel, "next", StringComparison.OrdinalIgnoreCase));
        }

        /// <inheritdoc/>
        /// <remarks>
        /// This method returns a cached base address if one is available. If no cached address is
        /// available, <see cref="ProviderBase{TProvider}.GetServiceEndpoint"/> is called to obtain
        /// an <see cref="Endpoint"/> with the type <c>rax:dns</c> and preferred type <c>cloudDNS</c>.
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
                    Endpoint endpoint = GetServiceEndpoint(null, "rax:dns", "cloudDNS", null);
                    Uri baseUri = new Uri(_internalUrl ? endpoint.InternalURL : endpoint.PublicURL);
                    _baseUri = baseUri;
                    return baseUri;
                });
        }

        /// <summary>
        /// Asynchronously poll the current state of a DNS job.
        /// </summary>
        /// <param name="job">The job in the DNS service.</param>
        /// <param name="showDetails"><see langword="true"/> to include detailed information about the job; otherwise, <see langword="false"/>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <param name="progress">An optional callback object to receive progress notifications. If this is <see langword="null"/>, no progress notifications are sent.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. When
        /// the task completes successfully, the <see cref="Task{TResult}.Result"/>
        /// property will contain a <see cref="DnsJob"/> object containing the
        /// updated state information for the job in the DNS service.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="job"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        private Task<DnsJob> PollJobStateAsync(DnsJob job, bool showDetails, CancellationToken cancellationToken, IProgress<DnsJob> progress)
        {
            if (job == null)
                throw new ArgumentNullException("job");

            Task<DnsJob> chain = GetJobStatusAsync(job, showDetails, cancellationToken);
            chain = chain.Select(
                task =>
                {
                    if (task.Result == null || task.Result.Id != job.Id)
                        throw new InvalidOperationException("Could not obtain status for job");

                    return task.Result;
                });

            if (progress != null)
            {
                chain = chain.Select(
                    task =>
                    {
                        progress.Report(task.Result);
                        return task.Result;
                    });
            }

            return chain;
        }

        /// <summary>
        /// Asynchronously poll the current state of a DNS job.
        /// </summary>
        /// <param name="job">The job in the DNS service.</param>
        /// <param name="showDetails"><see langword="true"/> to include detailed information about the job; otherwise, <see langword="false"/>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <param name="progress">An optional callback object to receive progress notifications. If this is <see langword="null"/>, no progress notifications are sent.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. When
        /// the task completes successfully, the <see cref="Task{TResult}.Result"/>
        /// property will contain a <see cref="DnsJob"/> object containing the
        /// updated state information for the job in the DNS service.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="job"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        private Task<DnsJob<TResponse>> PollJobStateAsync<TResponse>(DnsJob<TResponse> job, bool showDetails, CancellationToken cancellationToken, IProgress<DnsJob<TResponse>> progress)
        {
            if (job == null)
                throw new ArgumentNullException("job");

            Task<DnsJob<TResponse>> chain = GetJobStatusAsync(job, showDetails, cancellationToken);
            chain = chain.Select(
                task =>
                {
                    if (task.Result == null || task.Result.Id != job.Id)
                        throw new InvalidOperationException("Could not obtain status for job");

                    return task.Result;
                });

            if (progress != null)
            {
                chain = chain.Select(
                    task =>
                    {
                        progress.Report(task.Result);
                        return task.Result;
                    });
            }

            return chain;
        }
    }
}
