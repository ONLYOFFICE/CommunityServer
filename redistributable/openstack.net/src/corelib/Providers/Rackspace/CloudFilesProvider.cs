using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;

using JSIStudios.SimpleRESTServices.Client;
using JSIStudios.SimpleRESTServices.Client.Json;

using net.openstack.Core;
using net.openstack.Core.Domain;
using net.openstack.Core.Domain.Mapping;
using net.openstack.Core.Exceptions;
using net.openstack.Core.Exceptions.Response;
using net.openstack.Core.Providers;
using net.openstack.Core.Validators;
using net.openstack.Providers.Rackspace.Exceptions;
using net.openstack.Providers.Rackspace.Objects;
using net.openstack.Providers.Rackspace.Objects.Mapping;
using net.openstack.Providers.Rackspace.Objects.Monitoring;
using net.openstack.Providers.Rackspace.Objects.Response;
using net.openstack.Providers.Rackspace.Validators;

using Newtonsoft.Json;

namespace net.openstack.Providers.Rackspace
{
    /// <summary>
    /// Provides an implementation of <see cref="IObjectStorageProvider"/>
    /// for operating with Rackspace's Cloud Files product.
    /// </summary>
    /// <seealso href="http://docs.openstack.org/api/openstack-object-storage/1.0/content/">OpenStack Object Storage API v1 Reference</seealso>
    /// <seealso href="http://docs.rackspace.com/files/api/v1/cf-devguide/content/Overview-d1e70.html">Rackspace Cloud Files Developer Guide - API v1</seealso>
    /// <threadsafety static="true" instance="false"/>
    public class CloudFilesProvider : ProviderBase<IObjectStorageProvider>, IObjectStorageProvider
    {
        /// <summary>
        /// The <see cref="IObjectStorageValidator"/> to use for this provider. This is
        /// typically set to <see cref="CloudFilesValidator.Default"/>.
        /// </summary>
        private readonly IObjectStorageValidator _cloudFilesValidator;

        /// <summary>
        /// The <see cref="IObjectStorageMetadataProcessor"/> to use for this provider. This is
        /// typically set to <see cref="CloudFilesMetadataProcessor.Default"/>.
        /// </summary>
        private readonly IObjectStorageMetadataProcessor _cloudFilesMetadataProcessor;

        /// <summary>
        /// The <see cref="IEncodeDecodeProvider"/> to use for this provider. This is
        /// typically set to <see cref="EncodeDecodeProvider.Default"/>.
        /// </summary>
        private readonly IEncodeDecodeProvider _encodeDecodeProvider;

        /// <summary>
        /// The <see cref="IStatusParser"/> to use for this provider. This is
        /// typically set to <see cref="HttpStatusCodeParser.Default"/>.
        /// </summary>
        private readonly IStatusParser _statusParser;

