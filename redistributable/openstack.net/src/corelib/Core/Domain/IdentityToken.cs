namespace net.openstack.Core.Domain
{
    using System;
    using System.Collections.Generic;
    using net.openstack.Core.Providers;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the authentication token used for making authenticated calls to
    /// multiple APIs.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    public class IdentityToken : ExtensibleJsonObject
    {
        /// <summary>
        /// Gets the token expiration time in the format originally returned by the
        /// authentication response.
        /// </summary>
        /// <seealso cref="IIdentityProvider.GetToken"/>
        [JsonProperty("expires")]
        public string Expires { get; private set; }

        /// <summary>
        /// Gets the token ID which can be used to make authenticated API calls.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; private set; }

        /// <summary>
        /// Gets a <see cref="Tenant"/> object containing the name and ID of the
        /// tenant (or account) for the authenticated credentials.
        /// </summary>
        [JsonProperty("tenant")]
        public Tenant Tenant { get; private set; }

        /// <summary>
        /// Gets whether or not the token has expired. This property simply checks
        /// the <see cref="Expires"/> property against the current system time.
        /// If the <see cref="Expires"/> value is missing or not in a recognized
        /// format, the token is assumed to have expired.
        /// </summary>
        /// <value><see langword="true"/> if the token has expired; otherwise, <see langword="false"/>.</value>
        public bool IsExpired
        {
            get
            {
                DateTimeOffset expiration;
                if (!DateTimeOffset.TryParse(Expires, out expiration))
                    return true;

                return expiration <= DateTimeOffset.Now;
            }
        }

        /// <summary>
        /// Gets a Collection of <see cref="AuthenticationType"/> objects representing the ways the 
        /// user has authenticated.
        /// </summary>
        /// <preliminary/>
        [JsonProperty("RAX-AUTH:authenticatedBy", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public IEnumerable<AuthenticationType> AuthenticationTypes { get; private set; } 
    }
}
