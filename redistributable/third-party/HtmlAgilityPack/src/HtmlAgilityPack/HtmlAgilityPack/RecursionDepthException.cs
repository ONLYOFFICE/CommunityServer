using System;

namespace HtmlAgilityPack
{
    /// <summary>
    /// Represents an exception near stack overflow.
    /// </summary>
    public class RecursionDepthException : Exception
    {
        #region Constructors

        /// <summary>
        /// Creates an instance of the RecursionDepthException
        /// </summary>
        /// <param name="message">The exception's message.</param>
        public RecursionDepthException(string message)
            : base(message)
        {
        }

        #endregion
    }
}
