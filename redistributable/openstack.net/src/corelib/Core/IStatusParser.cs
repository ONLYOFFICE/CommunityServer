using net.openstack.Core.Domain;

namespace net.openstack.Core
{
    /// <summary>
    /// Represents an object which can convert a string to a <see cref="Status"/> object
    /// containing a status code and a textual representation of that status.
    /// </summary>
    public interface IStatusParser
    {
        /// <summary>
        /// Converts a string to an equivalent <see cref="Status"/> object. A return value indicates whether the operation succeeded.
        /// </summary>
        /// <param name="value">A string containing a status to convert.</param>
        /// <param name="status">When this method returns, contains a <see cref="Status"/> instance
        /// equivalent to <paramref name="value"/>, if the conversion succeeded, or <see langword="null"/> if
        /// the conversion failed. The conversion fails if the <paramref name="value"/> parameter is
        /// <see langword="null"/> or is not of the correct format.
        /// </param>
        /// <returns><see langword="true"/> if <paramref name="value"/> was converted successfully; otherwise, <see langword="false"/>.</returns>
        bool TryParse(string value, out Status status);
    }
}
