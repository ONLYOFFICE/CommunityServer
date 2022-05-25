namespace net.openstack.Core.Caching
{
    using System;

    /// <summary>
    /// Represents a thread-safe cache of objects identified by string keys.
    /// </summary>
    /// <typeparam name="T">Type type of objects stored in the cache.</typeparam>
    public interface ICache<T>
    {
        /// <summary>
        /// Gets a value cached for a particular key, updating the value if necessary.
        /// </summary>
        /// <remarks>
        /// This method returns a previously cached value when possible. If any of the following
        /// conditions are met, the <paramref name="refreshCallback"/> function will be called to
        /// obtain a new value for <paramref name="key"/> which is then added to the cache,
        /// replacing any previously cached value.
        /// 
        /// <list type="bullet">
        /// <item>The cache does not contain any value associated with <paramref name="key"/>.</item>
        /// <item><paramref name="forceCacheRefresh"/> is <see langword="true"/>.</item>
        /// <item>The previously cached value for <paramref name="key"/> is no longer valid. The exact
        /// algorithm for determining whether or not a value is valid in implementation-defined.</item>
        /// </list>
        ///
        /// <para>If any of the above conditions is met and <paramref name="refreshCallback"/> is <see langword="null"/>,
        /// the previously cached value for <paramref name="key"/>, if any, is removed from the cache
        /// and the default value for <typeparamref name="T"/> is returned.</para>
        /// </remarks>
        /// <param name="key">The key.</param>
        /// <param name="refreshCallback">A function which returns a new value for the specified <paramref name="key"/>, or <see langword="null"/> if no update function available (see remarks).</param>
        /// <param name="forceCacheRefresh">If <see langword="true"/>, the value associated with <paramref name="key"/> will be always be refreshed by calling <paramref name="refreshCallback"/>, even if a value is already cached.</param>
        /// <returns>
        /// The cached value associated with the specified <paramref name="key"/>. If no cached value is
        /// available (see remarks), the method returns the default value for <typeparamref name="T"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="key"/> is <see langword="null"/>.</exception>
        T Get(string key, Func<T> refreshCallback, bool forceCacheRefresh = false);
    }
}
