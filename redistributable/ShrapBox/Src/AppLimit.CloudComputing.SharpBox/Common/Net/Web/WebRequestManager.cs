using System;
using System.IO;
using System.Net;

namespace AppLimit.CloudComputing.SharpBox.Common.Net.Web
{
    /// <summary>
    /// This delegate can be used to defines callbacks which will be called after
    /// a webrequest is processed
    /// </summary>
    /// <param name="sender">the caller of the webrequest</param>
    /// <param name="e">The executed evemt args</param>
    public delegate void WebRequestExecuted(Object sender, WebRequestExecutedEventArgs e);

    /// <summary>
    /// This delegate can be used to defines callbacks which will be called before
    /// a webrequest is processed
    /// </summary>
    /// <param name="sender">the caller of the webrequest</param>
    /// <param name="e">the executed event args</param>
    public delegate void WebRequestExecuting(Object sender, WebRequestExecutingEventArgs e);

    /// <summary>
    /// This class allows to be informed about all oAuth requests in the 
    /// library. Do not add event receiver during runtime, only during start
    /// up of your application
    /// </summary>
    public sealed class WebRequestManager
    {
        #region Singleton Stuff

        private static readonly WebRequestManager instance = new WebRequestManager();

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static WebRequestManager()
        {
        }

        private WebRequestManager()
        {
            _webProxySettings = null;
        }

        /// <summary>
        /// This is the instance accessor of the oAuthServiceManager
        /// </summary>
        public static WebRequestManager Instance
        {
            get { return instance; }
        }

        #endregion

        private IWebProxy _webProxySettings;

        /// <summary>
        /// This event will be raised when the webrequest is prepared
        /// </summary>
        public event WebRequestExecuting RequestPreparedEvent;

        /// <summary>
        /// This event will beraised before a webrequest will be executed
        /// </summary>
        public event WebRequestExecuting RequestExecutingEvent;

        /// <summary>
        /// This event will be raised after a webrequest was executed
        /// </summary>
        public event WebRequestExecuted RequestExecutedEvent;

        internal void NotifyWebRequestPrepared(WebRequest r)
        {
            if (RequestPreparedEvent != null)
            {
                var e = new WebRequestExecutingEventArgs() { request = r };
                RequestPreparedEvent(this, e);
            }
        }

        internal void NotifyWebRequestExecuting(WebRequest r)
        {
            if (RequestExecutingEvent != null)
            {
                var e = new WebRequestExecutingEventArgs() { request = r };
                RequestExecutingEvent(this, e);
            }
        }

        internal void NotifyWebRequestExecuted(WebResponse response, TimeSpan timeNeeded, Stream resultStream, WebException exception)
        {
            if (RequestExecutedEvent != null)
            {
                var e = new WebRequestExecutedEventArgs
                    {
                        response = response,
                        timeNeeded = timeNeeded,
                        resultStream = resultStream,
                        exception = exception
                    };

                RequestExecutedEvent(this, e);
            }
        }


        /// <summary>
        /// This method allows to set an alternative proxy host. The system wide 
        /// settings wil be overwritten
        /// </summary>
        /// <param name="proxyHost"></param>
        /// <param name="proxyPort"></param>
        /// <param name="credentials"></param>
        public void SetProxySettings(String proxyHost, int proxyPort, ICredentials credentials)
        {
            if (proxyHost != null)
            {
                _webProxySettings = new WebProxy(proxyHost, proxyPort);
                _webProxySettings.Credentials = credentials;
            }
            else
                _webProxySettings = null;
        }

        internal IWebProxy GetProxySettings()
        {
            return _webProxySettings;
        }

        /// <summary>
        /// This method remove the proxy settings and ensures that no proxy will be used
        /// at all
        /// </summary>
        public void SetEmptyProxySettings()
        {
            _webProxySettings = new WebRequestManagerNullProxy();
        }
    }
}