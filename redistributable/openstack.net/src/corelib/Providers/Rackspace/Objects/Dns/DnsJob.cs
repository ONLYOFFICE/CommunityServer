namespace net.openstack.Providers.Rackspace.Objects.Dns
{
    using System;
    using JSIStudios.SimpleRESTServices.Client;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Newtonsoft.Json.Linq;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// Represents the job resource associated with a server-side asynchronous
    /// operation being performed by the DNS service.
    /// </summary>
    /// <seealso cref="IDnsService.GetJobStatusAsync"/>
    /// <seealso href="http://docs.rackspace.com/cdns/api/v1.0/cdns-devguide/content/sync_asynch_responses.html">Synchronous and Asynchronous Responses (Rackspace Cloud DNS Developer Guide - API v1.0)</seealso>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class DnsJob : ExtensibleJsonObject
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value
        /// <summary>
        /// This is the backing field for the <see cref="Request"/> property.
        /// </summary>
        [JsonProperty("request")]
        private string _request;

        /// <summary>
        /// This is the backing field for the <see cref="Status"/> property.
        /// </summary>
        [JsonProperty("status")]
        private DnsJobStatus _status;

        /// <summary>
        /// This is the backing field for the <see cref="Verb"/> property.
        /// </summary>
        [JsonProperty("verb")]
        [JsonConverter(typeof(StringEnumConverter))]
        private HttpMethod? _verb;

        /// <summary>
        /// This is the backing field for the <see cref="Id"/> property.
        /// </summary>
        [JsonProperty("jobId")]
        private JobId _id;

        /// <summary>
        /// This is the backing field for the <see cref="CallbackUri"/> property.
        /// </summary>
        [JsonProperty("callbackUrl")]
        private string _callbackUrl;

        /// <summary>
        /// This is the backing field for the <see cref="RequestUri"/> property.
        /// </summary>
        [JsonProperty("requestUrl")]
        private string _requestUrl;

        /// <summary>
        /// This is the backing field for the <see cref="Error"/> property.
        /// </summary>
        [JsonProperty("error")]
        private JObject _error;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="DnsJob"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected DnsJob()
        {
        }

        /// <summary>
        /// Gets the unique ID for this job.
        /// </summary>
        /// <value>
        /// The unique ID for the job, or <see langword="null"/> if the JSON response from the server
        /// did not include this property.
        /// </value>
        public JobId Id
        {
            get
            {
                return _id;
            }
        }

        /// <summary>
        /// Gets the status for this job.
        /// </summary>
        /// <value>
        /// The current status of the job, or <see langword="null"/> if the JSON response from the server
        /// did not include this property.
        /// </value>
        public DnsJobStatus Status
        {
            get
            {
                return _status;
            }
        }

        /// <summary>
        /// Gets the <see cref="Uri"/> representing the job resource itself.
        /// </summary>
        /// <value>
        /// The URI for the current job resource, or <see langword="null"/> if the JSON response
        /// from the server did not include this property.
        /// </value>
        public Uri CallbackUri
        {
            get
            {
                if (_callbackUrl == null)
                    return null;

                return new Uri(_callbackUrl);
            }
        }

        /// <summary>
        /// Gets the original <see cref="Uri"/> for which this job resource was
        /// created.
        /// </summary>
        /// <value>
        /// The URI for for which this job resource was created, or <see langword="null"/>
        /// if the JSON response from the server did not include this property.
        /// </value>
        public Uri RequestUri
        {
            get
            {
                if (_requestUrl == null)
                    return null;

                return new Uri(_requestUrl);
            }
        }

        /// <summary>
        /// Gets the HTTP method used for the API call that created this job resource.
        /// </summary>
        /// <value>
        /// The HTTP method used for the API call for which this job resource was
        /// created, or <see langword="null"/> if the JSON response from the server did not
        /// include this property.
        /// </value>
        public HttpMethod? Verb
        {
            get
            {
                return _verb;
            }
        }

        /// <summary>
        /// Gets the body of the original HTTP request for which this job resource
        /// was created.
        /// </summary>
        /// <value>
        /// The body of the original HTTP request for which this job resource was
        /// created, or <see langword="null"/> if the JSON response from the server did not
        /// include this property.
        /// </value>
        public string Request
        {
            get
            {
                return _request;
            }
        }

        /// <summary>
        /// Gets a <see cref="JObject"/> containing information about the specific
        /// error which occurred during the execution of this job. This property
        /// is only set when the job <see cref="Status"/> is <see cref="DnsJobStatus.Error"/>.
        /// </summary>
        /// <value>
        /// A <see cref="JObject"/> object representing the JSON-formatted information
        /// about the error that occurred while this task was running, or <see langword="null"/>
        /// if the JSON response from the server did not include this property.
        /// </value>
        public JObject Error
        {
            get
            {
                return _error;
            }
        }
    }
}
