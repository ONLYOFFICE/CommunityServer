using System;
using System.Runtime.Serialization;
using net.openstack.Core.Validators;

namespace net.openstack.Providers.Rackspace.Exceptions
{
    /// <summary>
    /// Represents errors which occur during validation of a block storage volume size.
    /// </summary>
    /// <seealso cref="IBlockStorageValidator.ValidateVolumeSize"/>
    /// <threadsafety static="true" instance="false"/>
    [Serializable]
    public class InvalidVolumeSizeException : Exception
    {
        [NonSerialized]
        private ExceptionData _state;

        /// <summary>
        /// Gets the requested volume size.
        /// </summary>
        public int Size
        {
            get
            {
                return _state.Size;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidVolumeSizeException"/> class
        /// with the specified volume size.
        /// </summary>
        /// <param name="size">The invalid volume size which was requested.</param>
        public InvalidVolumeSizeException(int size)
            : base(string.Format("The volume size value must be between 100 and 1000. The size requested was: {0}", size))
        {
            _state.Size = size;
#if !NET35
            SerializeObjectState += (ex, args) => args.AddSerializedState(_state);
#endif
        }

        [Serializable]
        private struct ExceptionData : ISafeSerializationData
        {
            public int Size
            {
                get;
                set;
            }

            void ISafeSerializationData.CompleteDeserialization(object deserialized)
            {
                ((InvalidVolumeSizeException)deserialized)._state = this;
            }
        }
    }
}
