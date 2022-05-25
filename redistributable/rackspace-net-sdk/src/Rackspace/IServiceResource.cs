namespace Rackspace
{
    /// <summary>
    /// Resources which want to expose service operations directly (e.g. resource.Delete())
    /// should implement this interface and the service will use it to add a reference to itself.
    /// </summary>
    /// <typeparam name="TService">The service type.</typeparam>
    internal interface IServiceResource<TService>
    {
        /// <summary>
        /// The service which originally constructed the resource. This instance will be used for further operations on the resource.
        /// </summary>
        TService Owner { get; set; }
    }
}