using SelectelSharp.Models.File;
using System.Collections.Specialized;
using System.Net;

namespace SelectelSharp.Requests.File
{
    public class DeleteFileRequest : FileRequest<DeleteFileResult>
    {
        public DeleteFileRequest(string containerName, string fileName) 
            : base(containerName, fileName)
        {
        }

        internal override RequestMethod Method
        {
            get
            {
                return RequestMethod.DELETE;
            }
        }

        internal override void Parse(NameValueCollection headers, object data, HttpStatusCode status)
        {
            if (status == HttpStatusCode.NoContent)
            {
                this.Result = DeleteFileResult.Deleted;
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
                this.Result = DeleteFileResult.NotFound;
            }
            else
            {
                base.ParseError(ex, status);
            }
        }
    }
}
