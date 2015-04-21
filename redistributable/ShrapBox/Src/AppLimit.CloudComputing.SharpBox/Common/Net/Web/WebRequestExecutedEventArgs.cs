using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

namespace AppLimit.CloudComputing.SharpBox.Common.Net.Web
{
    /// <summary>
    /// This class contains the argument of the WebRequestExecuted event
    /// </summary>
    public class WebRequestExecutedEventArgs : EventArgs    
    {        
        /// <summary>
        /// The web reponse
        /// </summary>
        public WebResponse response;

        /// <summary>
        /// The timespan of the webrequest
        /// </summary>
        public TimeSpan timeNeeded;
        
        /// <summary>
        /// The result as data stream
        /// </summary>
        public Stream resultStream;

        /// <summary>
        /// possibe web exception
        /// </summary>
        public WebException exception;
    }
}
