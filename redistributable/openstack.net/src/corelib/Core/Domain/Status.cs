namespace net.openstack.Core.Domain
{
    using System;

    /// <summary>
    /// Represents the status of an operation with a status code and description
    /// of the status.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    [Serializable]
    public class Status
    {
        /// <summary>
        /// Gets the status code.
        /// </summary>
        public int Code { get; private set; }

        /// <summary>
        /// Gets the description of the status.
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Status"/> class with the specified
        /// status code and description.
        /// </summary>
        /// <param name="code">The status code.</param>
        /// <param name="description">The description of the status.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="description"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="description"/> is empty.</exception>
        public Status(int code, string description)
        {
            if (description == null)
                throw new ArgumentNullException("description");
            if (string.IsNullOrEmpty(description))
                throw new ArgumentException("description cannot be empty");

            Code = code;
            Description = description;
        }
    }
}
