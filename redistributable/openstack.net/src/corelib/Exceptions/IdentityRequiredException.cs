using System;
using System.Runtime.Serialization;
using net.openstack.Core.Providers;
using OpenStack.Authentication;

namespace OpenStack
{
    /// <summary>
    /// The exception that is thrown when an <see cref="IIdentityProvider"/> instance is used as an <see cref="IAuthenticationProvider"/> and no default identity was specified.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    [Serializable]
    public class IdentityRequiredException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityRequiredException"/> class.
        /// </summary>
        public IdentityRequiredException()
            : base("You must set the default identity when constructing the IdentityProvider in order to use it as an IAuthenticationProvider.")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityRequiredException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        protected IdentityRequiredException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}