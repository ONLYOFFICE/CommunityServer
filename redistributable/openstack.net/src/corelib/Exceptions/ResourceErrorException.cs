using System;
using System.Runtime.Serialization;

namespace OpenStack
{
    /// <summary>
    /// The exception that is thrown when the requested resource is in an error state.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    [Serializable]
    public sealed class ResourceErrorException : Exception
    {
        /// <inheritdoc />
        public ResourceErrorException()
        { }

        /// <inheritdoc />
        public ResourceErrorException(string message)
            : base(message)
        { }

        /// <inheritdoc />
        private ResourceErrorException(SerializationInfo info, StreamingContext context) : base(info, context)
        { }
    }
}
