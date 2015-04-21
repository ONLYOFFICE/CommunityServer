/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
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