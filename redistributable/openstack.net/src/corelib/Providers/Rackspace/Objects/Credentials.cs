namespace net.openstack.Providers.Rackspace.Objects
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the credentials data for an Authenticate request to the Rackspace
    /// Identity Service.
    /// </summary>
    /// <seealso href="http://docs.rackspace.com/auth/api/v2.0/auth-client-devguide/content/POST_authenticate_v2.0_tokens_.html">Authenticate (Rackspace Cloud Identity Client Developer Guide - API v2.0)</seealso>
    /// <seealso href="http://docs.rackspace.com/auth/api/v2.0/auth-client-devguide/content/Sample_Request_Response-d1e64.html">Sample Authentication Request and Response (Rackspace Cloud Identity Client Developer Guide - API v2.0)</seealso>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class Credentials
    {
        /// <summary>
        /// Gets or sets the username to use for authentication.
        /// </summary>
        [JsonProperty("username", DefaultValueHandling = DefaultValueHandling.Include)]
        public string Username { get; private set; }

        /// <summary>
        /// Gets or sets the password to use for authentication.
        /// </summary>
        [JsonProperty("password", DefaultValueHandling = DefaultValueHandling.Include)]
        public string Password { get; private set; }

        /// <summary>
        /// Gets or sets the API key to use for authentication.
        /// </summary>
        [JsonProperty("apiKey", DefaultValueHandling = DefaultValueHandling.Include)]
        public string APIKey { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Credentials"/> class using the specified
        /// username, password, and API key.
        /// </summary>
        /// <param name="username">The username to use for authentication.</param>
        /// <param name="password">The password to use for authentication.</param>
        /// <param name="apiKey">The API key to use for authentication.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="username"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="username"/> is empty.
        /// <para>-or-</para>
        /// <para>If <paramref name="password"/> and <paramref name="apiKey"/> are both <see langword="null"/> or empty.</para>
        /// </exception>
        public Credentials(string username, string password, string apiKey)
        {
            if (username == null)
                throw new ArgumentNullException("username");
            if (string.IsNullOrEmpty(username))
                throw new ArgumentException("username cannot be empty");
            if (string.IsNullOrEmpty(password) && string.IsNullOrEmpty(apiKey))
                throw new ArgumentException("password and apiKey cannot both be null or empty");

            Username = username;
            Password = password;
            APIKey = apiKey;
        }
    }
}
