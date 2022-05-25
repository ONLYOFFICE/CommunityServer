using System;
using System.Collections.Generic;

namespace net.openstack.Providers.Rackspace.Objects
{
    /// <summary>
    /// Represents the detailed results of a bulk deletion operation.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    [Serializable]
    public class BulkDeletionResults
    {
        /// <summary>
        /// Gets a collection objects which were successfully deleted.
        /// </summary>
        public IEnumerable<string> SuccessfulObjects { get; private set; }

        /// <summary>
        /// Gets a collection of <see cref="BulkDeletionFailedObject"/> objects providing
        /// the name and status of objects which could not be deleted during the bulk
        /// deletion operation.
        /// </summary>
        public IEnumerable<BulkDeletionFailedObject> FailedObjects { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BulkDeletionResults"/> class
        /// with the specified collections of successful and failed objects.
        /// </summary>
        /// <param name="successfulObjects">The objects which were successfully deleted.</param>
        /// <param name="failedObjects">The objects which could not be deleted.</param>
        public BulkDeletionResults(IEnumerable<string> successfulObjects, IEnumerable<BulkDeletionFailedObject> failedObjects)
        {
            if (successfulObjects == null)
                throw new ArgumentNullException("successfulObjects");
            if (failedObjects == null)
                throw new ArgumentNullException("failedObjects");

            SuccessfulObjects = successfulObjects;
            FailedObjects = failedObjects;
        }
    }
}
