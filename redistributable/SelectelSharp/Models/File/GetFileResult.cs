#region Import

using SelectelSharp.Headers;
using System.Collections.Specialized;
using System.IO;

#endregion

namespace SelectelSharp.Models.File
{
    public class GetFileResult : FileInfo
    {                
        public Stream ResponseStream { get; set; }

        public GetFileResult(Stream responseStream, string name, NameValueCollection headers)
        {
            HeaderParsers.ParseHeaders(this, headers);
            this.ResponseStream = responseStream;
            this.Name = name;
        }
    }
}
