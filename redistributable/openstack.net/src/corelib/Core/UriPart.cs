namespace net.openstack.Core
{
    /// <summary>
    /// Represents a specific part of a URI.
    /// </summary>
    /// <preliminary/>
    public enum UriPart
    {
        /// <summary>
        /// Represents a non-specific URI part, where all characters except unreserved characters are percent-encoded.
        /// </summary>
        Any,

        /// <summary>
        /// Represents a non-specific URL part, where all characters except unreserved characters and the characters <c>(</c>, <c>)</c>, <c>!</c>, and <c>*</c> are percent-encoded.
        /// </summary>
        /// <remarks>
        /// When used with <see cref="UriUtility.UriEncode(string, UriPart)"/>, this URI part matches the behavior of <see cref="M:System.Web.HttpUtility.UrlEncode(string)"/>, with the exception of space characters (this method percent-encodes space characters as <c>%20</c>).
        /// </remarks>
        AnyUrl,

        /// <summary>
        /// The host part of a URI.
        /// </summary>
        Host,

        /// <summary>
        /// The complete path of a URI, formed from path segments separated by slashes (<c>/</c> characters).
        /// </summary>
        Path,

        /// <summary>
        /// A single segment of a URI path.
        /// </summary>
        PathSegment,

        /// <summary>
        /// The complete query string of a URI.
        /// </summary>
        Query,

        /// <summary>
        /// The value assigned to a query parameter within the query string of a URI.
        /// </summary>
        QueryValue,

        /// <summary>
        /// The fragment part of a URI.
        /// </summary>
        Fragment,
    }
}
