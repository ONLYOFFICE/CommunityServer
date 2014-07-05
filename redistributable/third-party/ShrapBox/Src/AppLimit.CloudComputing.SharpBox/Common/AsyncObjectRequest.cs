using System;
using System.Net;

namespace AppLimit.CloudComputing.SharpBox.Common
{
    /// <summary>
    /// This class implements an object request for all 
    /// async operations in SharpBox
    /// </summary>
    public class AsyncObjectRequest
    {
        /// <summary>
        /// Reference to the callback which will be called
        /// at the end of an async operation
        /// </summary>
        public AsyncCallback callback;

        /// <summary>
        /// The generated asyncresult
        /// </summary>
        public IAsyncResult result;

        /// <summary>
        /// In case of error the cached execption which can 
        /// be handled in the EndRequest method
        /// </summary>
        public Exception errorReason;   
    }
}
