using Newtonsoft.Json;
using SelectelSharp.Common;
using SelectelSharp.Headers;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace SelectelSharp.Models.File
{
    public class UploadArchiveResult
    {
        [JsonProperty("Response Status")]
        public string ResponseStatus { get; set; }

        [JsonProperty("Response Body")]
        public string ResponseBody { get; set; }

        public ReadOnlyCollection<string> Errors { get; set; }

        [JsonProperty("Number Files Created")]
        public int NumberFilesCreated { get; set; }

        [Header(HeaderKeys.ExtractId)]
        public string ExtractId { get; set; }

        public UploadArchiveResult()
        { }

        public UploadArchiveResult(NameValueCollection headers)
        {
            HeaderParsers.ParseHeaders(this, headers);
        }
    }
}
