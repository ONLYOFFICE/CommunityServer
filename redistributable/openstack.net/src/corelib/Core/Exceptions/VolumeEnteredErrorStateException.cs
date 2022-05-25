namespace net.openstack.Core.Exceptions
{
    using System;
    using System.Runtime.Serialization;
    using net.openstack.Core.Domain;

    /// <summary>
    /// Represents errors that occur when a volume enters an error state while waiting
    /// on it to enter a particular state.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    [Serializable]
    public class VolumeEnteredErrorStateException : Exception
    {
        [NonSerialized]
        private ExceptionData _state;

        /// <summary>
        /// Gets the error state the volume entered.
        /// </summary>
        /// <seealso cref="VolumeState"/>
        public VolumeState Status
        {
            get
            {
                return VolumeState.FromName(_state.Status);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VolumeEnteredErrorStateException"/> class with the
        /// specified volume state.
        /// </summary>
        /// <param name="status">The erroneous volume state.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="status"/> is <see langword="null"/>.</exception>
        public VolumeEnteredErrorStateException(VolumeState status)
            : base(string.Format("The volume entered an error state: '{0}'", status))
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
                ((VolumeEnteredErrorStateException)deserialized)._state = this;
            }
        }
    }
}
