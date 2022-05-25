using System;
using System.Runtime.Serialization;
using net.openstack.Core.Domain;

namespace net.openstack.Core.Exceptions
{
    /// <summary>
    /// The exception that is thrown when the server enters an error state during a
    /// call to <see cref="O:net.openstack.Core.Providers.IComputeProvider.WaitForServerState"/>.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    [Serializable]
    public class ServerEnteredErrorStateException : Exception
    {
        [NonSerialized]
        private ExceptionData _state;

        /// <summary>
        /// Gets the error state the server entered.
        /// </summary>
        /// <seealso cref="ServerState"/>
        public ServerState Status
        {
            get
            {
                return ServerState.FromName(_state.Status);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerEnteredErrorStateException"/> class
        /// with the specified error state.
        /// </summary>
        /// <param name="status">The error state entered by the server.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="status"/> is <see langword="null"/>.</exception>
        public ServerEnteredErrorStateException(ServerState status)
            : base(string.Format("The server entered an error state: '{0}'", status))
        {
            if (status == null)
                throw new ArgumentNullException("status");

            _state.Status = status.Name;
#if !NET35
            SerializeObjectState += (ex, args) => args.AddSerializedState(_state);
#endif
        }

        [Serializable]
        private struct ExceptionData : ISafeSerializationData
        {
            public string Status
            {
                get;
                set;
            }

            void ISafeSerializationData.CompleteDeserialization(object deserialized)
            {
                ((ServerEnteredErrorStateException)deserialized)._state = this;
            }
        }
    }
}
