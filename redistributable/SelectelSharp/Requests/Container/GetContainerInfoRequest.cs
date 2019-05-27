using SelectelSharp.Models.Container;
using System.Collections.Specialized;
using System.Net;

namespace SelectelSharp.Requests.Container
{
    public class GetContainerInfoRequest : ContainerRequest<ContainerInfo>
    {
        public GetContainerInfoRequest(string containerName) : base(containerName) { }

        internal override RequestMethod Method
        {
            get
            {
                return RequestMethod.HEAD;
            }
        }

        internal override void Parse(NameValueCollection headers, object data, HttpStatusCode status)
        {
            this.Result = new ContainerInfo(headers);
        }

        internal override void ParseError(WebException ex, HttpStatusCode status)
        {
            if (status == HttpStatusCode.NotFound)
            {
                this.Result = null;
            }
            else
            {
                base.ParseError(ex, status);
            }
        }
    }
}
