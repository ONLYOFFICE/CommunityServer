using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace AppLimit.CloudComputing.SharpBox.Common.Net.Web
{
    /// <summary>
    /// This class contains the argument of the WebRequestExecuting event
    /// </summary>
    public class WebRequestExecutingEventArgs : EventArgs
    {
        /// <summary>
        /// The webrequest which has to be processed as self
        /// </summary>
        public WebRequest request;
    }
}
