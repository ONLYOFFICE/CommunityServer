using SelectelSharp.Headers;
using SelectelSharp.Models;
using SelectelSharp.Models.File;
using System.Collections.Specialized;
using System.IO;
using System.Net;

namespace SelectelSharp.Requests.File
{
    public class GetFileRequest : FileRequest<GetFileResult>
    {
        private bool allowAnonymously;

        public GetFileRequest(string containerName, string fileName, ConditionalHeaders conditionalHeaders = null, bool allowAnonymously = false)
            : base(containerName, fileName)
        {            
            this.allowAnonymously = allowAnonymously;

            SetConditionalHeaders(conditionalHeaders);
        }

        public override bool AllowAnonymously
        {
            get
            {
                return allowAnonymously;
            }
        }

        public override bool DownloadData
        {
            get
            {
                return true;
            }
        }

        internal override void Parse(NameValueCollection headers, object data, HttpStatusCode status)
        {
            if (status == HttpStatusCode.OK)
            {
                this.Result = new GetFileResult((Stream)data, this.FileName, headers);
            }
            else
            {
                ParseError(null, status);
            }
        }
    }
}
