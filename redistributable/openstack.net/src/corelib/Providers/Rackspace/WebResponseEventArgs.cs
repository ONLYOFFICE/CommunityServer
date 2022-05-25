namespace net.openstack.Providers.Rackspace
{
    using System;
    using System.Net;

    /// <summary>
    /// This class contains data for events that happen in the context of a
    /// <see cref="WebResponse"/>.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    public class WebResponseEventArgs : EventArgs
    {
        /// <summary>
        /// This is the backing field for the <see cref="Response"/> property.
        /// </summary>
        private readonly HttpWebResponse _response;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebResponseEventArgs"/> class
        /// with the specified web response.
        /// </summary>
        /// <param name="response">The HTTP web response.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="response"/> is <see langword="null"/>.</exception>
        public WebResponseEventArgs(HttpWebResponse response)
        {
            if (response == null)
                throw new ArgumentNullException("response");

            _response = response;
        }

        /// <summary>
        /// Gets the <see cref="HttpWebResponse"/> associated with the event.
        /// </summary>
        public HttpWebResponse Response
        {
            get
            {
                return _response;
            }
        }
    }
}
