using System;
using System.Runtime.Serialization;
using net.openstack.Core.Providers;
using net.openstack.Core.Validators;

namespace net.openstack.Core.Exceptions
{
    /// <summary>
    /// Represents errors that occur while validating a container name for an <see cref="IObjectStorageProvider"/>.
    /// </summary>
    /// <seealso cref="IObjectStorageValidator.ValidateContainerName"/>
    /// <threadsafety static="true" instance="false"/>
    [Serializable]
    public class ContainerNameException : ArgumentException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContainerNameException"/> class.
        /// </summary>
        public ContainerNameException()
            : base("The specified container name is not valid.")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContainerNameException"/> class
        /// with the specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public ContainerNameException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContainerNameException"/> class with
        /// serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="info"/> is <see langword="null"/>.</exception>
        protected ContainerNameException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
