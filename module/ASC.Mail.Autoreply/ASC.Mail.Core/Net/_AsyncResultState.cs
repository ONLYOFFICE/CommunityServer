/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


namespace ASC.Mail.Net
{
    #region usings

    using System;
    using System.Threading;

    #endregion

    /// <summary>
    /// (For internal use only). This class provides holder for IAsyncResult interface and extends it's features.
    /// </summary>
    internal class AsyncResultState : IAsyncResult
    {
        #region Members

        private readonly Delegate m_pAsyncDelegate;
        private readonly object m_pAsyncObject;
        private readonly AsyncCallback m_pCallback;
        private readonly object m_pState;
        private IAsyncResult m_pAsyncResult;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="asyncObject">Caller's async object.</param>
        /// <param name="asyncDelegate">Delegate which is called asynchronously.</param>
        /// <param name="callback">Callback to call when the connect operation is complete.</param>
        /// <param name="state">User data.</param>
        public AsyncResultState(object asyncObject,
                                Delegate asyncDelegate,
                                AsyncCallback callback,
                                object state)
        {
            m_pAsyncObject = asyncObject;
            m_pAsyncDelegate = asyncDelegate;
            m_pCallback = callback;
            m_pState = state;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets delegate which is called asynchronously.
        /// </summary>
        public Delegate AsyncDelegate
        {
            get { return m_pAsyncDelegate; }
        }

        /// <summary>
        /// Gets or sets caller's async object.
        /// </summary>
        public object AsyncObject
        {
            get { return m_pAsyncObject; }
        }

        /// <summary>
        /// Gets source asynchronous result what we wrap.
        /// </summary>
        public IAsyncResult AsyncResult
        {
            get { return m_pAsyncResult; }
        }

        /// <summary>
        /// Gets if the user called the End*() method.
        /// </summary>
        public bool IsEndCalled { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Sets AsyncResult value.
        /// </summary>
        /// <param name="asyncResult">Asycnhronous result to wrap.</param>
        public void SetAsyncResult(IAsyncResult asyncResult)
        {
            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }

            m_pAsyncResult = asyncResult;
        }

        /// <summary>
        /// This method is called by AsyncDelegate when asynchronous operation completes. 
        /// </summary>
        /// <param name="ar">An IAsyncResult that stores state information and any user defined data for this asynchronous operation.</param>
        public void CompletedCallback(IAsyncResult ar)
        {
            if (m_pCallback != null)
            {
                m_pCallback(this);
            }
        }

        #endregion

        #region IAsyncResult Members

        /// <summary>
        /// Gets a user-defined object that qualifies or contains information about an asynchronous operation.
        /// </summary>
        public object AsyncState
        {
            get { return m_pState; }
        }

        /// <summary>
        /// Gets a WaitHandle that is used to wait for an asynchronous operation to complete.
        /// </summary>
        public WaitHandle AsyncWaitHandle
        {
            get { return m_pAsyncResult.AsyncWaitHandle; }
        }

        /// <summary>
        /// Gets an indication of whether the asynchronous operation completed synchronously.
        /// </summary>
        public bool CompletedSynchronously
        {
            get { return m_pAsyncResult.CompletedSynchronously; }
        }

        /// <summary>
        /// Gets an indication whether the asynchronous operation has completed.
        /// </summary>
        public bool IsCompleted
        {
            get { return m_pAsyncResult.IsCompleted; }
        }

        #endregion
    }
}