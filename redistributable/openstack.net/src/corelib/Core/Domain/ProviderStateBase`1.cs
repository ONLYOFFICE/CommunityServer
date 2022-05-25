namespace net.openstack.Core.Domain
{
    /// <summary>
    /// Provides a base class for domain objects which require access to the
    /// provider which created them.
    /// </summary>
    /// <typeparam name="T">The provider type.</typeparam>
    /// <threadsafety static="true" instance="false"/>
    public abstract class ProviderStateBase<T> : ExtensibleJsonObject
    {
        /// <summary>
        /// Gets the provider which created the instance.
        /// </summary>
        protected internal T Provider { get; set; }

        /// <summary>
        /// Gets the region where the current entity resides.
        /// </summary>
        protected internal string Region { get; set; }

        /// <summary>
        /// Gets the identity the current entity belongs to, or <see langword="null"/> if
        /// the identity was not explicitly specified in the request that created
        /// this object (i.e. the default identity of <see cref="Provider"/> was
        /// used).
        /// </summary>
        protected internal CloudIdentity Identity { get; set; }
    }
}
