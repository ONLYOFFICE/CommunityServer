namespace net.openstack.Core
{
    using System;
    using BitArray = System.Collections.BitArray;
    using Encoding = System.Text.Encoding;
    using MatchEvaluator = System.Text.RegularExpressions.MatchEvaluator;
    using NumberStyles = System.Globalization.NumberStyles;
    using Regex = System.Text.RegularExpressions.Regex;
    using RegexOptions = System.Text.RegularExpressions.RegexOptions;
    using StringBuilder = System.Text.StringBuilder;

    /// <summary>
    /// Provides static utility methods for encoding and decoding text within
    /// RFC 3986 URIs.
    /// </summary>
    /// <seealso href="http://www.ietf.org/rfc/rfc3986">RFC 3986: URI Generic Syntax</seealso>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    public static class UriUtility
    {
        private static readonly BitArray _unreservedCharacters;
        private static readonly BitArray _generalDelimiters;
        private static readonly BitArray _subDelimiters;
        private static readonly BitArray _reservedCharacters;
        private static readonly BitArray _allowedHostCharacters;
        private static readonly BitArray _allowedPathCharacters;
        private static readonly BitArray _allowedQueryCharacters;
        private static readonly BitArray _allowedFragmentCharacters;

        /// <summary>
        /// This is the regular expression for a single percent-encoded character.
        /// </summary>
        private static readonly Regex _percentEncodedPattern = new Regex(@"%([0-9A-Fa-f][0-9A-Fa-f])", RegexOptions.Compiled);

        static UriUtility()
        {
            _unreservedCharacters = new BitArray(256);
            for (char i = 'a'; i <= 'z'; i++)
                _unreservedCharacters.Set(i, true);
            for (char i = 'A'; i <= 'Z'; i++)
                _unreservedCharacters.Set(i, true);
            for (char i = '0'; i <= '9'; i++)
                _unreservedCharacters.Set(i, true);
            _unreservedCharacters.Set('-', true);
            _unreservedCharacters.Set('.', true);
            _unreservedCharacters.Set('_', true);
            _unreservedCharacters.Set('~', true);

            _generalDelimiters = new BitArray(256);
            _generalDelimiters.Set(':', true);
            _generalDelimiters.Set('/', true);
            _generalDelimiters.Set('?', true);
            _generalDelimiters.Set('#', true);
            _generalDelimiters.Set('[', true);
            _generalDelimiters.Set(']', true);
            _generalDelimiters.Set('@', true);

            _subDelimiters = new BitArray(256);
            _subDelimiters.Set('!', true);
            _subDelimiters.Set('$', true);
            _subDelimiters.Set('&', true);
            _subDelimiters.Set('(', true);
            _subDelimiters.Set(')', true);
            _subDelimiters.Set('*', true);
            _subDelimiters.Set('+', true);
            _subDelimiters.Set(',', true);
            _subDelimiters.Set(';', true);
            _subDelimiters.Set('=', true);
            _subDelimiters.Set('\'', true);

            _reservedCharacters = new BitArray(256).Or(_generalDelimiters).Or(_subDelimiters);

            _allowedHostCharacters = new BitArray(256).Or(_unreservedCharacters).Or(_subDelimiters);

            _allowedPathCharacters = new BitArray(256).Or(_unreservedCharacters).Or(_subDelimiters);
            _allowedPathCharacters.Set(':', true);
            _allowedPathCharacters.Set('@', true);

            _allowedQueryCharacters = new BitArray(256).Or(_allowedPathCharacters);
            _allowedQueryCharacters.Set('/', true);
            _allowedQueryCharacters.Set('?', true);

            _allowedFragmentCharacters = new BitArray(256).Or(_allowedPathCharacters);
            _allowedFragmentCharacters.Set('/', true);
            _allowedFragmentCharacters.Set('?', true);
        }

        /// <summary>
        /// Decodes the text of a URI by unescaping any percent-encoded character sequences and
        /// then evaluating the result using the default <see cref="Encoding.UTF8"/> encoding.
        /// </summary>
        /// <remarks>
        /// This method calls <see cref="UriDecode(string, Encoding)"/> using the default
        /// <see cref="Encoding.UTF8"/> encoding.
        /// </remarks>
        /// <param name="text">The encoded URI.</param>
        /// <returns>The decoded URI text.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="text"/> is <see langword="null"/>.</exception>
        public static string UriDecode(string text)
        {
            return UriDecode(text, Encoding.UTF8);
        }

        /// <summary>
        /// Decodes the text of a URI by unescaping any percent-encoded character sequences and
        /// then evaluating the result using the specified encoding.
        /// </summary>
        /// <param name="text">The encoded URI.</param>
        /// <param name="encoding">The encoding to use for Unicode characters in the URI. If this value is <see langword="null"/>, the <see cref="Encoding.UTF8"/> encoding will be used.</param>
        /// <returns>The decoded URI text.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="text"/> is <see langword="null"/>.</exception>
        public static string UriDecode(string text, Encoding encoding)
        {
            if (text == null)
                throw new ArgumentNullException("text");

            encoding = encoding ?? Encoding.UTF8;
            MatchEvaluator matchEvaluator =
                match =>
                {
                    string hexValue = match.Groups[1].Value;
                    return ((char)byte.Parse(hexValue, NumberStyles.HexNumber)).ToString();
                };
            string decodedText = _percentEncodedPattern.Replace(text, matchEvaluator);
            byte[] data = Array.ConvertAll(decodedText.ToCharArray(), c => (byte)c);
            return encoding.GetString(data);
        }

        /// <summary>
        /// Encodes text for inclusion in a URI using the <see cref="Encoding.UTF8"/> encoding.
        /// </summary>
        /// <remarks>
        /// This method calls <see cref="UriEncode(string, UriPart, Encoding)"/> using the default
        /// <see cref="Encoding.UTF8"/> encoding.
        /// </remarks>
        /// <param name="text">The text to encode for inclusion in a URI.</param>
        /// <param name="uriPart">A <see cref="UriPart"/> value indicating where in the URI the specified text will be included.</param>
        /// <returns>The URI-encoded text, suitable for the specified URI part.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="text"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="uriPart"/> is not a valid <see cref="UriPart"/>.</exception>
        public static string UriEncode(string text, UriPart uriPart)
        {
            return UriEncode(text, uriPart, Encoding.UTF8);
        }

        /// <summary>
        /// Encodes text for inclusion in a URI.
        /// </summary>
        /// <param name="text">The text to encode for inclusion in a URI.</param>
        /// <param name="uriPart">A <see cref="UriPart"/> value indicating where in the URI the specified text will be included.</param>
        /// <param name="encoding">The encoding to use for Unicode characters in the URI. If this value is <see langword="null"/>, the <see cref="Encoding.UTF8"/> encoding will be used.</param>
        /// <returns>The URI-encoded text, suitable for the specified URI part.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="text"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="uriPart"/> is not a valid <see cref="UriPart"/>.</exception>
        public static string UriEncode(string text, UriPart uriPart, Encoding encoding)
        {
            if (text == null)
                throw new ArgumentNullException("text");

            encoding = encoding ?? Encoding.UTF8;
            switch (uriPart)
            {
            case UriPart.Any:
                return UriEncodeAny(text, encoding);

            case UriPart.AnyUrl:
                return UriEncodeAnyUrl(text, encoding);

            case UriPart.Host:
                return UriEncodeHost(text, encoding);

            case UriPart.Path:
                return UriEncodePath(text, encoding);

            case UriPart.PathSegment:
                return UriEncodePathSegment(text, encoding);

            case UriPart.Query:
                return UriEncodeQuery(text, encoding);

            case UriPart.QueryValue:
                return UriEncodeQueryValue(text, encoding);

            case UriPart.Fragment:
                return UriEncodeFragment(text, encoding);

            default:
                throw new ArgumentException("The specified uriPart is not valid.", "uriPart");
            }
        }

        private static string UriEncodeAny(string text, Encoding encoding)
        {
            if (text == null)
                throw new ArgumentNullException("text");
            if (encoding == null)
                throw new ArgumentNullException("encoding");

            if (text.Length == 0)
                return text;

            StringBuilder builder = new StringBuilder();
            byte[] data = encoding.GetBytes(text);
            for (int i = 0; i < data.Length; i++)
            {
                if (_unreservedCharacters[data[i]])
                    builder.Append((char)data[i]);
                else
                    builder.Append('%').Append(data[i].ToString("x2"));
            }

            return builder.ToString();
        }

        private static string UriEncodeAnyUrl(string text, Encoding encoding)
        {
            if (text == null)
                throw new ArgumentNullException("text");
            if (encoding == null)
                throw new ArgumentNullException("encoding");

            if (text.Length == 0)
                return text;

            StringBuilder builder = new StringBuilder();
            byte[] data = encoding.GetBytes(text);
            for (int i = 0; i < data.Length; i++)
            {
                if (_unreservedCharacters[data[i]])
                {
                    builder.Append((char)data[i]);
                }
                else
                {
                    switch ((char)data[i])
                    {
                    case '(':
                    case ')':
                    case '*':
                    case '!':
                        builder.Append((char)data[i]);
                        break;

                    default:
                        builder.Append('%').Append(data[i].ToString("x2"));
                        break;
                    }
                }
            }

            return builder.ToString();
        }

        private static string UriEncodeHost(string text, Encoding encoding)
        {
            if (text == null)
                throw new ArgumentNullException("text");
            if (encoding == null)
                throw new ArgumentNullException("encoding");

            if (text.Length == 0)
                return text;

            StringBuilder builder = new StringBuilder();
            byte[] data = encoding.GetBytes(text);
            for (int i = 0; i < data.Length; i++)
            {
                if (_allowedHostCharacters[data[i]])
                    builder.Append((char)data[i]);
                else
                    builder.Append('%').Append(data[i].ToString("x2"));
            }

            return builder.ToString();
        }

        private static string UriEncodePath(string text, Encoding encoding)
        {
            if (text == null)
                throw new ArgumentNullException("text");
            if (encoding == null)
                throw new ArgumentNullException("encoding");

            if (text.Length == 0)
                return text;

            StringBuilder builder = new StringBuilder();
            byte[] data = encoding.GetBytes(text);
            for (int i = 0; i < data.Length; i++)
            {
                if (_allowedPathCharacters[data[i]] || data[i] == '/')
                    builder.Append((char)data[i]);
                else
                    builder.Append('%').Append(data[i].ToString("x2"));
            }

            return builder.ToString();
        }

        private static string UriEncodePathSegment(string text, Encoding encoding)
        {
            if (text == null)
                throw new ArgumentNullException("text");
            if (encoding == null)
                throw new ArgumentNullException("encoding");

            if (text.Length == 0)
                return text;

            StringBuilder builder = new StringBuilder();
            byte[] data = encoding.GetBytes(text);
            for (int i = 0; i < data.Length; i++)
            {
                if (_allowedPathCharacters[data[i]])
                    builder.Append((char)data[i]);
                else
                    builder.Append('%').Append(data[i].ToString("x2"));
            }

            return builder.ToString();
        }

        private static string UriEncodeQuery(string text, Encoding encoding)
        {
            if (text == null)
                throw new ArgumentNullException("text");
            if (encoding == null)
                throw new ArgumentNullException("encoding");

            if (text.Length == 0)
                return text;

            StringBuilder builder = new StringBuilder();
            byte[] data = encoding.GetBytes(text);
            for (int i = 0; i < data.Length; i++)
            {
                if (_allowedQueryCharacters[data[i]])
                    builder.Append((char)data[i]);
                else
                    builder.Append('%').Append(data[i].ToString("x2"));
            }

            return builder.ToString();
        }

        private static string UriEncodeQueryValue(string text, Encoding encoding)
        {
            if (text == null)
                throw new ArgumentNullException("text");
            if (encoding == null)
                throw new ArgumentNullException("encoding");

            if (text.Length == 0)
                return text;

            StringBuilder builder = new StringBuilder();
            byte[] data = encoding.GetBytes(text);
            for (int i = 0; i < data.Length; i++)
            {
                if (_allowedQueryCharacters[data[i]] && data[i] != '&')
                    builder.Append((char)data[i]);
                else
                    builder.Append('%').Append(data[i].ToString("x2"));
            }

            return builder.ToString();
        }

        private static string UriEncodeFragment(string text, Encoding encoding)
        {
            if (text == null)
                throw new ArgumentNullException("text");
            if (encoding == null)
                throw new ArgumentNullException("encoding");

            if (text.Length == 0)
                return text;

            StringBuilder builder = new StringBuilder();
            byte[] data = encoding.GetBytes(text);
            for (int i = 0; i < data.Length; i++)
            {
                if (_allowedFragmentCharacters[data[i]])
                    builder.Append((char)data[i]);
                else
                    builder.Append('%').Append(data[i].ToString("x2"));
            }

            return builder.ToString();
        }
    }
}
