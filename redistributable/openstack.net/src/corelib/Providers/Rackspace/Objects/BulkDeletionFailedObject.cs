using System;

using net.openstack.Core.Domain;

namespace net.openstack.Providers.Rackspace.Objects
{
    /// <summary>
    /// Describes an object which could not be deleted by a bulk deletion operation,
    /// along with a status providing the reason why the deletion failed.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    [Serializable]
    public class BulkDeletionFailedObject
    {
        /// <summary>
        /// Gets a <see cref="Status"/> object describing the reason the object
        /// could not be deleted.
        /// </summary>
        public Status Status { get; private set; }

        /// <summary>
        /// Gets the name of the object which could not be deleted.
        /// </summary>
        public string Object { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BulkDeletionFailedObject"/> class
        /// with the specified object name and status.
        /// </summary>
        /// <param name="obj">The name of the object which could not be deleted.</param>
        /// <param name="status">A <see cref="Status"/> object describing the reason the object could not be deleted.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="obj"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="status"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">If <paramref name="obj"/> is empty.</exception>
        public BulkDeletionFailedObject(string obj, Status status)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            if (status == null)
                throw new ArgumentNullException("status");
            if (string.IsNullOrEmpty(obj))
                throw new ArgumentException("obj cannot be empty");

            Object = obj;
            Status = status;
        }
    }
}
