namespace net.openstack.Providers.Rackspace
{
    using net.openstack.Core;

    /// <summary>
    /// Provides a default implementation of <see cref="IEncodeDecodeProvider"/> for
    /// use with Rackspace services. This implementation encodes text using
    /// <see cref="UriUtility.UriEncode(string, UriPart)"/> with <see cref="UriPart.AnyUrl"/>,
    /// and decodes text with <see cref="UriUtility.UriDecode(string)"/>.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    internal class EncodeDecodeProvider : IEncodeDecodeProvider
    {
        /// <summary>
        /// A default instance of <see cref="EncodeDecodeProvider"/>.
        /// </summary>
        private static readonly EncodeDecodeProvider _default = new EncodeDecodeProvider();

        /// <summary>
        /// Gets a default instance of <see cref="EncodeDecodeProvider"/>.
        /// </summary>
        public static EncodeDecodeProvider Default
        {
            get
            {
                return _default;
            }
        }

        /// <inheritdoc/>
        public string UrlEncode(string stringToEncode)
        {
            if (stringToEncode == null)
                return null;

            return UriUtility.UriEncode(stringToEncode, UriPart.AnyUrl);
        }

        /// <inheritdoc/>
        public string UrlDecode(string stringToDecode)
        {
            if (stringToDecode == null)
                return null;

            return UriUtility.UriDecode(stringToDecode);
        }
    }
}
