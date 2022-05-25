namespace net.openstack.Core.Domain
{
    using net.openstack.Core.Providers;
    using Newtonsoft.Json;

    /// <summary>
    /// Provides the CDN properties for a container in an Object Storage provider.
    /// </summary>
    /// <remarks>
    /// <note>
    /// CDN-enabled containers are a Rackspace-specific extension to the OpenStack Object Storage Service.
    /// </note>
    /// </remarks>
    /// <seealso href="http://docs.rackspace.com/files/api/v1/cf-devguide/content/CDN_Container_Services-d1e2632.html">CDN Container Services (Rackspace Cloud Files Developer Guide - API v1)</seealso>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    public class ContainerCDN : ExtensibleJsonObject
    {
        /// <summary>
        /// Gets the name of the container.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; private set; }

        /// <summary>
        /// Gets a streaming URL suitable for use in links to content you want to stream, such as video. If streaming is not available, the value is <see langword="null"/>.
        /// </summary>
        /// <seealso href="http://docs.rackspace.com/files/api/v1/cf-devguide/content/Streaming-CDN-Enabled_Containers-d1f3721.html">Streaming CDN-Enabled Containers (Rackspace Cloud Files Developer Guide - API v1)</seealso>
        [JsonProperty("cdn_streaming_uri")]
        public string CDNStreamingUri { get; private set; }

        /// <summary>
        /// Gets a URL SSL URL for accessing the container on the CDN. If SSL is not available, the value is <see langword="null"/>.
        /// </summary>
        /// <seealso href="http://docs.rackspace.com/files/api/v1/cf-devguide/content/CDN-Enabled_Containers_Served_via_SSL-d1e2821.html">CDN-Enabled Containers Served through SSL (Rackspace Cloud Files Developer Guide - API v1)</seealso>
        [JsonProperty("cdn_ssl_uri")]
        public string CDNSslUri { get; private set; }

        /// <summary>
        /// Gets a value indicating whether or not the container is CDN-Enabled.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the container is CDN-Enabled; otherwise, <see langword="false"/>.
        /// </value>
        /// <seealso cref="O:net.openstack.Core.Providers.IObjectStorageProvider.EnableCDNOnContainer"/>
        /// <seealso cref="IObjectStorageProvider.DisableCDNOnContainer"/>
        /// <seealso href="http://docs.rackspace.com/files/api/v1/cf-devguide/content/PUT_enableDisableCDNcontainer_v1__account___container__CDN_Container_Services-d1e2632.html">CDN-Enable and CDN-Disable a Container (Rackspace Cloud Files Developer Guide - API v1)</seealso>
        [JsonProperty("cdn_enabled")]
        public bool CDNEnabled { get; private set; }

        /// <summary>
        /// Gets the Time To Live (TTL) in seconds for a CDN-Enabled container.
        /// </summary>
        /// <seealso href="http://docs.rackspace.com/files/api/v1/cf-devguide/content/PUT_enableDisableCDNcontainer_v1__account___container__CDN_Container_Services-d1e2632.html">CDN-Enable and CDN-Disable a Container (Rackspace Cloud Files Developer Guide - API v1)</seealso>
        [JsonProperty("ttl")]
        public long Ttl { get; private set; }

        /// <summary>
        /// Gets a value indicating whether or not log retention is enabled for a CDN-Enabled container.
        /// </summary>
        /// <remarks>
        /// This setting specifies whether the CDN access logs should be collected and stored in the Cloud Files storage system.
        /// </remarks>
        /// <value>
        /// <see langword="true"/> if log retention is enabled for the container; otherwise, <see langword="false"/>.
        /// </value>
        /// <seealso href="http://docs.rackspace.com/files/api/v1/cf-devguide/content/HEAD_retrieveCDNcontainermeta_v1__account___container__CDN_Container_Services-d1e2632.html">List a CDN-Enabled Container's Metadata (Rackspace Cloud Files Developer Guide - API v1)</seealso>
        [JsonProperty("log_retention")]
        public bool LogRetention { get; private set; }

        /// <summary>
        /// Gets a publicly accessible URL for the container, which can be combined with any object name within the container to form the publicly accessible URL for that object for distribution over a CDN system.
        /// </summary>
        /// <seealso href="http://docs.rackspace.com/files/api/v1/cf-devguide/content/HEAD_retrieveCDNcontainermeta_v1__account___container__CDN_Container_Services-d1e2632.html">List a CDN-Enabled Container's Metadata (Rackspace Cloud Files Developer Guide - API v1)</seealso>
        [JsonProperty("cdn_uri")]
        public string CDNUri { get; private set; }

        /// <summary>
        /// Gets a publicly accessible URL for the container for use in streaming content to iOS devices. If iOS streaming is not available for the container, the value is <see langword="null"/>.
        /// </summary>
        /// <remarks>
        /// The <see cref="CDNIosUri"/> may be combined with any object name within the container to form the publicly accessible URL for streaming that object to iOS devices.
        /// </remarks>
        /// <seealso href="http://docs.rackspace.com/files/api/v1/cf-devguide/content/iOS-Streaming-d1f3725.html">iOS Streaming (Rackspace Cloud Files Developer Guide - API v1)</seealso>
        [JsonProperty("cdn_ios_uri")]
        public string CDNIosUri { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContainerCDN"/> class with the specified properties.
        /// </summary>
        /// <param name="name">The name of the container (see <see cref="Name"/>).</param>
        /// <param name="uri">The URI of the container (see <see cref="CDNUri"/>).</param>
        /// <param name="streamingUri">A streaming URL (see <see cref="CDNStreamingUri"/>).</param>
        /// <param name="sslUri">An SSL URL (see <see cref="CDNSslUri"/>).</param>
        /// <param name="iosUri">The iOS URI of the container (see <see cref="CDNIosUri"/>).</param>
        /// <param name="enabled">Whether or not the container is CDN-enabled (see <see cref="CDNEnabled"/>).</param>
        /// <param name="ttl">The time-to-live (see <see cref="Ttl"/>).</param>
        /// <param name="logRetention">Whether or not log retention is enabled (see <see cref="LogRetention"/>).</param>
        public ContainerCDN(string name, string uri, string streamingUri, string sslUri, string iosUri, bool enabled, long ttl, bool logRetention)
        {
            Name = name;
            CDNUri = uri;
            CDNStreamingUri = streamingUri;
            CDNSslUri = sslUri;
            CDNIosUri = iosUri;
            CDNEnabled = enabled;
            Ttl = ttl;
            LogRetention = logRetention;
        }
    }
}
