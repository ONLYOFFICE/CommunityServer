using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using net.openstack.Core.Domain;

namespace net.openstack.Core
{
    /// <summary>
    /// A status parser for HTTP status codes.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    public class HttpStatusCodeParser : IStatusParser
    {
        /// <summary>
        /// The default regular expression to use for matching HTTP status codes.
        /// </summary>
        protected static readonly string DefaultPattern = @"(?<StatusCode>\d*)\s*(?<Status>\w*)";

        /// <summary>
        /// A singleton instance of the default <see cref="HttpStatusCodeParser"/>.
        /// </summary>
        private static readonly HttpStatusCodeParser _default = new HttpStatusCodeParser(DefaultPattern);

        /// <summary>
        /// The compiled regular expression to use for matching HTTP status codes.
        /// </summary>
        private readonly Regex _expression;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpStatusCodeParser"/> class for the default regular
        /// expression.
        /// </summary>
        [Obsolete("Use HttpStatusCodeParser.Default instead.")]
        public HttpStatusCodeParser()
            : this(DefaultPattern)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpStatusCodeParser"/> class for the specified regular
        /// expression.
        /// </summary>
        /// <param name="pattern">
        /// The regular expression pattern to use.
        ///
        /// <para><paramref name="pattern"/> should contain the named capturing grounds <c>StatusCode</c> and <c>status</c>.</para>
        /// </param>
        /// <exception cref="ArgumentNullException"><paramref name="pattern"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="pattern"/> does not contain a capturing group named <c>StatusCode</c>.
        /// <para>-or-</para>
        /// <para><paramref name="pattern"/> does not contain a capturing group named <c>Status</c>.</para>
        /// </exception>
        protected HttpStatusCodeParser(string pattern)
        {
            if (pattern == null)
                throw new ArgumentNullException("pattern");

            _expression = new Regex(pattern, RegexOptions.Compiled);

            string[] groupNames = _expression.GetGroupNames();
            if (!groupNames.Contains("StatusCode", StringComparer.Ordinal))
                throw new ArgumentException("The pattern does not contain a StatusCode named capturing group.", "pattern");
            if (!groupNames.Contains("Status", StringComparer.Ordinal))
                throw new ArgumentException("The pattern does not contain a Status named capturing group.", "pattern");
        }

        /// <summary>
        /// Gets a default <see cref="HttpStatusCodeParser"/>.
        /// </summary>
        public static HttpStatusCodeParser Default
        {
            get
            {
                return _default;
            }
        }

        /// <inheritdoc/>
        public virtual bool TryParse(string value, out Status status)
        {
            if (value == null)
            {
                status = null;
                return false;
            }

            var match = _expression.Match(value);
            if (!match.Success)
            {
                status = null;
                return false;
            }

            HttpStatusCode statusCode = (HttpStatusCode)int.Parse(match.Groups["StatusCode"].Value);
            string description = match.Groups["Status"].Value;
            if (string.IsNullOrEmpty(description))
                description = statusCode.ToString();

            status = new Status((int)statusCode, description);
            return true;
        }
    }
}
