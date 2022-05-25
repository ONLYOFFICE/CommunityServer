using System;
using System.Runtime.Serialization;
using net.openstack.Core.Domain;

namespace net.openstack.Core.Exceptions
{
    /// <summary>
    /// The exception that is thrown when the server enters an error state during a
    /// call to <see cref="O:net.openstack.Core.Providers.IComputeProvider.WaitForImageState"/>.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    [Serializable]
    public class ImageEnteredErrorStateException : Exception
    {
        [NonSerialized]
        private ExceptionData _state;

        /// <summary>
        /// Gets the error state the image entered.
        /// </summary>
        /// <seealso cref="ImageState"/>
        public ImageState Status
        {
            get
            {
                return ImageState.FromName(_state.Status);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageEnteredErrorStateException"/> class
        /// with the specified error state.
        /// </summary>
        /// <param name="status">The error state entered by the image.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="status"/> is <see langword="null"/>.</exception>
        public ImageEnteredErrorStateException(ImageState status)
            : base(string.Format("The image entered an error state: '{0}'", status))
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
                ((ImageEnteredErrorStateException)deserialized)._state = this;
            }
        }
    }
}
