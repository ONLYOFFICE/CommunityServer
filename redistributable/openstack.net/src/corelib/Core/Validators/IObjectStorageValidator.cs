namespace net.openstack.Core.Validators
{
    using System;
    using net.openstack.Core.Exceptions;
    using net.openstack.Core.Providers;

    /// <summary>
    /// Represents an object that validates arguments for an implementation of <see cref="IObjectStorageProvider"/>
    /// prior to sending the calls to the underlying REST API.
    /// </summary>
    public interface IObjectStorageValidator
    {
        /// <summary>
        /// Validates a container name for an object storage provider.
        /// </summary>
        /// <param name="containerName">The container name.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="containerName"/> is <see langword="null"/>.</exception>
        /// <exception cref="ContainerNameException">If <paramref name="containerName"/> is not a valid container name.</exception>
        void ValidateContainerName(string containerName);

        /// <summary>
        /// Validates an object name for an object storage provider.
        /// </summary>
        /// <param name="objectName">The object name.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="objectName"/> is <see langword="null"/>.</exception>
        /// <exception cref="ObjectNameException">If <paramref name="objectName"/> is not a valid object name.</exception>
        void ValidateObjectName(string objectName);
    }
}
