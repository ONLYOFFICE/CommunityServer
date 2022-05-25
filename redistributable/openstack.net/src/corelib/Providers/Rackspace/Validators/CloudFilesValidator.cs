using System;
using net.openstack.Core;
using net.openstack.Core.Exceptions;
using net.openstack.Core.Validators;

namespace net.openstack.Providers.Rackspace.Validators
{
    /// <summary>
    /// Provides an implementation of <see cref="IObjectStorageValidator"/> for
    /// operation with Rackspace's Cloud Files product.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    public class CloudFilesValidator : IObjectStorageValidator
    {
        /// <summary>
        /// A default instance of <see cref="CloudBlockStorageValidator"/>.
        /// </summary>
        private static readonly CloudFilesValidator _default = new CloudFilesValidator();

        /// <summary>
        /// Gets a default instance of <see cref="CloudBlockStorageValidator"/>.
        /// </summary>
        public static CloudFilesValidator Default
        {
            get
            {
                return _default;
            }
        }

        /// <inheritdoc/>
        public void ValidateContainerName(string containerName)
        {
            if (containerName == null)
                throw new ArgumentNullException("containerName");

            var containerNameString = string.Format("Container Name:[{0}]", containerName);
            if (string.IsNullOrEmpty(containerName))
                throw new ArgumentNullException("containerName", "ERROR: Container Name cannot be empty.");
            if (UriUtility.UriEncode(containerName, UriPart.AnyUrl).Length > 256)
                throw new ContainerNameException(string.Format("ERROR: encoded URL Length greater than 256 char's. {0}", containerNameString));
            if (containerName.Contains("/"))
                throw new ContainerNameException(string.Format("ERROR: Container Name contains a /. {0}", containerNameString));
        }

        /// <inheritdoc/>
        public void ValidateObjectName(string objectName)
        {
            if (objectName == null)
                throw new ArgumentNullException("objectName");

            if (string.IsNullOrEmpty(objectName))
                throw new ArgumentNullException();
            if (UriUtility.UriEncode(objectName, UriPart.AnyUrl).Length > 1024)
                throw new ObjectNameException("ERROR: Url Encoded Object Name exceeds 1024 char's");
        }
    }
}