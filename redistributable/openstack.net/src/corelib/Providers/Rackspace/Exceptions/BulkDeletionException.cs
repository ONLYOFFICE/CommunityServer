using System;
using System.Runtime.Serialization;
using net.openstack.Providers.Rackspace.Objects;

namespace net.openstack.Providers.Rackspace.Exceptions
{
    /// <summary>
    /// Represents errors which occur during a bulk delete operation.
    /// </summary>
    /// <seealso cref="CloudFilesProvider.BulkDelete"/>
    /// <threadsafety static="true" instance="false"/>
    [Serializable]
    public class BulkDeletionException : Exception
    {
        [NonSerialized]
        private ExceptionData _state;

        /// <summary>
        /// Gets the detailed results of the bulk delete operation.
        /// </summary>
        public BulkDeletionResults Results
        {
            get
            {
                return _state.Results;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BulkDeletionException"/> class
        /// with the specified status and results.
        /// </summary>
        /// <param name="status">A description of the status of the operation.</param>
        /// <param name="results">The results of the bulk delete operation.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="status"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="results"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="status"/> is empty.
        /// </exception>
        public BulkDeletionException(string status, BulkDeletionResults results)
            : base(string.Format("The bulk deletion operation did not complete successfully. Status: {0}", status))
        {
            if (status == null)
                throw new ArgumentNullException("status");
            if (results == null)
                throw new ArgumentNullException("results");
            if (string.IsNullOrEmpty(status))
                throw new ArgumentException("status cannot be empty");

            _state.Results = results;
#if !NET35
            SerializeObjectState += (ex, args) => args.AddSerializedState(_state);
#endif
        }

        [Serializable]
        private struct ExceptionData : ISafeSerializationData
        {
            public BulkDeletionResults Results
            {
                get;
                set;
            }

            void ISafeSerializationData.CompleteDeserialization(object deserialized)
            {
                ((BulkDeletionException)deserialized)._state = this;
            }
        }
    }
}
