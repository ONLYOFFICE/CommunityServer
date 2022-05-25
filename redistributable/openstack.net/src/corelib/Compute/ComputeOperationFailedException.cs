using System;
using System.Runtime.Serialization;

namespace OpenStack.Compute
{
    /// <summary>
    /// The exception that is thrown when a Compute service operation (Create, Delete, Update) fails.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    [Serializable]
    public sealed class ComputeOperationFailedException : Exception
    {
        /// <inheritdoc />
        public ComputeOperationFailedException()
        { }

        /// <inheritdoc />
        public ComputeOperationFailedException(string message)
            : base(message)
        { }

        /// <inheritdoc />
        private ComputeOperationFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        { }
    }
}
