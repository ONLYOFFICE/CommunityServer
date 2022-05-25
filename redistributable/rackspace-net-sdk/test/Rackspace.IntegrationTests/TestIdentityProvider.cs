using System;
using net.openstack.Core.Domain;
using net.openstack.Providers.Rackspace;

namespace Rackspace
{
    public class TestIdentityProvider
    {
        private static readonly string EnvironmentVariablesNotFoundErrorMessage =
            "No identity environment variables found. Make sure the following environment variables exist: " + Environment.NewLine +
            "RACKSPACENET_USER" + Environment.NewLine +
            "RACKSPACENET_APIKEY";

        /// <summary>
        /// Gets an identity provider.
        /// </summary>
        /// <param name="type">An optional suffix to use, e.g. RackConnect would look for credentials in RackspaceNet_RackConnect_User/ApiKey.</param>
        public static CloudIdentityProvider GetIdentityProvider(string type = null)
        {
            var identity = GetIdentityFromEnvironment(type);
            return new CloudIdentityProvider(identity)
            {
                ApplicationUserAgent = "CI-BOT"
            };
        }

        /// <summary>
        /// Builds an identity instance using environment variables.
        /// </summary>
        /// <param name="type">An optional suffix to use, e.g. RackConnect would look for credentials in RackspaceNet_RackConnect_User/ApiKey.</param>
        public static CloudIdentity GetIdentityFromEnvironment(string type = null)
        {
            type = type == null ? null : string.Format($"_{type.ToUpper()}");

            var user = Environment.GetEnvironmentVariable(string.Format($"RACKSPACENET{type}_USER"));
            if (!string.IsNullOrEmpty(user))
            {
                var apiKey = Environment.GetEnvironmentVariable($"RACKSPACENET{type}_APIKEY");

                if (!string.IsNullOrEmpty(apiKey))
                {
                    return new CloudIdentity
                    {
                        Username = user,
                        APIKey = apiKey
                    };
                }
            }

            user = Environment.GetEnvironmentVariable($"BAMBOO_RACKSPACENET{type}_USER");
            if (!string.IsNullOrEmpty(user))
            {
                var apiKey = Environment.GetEnvironmentVariable($"BAMBOO_RACKSPACENET{type}_PASSWORD");

                if (!string.IsNullOrEmpty(apiKey))
                {
                    return new CloudIdentity
                    {
                        Username = user,
                        APIKey = apiKey
                    };
                }
            }

            throw new Exception(EnvironmentVariablesNotFoundErrorMessage);
        }
    }
}