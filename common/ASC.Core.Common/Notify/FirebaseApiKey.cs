using System;
using System.Configuration;

using Newtonsoft.Json;

namespace ASC.Core.Common.Notify
{
    [Serializable]
    class FirebaseApiKey
    {
        [JsonProperty("type")]
        public string Type
        {
            get
            {
                return "service_account";
            }
        }
        [JsonProperty("project_id")]
        public string ProjectId
        {
            get
            {
                return ConfigurationManagerExtension.AppSettings["firebase.project.id"] ?? String.Empty;
            }
        }
        [JsonProperty("private_key_id")]
        public string PrivateKeyId
        {
            get
            {
                return ConfigurationManagerExtension.AppSettings["firebase.private.key.id"] ?? String.Empty;
            }
        }
        [JsonProperty("private_key")]
        public string PrivateKey
        {
            get
            {
                return ConfigurationManagerExtension.AppSettings["firebase.private.key"] ?? String.Empty;
            }
        }
        [JsonProperty("client_email")]
        public string ClientEmail
        {
            get
            {
                return ConfigurationManagerExtension.AppSettings["firebase.client.email"] ?? String.Empty;
            }
        }
        [JsonProperty("client_id")]
        public string ClientId
        {
            get
            {
                return ConfigurationManagerExtension.AppSettings["firebase.client.id"] ?? String.Empty;
            }
        }
        [JsonProperty("auth_uri")]
        public string AuthUri
        {
            get
            {
                return "https://accounts.google.com/o/oauth2/auth";
            }
        }
        [JsonProperty("token_uri")]
        public string TokenUri
        {
            get
            {
                return "https://oauth2.googleapis.com/token";
            }
        }
        [JsonProperty("auth_provider_x509_cert_url")]
        public string AuthProviderX509CertUrl
        {
            get
            {
                return "https://www.googleapis.com/oauth2/v1/certs";
            }
        }
        [JsonProperty("client_x509_cert_url")]
        public string ClientX509CertUrl
        {
            get
            {
                return ConfigurationManagerExtension.AppSettings["firebase.x509.cert.url"] ?? String.Empty;
            }
        }
    }
}
