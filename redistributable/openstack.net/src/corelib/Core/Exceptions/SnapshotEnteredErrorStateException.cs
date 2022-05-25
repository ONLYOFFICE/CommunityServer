namespace net.openstack.Core.Exceptions
{
    using System;
    using System.Runtime.Serialization;
    using net.openstack.Core.Domain;

    /// <summary>
    /// Represents errors that occur when a snapshot enters an error state while waiting
    /// on it to enter a particular state.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    [Serializable]
    public class SnapshotEnteredErrorStateException : Exception
    {
        [NonSerialized]
        private ExceptionData _state;

        /// <summary>
        /// Gets the error state the snapshot entered.
        /// </summary>
        /// <seealso cref="SnapshotState"/>
        public SnapshotState Status
        {
            get
            {
                return SnapshotState.FromName(_state.Status);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SnapshotEnteredErrorStateException"/> class with the
        /// specified snapshot state.
        /// </summary>
        /// <param name="status">The erroneous snapshot state.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="status"/> is <see langword="null"/>.</exception>
        public SnapshotEnteredErrorStateException(SnapshotState status)
            : base(string.Format("The snapshot entered an error state: '{0}'", status))
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
                ((SnapshotEnteredErrorStateException)deserialized)._state = this;
            }
        }
    }
}
