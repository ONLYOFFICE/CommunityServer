/*
 * AjaxHandlerFactory.cs
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
 * MS	06-04-11	added use of IHttpAsyncHandler when configured with AjaxMethod attribute
 * MS	06-05-09	fixed response if type could not be loaded
 * MS	06-05-22	added possibility to have one file for prototype,core instead of two
 *					use default HttpSessionRequirement.ReadWrite if not configured, ajaxNet/ajaxSettings/oldStyle/sessionStateDefaultNone
 *					improved performance by saving GetCustomAttributes type array
 * MS	06-05-30	added ms.ashx
 * MS	06-06-07	added check for new urlNamespaceMappings/allowListOnly attribute
 * MS	06-06-11	removed WebEvent because of SecurityPermissions not available in medium trust environments
 * 
 */
using System;
using System.IO;
using System.Web;
using System.Web.Caching;
#if(NET20)
using System.Web.Management;
#endif

namespace AjaxPro
{
	public class AjaxHandlerFactory : IHttpHandlerFactory
	{
		#region IHttpHandlerFactory Members

        /// <summary>
        /// Enables a factory to reuse an existing handler instance.
        /// </summary>
        /// <param name="handler">The <see cref="T:System.Web.IHttpHandler"></see> object to reuse.</param>
		public void ReleaseHandler(IHttpHandler handler)
		{
			// TODO:  Add AjaxHandlerFactory.ReleaseHandler implementation
		}

        /// <summary>
        /// Returns an instance of a class that implements the <see cref="T:System.Web.IHttpHandler"></see> interface.
        /// </summary>
        /// <param name="context">An instance of the <see cref="T:System.Web.HttpContext"></see> class that provides references to intrinsic server objects (for example, Request, Response, Session, and Server) used to service HTTP requests.</param>
        /// <param name="requestType">The HTTP data transfer method (GET or POST) that the client uses.</param>
        /// <param name="url">The <see cref="P:System.Web.HttpRequest.RawUrl"></see> of the requested resource.</param>
        /// <param name="pathTranslated">The <see cref="P:System.Web.HttpRequest.PhysicalApplicationPath"></see> to the requested resource.</param>
        /// <returns>
        /// A new <see cref="T:System.Web.IHttpHandler"></see> object that processes the request.
        /// </returns>
		public IHttpHandler GetHandler(HttpContext context, string requestType, string url, string pathTranslated)
		{
			// First of all we want to check what a request is running. There are three different
			// requests that are made to this handler:
			//		1) GET core,prototype,converter.ashx which will include the common AJAX communication
			//		2) GET typename,assemblyname.ashx which will return the AJAX wrapper JavaScript code
			//		3) POST typename,assemblyname.ashx which will invoke a method.
			// The first two requests will return the JavaScript code or a HTTP 304 (not changed).

			string filename = Path.GetFileNameWithoutExtension(context.Request.Path);
			Type t = null;

			Exception typeException = null;
			bool isInTypesList = false;

			try
			{
				if (Utility.Settings != null && Utility.Settings.UrlNamespaceMappings.Contains(filename))
				{
					isInTypesList = true;
					t = Type.GetType(Utility.Settings.UrlNamespaceMappings[filename].ToString(), true);
				}

				if (t == null)
					t = Type.GetType(filename, true);
			}
			catch (Exception ex)
			{
				typeException = ex;
			}

			switch(requestType)
			{
				case "GET":		// get the JavaScript files

					switch(filename.ToLower())
					{
						case "prototype":
							return new EmbeddedJavaScriptHandler("prototype");

						case "core":
							return new EmbeddedJavaScriptHandler("core");

						case "ms":
							return new EmbeddedJavaScriptHandler("ms");

						case "prototype-core":
						case "core-prototype":
							return new EmbeddedJavaScriptHandler("prototype,core");

                        case "core-prototype-converter":
                            return new EmbeddedJavaScriptHandler("prototype,core,converter");
						case "converter":
							return new ConverterJavaScriptHandler();

						default:

							if (typeException != null)
							{
#if(WEBEVENT)
								string errorText = string.Format(Constant.AjaxID + " Error", context.User.Identity.Name);

								Management.WebAjaxErrorEvent ev = new Management.WebAjaxErrorEvent(errorText, WebEventCodes.WebExtendedBase + 201, typeException);
								ev.Raise();
#endif
								return null;
							}

							if (Utility.Settings.OnlyAllowTypesInList == true && isInTypesList == false)
								return null;

							return new TypeJavaScriptHandler(t);
					}

				case "POST":	// invoke the method

					if (Utility.Settings.OnlyAllowTypesInList == true && isInTypesList == false)
						return null;

					IAjaxProcessor[] p = new IAjaxProcessor[2];
					
					p[0] = new XmlHttpRequestProcessor(context, t);
					p[1] = new IFrameProcessor(context, t);

					for(int i=0; i<p.Length; i++)
					{
						if(p[i].CanHandleRequest)
						{
							if (typeException != null)
							{
#if(WEBEVENT)
								string errorText = string.Format(Constant.AjaxID + " Error", context.User.Identity.Name);

								Management.WebAjaxErrorEvent ev = new Management.WebAjaxErrorEvent(errorText, WebEventCodes.WebExtendedBase + 200, typeException);
								ev.Raise();
#endif
								p[i].SerializeObject(new NotSupportedException("This method is either not marked with an AjaxMethod or is not available."));
								return null;
							}

							AjaxMethodAttribute[] ma = (AjaxMethodAttribute[])p[i].AjaxMethod.GetCustomAttributes(typeof(AjaxMethodAttribute), true);

							bool useAsync = false;
							HttpSessionStateRequirement sessionReq = HttpSessionStateRequirement.ReadWrite;

							if (Utility.Settings.OldStyle.Contains("sessionStateDefaultNone"))
								sessionReq = HttpSessionStateRequirement.None;

							if(ma.Length > 0)
							{
								useAsync = ma[0].UseAsyncProcessing;

								if(ma[0].RequireSessionState != HttpSessionStateRequirement.UseDefault)
									sessionReq = ma[0].RequireSessionState;
							}

							switch (sessionReq)
							{
								case HttpSessionStateRequirement.Read:
									if (!useAsync)
										return new AjaxSyncHttpHandlerSessionReadOnly(p[i]);
									else
										return new AjaxAsyncHttpHandlerSessionReadOnly(p[i]);

								case HttpSessionStateRequirement.ReadWrite:
									if (!useAsync)
										return new AjaxSyncHttpHandlerSession(p[i]);
									else
										return new AjaxAsyncHttpHandlerSession(p[i]);

								case HttpSessionStateRequirement.None:
									if (!useAsync)
										return new AjaxSyncHttpHandler(p[i]);
									else
										return new AjaxAsyncHttpHandler(p[i]);

								default:
									if (!useAsync)
										return new AjaxSyncHttpHandlerSession(p[i]);
									else
										return new AjaxAsyncHttpHandlerSession(p[i]);
							}
						}
					}
					break;
			}

			return null;
		}

		#endregion
	}
}
