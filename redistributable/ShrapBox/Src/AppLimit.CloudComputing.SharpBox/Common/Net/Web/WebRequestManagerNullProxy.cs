using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace AppLimit.CloudComputing.SharpBox.Common.Net.Web
{
    internal class WebRequestManagerNullProxy : IWebProxy
    {
        public ICredentials Credentials
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public Uri GetProxy(Uri destination)
        {
            throw new NotImplementedException();
        }

        public bool IsBypassed(Uri host)
        {
            throw new NotImplementedException();
        }
    }
}
