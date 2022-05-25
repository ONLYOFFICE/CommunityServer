using System;
using System.Runtime.Serialization;
using net.openstack.Core.Providers;

namespace net.openstack.Core.Exceptions
{
    /// <summary>
    /// Thrown when an Object Storage container operation fails because the specified container was not empty.
    /// </summary>
    /// <seealso cref="IObjectStorageProvider.DeleteContainer"/>
    /// <threadsafety static="true" instance="false"/>
    [Serializable]
    public class ContainerNotEmptyException : InvalidOperationException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContainerNotEmptyException"/> class.
        /// </summary>
        public ContainerNotEmptyException()
            : base("The operation failed because the specified container is not empty.")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContainerNotEmptyException"/> class
        /// with the specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public ContainerNotEmptyException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContainerNotEmptyException"/> class
        /// with the specified error message and a reference to the inner exception that is
        /// the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">
        /// The exception that is the cause of the current exception. If the <paramref name="innerException"/>
        /// parameter is not <see langword="null"/>, the current exception is raised in a catch block that handles the
        /// inner exception.
        /// </param>
        public ContainerNotEmptyException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContainerNotEmptyException"/> class with
        /// serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="info"/> is <see langword="null"/>.</exception>
        protected ContainerNotEmptyException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
