namespace net.openstack.Core
{
    using System;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides extension methods for asynchronous operations on
    /// <see cref="WebRequest"/> objects.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    public static class WebRequestExtensions
    {
        /// <summary>
        /// Returns a response to an Internet request as an asynchronous operation.
        /// </summary>
        /// <remarks>
        /// This operation will not block. The returned <see cref="Task{TResult}"/> object will
        /// complete after a response to an Internet request is available.
        /// </remarks>
        /// <param name="request">The request.</param>
        /// <returns>A <see cref="Task"/> object which represents the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="request"/> is <see langword="null"/>.</exception>
        public static Task<WebResponse> GetResponseAsync(this WebRequest request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            return GetResponseAsync(request, CancellationToken.None);
        }

        /// <summary>
        /// Returns a response to an Internet request as an asynchronous operation.
        /// </summary>
        /// <remarks>
        /// This operation will not block. The returned <see cref="Task{TResult}"/> object will
        /// complete after a response to an Internet request is available.
        /// </remarks>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that will be assigned to the new <see cref="Task"/>.</param>
        /// <returns>A <see cref="Task"/> object which represents the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="request"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">
        /// If <see cref="WebRequest.Abort"/> was previously called.
        /// <para>-or-</para>
        /// <para>If the timeout period for the request expired.</para>
        /// <para>-or-</para>
        /// <para>If an error occurred while processing the request.</para>
        /// </exception>
        public static Task<WebResponse> GetResponseAsync(this WebRequest request, CancellationToken cancellationToken)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            bool timeout = false;
            TaskCompletionSource<WebResponse> completionSource = new TaskCompletionSource<WebResponse>();

            RegisteredWaitHandle timerRegisteredWaitHandle = null;
            RegisteredWaitHandle cancellationRegisteredWaitHandle = null;
            AsyncCallback completedCallback =
                result =>
                {
                    try
                    {
                        if (cancellationRegisteredWaitHandle != null)
                            cancellationRegisteredWaitHandle.Unregister(null);

                        if (timerRegisteredWaitHandle != null)
                            timerRegisteredWaitHandle.Unregister(null);

                        completionSource.TrySetResult(request.EndGetResponse(result));
                    }
                    catch (WebException ex)
                    {
                        if (timeout)
                            completionSource.TrySetException(new WebException("No response was received during the time-out period for a request.", WebExceptionStatus.Timeout));
                        else if (cancellationToken.IsCancellationRequested)
                            completionSource.TrySetCanceled();
                        else
                            completionSource.TrySetException(ex);
                    }
                    catch (Exception ex)
                    {
                        completionSource.TrySetException(ex);
                    }
                };

            IAsyncResult asyncResult = request.BeginGetResponse(completedCallback, null);
            if (!asyncResult.IsCompleted)
            {
                if (request.Timeout != Timeout.Infinite)
                {
                    WaitOrTimerCallback timedOutCallback =
                        (object state, bool timedOut) =>
                        {
                            if (timedOut)
                            {
                                timeout = true;
                                request.Abort();
                            }
                        };

                    timerRegisteredWaitHandle = ThreadPool.RegisterWaitForSingleObject(asyncResult.AsyncWaitHandle, timedOutCallback, null, request.Timeout, true);
                }

                if (cancellationToken.CanBeCanceled)
                {
                    WaitOrTimerCallback cancelledCallback =
                        (object state, bool timedOut) =>
                        {
                            if (cancellationToken.IsCancellationRequested)
                                request.Abort();
                        };

                    cancellationRegisteredWaitHandle = ThreadPool.RegisterWaitForSingleObject(cancellationToken.WaitHandle, cancelledCallback, null, Timeout.Infinite, true);
                }
            }

            return completionSource.Task;
        }
    }
}
