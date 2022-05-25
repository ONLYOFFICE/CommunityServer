using System;
using System.Runtime.Serialization;
using net.openstack.Core.Validators;

namespace net.openstack.Core.Exceptions
{
    /// <summary>
    /// Represents errors that occur while validating a CIDR.
    /// </summary>
    /// <seealso cref="INetworksValidator.ValidateCidr"/>
    /// <threadsafety static="true" instance="false"/>
    [Serializable]
    public class CidrFormatException : FormatException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CidrFormatException"/> class.
        /// </summary>
        public CidrFormatException()
            : base("The specified CIDR is not in the correct format.")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CidrFormatException"/> class
        /// with the specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public CidrFormatException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CidrFormatException"/> class with
        /// serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="info"/> is <see langword="null"/>.</exception>
        protected CidrFormatException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
