namespace net.openstack.Core
{
    using System.Threading.Tasks;

    /// <summary>
    /// Specifies when a <see cref="Task"/> representing an asynchronous server operation
    /// should be considered complete.
    /// </summary>
    /// <preliminary/>
    public enum AsyncCompletionOption
    {
        /// <summary>
        /// The <see cref="Task"/> representing the operation is considered complete after the
        /// request has been submitted to the server.
        /// </summary>
        RequestSubmitted,

        /// <summary>
        /// The <see cref="Task"/> representing the operation is considered complete after the
        /// server has completed processing the request.
        /// </summary>
        RequestCompleted,
    }
}
