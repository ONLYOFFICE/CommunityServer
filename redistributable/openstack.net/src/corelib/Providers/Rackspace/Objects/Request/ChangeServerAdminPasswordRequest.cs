namespace net.openstack.Providers.Rackspace.Objects.Request
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// This models the JSON request used for the Change Administrator Password request.
    /// </summary>
    /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/Change_Password-d1e3234.html">Change Administrator Password (OpenStack Compute API v2 and Extensions Reference)</seealso>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class ChangeServerAdminPasswordRequest
    {
        /// <summary>
        /// Gets additional information about the new password to assign to the server.
        /// </summary>
        [JsonProperty("changePassword")]
        public ChangeAdminPasswordDetails Details { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeServerAdminPasswordRequest"/>
        /// class with the given password.
        /// </summary>
        /// <param name="password">The new password to use on the server.</param>
        public ChangeServerAdminPasswordRequest(string password)
        {
            if (password == null)
                throw new ArgumentNullException("password");
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("password cannot be empty");

            Details = new ChangeAdminPasswordDetails(password);
        }

        /// <summary>
        /// This models the JSON body containing the details of the Change Administrator Password request.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        [JsonObject(MemberSerialization.OptIn)]
        internal class ChangeAdminPasswordDetails
        {
            /// <summary>
            /// Gets the new password to assign to the server.
            /// </summary>
            [JsonProperty("adminPass")]
            public string AdminPassword { get; private set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="ChangeAdminPasswordDetails"/>
            /// class with the given password.
            /// </summary>
            /// <param name="password">The new password to use on the server.</param>
            public ChangeAdminPasswordDetails(string password)
            {
                if (password == null)
                    throw new ArgumentNullException("password");
                if (string.IsNullOrEmpty(password))
                    throw new ArgumentException("password cannot be empty");

                AdminPassword = password;
            }
        }
    }
}
