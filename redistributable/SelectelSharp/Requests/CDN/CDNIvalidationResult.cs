using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelectelSharp.Requests.CDN
{
    public class CDNIvalidationResult
    {
        [JsonProperty("estimatedSeconds")] 
        public int EstimatedSeconds { get; private set; }

        [JsonProperty("progressUri")] 
        public Uri ProgressUri { get; private set; }

        [JsonProperty("supportId")] 
        public String SupportId { get; private set; }

        [JsonProperty("PurgeId")]
        public String PurgeId { get; private set; }

        [JsonProperty("httpStatus")] 
        public int HttpStatus { get; private set; }

        [JsonProperty("pingAfterSeconds")] 
        public int PingAfterSeconds { get; private set; }

        [JsonProperty("detail")] 
        public String Detail { get; private set; }
    }
}
