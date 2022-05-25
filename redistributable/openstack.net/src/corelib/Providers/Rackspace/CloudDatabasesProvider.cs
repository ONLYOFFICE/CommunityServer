namespace net.openstack.Providers.Rackspace
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Net;
    using System.Threading.Tasks;
    using net.openstack.Core;
    using net.openstack.Core.Collections;
    using net.openstack.Core.Domain;
    using net.openstack.Core.Providers;
    using net.openstack.Providers.Rackspace.Objects.Databases;
    using Newtonsoft.Json.Linq;
    using CancellationToken = System.Threading.CancellationToken;
    using FlavorId = net.openstack.Providers.Rackspace.Objects.Databases.FlavorId;
    using HttpMethod = JSIStudios.SimpleRESTServices.Client.HttpMethod;
    using HttpResponseCodeValidator = net.openstack.Providers.Rackspace.Validators.HttpResponseCodeValidator;
    using IHttpResponseCodeValidator = net.openstack.Core.Validators.IHttpResponseCodeValidator;
    using IRestService = JSIStudios.SimpleRESTServices.Client.IRestService;
    using JsonRestServices = JSIStudios.SimpleRESTServices.Client.Json.JsonRestServices;

    /// <summary>
    /// Provides an implementation of <see cref="IDatabaseService"/> for operating
    /// with Rackspace's Cloud Databases product.
    /// </summary>
    /// <seealso href="http://docs.rackspace.com/cdb/api/v1.0/cdb-devguide/content/overview.html">Rackspace Cloud Databases Developer Guide - API v1.0</seealso>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    public class CloudDatabasesProvider : ProviderBase<IDatabaseService>, IDatabaseService
    {
        /// <summary>
        /// This field caches the base URI used for accessing the Cloud Databases service.
        /// </summary>
        /// <seealso cref="GetBaseUriAsync"/>
        private Uri _baseUri;

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudDatabasesProvider"/> class with
        /// the specified values.
        /// </summary>
        /// <param name="defaultIdentity">The default identity to use for calls that do not explicitly specify an identity. If this value is <see langword="null"/>, no default identity is available so all calls must specify an explicit identity.</param>
        /// <param name="defaultRegion">The default region to use for calls that do not explicitly specify a region. If this value is <see langword="null"/>, the default region for the user will be used; otherwise if the service uses region-specific endpoints all calls must specify an explicit region.</param>
        /// <param name="identityProvider">The identity provider to use for authenticating requests to this provider. If this value is <see langword="null"/>, a new instance of <see cref="CloudIdentityProvider"/> is created using <paramref name="defaultIdentity"/> as the default identity.</param>
        public CloudDatabasesProvider(CloudIdentity defaultIdentity, string defaultRegion, IIdentityProvider identityProvider)
            : base(defaultIdentity, defaultRegion, identityProvider, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudDatabasesProvider"/> class with
        /// the specified values.
        /// </summary>
        /// <param name="defaultIdentity">The default identity to use for calls that do not explicitly specify an identity. If this value is <see langword="null"/>, no default identity is available so all calls must specify an explicit identity.</param>
        /// <param name="defaultRegion">The default region to use for calls that do not explicitly specify a region. If this value is <see langword="null"/>, the default region for the user will be used; otherwise if the service uses region-specific endpoints all calls must specify an explicit region.</param>
        /// <param name="identityProvider">The identity provider to use for authenticating requests to this provider. If this value is <see langword="null"/>, a new instance of <see cref="CloudIdentityProvider"/> is created using <paramref name="defaultIdentity"/> as the default identity.</param>
        /// <param name="restService">The implementation of <see cref="IRestService"/> to use for executing synchronous REST requests. If this value is <see langword="null"/>, the provider will use a new instance of <see cref="JsonRestServices"/>.</param>
        /// <param name="httpStatusCodeValidator">The HTTP status code validator to use for synchronous REST requests. If this value is <see langword="null"/>, the provider will use <see cref="HttpResponseCodeValidator.Default"/>.</param>
        protected CloudDatabasesProvider(CloudIdentity defaultIdentity, string defaultRegion, IIdentityProvider identityProvider, IRestService restService, IHttpResponseCodeValidator httpStatusCodeValidator)
            : base(defaultIdentity, defaultRegion, identityProvider, restService, httpStatusCodeValidator)
        {
        }

        #region IDatabaseService Members

        /// <inheritdoc/>
        public Task<DatabaseInstance> CreateDatabaseInstanceAsync(DatabaseInstanceConfiguration configuration, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<DatabaseInstance> progress)
        {
            if (configuration == null)
                throw new ArgumentNullException("configuration");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/instances");
            var parameters = new Dictionary<string, string>();

            JObject requestBody = new JObject(
                new JProperty("instance", JObject.FromObject(configuration)));

            Func<Task<Tuple<IdentityToken, Uri>>, Task<HttpWebRequest>> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.POST, template, parameters, requestBody);

            Func<Task<HttpWebRequest>, Task<JObject>> requestResource =
                GetResponseAsyncFunc<JObject>(cancellationToken);

            Func<Task<JObject>, DatabaseInstance> resultSelector =
                task =>
                {
                    JObject result = task.Result;
                    if (result == null)
                        return null;

                    JToken flavor = result["instance"];
                    if (flavor == null)
                        return null;

                    return flavor.ToObject<DatabaseInstance>();
                };

            Func<Task<DatabaseInstance>, Task<DatabaseInstance>> completionSelector =
                task =>
                {
                    DatabaseInstance databaseInstance = task.Result;
                    if (databaseInstance != null && completionOption == AsyncCompletionOption.RequestCompleted)
                        return WaitForDatabaseInstanceToLeaveStateAsync(databaseInstance.Id, DatabaseInstanceStatus.Build, cancellationToken, progress);

                    return InternalTaskExtensions.CompletedTask(databaseInstance);
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Then(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector)
                .Then(completionSelector);
        }

        /// <inheritdoc/>
        public Task<ReadOnlyCollectionPage<DatabaseInstance>> ListDatabaseInstancesAsync(DatabaseInstanceId marker, int? limit, CancellationToken cancellationToken)
        {
            if (limit <= 0)
                throw new ArgumentOutOfRangeException("limit");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/instances?marker={marker}&limit={limit}");
            var parameters = new Dictionary<string, string>();
            if (marker != null)
                parameters.Add("marker", marker.Value);
            if (limit != null)
                parameters.Add("limit", limit.ToString());

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<JObject>> requestResource =
                GetResponseAsyncFunc<JObject>(cancellationToken);

            Func<Task<JObject>, ReadOnlyCollectionPage<DatabaseInstance>> resultSelector =
                task =>
                {
                    JObject result = task.Result;
                    if (result == null)
                        return null;

                    JToken instances = result["instances"];
                    if (instances == null)
                        return null;

                    DatabaseInstance[] currentPage = instances.ToObject<DatabaseInstance[]>();
                    if (currentPage == null || currentPage.Length == 0)
                        return ReadOnlyCollectionPage<DatabaseInstance>.Empty;

                    DatabaseInstanceId nextMarker = currentPage[currentPage.Length - 1].Id;
                    Func<CancellationToken, Task<ReadOnlyCollectionPage<DatabaseInstance>>> getNextPageAsync =
                        nextCancellationToken => ListDatabaseInstancesAsync(nextMarker, limit, nextCancellationToken);
                    return new BasicReadOnlyCollectionPage<DatabaseInstance>(currentPage, getNextPageAsync);
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task<DatabaseInstance> GetDatabaseInstanceAsync(DatabaseInstanceId instanceId, CancellationToken cancellationToken)
        {
            if (instanceId == null)
                throw new ArgumentNullException("instanceId");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/instances/{instanceId}");
            var parameters = new Dictionary<string, string> { { "instanceId", instanceId.Value } };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<JObject>> requestResource =
                GetResponseAsyncFunc<JObject>(cancellationToken);

            Func<Task<JObject>, DatabaseInstance> resultSelector =
                task =>
                {
                    JObject result = task.Result;
                    if (result == null)
                        return null;

                    JToken flavor = result["instance"];
                    if (flavor == null)
                        return null;

                    return flavor.ToObject<DatabaseInstance>();
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task RemoveDatabaseInstanceAsync(DatabaseInstanceId instanceId, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<DatabaseInstance> progress)
        {
            if (instanceId == null)
                throw new ArgumentNullException("instanceId");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/instances/{instanceId}");
            var parameters = new Dictionary<string, string> { { "instanceId", instanceId.Value } };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.DELETE, template, parameters);

            Func<Task<HttpWebRequest>, Task<string>> requestResource =
                GetResponseAsyncFunc(cancellationToken);

            Func<Task<string>, Task<DatabaseInstance>> completionSelector =
                task =>
                {
                    string databaseInstance = task.Result;
                    if (databaseInstance != null && completionOption == AsyncCompletionOption.RequestCompleted)
                        return WaitForDatabaseInstanceToLeaveStateAsync(instanceId, DatabaseInstanceStatus.Shutdown, cancellationToken, progress);

                    return InternalTaskExtensions.CompletedTask(default(DatabaseInstance));
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Then(completionSelector);
        }

        /// <inheritdoc/>
        public Task<RootUser> EnableRootUserAsync(DatabaseInstanceId instanceId, CancellationToken cancellationToken)
        {
            if (instanceId == null)
                throw new ArgumentNullException("instanceId");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/instances/{instanceId}/root");
            var parameters = new Dictionary<string, string> { { "instanceId", instanceId.Value } };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.POST, template, parameters);

            Func<Task<HttpWebRequest>, Task<JObject>> requestResource =
                GetResponseAsyncFunc<JObject>(cancellationToken);

            Func<Task<JObject>, RootUser> resultSelector =
                task =>
                {
                    JObject result = task.Result;
                    if (result == null)
                        return null;

                    JToken user = result["user"];
                    if (user == null)
                        return null;

                    return user.ToObject<RootUser>();
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task<bool?> CheckRootEnabledStatusAsync(DatabaseInstanceId instanceId, CancellationToken cancellationToken)
        {
            if (instanceId == null)
                throw new ArgumentNullException("instanceId");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/instances/{instanceId}/root");
            var parameters = new Dictionary<string, string> { { "instanceId", instanceId.Value } };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<JObject>> requestResource =
                GetResponseAsyncFunc<JObject>(cancellationToken);

            Func<Task<JObject>, bool?> resultSelector =
                task =>
                {
                    JObject result = task.Result;
                    if (result == null)
                        return null;

                    JToken rootEnabled = result["rootEnabled"];
                    if (rootEnabled == null)
                        return null;

                    return rootEnabled.ToObject<bool?>();
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task RestartDatabaseInstanceAsync(DatabaseInstanceId instanceId, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<DatabaseInstance> progress)
        {
            if (instanceId == null)
                throw new ArgumentNullException("instanceId");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/instances/{instanceId}/action");
            var parameters = new Dictionary<string, string> { { "instanceId", instanceId.Value } };

            JObject requestBody =
                new JObject(
                    new JProperty("restart", new JObject()));

            Func<Task<Tuple<IdentityToken, Uri>>, Task<HttpWebRequest>> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.POST, template, parameters, requestBody);

            Func<Task<HttpWebRequest>, Task<string>> requestResource =
                GetResponseAsyncFunc(cancellationToken);

            Func<Task<string>, Task<DatabaseInstance>> resultSelector =
                task =>
                {
                    if (completionOption == AsyncCompletionOption.RequestCompleted)
                        return WaitForDatabaseInstanceToLeaveStateAsync(instanceId, DatabaseInstanceStatus.Reboot, cancellationToken, progress);

                    return InternalTaskExtensions.CompletedTask(default(DatabaseInstance));
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Then(prepareRequest)
                .Then(requestResource)
                .Then(resultSelector);
        }

        /// <inheritdoc/>
        public Task ResizeDatabaseInstanceAsync(DatabaseInstanceId instanceId, FlavorRef flavorRef, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<DatabaseInstance> progress)
        {
            if (instanceId == null)
                throw new ArgumentNullException("instanceId");
            if (flavorRef == null)
                throw new ArgumentNullException("flavorRef");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/instances/{instanceId}/action");
            var parameters = new Dictionary<string, string> { { "instanceId", instanceId.Value } };

            JObject requestBody =
                new JObject(
                    new JProperty("resize", new JObject(
                        new JProperty("flavorRef", JValue.FromObject(flavorRef.Value)))));

            Func<Task<Tuple<IdentityToken, Uri>>, Task<HttpWebRequest>> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.POST, template, parameters, requestBody);

            Func<Task<HttpWebRequest>, Task<string>> requestResource =
                GetResponseAsyncFunc(cancellationToken);

            Func<Task<string>, Task<DatabaseInstance>> resultSelector =
                task =>
                {
                    if (completionOption == AsyncCompletionOption.RequestCompleted)
                        return WaitForDatabaseInstanceToLeaveStateAsync(instanceId, DatabaseInstanceStatus.Resize, cancellationToken, progress);

                    return InternalTaskExtensions.CompletedTask(default(DatabaseInstance));
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Then(prepareRequest)
                .Then(requestResource)
                .Then(resultSelector);
        }

        /// <inheritdoc/>
        public Task ResizeDatabaseInstanceVolumeAsync(DatabaseInstanceId instanceId, int volumeSize, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<DatabaseInstance> progress)
        {
            if (instanceId == null)
                throw new ArgumentNullException("instanceId");
            if (volumeSize <= 0)
                throw new ArgumentOutOfRangeException("volumeSize");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/instances/{instanceId}/action");
            var parameters = new Dictionary<string, string> { { "instanceId", instanceId.Value } };

            JObject requestBody =
                new JObject(
                    new JProperty("resize", new JObject(
                        new JProperty("volume", new JObject(
                            new JProperty("size", JValue.FromObject(volumeSize)))))));

            Func<Task<Tuple<IdentityToken, Uri>>, Task<HttpWebRequest>> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.POST, template, parameters, requestBody);

            Func<Task<HttpWebRequest>, Task<string>> requestResource =
                GetResponseAsyncFunc(cancellationToken);

            Func<Task<string>, Task<DatabaseInstance>> resultSelector =
                task =>
                {
                    if (completionOption == AsyncCompletionOption.RequestCompleted)
                        return WaitForDatabaseInstanceToLeaveStateAsync(instanceId, DatabaseInstanceStatus.Reboot, cancellationToken, progress);

                    return InternalTaskExtensions.CompletedTask(default(DatabaseInstance));
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Then(prepareRequest)
                .Then(requestResource)
                .Then(resultSelector);
        }

        /// <inheritdoc/>
        public Task CreateDatabaseAsync(DatabaseInstanceId instanceId, DatabaseConfiguration configuration, CancellationToken cancellationToken)
        {
            if (instanceId == null)
                throw new ArgumentNullException("instanceId");
            if (configuration == null)
                throw new ArgumentNullException("configuration");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/instances/{instanceId}/databases");
            var parameters = new Dictionary<string, string> { { "instanceId", instanceId.Value } };

            JObject requestBody =
                new JObject(
                    new JProperty("databases", new JArray(
                        JObject.FromObject(configuration))));

            Func<Task<Tuple<IdentityToken, Uri>>, Task<HttpWebRequest>> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.POST, template, parameters, requestBody);

            Func<Task<HttpWebRequest>, Task<string>> requestResource =
                GetResponseAsyncFunc(cancellationToken);

            return AuthenticateServiceAsync(cancellationToken)
                .Then(prepareRequest)
                .Then(requestResource);
        }

        /// <inheritdoc/>
        public Task<ReadOnlyCollectionPage<Database>> ListDatabasesAsync(DatabaseInstanceId instanceId, DatabaseName marker, int? limit, CancellationToken cancellationToken)
        {
            if (instanceId == null)
                throw new ArgumentNullException("instanceId");
            if (limit <= 0)
                throw new ArgumentOutOfRangeException("limit");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/instances/{instanceId}/databases?marker={marker}&limit={limit}");
            var parameters = new Dictionary<string, string> { { "instanceId", instanceId.Value } };
            if (marker != null)
                parameters.Add("marker", marker.Value);
            if (limit != null)
                parameters.Add("limit", limit.ToString());

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<JObject>> requestResource =
                GetResponseAsyncFunc<JObject>(cancellationToken);

            Func<Task<JObject>, ReadOnlyCollectionPage<Database>> resultSelector =
                task =>
                {
                    JObject result = task.Result;
                    if (result == null)
                        return null;

                    JToken databases = result["databases"];
                    if (databases == null)
                        return null;

                    Database[] currentPage = databases.ToObject<Database[]>();
                    if (currentPage == null || currentPage.Length == 0)
                        return ReadOnlyCollectionPage<Database>.Empty;

                    DatabaseName nextMarker = currentPage[currentPage.Length - 1].Name;
                    Func<CancellationToken, Task<ReadOnlyCollectionPage<Database>>> getNextPageAsync =
                        nextCancellationToken => ListDatabasesAsync(instanceId, nextMarker, limit, nextCancellationToken);
                    return new BasicReadOnlyCollectionPage<Database>(currentPage, getNextPageAsync);
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task RemoveDatabaseAsync(DatabaseInstanceId instanceId, DatabaseName databaseName, CancellationToken cancellationToken)
        {
            if (instanceId == null)
                throw new ArgumentNullException("instanceId");
            if (databaseName == null)
                throw new ArgumentNullException("databaseName");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/instances/{instanceId}/databases/{databaseName}");
            var parameters = new Dictionary<string, string> { { "instanceId", instanceId.Value }, { "databaseName", EscapeDatabaseName(databaseName) } };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.DELETE, template, parameters);

            Func<Task<HttpWebRequest>, Task<string>> requestResource =
                GetResponseAsyncFunc(cancellationToken);

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource);
        }

        /// <inheritdoc/>
        public Task CreateUserAsync(DatabaseInstanceId instanceId, UserConfiguration configuration, CancellationToken cancellationToken)
        {
            if (instanceId == null)
                throw new ArgumentNullException("instanceId");
            if (configuration == null)
                throw new ArgumentNullException("configuration");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/instances/{instanceId}/users");
            var parameters = new Dictionary<string, string> { { "instanceId", instanceId.Value } };

            JObject requestBody =
                new JObject(
                    new JProperty("users", new JArray(
                        JObject.FromObject(configuration))));

            Func<Task<Tuple<IdentityToken, Uri>>, Task<HttpWebRequest>> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.POST, template, parameters, requestBody);

            Func<Task<HttpWebRequest>, Task<string>> requestResource =
                GetResponseAsyncFunc(cancellationToken);

            return AuthenticateServiceAsync(cancellationToken)
                .Then(prepareRequest)
                .Then(requestResource);
        }

        /// <inheritdoc/>
        public Task<ReadOnlyCollectionPage<DatabaseUser>> ListDatabaseUsersAsync(DatabaseInstanceId instanceId, UserName marker, int? limit, CancellationToken cancellationToken)
        {
            if (instanceId == null)
                throw new ArgumentNullException("instanceId");
            if (limit <= 0)
                throw new ArgumentOutOfRangeException("limit");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/instances/{instanceId}/users?marker={marker}&limit={limit}");
            var parameters = new Dictionary<string, string> { { "instanceId", instanceId.Value } };
            if (marker != null)
                parameters.Add("marker", marker.Value);
            if (limit != null)
                parameters.Add("limit", limit.ToString());

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<JObject>> requestResource =
                GetResponseAsyncFunc<JObject>(cancellationToken);

            Func<Task<JObject>, ReadOnlyCollectionPage<DatabaseUser>> resultSelector =
                task =>
                {
                    JObject result = task.Result;
                    if (result == null)
                        return null;

                    JToken users = result["users"];
                    if (users == null)
                        return null;

                    DatabaseUser[] currentPage = users.ToObject<DatabaseUser[]>();
                    if (currentPage == null || currentPage.Length == 0)
                        return ReadOnlyCollectionPage<DatabaseUser>.Empty;

                    UserName nextMarker = currentPage[currentPage.Length - 1].UserName;
                    Func<CancellationToken, Task<ReadOnlyCollectionPage<DatabaseUser>>> getNextPageAsync =
                        nextCancellationToken => ListDatabaseUsersAsync(instanceId, nextMarker, limit, nextCancellationToken);
                    return new BasicReadOnlyCollectionPage<DatabaseUser>(currentPage, getNextPageAsync);
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task SetUserPasswordAsync(DatabaseInstanceId instanceId, UserName userName, string password, CancellationToken cancellationToken)
        {
            if (instanceId == null)
                throw new ArgumentNullException("instanceId");
            if (userName == null)
                throw new ArgumentNullException("userName");
            if (password == null)
                throw new ArgumentNullException("password");
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("password cannot be empty");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/instances/{instanceId}/users");
            var parameters = new Dictionary<string, string> { { "instanceId", instanceId.Value } };

            JObject requestBody =
                new JObject(
                    new JProperty("users", new JArray(
                        new JObject(
                            new JProperty("name", JValue.CreateString(userName.Value)),
                            new JProperty("password", JValue.CreateString(password))))));

            Func<Task<Tuple<IdentityToken, Uri>>, Task<HttpWebRequest>> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.PUT, template, parameters, requestBody);

            Func<Task<HttpWebRequest>, Task<string>> requestResource =
                GetResponseAsyncFunc(cancellationToken);

            return AuthenticateServiceAsync(cancellationToken)
                .Then(prepareRequest)
                .Then(requestResource);
        }

        /// <inheritdoc/>
        public Task UpdateUserAsync(DatabaseInstanceId instanceId, UserName userName, UpdateUserConfiguration configuration, CancellationToken cancellationToken)
        {
            if (instanceId == null)
                throw new ArgumentNullException("instanceId");
            if (userName == null)
                throw new ArgumentNullException("userName");
            if (configuration == null)
                throw new ArgumentNullException("configuration");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/instances/{instanceId}/users/{name}");
            var parameters = new Dictionary<string, string> { { "instanceId", instanceId.Value }, { "name", EscapeUserName(userName) } };

            JObject requestBody =
                new JObject(
                    new JProperty("user", JObject.FromObject(configuration)));

            Func<Task<Tuple<IdentityToken, Uri>>, Task<HttpWebRequest>> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.PUT, template, parameters, requestBody);

            Func<Task<HttpWebRequest>, Task<string>> requestResource =
                GetResponseAsyncFunc(cancellationToken);

            return AuthenticateServiceAsync(cancellationToken)
                .Then(prepareRequest)
                .Then(requestResource);
        }

        /// <inheritdoc/>
        public Task<DatabaseUser> GetUserAsync(DatabaseInstanceId instanceId, UserName userName, CancellationToken cancellationToken)
        {
            if (instanceId == null)
                throw new ArgumentNullException("instanceId");
            if (userName == null)
                throw new ArgumentNullException("userName");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/instances/{instanceId}/users/{name}");
            var parameters = new Dictionary<string, string> { { "instanceId", instanceId.Value }, { "name", EscapeUserName(userName) } };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<JObject>> requestResource =
                GetResponseAsyncFunc<JObject>(cancellationToken);

            Func<Task<JObject>, DatabaseUser> resultSelector =
                task =>
                {
                    JObject result = task.Result;
                    if (result == null)
                        return null;

                    JToken user = result["user"];
                    if (user == null)
                        return null;

                    return user.ToObject<DatabaseUser>();
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task RemoveUserAsync(DatabaseInstanceId instanceId, UserName userName, CancellationToken cancellationToken)
        {
            if (instanceId == null)
                throw new ArgumentNullException("instanceId");
            if (userName == null)
                throw new ArgumentNullException("userName");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/instances/{instanceId}/users/{name}");
            var parameters = new Dictionary<string, string> { { "instanceId", instanceId.Value }, { "name", EscapeUserName(userName) } };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.DELETE, template, parameters);

            Func<Task<HttpWebRequest>, Task<string>> requestResource =
                GetResponseAsyncFunc(cancellationToken);

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource);
        }

        /// <inheritdoc/>
        public Task<ReadOnlyCollection<DatabaseName>> ListUserAccessAsync(DatabaseInstanceId instanceId, UserName userName, CancellationToken cancellationToken)
        {
            if (instanceId == null)
                throw new ArgumentNullException("instanceId");
            if (userName == null)
                throw new ArgumentNullException("userName");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/instances/{instanceId}/users/{name}/databases");
            var parameters = new Dictionary<string, string> { { "instanceId", instanceId.Value }, { "name", EscapeUserName(userName) } };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<JObject>> requestResource =
                GetResponseAsyncFunc<JObject>(cancellationToken);

            Func<Task<JObject>, ReadOnlyCollection<DatabaseName>> resultSelector =
                task =>
                {
                    JObject result = task.Result;
                    if (result == null)
                        return null;

                    JArray databases = result["databases"] as JArray;
                    if (databases == null)
                        return null;

                    List<DatabaseName> names = new List<DatabaseName>();
                    foreach (JObject @obj in databases)
                        names.Add(@obj["name"].ToObject<DatabaseName>());

                    return names.AsReadOnly();
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task GrantUserAccessAsync(DatabaseInstanceId instanceId, DatabaseName databaseName, UserName userName, CancellationToken cancellationToken)
        {
            if (instanceId == null)
                throw new ArgumentNullException("instanceId");
            if (databaseName == null)
                throw new ArgumentNullException("databaseName");
            if (userName == null)
                throw new ArgumentNullException("userName");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/instances/{instanceId}/users/{name}/databases");
            var parameters = new Dictionary<string, string> { { "instanceId", instanceId.Value }, { "name", EscapeUserName(userName) } };

            JObject requestBody =
                new JObject(
                    new JProperty("databases", new JArray(
                        new JObject(
                            new JProperty("name", JToken.FromObject(databaseName))))));

            Func<Task<Tuple<IdentityToken, Uri>>, Task<HttpWebRequest>> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.PUT, template, parameters, requestBody);

            Func<Task<HttpWebRequest>, Task<JObject>> requestResource =
                GetResponseAsyncFunc<JObject>(cancellationToken);

            return AuthenticateServiceAsync(cancellationToken)
                .Then(prepareRequest)
                .Then(requestResource);
        }

        /// <inheritdoc/>
        public Task RevokeUserAccessAsync(DatabaseInstanceId instanceId, DatabaseName databaseName, UserName userName, CancellationToken cancellationToken)
        {
            if (instanceId == null)
                throw new ArgumentNullException("instanceId");
            if (databaseName == null)
                throw new ArgumentNullException("databaseName");
            if (userName == null)
                throw new ArgumentNullException("userName");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/instances/{instanceId}/users/{name}/databases/{databaseName}");
            var parameters = new Dictionary<string, string>
            {
                { "instanceId", instanceId.Value },
                { "name", EscapeUserName(userName) },
                { "databaseName", EscapeDatabaseName(databaseName) },
            };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.DELETE, template, parameters);

            Func<Task<HttpWebRequest>, Task<JObject>> requestResource =
                GetResponseAsyncFunc<JObject>(cancellationToken);

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource);
        }

        /// <inheritdoc/>
        public Task<ReadOnlyCollection<DatabaseFlavor>> ListFlavorsAsync(CancellationToken cancellationToken)
        {
            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/flavors");
            var parameters = new Dictionary<string, string>();

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<JObject>> requestResource =
                GetResponseAsyncFunc<JObject>(cancellationToken);

            Func<Task<JObject>, ReadOnlyCollection<DatabaseFlavor>> resultSelector =
                task =>
                {
                    JObject result = task.Result;
                    if (result == null)
                        return null;

                    JToken records = result["flavors"];
                    if (records == null)
                        return null;

                    return records.ToObject<ReadOnlyCollection<DatabaseFlavor>>();
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task<DatabaseFlavor> GetFlavorAsync(FlavorId flavorId, CancellationToken cancellationToken)
        {
            if (flavorId == null)
                throw new ArgumentNullException("flavorId");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/flavors/{flavorId}");
            var parameters = new Dictionary<string, string> { { "flavorId", flavorId.Value } };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<JObject>> requestResource =
                GetResponseAsyncFunc<JObject>(cancellationToken);

            Func<Task<JObject>, DatabaseFlavor> resultSelector =
                task =>
                {
                    JObject result = task.Result;
                    if (result == null)
                        return null;

                    JToken flavor = result["flavor"];
                    if (flavor == null)
                        return null;

                    return flavor.ToObject<DatabaseFlavor>();
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task<Backup> CreateBackupAsync(BackupConfiguration configuration, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<DatabaseInstance> progress)
        {
            if (configuration == null)
                throw new ArgumentNullException("configuration");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/backups");
            var parameters = new Dictionary<string, string>();

            JObject requestBody =
                new JObject(
                    new JProperty("backup", JObject.FromObject(configuration)));

            Func<Task<Tuple<IdentityToken, Uri>>, Task<HttpWebRequest>> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.POST, template, parameters, requestBody);

            Func<Task<HttpWebRequest>, Task<JObject>> requestResource =
                GetResponseAsyncFunc<JObject>(cancellationToken);

            Func<Task<JObject>, Backup> resultSelector =
                task =>
                {
                    JObject result = task.Result;
                    if (result == null)
                        return null;

                    JToken backup = result["backup"];
                    if (backup == null)
                        return null;

                    return backup.ToObject<Backup>();
                };

            Func<Task<Backup>, Task<Backup>> completionSelector =
                task =>
                {
                    Func<Task<DatabaseInstance>, Task<Backup>> finalResultSelector =
                        subTask => GetBackupAsync(task.Result.Id, cancellationToken);

                    if (completionOption == AsyncCompletionOption.RequestCompleted)
                    {
                        return WaitForDatabaseInstanceToLeaveStateAsync(configuration.InstanceId, DatabaseInstanceStatus.Backup, cancellationToken, progress)
                            .Then(finalResultSelector);
                    }

                    return InternalTaskExtensions.CompletedTask(task.Result);
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Then(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector)
                .Then(completionSelector);
        }

        /// <inheritdoc/>
        public Task<ReadOnlyCollection<Backup>> ListBackupsAsync(CancellationToken cancellationToken)
        {
            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/backups");
            var parameters = new Dictionary<string, string>();

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<JObject>> requestResource =
                GetResponseAsyncFunc<JObject>(cancellationToken);

            Func<Task<JObject>, ReadOnlyCollection<Backup>> resultSelector =
                task =>
                {
                    JObject result = task.Result;
                    if (result == null)
                        return null;

                    JToken backups = result["backups"];
                    if (backups == null)
                        return null;

                    return backups.ToObject<ReadOnlyCollection<Backup>>();
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task<Backup> GetBackupAsync(BackupId backupId, CancellationToken cancellationToken)
        {
            if (backupId == null)
                throw new ArgumentNullException("backupId");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/backups/{backupId}");
            var parameters = new Dictionary<string, string> { { "backupId", backupId.Value } };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<JObject>> requestResource =
                GetResponseAsyncFunc<JObject>(cancellationToken);

            Func<Task<JObject>, Backup> resultSelector =
                task =>
                {
                    JObject result = task.Result;
                    if (result == null)
                        return null;

                    JToken backup = result["backup"];
                    if (backup == null)
                        return null;

                    return backup.ToObject<Backup>();
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        /// <inheritdoc/>
        public Task RemoveBackupAsync(BackupId backupId, CancellationToken cancellationToken)
        {
            if (backupId == null)
                throw new ArgumentNullException("backupId");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/backups/{backupId}");
            var parameters = new Dictionary<string, string> { { "backupId", backupId.Value } };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.DELETE, template, parameters);

            Func<Task<HttpWebRequest>, Task<string>> requestResource =
                GetResponseAsyncFunc(cancellationToken);

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource);
        }

        /// <inheritdoc/>
        public Task<ReadOnlyCollection<Backup>> ListBackupsForInstanceAsync(DatabaseInstanceId instanceId, CancellationToken cancellationToken)
        {
            if (instanceId == null)
                throw new ArgumentNullException("instanceId");

            UriTemplate.UriTemplate template = new UriTemplate.UriTemplate("/instances/{instanceId}/backups");
            var parameters = new Dictionary<string, string> { { "instanceId", instanceId.Value } };

            Func<Task<Tuple<IdentityToken, Uri>>, HttpWebRequest> prepareRequest =
                PrepareRequestAsyncFunc(HttpMethod.GET, template, parameters);

            Func<Task<HttpWebRequest>, Task<JObject>> requestResource =
                GetResponseAsyncFunc<JObject>(cancellationToken);

            Func<Task<JObject>, ReadOnlyCollection<Backup>> resultSelector =
                task =>
                {
                    JObject result = task.Result;
                    if (result == null)
                        return null;

                    JToken backups = result["backups"];
                    if (backups == null)
                        return null;

                    return backups.ToObject<ReadOnlyCollection<Backup>>();
                };

            return AuthenticateServiceAsync(cancellationToken)
                .Select(prepareRequest)
                .Then(requestResource)
                .Select(resultSelector);
        }

        #endregion

        /// <summary>
        /// Creates a <see cref="Task"/> that will complete after a database instance leaves a particular state.
        /// </summary>
        /// <remarks>
        /// The task is considered complete as soon as a call to <see cref="IDatabaseService.GetDatabaseInstanceAsync"/>
        /// indicates that the database instance is not in the state specified by <paramref name="state"/>. The method
        /// does not perform any other checks related to the initial or final state of the database instance.
        /// </remarks>
        /// <param name="instanceId">The database instance ID. This is obtained from <see cref="DatabaseInstance.Id">DatabaseInstance.Id</see>.</param>
        /// <param name="state">A <see cref="DatabaseInstanceStatus"/> representing the state the database instance should <em>not</em> be in at the end of the wait operation.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <param name="progress">An optional callback object to receive progress notifications. If this is <see langword="null"/>, no progress notifications are sent.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. When the operation
        /// completes successfully, the <see cref="Task{TResult}.Result"/> property will contain a
        /// <see cref="DatabaseInstance"/> object representing the database instance. In addition,
        /// the <see cref="DatabaseInstance.Status"/> property of the database instance will <em>not</em>
        /// be equal to <paramref name="state"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="instanceId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="state"/> is <see langword="null"/>.</para>
        /// </exception>
        protected Task<DatabaseInstance> WaitForDatabaseInstanceToLeaveStateAsync(DatabaseInstanceId instanceId, DatabaseInstanceStatus state, CancellationToken cancellationToken, IProgress<DatabaseInstance> progress)
        {
            if (instanceId == null)
                throw new ArgumentNullException("instanceId");
            if (state == null)
                throw new ArgumentNullException("state");

            TaskCompletionSource<DatabaseInstance> taskCompletionSource = new TaskCompletionSource<DatabaseInstance>();
            Func<Task<DatabaseInstance>> pollDatabaseInstance = () => PollDatabaseInstanceStateAsync(instanceId, cancellationToken, progress);

            IEnumerator<TimeSpan> backoffPolicy = BackoffPolicy.GetBackoffIntervals().GetEnumerator();
            Func<Task<DatabaseInstance>> moveNext =
                () =>
                {
                    if (!backoffPolicy.MoveNext())
                        throw new OperationCanceledException();

                    if (backoffPolicy.Current == TimeSpan.Zero)
                    {
                        return pollDatabaseInstance();
                    }
                    else
                    {
                        return Task.Factory.StartNewDelayed((int)backoffPolicy.Current.TotalMilliseconds, cancellationToken)
                            .Then(task => pollDatabaseInstance());
                    }
                };

            Task<DatabaseInstance> currentTask = moveNext();
            Action<Task<DatabaseInstance>> continuation = null;
            continuation =
                previousTask =>
                {
                    if (previousTask.Status != TaskStatus.RanToCompletion)
                    {
                        taskCompletionSource.SetFromTask(previousTask);
                        return;
                    }

                    DatabaseInstance result = previousTask.Result;
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
        /// Converts a database name to the form required by the Cloud Databases service
        /// by twice percent-encoding the <c>.</c> characters.
        /// </summary>
        /// <param name="databaseName">The database name to encode.</param>
        /// <returns>A string representation of the database name suitable for inclusion in the URI for a Cloud Databases API call.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="databaseName"/> is <see langword="null"/>.</exception>
        protected static string EscapeDatabaseName(DatabaseName databaseName)
        {
            return databaseName.Value.Replace(".", "%252e");
        }

        /// <summary>
        /// Converts a username to the form required by the Cloud Databases service
        /// by twice percent-encoding the <c>.</c> characters.
        /// </summary>
        /// <param name="username">The username to encode.</param>
        /// <returns>A string representation of the username suitable for inclusion in the URI for a Cloud Databases API call.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="username"/> is <see langword="null"/>.</exception>
        protected static string EscapeUserName(UserName username)
        {
            return username.Value.Replace(".", "%252e");
        }

        /// <summary>
        /// Asynchronously poll the current state of a database instance.
        /// </summary>
        /// <param name="instanceId">The database instance ID. This is obtained from <see cref="DatabaseInstance.Id">DatabaseInstance.Id</see>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <param name="progress">An optional callback object to receive progress notifications. If this is <see langword="null"/>, no progress notifications are sent.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. When
        /// the task completes successfully, the <see cref="Task{TResult}.Result"/>
        /// property will contain a <see cref="DatabaseInstance"/> object containing the
        /// updated state information for the database instance.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="instanceId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        private Task<DatabaseInstance> PollDatabaseInstanceStateAsync(DatabaseInstanceId instanceId, CancellationToken cancellationToken, IProgress<DatabaseInstance> progress)
        {
            Task<DatabaseInstance> chain = GetDatabaseInstanceAsync(instanceId, cancellationToken);
            chain = chain.Select(
                task =>
                {
                    if (task.IsFaulted)
                    {
                        AggregateException flattened = task.Exception.Flatten();
                        if (flattened.InnerExceptions.Count == 1)
                        {
                            WebException webException = flattened.InnerExceptions[0] as WebException;
                            HttpWebResponse httpWebResponse = webException != null ? webException.Response as HttpWebResponse : null;
                            if (httpWebResponse != null && httpWebResponse.StatusCode == HttpStatusCode.NotFound)
                                return null;
                        }
                    }

                    if (task.Result == null || task.Result.Id != instanceId)
                        throw new InvalidOperationException("Could not obtain status for database instance");

                    return task.Result;
                }, true);

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

        /// <inheritdoc/>
        /// <remarks>
        /// This method returns a cached base address if one is available. If no cached address is
        /// available, <see cref="ProviderBase{TProvider}.GetServiceEndpoint"/> is called to obtain
        /// an <see cref="Endpoint"/> with the type <c>rax:database</c> and preferred type <c>cloudDatabases</c>.
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
                    Endpoint endpoint = GetServiceEndpoint(null, "rax:database", "cloudDatabases", null);
                    _baseUri = new Uri(endpoint.PublicURL);
                    return _baseUri;
                });
        }
    }
}
