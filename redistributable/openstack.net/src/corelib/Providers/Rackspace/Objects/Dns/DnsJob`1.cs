namespace net.openstack.Providers.Rackspace.Objects.Dns
{
    using Newtonsoft.Json;

    /// <summary>
    /// Represents an asynchronous job which provides a strongly-typed response
    /// once the job <see cref="DnsJob.Status"/> reaches <see cref="DnsJobStatus.Completed"/>.
    /// </summary>
    /// <typeparam name="TResponse">The class modeling the JSON result of the asynchronous operation.</typeparam>
    /// <seealso cref="IDnsService.GetJobStatusAsync{TResponse}"/>
    /// <seealso href="http://docs.rackspace.com/cdns/api/v1.0/cdns-devguide/content/sync_asynch_responses.html">Synchronous and Asynchronous Responses (Rackspace Cloud DNS Developer Guide - API v1.0)</seealso>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class DnsJob<TResponse> : DnsJob
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value
        /// <summary>
        /// This is the backing field for the <see cref="Response"/> property.
        /// </summary>
        [JsonProperty("response")]
        private TResponse _response;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="DnsJob{TResponse}"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected DnsJob()
        {
        }

        /// <summary>
        /// Gets the strongly-typed response from this job.
        /// </summary>
        /// <value>
        /// A <typeparamref name="TResponse"/> object representing the JSON-formatted
        /// response from the asynchronous operation, or <see langword="null"/> if the JSON
        /// response from the server did not include this property.
        /// </value>
        public TResponse Response
        {
            get
            {
                return _response;
            }
        }
    }
}
