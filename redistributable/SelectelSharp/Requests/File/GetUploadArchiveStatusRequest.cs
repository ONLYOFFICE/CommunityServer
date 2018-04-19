using SelectelSharp.Headers;
using System.Collections.Specialized;
using System.Net;

namespace SelectelSharp.Requests.File
{
    public class GetUploadArchiveStatusRequest : 
        BaseRequest<string>
    {
        private string ExtractId { get; set; }

        internal override RequestMethod Method
        {
            get
            {
                return RequestMethod.GET;
            }
        }


        public GetUploadArchiveStatusRequest(string extractId)
            : base()
        {
            TryAddHeader(HeaderKeys.Accept, HeaderKeys.AcceptJson);
            this.ExtractId = extractId;
        }

        protected override string GetUrl(string storageUrl)
        {
            return string.Format("https://api.selcdn.ru/v1/extract-archive/{0}", this.ExtractId);
        }

        internal override void Parse(NameValueCollection headers, object content, HttpStatusCode status)
        {
            base.Parse(headers, content, status);
        }
    }
}
