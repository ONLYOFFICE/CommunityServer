/*
 * EmbeddedJavaScriptHandler.cs
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
 * MS	06-04-05	added oldstyled Object.prototype.extend code, enabled by web.config
 *					setting oldStyle\objectExtendPrototype
 * MS	06-05-22	added possibility to have one file for prototype,core instead of two
 * MS	06-06-06	fixed If-Modified-Since http header if using zip
 * MS	06-06-07	changed to internal
 * 
 * 
 */
using System;
using System.Reflection;
using System.Web;
using System.Web.Caching;
using System.IO;

namespace AjaxPro
{
	/// <summary>
	/// Represents an IHttpHandler for the client-side JavaScript prototype and core methods.
	/// </summary>
	internal class EmbeddedJavaScriptHandler : IHttpHandler
	{
		private string fileName;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmbeddedJavaScriptHandler"/> class.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
		internal EmbeddedJavaScriptHandler(string fileName)
		{
			this.fileName = fileName;
		}

		#region IHttpHandler Members

        /// <summary>
        /// Enables processing of HTTP Web requests by a custom HttpHandler that implements the <see cref="T:System.Web.IHttpHandler"></see> interface.
        /// </summary>
        /// <param name="context">An <see cref="T:System.Web.HttpContext"></see> object that provides references to the intrinsic server objects (for example, Request, Response, Session, and Server) used to service HTTP requests.</param>
		public void ProcessRequest(HttpContext context)
		{
			string etag = context.Request.Headers["If-None-Match"];
			string modSince = context.Request.Headers["If-Modified-Since"];

			if(context.Cache[Constant.AjaxID + "." + fileName] != null)
			{
				CacheInfo ci = (CacheInfo)context.Cache[Constant.AjaxID + "." + fileName];

				if(etag != null)
				{
					if(etag == ci.ETag)		// TODO: null check
					{
						context.Response.StatusCode = 304;
						return;
					}
				}
				
				if(modSince != null)
				{
					if (modSince.IndexOf(";") > 0)
					{
						// If-Modified-Since: Tue, 06 Jun 2006 10:13:38 GMT; length=2935
						modSince = modSince.Split(';')[0];
					}

					try
					{
						DateTime modSinced = Convert.ToDateTime(modSince.ToString()).ToUniversalTime();
						if(DateTime.Compare(modSinced, ci.LastModified.ToUniversalTime()) >= 0)
						{
							context.Response.StatusCode = 304;
							return;
						}
					}
					catch(FormatException)
					{
						if(context.Trace.IsEnabled) context.Trace.Write(Constant.AjaxID, "The header value for If-Modified-Since = " + modSince + " could not be converted to a System.DateTime.");
					}
				}
			}

			etag = MD5Helper.GetHash(System.Text.Encoding.Default.GetBytes(fileName));

			DateTime now = DateTime.Now;
			DateTime lastMod = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second); //.ToUniversalTime();

			context.Response.AddHeader("Content-Type", "application/x-javascript");
			context.Response.ContentEncoding = System.Text.Encoding.UTF8;
			context.Response.Cache.SetCacheability(System.Web.HttpCacheability.Public);
			context.Response.Cache.SetETag(etag);
			context.Response.Cache.SetLastModified(lastMod);

			context.Response.Write(@"//--------------------------------------------------------------
// Copyright (C) 2006 Michael Schwarz (http://www.ajaxpro.info).
// All rights reserved.
//--------------------------------------------------------------

");

			// Now, we want to read the JavaScript embedded source
			// from the assembly. If the filename includes any comma
			// we have to return more than one embedded JavaScript file.

			if (fileName != null && fileName.Length > 0)
			{
				string[] files = fileName.Split(',');
				Assembly assembly = Assembly.GetExecutingAssembly();
				Stream s;

				for (int i = 0; i < files.Length; i++)
				{
					s = assembly.GetManifestResourceStream(Constant.AssemblyName + "." + files[i] + ".js");

					if (s != null)
					{
						System.IO.StreamReader sr = new System.IO.StreamReader(s);

						context.Response.Write("// " + files[i] + ".js\r\n");

						context.Response.Write(sr.ReadToEnd());
						context.Response.Write("\r\n");

						sr.Close();

						if (files[i] == "prototype" && AjaxPro.Utility.Settings.OldStyle.Contains("objectExtendPrototype"))
						{
							context.Response.Write(@"
Object.prototype.extend = function(o, override) {
	return Object.extend.apply(this, [this, o, override != false]);
}
");
						}
					}
				}
			}

			context.Cache.Add(Constant.AjaxID + "." + fileName, new CacheInfo(etag, lastMod), null, 
				System.Web.Caching.Cache.NoAbsoluteExpiration, System.Web.Caching.Cache.NoSlidingExpiration,
				System.Web.Caching.CacheItemPriority.Normal, null);
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
				return false;
			}
		}

		#endregion
	}
}
