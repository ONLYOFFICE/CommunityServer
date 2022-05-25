namespace net.openstack.Core.Validators
{
    using System;
    using net.openstack.Core.Providers;
    using net.openstack.Providers.Rackspace.Exceptions;

    /// <summary>
    /// Represents an object that validates arguments for an implementation of <see cref="IBlockStorageProvider"/>
    /// prior to sending the calls to the underlying REST API.
    /// </summary>
    public interface IBlockStorageValidator
    {
        /// <summary>
        /// Validates the <c>size</c> parameter when creating a new block storage volume
        /// with <see cref="IBlockStorageProvider.CreateVolume"/>.
        /// </summary>
        /// <param name="size">The volume size in GB.</param>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="size"/> is less than 0.</exception>
        /// <exception cref="InvalidVolumeSizeException">If <paramref name="size"/> is not a valid volume size.</exception>
        void ValidateVolumeSize(int size);
    }
}