        /// <summary>
        /// The <see cref="IObjectMapper{BulkDeleteResponse, BulkDeletionResults}"/> to use for
        /// this provider. This is typically set to a new instance of <see cref="BulkDeletionResultMapper"/>.
        /// </summary>
        private readonly IObjectMapper<BulkDeleteResponse, BulkDeletionResults> _bulkDeletionResultMapper;

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudFilesProvider"/> class with
        /// no default identity or region, and the default identity provider and REST service
        /// implementation.
        /// </summary>
        public CloudFilesProvider()
            : this(null, null, null, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudFilesProvider"/> class with
        /// the specified default identity, no default region, and the default identity provider
        /// and REST service implementation.
        /// </summary>
        /// <param name="defaultIdentity">The default identity to use for calls that do not explicitly specify an identity. If this value is <see langword="null"/>, no default identity is available so all calls must specify an explicit identity.</param>
        public CloudFilesProvider(CloudIdentity defaultIdentity)
            : this(defaultIdentity, null, null, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudFilesProvider"/> class with
        /// no default identity or region, the default identity provider, and the specified
        /// REST service implementation.
        /// </summary>
        /// <param name="restService">The implementation of <see cref="IRestService"/> to use for executing REST requests. If this value is <see langword="null"/>, the provider will use a new instance of <see cref="JsonRestServices"/>.</param>
        public CloudFilesProvider(IRestService restService)
            : this(null, null, null, restService) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudFilesProvider"/> class with
        /// no default identity or region, the specified identity provider, and the default
        /// REST service implementation.
        /// </summary>
        /// <param name="identityProvider">The identity provider to use for authenticating requests to this provider. If this value is <see langword="null"/>, a new instance of <see cref="CloudIdentityProvider"/> is created with no default identity.</param>
        public CloudFilesProvider(IIdentityProvider identityProvider)
            : this(null, null, identityProvider, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudFilesProvider"/> class with
        /// the specified default identity and identity provider, no default region, and
        /// the default REST service implementation.
        /// </summary>
        /// <param name="defaultIdentity">The default identity to use for calls that do not explicitly specify an identity. If this value is <see langword="null"/>, no default identity is available so all calls must specify an explicit identity.</param>
        /// <param name="identityProvider">The identity provider to use for authenticating requests to this provider. If this value is <see langword="null"/>, a new instance of <see cref="CloudIdentityProvider"/> is created using <paramref name="defaultIdentity"/> as the default identity.</param>
        public CloudFilesProvider(CloudIdentity defaultIdentity, IIdentityProvider identityProvider)
            : this(defaultIdentity, null, identityProvider, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudFilesProvider"/> class with
        /// the specified default identity and REST service implementation, no default region,
        /// and the default identity provider.
        /// </summary>
        /// <param name="defaultIdentity">The default identity to use for calls that do not explicitly specify an identity. If this value is <see langword="null"/>, no default identity is available so all calls must specify an explicit identity.</param>
        /// <param name="restService">The implementation of <see cref="IRestService"/> to use for executing REST requests. If this value is <see langword="null"/>, the provider will use a new instance of <see cref="JsonRestServices"/>.</param>
        public CloudFilesProvider(CloudIdentity defaultIdentity, IRestService restService)
            : this(defaultIdentity, null, null, restService) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudFilesProvider"/> class with
        /// the specified default identity, no default region, the specified identity
        /// provider and REST service implementation, and the default
        /// Rackspace-Cloud-Files-specific implementations of the object storage validator,
        /// metadata processor, encoder, status parser, and bulk delete results mapper.
        /// </summary>
        /// <param name="defaultIdentity">The default identity to use for calls that do not explicitly specify an identity. If this value is <see langword="null"/>, no default identity is available so all calls must specify an explicit identity.</param>
        /// <param name="identityProvider">The identity provider to use for authenticating requests to this provider. If this value is <see langword="null"/>, a new instance of <see cref="CloudIdentityProvider"/> is created using <paramref name="defaultIdentity"/> as the default identity.</param>
        /// <param name="restService">The implementation of <see cref="IRestService"/> to use for executing REST requests. If this value is <see langword="null"/>, the provider will use a new instance of <see cref="JsonRestServices"/>.</param>
        public CloudFilesProvider(CloudIdentity defaultIdentity, IIdentityProvider identityProvider, IRestService restService)
            : this(defaultIdentity, null, identityProvider, restService) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudFilesProvider"/> class with
        /// the specified default identity, default region, identity provider, and REST service
        /// implementation, and the default Rackspace-Cloud-Files-specific implementations of
        /// the object storage validator, metadata processor, encoder, status parser, and bulk
        /// delete results mapper.
        /// </summary>
        /// <param name="defaultIdentity">The default identity to use for calls that do not explicitly specify an identity. If this value is <see langword="null"/>, no default identity is available so all calls must specify an explicit identity.</param>
        /// <param name="defaultRegion">The default region to use for calls that do not explicitly specify a region. If this value is <see langword="null"/>, the default region for the user will be used; otherwise if the service uses region-specific endpoints all calls must specify an explicit region.</param>
        /// <param name="identityProvider">The identity provider to use for authenticating requests to this provider. If this value is <see langword="null"/>, a new instance of <see cref="CloudIdentityProvider"/> is created using <paramref name="defaultIdentity"/> as the default identity.</param>
        /// <param name="restService">The implementation of <see cref="IRestService"/> to use for executing REST requests. If this value is <see langword="null"/>, the provider will use a new instance of <see cref="JsonRestServices"/>.</param>
        public CloudFilesProvider(CloudIdentity defaultIdentity, string defaultRegion, IIdentityProvider identityProvider, IRestService restService)
            : this(defaultIdentity, defaultRegion, identityProvider, restService, CloudFilesValidator.Default, CloudFilesMetadataProcessor.Default, EncodeDecodeProvider.Default, HttpStatusCodeParser.Default, new BulkDeletionResultMapper(HttpStatusCodeParser.Default)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudFilesProvider"/> class with
        /// no default identity, the specified default region, and the default identity
        /// provider, REST service implementation, validator, metadata processor, encoder,
        /// status parser, and bulk delete results mapper.
        /// </summary>
        /// <param name="defaultRegion">The default region to use for calls that do not explicitly specify a region. If this value is <see langword="null"/>, the default region for the user will be used; otherwise if the service uses region-specific endpoints all calls must specify an explicit region.</param>
        /// <param name="identityProvider">The identity provider to use for authenticating requests to this provider. If this value is <see langword="null"/>, a new instance of <see cref="CloudIdentityProvider"/> is created with no default identity.</param>
        /// <param name="restService">The implementation of <see cref="IRestService"/> to use for executing REST requests. If this value is <see langword="null"/>, the provider will use a new instance of <see cref="JsonRestServices"/>.</param>
        /// <param name="cloudFilesValidator">The <see cref="IObjectStorageValidator"/> to use for validating requests to this service.</param>
        /// <param name="cloudFilesMetadataProcessor">The <see cref="IObjectStorageMetadataProcessor"/> to use for processing metadata returned in HTTP headers.</param>
        /// <param name="encodeDecodeProvider">The <see cref="IEncodeDecodeProvider"/> to use for encoding data in URI query strings.</param>
        /// <param name="statusParser">The <see cref="IStatusParser"/> to use for parsing HTTP status codes.</param>
        /// <param name="mapper">The object mapper to use for mapping <see cref="BulkDeleteResponse"/> objects to <see cref="BulkDeletionResults"/> objects.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="cloudFilesValidator"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="cloudFilesMetadataProcessor"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="encodeDecodeProvider"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="statusParser"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="mapper"/> is <see langword="null"/>.</para>
        /// </exception>
        internal CloudFilesProvider(string defaultRegion, IIdentityProvider identityProvider, IRestService restService, IObjectStorageValidator cloudFilesValidator, IObjectStorageMetadataProcessor cloudFilesMetadataProcessor, IEncodeDecodeProvider encodeDecodeProvider, IStatusParser statusParser, IObjectMapper<BulkDeleteResponse, BulkDeletionResults> mapper)
            : this(null, defaultRegion, identityProvider, restService, cloudFilesValidator, cloudFilesMetadataProcessor, encodeDecodeProvider, statusParser, mapper) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudFilesProvider"/> class with
        /// the specified default identity, default region, identity provider, REST service
        /// implementation, validator, metadata processor, encoder, status parser, and bulk
        /// delete results mapper.
        /// </summary>
        /// <param name="defaultIdentity">The default identity to use for calls that do not explicitly specify an identity. If this value is <see langword="null"/>, no default identity is available so all calls must specify an explicit identity.</param>
        /// <param name="defaultRegion">The default region to use for calls that do not explicitly specify a region. If this value is <see langword="null"/>, the default region for the user will be used; otherwise if the service uses region-specific endpoints all calls must specify an explicit region.</param>
        /// <param name="identityProvider">The identity provider to use for authenticating requests to this provider. If this value is <see langword="null"/>, a new instance of <see cref="CloudIdentityProvider"/> is created using <paramref name="defaultIdentity"/> as the default identity.</param>
        /// <param name="restService">The implementation of <see cref="IRestService"/> to use for executing REST requests. If this value is <see langword="null"/>, the provider will use a new instance of <see cref="JsonRestServices"/>.</param>
        /// <param name="cloudFilesValidator">The <see cref="IObjectStorageValidator"/> to use for validating requests to this service.</param>
        /// <param name="cloudFilesMetadataProcessor">The <see cref="IObjectStorageMetadataProcessor"/> to use for processing metadata returned in HTTP headers.</param>
        /// <param name="encodeDecodeProvider">The <see cref="IEncodeDecodeProvider"/> to use for encoding data in URI query strings.</param>
        /// <param name="statusParser">The <see cref="IStatusParser"/> to use for parsing HTTP status codes.</param>
        /// <param name="bulkDeletionResultMapper">The object mapper to use for mapping <see cref="BulkDeleteResponse"/> objects to <see cref="BulkDeletionResults"/> objects.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="cloudFilesValidator"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="cloudFilesMetadataProcessor"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="encodeDecodeProvider"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="statusParser"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="bulkDeletionResultMapper"/> is <see langword="null"/>.</para>
        /// </exception>
        internal CloudFilesProvider(CloudIdentity defaultIdentity, string defaultRegion, IIdentityProvider identityProvider, IRestService restService, IObjectStorageValidator cloudFilesValidator, IObjectStorageMetadataProcessor cloudFilesMetadataProcessor, IEncodeDecodeProvider encodeDecodeProvider, IStatusParser statusParser, IObjectMapper<BulkDeleteResponse, BulkDeletionResults> bulkDeletionResultMapper)
            : base(defaultIdentity, defaultRegion, identityProvider, restService)
        {
            if (cloudFilesValidator == null)
                throw new ArgumentNullException("cloudFilesValidator");
            if (cloudFilesMetadataProcessor == null)
                throw new ArgumentNullException("cloudFilesMetadataProcessor");
            if (encodeDecodeProvider == null)
                throw new ArgumentNullException("encodeDecodeProvider");
            if (statusParser == null)
                throw new ArgumentNullException("statusParser");
            if (bulkDeletionResultMapper == null)
                throw new ArgumentNullException("bulkDeletionResultMapper");

            _cloudFilesValidator = cloudFilesValidator;
            _cloudFilesMetadataProcessor = cloudFilesMetadataProcessor;
            _encodeDecodeProvider = encodeDecodeProvider;
            _statusParser = statusParser;
            _bulkDeletionResultMapper = bulkDeletionResultMapper;
        }


        #endregion

        #region Containers

        /// <inheritdoc />
        public IEnumerable<Container> ListContainers(int? limit = null, string marker = null, string markerEnd = null, string region = null, bool useInternalUrl = false, CloudIdentity identity = null)
        {
            if (limit <= 0)
                throw new ArgumentOutOfRangeException("limit");
            CheckIdentity(identity);

            var urlPath = new Uri(string.Format("{0}", GetServiceEndpointCloudFiles(identity, region, useInternalUrl)));

            var queryStringParameter = new Dictionary<string, string>();

            if (limit != null)
                queryStringParameter.Add("limit", limit.ToString());

            if (!string.IsNullOrEmpty(marker))
                queryStringParameter.Add("marker", marker);

            if (!string.IsNullOrEmpty(markerEnd))
                queryStringParameter.Add("end_marker", markerEnd);

            var response = ExecuteRESTRequest<Container[]>(identity, urlPath, HttpMethod.GET, null, queryStringParameter);

            if (response == null || response.Data == null)
                return null;

            return response.Data;
        }

        /// <inheritdoc />
        public ObjectStore CreateContainer(string container, Dictionary<string, string> headers = null, string region = null, bool useInternalUrl = false, CloudIdentity identity = null)
        {
            if (container == null)
                throw new ArgumentNullException("container");
            if (string.IsNullOrEmpty(container))
                throw new ArgumentException("container cannot be empty");
            CheckIdentity(identity);

            _cloudFilesValidator.ValidateContainerName(container);
            var urlPath = new Uri(string.Format("{0}/{1}", GetServiceEndpointCloudFiles(identity, region, useInternalUrl), _encodeDecodeProvider.UrlEncode(container)));

            var response = ExecuteRESTRequest(identity, urlPath, HttpMethod.PUT, headers: headers);

            switch (response.StatusCode)
            {
            case HttpStatusCode.Created:
                return ObjectStore.ContainerCreated;

            case HttpStatusCode.Accepted:
                return ObjectStore.ContainerExists;

            default:
                throw new ResponseException(string.Format("Unexpected status {0} returned by Create Container.", response.StatusCode), response);
            }
        }

        /// <inheritdoc />
        public void DeleteContainer(string container, bool deleteObjects = false, string region = null, bool useInternalUrl = false, CloudIdentity identity = null)
        {
            if (container == null)
                throw new ArgumentNullException("container");
            if (string.IsNullOrEmpty(container))
                throw new ArgumentException("container cannot be empty");
            CheckIdentity(identity);

            _cloudFilesValidator.ValidateContainerName(container);

            if (deleteObjects)
            {
                var headers = GetContainerHeader(container, region, useInternalUrl, identity);
                var countHeader = headers.FirstOrDefault(h => h.Key.Equals(ContainerObjectCount, StringComparison.OrdinalIgnoreCase));
                if (!EqualityComparer<KeyValuePair<string, string>>.Default.Equals(countHeader, default(KeyValuePair<string, string>)))
                {
                    int count;
                    if (!int.TryParse(countHeader.Value, out count))
                        throw new Exception(string.Format("Unable to parse the object count header for container: {0}.  Actual count value: {1}", container, countHeader.Value));

                    if (count > 0)
                    {
                        var objects = ListObjects(container, count, region: region, useInternalUrl: useInternalUrl, identity: identity);

                        if(objects.Any())
                            DeleteObjects(container, objects.Select(o => o.Name), region: region, useInternalUrl: useInternalUrl, identity: identity);
                    }
                }
            }
            
            var urlPath = new Uri(string.Format("{0}/{1}", GetServiceEndpointCloudFiles(identity, region, useInternalUrl), _encodeDecodeProvider.UrlEncode(container)));

            try
            {
                ExecuteRESTRequest(identity, urlPath, HttpMethod.DELETE);
            }
            catch (ServiceConflictException ex)
            {
                throw new ContainerNotEmptyException(null, ex);
            }
        }

        /// <inheritdoc />
        public Dictionary<string, string> GetContainerHeader(string container, string region = null, bool useInternalUrl = false, CloudIdentity identity = null)
        {
            if (container == null)
                throw new ArgumentNullException("container");
            if (string.IsNullOrEmpty(container))
                throw new ArgumentException("container cannot be empty");
            CheckIdentity(identity);

            _cloudFilesValidator.ValidateContainerName(container);
            var urlPath = new Uri(string.Format("{0}/{1}", GetServiceEndpointCloudFiles(identity, region, useInternalUrl), _encodeDecodeProvider.UrlEncode(container)));

            var response = ExecuteRESTRequest(identity, urlPath, HttpMethod.HEAD);

            var processedHeaders = _cloudFilesMetadataProcessor.ProcessMetadata(response.Headers);

            return processedHeaders[ProcessedHeadersHeaderKey];
        }

        /// <inheritdoc />
        public Dictionary<string, string> GetContainerMetaData(string container, string region = null, bool useInternalUrl = false, CloudIdentity identity = null)
        {
            if (container == null)
                throw new ArgumentNullException("container");
            if (string.IsNullOrEmpty(container))
                throw new ArgumentException("container cannot be empty");
            CheckIdentity(identity);

            _cloudFilesValidator.ValidateContainerName(container);
            var urlPath = new Uri(string.Format("{0}/{1}", GetServiceEndpointCloudFiles(identity, region, useInternalUrl), _encodeDecodeProvider.UrlEncode(container)));

            var response = ExecuteRESTRequest(identity, urlPath, HttpMethod.GET); // Should be HEAD

            var processedHeaders = _cloudFilesMetadataProcessor.ProcessMetadata(response.Headers);

            return processedHeaders[ProcessedHeadersMetadataKey];
        }

        /// <inheritdoc />
        public ContainerCDN GetContainerCDNHeader(string container, string region = null, CloudIdentity identity = null)
        {
            if (container == null)
                throw new ArgumentNullException("container");
            if (string.IsNullOrEmpty(container))
                throw new ArgumentException("container cannot be empty");
            CheckIdentity(identity);

            _cloudFilesValidator.ValidateContainerName(container);

            var urlPath = new Uri(string.Format("{0}/{1}", GetServiceEndpointCloudFilesCDN(identity, region), _encodeDecodeProvider.UrlEncode(container)));
            var response = ExecuteRESTRequest(identity, urlPath, HttpMethod.HEAD);

            string name = container;
            string uri = null;
            string streamingUri = null;
            string sslUri = null;
            string iosUri = null;
            bool enabled = false;
            long ttl = 0;
            bool logRetention = false;

            foreach (var header in response.Headers)
            {
                if (header.Key.Equals(CdnUri, StringComparison.OrdinalIgnoreCase))
                {
                    uri = header.Value;
                }
                else if (header.Key.Equals(CdnSslUri, StringComparison.OrdinalIgnoreCase))
                {
                    sslUri = header.Value;
                }
                else if (header.Key.Equals(CdnStreamingUri, StringComparison.OrdinalIgnoreCase))
                {
                    streamingUri = header.Value;
                }
                else if (header.Key.Equals(CdnTTL, StringComparison.OrdinalIgnoreCase))
                {
                    ttl = long.Parse(header.Value);
                }
                else if (header.Key.Equals(CdnEnabled, StringComparison.OrdinalIgnoreCase))
                {
                    enabled = bool.Parse(header.Value);
                }
                else if (header.Key.Equals(CdnLogRetention, StringComparison.OrdinalIgnoreCase))
                {
                    logRetention = bool.Parse(header.Value);
                }
                else if (header.Key.Equals(CdnIosUri, StringComparison.OrdinalIgnoreCase))
                {
                    iosUri = header.Value;
                }
            }

            ContainerCDN result = new ContainerCDN(name, uri, streamingUri, sslUri, iosUri, enabled, ttl, logRetention);
            return result;
        }

        /// <inheritdoc />
        public IEnumerable<ContainerCDN> ListCDNContainers(int? limit = null, string markerId = null, string markerEnd = null, bool cdnEnabled = false, string region = null, CloudIdentity identity = null)
        {
            if (limit < 0)
                throw new ArgumentOutOfRangeException("limit");
            CheckIdentity(identity);

            var urlPath = new Uri(string.Format("{0}", GetServiceEndpointCloudFilesCDN(identity, region)));

            var queryStringParameter = new Dictionary<string, string>
                {
                    {"format", "json"},
                    {"enabled_only", cdnEnabled.ToString().ToLower()}
                };

            if (limit != null)
                queryStringParameter.Add("limit", limit.ToString());

            if (!string.IsNullOrEmpty(markerId))
                queryStringParameter.Add("marker", markerId);

            if (!string.IsNullOrEmpty(markerEnd))
                queryStringParameter.Add("end_marker", markerEnd);

            var response = ExecuteRESTRequest<ContainerCDN[]>(identity, urlPath, HttpMethod.GET, null, queryStringParameter);

            if (response == null || response.Data == null)
                return null;

            return response.Data;
        }

        /// <inheritdoc />
        public Dictionary<string, string> EnableCDNOnContainer(string container, long timeToLive, string region = null, CloudIdentity identity = null)
        {
            return EnableCDNOnContainer(container, timeToLive, false, region, identity);
        }

        /// <inheritdoc />
        public Dictionary<string, string> EnableCDNOnContainer(string container, bool logRetention, string region = null, CloudIdentity identity = null)
        {
            return EnableCDNOnContainer(container, 259200, logRetention, region, identity);
        }

        /// <inheritdoc />
        public Dictionary<string, string> EnableCDNOnContainer(string container, long timeToLive, bool logRetention, string region = null, CloudIdentity identity = null)
        {
            if (container == null)
                throw new ArgumentNullException("container");
            if (string.IsNullOrEmpty(container))
                throw new ArgumentException("container cannot be empty");
            if (timeToLive < 0)
                throw new ArgumentOutOfRangeException("timeToLive");
            CheckIdentity(identity);

            _cloudFilesValidator.ValidateContainerName(container);

            if (timeToLive > 1577836800 || timeToLive < 900)
            {
                throw new TTLLengthException("TTL range must be 900 to 1577836800 seconds TTL: " + timeToLive.ToString(CultureInfo.InvariantCulture));
            }

            var headers = new Dictionary<string, string>
                {
                 {CdnTTL, timeToLive.ToString(CultureInfo.InvariantCulture)},
                 {CdnLogRetention, logRetention.ToString(CultureInfo.InvariantCulture)},
                 {CdnEnabled, "true"}
                };
            var urlPath = new Uri(string.Format("{0}/{1}", GetServiceEndpointCloudFilesCDN(identity, region), _encodeDecodeProvider.UrlEncode(container)));

            var response = ExecuteRESTRequest(identity, urlPath, HttpMethod.PUT, headers: headers);

            if (response == null)
                return null;

            return response.Headers.ToDictionary(header => header.Key, header => header.Value);
        }

        /// <inheritdoc />
        public Dictionary<string, string> DisableCDNOnContainer(string container, string region = null, CloudIdentity identity = null)
        {
            if (container == null)
                throw new ArgumentNullException("container");
            if (string.IsNullOrEmpty(container))
                throw new ArgumentException("container cannot be empty");
            CheckIdentity(identity);

            _cloudFilesValidator.ValidateContainerName(container);

            var headers = new Dictionary<string, string>
                {
                {CdnEnabled, "false"}
                };
            var urlPath = new Uri(string.Format("{0}/{1}", GetServiceEndpointCloudFilesCDN(identity, region), _encodeDecodeProvider.UrlEncode(container)));

            var response = ExecuteRESTRequest(identity, urlPath, HttpMethod.PUT, headers: headers);

            if (response == null)
                return null;

            return response.Headers.ToDictionary(header => header.Key, header => header.Value);
        }

        /// <inheritdoc />
        public void UpdateContainerMetadata(string container, Dictionary<string, string> metadata, string region = null, bool useInternalUrl = false, CloudIdentity identity = null)
        {
            if (container == null)
                throw new ArgumentNullException("container");
            if (metadata == null)
                throw new ArgumentNullException("metadata");
            if (string.IsNullOrEmpty(container))
                throw new ArgumentException("container cannot be empty");
            CheckIdentity(identity);

            _cloudFilesValidator.ValidateContainerName(container);

            var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (KeyValuePair<string, string> m in metadata)
            {
                if (string.IsNullOrEmpty(m.Key))
                    throw new ArgumentException("metadata keys cannot be null or empty");
                if (m.Key.Contains('_'))
                    throw new NotSupportedException("This provider does not support metadata keys containing an underscore.");
                if (m.Key.Contains('\''))
                    throw new NotSupportedException("This provider does not support metadata keys containing an apostrophe.");

                headers.Add(ContainerMetaDataPrefix + m.Key, EncodeUnicodeValue(m.Value));
            }

            var urlPath = new Uri(string.Format("{0}/{1}", GetServiceEndpointCloudFiles(identity, region, useInternalUrl), _encodeDecodeProvider.UrlEncode(container)));

            ExecuteRESTRequest(identity, urlPath, HttpMethod.POST, headers: headers);
        }

        /// <inheritdoc />
        public void DeleteContainerMetadata(string container, IEnumerable<string> keys, string region = null, bool useInternalUrl = false, CloudIdentity identity = null)
        {
            if (container == null)
                throw new ArgumentNullException("container");
            if (keys == null)
                throw new ArgumentNullException("keys");
            if (string.IsNullOrEmpty(container))
                throw new ArgumentException("container cannot be empty");
            CheckIdentity(identity);

            Dictionary<string, string> headers = keys.ToDictionary(i => i, i => default(string), StringComparer.OrdinalIgnoreCase);
            UpdateContainerMetadata(container, headers, region, useInternalUrl, identity);
        }

        /// <inheritdoc />
        public void DeleteContainerMetadata(string container, string key, string region = null, bool useInternalUrl = false, CloudIdentity identity = null)
        {
            if (key == null)
                throw new ArgumentNullException("key");
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("key cannot be empty");

            DeleteContainerMetadata(container, new[] { key }, region, useInternalUrl, identity);
        }

        /// <inheritdoc />
        public void UpdateContainerCdnHeaders(string container, Dictionary<string, string> headers, string region = null, CloudIdentity identity = null)
        {
            if (container == null)
                throw new ArgumentNullException("container");
            if (headers == null)
                throw new ArgumentNullException("headers");
            if (string.IsNullOrEmpty(container))
                throw new ArgumentException("container cannot be empty");
            CheckIdentity(identity);

            _cloudFilesValidator.ValidateContainerName(container);
            VerifyContainerIsCDNEnabled(container, region, identity);

            var urlPath = new Uri(string.Format("{0}/{1}", GetServiceEndpointCloudFilesCDN(identity, region), _encodeDecodeProvider.UrlEncode(container)));
            ExecuteRESTRequest(identity, urlPath, HttpMethod.POST, headers: headers);
        }

        /// <inheritdoc />
        public void EnableStaticWebOnContainer(string container, string index, string error, string css, bool listing, string region = null, bool useInternalUrl = false, CloudIdentity identity = null)
        {
            if (container == null)
                throw new ArgumentNullException("container");
            if (index == null)
                throw new ArgumentNullException("index");
            if (error == null)
                throw new ArgumentNullException("error");
            if (css == null)
                throw new ArgumentNullException("css");
            if (string.IsNullOrEmpty(container))
                throw new ArgumentException("container cannot be empty");
            if (string.IsNullOrEmpty(index))
                throw new ArgumentException("index cannot be empty");
            if (string.IsNullOrEmpty(error))
                throw new ArgumentException("error cannot be empty");
            if (string.IsNullOrEmpty(css))
                throw new ArgumentException("css cannot be empty");
            CheckIdentity(identity);

            _cloudFilesValidator.ValidateContainerName(container);
            _cloudFilesValidator.ValidateObjectName(index);
            _cloudFilesValidator.ValidateObjectName(error);
            _cloudFilesValidator.ValidateObjectName(css);
            VerifyContainerIsCDNEnabled(container, region, identity);

            var metadata = new Dictionary<string, string>
                                {
                                    {WebIndex, UriUtility.UriEncode(index, UriPart.AnyUrl, Encoding.UTF8)},
                                    {WebError, UriUtility.UriEncode(error, UriPart.AnyUrl, Encoding.UTF8)},
                                    {WebListingsCSS, UriUtility.UriEncode(css, UriPart.AnyUrl, Encoding.UTF8)},
                                    {WebListings, listing.ToString()}
                                };
            UpdateContainerMetadata(container, metadata, region, useInternalUrl, identity);
        }

        /// <inheritdoc />
        public void EnableStaticWebOnContainer(string container, string index, string error, bool listing, string region = null, bool useInternalUrl = false, CloudIdentity identity = null)
        {
            if (container == null)
                throw new ArgumentNullException("container");
            if (index == null)
                throw new ArgumentNullException("index");
            if (error == null)
                throw new ArgumentNullException("error");
            if (string.IsNullOrEmpty(container))
                throw new ArgumentException("container cannot be empty");
            if (string.IsNullOrEmpty(index))
                throw new ArgumentException("index cannot be empty");
            if (string.IsNullOrEmpty(error))
                throw new ArgumentException("error cannot be empty");
            CheckIdentity(identity);

            _cloudFilesValidator.ValidateContainerName(container);
            _cloudFilesValidator.ValidateObjectName(index);
            _cloudFilesValidator.ValidateObjectName(error);
            VerifyContainerIsCDNEnabled(container, region, identity);

            var headers = new Dictionary<string, string>
                                  {
                                      {WebIndex, UriUtility.UriEncode(index, UriPart.AnyUrl, Encoding.UTF8)},
                                      {WebError, UriUtility.UriEncode(error, UriPart.AnyUrl, Encoding.UTF8)},
                                      {WebListings, listing.ToString()}
                                  };
            UpdateContainerMetadata(container, headers, region, useInternalUrl, identity);
        }

        /// <inheritdoc />
        public void EnableStaticWebOnContainer(string container, string css, bool listing, string region = null, bool useInternalUrl = false, CloudIdentity identity = null)
        {
            if (container == null)
                throw new ArgumentNullException("container");
            if (css == null)
                throw new ArgumentNullException("css");
            if (string.IsNullOrEmpty(container))
                throw new ArgumentException("container cannot be empty");
            if (string.IsNullOrEmpty(css))
                throw new ArgumentException("css cannot be empty");
            CheckIdentity(identity);

            _cloudFilesValidator.ValidateContainerName(container);
            _cloudFilesValidator.ValidateObjectName(css);
            VerifyContainerIsCDNEnabled(container, region, identity);

            var headers = new Dictionary<string, string>
                                {
                                    {WebListingsCSS, UriUtility.UriEncode(css, UriPart.AnyUrl, Encoding.UTF8)},
                                    {WebListings, listing.ToString()}
                                };
            UpdateContainerMetadata(container, headers, region, useInternalUrl, identity);
        }

        /// <inheritdoc />
        public void EnableStaticWebOnContainer(string container, string index, string error, string region = null, bool useInternalUrl = false, CloudIdentity identity = null)
        {
            if (container == null)
                throw new ArgumentNullException("container");
            if (index == null)
                throw new ArgumentNullException("index");
            if (error == null)
                throw new ArgumentNullException("error");
            if (string.IsNullOrEmpty(container))
                throw new ArgumentException("container cannot be empty");
            if (string.IsNullOrEmpty(index))
                throw new ArgumentException("index cannot be empty");
            if (string.IsNullOrEmpty(error))
                throw new ArgumentException("error cannot be empty");
            CheckIdentity(identity);

            _cloudFilesValidator.ValidateContainerName(container);
            _cloudFilesValidator.ValidateObjectName(index);
            _cloudFilesValidator.ValidateObjectName(error);
            VerifyContainerIsCDNEnabled(container, region, identity);

            var headers = new Dictionary<string, string>
                                  {
                                      {WebIndex, UriUtility.UriEncode(index, UriPart.AnyUrl, Encoding.UTF8)},
                                      {WebError, UriUtility.UriEncode(error, UriPart.AnyUrl, Encoding.UTF8)}
                                  };
            UpdateContainerMetadata(container, headers, region, useInternalUrl, identity);
        }

        /// <inheritdoc />
        public void DisableStaticWebOnContainer(string container, string region = null, bool useInternalUrl = false, CloudIdentity identity = null)
        {
            if (container == null)
                throw new ArgumentNullException("container");
            if (string.IsNullOrEmpty(container))
                throw new ArgumentException("container cannot be empty");
            CheckIdentity(identity);

            _cloudFilesValidator.ValidateContainerName(container);
            VerifyContainerIsCDNEnabled(container, region, identity);

            var headers = new Dictionary<string, string>
                                {
                                    {WebIndex, string.Empty},
                                    {WebError, string.Empty},
                                    {WebListingsCSS, string.Empty},
                                    {WebListings, string.Empty}
                                };
            UpdateContainerMetadata(container, headers, region, useInternalUrl, identity);
        }

        #endregion

        #region Container Objects

        /// <inheritdoc />
        public Dictionary<string, string> GetObjectHeaders(string container, string objectName, string region = null, bool useInternalUrl = false, CloudIdentity identity = null)
        {
            if (container == null)
                throw new ArgumentNullException("container");
            if (objectName == null)
                throw new ArgumentNullException("objectName");
            if (string.IsNullOrEmpty(container))
                throw new ArgumentException("container cannot be empty");
            if (string.IsNullOrEmpty(objectName))
                throw new ArgumentException("objectName cannot be empty");
            CheckIdentity(identity);

            _cloudFilesValidator.ValidateContainerName(container);
            _cloudFilesValidator.ValidateObjectName(objectName);
            var urlPath = new Uri(string.Format("{0}/{1}/{2}", GetServiceEndpointCloudFiles(identity, region, useInternalUrl), _encodeDecodeProvider.UrlEncode(container), _encodeDecodeProvider.UrlEncode(objectName)));

            var response = ExecuteRESTRequest(identity, urlPath, HttpMethod.HEAD);

            var processedHeaders = _cloudFilesMetadataProcessor.ProcessMetadata(response.Headers);

            return processedHeaders[ProcessedHeadersHeaderKey];
        }

        /// <inheritdoc />
        public Dictionary<string, string> GetObjectMetaData(string container, string objectName, string region = null, bool useInternalUrl = false, CloudIdentity identity = null)
        {
            if (container == null)
                throw new ArgumentNullException("container");
            if (objectName == null)
                throw new ArgumentNullException("objectName");
            if (string.IsNullOrEmpty(container))
                throw new ArgumentException("container cannot be empty");
            if (string.IsNullOrEmpty(objectName))
                throw new ArgumentException("objectName cannot be empty");
            CheckIdentity(identity);

            _cloudFilesValidator.ValidateContainerName(container);
            _cloudFilesValidator.ValidateObjectName(objectName);
            var urlPath = new Uri(string.Format("{0}/{1}/{2}", GetServiceEndpointCloudFiles(identity, region, useInternalUrl), _encodeDecodeProvider.UrlEncode(container), _encodeDecodeProvider.UrlEncode(objectName)));

            var response = ExecuteRESTRequest(identity, urlPath, HttpMethod.HEAD);

            var processedHeaders = _cloudFilesMetadataProcessor.ProcessMetadata(response.Headers);

            return processedHeaders[ProcessedHeadersMetadataKey];
        }

        /// <inheritdoc />
        public void UpdateObjectHeaders(string container, string objectName, Dictionary<string,string> headers, string region = null, bool useInternalUrl = false, CloudIdentity identity = null)
        {
            if (container == null)
                throw new ArgumentNullException("container");
            if (objectName == null)
                throw new ArgumentNullException("objectName");
            if (headers == null)
                throw new ArgumentNullException("headers");
            if (string.IsNullOrEmpty(container))
                throw new ArgumentException("container cannot be empty");
            if (string.IsNullOrEmpty(objectName))
                throw new ArgumentException("objectName cannot be empty");
            CheckIdentity(identity);

            _cloudFilesValidator.ValidateContainerName(container);
            _cloudFilesValidator.ValidateObjectName(objectName);

            var hdrs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (KeyValuePair<string, string> m in headers)
            {
                if (string.IsNullOrEmpty(m.Key))
                    throw new ArgumentException("cors cannot contain any empty keys");

                hdrs.Add(m.Key, EncodeUnicodeValue(m.Value));
            }

            var urlPath = new Uri(string.Format("{0}/{1}/{2}", GetServiceEndpointCloudFiles(identity, region, useInternalUrl), _encodeDecodeProvider.UrlEncode(container), _encodeDecodeProvider.UrlEncode(objectName)));

            RequestSettings settings = BuildDefaultRequestSettings();
            // make sure the content type is not changed by the metadata operation
            settings.ContentType = null;

            ExecuteRESTRequest(identity, urlPath, HttpMethod.POST, headers: hdrs, settings: settings);
           
        }

        /// <inheritdoc />
        public void UpdateObjectContentType(string container, string objectName, string contentType, string region = null, bool useInternalUrl = false, CloudIdentity identity = null)
        {
            if (container == null)
                throw new ArgumentNullException("container");
            if (objectName == null)
                throw new ArgumentNullException("objectName");
            if (contentType == null)
                throw new ArgumentNullException("contentType");
            if (string.IsNullOrEmpty(container))
                throw new ArgumentException("container cannot be empty");
            if (string.IsNullOrEmpty(objectName))
                throw new ArgumentException("objectName cannot be empty");
            CheckIdentity(identity);

            _cloudFilesValidator.ValidateContainerName(container);
            _cloudFilesValidator.ValidateObjectName(objectName);

            var hdrs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var urlPath = new Uri(string.Format("{0}/{1}/{2}", GetServiceEndpointCloudFiles(identity, region, useInternalUrl), _encodeDecodeProvider.UrlEncode(container), _encodeDecodeProvider.UrlEncode(objectName)));

            RequestSettings settings = BuildDefaultRequestSettings();
            settings.ContentType = contentType;
            CopyObject(container, objectName, container, objectName, contentType, region: region);
        }

        /// <inheritdoc />
        public void UpdateObjectMetadata(string container, string objectName, Dictionary<string, string> metadata, string region = null, bool useInternalUrl = false, CloudIdentity identity = null)
        {
            if (container == null)
                throw new ArgumentNullException("container");
            if (objectName == null)
                throw new ArgumentNullException("objectName");
            if (metadata == null)
                throw new ArgumentNullException("metadata");
            if (string.IsNullOrEmpty(container))
                throw new ArgumentException("container cannot be empty");
            if (string.IsNullOrEmpty(objectName))
                throw new ArgumentException("objectName cannot be empty");
            CheckIdentity(identity);

            _cloudFilesValidator.ValidateContainerName(container);
            _cloudFilesValidator.ValidateObjectName(objectName);

            var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (KeyValuePair<string, string> m in metadata)
            {
                if (string.IsNullOrEmpty(m.Key))
                    throw new ArgumentException("metadata cannot contain any empty keys");

                headers.Add(ObjectMetaDataPrefix + m.Key, EncodeUnicodeValue(m.Value));
            }

            var urlPath = new Uri(string.Format("{0}/{1}/{2}", GetServiceEndpointCloudFiles(identity, region, useInternalUrl), _encodeDecodeProvider.UrlEncode(container), _encodeDecodeProvider.UrlEncode(objectName)));

            RequestSettings settings = BuildDefaultRequestSettings();
            // make sure the content type is not changed by the metadata operation
            settings.ContentType = null;

            ExecuteRESTRequest(identity, urlPath, HttpMethod.POST, headers: headers, settings: settings);
        }

        /// <inheritdoc />
        public void DeleteObjectMetadata(string container, string objectName, IEnumerable<string> keys, string region = null, bool useInternalUrl = false, CloudIdentity identity = null)
        {
            if (container == null)
                throw new ArgumentNullException("container");
            if (objectName == null)
                throw new ArgumentNullException("objectName");
            if (keys == null)
                throw new ArgumentNullException("keys");
            if (string.IsNullOrEmpty(container))
                throw new ArgumentException("container cannot be empty");
            if (string.IsNullOrEmpty(objectName))
                throw new ArgumentException("objectName cannot be empty");
            CheckIdentity(identity);

            _cloudFilesValidator.ValidateContainerName(container);
            _cloudFilesValidator.ValidateObjectName(objectName);

            var headers = new Dictionary<string, string>(GetObjectMetaData(container, objectName, region, useInternalUrl, identity), StringComparer.OrdinalIgnoreCase);
            foreach (string key in keys)
            {
                if (string.IsNullOrEmpty(key))
                    throw new ArgumentException("keys cannot contain any null or empty values");

                headers.Remove(key);
            }

            UpdateObjectMetadata(container, objectName, headers, region, useInternalUrl, identity);
        }

        /// <inheritdoc />
        public void DeleteObjectMetadata(string container, string objectName, string key, string region = null, bool useInternalUrl = false, CloudIdentity identity = null)
        {
            if (key == null)
                throw new ArgumentNullException("key");
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("key cannot be empty");

            DeleteObjectMetadata(container, objectName, new[] { key }, region, useInternalUrl, identity);
        }

        /// <inheritdoc />
        public IEnumerable<ContainerObject> ListObjects(string container, int? limit = null, string marker = null, string markerEnd = null,  string prefix = null, string region = null, bool useInternalUrl = false, CloudIdentity identity = null)
        {
            if (container == null)
                throw new ArgumentNullException("container");
            if (string.IsNullOrEmpty(container))
                throw new ArgumentException("container cannot be empty");
            if (limit < 0)
                throw new ArgumentOutOfRangeException("limit");
            CheckIdentity(identity);

            _cloudFilesValidator.ValidateContainerName(container);
            var urlPath = new Uri(string.Format("{0}/{1}", GetServiceEndpointCloudFiles(identity, region, useInternalUrl), _encodeDecodeProvider.UrlEncode(container)));

            var queryStringParameter = new Dictionary<string, string>();

            if (limit != null)
                queryStringParameter.Add("limit", limit.ToString());

            if (!string.IsNullOrEmpty(marker))
                queryStringParameter.Add("marker", marker);

            if (!string.IsNullOrEmpty(markerEnd))
                queryStringParameter.Add("end_marker", markerEnd);

            if (!string.IsNullOrEmpty(prefix))
                queryStringParameter.Add("prefix", prefix);

            var response = ExecuteRESTRequest<ContainerObject[]>(identity, urlPath, HttpMethod.GET, null, queryStringParameter);

            if (response == null || response.Data == null)
                return null;

            return response.Data;
        }

        /// <inheritdoc />
        public void CreateObjectFromFile(string container, string filePath, string objectName = null, string contentType = null, int chunkSize = 4096, Dictionary<string, string> headers = null, string region = null, Action<long> progressUpdated = null, bool useInternalUrl = false, CloudIdentity identity = null)
        {
            if (container == null)
                throw new ArgumentNullException("container");
            if (filePath == null)
                throw new ArgumentNullException("filePath");
            if (string.IsNullOrEmpty(container))
                throw new ArgumentException("container cannot be empty");
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("filePath cannot be empty");
            if (chunkSize < 0)
                throw new ArgumentOutOfRangeException("chunkSize");
            CheckIdentity(identity);

            if (string.IsNullOrEmpty(objectName))
                objectName = Path.GetFileName(filePath);

            using (var stream = File.OpenRead(filePath))
            {
                CreateObject(container, stream, objectName, contentType, chunkSize, headers, region, progressUpdated, useInternalUrl, identity);
            }
        }

        /// <inheritdoc />
        public void CreateObject(string container, Stream stream, string objectName, string contentType = null, int chunkSize = 4096, Dictionary<string, string> headers = null, string region = null, Action<long> progressUpdated = null, bool useInternalUrl = false, CloudIdentity identity = null)
        {
            if (container == null)
                throw new ArgumentNullException("container");
            if (stream == null)
                throw new ArgumentNullException("stream");
            if (objectName == null)
                throw new ArgumentNullException("objectName");
            if (string.IsNullOrEmpty(container))
                throw new ArgumentException("container cannot be empty");
            if (string.IsNullOrEmpty(objectName))
                throw new ArgumentException("objectName cannot be empty");
            if (chunkSize < 0)
                throw new ArgumentOutOfRangeException("chunkSize");
            CheckIdentity(identity);

            _cloudFilesValidator.ValidateContainerName(container);
            _cloudFilesValidator.ValidateObjectName(objectName);

            if (stream.Length > LargeFileBatchThreshold)
            {
                CreateObjectInSegments(_encodeDecodeProvider.UrlEncode(container), stream, _encodeDecodeProvider.UrlEncode(objectName), contentType, chunkSize, headers, region, progressUpdated, useInternalUrl, identity);
                return;
            }
            var urlPath = new Uri(string.Format("{0}/{1}/{2}", GetServiceEndpointCloudFiles(identity, region, useInternalUrl), _encodeDecodeProvider.UrlEncode(container), _encodeDecodeProvider.UrlEncode(objectName)));

            RequestSettings settings = BuildDefaultRequestSettings();
            settings.ChunkRequest = true;
            settings.ContentType = contentType;

            StreamRESTRequest(identity, urlPath, HttpMethod.PUT, stream, chunkSize, headers: headers, progressUpdated: progressUpdated, requestSettings: settings);
        }

        /// <inheritdoc />
        public void GetObject(string container, string objectName, Stream outputStream, int chunkSize = 4096, Dictionary<string, string> headers = null, string region = null, bool verifyEtag = false, Action<long> progressUpdated = null, bool useInternalUrl = false, CloudIdentity identity = null)
        {
            if (container == null)
                throw new ArgumentNullException("container");
            if (objectName == null)
                throw new ArgumentNullException("objectName");
            if (outputStream == null)
                throw new ArgumentNullException("outputStream");
            if (string.IsNullOrEmpty(container))
                throw new ArgumentException("container cannot be empty");
            if (string.IsNullOrEmpty(objectName))
                throw new ArgumentException("objectName cannot be empty");
            if (chunkSize < 0)
                throw new ArgumentOutOfRangeException("chunkSize");
            CheckIdentity(identity);

            _cloudFilesValidator.ValidateContainerName(container);
            _cloudFilesValidator.ValidateObjectName(objectName);

            var urlPath = new Uri(string.Format("{0}/{1}/{2}", GetServiceEndpointCloudFiles(identity, region, useInternalUrl), _encodeDecodeProvider.UrlEncode(container), _encodeDecodeProvider.UrlEncode(objectName)));

            long? initialPosition;
            try
            {
                initialPosition = outputStream.Position;
            }
            catch (NotSupportedException)
            {
                if (verifyEtag)
                    throw;

                initialPosition = null;
            }

            // This flag indicates whether the outputStream needs to be set prior to copying data.
            // See: https://github.com/openstacknetsdk/openstack.net/issues/297
            bool requiresPositionReset = false;

            var response = ExecuteRESTRequest(identity, urlPath, HttpMethod.GET, (resp, isError) =>
            {
                if (resp == null)
                    return new Response(0, null, null);

                string body;

                if (!isError)
                {
                    using (var respStream = resp.GetResponseStream())
                    {
                        // The second condition will throw a proper NotSupportedException if the position
                        // cannot be checked.
                        if (requiresPositionReset && outputStream.Position != initialPosition)
                            outputStream.Position = initialPosition.Value;

                        requiresPositionReset = true;
                        CopyStream(respStream, outputStream, chunkSize, progressUpdated);
                    }

                    body = "[Binary]";
                }
                else
                {
                    using (StreamReader reader = new StreamReader(resp.GetResponseStream()))
                    {
                        body = reader.ReadToEnd();
                    }
                }

                var respHeaders = resp.Headers.AllKeys.Select(key => new HttpHeader(key, resp.GetResponseHeader(key))).ToList();

                return new Response(resp.StatusCode, respHeaders, body);
            }, headers: headers);

            string etag;
            if (verifyEtag && response.TryGetHeader(Etag, out etag))
            {
                // flush the contents of the stream to the output device
                outputStream.Flush();
                // reset the head of the stream to the beginning
                outputStream.Position = initialPosition.Value;

                string objectManifest;
                if (response.TryGetHeader(ObjectManifest, out objectManifest) && !string.IsNullOrEmpty(objectManifest))
                {
                    throw new NotSupportedException("ETag validation for dynamic large objects is not yet supported.");
                }
                else
                {
                    string staticLargeObject;
                    if (response.TryGetHeader(StaticLargeObject, out staticLargeObject) && string.Equals(bool.TrueString, staticLargeObject, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new NotSupportedException("ETag validation for static large objects is not yet supported.");
                    }
                }

                bool weakETag = etag.StartsWith("W/", StringComparison.Ordinal);
                if (weakETag)
                    etag = etag.Substring("W/".Length);

                etag = etag.Trim('"');
                using (var md5 = MD5.Create())
                {
                    md5.ComputeHash(outputStream);

                    var sbuilder = new StringBuilder();
                    var hash = md5.Hash;
                    foreach (var b in hash)
                    {
                        sbuilder.Append(b.ToString("x2"));
                    }
                    var convertedMd5 = sbuilder.ToString();
                    if (!string.Equals(convertedMd5, etag, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidETagException();
                    }
                }
            }
        }

        /// <inheritdoc />
        public void GetObjectSaveToFile(string container, string saveDirectory, string objectName, string fileName = null, int chunkSize = 65536, Dictionary<string, string> headers = null, string region = null, bool verifyEtag = false, Action<long> progressUpdated = null, bool useInternalUrl = false, CloudIdentity identity = null)
        {
            if (container == null)
                throw new ArgumentNullException("container");
            if (saveDirectory == null)
                throw new ArgumentNullException("saveDirectory");
            if (objectName == null)
                throw new ArgumentNullException("objectName");
            if (string.IsNullOrEmpty(container))
                throw new ArgumentException("container cannot be empty");
            if (string.IsNullOrEmpty(saveDirectory))
                throw new ArgumentException("saveDirectory cannot be empty");
            if (string.IsNullOrEmpty(objectName))
                throw new ArgumentException("objectName cannot be empty");
            if (chunkSize < 0)
                throw new ArgumentOutOfRangeException("chunkSize");
            CheckIdentity(identity);

            if (string.IsNullOrEmpty(fileName))
                fileName = objectName;

            var filePath = Path.Combine(saveDirectory, string.IsNullOrEmpty(fileName) ? objectName : fileName);

            try
            {
                using (var fileStream = File.Open(filePath, FileMode.Create, FileAccess.ReadWrite))
                {
                    GetObject(container, objectName, fileStream, chunkSize, headers, region, verifyEtag, progressUpdated, useInternalUrl, identity);
                }
            }
            catch (InvalidETagException)
            {
                File.Delete(filePath);
                throw;
            }
        }

        /// <inheritdoc />
        public void CopyObject(string sourceContainer, string sourceObjectName, string destinationContainer, string destinationObjectName, string destinationContentType = null, Dictionary<string, string> headers = null, string region = null, bool useInternalUrl = false, CloudIdentity identity = null)
        {
            if (sourceContainer == null)
                throw new ArgumentNullException("sourceContainer");
            if (sourceObjectName == null)
                throw new ArgumentNullException("sourceObjectName");
            if (destinationContainer == null)
                throw new ArgumentNullException("destinationContainer");
            if (destinationObjectName == null)
                throw new ArgumentNullException("destinationObjectName");
            if (string.IsNullOrEmpty(sourceContainer))
                throw new ArgumentException("sourceContainer cannot be empty");
            if (string.IsNullOrEmpty(sourceObjectName))
                throw new ArgumentException("sourceObjectName cannot be empty");
            if (string.IsNullOrEmpty(destinationContainer))
                throw new ArgumentException("destinationContainer cannot be empty");
            if (string.IsNullOrEmpty(destinationObjectName))
                throw new ArgumentException("destinationObjectName cannot be empty");
            CheckIdentity(identity);

            _cloudFilesValidator.ValidateContainerName(sourceContainer);
            _cloudFilesValidator.ValidateObjectName(sourceObjectName);

            _cloudFilesValidator.ValidateContainerName(destinationContainer);
            _cloudFilesValidator.ValidateObjectName(destinationObjectName);

            var urlPath = new Uri(string.Format("{0}/{1}/{2}", GetServiceEndpointCloudFiles(identity, region, useInternalUrl), _encodeDecodeProvider.UrlEncode(sourceContainer), _encodeDecodeProvider.UrlEncode(sourceObjectName)));

            if (headers == null)
                headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            headers.Add(Destination, string.Format("{0}/{1}", UriUtility.UriEncode(destinationContainer, UriPart.AnyUrl, Encoding.UTF8), UriUtility.UriEncode(destinationObjectName, UriPart.AnyUrl, Encoding.UTF8)));

            RequestSettings settings = BuildDefaultRequestSettings();
            if (destinationContentType != null)
            {
                settings.ContentType = destinationContentType;
            }
            else
            {
                // make sure to preserve the content type during the copy operation
                settings.ContentType = null;
            }

            ExecuteRESTRequest(identity, urlPath, HttpMethod.COPY, headers: headers, settings: settings);
        }

        /// <inheritdoc />
        public void DeleteObject(string container, string objectName, Dictionary<string, string> headers = null, bool deleteSegments = true, string region = null, bool useInternalUrl = false, CloudIdentity identity = null)
        {
            if (container == null)
                throw new ArgumentNullException("container");
            if (objectName == null)
                throw new ArgumentNullException("objectName");
            if (string.IsNullOrEmpty(container))
                throw new ArgumentException("container cannot be empty");
            if (string.IsNullOrEmpty(objectName))
                throw new ArgumentException("objectName cannot be empty");
            CheckIdentity(identity);

            _cloudFilesValidator.ValidateContainerName(container);
            _cloudFilesValidator.ValidateObjectName(objectName);

            Dictionary<string, string> objectHeader = null;
            if(deleteSegments)
                objectHeader = GetObjectHeaders(container, objectName, region, useInternalUrl, identity);

            if (deleteSegments && objectHeader != null && objectHeader.Any(h => h.Key.Equals(ObjectManifest, StringComparison.OrdinalIgnoreCase)))
            {
                var objects = ListObjects(container, region: region, useInternalUrl: useInternalUrl,
                                               identity: identity);

                if (objects != null && objects.Any())
                {
                    var segments = objects.Where(f => f.Name.StartsWith(string.Format("{0}.seg", objectName)));
                    var delObjects = new List<string> { objectName };
                    if (segments.Any())
                        delObjects.AddRange(segments.Select(s => s.Name));

                    DeleteObjects(container, delObjects, headers, region, useInternalUrl, identity);
                }
                else
                    throw new Exception(string.Format("Object \"{0}\" in container \"{1}\" does not exist.", objectName, container));
            }
            else
            {
                var urlPath = new Uri(string.Format("{0}/{1}/{2}", GetServiceEndpointCloudFiles(identity, region, useInternalUrl), _encodeDecodeProvider.UrlEncode(container), _encodeDecodeProvider.UrlEncode(objectName)));

                ExecuteRESTRequest(identity, urlPath, HttpMethod.DELETE, headers: headers);
            }
        }

        /// <summary>
        /// Deletes a collection of objects from a container.
        /// </summary>
        /// <param name="container">The container name.</param>
        /// <param name="objects">A names of objects to delete.</param>
        /// <param name="headers">A collection of custom HTTP headers to include with the request.</param>
        /// <param name="region">The region in which to execute this action. If not specified, the user's default region will be used.</param>
        /// <param name="useInternalUrl"><see langword="true"/> to use the endpoint's <see cref="Endpoint.InternalURL"/>; otherwise <see langword="false"/> to use the endpoint's <see cref="Endpoint.PublicURL"/>.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="container"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="objects"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="container"/> is empty.
        /// <para>-or-</para>
        /// <para>If <paramref name="objects"/> contains any null or empty values.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="headers"/> contains two equivalent keys when compared using <see cref="StringComparer.OrdinalIgnoreCase"/>.</para>
        /// </exception>
        /// <exception cref="ContainerNameException">If <paramref name="container"/> is not a valid container name.</exception>
        /// <exception cref="ObjectNameException">If <paramref name="objects"/> contains an item that is not a valid object name.</exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="identity"/> type.
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// <para>-or-</para>
        /// <para><paramref name="useInternalUrl"/> is <see langword="true"/> and the provider does not support internal URLs.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// <para>-or-</para>
        /// <para>If <paramref name="region"/> is <see langword="null"/> and no default region is available for the provider.</para>
        /// </exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso href="http://docs.rackspace.com/files/api/v1/cf-devguide/content/Bulk_Delete-d1e2338.html.html">Bulk Delete (Rackspace Cloud Files Developer Guide - API v1)</seealso>
        public void DeleteObjects(string container, IEnumerable<string> objects, Dictionary<string, string> headers = null, string region = null, bool useInternalUrl = false, CloudIdentity identity = null)
        {
            if (container == null)
                throw new ArgumentNullException("container");
            if (objects == null)
                throw new ArgumentNullException("objects");
            if (string.IsNullOrEmpty(container))
                throw new ArgumentException("container cannot be empty");

            BulkDelete(objects.Select(o => new KeyValuePair<string, string>(container, o)), headers, region, useInternalUrl, identity);
        }

        /// <summary>
        /// Deletes a collection of objects stored in object storage.
        /// </summary>
        /// <param name="items">The collection of items to delete. The keys of each pair specifies the container name, and the value specifies the object name.</param>
        /// <param name="headers">A collection of custom HTTP headers to include with the request.</param>
        /// <param name="region">The region in which to execute this action. If not specified, the user's default region will be used.</param>
        /// <param name="useInternalUrl"><see langword="true"/> to use the endpoint's <see cref="Endpoint.InternalURL"/>; otherwise <see langword="false"/> to use the endpoint's <see cref="Endpoint.PublicURL"/>.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="items"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="items"/> contains any values with empty keys or values.
        /// <para>-or-</para>
        /// <para>If <paramref name="headers"/> contains two equivalent keys when compared using <see cref="StringComparer.OrdinalIgnoreCase"/>.</para>
        /// </exception>
        /// <exception cref="ContainerNameException">If <paramref name="items"/> contains a pair where the key is not a valid container name.</exception>
        /// <exception cref="ObjectNameException">If <paramref name="items"/> contains a pair where the value is not a valid object name.</exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="identity"/> type.
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// <para>-or-</para>
        /// <para><paramref name="useInternalUrl"/> is <see langword="true"/> and the provider does not support internal URLs.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// <para>-or-</para>
        /// <para>If <paramref name="region"/> is <see langword="null"/> and no default region is available for the provider.</para>
        /// </exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso href="http://docs.rackspace.com/files/api/v1/cf-devguide/content/Bulk_Delete-d1e2338.html.html">Bulk Delete (Rackspace Cloud Files Developer Guide - API v1)</seealso>
        public void BulkDelete(IEnumerable<KeyValuePair<string, string>> items, Dictionary<string, string> headers = null, string region = null, bool useInternalUrl = false, CloudIdentity identity = null)
        {
            var urlPath = new Uri(string.Format("{0}/?bulk-delete", GetServiceEndpointCloudFiles(identity, region, useInternalUrl)));

            var encoded = items.Select(
                pair =>
                {
                    if (string.IsNullOrEmpty(pair.Key))
                        throw new ArgumentException("items", "items cannot contain any entries with a null or empty key (container name)");
                    if (string.IsNullOrEmpty(pair.Value))
                        throw new ArgumentException("items", "items cannot contain any entries with a null or empty value (object name)");
                    _cloudFilesValidator.ValidateContainerName(pair.Key);
                    _cloudFilesValidator.ValidateObjectName(pair.Value);

                    return string.Format("/{0}/{1}", _encodeDecodeProvider.UrlEncode(pair.Key), _encodeDecodeProvider.UrlEncode(pair.Value));
                });
            var body = string.Join("\n", encoded.ToArray());

            var response = ExecuteRESTRequest<BulkDeleteResponse>(identity, urlPath, HttpMethod.POST, body: body, headers: headers, settings: new JsonRequestSettings { ContentType = "text/plain" });

            Status status;
            if (_statusParser.TryParse(response.Data.Status, out status))
            {
                if (status.Code != 200 && !response.Data.Errors.Any())
                {
                    response.Data.AllItems = encoded;
                    throw new BulkDeletionException(response.Data.Status, _bulkDeletionResultMapper.Map(response.Data));
                }
            }
        }

        /// <summary>
        /// Upload and automatically extract an archive of files.
        /// </summary>
        /// <param name="filePath">The source file path. Example <localUri>c:\folder1\folder2\archive_name.tar.gz</localUri></param>
        /// <param name="uploadPath">The target path for the extracted files. For details about this value, see the Extract Archive reference link in the documentation for this method.</param>
        /// <param name="archiveFormat">The archive format.</param>
        /// <param name="contentType">The content type of the files extracted from the archive. If the value is <see langword="null"/> or empty, the content type of the extracted files is unspecified.</param>
        /// <param name="chunkSize">The buffer size to use for copying streaming data.</param>
        /// <param name="headers">A collection of custom HTTP headers to associate with the object (see <see cref="GetObjectHeaders"/>).</param>
        /// <param name="region">The region in which to execute this action. If not specified, the user's default region will be used.</param>
        /// <param name="progressUpdated">A callback for progress updates. If the value is <see langword="null"/>, no progress updates are reported.</param>
        /// <param name="useInternalUrl"><see langword="true"/> to use the endpoint's <see cref="Endpoint.InternalURL"/>; otherwise <see langword="false"/> to use the endpoint's <see cref="Endpoint.PublicURL"/>.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns>An <see cref="ExtractArchiveResponse"/> object containing the detailed result of the extract archive operation.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="filePath"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="uploadPath"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="archiveFormat"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="filePath"/> is empty.
        /// <para>-or-</para>
        /// <para>If <paramref name="headers"/> contains two equivalent keys when compared using <see cref="StringComparer.OrdinalIgnoreCase"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="chunkSize"/> is less than 0.</exception>
        /// <exception cref="FileNotFoundException">If the file <paramref name="filePath"/> could not be found.</exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="identity"/> type.
        /// <para>-or-</para>
        /// <para>The specified <paramref name="archiveFormat"/> is not supported by the provider.</para>
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// <para>-or-</para>
        /// <para><paramref name="useInternalUrl"/> is <see langword="true"/> and the provider does not support internal URLs.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// <para>-or-</para>
        /// <para>If <paramref name="region"/> is <see langword="null"/> and no default region is available for the provider.</para>
        /// </exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso href="http://docs.rackspace.com/files/api/v1/cf-devguide/content/Extract_Archive-d1e2338.html">Extract Archive (Rackspace Cloud Files Developer Guide - API v1)</seealso>
        /// <preliminary/>
        public ExtractArchiveResponse ExtractArchiveFromFile(string filePath, string uploadPath, ArchiveFormat archiveFormat, string contentType = null, int chunkSize = 4096, Dictionary<string, string> headers = null, string region = null, Action<long> progressUpdated = null, bool useInternalUrl = false, CloudIdentity identity = null)
        {
            if (filePath == null)
                throw new ArgumentNullException("filePath");
            if (uploadPath == null)
                throw new ArgumentNullException("uploadPath");
            if (archiveFormat == null)
                throw new ArgumentNullException("archiveFormat");
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("filePath cannot be empty");
            if (chunkSize < 0)
                throw new ArgumentOutOfRangeException("chunkSize");
            CheckIdentity(identity);

            using (var stream = File.OpenRead(filePath))
            {
                return ExtractArchive(stream, uploadPath, archiveFormat, contentType, chunkSize, headers, region, progressUpdated, useInternalUrl, identity);
            }
        }

        /// <summary>
        /// Upload and automatically extract an archive of files.
        /// </summary>
        /// <param name="stream">A <see cref="Stream"/> providing the data for the archive.</param>
        /// <param name="uploadPath">The target path for the extracted files. For details about this value, see the Extract Archive reference link in the documentation for this method.</param>
        /// <param name="archiveFormat">The archive format.</param>
        /// <param name="contentType">The content type of the files extracted from the archive. If the value is <see langword="null"/> or empty, the content type of the extracted files is unspecified.</param>
        /// <param name="chunkSize">The buffer size to use for copying streaming data.</param>
        /// <param name="headers">A collection of custom HTTP headers to associate with the object (see <see cref="GetObjectHeaders"/>).</param>
        /// <param name="region">The region in which to execute this action. If not specified, the user's default region will be used.</param>
        /// <param name="progressUpdated">A callback for progress updates. If the value is <see langword="null"/>, no progress updates are reported.</param>
        /// <param name="useInternalUrl"><see langword="true"/> to use the endpoint's <see cref="Endpoint.InternalURL"/>; otherwise <see langword="false"/> to use the endpoint's <see cref="Endpoint.PublicURL"/>.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns>An <see cref="ExtractArchiveResponse"/> object containing the detailed result of the extract archive operation.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="stream"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="uploadPath"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="archiveFormat"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="headers"/> contains two equivalent keys when compared using <see cref="StringComparer.OrdinalIgnoreCase"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="chunkSize"/> is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="identity"/> type.
        /// <para>-or-</para>
        /// <para>The specified <paramref name="archiveFormat"/> is not supported by the provider.</para>
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// <para>-or-</para>
        /// <para><paramref name="useInternalUrl"/> is <see langword="true"/> and the provider does not support internal URLs.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// <para>-or-</para>
        /// <para>If <paramref name="region"/> is <see langword="null"/> and no default region is available for the provider.</para>
        /// </exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso href="http://docs.rackspace.com/files/api/v1/cf-devguide/content/Extract_Archive-d1e2338.html">Extract Archive (Rackspace Cloud Files Developer Guide - API v1)</seealso>
        /// <preliminary/>
        public ExtractArchiveResponse ExtractArchive(Stream stream, string uploadPath, ArchiveFormat archiveFormat, string contentType = null, int chunkSize = 4096, Dictionary<string, string> headers = null, string region = null, Action<long> progressUpdated = null, bool useInternalUrl = false, CloudIdentity identity = null)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");
            if (uploadPath == null)
                throw new ArgumentNullException("uploadPath");
            if (archiveFormat == null)
                throw new ArgumentNullException("archiveFormat");
            if (chunkSize < 0)
                throw new ArgumentOutOfRangeException("chunkSize");
            CheckIdentity(identity);

            UriTemplate.UriTemplate template;
            if (!string.IsNullOrEmpty(uploadPath))
                template = new UriTemplate.UriTemplate("{uploadPath}?extract-archive={archiveFormat}");
            else
                template = new UriTemplate.UriTemplate("?extract-archive={archiveFormat}");

            Uri baseAddress = new Uri(GetServiceEndpointCloudFiles(identity, region, useInternalUrl));
            Dictionary<string, string> parameters = new Dictionary<string, string> { { "archiveFormat", archiveFormat.ToString() } };
            if (!string.IsNullOrEmpty(uploadPath))
                parameters.Add("uploadPath", UriUtility.UriEncode(uploadPath, UriPart.AnyUrl, Encoding.UTF8));

            Uri urlPath = template.BindByName(baseAddress, parameters);

            RequestSettings settings = BuildDefaultRequestSettings();
            settings.ChunkRequest = true;
            settings.ContentType = contentType;

            Response response = StreamRESTRequest(identity, urlPath, HttpMethod.PUT, stream, chunkSize, headers: headers, progressUpdated: progressUpdated, requestSettings: settings);
            return JsonConvert.DeserializeObject<ExtractArchiveResponse>(response.RawBody);
        }

        /// <inheritdoc />
        public void MoveObject(string sourceContainer, string sourceObjectName, string destinationContainer, string destinationObjectName, string destinationContentType = null, Dictionary<string, string> headers = null, string region = null, bool useInternalUrl = false, CloudIdentity identity = null)
        {
            // Do nothing if the source and destination locations are the same. Prevents the object from being deleted accidentally.
            var prefix = GetServiceEndpointCloudFiles(identity, region, useInternalUrl);
            var src = new Uri($"{prefix}/{_encodeDecodeProvider.UrlEncode(sourceContainer)}/{_encodeDecodeProvider.UrlEncode(sourceObjectName)}");
            var dest = new Uri($"{prefix}/{_encodeDecodeProvider.UrlEncode(destinationContainer)}/{_encodeDecodeProvider.UrlEncode(destinationObjectName)}");
            if (src == dest)
                return;

            CopyObject(sourceContainer, sourceObjectName, destinationContainer, destinationObjectName, destinationContentType, headers, region, useInternalUrl, identity);
            DeleteObject(sourceContainer, sourceObjectName, headers, true, region, useInternalUrl, identity);
        }

        /// <inheritdoc />
        public void PurgeObjectFromCDN(string container, string objectName, IEnumerable<string> emails = null, string region = null, CloudIdentity identity = null)
        {
            if (container == null)
                throw new ArgumentNullException("container");
            if (objectName == null)
                throw new ArgumentNullException("objectName");
            if (string.IsNullOrEmpty(container))
                throw new ArgumentException("container cannot be empty");
            if (string.IsNullOrEmpty(objectName))
                throw new ArgumentException("objectName cannot be empty");
            if (emails != null && emails.Any(string.IsNullOrEmpty))
                throw new ArgumentException("emails cannot contain any null or empty values");

            _cloudFilesValidator.ValidateContainerName(container);
            _cloudFilesValidator.ValidateObjectName(objectName);
            VerifyContainerIsCDNEnabled(container, region, identity);

            string email = emails != null ? string.Join(",", emails.ToArray()) : null;
            var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (!string.IsNullOrEmpty(email))
            {
                headers[CdnPurgeEmail] = email;
            }

            var urlPath = new Uri(string.Format("{0}/{1}/{2}", GetServiceEndpointCloudFilesCDN(identity, region), _encodeDecodeProvider.UrlEncode(container), _encodeDecodeProvider.UrlEncode(objectName)));
            ExecuteRESTRequest(identity, urlPath, HttpMethod.DELETE, headers: headers);
        }

        #endregion

        #region Accounts

        /// <inheritdoc />
        public Dictionary<string, string> GetAccountHeaders(string region = null, bool useInternalUrl = false, CloudIdentity identity = null)
        {
            CheckIdentity(identity);

            var urlPath = new Uri(string.Format("{0}", GetServiceEndpointCloudFiles(identity, region, useInternalUrl)));

            var response = ExecuteRESTRequest(identity, urlPath, HttpMethod.HEAD);

            var processedHeaders = _cloudFilesMetadataProcessor.ProcessMetadata(response.Headers);

            return processedHeaders[ProcessedHeadersHeaderKey];

        }

        /// <inheritdoc />
        public Dictionary<string, string> GetAccountMetaData(string region = null, bool useInternalUrl = false, CloudIdentity identity = null)
        {
            CheckIdentity(identity);

            var urlPath = new Uri(string.Format("{0}", GetServiceEndpointCloudFiles(identity, region, useInternalUrl)));

            var response = ExecuteRESTRequest(identity, urlPath, HttpMethod.HEAD);

            var processedHeaders = _cloudFilesMetadataProcessor.ProcessMetadata(response.Headers);

            return processedHeaders[ProcessedHeadersMetadataKey];
        }

        /// <inheritdoc />
        public void UpdateAccountMetadata(Dictionary<string, string> metadata, string region = null, bool useInternalUrl = false, CloudIdentity identity = null)
        {
            if (metadata == null)
                throw new ArgumentNullException("metadata");
            CheckIdentity(identity);

            var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (KeyValuePair<string, string> m in metadata)
            {
                if (string.IsNullOrEmpty(m.Key))
                    throw new ArgumentException("metadata keys cannot be null or empty");
                if (m.Key.Contains('_'))
                    throw new NotSupportedException("This provider does not support metadata keys containing an underscore.");
                if (m.Key.Contains('\''))
                    throw new NotSupportedException("This provider does not support metadata keys containing an apostrophe.");

                headers.Add(AccountMetaDataPrefix + m.Key, EncodeUnicodeValue(m.Value));
            }

            var urlPath = new Uri(string.Format("{0}", GetServiceEndpointCloudFiles(identity, region, useInternalUrl)));
            ExecuteRESTRequest(identity, urlPath, HttpMethod.POST, headers: headers);
        }

        /// <summary>
        /// Construct a <see cref="Uri"/> which allows public access to an object hosted in Cloud Files.
        /// </summary>
        /// <param name="method">The HTTP method that will be used to access the object. This is typically <see cref="HttpMethod.GET"/> or <see cref="HttpMethod.PUT"/>.</param>
        /// <param name="container">The container name.</param>
        /// <param name="objectName">The object name. Example <localUri>image_name.jpeg</localUri></param>
        /// <param name="key">The account key to use with the TempURL feature, as specified in the account <see cref="TempUrlKey"/> metadata.</param>
        /// <param name="expiration">The expiration time for the generated URI.</param>
        /// <param name="region">The region in which to access the object. If not specified, the user's default region will be used.</param>
        /// <param name="useInternalUrl"><see langword="true"/> to use the endpoint's <see cref="Endpoint.InternalURL"/>; otherwise <see langword="false"/> to use the endpoint's <see cref="Endpoint.PublicURL"/>.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns>An absolute <see cref="Uri"/> which allows unauthenticated (public) access to the specified object until the <paramref name="expiration"/> time passes or the account key is changed.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="container"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="objectName"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="key"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="container"/> is empty.
        /// <para>-or-</para>
        /// <para>If <paramref name="objectName"/> is empty.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="key"/> is empty.</para>
        /// </exception>
        /// <exception cref="ContainerNameException">If <paramref name="container"/> is not a valid container name.</exception>
        /// <exception cref="ObjectNameException">If <paramref name="objectName"/> is not a valid object name.</exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="identity"/> type.
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// <para>-or-</para>
        /// <para>If <paramref name="region"/> is <see langword="null"/> and no default region is available for the provider.</para>
        /// </exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso cref="TempUrlKey"/>
        /// <seealso href="http://docs.openstack.org/api/openstack-object-storage/1.0/content/tempurl.html">Temporary URL middleware (OpenStack Object Storage API v1 Reference)</seealso>
        /// <seealso href="http://docs.rackspace.com/files/api/v1/cf-devguide/content/Create_TempURL-d1a444.html">Create the TempURL (Rackspace Cloud Files Developer Guide - API v1)</seealso>
        /// <preliminary/>
        public Uri CreateTemporaryPublicUri(HttpMethod method, string container, string objectName, string key, DateTimeOffset expiration, string region = null, bool useInternalUrl = false, CloudIdentity identity = null)
        {
            if (container == null)
                throw new ArgumentNullException("container");
            if (objectName == null)
                throw new ArgumentNullException("objectName");
            if (key == null)
                throw new ArgumentNullException("key");
            if (string.IsNullOrEmpty(container))
                throw new ArgumentException("container cannot be empty");
            if (string.IsNullOrEmpty(objectName))
                throw new ArgumentException("objectName cannot be empty");
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("key cannot be empty");
            CheckIdentity(identity);

            _cloudFilesValidator.ValidateContainerName(container);
            _cloudFilesValidator.ValidateObjectName(objectName);

            Uri baseAddress = new Uri(GetServiceEndpointCloudFiles(identity, region, useInternalUrl), UriKind.Absolute);

            StringBuilder body = new StringBuilder();
            body.Append(method.ToString().ToUpperInvariant()).Append('\n');
            body.Append(expiration.ToTimestamp() / 1000).Append('\n');
            body.Append(baseAddress.PathAndQuery).Append('/').Append(container).Append('/').Append(objectName);

            using (HMAC hmac = HMAC.Create())
            {
                hmac.HashName = "SHA1";
                hmac.Key = Encoding.UTF8.GetBytes(key);
                byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(body.ToString()));
                string sig = string.Join(string.Empty, Array.ConvertAll(hash, i => i.ToString("x2")));

                UriTemplate.UriTemplate uriTemplate = new UriTemplate.UriTemplate("{container}/{objectName}?temp_url_sig={sig}&temp_url_expires={expires}");
                Dictionary<string, string> parameters = new Dictionary<string, string>()
                {
                    { "container", container },
                    { "objectName", objectName },
                    { "sig", sig },
                    { "expires", (expiration.ToTimestamp() / 1000).ToString() },
                };

                return uriTemplate.BindByName(baseAddress, parameters);
            }
        }

        /// <summary>
        /// Construct a <see cref="Uri"/> and field information supporting the public upload of objects to a Cloud Files container via an HTTP form submission.
        /// </summary>
        /// <remarks>
        /// The HTTP form used for uploading files has the following form, where <em>uri</em> is a placeholder
        /// for the URI described in the return value from this method.
        ///
        /// <code>
        /// &lt;form action="<em>uri</em>" method="POST" enctype="multipart/form-data"&gt;
        ///   &lt;input type="file" name="file1"/&gt;&lt;br/&gt;
        ///   &lt;input type="submit"/&gt;
        /// &lt;/form&gt;
        /// </code>
        ///
        /// <para>
        /// In addition to the above <c>&lt;input&gt;</c> fields, the form should include one hidden input
        /// for each of the key/value pairs described in the return value from this method. Each of these
        /// fields should have the following form, where <em>key</em> and <em>value</em> are placeholders
        /// for one key/value pair.
        /// </para>
        ///
        /// <code>
        /// &lt;input type="hidden" name="<em>key</em>" value="<em>value</em>"/&gt;
        /// </code>
        /// </remarks>
        /// <param name="container">The container name where uploaded files are placed.</param>
        /// <param name="objectPrefix">The prefix applied to uploaded objects.</param>
        /// <param name="key">The account key to use with the Form POST feature, as specified in the account <see cref="TempUrlKey"/> metadata.</param>
        /// <param name="expiration">The expiration time for the generated URI.</param>
        /// <param name="redirectUri">The URI to redirect the user to after the upload operation completes.</param>
        /// <param name="maxFileSize">Specifies the maximum size in bytes of a single file.</param>
        /// <param name="maxFileCount">The maximum number of files which can be uploaded in a single request.</param>
        /// <param name="region">The region in which to access the object. If not specified, the user's default region will be used.</param>
        /// <param name="useInternalUrl"><see langword="true"/> to use the endpoint's <see cref="Endpoint.InternalURL"/>; otherwise <see langword="false"/> to use the endpoint's <see cref="Endpoint.PublicURL"/>.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns>
        /// A <see cref="Tuple{T1, T2}"/> object containing the information necessary to submit a POST operation uploading one or more files to Cloud Files.
        /// The first item in the tuple is the absolute URI where the form data should be submitted, which is valid until the <paramref name="expiration"/>
        /// time passes or the account key is changed. The value is a collection of key/value pairs describing
        /// the names/values of additional fields to submit with the form.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="container"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="objectPrefix"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="key"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="redirectUri"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="maxFileSize"/> is less than 0.
        /// <para>-or-</para>
        /// <para>If <paramref name="maxFileCount"/> is less or equal to 0.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="container"/> is empty.
        /// <para>-or-</para>
        /// <para>If <paramref name="objectPrefix"/> is empty.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="key"/> is empty.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="maxFileSize"/> is greater than <see cref="LargeFileBatchThreshold"/>.</para>
        /// </exception>
        /// <exception cref="ContainerNameException">If <paramref name="container"/> is not a valid container name.</exception>
        /// <exception cref="ObjectNameException">If <paramref name="objectPrefix"/> is not a valid object name.</exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="identity"/> type.
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// <para>-or-</para>
        /// <para>If <paramref name="region"/> is <see langword="null"/> and no default region is available for the provider.</para>
        /// </exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso cref="TempUrlKey"/>
        /// <seealso href="http://docs.openstack.org/api/openstack-object-storage/1.0/content/form-post.html">Form POST middleware (OpenStack Object Storage API v1 Reference)</seealso>
        /// <seealso href="http://docs.rackspace.com/files/api/v1/cf-devguide/content/FormPost-d1a555.html">FormPost (Rackspace Cloud Files Developer Guide - API v1)</seealso>
        /// <preliminary/>
        public Tuple<Uri, ReadOnlyDictionary<string, string>> CreateFormPostUri(string container, string objectPrefix, string key, DateTimeOffset expiration, Uri redirectUri, long maxFileSize, int maxFileCount, string region = null, bool useInternalUrl = false, CloudIdentity identity = null)
        {
            if (container == null)
                throw new ArgumentNullException("container");
            if (objectPrefix == null)
                throw new ArgumentNullException("objectPrefix");
            if (key == null)
                throw new ArgumentNullException("key");
            if (redirectUri == null)
                throw new ArgumentNullException("redirectUri");
            if (maxFileSize < 0)
                throw new ArgumentOutOfRangeException("maxFileSize");
            if (maxFileCount <= 0)
                throw new ArgumentOutOfRangeException("maxFileCount");
            if (string.IsNullOrEmpty(container))
                throw new ArgumentException("container cannot be empty");
            if (string.IsNullOrEmpty(objectPrefix))
                throw new ArgumentException("objectPrefix cannot be empty");
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("key cannot be empty");
            if (maxFileSize > LargeFileBatchThreshold)
                throw new ArgumentException("maxFileSize cannot exceed LargeFileBatchThreshold");
            CheckIdentity(identity);

            _cloudFilesValidator.ValidateContainerName(container);
            _cloudFilesValidator.ValidateObjectName(objectPrefix);

            Uri baseAddress = new Uri(GetServiceEndpointCloudFiles(identity, region, useInternalUrl), UriKind.Absolute);

            StringBuilder body = new StringBuilder();
            body.Append(baseAddress.PathAndQuery).Append('/').Append(container).Append('/').Append(objectPrefix).Append('\n');
            body.Append(redirectUri.AbsoluteUri).Append('\n');
            body.Append(maxFileSize).Append('\n');
            body.Append(maxFileCount).Append('\n');
            body.Append(expiration.ToTimestamp() / 1000);

            using (HMAC hmac = HMAC.Create())
            {
                hmac.HashName = "SHA1";
                hmac.Key = Encoding.UTF8.GetBytes(key);
                byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(body.ToString()));
                string sig = string.Join(string.Empty, Array.ConvertAll(hash, i => i.ToString("x2")));

                UriTemplate.UriTemplate uriTemplate = new UriTemplate.UriTemplate("{container}/{objectPrefix}");
                Dictionary<string, string> parameters = new Dictionary<string, string>()
                {
                    { "container", container },
                    { "objectPrefix", objectPrefix },
                };

                Uri uri = uriTemplate.BindByName(baseAddress, parameters);
                Dictionary<string, string> fields = new Dictionary<string, string>
                {
                    { "expires", (expiration.ToTimestamp() / 1000).ToString() },
                    { "redirect", redirectUri.AbsoluteUri },
                    { "max_file_size", maxFileSize.ToString() },
                    { "max_file_count", maxFileCount.ToString() },
                    { "signature", sig },
                };

                return Tuple.Create(uri, new ReadOnlyDictionary<string, string>(fields));
            }
        }

        private static string EncodeUnicodeValue(string value)
        {
            if (value == null)
                return null;

            return Encoding.GetEncoding("ISO-8859-1").GetString(Encoding.UTF8.GetBytes(value));
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Gets the public or internal service endpoint to use for Cloud Files requests for the specified identity and region.
        /// </summary>
        /// <remarks>
        /// This method uses <c>object-store</c> for the service type, and <c>cloudFiles</c> for the preferred service name.
        /// </remarks>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <param name="region">The preferred region for the service. If this value is <see langword="null"/>, the user's default region will be used.</param>
        /// <param name="useInternalUrl"><see langword="true"/> to use the internal service endpoint; otherwise <see langword="false"/> to use the public service endpoint.</param>
        /// <returns>The URL for the requested Cloud Files endpoint.</returns>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="identity"/> type.
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// </exception>
        /// <exception cref="NoDefaultRegionSetException">If <paramref name="region"/> is <see langword="null"/> and no default region is available for the identity or provider.</exception>
        /// <exception cref="UserAuthenticationException">If no service catalog is available for the user.</exception>
        /// <exception cref="UserAuthorizationException">If no endpoint is available for the requested service.</exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        protected string GetServiceEndpointCloudFiles(CloudIdentity identity, string region = null, bool useInternalUrl = false)
        {
            return useInternalUrl ? base.GetInternalServiceEndpoint(identity, "object-store", "cloudFiles", region) : base.GetPublicServiceEndpoint(identity, "object-store", "cloudFiles", region);
        }

        /// <summary>
        /// Gets the public service endpoint to use for Cloud Files CDN requests for the specified identity and region.
        /// </summary>
        /// <remarks>
        /// This method uses <c>rax:object-cdn</c> for the service type, and <c>cloudFilesCDN</c> for the preferred service name.
        /// </remarks>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <param name="region">The preferred region for the service. If this value is <see langword="null"/>, the user's default region will be used.</param>
        /// <returns>The public URL for the requested Cloud Files CDN endpoint.</returns>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="identity"/> type.
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// </exception>
        /// <exception cref="NoDefaultRegionSetException">If <paramref name="region"/> is <see langword="null"/> and no default region is available for the identity or provider.</exception>
        /// <exception cref="UserAuthenticationException">If no service catalog is available for the user.</exception>
        /// <exception cref="UserAuthorizationException">If no endpoint is available for the requested service.</exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        protected string GetServiceEndpointCloudFilesCDN(CloudIdentity identity, string region = null)
        {
            return base.GetPublicServiceEndpoint(identity, "rax:object-cdn", "cloudFilesCDN", region);
        }

        /// <summary>
        /// Copy data from an input stream to an output stream.
        /// </summary>
        /// <remarks>
        /// The argument to the callback method is the total number of bytes written to the output stream thus far.
        /// Note that <see cref="Stream.Flush()"/> is not called on <paramref name="output"/> prior to reporting a
        /// progress update, so data may remain in the stream's buffer.
        /// </remarks>
        /// <param name="input">The input stream.</param>
        /// <param name="output">The output stream.</param>
        /// <param name="bufferSize">The size of the buffer to use for copying data.</param>
        /// <param name="progressUpdated">A callback for progress updates. If the value is <see langword="null"/>, no progress updates are reported.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="input"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="output"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="bufferSize"/> is less than or equal to 0.</exception>
        public static void CopyStream(Stream input, Stream output, int bufferSize, Action<long> progressUpdated)
        {
            if (input == null)
                throw new ArgumentNullException("input");
            if (output == null)
                throw new ArgumentNullException("output");
            if (bufferSize <= 0)
                throw new ArgumentOutOfRangeException("bufferSize");

            var buffer = new byte[bufferSize];
            int len;
            long bytesWritten = 0;

            while ((len = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, len);
                bytesWritten += len;

                if (progressUpdated != null)
                    progressUpdated(bytesWritten);
            }
        }

        /// <summary>
        /// Creates an object consisting of multiple segments, each no larger than
        /// <see cref="LargeFileBatchThreshold"/>, using data from a <see cref="Stream"/>.
        /// If the destination file already exists, the contents are overwritten.
        /// </summary>
        /// <remarks>
        /// In addition to the individual segments containing file data, this method creates
        /// the manifest required for treating the segments as a single object in future GET
        /// requests.
        /// </remarks>
        /// <param name="container">The container name.</param>
        /// <param name="stream">A <see cref="Stream"/> providing the data for the file.</param>
        /// <param name="objectName">The destination object name. Example <localUri>image_name.jpeg</localUri></param>
        /// <param name="contentType">The content type of the created object. If the value is <see langword="null"/> or empty, the content type of the created object is unspecified.</param>
        /// <param name="chunkSize">The buffer size to use for copying streaming data.</param>
        /// <param name="headers">A collection of custom HTTP headers to associate with the object (see <see cref="GetObjectHeaders"/>).</param>
        /// <param name="region">The region in which to execute this action. If not specified, the user's default region will be used.</param>
        /// <param name="progressUpdated">A callback for progress updates. If the value is <see langword="null"/>, no progress updates are reported.</param>
        /// <param name="useInternalUrl"><see langword="true"/> to use the endpoint's <see cref="Endpoint.InternalURL"/>; otherwise <see langword="false"/> to use the endpoint's <see cref="Endpoint.PublicURL"/>.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="container"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="stream"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="objectName"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="container"/> is empty.
        /// <para>-or-</para>
        /// <para>If <paramref name="objectName"/> is empty.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="headers"/> contains two equivalent keys when compared using <see cref="StringComparer.OrdinalIgnoreCase"/>.</para>
        /// </exception>
        /// <exception cref="ContainerNameException">If <paramref name="container"/> is not a valid container name.</exception>
        /// <exception cref="ObjectNameException">If <paramref name="objectName"/> is not a valid object name.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="chunkSize"/> is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="identity"/> type.
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// <para>-or-</para>
        /// <para><paramref name="useInternalUrl"/> is <see langword="true"/> and the provider does not support internal URLs.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// <para>-or-</para>
        /// <para>If <paramref name="region"/> is <see langword="null"/> and no default region is available for the provider.</para>
        /// </exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso href="http://docs.openstack.org/api/openstack-object-storage/1.0/content/PUT_createOrReplaceObject_v1__account___container___object__storage_object_services.html">Create or replace object (OpenStack Object Storage API v1 Reference)</seealso>
        /// <seealso href="http://docs.openstack.org/api/openstack-object-storage/1.0/content/manifest-objects.html">Manifest objects (OpenStack Object Storage API v1 Reference)</seealso>
        private void CreateObjectInSegments(string container, Stream stream, string objectName, string contentType = null, int chunkSize = 4096, Dictionary<string, string> headers = null, string region = null, Action<long> progressUpdated = null, bool useInternalUrl = false, CloudIdentity identity = null)
        {
            if (container == null)
                throw new ArgumentNullException("container");
            if (stream == null)
                throw new ArgumentNullException("stream");
            if (objectName == null)
                throw new ArgumentNullException("objectName");
            if (string.IsNullOrEmpty(container))
                throw new ArgumentException("container cannot be empty");
            if (string.IsNullOrEmpty(objectName))
                throw new ArgumentException("objectName cannot be empty");
            if (chunkSize < 0)
                throw new ArgumentOutOfRangeException("chunkSize");
            CheckIdentity(identity);

            _cloudFilesValidator.ValidateContainerName(container);
            _cloudFilesValidator.ValidateObjectName(objectName);

            long totalLength = stream.Length - stream.Position;
            long segmentCount = (totalLength / LargeFileBatchThreshold) + (((totalLength % LargeFileBatchThreshold) != 0) ? 1 : 0);

            long totalBytesWritten = 0;
            for (int i = 0; i < segmentCount; i++)
            {
                // the total amount of data left to write
                long remaining = (totalLength - LargeFileBatchThreshold * i);
                // the size of the current segment
                long length = Math.Min(remaining, LargeFileBatchThreshold);

                Uri urlPath = new Uri(string.Format("{0}/{1}/{2}.seg{3}", GetServiceEndpointCloudFiles(identity, region, useInternalUrl), UriUtility.UriEncode(container, UriPart.AnyUrl, Encoding.UTF8), UriUtility.UriEncode(objectName, UriPart.AnyUrl, Encoding.UTF8), i.ToString("0000")));
                long segmentBytesWritten = 0;

                RequestSettings settings = BuildDefaultRequestSettings();
                settings.ChunkRequest = true;
                settings.ContentType = contentType;

                StreamRESTRequest(identity, urlPath, HttpMethod.PUT, stream, chunkSize, length, headers: headers, requestSettings: settings, progressUpdated:
                    bytesWritten =>
                    {
                        if (progressUpdated != null)
                        {
                            segmentBytesWritten = bytesWritten;
                            progressUpdated(totalBytesWritten + segmentBytesWritten);
                        }
                    });

                totalBytesWritten += length;
            }

            // upload the manifest file
            Uri segmentUrlPath = new Uri(string.Format("{0}/{1}/{2}", GetServiceEndpointCloudFiles(identity, region, useInternalUrl), UriUtility.UriEncode(container, UriPart.AnyUrl, Encoding.UTF8), UriUtility.UriEncode(objectName, UriPart.AnyUrl, Encoding.UTF8)));
            string objectManifestValue = string.Format("{0}.seg", objectName);

            if (headers == null)
                headers = new Dictionary<string, string>();

            headers.Add(ObjectManifest, string.Format("{0}/{1}", UriUtility.UriEncode(container, UriPart.Any, Encoding.UTF8), UriUtility.UriEncode(objectManifestValue, UriPart.Any, Encoding.UTF8)));

            RequestSettings requestSettings = BuildDefaultRequestSettings();
            requestSettings.ChunkRequest = true;
            requestSettings.ContentType = contentType;

            StreamRESTRequest(identity, segmentUrlPath, HttpMethod.PUT, new MemoryStream(new Byte[0]), chunkSize, headers: headers, requestSettings: requestSettings, progressUpdated:
                (bytesWritten) =>
                {
                    if (progressUpdated != null)
                    {
                        progressUpdated(totalBytesWritten);
                    }
                });
        }

        /// <summary>
        /// Verifies that a particular container is CDN-enabled.
        /// </summary>
        /// <remarks>
        /// Normally, the <see cref="ContainerCDN.CDNEnabled"/> property is used to check if a container is
        /// CDN-enabled. However, if a container has <em>never</em> been CDN-enabled, the
        /// <see cref="GetContainerCDNHeader"/> method throws a misleading <see cref="ItemNotFoundException"/>.
        /// This method uses <see cref="GetContainerHeader"/> to distinguish between these cases, ensuring
        /// that a <see cref="CDNNotEnabledException"/> gets thrown whenever a container exists but is not
        /// CDN-enabled.
        /// </remarks>
        /// <param name="container">The container name.</param>
        /// <param name="region">The region in which to execute this action. If not specified, the user's default region will be used.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="container"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="container"/> is empty.</exception>
        /// <exception cref="ContainerNameException">If <paramref name="container"/> is not a valid container name.</exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="identity"/> type.
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// <para>-or-</para>
        /// <para>If <paramref name="region"/> is <see langword="null"/> and no default region is available for the provider.</para>
        /// </exception>
        /// <exception cref="CDNNotEnabledException">If the container does not have a CDN header, or if the <see cref="ContainerCDN.CDNEnabled"/> property is <see langword="false"/>.</exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        protected void VerifyContainerIsCDNEnabled(string container, string region, CloudIdentity identity)
        {
            if (container == null)
                throw new ArgumentNullException("container");
            if (string.IsNullOrEmpty(container))
                throw new ArgumentException("container cannot be empty");
            _cloudFilesValidator.ValidateContainerName(container);
            CheckIdentity(identity);

            try
            {
                // If the container is currently CDN enabled, or was CDN enabled at some
                // point in the past, GetContainerCDNHeader returns non-null and the CDNEnabled
                // property determines whether or not the container is currently CDN enabled.
                if (!GetContainerCDNHeader(container, region, identity).CDNEnabled)
                {
                    throw new CDNNotEnabledException("The specified container is not CDN-enabled.");
                }
            }
            catch (ItemNotFoundException ex)
            {
                // In response to an ItemNotFoundException, the GetContainerHeader method is used
                // to distinguish between cases where the container does not exist (or is not
                // accessible), and cases where the container exists but has never been CDN enabled.
                GetContainerHeader(container, region, false, identity);

                // If we get to this line, we know the container exists but has never been CDN enabled.
                throw new CDNNotEnabledException("The specified container is not CDN-enabled.", ex);
            }
        }

        #endregion

        #region constants

        #region Headers

        #region Auth Constants

        /// <summary>
        /// The <strong>X-Auth-Token</strong> header, which specifies the token to use for authenticated requests.
        /// </summary>
        /// <seealso href="http://docs.openstack.org/api/openstack-object-storage/1.0/content/authentication.html">Authentication (OpenStack Object Storage API v1 Reference)</seealso>
        /// <seealso href="http://docs.rackspace.com/files/api/v1/cf-devguide/content/Authentication-d1e639.html">Authentication (Rackspace Cloud Files Developer Guide - API v1)</seealso>
        public const string AuthToken = "x-auth-token";

        /// <summary>
        /// The <strong>X-Cdn-Management-Url</strong> header.
        /// <note type="warning">The value of this header is not defined. Do not use.</note>
        /// </summary>
        public const string CdnManagementUrl = "x-cdn-management-url";

        /// <summary>
        /// The <strong>X-Storage-Url</strong> header, which specifies the base URI for all object storage requests.
        /// </summary>
        /// <seealso href="http://docs.openstack.org/api/openstack-object-storage/1.0/content/authentication.html">Authentication (OpenStack Object Storage API v1 Reference)</seealso>
        /// <seealso href="http://docs.rackspace.com/files/api/v1/cf-devguide/content/Authentication-d1e639.html">Authentication (Rackspace Cloud Files Developer Guide - API v1)</seealso>
        public const string StorageUrl = "x-storage-url";

        #endregion

        #region Account Constants

        /// <summary>
        /// The <strong>X-Account-Meta-</strong> header prefix, which specifies the HTTP header prefix for metadata keys associated with an account.
        /// </summary>
        /// <seealso href="http://docs.openstack.org/api/openstack-object-storage/1.0/content/POST_updateAccountMeta_v1__account__storage_account_services.html">Create, update, or delete account metadata (OpenStack Object Storage API v1 Reference)</seealso>
        public const string AccountMetaDataPrefix = "x-account-meta-";

        /// <summary>
        /// The <strong>X-Account-Bytes-Used</strong> header, which specifies the total number of bytes that are stored in Object Storage for the account.
        /// </summary>
        /// <seealso href="http://docs.openstack.org/api/openstack-object-storage/1.0/content/HEAD_showAccountMeta_v1__account__storage_account_services.html">Show account metadata (OpenStack Object Storage API v1 Reference)</seealso>
        /// <seealso href="http://docs.rackspace.com/files/api/v1/cf-devguide/content/GET_listcontainers_v1__account__accountServicesOperations_d1e000.html">Show Account Details and List Containers (Rackspace Cloud Files Developer Guide - API v1)</seealso>
        public const string AccountBytesUsed = "x-account-bytes-used";

        /// <summary>
        /// The <strong>X-Account-Container-Count</strong> header, which specifies the number of containers associated with an account.
        /// </summary>
        /// <seealso href="http://docs.openstack.org/api/openstack-object-storage/1.0/content/HEAD_showAccountMeta_v1__account__storage_account_services.html">Show account metadata (OpenStack Object Storage API v1 Reference)</seealso>
        /// <seealso href="http://docs.rackspace.com/files/api/v1/cf-devguide/content/GET_listcontainers_v1__account__accountServicesOperations_d1e000.html">Show Account Details and List Containers (Rackspace Cloud Files Developer Guide - API v1)</seealso>
        public const string AccountContainerCount = "x-account-container-count";

        /// <summary>
        /// The <strong>X-Account-Object-Count</strong> header, which specifies the number of objects in an account.
        /// </summary>
        /// <seealso href="http://docs.openstack.org/api/openstack-object-storage/1.0/content/GET_showAccountDetails_v1__account__storage_account_services.html">Show account details and list containers (OpenStack Object Storage API v1 Reference)</seealso>
        /// <seealso href="http://docs.rackspace.com/files/api/v1/cf-devguide/content/GET_listcontainers_v1__account__accountServicesOperations_d1e000.html">Show Account Details and List Containers (Rackspace Cloud Files Developer Guide - API v1)</seealso>
        public const string AccountObjectCount = "x-account-object-count";

        /// <summary>
        /// The <strong>Temp-Url-Key</strong> account metadata key for use with <see cref="CreateTemporaryPublicUri"/>.
        /// </summary>
        /// <remarks>
        /// This account metadata value is passed as the <em>key</em> parameter to <see cref="CreateTemporaryPublicUri"/>.
        /// </remarks>
        /// <seealso cref="GetAccountMetaData"/>
        /// <seealso cref="UpdateAccountMetadata"/>
        /// <seealso href="http://docs.rackspace.com/files/api/v1/cf-devguide/content/Set_Account_Metadata-d1a4460.html">Set Account TempURL Metadata Key (Rackspace Cloud Files Developer Guide - API v1)</seealso>
        public const string TempUrlKey = "Temp-Url-Key";

        #endregion

        #region Container Constants

        /// <summary>
        /// The <strong>X-Container-Meta-</strong> header prefix, which specifies the HTTP header prefix for metadata keys associated with a container.
        /// </summary>
        /// <seealso href="http://docs.openstack.org/api/openstack-object-storage/1.0/content/POST_updateContainerMeta_v1__account___container__storage_container_services.html">Create, update, or delete container metadata (OpenStack Object Storage API v1 Reference)</seealso>
        public const string ContainerMetaDataPrefix = "x-container-meta-";

        /// <summary>
        /// The <strong>X-Remove-Container-Meta-</strong> header prefix, which specifies the HTTP header prefix for removing metadata keys from a container.
        /// </summary>
        /// <seealso href="http://docs.openstack.org/api/openstack-object-storage/1.0/content/POST_updateContainerMeta_v1__account___container__storage_container_services.html">Create, update, or delete container metadata (OpenStack Object Storage API v1 Reference)</seealso>
        [Obsolete("This value is not required in the .NET SDK, since a shorter way to remove metadata is to simply assign an empty string as the value for a metadata key.")]
        public const string ContainerRemoveMetaDataPrefix = "x-remove-container-meta-";

        /// <summary>
        /// The <strong>X-Container-Bytes-Used</strong> header, which specifies the total size of all objects stored in a container.
        /// </summary>
        /// <seealso href="http://docs.openstack.org/api/openstack-object-storage/1.0/content/HEAD_showContainerMeta_v1__account___container__storage_container_services.html">Show container metadata (OpenStack Object Storage API v1 Reference)</seealso>
        public const string ContainerBytesUsed = "x-container-bytes-used";

        /// <summary>
        /// The <strong>X-Container-Object-Count</strong> header, which specifies the total number of objects stored in a container.
        /// </summary>
        /// <seealso href="http://docs.openstack.org/api/openstack-object-storage/1.0/content/HEAD_showContainerMeta_v1__account___container__storage_container_services.html">Show container metadata (OpenStack Object Storage API v1 Reference)</seealso>
        public const string ContainerObjectCount = "x-container-object-count";

        /// <summary>
        /// The <strong>Web-Index</strong> metadata key, which specifies the index page for every pseudo-directory in a website.
        /// </summary>
        /// <remarks>
        /// If your pseudo-directory does not have a file with the same name as your index file, visits to the sub-directory return a 404 error.
        /// </remarks>
        /// <seealso href="http://docs.openstack.org/api/openstack-object-storage/1.0/content/static-website.html">Create static website (OpenStack Object Storage API v1 Reference)</seealso>
        /// <seealso href="http://docs.rackspace.com/files/api/v1/cf-devguide/content/Create_Static_Website-dle4000.html">Create Static Website (Rackspace Cloud Files Developer Guide - API v1)</seealso>
        public const string WebIndex = "web-index";

        /// <summary>
        /// The <strong>Web-Error</strong> metadata key, which specifies the suffix for error pages displayed for a website.
        /// </summary>
        /// <remarks>
        /// You may create and set custom error pages for visitors to your website; currently, only
        /// 401 (Unauthorized) and 404 (Not Found) errors are supported. To do this, set the metadata
        /// value <see cref="WebError"/>.
        ///
        /// <para>
        /// Error pages are served with the &lt;status&gt; code prepended to the name of the error
        /// page you set. For instance, if you set <see cref="WebError"/> to <fictitiousUri>error.html</fictitiousUri>,
        /// 401 errors will display the page <fictitiousUri>401error.html</fictitiousUri>. Similarly, 404
        /// errors will display <fictitiousUri>404error.html</fictitiousUri>. You must have both of these
        /// pages created in your container when you set the <see cref="WebError"/> metadata, or your site
        /// will display generic error pages.
        /// </para>
        ///
        /// <para>
        /// You need only set the <see cref="WebError"/> metadata once for your entire static website.
        /// </para>
        /// </remarks>
        /// <seealso href="http://docs.openstack.org/api/openstack-object-storage/1.0/content/Set_Error_Pages_for_Static_Website-dle4005.html">Set Error Pages for Static Website (OpenStack Object Storage API v1 Reference - API v1)</seealso>
        /// <seealso href="http://docs.rackspace.com/files/api/v1/cf-devguide/content/Set_Error_Pages_for_Static_Website-dle4005.html">Set Error Pages for Static Website (Rackspace Cloud Files Developer Guide - API v1)</seealso>
        public const string WebError = "web-error";

        /// <summary>
        /// The <strong>Web-Listings</strong> metadata key, which specifies whether or not pseudo-directories should
        /// display a list of files instead of returning a 404 error when the pseudo-directory does
        /// not contain an index file.
        /// </summary>
        /// <remarks>
        /// To display a list of files in pseudo-directories instead of an index, set the
        /// <see cref="WebListings"/> metadata value to <c>"TRUE"</c> for a container.
        /// </remarks>
        /// <seealso href="http://docs.openstack.org/api/openstack-object-storage/1.0/content/static-website.html">Create static website (OpenStack Object Storage API v1 Reference)</seealso>
        public const string WebListings = "web-listings";

        /// <summary>
        /// The <strong>Web-Listings-CSS</strong> metadata key, which specifies the stylesheet to use for file listings
        /// when <see cref="WebListings"/> is <see langword="true"/> and a pseudo-directory does not contain an
        /// index file.
        /// </summary>
        /// <seealso href="http://docs.openstack.org/api/openstack-object-storage/1.0/content/static-website.html">Create static website (OpenStack Object Storage API v1 Reference)</seealso>
        public const string WebListingsCSS = "web-listings-css";

        /// <summary>
        /// The <strong>X-Versions-Location</strong> header, which specifies the name of the container where previous
        /// versions of objects are stored for a container.
        /// </summary>
        /// <seealso href="http://docs.openstack.org/api/openstack-object-storage/1.0/content/set-object-versions.html">Object versioning (OpenStack Object Storage API v1 Reference - API v1)</seealso>
        public const string VersionsLocation = "x-versions-location";

        #endregion

        #region CDN Container Constants

        /// <summary>
        /// The <strong>X-Cdn-Uri</strong> header, which specifies the publicly-available URL
        /// for a CDN-enabled container.
        /// </summary>
        /// <remarks>
        /// <note type="note">
        /// This header is a Rackspace-specific extension to the OpenStack Object Storage Service.
        /// </note>
        /// </remarks>
        /// <seealso cref="ContainerCDN.CDNUri"/>
        /// <seealso href="http://docs.rackspace.com/files/api/v1/cf-devguide/content/HEAD_retrieveCDNcontainermeta_v1__account___container__CDN_Container_Services-d1e2632.html">List a CDN-Enabled Container's Metadata (Rackspace Cloud Files Developer Guide - API v1)</seealso>
        public const string CdnUri = "x-cdn-uri";

        /// <summary>
        /// The <strong>X-Cdn-Ssl-Uri</strong> header, which specifies the publicly-available
        /// URL for SSL access to a CDN-enabled container.
        /// </summary>
        /// <remarks>
        /// <note type="note">
        /// This header is a Rackspace-specific extension to the OpenStack Object Storage Service.
        /// </note>
        /// </remarks>
        /// <seealso cref="ContainerCDN.CDNSslUri"/>
        /// <seealso href="http://docs.rackspace.com/files/api/v1/cf-devguide/content/HEAD_retrieveCDNcontainermeta_v1__account___container__CDN_Container_Services-d1e2632.html">List a CDN-Enabled Container's Metadata (Rackspace Cloud Files Developer Guide - API v1)</seealso>
        public const string CdnSslUri = "x-cdn-ssl-uri";

        /// <summary>
        /// The <strong>X-Cdn-Streaming-Uri</strong> header, which specifies the publicly-available
        /// URL for streaming access to a CDN-enabled container (using Adobe HTTP Dynamic Streaming).
        /// </summary>
        /// <remarks>
        /// <note type="note">
        /// This header is a Rackspace-specific extension to the OpenStack Object Storage Service.
        /// </note>
        /// </remarks>
        /// <seealso cref="ContainerCDN.CDNStreamingUri"/>
        /// <seealso href="http://docs.rackspace.com/files/api/v1/cf-devguide/content/HEAD_retrieveCDNcontainermeta_v1__account___container__CDN_Container_Services-d1e2632.html">List a CDN-Enabled Container's Metadata (Rackspace Cloud Files Developer Guide - API v1)</seealso>
        public const string CdnStreamingUri = "x-cdn-streaming-uri";

        /// <summary>
        /// The <strong>X-Ttl</strong> header, which specifies the Time To Live (TTL) in seconds for a CDN-enabled container.
        /// </summary>
        /// <remarks>
        /// <note type="note">
        /// This header is a Rackspace-specific extension to the OpenStack Object Storage Service.
        /// </note>
        /// </remarks>
        /// <seealso cref="ContainerCDN.Ttl"/>
        /// <seealso href="http://docs.rackspace.com/files/api/v1/cf-devguide/content/HEAD_retrieveCDNcontainermeta_v1__account___container__CDN_Container_Services-d1e2632.html">List a CDN-Enabled Container's Metadata (Rackspace Cloud Files Developer Guide - API v1)</seealso>
        public const string CdnTTL = "x-ttl";

        /// <summary>
        /// The <strong>X-Log-Retention</strong> header, which specifies whether or not log retention is enabled for a CDN-enabled container.
        /// </summary>
        /// <remarks>
        /// <note type="note">
        /// This header is a Rackspace-specific extension to the OpenStack Object Storage Service.
        /// </note>
        /// </remarks>
        /// <seealso cref="ContainerCDN.LogRetention"/>
        /// <seealso href="http://docs.rackspace.com/files/api/v1/cf-devguide/content/HEAD_retrieveCDNcontainermeta_v1__account___container__CDN_Container_Services-d1e2632.html">List a CDN-Enabled Container's Metadata (Rackspace Cloud Files Developer Guide - API v1)</seealso>
        public const string CdnLogRetention = "x-log-retention";

        /// <summary>
        /// The <strong>X-Cdn-Enabled</strong> header, which specifies whether or not a container is CDN-enabled.
        /// </summary>
        /// <remarks>
        /// <note type="note">
        /// This header is a Rackspace-specific extension to the OpenStack Object Storage Service.
        /// </note>
        /// </remarks>
        /// <seealso cref="ContainerCDN.CDNEnabled"/>
        /// <seealso href="http://docs.rackspace.com/files/api/v1/cf-devguide/content/HEAD_retrieveCDNcontainermeta_v1__account___container__CDN_Container_Services-d1e2632.html">List a CDN-Enabled Container's Metadata (Rackspace Cloud Files Developer Guide - API v1)</seealso>
        public const string CdnEnabled = "x-cdn-enabled";

        /// <summary>
        /// The <strong>X-Cdn-Ios-Uri</strong> header, which specifies the publicly-available URL for
        /// iOS streaming access to a CDN-enabled container (using Apple HTTP Live Streaming).
        /// </summary>
        /// <remarks>
        /// <note type="note">
        /// This header is a Rackspace-specific extension to the OpenStack Object Storage Service.
        /// </note>
        /// </remarks>
        /// <seealso cref="ContainerCDN.CDNIosUri"/>
        /// <seealso href="http://docs.rackspace.com/files/api/v1/cf-devguide/content/iOS-Streaming-d1f3725.html">iOS Streaming (Rackspace Cloud Files Developer Guide - API v1)</seealso>
        public const string CdnIosUri = "x-cdn-ios-uri";

        #endregion

        #region Object Constants

        /// <summary>
        /// The <strong>X-Object-Meta-</strong> header prefix, which specifies the HTTP header prefix for metadata keys associated with an object.
        /// </summary>
        /// <seealso href="http://docs.openstack.org/api/openstack-object-storage/1.0/content/POST_updateObjectMeta_v1__account___container___object__storage_object_services.html">Create or update object metadata (OpenStack Object Storage API v1 Reference)</seealso>
        public const string ObjectMetaDataPrefix = "x-object-meta-";

        /// <summary>
        /// The <strong>X-Delete-After</strong> header, which specifies the relative time (in seconds
        /// from "now") after which an object should expire, not be served, and be
        /// deleted completely from the storage system.
        /// </summary>
        /// <seealso href="http://docs.openstack.org/api/openstack-object-storage/1.0/content/expire-objects.html">Schedule objects for deletion (OpenStack Object Storage API v1 Reference)</seealso>
        public const string ObjectDeleteAfter = "x-delete-after";

        /// <summary>
        /// The <strong>X-Delete-At</strong> header, which specifies the absolute time (in Unix Epoch
        /// format) after which an object should expire, not be served, and be deleted
        /// completely from the storage system.
        /// </summary>
        /// <remarks>
        /// Unix time is specified as the number of seconds elapsed since 00:00:00 UTC,
        /// 1 January 1970, not counting leap seconds.
        /// </remarks>
        /// <seealso href="http://en.wikipedia.org/wiki/Unix_time">Unix time (Wikipedia)</seealso>
        /// <seealso href="http://docs.openstack.org/api/openstack-object-storage/1.0/content/expire-objects.html">Schedule objects for deletion (OpenStack Object Storage API v1 Reference)</seealso>
        public const string ObjectDeleteAt = "x-delete-at";

        /// <summary>
        /// The <strong>ETag</strong> header, which specifies the MD5 checksum of the data in an object stored in Object Storage.
        /// </summary>
        /// <seealso href="http://docs.openstack.org/api/openstack-object-storage/1.0/content/PUT_createOrReplaceObject_v1__account___container___object__storage_object_services.html">Create or replace object (OpenStack Object Storage API v1 Reference)</seealso>
        public const string Etag = "etag";

        /// <summary>
        /// The <strong>Destination</strong> header, which specifies the destination container and object
        /// name for a Copy Object operation.
        /// </summary>
        /// <seealso href="http://docs.openstack.org/api/openstack-object-storage/1.0/content/COPY_copyObject_v1__account___container___object__storage_object_services.html">Copy object (OpenStack Object Storage API v1 Reference)</seealso>
        public const string Destination = "destination";

        /// <summary>
        /// The <strong>If-None-Match</strong> header, which allows calls to create or update objects to
        /// query whether the server already has a copy of the object before any data is sent.
        /// </summary>
        /// <remarks>
        /// Currently the service only supports specifying the header <c>If-None-Match: *</c>, which
        /// results in a <seealso cref="HttpStatusCode.PreconditionFailed"/> response if the Object
        /// Storage Service contains any file matching the name of the object being updated. Neither
        /// the content of the object nor its associated metadata are checked by the service.
        /// </remarks>
        /// <seealso href="http://docs.openstack.org/api/openstack-object-storage/1.0/content/PUT_createOrReplaceObject__v1__account___container___object__storage_object_services.html">Create or replace object (OpenStack Object Storage API v1 Reference)</seealso>
        /// <preliminary/>
        public const string IfNoneMatch = "if-none-match";

        /// <summary>
        /// The <strong>X-Object-Manifest</strong> header, which specifies the container and prefix for the segments of a
        /// dynamic large object.
        /// </summary>
        /// <seealso href="http://docs.openstack.org/api/openstack-object-storage/1.0/content/dynamic-large-object-creation.html">Dynamic large objects (OpenStack Object Storage API v1 Reference)</seealso>
        public const string ObjectManifest = "x-object-manifest";

        /// <summary>
        /// The <strong>X-Static-Large-Object</strong> header, which specifies whether an object is a manifest for a static
        /// large object.
        /// </summary>
        /// <seealso href="http://docs.openstack.org/api/openstack-object-storage/1.0/content/static-large-objects.html">Static large objects (OpenStack Object Storage API v1 Reference)</seealso>
        /// <preliminary/>
        public const string StaticLargeObject = "x-static-large-object";

        /// <summary>
        /// The <strong>X-Detect-Content-Type</strong> header, which specifies that the Object Storage service should
        /// automatically assign the content type for the object.
        /// </summary>
        /// <remarks>
        /// The provider may use any algorithm to assign the content type for the object, including but not limited
        /// to filename extension analysis and file contents analysis. The resulting content type is not required
        /// to accurately reflect the true contents of the file.
        ///
        /// <note>If this header is set to <c>True</c>, the <strong>Content-Type</strong> header is ignored.</note>
        /// </remarks>
        /// <seealso href="http://docs.openstack.org/api/openstack-object-storage/1.0/content/PUT_createOrReplaceObject_v1__account___container___object__storage_object_services.html">Create or replace object (OpenStack Object Storage API v1 Reference)</seealso>
        public const string DetectContentType = "x-detect-content-type";

        #endregion

        #region CDN Object Constants

        /// <summary>
        /// The <strong>X-Purge-Email</strong> header, which specifies the comma-separated list of email addresses to notify when a CDN purge request completes.
        /// </summary>
        /// <remarks>
        /// <note type="note">
        /// This header is a Rackspace-specific extension to the OpenStack Object Storage Service.
        /// </note>
        /// </remarks>
        /// <seealso href="http://docs.rackspace.com/files/api/v1/cf-devguide/content/DELETE_deleteCDNobject_v1__account___object__CDN_Object_Services.html">Delete CDN-Enabled Object (Rackspace Cloud Files Developer Guide - API v1)</seealso>
        public const string CdnPurgeEmail = "x-purge-email";

        #endregion

        /// <summary>
        /// The <strong>X-Newest</strong> header, which indicates that Cloud Files should locate
        /// the most recent version of an object or container listing rather than return the
        /// first response provided by an underlying storage node.
        /// </summary>
        /// <remarks>
        /// Setting this header to <c>True</c> can have a substantial performance impact on the
        /// <see cref="HttpMethod.GET"/> or <see cref="HttpMethod.HEAD"/> request. It should only
        /// be used when absolutely necessary.
        /// </remarks>
        /// <seealso href="http://docs.openstack.org/api/openstack-object-storage/1.0/content/GET_showAccountDetails_v1__account__storage_account_services.html">Show account details and list containers (OpenStack Object Storage API v1 Reference)</seealso>
        /// <seealso href="http://docs.openstack.org/api/openstack-object-storage/1.0/content/HEAD_showAccountMeta_v1__account__storage_account_services.html">Show account metadata (OpenStack Object Storage API v1 Reference)</seealso>
        /// <seealso href="http://docs.openstack.org/api/openstack-object-storage/1.0/content/HEAD_showContainerMeta_v1__account___container__storage_container_services.html">Show container metadata (OpenStack Object Storage API v1 Reference)</seealso>
        /// <seealso href="http://docs.openstack.org/api/openstack-object-storage/1.0/content/GET_getObject_v1__account___container___object__storage_object_services.html">Get object content and metadata (OpenStack Object Storage API v1 Reference)</seealso>
        /// <seealso href="http://docs.openstack.org/api/openstack-object-storage/1.0/content/HEAD_showObjectMeta_v1__account___container___object__storage_object_services.html">Show object metadata (OpenStack Object Storage API v1 Reference)</seealso>
        public const string Newest = "x-newest";

        #endregion

        /// <summary>
        /// The maximum value of <see cref="LargeFileBatchThreshold"/> supported by this provider.
        /// This value is set to the minimum value for which creation of a single object larger than
        /// the value may result in the server closing the TCP/IP connection and purging the object's
        /// data.
        /// </summary>
        /// <seealso href="http://docs.openstack.org/api/openstack-object-storage/1.0/content/large-object-creation.html">Large objects (OpenStack Object Storage API v1 Reference)</seealso>
        public static readonly long MaxLargeFileBatchThreshold = 5368709120; // 5GB

        /// <summary>
        /// This is the backing field for <see cref="LargeFileBatchThreshold"/>. The
        /// default value is <see cref="MaxLargeFileBatchThreshold"/>.
        /// </summary>
        private long _largeFileBatchThreshold = MaxLargeFileBatchThreshold;

        /// <summary>
        /// Gets or sets the maximum allowable size of a single object stored in this provider.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="value"/> is less than 0.</exception>
        /// <exception cref="ArgumentException">If <paramref name="value"/> exceeds <see cref="MaxLargeFileBatchThreshold"/>.</exception>
        public long LargeFileBatchThreshold
        {
            get
            {
                return _largeFileBatchThreshold;
            }

            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException("value");
                if (value > MaxLargeFileBatchThreshold)
                    throw new ArgumentException(string.Format("The large file threshold cannot exceed the provider's maximum value {0}", MaxLargeFileBatchThreshold), "value");

                _largeFileBatchThreshold = value;
            }
        }

        /// <summary>
        /// This value is used as the key for storing metadata information in the dictionary
        /// returned by <see cref="IObjectStorageMetadataProcessor.ProcessMetadata"/>.
        /// </summary>
        /// <seealso cref="IObjectStorageMetadataProcessor"/>
        public const string ProcessedHeadersMetadataKey = "metadata";

        /// <summary>
        /// This value is used as the key for storing non-metadata header information in the
        /// dictionary returned by <see cref="IObjectStorageMetadataProcessor.ProcessMetadata"/>.
        /// </summary>
        /// <seealso cref="IObjectStorageMetadataProcessor"/>
        public const string ProcessedHeadersHeaderKey = "headers";

        #endregion
 
    }
}
