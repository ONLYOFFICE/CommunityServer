using System;
using System.Runtime.Serialization;

namespace OpenStack
{
    /// <summary>
    /// The exception that is thrown when a service requires that a region is explicitly set and one was not provided.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    [Serializable]
    public class RegionRequiredException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RegionRequiredException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="args">The message formatting arguments.</param>
        public RegionRequiredException(string message, params object[] args) : base(string.Format(message, args))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RegionRequiredException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        protected RegionRequiredException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
