/*
 * AjaxAsyncHttpHandler.cs
 * 
 * Copyright © 2007 Michael Schwarz (http://www.ajaxpro.info).
 * All Rights Reserved.
 * 
 * Permission is hereby granted, free of charge, to any person 
 * obtaining a copy of this software and associated documentation 
 * files (the "Software"), to deal in the Software without 
 * restriction, including without limitation the rights to use, 
 * copy, modify, merge, publish, distribute, sublicense, and/or 
 * sell copies of the Software, and to permit persons to whom the 
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be 
 * included in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
 * IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR 
 * ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
 * CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN 
 * CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
/*
 * MS	06-06-07	changed to internal
 * MS	06-06-11	changed to use IAsyncResult and BeginInvoke instead of new Thread
 * 
 * 
 * 
 * 
 */
using System;
using System.Reflection;
using System.Web;
using System.Threading;
using System.Web.SessionState;

namespace AjaxPro
{
	internal delegate void AsyncAjaxProcDelegate();

	internal class AjaxAsyncHttpHandler : IHttpAsyncHandler
	{
		private IAjaxProcessor p;
		private HttpContext ctx = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="AjaxAsyncHttpHandler"/> class.
        /// </summary>
        /// <param name="p">The p.</param>
		internal AjaxAsyncHttpHandler(IAjaxProcessor p)
			: base()
		{
			this.p = p;
		}

		#region IHttpAsyncHandler Members

        /// <summary>
        /// Initiates an asynchronous call to the HTTP handler.
        /// </summary>
        /// <param name="context">An <see cref="T:System.Web.HttpContext"></see> object that provides references to intrinsic server objects (for example, Request, Response, Session, and Server) used to service HTTP requests.</param>
        /// <param name="cb">The <see cref="T:System.AsyncCallback"></see> to call when the asynchronous method call is complete. If cb is null, the delegate is not called.</param>
        /// <param name="extraData">Any extra data needed to process the request.</param>
        /// <returns>
        /// An <see cref="T:System.IAsyncResult"></see> that contains information about the status of the process.
        /// </returns>
		public IAsyncResult BeginProcessRequest(HttpContext context, System.AsyncCallback cb, object extraData)
		{
			ctx = context;
			IntPtr token = System.Security.Principal.WindowsIdentity.GetCurrent().Token;

			AjaxProcHelper m = new AjaxProcHelper(p, token);
			AsyncAjaxProcDelegate apd = new AsyncAjaxProcDelegate(m.Run);

			IAsyncResult ar = apd.BeginInvoke(cb, apd);

			return ar;
		}

        /// <summary>
        /// Ends the process request.
        /// </summary>
        /// <param name="ar">The ar.</param>
		public void EndProcessRequest(IAsyncResult ar)
		{
			AsyncAjaxProcDelegate apd = (AsyncAjaxProcDelegate)ar.AsyncState;

			apd.EndInvoke(ar);

			//AjaxProcHelper m = (AjaxProcHelper)apd.Target;
		}

		#endregion

		#region IHttpHandler Members

        /// <summary>
        /// Enables processing of HTTP Web requests by a custom HttpHandler that implements the <see cref="T:System.Web.IHttpHandler"></see> interface.
        /// </summary>
        /// <param name="context">An <see cref="T:System.Web.HttpContext"></see> object that provides references to the intrinsic server objects (for example, Request, Response, Session, and Server) used to service HTTP requests.</param>
		public void ProcessRequest(HttpContext context)
		{
			// TODO:  Add AjaxAsyncHttpHandler.ProcessRequest implementation
		}

        /// <summary>
        /// Gets a value indicating whether another request can use the <see cref="T:System.Web.IHttpHandler"></see> instance.
        /// </summary>
        /// <value></value>
        /// <returns>true if the <see cref="T:System.Web.IHttpHandler"></see> instance is reusable; otherwise, false.</returns>
		public bool IsReusable
		{
			get
			{
				// TODO:  Add AjaxAsyncHttpHandler.IsReusable getter implementation
				return false;
			}
		}

		#endregion
	}


	internal class AjaxAsyncHttpHandlerSession : AjaxAsyncHttpHandler, IRequiresSessionState
	{
		internal AjaxAsyncHttpHandlerSession(IAjaxProcessor p) : base(p) { }
	}

	internal class AjaxAsyncHttpHandlerSessionReadOnly : AjaxAsyncHttpHandler, IReadOnlySessionState
	{
		internal AjaxAsyncHttpHandlerSessionReadOnly(IAjaxProcessor p) : base(p) { }
	}
}
