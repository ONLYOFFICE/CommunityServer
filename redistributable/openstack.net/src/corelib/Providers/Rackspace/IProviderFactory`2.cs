namespace net.openstack.Providers.Rackspace
{
    using System;

    /// <summary>
    /// Represents a factory for obtaining service providers for particular keys.
    /// </summary>
    /// <typeparam name="T">The provider type.</typeparam>
    /// <typeparam name="T2">The key type.</typeparam>
    internal interface IProviderFactory<T, T2>
    {
        /// <summary>
        /// Get a provider for the specified <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The key. If this value is <see langword="null"/>, a default provider will be returned if available.</param>
        /// <returns>The provider.</returns>
        /// <exception cref="NotSupportedException">If <paramref name="key"/> is <see langword="null"/>, and the factory does not support a default provider.</exception>
        T Get(T2 key);
    }
}
