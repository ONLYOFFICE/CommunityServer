using System.Collections.ObjectModel;

namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System;
    using System.Collections.Generic;

    using Newtonsoft.Json;

    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This class models the JSON representation of the configurable properties of a monitoring account.
    /// </summary>
    /// <seealso cref="IMonitoringService.UpdateAccountAsync"/>
    /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-account.html">Account (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class AccountConfiguration : ExtensibleJsonObject
    {
        /// <summary>
        /// This is the backing field for the <see cref="Metadata"/> property.
        /// </summary>
        [JsonProperty("metadata", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private Dictionary<string, string> _metadata;

        /// <summary>
        /// This is the backing field for the <see cref="WebhookToken"/> property.
        /// </summary>
        [JsonProperty("webhook_token", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private WebhookToken _webhookToken;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountConfiguration"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected AccountConfiguration()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountConfiguration"/> class
        /// with the specified metadata.
        /// </summary>
        /// <param name="metadata">The metadata to associate with the monitoring account.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="metadata"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="metadata"/> contains any empty keys.</exception>
        public AccountConfiguration(IDictionary<string, string> metadata)
        {
            if (metadata == null)
                throw new ArgumentNullException("metadata");
            if (metadata.ContainsKey(string.Empty))
                throw new ArgumentException("metadata cannot contain any empty keys", "metadata");

            _metadata = new Dictionary<string,string>(metadata);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountConfiguration"/> class
        /// with the specified webhook token.
        /// </summary>
        /// <param name="webhookToken">The webhook token to associate with the account.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="webhookToken"/> is <see langword="null"/>.</exception>
        public AccountConfiguration(WebhookToken webhookToken)
        {
            if (webhookToken == null)
                throw new ArgumentNullException("webhookToken");

            _webhookToken = webhookToken;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountConfiguration"/> class
        /// with the specified metadata and webhook token.
        /// </summary>
        /// <param name="metadata">The metadata to associate with the monitoring account. If this value is <see langword="null"/>, the metadata associated with the account is not changed.</param>
        /// <param name="webhookToken">The webhook token to associate with the account. If this value is <see langword="null"/>, the webhook token associated with the account is not changed.</param>
        /// <exception cref="ArgumentException">If <paramref name="metadata"/> contains any empty keys.</exception>
        public AccountConfiguration(IDictionary<string, string> metadata, WebhookToken webhookToken)
        {
            if (metadata != null)
            {
                if (metadata.ContainsKey(string.Empty))
                    throw new ArgumentException("metadata cannot contain any empty keys", "metadata");

                _metadata = new Dictionary<string,string>(metadata);
            }

            _webhookToken = webhookToken;
        }

        /// <summary>
        /// Gets the collection of metadata associated with the monitoring account.
        /// </summary>
        public ReadOnlyDictionary<string, string> Metadata
        {
            get
            {
                return new ReadOnlyDictionary<string,string>(_metadata);
            }
        }

        /// <summary>
        /// Gets the webhook token associated with the monitoring account.
        /// </summary>
        public WebhookToken WebhookToken
        {
            get
            {
                return _webhookToken;
            }
        }
    }
}
