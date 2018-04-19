using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SelectelSharp.Requests.Container;
using SelectelSharp.Models.Link;
using System.Collections.Specialized;
using System.Net;

namespace SelectelSharp.Requests.File
{
    public class SymlinkRequest : ContainerRequest<bool>
    {
        private string link;

        internal override RequestMethod Method
        {
            get
            {
                return RequestMethod.PUT;
            }
        }

        public SymlinkRequest(string containerName, Symlink link)
            : base(containerName)
        {
            this.link = link.Link;
            SetCustomHeaders(link.GetHeaders());
        }

        internal override void Parse(NameValueCollection headers, object content, HttpStatusCode status)
        {
            if (status == HttpStatusCode.Created && !String.IsNullOrEmpty(headers["Etag"]))
            {
                this.Result = true;
            }
            else
            {
                this.Result = false;
            }
        }

        internal override void ParseError(WebException ex, HttpStatusCode status)
        {
            base.ParseError(ex, status);
        }

        protected override string GetUrl(string storageUrl)
        {
            return string.Format("{0}/{1}/{2}", storageUrl, this.ContainerName, this.link);
        }
    }
}
