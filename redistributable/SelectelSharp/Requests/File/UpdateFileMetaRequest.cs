using SelectelSharp.Headers;
using SelectelSharp.Models.File;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;

namespace SelectelSharp.Requests.File
{
    public class UpdateFileMetaRequest : FileRequest<UpdateFileResult>
    {
        internal override RequestMethod Method
        {
            get
            {
                return RequestMethod.POST;
            }
        }

        public UpdateFileMetaRequest(
            string containerName, 
            string fileName, 
            IDictionary<string, object> customHeaders = null, 
            CORSHeaders corsHeaders = null)
            : base(containerName, fileName)
        {
            SetCustomHeaders(customHeaders);
            SetCORSHeaders(corsHeaders);
        }

        internal override void Parse(NameValueCollection headers, object data, HttpStatusCode status)
        {
            if (status == HttpStatusCode.NoContent)
            {
                this.Result = UpdateFileResult.Updated;
            }
            else
            {
                base.ParseError(null, status);
            }
        }

        internal override void ParseError(WebException ex, HttpStatusCode status)
        {
            if (status == HttpStatusCode.NotFound)
            {
                this.Result = UpdateFileResult.NotFound;
            }
            else
            {
                base.ParseError(null, status);
            }
        }
    }
}
