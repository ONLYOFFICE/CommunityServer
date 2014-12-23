/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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