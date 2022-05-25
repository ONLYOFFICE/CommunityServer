namespace net.openstack.Providers.Rackspace
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using JSIStudios.SimpleRESTServices.Client;
    using JSIStudios.SimpleRESTServices.Client.Json;
    using net.openstack.Core;
    using net.openstack.Core.Collections;
    using net.openstack.Core.Domain;
    using net.openstack.Core.Domain.Queues;
    using net.openstack.Core.Providers;
    using net.openstack.Providers.Rackspace.Objects.Queues.Request;
    using net.openstack.Providers.Rackspace.Objects.Queues.Response;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using HttpResponseCodeValidator = net.openstack.Providers.Rackspace.Validators.HttpResponseCodeValidator;
    using IHttpResponseCodeValidator = net.openstack.Core.Validators.IHttpResponseCodeValidator;

    /// <summary>
    /// Provides an implementation of <see cref="IQueueingService"/> for operating
    /// with Rackspace's Cloud Queues product.
    /// </summary>
    /// <seealso href="https://wiki.openstack.org/w/index.php?title=Marconi/specs/api/v1">OpenStack Marconi API v1 Blueprint</seealso>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    public class CloudQueuesProvider : ProviderBase<IQueueingService>, IQueueingService
    {
        /// <summary>
        /// Specifies whether the <see cref="Endpoint.PublicURL"/> or <see cref="Endpoint.InternalURL"/>
        /// should be used for accessing the Cloud Queues API.
        /// </summary>
        private readonly bool _internalUrl;

        /// <summary>
        /// The value of the Client-Id header to send with message requests from this service.
        /// </summary>
        private readonly Guid _clientId;

        /// <summary>
        /// This field caches the base URI used for accessing the Cloud Queues service.
        /// </summary>
        /// <seealso cref="GetBaseUriAsync"/>
        private Uri _baseUri;

        /// <summary>
        /// This field caches the home document returned by <see cref="GetHomeAsync"/>.
        /// </summary>
        private HomeDocument _homeDocument;

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudQueuesProvider"/> class with
        /// the specified values.
        /// </summary>
        /// <param name="defaultIdentity">The default identity to use for calls that do not explicitly specify an identity. If this value is <see langword="null"/>, the <paramref name="identityProvider"/> provides the default identity.</param>
        /// <param name="defaultRegion">The default region to use for calls that do not explicitly specify a region. If this value is <see langword="null"/>, the default region for the user will be used; otherwise if the service uses region-specific endpoints all calls must specify an explicit region.</param>
        /// <param name="clientId">The value of the <strong>Client-Id</strong> header to send with message requests from this service.</param>
        /// <param name="internalUrl"><see langword="true"/> to use the endpoint's <see cref="Endpoint.InternalURL"/>; otherwise <see langword="false"/> to use the endpoint's <see cref="Endpoint.PublicURL"/>.</param>
        /// <param name="identityProvider">The identity provider to use for authenticating requests to this provider. If this value is <see langword="null"/>, a new instance of <see cref="CloudIdentityProvider"/> is created using <paramref name="defaultIdentity"/> as the default identity.</param>
        /// <exception cref="ArgumentException">If both <paramref name="defaultIdentity"/> and <paramref name="identityProvider"/> are <see langword="null"/>.</exception>
        public CloudQueuesProvider(CloudIdentity defaultIdentity, string defaultRegion, Guid clientId, bool internalUrl, IIdentityProvider identityProvider)
            : this(defaultIdentity, defaultRegion, clientId, internalUrl, identityProvider, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudQueuesProvider"/> class with
        /// the specified values.
        /// </summary>
        /// <remarks>
        /// <note type="inherit">
        /// The default implementation does not rely on <paramref name="restService"/> or
        /// <paramref name="httpStatusCodeValidator"/> for any services. If a derived class
        /// uses synchronous methods for any web API calls, the constructor for the derived
        /// class can either specify or expose these parameters.
        /// </note>
        /// </remarks>
        /// <param name="defaultIdentity">The default identity to use for calls that do not explicitly specify an identity. If this value is <see langword="null"/>, the <paramref name="identityProvider"/> provides the default identity.</param>
        /// <param name="defaultRegion">The default region to use for calls that do not explicitly specify a region. If this value is <see langword="null"/>, the default region for the user will be used; otherwise if the service uses region-specific endpoints all calls must specify an explicit region.</param>
        /// <param name="clientId">The value of the <strong>Client-Id</strong> header to send with message requests from this service.</param>
        /// <param name="internalUrl"><see langword="true"/> to use the endpoint's <see cref="Endpoint.InternalURL"/>; otherwise <see langword="false"/> to use the endpoint's <see cref="Endpoint.PublicURL"/>.</param>
        /// <param name="identityProvider">The identity provider to use for authenticating requests to this provider. If this value is <see langword="null"/>, a new instance of <see cref="CloudIdentityProvider"/> is created using <paramref name="defaultIdentity"/> as the default identity.</param>
        /// <param name="restService">The implementation of <see cref="IRestService"/> to use for executing synchronous REST requests. If this value is <see langword="null"/>, the provider will use a new instance of <see cref="JsonRestServices"/>.</param>
        /// <param name="httpStatusCodeValidator">The HTTP status code validator to use for synchronous REST requests. If this value is <see langword="null"/>, the provider will use <see cref="HttpResponseCodeValidator.Default"/>.</param>
        /// <exception cref="ArgumentException">If both <paramref name="defaultIdentity"/> and <paramref name="identityProvider"/> are <see langword="null"/>.</exception>
        protected CloudQueuesProvider(CloudIdentity defaultIdentity, string defaultRegion, Guid clientId, bool internalUrl, IIdentityProvider identityProvider, IRestService restService, IHttpResponseCodeValidator httpStatusCodeValidator)
            : base(defaultIdentity, defaultRegion, identityProvider, restService, httpStatusCodeValidator)
        {
            if (defaultIdentity == null && identityProvider == null)
                throw new ArgumentException("defaultIdentity and identityProvider cannot both be null");

            _clientId = clientId;
            _internalUrl = internalUrl;
        }

        #region IQueueingService Members

        /// <inheritdoc/>
        public Task<HomeDocument> GetHomeAsync(CancellationToken cancellationToken)
        {
            if (_homeDocument != null)
            {
                return InternalTaskExtensions.CompletedTask(_homeDocument);
            }

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/");
            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, new Dictionary<string, string>());

            Func<Task<HttpWebRequest>, Task<HomeDocument>> requestResource =
                GetResponseAsyncFunc<HomeDocument>(cancellationToken);

            Func<Task<HomeDocument>, HomeDocument> cacheResult =
                task =>
                {
                    _homeDocument = task.Result;
                    return task.Result;
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(cacheResult);
        }

        /// <inheritdoc/>
        public Task GetNodeHealthAsync(CancellationToken cancellationToken)
        {
            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/health");
            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.HEAD, template, new Dictionary<string, string>());

            Func<Task<HttpWebRequest>, Task<string>> requestResource =
                GetResponseAsyncFunc(cancellationToken);

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource);
        }

        /// <inheritdoc/>
        public Task<bool> CreateQueueAsync(QueueName queueName, CancellationToken cancellationToken)
        {
            if (queueName == null)
                throw new ArgumentNullException("queueName");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/queues/{queue_name}");
            var parameters = new Dictionary<string, string>
                {
                    { "queue_name", queueName.Value }
                };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.PUT, template, parameters);

            Func<Task<Tuple<HttpWebResponse, string>>, Task<bool>> parseResult =
                task =>
                {
                    if (task.Result.Item1.StatusCode == HttpStatusCode.Created)
                        return InternalTaskExtensions.CompletedTask(true);
                    else
                        return InternalTaskExtensions.CompletedTask(false);
                };

            Func<Task<HttpWebRequest>, Task<bool>> requestResource =
                GetResponseAsyncFunc(cancellationToken, parseResult);

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource);
        }

        /// <inheritdoc/>
        public Task<ReadOnlyCollectionPage<CloudQueue>> ListQueuesAsync(QueueName marker, int? limit, bool detailed, CancellationToken cancellationToken)
        {
            if (limit <= 0)
                throw new ArgumentOutOfRangeException("limit");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/queues?marker={marker}&limit={limit}&detailed={detailed}");
            var parameters = new Dictionary<string, string>
                {
                    { "detailed", detailed.ToString().ToLowerInvariant() },
                };
            if (marker != null)
                parameters.Add("marker", marker.Value);
            if (limit.HasValue)
                parameters.Add("limit", limit.ToString());

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<ListCloudQueuesResponse>> requestResource =
                GetResponseAsyncFunc<ListCloudQueuesResponse>(cancellationToken);

            Func<Task<ListCloudQueuesResponse>, ReadOnlyCollectionPage<CloudQueue>> resultSelector =
                task =>
                {
                    ReadOnlyCollectionPage<CloudQueue> page = null;
                    if (task.Result != null && task.Result.Queues != null)
                    {
                        CloudQueue lastQueue = task.Result.Queues.LastOrDefault();
                        QueueName nextMarker = lastQueue != null ? lastQueue.Name : marker;
                        Func<CancellationToken, Task<ReadOnlyCollectionPage<CloudQueue>>> getNextPageAsync =
                            nextCancellationToken => ListQueuesAsync(nextMarker, limit, detailed, nextCancellationToken);
                        page = new BasicReadOnlyCollectionPage<CloudQueue>(task.Result.Queues, getNextPageAsync);
                    }

                    return page ?? ReadOnlyCollectionPage<CloudQueue>.Empty;
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task<bool> QueueExistsAsync(QueueName queueName, CancellationToken cancellationToken)
        {
            if (queueName == null)
                throw new ArgumentNullException("queueName");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/queues/{queue_name}");
            var parameters = new Dictionary<string, string>() { { "queue_name", queueName.Value } };
            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.HEAD, template, parameters);

            Func<Task<HttpWebRequest>, Task<string>> requestResource =
                GetResponseAsyncFunc(cancellationToken);

            Func<Task<string>, bool> interpretResult =
                task =>
                {
                    // a WebException would have been thrown if the queue didn't exist
                    if (task.Status == TaskStatus.RanToCompletion)
                        return true;

                    task.Exception.Flatten().Handle(
                        ex =>
                        {
                            WebException webException = ex as WebException;
                            if (webException == null)
                                return false;

                            HttpWebResponse response = webException.Response as HttpWebResponse;
                            if (response == null)
                                return false;

                            if (response.StatusCode != HttpStatusCode.NotFound)
                                return false;

                            // a 404 error means the queue does not exist
                            return true;
                        });

                    return false;
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(interpretResult, true);
        }

        /// <inheritdoc/>
        public Task DeleteQueueAsync(QueueName queueName, CancellationToken cancellationToken)
        {
            if (queueName == null)
                throw new ArgumentNullException("queueName");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/queues/{queue_name}");
            var parameters = new Dictionary<string, string>
                {
                    { "queue_name", queueName.Value }
                };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.DELETE, template, parameters);

            Func<Task<HttpWebRequest>, Task<string>> requestResource =
                GetResponseAsyncFunc(cancellationToken);

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource);
        }

        /// <inheritdoc/>
        public Task SetQueueMetadataAsync<T>(QueueName queueName, T metadata, CancellationToken cancellationToken)
            where T : class
        {
            if (queueName == null)
                throw new ArgumentNullException("queueName");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/queues/{queue_name}/metadata");
            var parameters = new Dictionary<string, string>
                {
                    { "queue_name", queueName.Value }
                };

            Func<Task<Tuple<IdentityToken, Uri>>, Task<HttpWebRequest>> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.PUT, template, parameters, metadata);

            Func<Task<HttpWebRequest>, Task<string>> requestResource =
                GetResponseAsyncFunc(cancellationToken);

            return AuthenticateServiceAsync(cancellationToken)
                .Then(prepareRequest)
                .Then(requestResource);
        }

        /// <inheritdoc/>
        public Task<JObject> GetQueueMetadataAsync(QueueName queueName, CancellationToken cancellationToken)
        {
            if (queueName == null)
                throw new ArgumentNullException("queueName");

            return GetQueueMetadataAsync<JObject>(queueName, cancellationToken);
        }

        /// <inheritdoc/>
        public Task<T> GetQueueMetadataAsync<T>(QueueName queueName, CancellationToken cancellationToken)
            where T : class
        {
            if (queueName == null)
                throw new ArgumentNullException("queueName");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/queues/{queue_name}/metadata");
            var parameters = new Dictionary<string, string>
                {
                    { "queue_name", queueName.Value }
                };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<T>> requestResource =
                GetResponseAsyncFunc<T>(cancellationToken);

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource);
        }

        /// <inheritdoc/>
        public Task<QueueStatistics> GetQueueStatisticsAsync(QueueName queueName, CancellationToken cancellationToken)
        {
            if (queueName == null)
                throw new ArgumentNullException("queueName");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/queues/{queue_name}/stats");
            var parameters = new Dictionary<string, string>() { { "queue_name", queueName.Value } };
            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<QueueStatistics>> requestResource =
                GetResponseAsyncFunc<QueueStatistics>(cancellationToken);

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource);
        }

        /// <inheritdoc/>
        public Task<QueuedMessageList> ListMessagesAsync(QueueName queueName, QueuedMessageListId marker, int? limit, bool echo, bool includeClaimed, CancellationToken cancellationToken)
        {
            if (queueName == null)
                throw new ArgumentNullException("queueName");
            if (limit <= 0)
                throw new ArgumentOutOfRangeException("limit");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/queues/{queue_name}/messages?marker={marker}&limit={limit}&echo={echo}&include_claimed={include_claimed}");

            var parameters =
                new Dictionary<string, string>()
                {
                    { "queue_name", queueName.Value },
                    { "echo", echo.ToString() },
                    { "include_claimed", includeClaimed.ToString() }
                };
            if (marker != null)
                parameters["marker"] = marker.Value;
            if (limit != null)
                parameters["limit"] = limit.ToString();

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<ListCloudQueueMessagesResponse>> requestResource =
                GetResponseAsyncFunc<ListCloudQueueMessagesResponse>(cancellationToken);

            Func<Task<ListCloudQueueMessagesResponse>, QueuedMessageList> resultSelector =
                task =>
                {
                    ReadOnlyCollection<QueuedMessage> messages = null;
                    if (task.Result != null)
                        messages = task.Result.Messages;

                    QueuedMessageListId nextMarker = null;
                    if (task.Result != null && task.Result.Links != null)
                    {
                        Link nextLink = task.Result.Links.FirstOrDefault(i => string.Equals(i.Rel, "next", StringComparison.OrdinalIgnoreCase));
                        if (nextLink != null)
                        {
                            Uri baseUri = new Uri("https://example.com");
                            Uri absoluteUri;
                            if (nextLink.Href.StartsWith("/v1"))
                                absoluteUri = new Uri(baseUri, nextLink.Href.Substring("/v1".Length));
                            else
                                absoluteUri = new Uri(baseUri, nextLink.Href);

                            UriTemplate.UriTemplateMatch match = template.Match(baseUri, absoluteUri);
                            if (!string.IsNullOrEmpty(match.BoundVariables["marker"]))
                                nextMarker = new QueuedMessageListId(match.BoundVariables["marker"]);
                        }
                    }

                    if (messages == null || messages.Count == 0)
                    {
                        // use the same marker again
                        messages = messages ?? new ReadOnlyCollection<QueuedMessage>(new QueuedMessage[0]);
                        nextMarker = marker;
                    }

                    Func<CancellationToken, Task<ReadOnlyCollectionPage<QueuedMessage>>> getNextPageAsync = null;
                    if (nextMarker != null || messages.Count == 0)
                    {
                        getNextPageAsync =
                            nextCancellationToken => ListMessagesAsync(queueName, nextMarker, limit, echo, includeClaimed, nextCancellationToken)
                                .Select(t => (ReadOnlyCollectionPage<QueuedMessage>)t.Result);
                    }

                    return new QueuedMessageList(messages, getNextPageAsync, nextMarker);
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task<QueuedMessage> GetMessageAsync(QueueName queueName, MessageId messageId, CancellationToken cancellationToken)
        {
            if (queueName == null)
                throw new ArgumentNullException("queueName");
            if (messageId == null)
                throw new ArgumentNullException("messageId");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/queues/{queue_name}/messages/{message_id}");

            var parameters =
                new Dictionary<string, string>()
                {
                    { "queue_name", queueName.Value },
                    { "message_id", messageId.Value },
                };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<QueuedMessage>> requestResource =
                GetResponseAsyncFunc<QueuedMessage>(cancellationToken);

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource);
        }

        /// <inheritdoc/>
        public Task<ReadOnlyCollection<QueuedMessage>> GetMessagesAsync(QueueName queueName, IEnumerable<MessageId> messageIds, CancellationToken cancellationToken)
        {
            if (queueName == null)
                throw new ArgumentNullException("queueName");
            if (messageIds == null)
                throw new ArgumentNullException("messageIds");
            if (messageIds.Contains(null))
                throw new ArgumentException("messageIds cannot contain any null values");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/queues/{queue_name}/messages?ids={ids}");

            var parameters =
                new Dictionary<string, string>()
                {
                    { "queue_name", queueName.Value },
                    { "ids", string.Join(",", messageIds.Select(i => i.Value).ToArray()) },
                };

            Func<Uri, Uri> uriTransform =
                uri => new Uri(uri.GetLeftPart(UriPartial.Path) + uri.Query.Replace("%2c", ","));

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters, uriTransform);

            Func<Task<HttpWebRequest>, Task<ReadOnlyCollection<QueuedMessage>>> requestResource =
                GetResponseAsyncFunc<ReadOnlyCollection<QueuedMessage>>(cancellationToken);

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource);
        }

        /// <inheritdoc/>
        public Task<MessagesEnqueued> PostMessagesAsync(QueueName queueName, IEnumerable<Message> messages, CancellationToken cancellationToken)
        {
            if (queueName == null)
                throw new ArgumentNullException("queueName");
            if (messages == null)
                throw new ArgumentNullException("messages");

            return PostMessagesAsync(queueName, cancellationToken, messages.ToArray());
        }

        /// <inheritdoc/>
        public Task<MessagesEnqueued> PostMessagesAsync(QueueName queueName, CancellationToken cancellationToken, params Message[] messages)
        {
            if (queueName == null)
                throw new ArgumentNullException("queueName");
            if (messages == null)
                throw new ArgumentNullException("messages");
            if (messages.Contains(null))
                throw new ArgumentException("messages cannot contain any null values");

            if (messages.Length == 0)
            {
                return QueueExistsAsync(queueName, cancellationToken)
                    .Select(_ => MessagesEnqueued.Empty);
            }

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/queues/{queue_name}/messages");

            var parameters =
                new Dictionary<string, string>()
                {
                    { "queue_name", queueName.Value },
                };

            Func<Task<Tuple<IdentityToken, Uri>>, Task<HttpWebRequest>> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.POST, template, parameters, messages);

            Func<Task<HttpWebRequest>, Task<MessagesEnqueued>> requestResource =
                GetResponseAsyncFunc<MessagesEnqueued>(cancellationToken);

            return AuthenticateServiceAsync(cancellationToken)
                .Then(prepareRequest)
                .Then(requestResource);
        }

        /// <inheritdoc/>
        public Task<MessagesEnqueued> PostMessagesAsync<T>(QueueName queueName, IEnumerable<Message<T>> messages, CancellationToken cancellationToken)
        {
            if (queueName == null)
                throw new ArgumentNullException("queueName");
            if (messages == null)
                throw new ArgumentNullException("messages");

            return PostMessagesAsync(queueName, cancellationToken, messages.ToArray());
        }

        /// <inheritdoc/>
        public Task<MessagesEnqueued> PostMessagesAsync<T>(QueueName queueName, CancellationToken cancellationToken, params Message<T>[] messages)
        {
            if (queueName == null)
                throw new ArgumentNullException("queueName");
            if (messages == null)
                throw new ArgumentNullException("messages");
            if (messages.Contains(null))
                throw new ArgumentException("messages cannot contain any null values");

            if (messages.Length == 0)
            {
                return QueueExistsAsync(queueName, cancellationToken)
                    .Select(_ => MessagesEnqueued.Empty);
            }

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/queues/{queue_name}/messages");

            var parameters =
                new Dictionary<string, string>()
                {
                    { "queue_name", queueName.Value },
                };

            Func<Task<Tuple<IdentityToken, Uri>>, Task<HttpWebRequest>> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.POST, template, parameters, messages);

            Func<Task<HttpWebRequest>, Task<MessagesEnqueued>> requestResource =
                GetResponseAsyncFunc<MessagesEnqueued>(cancellationToken);

            return AuthenticateServiceAsync(cancellationToken)
                .Then(prepareRequest)
                .Then(requestResource);
        }

        /// <inheritdoc/>
        public Task DeleteMessageAsync(QueueName queueName, MessageId messageId, Claim claim, CancellationToken cancellationToken)
        {
            if (queueName == null)
                throw new ArgumentNullException("queueName");
            if (messageId == null)
                throw new ArgumentNullException("messageId");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/queues/{queue_name}/messages/{message_id}?claim_id={claim_id}");

            var parameters =
                new Dictionary<string, string>()
                {
                    { "queue_name", queueName.Value },
                    { "message_id", messageId.Value },
                };
            if (claim != null)
                parameters["claim_id"] = claim.Id.Value;

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.DELETE, template, parameters);

            Func<Task<HttpWebRequest>, Task<string>> requestResource =
                GetResponseAsyncFunc(cancellationToken);

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource);
        }

        /// <inheritdoc/>
        public Task DeleteMessagesAsync(QueueName queueName, IEnumerable<MessageId> messageIds, CancellationToken cancellationToken)
        {
            if (queueName == null)
                throw new ArgumentNullException("queueName");
            if (messageIds == null)
                throw new ArgumentNullException("messageIds");
            if (messageIds.Contains(null))
                throw new ArgumentException("messageIds cannot contain any null values");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/queues/{queue_name}/messages?ids={ids}");

            var parameters =
                new Dictionary<string, string>()
                {
                    { "queue_name", queueName.Value },
                    { "ids", string.Join(",", messageIds.Select(i => i.Value).ToArray()) },
                };

            Func<Uri, Uri> uriTransform =
                uri => new Uri(uri.GetLeftPart(UriPartial.Path) + uri.Query.Replace("%2c", ","));

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.DELETE, template, parameters, uriTransform);

            Func<Task<HttpWebRequest>, Task<string>> requestResource =
                GetResponseAsyncFunc(cancellationToken);

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource);
        }

        /// <inheritdoc/>
        public Task<Claim> ClaimMessageAsync(QueueName queueName, int? limit, TimeSpan timeToLive, TimeSpan gracePeriod, CancellationToken cancellationToken)
        {
            if (queueName == null)
                throw new ArgumentNullException("queueName");
            if (limit < 0)
                throw new ArgumentOutOfRangeException("limit");
            if (timeToLive <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException("timeToLive");
            if (gracePeriod < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException("gracePeriod");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/queues/{queue_name}/claims?limit={limit}");

            var parameters =
                new Dictionary<string, string>()
                {
                    { "queue_name", queueName.Value },
                };
            if (limit != null)
                parameters["limit"] = limit.ToString();

            var request = new ClaimMessagesRequest(timeToLive, gracePeriod);

            Func<Task<Tuple<IdentityToken, Uri>>, Task<HttpWebRequest>> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.POST, template, parameters, request);

            Func<Task<Tuple<HttpWebResponse, string>>, Task<Tuple<Uri, IEnumerable<QueuedMessage>>>> parseResult =
                task =>
                {
                    string location = task.Result.Item1.Headers[HttpResponseHeader.Location];
                    Uri locationUri = location != null ? new Uri(_baseUri, location) : null;

                    if (task.Result.Item1.StatusCode == HttpStatusCode.NoContent)
                    {
                        // the queue did not contain any messages to claim
                        Tuple<Uri, IEnumerable<QueuedMessage>> result = Tuple.Create(locationUri, Enumerable.Empty<QueuedMessage>());
                        return InternalTaskExtensions.CompletedTask(result);
                    }

                    IEnumerable<QueuedMessage> messages = JsonConvert.DeserializeObject<IEnumerable<QueuedMessage>>(task.Result.Item2);

                    return InternalTaskExtensions.CompletedTask(Tuple.Create(locationUri, messages));
                };
            Func<Task<HttpWebRequest>, Task<Tuple<Uri, IEnumerable<QueuedMessage>>>> requestResource =
                GetResponseAsyncFunc<Tuple<Uri, IEnumerable<QueuedMessage>>>(cancellationToken, parseResult);

            Func<Task<Tuple<Uri, IEnumerable<QueuedMessage>>>, Claim> resultSelector =
                task => new Claim(this, queueName, task.Result.Item1, timeToLive, TimeSpan.Zero, true, task.Result.Item2);

            return AuthenticateServiceAsync(cancellationToken)
                .Then(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task<Claim> QueryClaimAsync(QueueName queueName, Claim claim, CancellationToken cancellationToken)
        {
            if (queueName == null)
                throw new ArgumentNullException("queueName");
            if (claim == null)
                throw new ArgumentNullException("claim");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/queues/{queue_name}/claims/{claim_id}");

            var parameters =
                new Dictionary<string, string>()
                {
                    { "queue_name", queueName.Value },
                    { "claim_id", claim.Id.Value }
                };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<Tuple<HttpWebResponse, string>>, Task<Tuple<Uri, TimeSpan, TimeSpan, IEnumerable<QueuedMessage>>>> parseResult =
                task =>
                {
                    // this response uses ContentLocation instead of Location
                    string location = task.Result.Item1.Headers[HttpResponseHeader.ContentLocation];
                    Uri locationUri = location != null ? new Uri(_baseUri, location) : null;

                    JObject result = JsonConvert.DeserializeObject<JObject>(task.Result.Item2);
                    TimeSpan age = TimeSpan.FromSeconds((int)result["age"]);
                    TimeSpan ttl = TimeSpan.FromSeconds((int)result["ttl"]);
                    IEnumerable<QueuedMessage> messages = result["messages"].ToObject<IEnumerable<QueuedMessage>>();
                    return InternalTaskExtensions.CompletedTask(Tuple.Create(locationUri, ttl, age, messages));
                };
            Func<Task<HttpWebRequest>, Task<Tuple<Uri, TimeSpan, TimeSpan, IEnumerable<QueuedMessage>>>> requestResource =
                GetResponseAsyncFunc(cancellationToken, parseResult);

            Func<Task<Tuple<Uri, TimeSpan, TimeSpan, IEnumerable<QueuedMessage>>>, Claim> resultSelector =
                task => new Claim(this, queueName, task.Result.Item1, task.Result.Item2, task.Result.Item3, false, task.Result.Item4);

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task UpdateClaimAsync(QueueName queueName, Claim claim, TimeSpan timeToLive, CancellationToken cancellationToken)
        {
            if (queueName == null)
                throw new ArgumentNullException("queueName");
            if (claim == null)
                throw new ArgumentNullException("claim");
            if (timeToLive <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException("timeToLive");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/queues/{queue_name}/claims/{claim_id}");

            var parameters =
                new Dictionary<string, string>()
                {
                    { "queue_name", queueName.Value },
                    { "claim_id", claim.Id.Value }
                };

            JObject request = new JObject(new JProperty("ttl", new JValue((int)timeToLive.TotalSeconds)));
            Func<Task<Tuple<IdentityToken, Uri>>, Task<HttpWebRequest>> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.PATCH, template, parameters, request);

            Func<Task<HttpWebRequest>, Task<string>> requestResource =
                GetResponseAsyncFunc(cancellationToken);

            return AuthenticateServiceAsync(cancellationToken)
                .Then(prepareRequest)
                .Then(requestResource);
        }

        /// <inheritdoc/>
        public Task ReleaseClaimAsync(QueueName queueName, Claim claim, CancellationToken cancellationToken)
        {
            if (queueName == null)
                throw new ArgumentNullException("queueName");
            if (claim == null)
                throw new ArgumentNullException("claim");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/queues/{queue_name}/claims/{claim_id}");

            var parameters =
                new Dictionary<string, string>()
                {
                    { "queue_name", queueName.Value },
                    { "claim_id", claim.Id.Value },
                };

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
        /// an <see cref="Endpoint"/> with the type <c>rax:queues</c> and preferred type <c>cloudQueues</c>.
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
                    Endpoint endpoint = GetServiceEndpoint(null, "rax:queues", "cloudQueues", null);
                    Uri baseUri = new Uri(_internalUrl ? endpoint.InternalURL : endpoint.PublicURL);
                    _baseUri = baseUri;
                    return baseUri;
                });
        }

        /// <inheritdoc/>
        /// <remarks>
        /// This method calls <see cref="ProviderBase{TProvider}.PrepareRequestImpl"/> to create the
        /// initial <see cref="HttpWebRequest"/>, and then sets the <c>Client-Id</c> header according
        /// to the Marconi (Cloud Queues) specification before returning.
        /// </remarks>
        protected override HttpWebRequest PrepareRequestImpl(HttpMethod method, IdentityToken identityToken, UriTemplate.UriTemplate template, Uri baseUri, IDictionary<string, string> parameters, Func<Uri, Uri> uriTransform)
        {
            HttpWebRequest request = base.PrepareRequestImpl(method, identityToken, template, baseUri, parameters, uriTransform);
            request.Headers["Client-Id"] = _clientId.ToString("D");
            return request;
        }
    }
}
