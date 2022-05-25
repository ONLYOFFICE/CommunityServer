namespace net.openstack.Providers.Rackspace
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading.Tasks;
    using JSIStudios.SimpleRESTServices.Client;
    using net.openstack.Core;
    using net.openstack.Core.Collections;
    using net.openstack.Core.Domain;
    using net.openstack.Core.Providers;
    using net.openstack.Providers.Rackspace.Objects.LoadBalancers;
    using net.openstack.Providers.Rackspace.Objects.LoadBalancers.Request;
    using net.openstack.Providers.Rackspace.Objects.LoadBalancers.Response;
    using net.openstack.Providers.Rackspace.Validators;
    using Newtonsoft.Json.Linq;
    using CancellationToken = System.Threading.CancellationToken;
    using IHttpResponseCodeValidator = net.openstack.Core.Validators.IHttpResponseCodeValidator;
    using JsonRestServices = JSIStudios.SimpleRESTServices.Client.Json.JsonRestServices;

    /// <summary>
    /// Provides an implementation of <see cref="ILoadBalancerService"/> for operating
    /// with Rackspace's Cloud Load Balancers product.
    /// </summary>
    /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Overview-d1e82.html">Rackspace Cloud Load Balancers Developer Guide - API v1.0</seealso>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    public class CloudLoadBalancerProvider : ProviderBase<ILoadBalancerService>, ILoadBalancerService
    {
        /// <summary>
        /// This field caches the base URI used for accessing the Cloud Load Balancers service.
        /// </summary>
        /// <seealso cref="GetBaseUriAsync"/>
        private Uri _baseUri;

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudLoadBalancerProvider"/> class with
        /// the specified values.
        /// </summary>
        /// <param name="defaultIdentity">The default identity to use for calls that do not explicitly specify an identity. If this value is <see langword="null"/>, no default identity is available so all calls must specify an explicit identity.</param>
        /// <param name="defaultRegion">The default region to use for calls that do not explicitly specify a region. If this value is <see langword="null"/>, the default region for the user will be used; otherwise if the service uses region-specific endpoints all calls must specify an explicit region.</param>
        /// <param name="identityProvider">The identity provider to use for authenticating requests to this provider. If this value is <see langword="null"/>, a new instance of <see cref="CloudIdentityProvider"/> is created using <paramref name="defaultIdentity"/> as the default identity.</param>
        public CloudLoadBalancerProvider(CloudIdentity defaultIdentity, string defaultRegion, IIdentityProvider identityProvider)
            : base(defaultIdentity, defaultRegion, identityProvider, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudLoadBalancerProvider"/> class with
        /// the specified values.
        /// </summary>
        /// <param name="defaultIdentity">The default identity to use for calls that do not explicitly specify an identity. If this value is <see langword="null"/>, no default identity is available so all calls must specify an explicit identity.</param>
        /// <param name="defaultRegion">The default region to use for calls that do not explicitly specify a region. If this value is <see langword="null"/>, the default region for the user will be used; otherwise if the service uses region-specific endpoints all calls must specify an explicit region.</param>
        /// <param name="identityProvider">The identity provider to use for authenticating requests to this provider. If this value is <see langword="null"/>, a new instance of <see cref="CloudIdentityProvider"/> is created using <paramref name="defaultIdentity"/> as the default identity.</param>
        /// <param name="restService">The implementation of <see cref="IRestService"/> to use for executing synchronous REST requests. If this value is <see langword="null"/>, the provider will use a new instance of <see cref="JsonRestServices"/>.</param>
        /// <param name="httpStatusCodeValidator">The HTTP status code validator to use for synchronous REST requests. If this value is <see langword="null"/>, the provider will use <see cref="HttpResponseCodeValidator.Default"/>.</param>
        protected CloudLoadBalancerProvider(CloudIdentity defaultIdentity, string defaultRegion, IIdentityProvider identityProvider, IRestService restService, IHttpResponseCodeValidator httpStatusCodeValidator)
            : base(defaultIdentity, defaultRegion, identityProvider, restService, httpStatusCodeValidator)
        {
        }

        #region ILoadBalancerService Members

        /// <inheritdoc/>
        public Task<ReadOnlyCollectionPage<LoadBalancer>> ListLoadBalancersAsync(LoadBalancerId markerId, int? limit, CancellationToken cancellationToken)
        {
            if (limit < 0)
                throw new ArgumentOutOfRangeException("limit");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/loadbalancers?marker={markerId}&limit={limit}");
            var parameters = new Dictionary<string, string>();
            if (markerId != null)
                parameters.Add("markerId", markerId.Value);
            if (limit != null)
            {
                if (markerId != null)
                {
                    // the server includes the item with the ID "markerId" in the result.
                    parameters.Add("limit", (limit + 1).ToString());
                }
                else
                {
                    parameters.Add("limit", limit.ToString());
                }
            }
            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<ListLoadBalancersResponse>> requestResource =
                GetResponseAsyncFunc<ListLoadBalancersResponse>(cancellationToken);

            Func<Task<ListLoadBalancersResponse>, ReadOnlyCollectionPage<LoadBalancer>> resultSelector =
                task =>
                {
                    if (task.Result == null || task.Result.LoadBalancers == null)
                        return ReadOnlyCollectionPage<LoadBalancer>.Empty;

                    ReadOnlyCollection<LoadBalancer> result = task.Result.LoadBalancers;
                    if (markerId != null && result.Count > 0)
                    {
                        if (result[0].Id != markerId)
                            throw new InvalidOperationException("Expected the pagination result to include the marked load balancer.");

                        // remove the marker so pagination behaves normally
                        result = new List<LoadBalancer>(result.Skip(1)).AsReadOnly();
                    }

                    if (!result.Any())
                        return ReadOnlyCollectionPage<LoadBalancer>.Empty;

                    LoadBalancerId nextMarker = result[result.Count - 1].Id;
                    Func<CancellationToken, Task<ReadOnlyCollectionPage<LoadBalancer>>> getNextPageAsync =
                        nextCancellationToken => ListLoadBalancersAsync(nextMarker, limit, nextCancellationToken);
                    return new BasicReadOnlyCollectionPage<LoadBalancer>(result, getNextPageAsync);
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task<LoadBalancer> GetLoadBalancerAsync(LoadBalancerId loadBalancerId, CancellationToken cancellationToken)
        {
            if (loadBalancerId == null)
                throw new ArgumentNullException("loadBalancerId");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/loadbalancers/{loadBalancerId}");
            var parameters = new Dictionary<string, string> { { "loadBalancerId", loadBalancerId.Value } };
            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<GetLoadBalancerResponse>> requestResource =
                GetResponseAsyncFunc<GetLoadBalancerResponse>(cancellationToken);

            Func<Task<GetLoadBalancerResponse>, LoadBalancer> resultSelector =
                task => task.Result.LoadBalancer;

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task<LoadBalancer> CreateLoadBalancerAsync(LoadBalancerConfiguration configuration, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<LoadBalancer> progress)
        {
            if (configuration == null)
                throw new ArgumentNullException("configuration");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/loadbalancers");
            var parameters = new Dictionary<string, string>();

            CreateLoadBalancerRequest requestBody = new CreateLoadBalancerRequest(configuration);
            Func<Task<Tuple<IdentityToken, Uri>>, Task<HttpWebRequest>> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.POST, template, parameters, requestBody);

            Func<Task<HttpWebRequest>, Task<GetLoadBalancerResponse>> requestResource =
                GetResponseAsyncFunc<GetLoadBalancerResponse>(cancellationToken);

            Func<Task<GetLoadBalancerResponse>, Task<LoadBalancer>> resultSelector =
                task =>
                {
                    LoadBalancer loadBalancer = task.Result.LoadBalancer;
                    if (loadBalancer != null && completionOption == AsyncCompletionOption.RequestCompleted)
                        return WaitForLoadBalancerToLeaveStateAsync(loadBalancer.Id, LoadBalancerStatus.Build, cancellationToken, progress);

                    return InternalTaskExtensions.CompletedTask(loadBalancer);
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Then(prepareRequest)
                .Then(requestResource)
                .Then(resultSelector);
        }

        /// <inheritdoc/>
        public Task UpdateLoadBalancerAsync(LoadBalancerId loadBalancerId, LoadBalancerUpdate configuration, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<LoadBalancer> progress)
        {
            if (loadBalancerId == null)
                throw new ArgumentNullException("loadBalancerId");
            if (configuration == null)
                throw new ArgumentNullException("configuration");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/loadbalancers/{loadBalancerId}");
            var parameters = new Dictionary<string, string> { { "loadBalancerId", loadBalancerId.Value } };

            UpdateLoadBalancerRequest requestBody = new UpdateLoadBalancerRequest(configuration);
            Func<Task<Tuple<IdentityToken, Uri>>, Task<HttpWebRequest>> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.PUT, template, parameters, requestBody);

            Func<Task<HttpWebRequest>, Task<string>> requestResource =
                GetResponseAsyncFunc<string>(cancellationToken);

            Func<Task<string>, Task<LoadBalancer>> resultSelector =
                task =>
                {
                    if (completionOption == AsyncCompletionOption.RequestCompleted)
                        return WaitForLoadBalancerToLeaveStateAsync(loadBalancerId, LoadBalancerStatus.PendingUpdate, cancellationToken, progress);

                    return InternalTaskExtensions.CompletedTask(default(LoadBalancer));
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Then(prepareRequest)
                .Then(requestResource)
                .Then(resultSelector);
        }

        /// <inheritdoc/>
        public Task RemoveLoadBalancerAsync(LoadBalancerId loadBalancerId, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<LoadBalancer> progress)
        {
            if (loadBalancerId == null)
                throw new ArgumentNullException("loadBalancerId");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/loadbalancers/{loadBalancerId}");
            var parameters = new Dictionary<string, string> { { "loadBalancerId", loadBalancerId.Value } };
            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.DELETE, template, parameters);

            Func<Task<HttpWebRequest>, Task<string>> requestResource =
                GetResponseAsyncFunc(cancellationToken);

            Func<Task<string>, Task<LoadBalancer>> resultSelector =
                task =>
                {
                    if (completionOption == AsyncCompletionOption.RequestCompleted)
                        return WaitForLoadBalancerToLeaveStateAsync(loadBalancerId, LoadBalancerStatus.PendingDelete, cancellationToken, progress);

                    return InternalTaskExtensions.CompletedTask(default(LoadBalancer));
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Then(resultSelector);
        }

        /// <inheritdoc/>
        public Task RemoveLoadBalancerRangeAsync(IEnumerable<LoadBalancerId> loadBalancerIds, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<LoadBalancer[]> progress)
        {
            if (loadBalancerIds == null)
                throw new ArgumentNullException("loadBalancerIds");

            return RemoveLoadBalancerRangeAsync(loadBalancerIds.ToArray(), completionOption, cancellationToken, progress);
        }

        /// <summary>
        /// Removes one or more load balancers.
        /// </summary>
        /// <param name="loadBalancerIds">The IDs of load balancers to remove. These is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="completionOption">Specifies when the <see cref="Task"/> representing the asynchronous server operation should be considered complete.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <param name="progress">An optional callback object to receive progress notifications, if <paramref name="completionOption"/> is <see cref="AsyncCompletionOption.RequestCompleted"/>. If this is <see langword="null"/>, no progress notifications are sent.</param>
        /// <returns>A <see cref="Task"/> object representing the asynchronous operation.</returns>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. If
        /// <paramref name="completionOption"/> is <see cref="AsyncCompletionOption.RequestCompleted"/>,
        /// the task will not be considered complete until all of the load balancers
        /// transition out of the <see cref="LoadBalancerStatus.PendingDelete"/> state.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="loadBalancerIds"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="loadBalancerIds"/> contains any <see langword="null"/> values.
        /// <para>-or-</para>
        /// <para>If <paramref name="completionOption"/> is not a valid <see cref="AsyncCompletionOption"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Remove_Load_Balancer-d1e2093.html">Remove Load Balancer (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        public Task RemoveLoadBalancerRangeAsync(LoadBalancerId[] loadBalancerIds, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<LoadBalancer[]> progress)
        {
            if (loadBalancerIds == null)
                throw new ArgumentNullException("loadBalancerIds");
            if (loadBalancerIds.Contains(null))
                throw new ArgumentException("loadBalancerIds cannot contain any null values", "loadBalancerIds");

            if (loadBalancerIds.Length == 0)
            {
                return InternalTaskExtensions.CompletedTask();
            }
            else if (loadBalancerIds.Length == 1)
            {
                IProgress<LoadBalancer> wrapper = null;
                if (progress != null)
                    wrapper = new ArrayElementProgressWrapper<LoadBalancer>(progress);

                return RemoveLoadBalancerAsync(loadBalancerIds[0], completionOption, cancellationToken, wrapper);
            }
            else
            {
                UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/loadbalancers?id={id}");
                var parameters = new Dictionary<string, string> { { "id", string.Join(",", Array.ConvertAll(loadBalancerIds, i => i.Value)) } };

                Func<Uri, Uri> uriTransform =
                    uri =>
                    {
                        string path = uri.GetLeftPart(UriPartial.Path);
                        string query = uri.Query.Replace(",", "&id=").Replace("%2c", "&id=");
                        return new Uri(path + query);
                    };

                Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                    PrepareRequestAsyncFunc(HttpMethod.DELETE, template, parameters, uriTransform);

                Func<Task<HttpWebRequest>, Task<string>> requestResource =
                    GetResponseAsyncFunc(cancellationToken);

                Func<Task<string>, Task<LoadBalancer[]>> resultSelector =
                    task =>
                    {
                        if (completionOption == AsyncCompletionOption.RequestCompleted)
                            return WaitForLoadBalancersToLeaveStateAsync(loadBalancerIds, LoadBalancerStatus.PendingDelete, cancellationToken, progress);

                        return InternalTaskExtensions.CompletedTask(default(LoadBalancer[]));
                    };

                return AuthenticateServiceAsync(cancellationToken)
                    .Select(prepareRequest)
                    .Then(requestResource)
                    .Then(resultSelector);
            }
        }

        /// <inheritdoc/>
        public Task<string> GetErrorPageAsync(LoadBalancerId loadBalancerId, CancellationToken cancellationToken)
        {
            if (loadBalancerId == null)
                throw new ArgumentNullException("loadBalancerId");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/loadbalancers/{loadBalancerId}/errorpage");
            var parameters = new Dictionary<string, string>()
            {
                { "loadBalancerId", loadBalancerId.Value }
            };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<GetLoadBalancerErrorPageResponse>> requestResource =
                GetResponseAsyncFunc<GetLoadBalancerErrorPageResponse>(cancellationToken);

            Func<Task<GetLoadBalancerErrorPageResponse>, string> resultSelector =
                task => task.Result != null ? task.Result.Content : null;

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task SetErrorPageAsync(LoadBalancerId loadBalancerId, string content, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<LoadBalancer> progress)
        {
            if (loadBalancerId == null)
                throw new ArgumentNullException("loadBalancerId");
            if (content == null)
                throw new ArgumentNullException("content");
            if (string.IsNullOrEmpty(content))
                throw new ArgumentException("content cannot be empty");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/loadbalancers/{loadBalancerId}/errorpage");
            var parameters = new Dictionary<string, string>()
            {
                { "loadBalancerId", loadBalancerId.Value }
            };

            SetLoadBalancerErrorPageRequest requestBody = new SetLoadBalancerErrorPageRequest(content);
            Func<Task<Tuple<IdentityToken, Uri>>, Task<HttpWebRequest>> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.PUT, template, parameters, requestBody);

            Func<Task<HttpWebRequest>, Task<string>> requestResource =
                GetResponseAsyncFunc(cancellationToken);

            Func<Task<string>, Task<LoadBalancer>> resultSelector =
                task =>
                {
                    if (completionOption == AsyncCompletionOption.RequestCompleted)
                        return WaitForLoadBalancerToLeaveStateAsync(loadBalancerId, LoadBalancerStatus.PendingUpdate, cancellationToken, progress);

                    return InternalTaskExtensions.CompletedTask(default(LoadBalancer));
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Then(prepareRequest)
                .Then(requestResource)
                .Then(resultSelector);
        }

        /// <inheritdoc/>
        public Task RemoveErrorPageAsync(LoadBalancerId loadBalancerId, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<LoadBalancer> progress)
        {
            if (loadBalancerId == null)
                throw new ArgumentNullException("loadBalancerId");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/loadbalancers/{loadBalancerId}/errorpage");
            var parameters = new Dictionary<string, string>()
            {
                { "loadBalancerId", loadBalancerId.Value }
            };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.DELETE, template, parameters);

            Func<Task<HttpWebRequest>, Task<string>> requestResource =
                GetResponseAsyncFunc(cancellationToken);

            Func<Task<string>, Task<LoadBalancer>> resultSelector =
                task =>
                {
                    if (completionOption == AsyncCompletionOption.RequestCompleted)
                        return WaitForLoadBalancerToLeaveStateAsync(loadBalancerId, LoadBalancerStatus.PendingUpdate, cancellationToken, progress);

                    return InternalTaskExtensions.CompletedTask(default(LoadBalancer));
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Then(resultSelector);
        }

        /// <inheritdoc/>
        public Task<LoadBalancerStatistics> GetStatisticsAsync(LoadBalancerId loadBalancerId, CancellationToken cancellationToken)
        {
            if (loadBalancerId == null)
                throw new ArgumentNullException("loadBalancerId");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/loadbalancers/{loadBalancerId}/stats");
            var parameters = new Dictionary<string, string>()
            {
                { "loadBalancerId", loadBalancerId.Value }
            };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<LoadBalancerStatistics>> requestResource =
                GetResponseAsyncFunc<LoadBalancerStatistics>(cancellationToken);

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource);
        }

        /// <inheritdoc/>
        public Task<ReadOnlyCollection<Node>> ListNodesAsync(LoadBalancerId loadBalancerId, CancellationToken cancellationToken)
        {
            if (loadBalancerId == null)
                throw new ArgumentNullException("loadBalancerId");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/loadbalancers/{loadBalancerId}/nodes");
            var parameters = new Dictionary<string, string> { { "loadBalancerId", loadBalancerId.Value } };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<ListLoadBalancerNodesResponse>> requestResource =
                GetResponseAsyncFunc<ListLoadBalancerNodesResponse>(cancellationToken);

            Func<Task<ListLoadBalancerNodesResponse>, ReadOnlyCollection<Node>> resultSelector =
                task => (task.Result != null ? task.Result.Nodes : null) ?? new ReadOnlyCollection<Node>(new Node[0]);

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task<Node> GetNodeAsync(LoadBalancerId loadBalancerId, NodeId nodeId, CancellationToken cancellationToken)
        {
            if (loadBalancerId == null)
                throw new ArgumentNullException("loadBalancerId");
            if (nodeId == null)
                throw new ArgumentNullException("nodeId");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/loadbalancers/{loadBalancerId}/nodes/{nodeId}");
            var parameters = new Dictionary<string, string> { { "loadBalancerId", loadBalancerId.Value }, { "nodeId", nodeId.Value } };
            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<GetLoadBalancerNodeResponse>> requestResource =
                GetResponseAsyncFunc<GetLoadBalancerNodeResponse>(cancellationToken);

            Func<Task<GetLoadBalancerNodeResponse>, Node> resultSelector =
                task => task.Result.Node;

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task<Node> AddNodeAsync(LoadBalancerId loadBalancerId, NodeConfiguration nodeConfiguration, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<LoadBalancer> progress)
        {
            if (nodeConfiguration == null)
                throw new ArgumentNullException("nodeConfiguration");

            Func<Task<ReadOnlyCollection<Node>>, Node> resultSelector =
                task => task.Result.Single();

            return
                AddNodeRangeAsync(loadBalancerId, new[] { nodeConfiguration }, completionOption, cancellationToken, progress)
                    .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task<ReadOnlyCollection<Node>> AddNodeRangeAsync(LoadBalancerId loadBalancerId, IEnumerable<NodeConfiguration> nodeConfigurations, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<LoadBalancer> progress)
        {
            if (nodeConfigurations == null)
                throw new ArgumentNullException("nodeConfigurations");

            return AddNodeRangeAsync(loadBalancerId, nodeConfigurations.ToArray(), completionOption, cancellationToken, progress);
        }

        /// <summary>
        /// Add one or more nodes to a load balancer.
        /// </summary>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="nodeConfigurations">A collection of <see cref="NodeConfiguration"/> objects describing the load balancer nodes to add.</param>
        /// <param name="completionOption">Specifies when the <see cref="Task"/> representing the asynchronous server operation should be considered complete.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <param name="progress">An optional callback object to receive progress notifications, if <paramref name="completionOption"/> is <see cref="AsyncCompletionOption.RequestCompleted"/>. If this is <see langword="null"/>, no progress notifications are sent.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. When the operation
        /// completes, the <see cref="Task{TResult}.Result"/> property will contain a collection of
        /// <see cref="Node"/> objects describing the new load balancer nodes.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="loadBalancerId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="nodeConfigurations"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="nodeConfigurations"/> contains any <see langword="null"/> values.
        /// <para>-or-</para>
        /// <para>If <paramref name="completionOption"/> is not a valid <see cref="AsyncCompletionOption"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Add_Nodes-d1e2379.html">Add Nodes (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        public Task<ReadOnlyCollection<Node>> AddNodeRangeAsync(LoadBalancerId loadBalancerId, NodeConfiguration[] nodeConfigurations, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<LoadBalancer> progress)
        {
            if (loadBalancerId == null)
                throw new ArgumentNullException("loadBalancerId");
            if (nodeConfigurations == null)
                throw new ArgumentNullException("nodeConfigurations");
            if (nodeConfigurations.Contains(null))
                throw new ArgumentException("nodeConfigurations cannot contain any null values", "nodeConfigurations");

            if (nodeConfigurations.Length == 0)
            {
                return InternalTaskExtensions.CompletedTask(new ReadOnlyCollection<Node>(new Node[0]));
            }
            else
            {
                UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/loadbalancers/{loadBalancerId}/nodes");
                var parameters = new Dictionary<string, string>()
                {
                    { "loadBalancerId", loadBalancerId.Value },
                };

                AddNodesRequest requestBody = new AddNodesRequest(nodeConfigurations);
                Func<Task<Tuple<IdentityToken, Uri>>, Task<HttpWebRequest>> prepareRequest =
                    PrepareRequestAsyncFunc(HttpMethod.POST, template, parameters, requestBody);

                Func<Task<HttpWebRequest>, Task<ListLoadBalancerNodesResponse>> requestResource =
                    GetResponseAsyncFunc<ListLoadBalancerNodesResponse>(cancellationToken);

                Func<Task<ListLoadBalancerNodesResponse>, Task<ReadOnlyCollection<Node>>> resultSelector =
                    task =>
                    {
                        if (completionOption == AsyncCompletionOption.RequestCompleted)
                        {
                            return WaitForLoadBalancerToLeaveStateAsync(loadBalancerId, LoadBalancerStatus.PendingUpdate, cancellationToken, progress)
                                .Select(t => task.Result.Nodes);
                        }

                        return InternalTaskExtensions.CompletedTask(task.Result.Nodes);
                    };

                return AuthenticateServiceAsync(cancellationToken)
                    .Then(prepareRequest)
                    .Then(requestResource)
                    .Then(resultSelector);
            }
        }

        /// <inheritdoc/>
        public Task UpdateNodeAsync(LoadBalancerId loadBalancerId, NodeId nodeId, NodeUpdate configuration, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<LoadBalancer> progress)
        {
            if (loadBalancerId == null)
                throw new ArgumentNullException("loadBalancerId");
            if (nodeId == null)
                throw new ArgumentNullException("nodeId");
            if (configuration == null)
                throw new ArgumentNullException("configuration");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/loadbalancers/{loadBalancerId}/nodes/{nodeId}");
            var parameters = new Dictionary<string, string> { { "loadBalancerId", loadBalancerId.Value }, { "nodeId", nodeId.Value } };

            UpdateLoadBalancerNodeRequest requestBody = new UpdateLoadBalancerNodeRequest(configuration);
            Func<Task<Tuple<IdentityToken, Uri>>, Task<HttpWebRequest>> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.PUT, template, parameters, requestBody);

            Func<Task<HttpWebRequest>, Task<string>> requestResource =
                GetResponseAsyncFunc<string>(cancellationToken);

            Func<Task<string>, Task<LoadBalancer>> resultSelector =
                task =>
                {
                    if (completionOption == AsyncCompletionOption.RequestCompleted)
                        return WaitForLoadBalancerToLeaveStateAsync(loadBalancerId, LoadBalancerStatus.PendingUpdate, cancellationToken, progress);

                    return InternalTaskExtensions.CompletedTask(default(LoadBalancer));
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Then(prepareRequest)
                .Then(requestResource)
                .Then(resultSelector);
        }

        /// <inheritdoc/>
        public Task RemoveNodeAsync(LoadBalancerId loadBalancerId, NodeId nodeId, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<LoadBalancer> progress)
        {
            if (loadBalancerId == null)
                throw new ArgumentNullException("loadBalancerId");
            if (nodeId == null)
                throw new ArgumentNullException("nodeId");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/loadbalancers/{loadBalancerId}/nodes/{nodeId}");
            var parameters = new Dictionary<string, string> { { "loadBalancerId", loadBalancerId.Value }, { "nodeId", nodeId.Value } };
            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.DELETE, template, parameters);

            Func<Task<HttpWebRequest>, Task<string>> requestResource =
                GetResponseAsyncFunc(cancellationToken);

            Func<Task<string>, Task<LoadBalancer>> resultSelector =
                task =>
                {
                    if (completionOption == AsyncCompletionOption.RequestCompleted)
                        return WaitForLoadBalancerToLeaveStateAsync(loadBalancerId, LoadBalancerStatus.PendingUpdate, cancellationToken, progress);

                    return InternalTaskExtensions.CompletedTask(default(LoadBalancer));
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Then(resultSelector);
        }

        /// <inheritdoc/>
        public Task RemoveNodeRangeAsync(LoadBalancerId loadBalancerId, IEnumerable<NodeId> nodeIds, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<LoadBalancer> progress)
        {
            if (nodeIds == null)
                throw new ArgumentNullException("nodeIds");

            return RemoveNodeRangeAsync(loadBalancerId, nodeIds.ToArray(), completionOption, cancellationToken, progress);
        }

        /// <summary>
        /// Remove one or more nodes from a load balancer.
        /// </summary>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="nodeIds">The load balancer node IDs of nodes to remove. These are obtained from <see cref="Node.Id">Node.Id</see>.</param>
        /// <param name="completionOption">Specifies when the <see cref="Task"/> representing the asynchronous server operation should be considered complete.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <param name="progress">An optional callback object to receive progress notifications, if <paramref name="completionOption"/> is <see cref="AsyncCompletionOption.RequestCompleted"/>. If this is <see langword="null"/>, no progress notifications are sent.</param>
        /// <returns>A <see cref="Task"/> object representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="loadBalancerId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="nodeIds"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="nodeIds"/> contains any <see langword="null"/> values.
        /// <para>-or-</para>
        /// <para>If <paramref name="completionOption"/> is not a valid <see cref="AsyncCompletionOption"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Remove_Nodes-d1e2675.html">Remove Nodes (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        public Task RemoveNodeRangeAsync(LoadBalancerId loadBalancerId, NodeId[] nodeIds, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<LoadBalancer> progress)
        {
            if (loadBalancerId == null)
                throw new ArgumentNullException("loadBalancerId");
            if (nodeIds == null)
                throw new ArgumentNullException("nodeIds");
            if (nodeIds.Contains(null))
                throw new ArgumentException("nodeIds cannot contain any null values", "nodeIds");

            if (nodeIds.Length == 0)
            {
                return InternalTaskExtensions.CompletedTask();
            }
            else if (nodeIds.Length == 1)
            {
                return RemoveNodeAsync(loadBalancerId, nodeIds[0], completionOption, cancellationToken, progress);
            }
            else
            {
                UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/loadbalancers/{loadBalancerId}/nodes?id={id}");
                var parameters = new Dictionary<string, string> { { "loadBalancerId", loadBalancerId.Value }, { "id", string.Join(",", Array.ConvertAll(nodeIds, i => i.Value)) } };

                Func<Uri, Uri> uriTransform =
                    uri =>
                    {
                        string path = uri.GetLeftPart(UriPartial.Path);
                        string query = uri.Query.Replace(",", "&id=").Replace("%2c", "&id=");
                        return new Uri(path + query);
                    };

                Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                    PrepareRequestAsyncFunc(HttpMethod.DELETE, template, parameters, uriTransform);

                Func<Task<HttpWebRequest>, Task<string>> requestResource =
                    GetResponseAsyncFunc(cancellationToken);

                Func<Task<string>, Task<LoadBalancer>> resultSelector =
                    task =>
                    {
                        if (completionOption == AsyncCompletionOption.RequestCompleted)
                            return WaitForLoadBalancerToLeaveStateAsync(loadBalancerId, LoadBalancerStatus.PendingUpdate, cancellationToken, progress);

                        return InternalTaskExtensions.CompletedTask(default(LoadBalancer));
                    };

                return AuthenticateServiceAsync(cancellationToken)
                    .Select(prepareRequest)
                    .Then(requestResource)
                    .Then(resultSelector);
            }
        }

        /// <inheritdoc/>
        public Task<ReadOnlyCollectionPage<NodeServiceEvent>> ListNodeServiceEventsAsync(LoadBalancerId loadBalancerId, NodeServiceEventId markerId, int? limit, CancellationToken cancellationToken)
        {
            if (loadBalancerId == null)
                throw new ArgumentNullException("loadBalancerId");
            if (limit <= 0)
                throw new ArgumentOutOfRangeException("limit");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/loadbalancers/{loadBalancerId}/nodes/events?marker={markerId}&limit={limit}");
            var parameters = new Dictionary<string, string> { { "loadBalancerId", loadBalancerId.Value } };
            if (markerId != null)
                parameters.Add("markerId", markerId.Value);
            if (limit != null)
            {
                if (markerId != null)
                {
                    // the server includes the item with the ID "markerId" in the result.
                    parameters.Add("limit", (limit + 1).ToString());
                }
                else
                {
                    parameters.Add("limit", limit.ToString());
                }
            }

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<ListNodeServiceEventsResponse>> requestResource =
                GetResponseAsyncFunc<ListNodeServiceEventsResponse>(cancellationToken);

            Func<Task<ListNodeServiceEventsResponse>, ReadOnlyCollectionPage<NodeServiceEvent>> resultSelector =
                task =>
                {
                    if (task.Result == null || task.Result.NodeServiceEvents == null)
                        return ReadOnlyCollectionPage<NodeServiceEvent>.Empty;

                    ReadOnlyCollection<NodeServiceEvent> result = task.Result.NodeServiceEvents;
                    if (markerId != null && result.Count > 0)
                    {
                        if (result[0].Id != markerId)
                            throw new InvalidOperationException("Expected the pagination result to include the marked node service event.");

                        // remove the marker so pagination behaves normally
                        result = new List<NodeServiceEvent>(result.Skip(1)).AsReadOnly();
                    }

                    if (!result.Any())
                        return ReadOnlyCollectionPage<NodeServiceEvent>.Empty;

                    NodeServiceEventId nextMarker = result[result.Count - 1].Id;
                    Func<CancellationToken, Task<ReadOnlyCollectionPage<NodeServiceEvent>>> getNextPageAsync =
                        nextCancellationToken => ListNodeServiceEventsAsync(loadBalancerId, nextMarker, limit, nextCancellationToken);
                    return new BasicReadOnlyCollectionPage<NodeServiceEvent>(result, getNextPageAsync);
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task<ReadOnlyCollection<LoadBalancerVirtualAddress>> ListVirtualAddressesAsync(LoadBalancerId loadBalancerId, CancellationToken cancellationToken)
        {
            if (loadBalancerId == null)
                throw new ArgumentNullException("loadBalancerId");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/loadbalancers/{loadBalancerId}/virtualips");
            var parameters = new Dictionary<string, string> { { "loadBalancerId", loadBalancerId.Value } };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<ListVirtualAddressesResponse>> requestResource =
                GetResponseAsyncFunc<ListVirtualAddressesResponse>(cancellationToken);

            Func<Task<ListVirtualAddressesResponse>, ReadOnlyCollection<LoadBalancerVirtualAddress>> resultSelector =
                task => (task.Result != null ? task.Result.VirtualAddresses : null) ?? new ReadOnlyCollection<LoadBalancerVirtualAddress>(new LoadBalancerVirtualAddress[0]);

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task<LoadBalancerVirtualAddress> AddVirtualAddressAsync(LoadBalancerId loadBalancerId, LoadBalancerVirtualAddressType type, AddressFamily addressFamily, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<LoadBalancer> progress)
        {
            if (loadBalancerId == null)
                throw new ArgumentNullException("loadBalancerId");
            if (type == null)
                throw new ArgumentNullException("type");
            if (addressFamily != AddressFamily.InterNetwork && addressFamily != AddressFamily.InterNetworkV6)
                throw new ArgumentException("Unsupported address family.", "addressFamily");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/loadbalancers/{loadBalancerId}/virtualips");
            var parameters = new Dictionary<string, string>()
            {
                { "loadBalancerId", loadBalancerId.Value },
            };

            LoadBalancerVirtualAddress requestBody = new LoadBalancerVirtualAddress(type, addressFamily);
            Func<Task<Tuple<IdentityToken, Uri>>, Task<HttpWebRequest>> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.POST, template, parameters, requestBody);

            Func<Task<HttpWebRequest>, Task<LoadBalancerVirtualAddress>> requestResource =
                GetResponseAsyncFunc<LoadBalancerVirtualAddress>(cancellationToken);

            Func<Task<LoadBalancerVirtualAddress>, Task<LoadBalancerVirtualAddress>> resultSelector =
                task =>
                {
                    if (completionOption == AsyncCompletionOption.RequestCompleted)
                    {
                        return WaitForLoadBalancerToLeaveStateAsync(loadBalancerId, LoadBalancerStatus.PendingUpdate, cancellationToken, progress)
                            .Select(t => task.Result);
                    }

                    return InternalTaskExtensions.CompletedTask(task.Result);
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Then(prepareRequest)
                .Then(requestResource)
                .Then(resultSelector);
        }

        /// <inheritdoc/>
        public Task RemoveVirtualAddressAsync(LoadBalancerId loadBalancerId, VirtualAddressId virtualAddressId, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<LoadBalancer> progress)
        {
            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/loadbalancers/{loadBalancerId}/virtualips/{virtualipId}");
            var parameters = new Dictionary<string, string>()
                {
                    { "loadBalancerId", loadBalancerId.Value },
                    { "virtualipId", virtualAddressId.Value }
                };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.DELETE, template, parameters);

            Func<Task<HttpWebRequest>, Task<string>> requestResource =
                GetResponseAsyncFunc(cancellationToken);

            Func<Task<string>, Task<LoadBalancer>> resultSelector =
                task =>
                {
                    if (completionOption == AsyncCompletionOption.RequestCompleted)
                        return WaitForLoadBalancerToLeaveStateAsync(loadBalancerId, LoadBalancerStatus.PendingUpdate, cancellationToken, progress);

                    return InternalTaskExtensions.CompletedTask(default(LoadBalancer));
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Then(resultSelector);
        }

        /// <inheritdoc/>
        public Task RemoveVirtualAddressRangeAsync(LoadBalancerId loadBalancerId, IEnumerable<VirtualAddressId> virtualAddressIds, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<LoadBalancer> progress)
        {
            if (virtualAddressIds == null)
                throw new ArgumentNullException("virtualAddressIds");

            return RemoveVirtualAddressRangeAsync(loadBalancerId, virtualAddressIds.ToArray(), completionOption, cancellationToken, progress);
        }

        /// <summary>
        /// Remove a collection of virtual addresses associated with a load balancer.
        /// </summary>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="virtualAddressIds">The virtual address IDs. These are obtained from <see cref="LoadBalancerVirtualAddress.Id">LoadBalancerVirtualAddress.Id</see>.</param>
        /// <param name="completionOption">Specifies when the <see cref="Task"/> representing the asynchronous server operation should be considered complete.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <param name="progress">An optional callback object to receive progress notifications, if <paramref name="completionOption"/> is <see cref="AsyncCompletionOption.RequestCompleted"/>. If this is <see langword="null"/>, no progress notifications are sent.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. If <paramref name="completionOption"/> is
        /// <see cref="AsyncCompletionOption.RequestCompleted"/>, the task will not be considered complete until
        /// the load balancer transitions out of the <see cref="LoadBalancerStatus.PendingUpdate"/> state.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="loadBalancerId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="virtualAddressIds"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="virtualAddressIds"/> contains any <see langword="null"/> values.
        /// <para>-or-</para>
        /// <para>If <paramref name="completionOption"/> is not a valid <see cref="AsyncCompletionOption"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Remove_Virtual_IP-d1e2919.html">Remove Virtual IP (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        public Task RemoveVirtualAddressRangeAsync(LoadBalancerId loadBalancerId, VirtualAddressId[] virtualAddressIds, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<LoadBalancer> progress)
        {
            if (loadBalancerId == null)
                throw new ArgumentNullException("loadBalancerId");
            if (virtualAddressIds == null)
                throw new ArgumentNullException("metadataIds");
            if (virtualAddressIds.Contains(null))
                throw new ArgumentException("virtualAddressIds cannot contain any null values", "virtualAddressIds");

            if (virtualAddressIds.Length == 0)
            {
                return InternalTaskExtensions.CompletedTask();
            }
            else if (virtualAddressIds.Length == 1)
            {
                return RemoveVirtualAddressAsync(loadBalancerId, virtualAddressIds[0], completionOption, cancellationToken, progress);
            }
            else
            {
                UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/loadbalancers/{loadBalancerId}/virtualips?id={id}");
                var parameters = new Dictionary<string, string>()
                {
                    { "loadBalancerId", loadBalancerId.Value },
                    { "id", string.Join(",", Array.ConvertAll(virtualAddressIds, i => i.Value)) }
                };

                Func<Uri, Uri> uriTransform =
                    uri =>
                    {
                        string path = uri.GetLeftPart(UriPartial.Path);
                        string query = uri.Query.Replace(",", "&id=").Replace("%2c", "&id=");
                        return new Uri(path + query);
                    };

                Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                    PrepareRequestAsyncFunc(HttpMethod.DELETE, template, parameters, uriTransform);

                Func<Task<HttpWebRequest>, Task<string>> requestResource =
                    GetResponseAsyncFunc(cancellationToken);

                Func<Task<string>, Task<LoadBalancer>> resultSelector =
                    task =>
                    {
                        if (completionOption == AsyncCompletionOption.RequestCompleted)
                            return WaitForLoadBalancerToLeaveStateAsync(loadBalancerId, LoadBalancerStatus.PendingUpdate, cancellationToken, progress);

                        return InternalTaskExtensions.CompletedTask(default(LoadBalancer));
                    };

                return AuthenticateServiceAsync(cancellationToken)
                    .Select(prepareRequest)
                    .Then(requestResource)
                    .Then(resultSelector);
            }
        }

        /// <inheritdoc/>
        public Task<ReadOnlyCollection<string>> ListAllowedDomainsAsync(CancellationToken cancellationToken)
        {
            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/loadbalancers/alloweddomains");
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<ListAllowedDomainsResponse>> requestResource =
                GetResponseAsyncFunc<ListAllowedDomainsResponse>(cancellationToken);

            Func<Task<ListAllowedDomainsResponse>, ReadOnlyCollection<string>> resultSelector =
                task => new ReadOnlyCollection<string>(task.Result.AllowedDomains.ToArray());

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task<ReadOnlyCollectionPage<LoadBalancer>> ListBillableLoadBalancersAsync(DateTimeOffset? startTime, DateTimeOffset? endTime, int? offset, int? limit, CancellationToken cancellationToken)
        {
            if (endTime < startTime)
                throw new ArgumentOutOfRangeException("endTime");
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset");
            if (limit <= 0)
                throw new ArgumentOutOfRangeException("limit");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/loadbalancers/billable?startTime={startTime}&endTime={endTime}&offset={offset}&limit={limit}");
            var parameters = new Dictionary<string, string>();
            if (startTime != null)
                parameters.Add("startTime", startTime.Value.ToString("yyyy-MM-dd"));
            if (endTime != null)
                parameters.Add("endTime", endTime.Value.ToString("yyyy-MM-dd"));
            if (offset != null)
                parameters.Add("offset", offset.ToString());
            if (limit != null)
                parameters.Add("limit", limit.ToString());

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<ListLoadBalancersResponse>> requestResource =
                GetResponseAsyncFunc<ListLoadBalancersResponse>(cancellationToken);

            Func<Task<ListLoadBalancersResponse>, ReadOnlyCollectionPage<LoadBalancer>> resultSelector =
                task =>
                {
                    ReadOnlyCollectionPage<LoadBalancer> page = null;
                    if (task.Result != null && task.Result.LoadBalancers != null && task.Result.LoadBalancers.Count > 0)
                    {
                        int nextOffset = (offset ?? 0) + task.Result.LoadBalancers.Count;
                        Func<CancellationToken, Task<ReadOnlyCollectionPage<LoadBalancer>>> getNextPageAsync =
                            nextCancellationToken => ListBillableLoadBalancersAsync(startTime, endTime, nextOffset, limit, nextCancellationToken);
                        page = new BasicReadOnlyCollectionPage<LoadBalancer>(task.Result.LoadBalancers, getNextPageAsync);
                    }

                    return page ?? ReadOnlyCollectionPage<LoadBalancer>.Empty;
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task<ReadOnlyCollection<LoadBalancerUsage>> ListAccountLevelUsageAsync(DateTimeOffset? startTime, DateTimeOffset? endTime, CancellationToken cancellationToken)
        {
            if (endTime < startTime)
                throw new ArgumentOutOfRangeException("endTime");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/loadbalancers/usage?startTime={startTime}&endTime={endTime}");
            var parameters = new Dictionary<string, string>();
            if (startTime != null)
                parameters.Add("startTime", startTime.Value.ToString("yyyy-MM-dd"));
            if (endTime != null)
                parameters.Add("endTime", endTime.Value.ToString("yyyy-MM-dd"));

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<ListLoadBalancerUsageResponse>> requestResource =
                GetResponseAsyncFunc<ListLoadBalancerUsageResponse>(cancellationToken);

            Func<Task<ListLoadBalancerUsageResponse>, ReadOnlyCollection<LoadBalancerUsage>> resultSelector =
                task => (task.Result != null ? task.Result.UsageRecords : null) ?? new ReadOnlyCollection<LoadBalancerUsage>(new LoadBalancerUsage[0]);

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task<ReadOnlyCollection<LoadBalancerUsage>> ListHistoricalUsageAsync(LoadBalancerId loadBalancerId, DateTimeOffset? startTime, DateTimeOffset? endTime, CancellationToken cancellationToken)
        {
            if (loadBalancerId == null)
                throw new ArgumentNullException("loadBalancerId");
            if (endTime < startTime)
                throw new ArgumentOutOfRangeException("endTime");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/loadbalancers/{loadBalancerId}/usage?startTime={startTime}&endTime={endTime}");
            var parameters = new Dictionary<string, string> { { "loadBalancerId", loadBalancerId.Value } };
            if (startTime != null)
                parameters.Add("startTime", startTime.Value.ToString("yyyy-MM-dd"));
            if (endTime != null)
                parameters.Add("endTime", endTime.Value.ToString("yyyy-MM-dd"));

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<ListLoadBalancerUsageResponse>> requestResource =
                GetResponseAsyncFunc<ListLoadBalancerUsageResponse>(cancellationToken);

            Func<Task<ListLoadBalancerUsageResponse>, ReadOnlyCollection<LoadBalancerUsage>> resultSelector =
                task => (task.Result != null ? task.Result.UsageRecords : null) ?? new ReadOnlyCollection<LoadBalancerUsage>(new LoadBalancerUsage[0]);

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task<ReadOnlyCollection<LoadBalancerUsage>> ListCurrentUsageAsync(LoadBalancerId loadBalancerId, CancellationToken cancellationToken)
        {
            if (loadBalancerId == null)
                throw new ArgumentNullException("loadBalancerId");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/loadbalancers/{loadBalancerId}/usage/current");
            var parameters = new Dictionary<string, string> { { "loadBalancerId", loadBalancerId.Value } };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<ListLoadBalancerUsageResponse>> requestResource =
                GetResponseAsyncFunc<ListLoadBalancerUsageResponse>(cancellationToken);

            Func<Task<ListLoadBalancerUsageResponse>, ReadOnlyCollection<LoadBalancerUsage>> resultSelector =
                task => (task.Result != null ? task.Result.UsageRecords : null) ?? new ReadOnlyCollection<LoadBalancerUsage>(new LoadBalancerUsage[0]);

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task<ReadOnlyCollection<NetworkItem>> ListAccessListAsync(LoadBalancerId loadBalancerId, CancellationToken cancellationToken)
        {
            if (loadBalancerId == null)
                throw new ArgumentNullException("loadBalancerId");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/loadbalancers/{loadBalancerId}/accesslist");
            var parameters = new Dictionary<string, string>()
            {
                { "loadBalancerId", loadBalancerId.Value }
            };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<GetAccessListResponse>> requestResource =
                GetResponseAsyncFunc<GetAccessListResponse>(cancellationToken);

            Func<Task<GetAccessListResponse>, ReadOnlyCollection<NetworkItem>> resultSelector =
                task => task.Result.AccessList;

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task CreateAccessListAsync(LoadBalancerId loadBalancerId, NetworkItem networkItem, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<LoadBalancer> progress)
        {
            if (networkItem == null)
                throw new ArgumentNullException("networkItem");

            return CreateAccessListAsync(loadBalancerId, new[] { networkItem }, completionOption, cancellationToken, progress);
        }

        /// <inheritdoc/>
        public Task CreateAccessListAsync(LoadBalancerId loadBalancerId, IEnumerable<NetworkItem> networkItems, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<LoadBalancer> progress)
        {
            if (networkItems == null)
                throw new ArgumentNullException("networkItems");

            return CreateAccessListAsync(loadBalancerId, networkItems.ToArray(), completionOption, cancellationToken, progress);
        }

        /// <summary>
        /// Add a collection of network items to the access list for a load balancer.
        /// </summary>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="networkItems">A collection of <see cref="NetworkItem"/> objects describing the network items to add to the load balancer's access list.</param>
        /// <param name="completionOption">Specifies when the <see cref="Task"/> representing the asynchronous server operation should be considered complete.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <param name="progress">An optional callback object to receive progress notifications, if <paramref name="completionOption"/> is <see cref="AsyncCompletionOption.RequestCompleted"/>. If this is <see langword="null"/>, no progress notifications are sent.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. When the operation
        /// completes, the <see cref="Task{TResult}.Result"/> property will contain a collection of
        /// <see cref="NetworkItem"/> objects describing the network items added to the access list
        /// for the load balancer.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="loadBalancerId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="networkItems"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="networkItems"/> contains any <see langword="null"/> values.
        /// <para>-or-</para>
        /// <para>If <paramref name="completionOption"/> is not a valid <see cref="AsyncCompletionOption"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Manage_Access_Lists-d1e3187.html">Manage Access Lists (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        public Task CreateAccessListAsync(LoadBalancerId loadBalancerId, NetworkItem[] networkItems, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<LoadBalancer> progress)
        {
            if (loadBalancerId == null)
                throw new ArgumentNullException("loadBalancerId");
            if (networkItems == null)
                throw new ArgumentNullException("networkItems");
            if (networkItems.Contains(null))
                throw new ArgumentException("networkItems cannot contain any null values");

            if (networkItems.Length == 0)
            {
                return InternalTaskExtensions.CompletedTask(Enumerable.Empty<NetworkItem>());
            }
            else
            {
                UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/loadbalancers/{loadBalancerId}/accesslist");
                var parameters = new Dictionary<string, string>()
                {
                    { "loadBalancerId", loadBalancerId.Value },
                };

                CreateAccessListRequest requestBody = new CreateAccessListRequest(networkItems);
                Func<Task<Tuple<IdentityToken, Uri>>, Task<HttpWebRequest>> prepareRequest =
                    PrepareRequestAsyncFunc(HttpMethod.POST, template, parameters, requestBody);

                Func<Task<HttpWebRequest>, Task<string>> requestResource =
                    GetResponseAsyncFunc(cancellationToken);

                Func<Task<string>, Task<LoadBalancer>> resultSelector =
                    task =>
                    {
                        if (completionOption == AsyncCompletionOption.RequestCompleted)
                            return WaitForLoadBalancerToLeaveStateAsync(loadBalancerId, LoadBalancerStatus.PendingUpdate, cancellationToken, progress);

                        return InternalTaskExtensions.CompletedTask(default(LoadBalancer));
                    };

                return AuthenticateServiceAsync(cancellationToken)
                    .Then(prepareRequest)
                    .Then(requestResource)
                    .Then(resultSelector);
            }
        }

        /// <inheritdoc/>
        public Task RemoveAccessListAsync(LoadBalancerId loadBalancerId, NetworkItemId networkItemId, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<LoadBalancer> progress)
        {
            if (loadBalancerId == null)
                throw new ArgumentNullException("loadBalancerId");
            if (networkItemId == null)
                throw new ArgumentNullException("networkItemId");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/loadbalancers/{loadBalancerId}/accesslist/{networkItemId}");
            var parameters = new Dictionary<string, string>()
            {
                { "loadBalancerId", loadBalancerId.Value },
                { "networkItemId", networkItemId.Value }
            };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.DELETE, template, parameters);

            Func<Task<HttpWebRequest>, Task<string>> requestResource =
                GetResponseAsyncFunc(cancellationToken);

            Func<Task<string>, Task<LoadBalancer>> resultSelector =
                task =>
                {
                    if (completionOption == AsyncCompletionOption.RequestCompleted)
                        return WaitForLoadBalancerToLeaveStateAsync(loadBalancerId, LoadBalancerStatus.PendingUpdate, cancellationToken, progress);

                    return InternalTaskExtensions.CompletedTask(default(LoadBalancer));
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Then(resultSelector);
        }

        /// <inheritdoc/>
        public Task RemoveAccessListRangeAsync(LoadBalancerId loadBalancerId, IEnumerable<NetworkItemId> networkItemIds, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<LoadBalancer> progress)
        {
            if (networkItemIds == null)
                throw new ArgumentNullException("networkItemIds");

            return RemoveAccessListRangeAsync(loadBalancerId, networkItemIds.ToArray(), completionOption, cancellationToken, progress);
        }

        /// <summary>
        /// Remove a collection of network items from the access list of a load balancer.
        /// </summary>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="networkItemIds">The network item IDs. These are obtained from <see cref="NetworkItem.Id"/>.</param>
        /// <param name="completionOption">Specifies when the <see cref="Task"/> representing the asynchronous server operation should be considered complete.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <param name="progress">An optional callback object to receive progress notifications, if <paramref name="completionOption"/> is <see cref="AsyncCompletionOption.RequestCompleted"/>. If this is <see langword="null"/>, no progress notifications are sent.</param>
        /// <returns>A <see cref="Task"/> object representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="loadBalancerId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="networkItemIds"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="networkItemIds"/> contains any <see langword="null"/> values.
        /// <para>-or-</para>
        /// <para>If <paramref name="completionOption"/> is not a valid <see cref="AsyncCompletionOption"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Manage_Access_Lists-d1e3187.html">Manage Access Lists (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        public Task RemoveAccessListRangeAsync(LoadBalancerId loadBalancerId, NetworkItemId[] networkItemIds, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<LoadBalancer> progress)
        {
            if (loadBalancerId == null)
                throw new ArgumentNullException("loadBalancerId");
            if (networkItemIds == null)
                throw new ArgumentNullException("networkItemIds");
            if (networkItemIds.Contains(null))
                throw new ArgumentException("networkItemIds cannot contain any null values", "networkItemIds");

            if (networkItemIds.Length == 0)
            {
                return InternalTaskExtensions.CompletedTask();
            }
            else if (networkItemIds.Length == 1)
            {
                return RemoveAccessListAsync(loadBalancerId, networkItemIds[0], completionOption, cancellationToken, progress);
            }
            else
            {
                UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/loadbalancers/{loadBalancerId}/accesslist?id={id}");
                var parameters = new Dictionary<string, string>
                {
                    { "loadBalancerId", loadBalancerId.Value },
                    { "id", string.Join(",", Array.ConvertAll(networkItemIds, i => i.Value)) }
                };

                Func<Uri, Uri> uriTransform =
                    uri =>
                    {
                        string path = uri.GetLeftPart(UriPartial.Path);
                        string query = uri.Query.Replace(",", "&id=").Replace("%2c", "&id=");
                        return new Uri(path + query);
                    };

                Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                    PrepareRequestAsyncFunc(HttpMethod.DELETE, template, parameters, uriTransform);

                Func<Task<HttpWebRequest>, Task<string>> requestResource =
                    GetResponseAsyncFunc(cancellationToken);

                Func<Task<string>, Task<LoadBalancer>> resultSelector =
                    task =>
                    {
                        if (completionOption == AsyncCompletionOption.RequestCompleted)
                            return WaitForLoadBalancerToLeaveStateAsync(loadBalancerId, LoadBalancerStatus.PendingUpdate, cancellationToken, progress);

                        return InternalTaskExtensions.CompletedTask(default(LoadBalancer));
                    };

                return AuthenticateServiceAsync(cancellationToken)
                    .Select(prepareRequest)
                    .Then(requestResource)
                    .Then(resultSelector);
            }
        }

        /// <inheritdoc/>
        public Task ClearAccessListAsync(LoadBalancerId loadBalancerId, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<LoadBalancer> progress)
        {
            if (loadBalancerId == null)
                throw new ArgumentNullException("loadBalancerId");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/loadbalancers/{loadBalancerId}/accesslist");
            var parameters = new Dictionary<string, string>()
            {
                { "loadBalancerId", loadBalancerId.Value },
            };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.DELETE, template, parameters);

            Func<Task<HttpWebRequest>, Task<string>> requestResource =
                GetResponseAsyncFunc(cancellationToken);

            Func<Task<string>, Task<LoadBalancer>> resultSelector =
                task =>
                {
                    if (completionOption == AsyncCompletionOption.RequestCompleted)
                        return WaitForLoadBalancerToLeaveStateAsync(loadBalancerId, LoadBalancerStatus.PendingUpdate, cancellationToken, progress);

                    return InternalTaskExtensions.CompletedTask(default(LoadBalancer));
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Then(resultSelector);
        }

        /// <inheritdoc/>
        public Task<HealthMonitor> GetHealthMonitorAsync(LoadBalancerId loadBalancerId, CancellationToken cancellationToken)
        {
            if (loadBalancerId == null)
                throw new ArgumentNullException("loadBalancerId");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/loadbalancers/{loadBalancerId}/healthmonitor");
            var parameters = new Dictionary<string, string>()
            {
                { "loadBalancerId", loadBalancerId.Value }
            };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<JObject>> requestResource =
                GetResponseAsyncFunc<JObject>(cancellationToken);

            Func<Task<JObject>, HealthMonitor> resultSelector =
                task =>
                {
                    if (task.Result == null)
                        return null;

                    JObject healthMonitorObject = task.Result["healthMonitor"] as JObject;
                    if (healthMonitorObject == null)
                        return null;

                    return HealthMonitor.FromJObject(healthMonitorObject);
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task SetHealthMonitorAsync(LoadBalancerId loadBalancerId, HealthMonitor monitor, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<LoadBalancer> progress)
        {
            if (loadBalancerId == null)
                throw new ArgumentNullException("loadBalancerId");
            if (monitor == null)
                throw new ArgumentNullException("monitor");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/loadbalancers/{loadBalancerId}/healthmonitor");
            var parameters = new Dictionary<string, string>()
            {
                { "loadBalancerId", loadBalancerId.Value }
            };

            Func<Task<Tuple<IdentityToken, Uri>>, Task<HttpWebRequest>> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.PUT, template, parameters, monitor);

            Func<Task<HttpWebRequest>, Task<string>> requestResource =
                GetResponseAsyncFunc(cancellationToken);

            Func<Task<string>, Task<LoadBalancer>> resultSelector =
                task =>
                {
                    if (completionOption == AsyncCompletionOption.RequestCompleted)
                        return WaitForLoadBalancerToLeaveStateAsync(loadBalancerId, LoadBalancerStatus.PendingUpdate, cancellationToken, progress);

                    return InternalTaskExtensions.CompletedTask(default(LoadBalancer));
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Then(prepareRequest)
                .Then(requestResource)
                .Then(resultSelector);
        }

        /// <inheritdoc/>
        public Task RemoveHealthMonitorAsync(LoadBalancerId loadBalancerId, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<LoadBalancer> progress)
        {
            if (loadBalancerId == null)
                throw new ArgumentNullException("loadBalancerId");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/loadbalancers/{loadBalancerId}/healthmonitor");
            var parameters = new Dictionary<string, string>()
            {
                { "loadBalancerId", loadBalancerId.Value },
            };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.DELETE, template, parameters);

            Func<Task<HttpWebRequest>, Task<string>> requestResource =
                GetResponseAsyncFunc(cancellationToken);

            Func<Task<string>, Task<LoadBalancer>> resultSelector =
                task =>
                {
                    if (completionOption == AsyncCompletionOption.RequestCompleted)
                        return WaitForLoadBalancerToLeaveStateAsync(loadBalancerId, LoadBalancerStatus.PendingUpdate, cancellationToken, progress);

                    return InternalTaskExtensions.CompletedTask(default(LoadBalancer));
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Then(resultSelector);
        }

        /// <inheritdoc/>
        public Task<SessionPersistence> GetSessionPersistenceAsync(LoadBalancerId loadBalancerId, CancellationToken cancellationToken)
        {
            if (loadBalancerId == null)
                throw new ArgumentNullException("loadBalancerId");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/loadbalancers/{loadBalancerId}/sessionpersistence");
            var parameters = new Dictionary<string, string>()
            {
                { "loadBalancerId", loadBalancerId.Value }
            };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<SessionPersistence>> requestResource =
                GetResponseAsyncFunc<SessionPersistence>(cancellationToken);

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource);
        }

        /// <inheritdoc/>
        public Task SetSessionPersistenceAsync(LoadBalancerId loadBalancerId, SessionPersistence sessionPersistence, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<LoadBalancer> progress)
        {
            if (loadBalancerId == null)
                throw new ArgumentNullException("loadBalancerId");
            if (sessionPersistence == null)
                throw new ArgumentNullException("sessionPersistence");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/loadbalancers/{loadBalancerId}/sessionpersistence");
            var parameters = new Dictionary<string, string>()
            {
                { "loadBalancerId", loadBalancerId.Value }
            };

            Func<Task<Tuple<IdentityToken, Uri>>, Task<HttpWebRequest>> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.PUT, template, parameters, sessionPersistence);

            Func<Task<HttpWebRequest>, Task<string>> requestResource =
                GetResponseAsyncFunc(cancellationToken);

            Func<Task<string>, Task<LoadBalancer>> resultSelector =
                task =>
                {
                    if (completionOption == AsyncCompletionOption.RequestCompleted)
                        return WaitForLoadBalancerToLeaveStateAsync(loadBalancerId, LoadBalancerStatus.PendingUpdate, cancellationToken, progress);

                    return InternalTaskExtensions.CompletedTask(default(LoadBalancer));
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Then(prepareRequest)
                .Then(requestResource)
                .Then(resultSelector);
        }

        /// <inheritdoc/>
        public Task RemoveSessionPersistenceAsync(LoadBalancerId loadBalancerId, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<LoadBalancer> progress)
        {
            if (loadBalancerId == null)
                throw new ArgumentNullException("loadBalancerId");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/loadbalancers/{loadBalancerId}/sessionpersistence");
            var parameters = new Dictionary<string, string>()
            {
                { "loadBalancerId", loadBalancerId.Value }
            };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.DELETE, template, parameters);

            Func<Task<HttpWebRequest>, Task<string>> requestResource =
                GetResponseAsyncFunc(cancellationToken);

            Func<Task<string>, Task<LoadBalancer>> resultSelector =
                task =>
                {
                    if (completionOption == AsyncCompletionOption.RequestCompleted)
                        return WaitForLoadBalancerToLeaveStateAsync(loadBalancerId, LoadBalancerStatus.PendingUpdate, cancellationToken, progress);

                    return InternalTaskExtensions.CompletedTask(default(LoadBalancer));
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Then(resultSelector);
        }

        /// <inheritdoc/>
        public Task<bool> GetConnectionLoggingAsync(LoadBalancerId loadBalancerId, CancellationToken cancellationToken)
        {
            if (loadBalancerId == null)
                throw new ArgumentNullException("loadBalancerId");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/loadbalancers/{loadBalancerId}/connectionlogging");
            var parameters = new Dictionary<string, string>()
            {
                { "loadBalancerId", loadBalancerId.Value }
            };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<GetLoadBalancerConnectionLoggingResponse>> requestResource =
                GetResponseAsyncFunc<GetLoadBalancerConnectionLoggingResponse>(cancellationToken);

            Func<Task<GetLoadBalancerConnectionLoggingResponse>, bool> resultSelector =
                task => task.Result != null ? task.Result.Enabled ?? false : false;

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task SetConnectionLoggingAsync(LoadBalancerId loadBalancerId, bool enabled, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<LoadBalancer> progress)
        {
            if (loadBalancerId == null)
                throw new ArgumentNullException("loadBalancerId");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/loadbalancers/{loadBalancerId}/connectionlogging");
            var parameters = new Dictionary<string, string>()
            {
                { "loadBalancerId", loadBalancerId.Value }
            };

            SetLoadBalancerConnectionLoggingRequest request = new SetLoadBalancerConnectionLoggingRequest(enabled);
            Func<Task<Tuple<IdentityToken, Uri>>, Task<HttpWebRequest>> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.PUT, template, parameters, request);

            Func<Task<HttpWebRequest>, Task<string>> requestResource =
                GetResponseAsyncFunc(cancellationToken);

            Func<Task<string>, Task<LoadBalancer>> resultSelector =
                task =>
                {
                    if (completionOption == AsyncCompletionOption.RequestCompleted)
                        return WaitForLoadBalancerToLeaveStateAsync(loadBalancerId, LoadBalancerStatus.PendingUpdate, cancellationToken, progress);

                    return InternalTaskExtensions.CompletedTask(default(LoadBalancer));
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Then(prepareRequest)
                .Then(requestResource)
                .Then(resultSelector);
        }

        /// <inheritdoc/>
        public Task<ConnectionThrottles> ListThrottlesAsync(LoadBalancerId loadBalancerId, CancellationToken cancellationToken)
        {
            if (loadBalancerId == null)
                throw new ArgumentNullException("loadBalancerId");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/loadbalancers/{loadBalancerId}/connectionthrottle");
            var parameters = new Dictionary<string, string>()
            {
                { "loadBalancerId", loadBalancerId.Value }
            };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<ListLoadBalancerThrottlesResponse>> requestResource =
                GetResponseAsyncFunc<ListLoadBalancerThrottlesResponse>(cancellationToken);

            Func<Task<ListLoadBalancerThrottlesResponse>, ConnectionThrottles> resultSelector =
                task => task.Result.Throttles;

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task UpdateThrottlesAsync(LoadBalancerId loadBalancerId, ConnectionThrottles throttleConfiguration, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<LoadBalancer> progress)
        {
            if (loadBalancerId == null)
                throw new ArgumentNullException("loadBalancerId");
            if (throttleConfiguration == null)
                throw new ArgumentNullException("throttleConfiguration");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/loadbalancers/{loadBalancerId}/connectionthrottle");
            var parameters = new Dictionary<string, string>()
            {
                { "loadBalancerId", loadBalancerId.Value }
            };

            Func<Task<Tuple<IdentityToken, Uri>>, Task<HttpWebRequest>> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.PUT, template, parameters, throttleConfiguration);

            Func<Task<HttpWebRequest>, Task<string>> requestResource =
                GetResponseAsyncFunc(cancellationToken);

            Func<Task<string>, Task<LoadBalancer>> resultSelector =
                task =>
                {
                    if (completionOption == AsyncCompletionOption.RequestCompleted)
                        return WaitForLoadBalancerToLeaveStateAsync(loadBalancerId, LoadBalancerStatus.PendingUpdate, cancellationToken, progress);

                    return InternalTaskExtensions.CompletedTask(default(LoadBalancer));
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Then(prepareRequest)
                .Then(requestResource)
                .Then(resultSelector);
        }

        /// <inheritdoc/>
        public Task RemoveThrottlesAsync(LoadBalancerId loadBalancerId, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<LoadBalancer> progress)
        {
            if (loadBalancerId == null)
                throw new ArgumentNullException("loadBalancerId");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/loadbalancers/{loadBalancerId}/connectionthrottle");
            var parameters = new Dictionary<string, string>()
            {
                { "loadBalancerId", loadBalancerId.Value }
            };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.DELETE, template, parameters);

            Func<Task<HttpWebRequest>, Task<string>> requestResource =
                GetResponseAsyncFunc(cancellationToken);

            Func<Task<string>, Task<LoadBalancer>> resultSelector =
                task =>
                {
                    if (completionOption == AsyncCompletionOption.RequestCompleted)
                        return WaitForLoadBalancerToLeaveStateAsync(loadBalancerId, LoadBalancerStatus.PendingUpdate, cancellationToken, progress);

                    return InternalTaskExtensions.CompletedTask(default(LoadBalancer));
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Then(resultSelector);
        }

        /// <inheritdoc/>
        public Task<bool> GetContentCachingAsync(LoadBalancerId loadBalancerId, CancellationToken cancellationToken)
        {
            if (loadBalancerId == null)
                throw new ArgumentNullException("loadBalancerId");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/loadbalancers/{loadBalancerId}/contentcaching");
            var parameters = new Dictionary<string, string>()
            {
                { "loadBalancerId", loadBalancerId.Value }
            };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<GetLoadBalancerContentCachingResponse>> requestResource =
                GetResponseAsyncFunc<GetLoadBalancerContentCachingResponse>(cancellationToken);

            Func<Task<GetLoadBalancerContentCachingResponse>, bool> resultSelector =
                task => task.Result != null ? task.Result.Enabled ?? false : false;

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task SetContentCachingAsync(LoadBalancerId loadBalancerId, bool enabled, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<LoadBalancer> progress)
        {
            if (loadBalancerId == null)
                throw new ArgumentNullException("loadBalancerId");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/loadbalancers/{loadBalancerId}/contentcaching");
            var parameters = new Dictionary<string, string>()
            {
                { "loadBalancerId", loadBalancerId.Value }
            };

            SetLoadBalancerContentCachingRequest request = new SetLoadBalancerContentCachingRequest(enabled);
            Func<Task<Tuple<IdentityToken, Uri>>, Task<HttpWebRequest>> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.PUT, template, parameters, request);

            Func<Task<HttpWebRequest>, Task<string>> requestResource =
                GetResponseAsyncFunc(cancellationToken);

            Func<Task<string>, Task<LoadBalancer>> resultSelector =
                task =>
                {
                    if (completionOption == AsyncCompletionOption.RequestCompleted)
                        return WaitForLoadBalancerToLeaveStateAsync(loadBalancerId, LoadBalancerStatus.PendingUpdate, cancellationToken, progress);

                    return InternalTaskExtensions.CompletedTask(default(LoadBalancer));
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Then(prepareRequest)
                .Then(requestResource)
                .Then(resultSelector);
        }

        /// <inheritdoc/>
        public Task<ReadOnlyCollection<LoadBalancingProtocol>> ListProtocolsAsync(CancellationToken cancellationToken)
        {
            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/loadbalancers/protocols");
            var parameters = new Dictionary<string, string>();

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<ListLoadBalancingProtocolsResponse>> requestResource =
                GetResponseAsyncFunc<ListLoadBalancingProtocolsResponse>(cancellationToken);

            Func<Task<ListLoadBalancingProtocolsResponse>, ReadOnlyCollection<LoadBalancingProtocol>> resultSelector =
                task => (task.Result != null ? task.Result.Protocols : null) ?? new ReadOnlyCollection<LoadBalancingProtocol>(new LoadBalancingProtocol[0]);

            // authenticate -> request resource -> check result -> parse result -> cache result -> return
            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task<ReadOnlyCollection<LoadBalancingAlgorithm>> ListAlgorithmsAsync(CancellationToken cancellationToken)
        {
            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/loadbalancers/algorithms");
            var parameters = new Dictionary<string, string>();

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<ListLoadBalancingAlgorithmsResponse>> requestResource =
                GetResponseAsyncFunc<ListLoadBalancingAlgorithmsResponse>(cancellationToken);

            Func<Task<ListLoadBalancingAlgorithmsResponse>, ReadOnlyCollection<LoadBalancingAlgorithm>> resultSelector =
                task => (task.Result != null ? new ReadOnlyCollection<LoadBalancingAlgorithm>(task.Result.Algorithms.ToArray()) : null) ?? new ReadOnlyCollection<LoadBalancingAlgorithm>(new LoadBalancingAlgorithm[0]);

            // authenticate -> request resource -> check result -> parse result -> cache result -> return
            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task<LoadBalancerSslConfiguration> GetSslConfigurationAsync(LoadBalancerId loadBalancerId, CancellationToken cancellationToken)
        {
            if (loadBalancerId == null)
                throw new ArgumentNullException("loadBalancerId");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/loadbalancers/{loadBalancerId}/ssltermination");
            var parameters = new Dictionary<string, string>()
            {
                { "loadBalancerId", loadBalancerId.Value }
            };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<GetLoadBalancerSslConfigurationResponse>> requestResource =
                GetResponseAsyncFunc<GetLoadBalancerSslConfigurationResponse>(cancellationToken);

            Func<Task<GetLoadBalancerSslConfigurationResponse>, LoadBalancerSslConfiguration> resultSelector =
                task => task.Result != null ? task.Result.SslConfiguration : null;

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task UpdateSslConfigurationAsync(LoadBalancerId loadBalancerId, LoadBalancerSslConfiguration configuration, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<LoadBalancer> progress)
        {
            if (loadBalancerId == null)
                throw new ArgumentNullException("loadBalancerId");
            if (configuration == null)
                throw new ArgumentNullException("configuration");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/loadbalancers/{loadBalancerId}/ssltermination");
            var parameters = new Dictionary<string, string>()
            {
                { "loadBalancerId", loadBalancerId.Value }
            };

            LoadBalancerSslConfiguration requestBody = configuration;
            Func<Task<Tuple<IdentityToken, Uri>>, Task<HttpWebRequest>> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.PUT, template, parameters, requestBody);

            Func<Task<HttpWebRequest>, Task<string>> requestResource =
                GetResponseAsyncFunc(cancellationToken);

            Func<Task<string>, Task<LoadBalancer>> resultSelector =
                task =>
                {
                    if (completionOption == AsyncCompletionOption.RequestCompleted)
                        return WaitForLoadBalancerToLeaveStateAsync(loadBalancerId, LoadBalancerStatus.PendingUpdate, cancellationToken, progress);

                    return InternalTaskExtensions.CompletedTask(default(LoadBalancer));
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Then(prepareRequest)
                .Then(requestResource)
                .Then(resultSelector);
        }

        /// <inheritdoc/>
        public Task RemoveSslConfigurationAsync(LoadBalancerId loadBalancerId, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<LoadBalancer> progress)
        {
            if (loadBalancerId == null)
                throw new ArgumentNullException("loadBalancerId");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/loadbalancers/{loadBalancerId}/ssltermination");
            var parameters = new Dictionary<string, string>()
            {
                { "loadBalancerId", loadBalancerId.Value }
            };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.DELETE, template, parameters);

            Func<Task<HttpWebRequest>, Task<string>> requestResource =
                GetResponseAsyncFunc(cancellationToken);

            Func<Task<string>, Task<LoadBalancer>> resultSelector =
                task =>
                {
                    if (completionOption == AsyncCompletionOption.RequestCompleted)
                        return WaitForLoadBalancerToLeaveStateAsync(loadBalancerId, LoadBalancerStatus.PendingUpdate, cancellationToken, progress);

                    return InternalTaskExtensions.CompletedTask(default(LoadBalancer));
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Then(resultSelector);
        }

        /// <inheritdoc/>
        public Task<ReadOnlyCollection<LoadBalancerMetadataItem>> ListLoadBalancerMetadataAsync(LoadBalancerId loadBalancerId, CancellationToken cancellationToken)
        {
            if (loadBalancerId == null)
                throw new ArgumentNullException("loadBalancerId");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/loadbalancers/{loadBalancerId}/metadata");
            var parameters = new Dictionary<string, string>()
            {
                { "loadBalancerId", loadBalancerId.Value },
            };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<ListLoadBalancerMetadataResponse>> requestResource =
                GetResponseAsyncFunc<ListLoadBalancerMetadataResponse>(cancellationToken);

            Func<Task<ListLoadBalancerMetadataResponse>, ReadOnlyCollection<LoadBalancerMetadataItem>> resultSelector =
                task => (task.Result != null ? task.Result.Metadata : null) ?? new ReadOnlyCollection<LoadBalancerMetadataItem>(new LoadBalancerMetadataItem[0]);

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task<LoadBalancerMetadataItem> GetLoadBalancerMetadataItemAsync(LoadBalancerId loadBalancerId, MetadataId metadataId, CancellationToken cancellationToken)
        {
            if (loadBalancerId == null)
                throw new ArgumentNullException("loadBalancerId");
            if (metadataId == null)
                throw new ArgumentNullException("metadataId");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/loadbalancers/{loadBalancerId}/metadata/{metaId}");
            var parameters = new Dictionary<string, string>()
            {
                { "loadBalancerId", loadBalancerId.Value },
                { "metaId", metadataId.Value },
            };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<GetLoadBalancerMetadataItemResponse>> requestResource =
                GetResponseAsyncFunc<GetLoadBalancerMetadataItemResponse>(cancellationToken);

            Func<Task<GetLoadBalancerMetadataItemResponse>, LoadBalancerMetadataItem> resultSelector =
                task => task.Result != null ? task.Result.MetadataItem : null;

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task<ReadOnlyCollection<LoadBalancerMetadataItem>> ListNodeMetadataAsync(LoadBalancerId loadBalancerId, NodeId nodeId, CancellationToken cancellationToken)
        {
            if (loadBalancerId == null)
                throw new ArgumentNullException("loadBalancerId");
            if (nodeId == null)
                throw new ArgumentNullException("nodeId");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/loadbalancers/{loadBalancerId}/nodes/{nodeId}/metadata");
            var parameters = new Dictionary<string, string>()
            {
                { "loadBalancerId", loadBalancerId.Value },
                { "nodeId", nodeId.Value },
            };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<ListLoadBalancerMetadataResponse>> requestResource =
                GetResponseAsyncFunc<ListLoadBalancerMetadataResponse>(cancellationToken);

            Func<Task<ListLoadBalancerMetadataResponse>, ReadOnlyCollection<LoadBalancerMetadataItem>> resultSelector =
                task => (task.Result != null ? task.Result.Metadata : null) ?? new ReadOnlyCollection<LoadBalancerMetadataItem>(new LoadBalancerMetadataItem[0]);

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task<LoadBalancerMetadataItem> GetNodeMetadataItemAsync(LoadBalancerId loadBalancerId, NodeId nodeId, MetadataId metadataId, CancellationToken cancellationToken)
        {
            if (loadBalancerId == null)
                throw new ArgumentNullException("loadBalancerId");
            if (nodeId == null)
                throw new ArgumentNullException("nodeId");
            if (metadataId == null)
                throw new ArgumentNullException("metadataId");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/loadbalancers/{loadBalancerId}/nodes/{nodeId}/metadata/{metaId}");
            var parameters = new Dictionary<string, string>()
            {
                { "loadBalancerId", loadBalancerId.Value },
                { "nodeId", nodeId.Value },
                { "metaId", metadataId.Value },
            };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<GetLoadBalancerMetadataItemResponse>> requestResource =
                GetResponseAsyncFunc<GetLoadBalancerMetadataItemResponse>(cancellationToken);

            Func<Task<GetLoadBalancerMetadataItemResponse>, LoadBalancerMetadataItem> resultSelector =
                task => task.Result != null ? task.Result.MetadataItem : null;

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task<ReadOnlyCollection<LoadBalancerMetadataItem>> AddLoadBalancerMetadataAsync(LoadBalancerId loadBalancerId, IEnumerable<KeyValuePair<string, string>> metadata, CancellationToken cancellationToken)
        {
            if (loadBalancerId == null)
                throw new ArgumentNullException("loadBalancerId");
            if (metadata == null)
                throw new ArgumentNullException("metadata");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/loadbalancers/{loadBalancerId}/metadata");
            var parameters = new Dictionary<string, string>()
            {
                { "loadBalancerId", loadBalancerId.Value },
            };

            AddLoadBalancerMetadataRequest requestBody = new AddLoadBalancerMetadataRequest(metadata);
            Func<Task<Tuple<IdentityToken, Uri>>, Task<HttpWebRequest>> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.POST, template, parameters, requestBody);

            Func<Task<HttpWebRequest>, Task<ListLoadBalancerMetadataResponse>> requestResource =
                GetResponseAsyncFunc<ListLoadBalancerMetadataResponse>(cancellationToken);

            Func<Task<ListLoadBalancerMetadataResponse>, ReadOnlyCollection<LoadBalancerMetadataItem>> resultSelector =
                task => (task.Result != null ? task.Result.Metadata : null) ?? new ReadOnlyCollection<LoadBalancerMetadataItem>(new LoadBalancerMetadataItem[0]);

            return AuthenticateServiceAsync(cancellationToken)
                .Then(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task<ReadOnlyCollection<LoadBalancerMetadataItem>> AddNodeMetadataAsync(LoadBalancerId loadBalancerId, NodeId nodeId, IEnumerable<KeyValuePair<string, string>> metadata, CancellationToken cancellationToken)
        {
            if (loadBalancerId == null)
                throw new ArgumentNullException("loadBalancerId");
            if (nodeId == null)
                throw new ArgumentNullException("nodeId");
            if (metadata == null)
                throw new ArgumentNullException("metadata");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/loadbalancers/{loadBalancerId}/nodes/{nodeId}/metadata");
            var parameters = new Dictionary<string, string>()
            {
                { "loadBalancerId", loadBalancerId.Value },
                { "nodeId", nodeId.Value },
            };

            AddLoadBalancerMetadataRequest requestBody = new AddLoadBalancerMetadataRequest(metadata);
            Func<Task<Tuple<IdentityToken, Uri>>, Task<HttpWebRequest>> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.POST, template, parameters, requestBody);

            Func<Task<HttpWebRequest>, Task<ListLoadBalancerMetadataResponse>> requestResource =
                GetResponseAsyncFunc<ListLoadBalancerMetadataResponse>(cancellationToken);

            Func<Task<ListLoadBalancerMetadataResponse>, ReadOnlyCollection<LoadBalancerMetadataItem>> resultSelector =
                task => (task.Result != null ? task.Result.Metadata : null) ?? new ReadOnlyCollection<LoadBalancerMetadataItem>(new LoadBalancerMetadataItem[0]);

            return AuthenticateServiceAsync(cancellationToken)
                .Then(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task UpdateLoadBalancerMetadataItemAsync(LoadBalancerId loadBalancerId, MetadataId metadataId, string value, CancellationToken cancellationToken)
        {
            if (loadBalancerId == null)
                throw new ArgumentNullException("loadBalancerId");
            if (metadataId == null)
                throw new ArgumentNullException("metadataId");
            if (value == null)
                throw new ArgumentNullException("value");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/loadbalancers/{loadBalancerId}/metadata/{metaId}");
            var parameters = new Dictionary<string, string>()
            {
                { "loadBalancerId", loadBalancerId.Value },
                { "metaId", metadataId.Value }
            };

            UpdateLoadBalancerMetadataItemRequest requestBody = new UpdateLoadBalancerMetadataItemRequest(value);
            Func<Task<Tuple<IdentityToken, Uri>>, Task<HttpWebRequest>> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.PUT, template, parameters, requestBody);

            Func<Task<HttpWebRequest>, Task<string>> requestResource =
                GetResponseAsyncFunc(cancellationToken);

            return AuthenticateServiceAsync(cancellationToken)
                .Then(prepareRequest)
                .Then(requestResource);
        }

        /// <inheritdoc/>
        public Task UpdateNodeMetadataItemAsync(LoadBalancerId loadBalancerId, NodeId nodeId, MetadataId metadataId, string value, CancellationToken cancellationToken)
        {
            if (loadBalancerId == null)
                throw new ArgumentNullException("loadBalancerId");
            if (nodeId == null)
                throw new ArgumentNullException("nodeId");
            if (metadataId == null)
                throw new ArgumentNullException("metadataId");
            if (value == null)
                throw new ArgumentNullException("value");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/loadbalancers/{loadBalancerId}/nodes/{nodeId}/metadata/{metaId}");
            var parameters = new Dictionary<string, string>()
            {
                { "loadBalancerId", loadBalancerId.Value },
                { "nodeId", nodeId.Value },
                { "metaId", metadataId.Value }
            };

            UpdateLoadBalancerMetadataItemRequest requestBody = new UpdateLoadBalancerMetadataItemRequest(value);
            Func<Task<Tuple<IdentityToken, Uri>>, Task<HttpWebRequest>> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.PUT, template, parameters, requestBody);

            Func<Task<HttpWebRequest>, Task<string>> requestResource =
                GetResponseAsyncFunc(cancellationToken);

            return AuthenticateServiceAsync(cancellationToken)
                .Then(prepareRequest)
                .Then(requestResource);
        }

        /// <inheritdoc/>
        public Task RemoveLoadBalancerMetadataItemAsync(LoadBalancerId loadBalancerId, IEnumerable<MetadataId> metadataIds, CancellationToken cancellationToken)
        {
            if (metadataIds == null)
                throw new ArgumentNullException("metadataIds");

            return RemoveLoadBalancerMetadataItemAsync(loadBalancerId, metadataIds.ToArray(), cancellationToken);
        }

        /// <summary>
        /// Removes one or more metadata items associated with a load balancer.
        /// </summary>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="metadataIds">The metadata item IDs. These are obtained from <see cref="LoadBalancerMetadataItem.Id">LoadBalancerMetadataItem.Id</see>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>A <see cref="Task"/> object representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="loadBalancerId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="metadataIds"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="metadataIds"/> contains any <see langword="null"/> values.
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Remove_Metadata-d1e2675.html">Remove Metadata (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        public Task RemoveLoadBalancerMetadataItemAsync(LoadBalancerId loadBalancerId, MetadataId[] metadataIds, CancellationToken cancellationToken)
        {
            if (loadBalancerId == null)
                throw new ArgumentNullException("loadBalancerId");
            if (metadataIds == null)
                throw new ArgumentNullException("metadataIds");
            if (metadataIds.Contains(null))
                throw new ArgumentException("metadataIds cannot contain any null values", "metadataIds");

            if (metadataIds.Length == 0)
            {
                return InternalTaskExtensions.CompletedTask();
            }
            else if (metadataIds.Length == 1)
            {
                UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/loadbalancers/{loadBalancerId}/metadata/{metaId}");
                var parameters = new Dictionary<string, string>()
                {
                    { "loadBalancerId", loadBalancerId.Value },
                    { "metaId", metadataIds[0].Value }
                };

                Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                    PrepareRequestAsyncFunc(HttpMethod.DELETE, template, parameters);

                Func<Task<HttpWebRequest>, Task<string>> requestResource =
                    GetResponseAsyncFunc(cancellationToken);

                return AuthenticateServiceAsync(cancellationToken)
                    .Select(prepareRequest)
                    .Then(requestResource);
            }
            else
            {
                UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/loadbalancers/{loadBalancerId}/metadata?id={id}");
                var parameters = new Dictionary<string, string>()
                {
                    { "loadBalancerId", loadBalancerId.Value },
                    { "id", string.Join(",", Array.ConvertAll(metadataIds, i => i.Value)) }
                };

                Func<Uri, Uri> uriTransform =
                    uri =>
                    {
                        string path = uri.GetLeftPart(UriPartial.Path);
                        string query = uri.Query.Replace(",", "&id=").Replace("%2c", "&id=");
                        return new Uri(path + query);
                    };

                Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                    PrepareRequestAsyncFunc(HttpMethod.DELETE, template, parameters, uriTransform);

                Func<Task<HttpWebRequest>, Task<string>> requestResource =
                    GetResponseAsyncFunc(cancellationToken);

                return AuthenticateServiceAsync(cancellationToken)
                    .Select(prepareRequest)
                    .Then(requestResource);
            }
        }

        /// <inheritdoc/>
        public Task RemoveNodeMetadataItemAsync(LoadBalancerId loadBalancerId, NodeId nodeId, IEnumerable<MetadataId> metadataIds, CancellationToken cancellationToken)
        {
            if (metadataIds == null)
                throw new ArgumentNullException("metadataIds");

            return RemoveNodeMetadataItemAsync(loadBalancerId, nodeId, metadataIds.ToArray(), cancellationToken);
        }

        /// <summary>
        /// Removes one or more metadata items associated with a load balancer node.
        /// </summary>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="nodeId">The load balancer node ID. This is obtained from <see cref="Node.Id">Node.Id</see>.</param>
        /// <param name="metadataIds">The metadata item IDs. These are obtained from <see cref="LoadBalancerMetadataItem.Id">LoadBalancerMetadataItem.Id</see>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>A <see cref="Task"/> object representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="loadBalancerId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="nodeId"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="metadataIds"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="metadataIds"/> contains any <see langword="null"/> values.
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Remove_Metadata-d1e2675.html">Remove Metadata (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        public Task RemoveNodeMetadataItemAsync(LoadBalancerId loadBalancerId, NodeId nodeId, MetadataId[] metadataIds, CancellationToken cancellationToken)
        {
            if (loadBalancerId == null)
                throw new ArgumentNullException("loadBalancerId");
            if (nodeId == null)
                throw new ArgumentNullException("nodeId");
            if (metadataIds == null)
                throw new ArgumentNullException("metadataIds");
            if (metadataIds.Contains(null))
                throw new ArgumentException("metadataIds cannot contain any null values", "metadataIds");

            if (metadataIds.Length == 0)
            {
                return InternalTaskExtensions.CompletedTask();
            }
            else if (metadataIds.Length == 1)
            {
                UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/loadbalancers/{loadBalancerId}/nodes/{nodeId}/metadata/{metaId}");
                var parameters = new Dictionary<string, string>()
                {
                    { "loadBalancerId", loadBalancerId.Value },
                    { "nodeId", nodeId.Value },
                    { "metaId", metadataIds[0].Value }
                };

                Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                    PrepareRequestAsyncFunc(HttpMethod.DELETE, template, parameters);

                Func<Task<HttpWebRequest>, Task<string>> requestResource =
                    GetResponseAsyncFunc(cancellationToken);

                return AuthenticateServiceAsync(cancellationToken)
                    .Select(prepareRequest)
                    .Then(requestResource);
            }
            else
            {
                UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/loadbalancers/{loadBalancerId}/nodes/{nodeId}/metadata?id={id}");
                var parameters = new Dictionary<string, string>()
                {
                    { "loadBalancerId", loadBalancerId.Value },
                    { "nodeId", nodeId.Value },
                    { "id", string.Join(",", Array.ConvertAll(metadataIds, i => i.Value)) }
                };

                Func<Uri, Uri> uriTransform =
                    uri =>
                    {
                        string path = uri.GetLeftPart(UriPartial.Path);
                        string query = uri.Query.Replace(",", "&id=").Replace("%2c", "&id=");
                        return new Uri(path + query);
                    };

                Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                    PrepareRequestAsyncFunc(HttpMethod.DELETE, template, parameters, uriTransform);

                Func<Task<HttpWebRequest>, Task<string>> requestResource =
                    GetResponseAsyncFunc(cancellationToken);

                return AuthenticateServiceAsync(cancellationToken)
                    .Select(prepareRequest)
                    .Then(requestResource);
            }
        }

        #endregion

        /// <summary>
        /// Creates a <see cref="Task"/> that will complete after a load balancer leaves a particular state.
        /// </summary>
        /// <remarks>
        /// The task is considered complete as soon as a call to <see cref="ILoadBalancerService.GetLoadBalancerAsync"/>
        /// indicates that the load balancer is not in the state specified by <paramref name="state"/>. The method
        /// does not perform any other checks related to the initial or final state of the load balancer.
        /// </remarks>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="state">A <see cref="LoadBalancerStatus"/> representing the state the load balancer should <em>not</em> be in at the end of the wait operation.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <param name="progress">An optional callback object to receive progress notifications. If this is <see langword="null"/>, no progress notifications are sent.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. When the operation
        /// completes successfully, the <see cref="Task{TResult}.Result"/> property will contain a
        /// <see cref="LoadBalancer"/> object representing the load balancer. In addition, the load
        /// <see cref="LoadBalancer.Status"/> property of the load balancer will <em>not</em> be
        /// equal to <paramref name="state"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="loadBalancerId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="state"/> is <see langword="null"/>.</para>
        /// </exception>
        protected Task<LoadBalancer> WaitForLoadBalancerToLeaveStateAsync(LoadBalancerId loadBalancerId, LoadBalancerStatus state, CancellationToken cancellationToken, IProgress<LoadBalancer> progress)
        {
            if (loadBalancerId == null)
                throw new ArgumentNullException("loadBalancerId");
            if (state == null)
                throw new ArgumentNullException("state");

            TaskCompletionSource<LoadBalancer> taskCompletionSource = new TaskCompletionSource<LoadBalancer>();
            Func<Task<LoadBalancer>> pollLoadBalancer = () => PollLoadBalancerStateAsync(loadBalancerId, cancellationToken, progress);

            IEnumerator<TimeSpan> backoffPolicy = BackoffPolicy.GetBackoffIntervals().GetEnumerator();
            Func<Task<LoadBalancer>> moveNext =
                () =>
                {
                    if (!backoffPolicy.MoveNext())
                        throw new OperationCanceledException();

                    if (backoffPolicy.Current == TimeSpan.Zero)
                    {
                        return pollLoadBalancer();
                    }
                    else
                    {
                        return Task.Factory.StartNewDelayed((int)backoffPolicy.Current.TotalMilliseconds, cancellationToken)
                            .Then(task => pollLoadBalancer());
                    }
                };

            Task<LoadBalancer> currentTask = moveNext();
            Action<Task<LoadBalancer>> continuation = null;
            continuation =
                previousTask =>
                {
                    if (previousTask.Status != TaskStatus.RanToCompletion)
                    {
                        taskCompletionSource.SetFromTask(previousTask);
                        return;
                    }

                    LoadBalancer result = previousTask.Result;
                    if (result == null || result.Status != state)
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
        /// Asynchronously poll the current state of a load balancer.
        /// </summary>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <param name="progress">An optional callback object to receive progress notifications. If this is <see langword="null"/>, no progress notifications are sent.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. When
        /// the task completes successfully, the <see cref="Task{TResult}.Result"/>
        /// property will contain a <see cref="LoadBalancer"/> object containing the
        /// updated state information for the load balancer.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="loadBalancerId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        private Task<LoadBalancer> PollLoadBalancerStateAsync(LoadBalancerId loadBalancerId, CancellationToken cancellationToken, IProgress<LoadBalancer> progress)
        {
            Task<LoadBalancer> chain = GetLoadBalancerAsync(loadBalancerId, cancellationToken);
            chain = chain.Select(
                task =>
                {
                    if (task.Result == null || task.Result.Id != loadBalancerId)
                        throw new InvalidOperationException("Could not obtain status for load balancer");

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
        /// Creates a <see cref="Task"/> that will complete after a group of load balancers all leave a particular state.
        /// </summary>
        /// <remarks>
        /// The task is considered complete as soon as calls to <see cref="ILoadBalancerService.GetLoadBalancerAsync"/>
        /// indicates that none of the load balancers are in the state specified by <paramref name="state"/>. The method
        /// does not perform any other checks related to the initial or final state of the load balancers.
        /// </remarks>
        /// <param name="loadBalancerIds">The load balancer IDs. These are obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="state">A <see cref="LoadBalancerStatus"/> representing the state the load balancers should <em>not</em> be in at the end of the wait operation.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <param name="progress">An optional callback object to receive progress notifications. If this is <see langword="null"/>, no progress notifications are sent.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. When the operation
        /// completes successfully, the <see cref="Task{TResult}.Result"/> property will contain a
        /// collection of <see cref="LoadBalancer"/> objects representing the load balancers. In
        /// addition, the load <see cref="LoadBalancer.Status"/> property of the load balancer will
        /// <em>not</em> be equal to <paramref name="state"/> for <em>any</em> of the load balancer
        /// instances.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="loadBalancerIds"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="state"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">If <paramref name="loadBalancerIds"/> contains any <see langword="null"/> values.</exception>
        protected Task<LoadBalancer[]> WaitForLoadBalancersToLeaveStateAsync(LoadBalancerId[] loadBalancerIds, LoadBalancerStatus state, CancellationToken cancellationToken, IProgress<LoadBalancer[]> progress)
        {
            if (loadBalancerIds == null)
                throw new ArgumentNullException("loadBalancerIds");
            if (state == null)
                throw new ArgumentNullException("state");
            if (loadBalancerIds.Contains(null))
                throw new ArgumentException("loadBalancerIds cannot contain any null values");

            TaskCompletionSource<LoadBalancer[]> taskCompletionSource = new TaskCompletionSource<LoadBalancer[]>();
            Func<Task<LoadBalancer[]>> pollLoadBalancers =
                () =>
                {
                    Task<LoadBalancer>[] tasks = Array.ConvertAll(
                        loadBalancerIds,
                        loadBalancerId =>
                        {
                            return PollLoadBalancerStateAsync(loadBalancerId, cancellationToken, null);
                        });

                    return Task.Factory.WhenAll(tasks).Select(
                        completedTasks =>
                        {
                            LoadBalancer[] loadBalancers = Array.ConvertAll(completedTasks.Result, completedTask => completedTask.Result);
                            if (progress != null)
                                progress.Report(loadBalancers);

                            return loadBalancers;
                        });
                };

            IEnumerator<TimeSpan> backoffPolicy = BackoffPolicy.GetBackoffIntervals().GetEnumerator();
            Func<Task<LoadBalancer[]>> moveNext =
                () =>
                {
                    if (!backoffPolicy.MoveNext())
                        throw new OperationCanceledException();

                    if (backoffPolicy.Current == TimeSpan.Zero)
                    {
                        return pollLoadBalancers();
                    }
                    else
                    {
                        return Task.Factory.StartNewDelayed((int)backoffPolicy.Current.TotalMilliseconds, cancellationToken)
                            .Then(task => pollLoadBalancers());
                    }
                };

            Task<LoadBalancer[]> currentTask = moveNext();
            Action<Task<LoadBalancer[]>> continuation = null;
            continuation =
                previousTask =>
                {
                    if (previousTask.Status != TaskStatus.RanToCompletion)
                    {
                        taskCompletionSource.SetFromTask(previousTask);
                        return;
                    }

                    LoadBalancer[] results = previousTask.Result;
                    if (results.All(result => result == null || result.Status == state))
                    {
                        // finished waiting
                        taskCompletionSource.SetResult(results);
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

        /// <inheritdoc/>
        /// <remarks>
        /// This method returns a cached base address if one is available. If no cached address is
        /// available, <see cref="ProviderBase{TProvider}.GetServiceEndpoint"/> is called to obtain
        /// an <see cref="Endpoint"/> with the type <c>rax:load-balancer</c> and preferred type <c>cloudLoadBalancers</c>.
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
                    Endpoint endpoint = GetServiceEndpoint(null, "rax:load-balancer", "cloudLoadBalancers", null);
                    _baseUri = new Uri(endpoint.PublicURL);
                    return _baseUri;
                });
        }

        /// <summary>
        /// This class provides a wrapper implementation of <see cref="IProgress{T}"/> which
        /// wraps a single progress report values into a single-element array.
        /// </summary>
        /// <typeparam name="T">The type of progress update value.</typeparam>
        private class ArrayElementProgressWrapper<T> : IProgress<T>
        {
            /// <summary>
            /// The delegate progress handler to dispatch progress reports to.
            /// </summary>
            private readonly IProgress<T[]> _delegate;

            /// <summary>
            /// Initializes a new instance of the <see cref="ArrayElementProgressWrapper{T}"/> class
            /// that dispatches progress reports to the specified delegate. The reported progress
            /// values are wrapped in a single-element array.
            /// </summary>
            /// <param name="delegate">The delegate to dispatch progress reports to.</param>
            /// <exception cref="ArgumentNullException">If <paramref name="delegate"/> is <see langword="null"/>.</exception>
            public ArrayElementProgressWrapper(IProgress<T[]> @delegate)
            {
                if (@delegate == null)
                    throw new ArgumentNullException("delegate");

                _delegate = @delegate;
            }

            /// <inheritdoc/>
            public void Report(T value)
            {
                _delegate.Report(new T[] { value });
            }
        }
    }
}
