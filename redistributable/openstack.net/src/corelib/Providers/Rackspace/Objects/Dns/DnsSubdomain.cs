namespace net.openstack.Providers.Rackspace.Objects.Dns
{
    using System;
    using Newtonsoft.Json;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This class describes a single subdomain associated with a domain in the DNS service.
    /// </summary>
    /// <seealso cref="IDnsService.ListSubdomainsAsync"/>
    /// <seealso href="http://docs.rackspace.com/cdns/api/v1.0/cdns-devguide/content/List_Subdomains-d1e4295.html">List Subdomains (Rackspace Cloud DNS Developer Guide - API v1.0)</seealso>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class DnsSubdomain : ExtensibleJsonObject
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value
        /// <summary>
        /// This is the backing field for the <see cref="EmailAddress"/> property.
        /// </summary>
        [JsonProperty("emailAddress")]
        private string _emailAddress;

        /// <summary>
        /// This is the backing field for the <see cref="Name"/> property.
        /// </summary>
        [JsonProperty("name")]
        private string _name;

        /// <summary>
        /// This is the backing field for the <see cref="Id"/> property.
        /// </summary>
        [JsonProperty("id")]
        private DomainId _id;

        /// <summary>
        /// This is the backing field for the <see cref="Comment"/> property.
        /// </summary>
        [JsonProperty("comment")]
        private string _comment;

        /// <summary>
        /// This is the backing field for the <see cref="Created"/> property.
        /// </summary>
        [JsonProperty("created")]
        private DateTimeOffset? _created;

        /// <summary>
        /// This is the backing field for the <see cref="Updated"/> property.
        /// </summary>
        [JsonProperty("updated")]
        private DateTimeOffset? _updated;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="DnsSubdomain"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected DnsSubdomain()
        {
        }

        /// <summary>
        /// Gets the unique ID representing this subdomain within the DNS service.
        /// </summary>
        /// <value>
        /// The unique ID for the subdomain, or <see langword="null"/> if the JSON response from the server
        /// did not include the associated property.
        /// </value>
        public DomainId Id
        {
            get
            {
                return _id;
            }
        }

        /// <summary>
        /// Gets the fully-qualified DNS subdomain name.
        /// </summary>
        /// <value>
        /// The fully-qualified subdomain name, or <see langword="null"/> if the JSON response from the server
        /// did not include the associated property.
        /// </value>
        public string Name
        {
            get
            {
                return _name;
            }
        }

        /// <summary>
        /// Gets the email address associated with this subdomain.
        /// </summary>
        /// <value>
        /// The email address for the subdomain, or <see langword="null"/> if the JSON response from the server
        /// did not include the associated property.
        /// </value>
        public string EmailAddress
        {
            get
            {
                return _emailAddress;
            }
        }

        /// <summary>
        /// Gets the optional comment associated with the subdomain.
        /// </summary>
        /// <value>
        /// The comment associated with the subdomain, or <see langword="null"/> if the JSON response from the server
        /// did not include this property or if the subdomain does not have an associated comment.
        /// </value>
        public string Comment
        {
            get
            {
                return _comment;
            }
        }

        /// <summary>
        /// Gets the timestamp when this subdomain was initially created.
        /// </summary>
        /// <value>
        /// The creation timestamp of the subdomain, or <see langword="null"/> if the JSON response from the server
        /// did not include the associated property.
        /// </value>
        public DateTimeOffset? Created
        {
            get
            {
                return _created;
            }
        }

        /// <summary>
        /// Gets the timestamp when this subdomain was last updated.
        /// </summary>
        /// <value>
        /// The last-updated timestamp of the subdomain, or <see langword="null"/> if the JSON response from the server
        /// did not include the associated property.
        /// </value>
        public DateTimeOffset? Updated
        {
            get
            {
                return _updated;
            }
        }
    }
}
