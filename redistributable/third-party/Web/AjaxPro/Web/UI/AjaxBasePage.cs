/*
 * AjaxBasePage.cs
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
#if(NET20)
/*
 * MS	06-05-22	inital version
 * MS	06-09-14	mark for NET20 only
 * 
 * 
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Web.UI.Design;
using System.Reflection;

namespace AjaxPro.Web.UI
{
	public class AjaxBasePage : System.Web.UI.Page, AjaxPro.IContextInitializer
	{
		protected HttpContext m_HttpContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="AjaxBasePage"/> class.
        /// </summary>
		public AjaxBasePage()
			: base()
		{
			this.Load += new EventHandler(AjaxBasePage_Load);
		}

        /// <summary>
        /// Handles the Load event of the AjaxBasePage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		void AjaxBasePage_Load(object sender, EventArgs e)
		{
		}

        /// <summary>
        /// Gets the <see cref="T:System.Web.Caching.Cache"></see> object associated with the application in which the page resides.
        /// </summary>
        /// <value></value>
        /// <returns>The <see cref="T:System.Web.Caching.Cache"></see> associated with the page's application.</returns>
        /// <exception cref="T:System.Web.HttpException">An instance of <see cref="T:System.Web.Caching.Cache"></see> is not created. </exception>
		protected new System.Web.Caching.Cache Cache
		{
			get
			{
				return m_HttpContext.Cache;
			}
		}

        /// <summary>
        /// Gets the <see cref="T:System.Web.HttpContext"></see> object associated with the page.
        /// </summary>
        /// <value></value>
        /// <returns>An <see cref="T:System.Web.HttpContext"></see> object that contains information associated with the current page.</returns>
		protected override HttpContext Context
		{
			get
			{
				return m_HttpContext;
			}
		}

        /// <summary>
        /// Gets the current Session object provided by ASP.NET.
        /// </summary>
        /// <value></value>
        /// <returns>The current session-state data.</returns>
        /// <exception cref="T:System.Web.HttpException">Occurs when the session information is set to null. </exception>
		protected new System.Web.SessionState.HttpSessionState Session
		{
			get
			{
				return m_HttpContext.Session;
			}
		}

		#region IContextInitializer Member

        /// <summary>
        /// Initialize the HttpContext to the class implementing IContextInitializer.
        /// </summary>
        /// <param name="context">The HttpContext.</param>
		public void InitializeContext(HttpContext context)
		{
			m_HttpContext = context;
		}

		#endregion
	}
}
#endif