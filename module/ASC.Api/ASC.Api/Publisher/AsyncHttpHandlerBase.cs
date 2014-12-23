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

using System;
using System.Threading;
using System.Web;
using System.Web.SessionState;

namespace ASC.Api.Publisher
{
    public abstract class AsyncHttpHandlerBase : IHttpAsyncHandler, IReadOnlySessionState
    {
        private Action<HttpContext> _processRequest;

        #region IHttpAsyncHandler Members

        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            HttpContext.Current = context;
            OnProcessRequest(context);
        }

        public IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback cb, object extraData)
        {
            var reqState = new AsyncRequestState(context, cb, extraData);
            _processRequest = OnProcessRequest;
            new Thread(new AsyncRequest(reqState).ProcessRequest).Start(_processRequest);
            return reqState;
        }

        public void EndProcessRequest(IAsyncResult result)
        {
            var ars = result as AsyncRequestState;
            if (ars != null)
            {
                // here you could perform some cleanup, write something else to the
                // Response, or whatever else you need to do
            }
        }

        #endregion

        public abstract void OnProcessRequest(HttpContext context);
    }



    internal class AsyncRequest
    {
        private readonly AsyncRequestState _asyncRequestState;

        public AsyncRequest(AsyncRequestState ars)
        {
            _asyncRequestState = ars;
        }

        public void ProcessRequest(object parameter)
        {
            ((Action<HttpContext>) parameter)(_asyncRequestState.Ctx);
            _asyncRequestState.CompleteRequest();
        }
    }

    internal class AsyncRequestState : IAsyncResult
    {
        internal AsyncCallback Cb;
        internal HttpContext Ctx;
        internal object ExtraData;
        private EventWaitHandle _callCompleteEvent;
        private bool _isCompleted;

        public AsyncRequestState(HttpContext ctx,
                                 AsyncCallback cb,
                                 object extraData)
        {
            Ctx = ctx;
            Cb = cb;
            ExtraData = extraData;
        }

        // IAsyncResult interface property implementations

        #region IAsyncResult Members

        public object AsyncState
        {
            get { return (ExtraData); }
        }

        public bool CompletedSynchronously
        {
            get { return (false); }
        }

        public bool IsCompleted
        {
            get { return (_isCompleted); }
        }

        public WaitHandle AsyncWaitHandle
        {
            get
            {
                lock (this)
                {
                    return _callCompleteEvent ?? (_callCompleteEvent = new ManualResetEvent(false));
                }
            }
        }

        #endregion

        internal void CompleteRequest()
        {
            _isCompleted = true;
            lock (this)
            {
                if (_callCompleteEvent != null)
                    _callCompleteEvent.Set();
            }
            // if a callback was registered, invoke it now
            if (Cb != null)
                Cb(this);
        }
    }
}