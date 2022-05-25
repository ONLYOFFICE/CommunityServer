namespace net.openstack.Providers.Rackspace.Objects.Response
{
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;

    [JsonObject(MemberSerialization.OptIn)]
    internal class BulkDeleteResponse
    {
        [JsonProperty("Number Not Found")]
        public int NumberNotFound { get; set; }

        [JsonProperty("Response Status")]
        public string Status { get; set; }

        [JsonProperty("Errors")]
        public IEnumerable<IEnumerable<string>> Errors { get; set; }

        [JsonProperty("Number Deleted")]
        public int NumberDeleted { get; set; }

        [JsonProperty("Response Body")]
        public string ResponseBody { get; set; }

        public IEnumerable<string> AllItems { get; set; }  

        public bool IsItemError(string s)
        {
            return Errors.Any(e => e.Any(e2 => e2.Equals(s)));
        }
    }
}
