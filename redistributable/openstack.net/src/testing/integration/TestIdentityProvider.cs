using System;
using System.Net.Http.Headers;
using net.openstack.Core.Domain;
using net.openstack.Core.Providers;

namespace OpenStack
{
    public class TestIdentityProvider
    {
        private static readonly string EnvironmentVariablesNotFoundErrorMessage = 
            "No identity environment variables found. Make sure the following environment variables exist: " + Environment.NewLine +
                            "OPENSTACKNET_IDENTITY_URL" + Environment.NewLine +
                            "OPENSTACKNET_{0}USER" + Environment.NewLine +
                            "OPENSTACKNET_{0}PASSWORD" + Environment.NewLine +
                            "OPENSTACKNET_{0}PROJECT";

        public static IIdentityProvider GetOperatorIdentity()
        {
            return GetIdentityProvider("OPERATOR");
        }

        public static IIdentityProvider GetIdentityProvider(string role = null)
        {
            var identity = GetIdentityFromEnvironment(role);
            var identityEndpoint = GetIdentityEndpointFromEnvironment();
            return new OpenStackIdentityProvider(identityEndpoint, identity)
            {
                ApplicationUserAgent = new ProductInfoHeaderValue("(CI-BOT)").ToString()
            };
        }

        public static Uri GetIdentityEndpointFromEnvironment()
        {
            var identityUrl = Environment.GetEnvironmentVariable("OPENSTACKNET_IDENTITY_URL");
            if(!string.IsNullOrEmpty(identityUrl))
                return new Uri(identityUrl);

            identityUrl = Environment.GetEnvironmentVariable("BAMBOO_OPENSTACKNET_IDENTITY_URL");
            if (!string.IsNullOrEmpty(identityUrl))
                return new Uri(identityUrl);

            throw new Exception(EnvironmentVariablesNotFoundErrorMessage);
        }

        public static CloudIdentity GetIdentityFromEnvironment(string role = null)
        {
            string roleSuffix = null;
            if (role != null)
                roleSuffix = role + "_";

            var user = Environment.GetEnvironmentVariable($"OPENSTACKNET_{roleSuffix}USER");
            if (!string.IsNullOrEmpty(user))
            {
                var password = Environment.GetEnvironmentVariable($"OPENSTACKNET_{roleSuffix}PASSWORD");
                var projectName = Environment.GetEnvironmentVariable($"OPENSTACKNET_{roleSuffix}PROJECT");

                if (!string.IsNullOrEmpty(password) && !string.IsNullOrEmpty(projectName))
                {
                    return new CloudIdentityWithProject
                    {
                        Username = user,
                        Password = password,
                        ProjectName = projectName
                    };
                }
            }

            user = Environment.GetEnvironmentVariable($"BAMBOO_OPENSTACKNET_{roleSuffix}USER");
            if (!string.IsNullOrEmpty(user))
            {
                var password = Environment.GetEnvironmentVariable($"BAMBOO_OPENSTACKNET_{roleSuffix}PASSWORD");
                var projectName = Environment.GetEnvironmentVariable($"BAMBOO_OPENSTACKNET_{roleSuffix}PROJECT");

                if (!string.IsNullOrEmpty(password) && !string.IsNullOrEmpty(projectName))
                {
                    return new CloudIdentityWithProject
                    {
                        Username = user,
                        Password = password,
                        ProjectName = projectName
                    };
                }
            }

            throw new Exception(string.Format(EnvironmentVariablesNotFoundErrorMessage, roleSuffix));
        }
    }
}