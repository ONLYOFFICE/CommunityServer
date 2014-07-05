using System;
using System.Net;

namespace AppLimit.CloudComputing.SharpBox.Common
{
    /// <summary>
    /// This class implements the used async result class 
    /// which will be generated in all async operations
    /// </summary>
    public class AsyncResultEx : IAsyncResult
    {
        /// <summary>
        /// ctor of AsyncResultEx which handles
        /// the used state object
        /// </summary>
        /// <param name="AsyncState"></param>
        public AsyncResultEx(Object AsyncState)
        {
            this.AsyncState = AsyncState;
        }

        /// <summary>
        /// This properties allows read access to the state object
        /// </summary>
        public object AsyncState
        {
            get;
            private set;
        }

        /// <summary>
        /// The wait handle, which is not needed in SharpBox
        /// </summary>
        public System.Threading.WaitHandle AsyncWaitHandle
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// The sync boolean, which is not needed in SharpBox
        /// </summary>        
        public bool CompletedSynchronously
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// IsCompleted, which is not needed in SharpBox
        /// </summary>                
        public bool IsCompleted
        {
            get { throw new NotImplementedException(); }
        }
    }
}
