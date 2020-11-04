#region Import

using SelectelSharp.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#endregion

namespace SelectelSharp.Requests.File
{
    public class CopyFileRequest : FileRequest<bool>
    {
        public CopyFileRequest(String container, String path, String newContainer, String newPath) : base(container, path)
        {
            TryAddHeader(HeaderKeys.Destination, String.Format("{0}/{1}", newContainer, newPath));            
        }

        internal override void Parse(System.Collections.Specialized.NameValueCollection headers, object content, System.Net.HttpStatusCode status)
        {
            if (status == System.Net.HttpStatusCode.Created)
            {
                this.Result = true;
            }
            else
            {
                this.Result = false;
            }
        }

        internal override RequestMethod Method
        {
            get
            {
                return RequestMethod.COPY;
            }
        }


    }
}
