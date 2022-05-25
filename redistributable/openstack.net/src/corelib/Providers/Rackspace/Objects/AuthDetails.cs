namespace net.openstack.Providers.Rackspace.Objects
{
    using Newtonsoft.Json;

    [JsonObject(MemberSerialization.OptIn)]
    internal class AuthDetails
    {
        [JsonProperty("passwordCredentials", DefaultValueHandling = DefaultValueHandling.Include)]
        public Credentials PasswordCredentials { get; set; }

        [JsonProperty("RAX-KSKEY:apiKeyCredentials", DefaultValueHandling = DefaultValueHandling.Include)]
        public Credentials APIKeyCredentials { get; set; }

        [JsonProperty("RAX-AUTH:domain", DefaultValueHandling = DefaultValueHandling.Include)]
        public Domain Domain { get; set; }
    }
}
