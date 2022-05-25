namespace net.openstack.Providers.Rackspace.Objects.Dns
{
    using System;
    using Newtonsoft.Json;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This models the JSON representation of a serialized domain for calls to
    /// <see cref="IDnsService.ImportDomainAsync"/>.
    /// </summary>
    /// <seealso cref="IDnsService.ExportDomainAsync"/>
    /// <seealso cref="IDnsService.ImportDomainAsync"/>
    /// <seealso href="http://docs.rackspace.com/cdns/api/v1.0/cdns-devguide/content/export_domain.html">Export Domain (Rackspace Cloud DNS Developer Guide - API v1.0)</seealso>
    /// <seealso href="http://docs.rackspace.com/cdns/api/v1.0/cdns-devguide/content/import_domain.html">Import Domain (Rackspace Cloud DNS Developer Guide - API v1.0)</seealso>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class SerializedDomain : ExtensibleJsonObject
    {
        /// <summary>
        /// This is the backing field for the <see cref="Contents"/> property.
        /// </summary>
        [JsonProperty("contents")]
        private string _contents;

        /// <summary>
        /// This is the backing field for the <see cref="Contents"/> property.
        /// </summary>
        [JsonProperty("contentType")]
        private SerializedDomainFormat _contentType;

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializedDomain"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected SerializedDomain()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializedDomain"/> class
        /// with the specified contents and content type.
        /// </summary>
        /// <param name="contents">The contents of the serialized domain.</param>
        /// <param name="contentType">The content type of the serialized domain.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="contents"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="contentType"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">If <paramref name="contents"/> is empty.</exception>
        public SerializedDomain(string contents, SerializedDomainFormat contentType)
        {
            if (contents == null)
                throw new ArgumentNullException("contents");
            if (contentType == null)
                throw new ArgumentNullException("contentType");
            if (string.IsNullOrEmpty(contents))
                throw new ArgumentException("contents cannot be empty");

            _contents = contents;
            _contentType = contentType;
        }

        /// <summary>
        /// Gets the contents of the serialized domain.
        /// </summary>
        public string Contents
        {
            get
            {
                return _contents;
            }
        }

        /// <summary>
        /// Gets the content type of the serialized domain.
        /// </summary>
        public SerializedDomainFormat ContentType
        {
            get
            {
                return _contentType;
            }
        }
    }
}
