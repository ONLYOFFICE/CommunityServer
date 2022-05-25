namespace net.openstack.Providers.Rackspace
{
    using System;
    using System.Net;

    /// <summary>
    /// This class contains data for events that happen in the context of a
    /// <see cref="WebRequest"/>.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    public class WebRequestEventArgs : EventArgs
    {
        /// <summary>
        /// This is the backing field for the <see cref="Request"/> property.
        /// </summary>
        private readonly HttpWebRequest _request;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebRequestEventArgs"/> class
        /// with the specified web request.
        /// </summary>
        /// <param name="request">The HTTP web request.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="request"/> is <see langword="null"/>.</exception>
        public WebRequestEventArgs(HttpWebRequest request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            _request = request;
        }

        /// <summary>
        /// Gets the <see cref="HttpWebRequest"/> associated with the event.
        /// </summary>
        public HttpWebRequest Request
        {
            get
            {
                return _request;
            }
        }
    }
}
