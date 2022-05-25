using System;
using net.openstack.Core.Validators;
using net.openstack.Providers.Rackspace.Exceptions;

namespace net.openstack.Providers.Rackspace.Validators
{
    /// <summary>
    /// Provides an implementation of <see cref="IBlockStorageValidator"/> for
    /// operation with Rackspace's Cloud Block Storage product.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    public class CloudBlockStorageValidator : IBlockStorageValidator
    {
        /// <summary>
        /// A default instance of <see cref="CloudBlockStorageValidator"/>.
        /// </summary>
        private static readonly CloudBlockStorageValidator _default = new CloudBlockStorageValidator();

        /// <summary>
        /// Gets a default implementation of <see cref="CloudBlockStorageValidator"/>.
        /// </summary>
        public static CloudBlockStorageValidator Default
        {
            get
            {
                return _default;
            }
        }

        /// <inheritdoc/>
        public void ValidateVolumeSize(int size)
        {
            if (size < 0)
                throw new ArgumentOutOfRangeException("size");

            if (size < 1 || size > 1000)
                throw new InvalidVolumeSizeException(size);
        }
    }
}
