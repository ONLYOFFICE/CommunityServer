namespace net.openstack.Providers.Hp
{
    using System;

    /// <summary>
    /// Provides endpoints for the HP Cloud Identity Service.
    /// </summary>
    /// <remarks>
    /// As HP accounts are global, any region can be used for conducting identity
    /// operations. Choose the region closest to you for improved performance, or
    /// use <see cref="Default"/> for general API operations.
    /// </remarks>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    public static class PredefinedHpIdentityEndpoints
    {
        /// <summary>
        /// Gets the HP Cloud Identity Service endpoint for the US-West region.
        /// </summary>
        /// <remarks>
        /// As HP accounts are global, any region can be used for conducting identity
        /// operations. Choose the region closest to you for improved performance, or
        /// use <see cref="Default"/> for general API operations.
        /// </remarks>
        public static readonly Uri UsWest = new Uri("https://region-a.geo-1.identity.hpcloudsvc.com:35357/v2.0/");

        /// <summary>
        /// Gets the HP Cloud Identity Service endpoint for the US-East region.
        /// </summary>
        /// <remarks>
        /// As HP accounts are global, any region can be used for conducting identity
        /// operations. Choose the region closest to you for improved performance, or
        /// use <see cref="Default"/> for general API operations.
        /// </remarks>
        public static readonly Uri UsEast = new Uri("https://region-b.geo-1.identity.hpcloudsvc.com:35357/v2.0/");

        /// <summary>
        /// Gets the default endpoint for the HP Cloud Identity Service.
        /// </summary>
        public static readonly Uri Default = UsEast;
    }
}
