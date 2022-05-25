using System;
using System.Runtime.Serialization;
using net.openstack.Core.Providers;
using net.openstack.Core.Validators;

namespace net.openstack.Core.Exceptions
{
    /// <summary>
    /// Represents errors that occur while validating an object name for an <see cref="IObjectStorageProvider"/>.
    /// </summary>
    /// <seealso cref="IObjectStorageValidator.ValidateObjectName"/>
    /// <threadsafety static="true" instance="false"/>
    [Serializable]
    public class ObjectNameException : ArgumentException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectNameException"/> class.
        /// </summary>
        public ObjectNameException()
            : base("The specified object name is not valid.")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectNameException"/> class
        /// with the specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public ObjectNameException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectNameException"/> class with
        /// serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="info"/> is <see langword="null"/>.</exception>
        protected ObjectNameException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
