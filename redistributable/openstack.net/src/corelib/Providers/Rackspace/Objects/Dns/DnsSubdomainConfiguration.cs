namespace net.openstack.Providers.Rackspace.Objects.Dns
{
    using System;
    using Newtonsoft.Json;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This models a DNS subdomain for calls to the <see cref="IDnsService.CreateDomainsAsync"/>.
    /// </summary>
    /// <seealso href="http://docs.rackspace.com/cdns/api/v1.0/cdns-devguide/content/create_domains.html">Create Domain(s) (Rackspace Cloud DNS Developer Guide - API v1.0)</seealso>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class DnsSubdomainConfiguration : ExtensibleJsonObject
    {
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
        /// This is the backing field for the <see cref="Comment"/> property.
        /// </summary>
        [JsonProperty("comment")]
        private string _comment;

        /// <summary>
        /// Initializes a new instance of the <see cref="DnsSubdomainConfiguration"/> class
        /// with the specified name, email address, and optional comment to associate with
        /// the subdomain.
        /// </summary>
        /// <param name="emailAddress">The email address associated with the subdomain.</param>
        /// <param name="name">
        /// The name of the subdomain. This is the fully-qualified name of the subdomain, e.g.
        /// if the primary domain is <fictitiousUri>example.com</fictitiousUri>, then
        /// the subdomain name could be <fictitiousUri>www.example.com</fictitiousUri>
        /// but not just <fictitiousUri>www</fictitiousUri>.</param>
        /// <param name="comment">An optional comment to associate with the subdomain.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="name"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="emailAddress"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="name"/> is empty.
        /// <para>-or-</para>
        /// <para>If <paramref name="emailAddress"/> is empty.</para>
        /// </exception>
        public DnsSubdomainConfiguration(string emailAddress, string name, string comment)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (emailAddress == null)
                throw new ArgumentNullException("emailAddress");
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name cannot be empty");
            if (string.IsNullOrEmpty(emailAddress))
                throw new ArgumentException("emailAddress cannot be empty");

            _emailAddress = emailAddress;
            _name = name;
            _comment = comment;
        }

        /// <summary>
        /// Gets the fully-qualified name of this subdomain.
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
        }

        /// <summary>
        /// Gets the email address associated with the subdomain.
        /// </summary>
        public string EmailAddress
        {
            get
            {
                return _emailAddress;
            }
        }

        /// <summary>
        /// Gets the comment associated with the subdomain.
        /// </summary>
        /// <value>
        /// The comment associated with the subdomain, or <see langword="null"/> if no comment
        /// was provided for this subdomain.
        /// </value>
        public string Comment
        {
            get
            {
                return _comment;
            }
        }
    }
}
