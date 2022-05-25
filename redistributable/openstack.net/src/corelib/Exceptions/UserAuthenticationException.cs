using System;
using System.Runtime.Serialization;

namespace OpenStack
{
    /// <summary>
    /// The exception thrown when the user authentication process fails, or
    /// the authentication process did not provide a value for the ServiceCatalog.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    [Serializable]
    public class UserAuthenticationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserAuthenticationException" /> class
        /// with the specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="args">The message formatting arguments.</param>
        public UserAuthenticationException(string message, params object[] args)
            : base(string.Format(message, args))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserAuthenticationException"/> class with
        /// serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="info"/> is <see langword="null"/>.</exception>
        protected UserAuthenticationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
