/*
 * IAjaxProcessor.cs
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
 * MS	06-04-26	renamed property Method to AjaxMethod to be CLSCompliant
 * MS	06-05-09	set CanHandleRequest to virtual
 * MS	06-05-22	added common GetMethodInfo
 * MS	06-05-30	changed using new http header X-AjaxPro
 * MS	06-06-06	added ContentType property
 * MS	07-04-24	fixed Ajax token
 * 
 */
using System;
using System.Security;
using System.Web;
using System.Reflection;
using AjaxPro.Security;

namespace AjaxPro
{
	public abstract class IAjaxProcessor
	{
		protected HttpContext context;
		protected Type type;
		protected MethodInfo method;

		protected AjaxMethodAttribute[] m_methodAttributes = null;
		protected AjaxNamespaceAttribute[] m_namespaceAttributes = null;
		protected AjaxServerCacheAttribute[] m_serverCacheAttributes = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="IAjaxProcessor"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="type">The type.</param>
		public IAjaxProcessor(HttpContext context, Type type)
		{
			this.context = context;
			this.type = type;
		}

        /// <summary>
        /// Gets the method info.
        /// </summary>
        /// <param name="methodName">Name of the method.</param>
        /// <returns></returns>
		public MethodInfo GetMethodInfo(string methodName)
		{
			if (method != null)
				return method;

			if (methodName == null || type == null)
				return null;

			MethodInfo[] methods = type.GetMethods();
			bool[] isAjaxMethod = new bool[methods.Length];

			for (int i = 0; i < methods.Length; i++)
				isAjaxMethod[i] = (methods[i].GetCustomAttributes(typeof(AjaxMethodAttribute), true).Length > 0);

			AjaxNamespaceAttribute[] ns;

			for (int i = 0; i < methods.Length; i++)
			{
				if(!isAjaxMethod[i])
					continue;

				ns = (AjaxNamespaceAttribute[])methods[i].GetCustomAttributes(typeof(AjaxNamespaceAttribute), true);

				if (ns.Length > 0 && ns[0].ClientNamespace == methodName)
				{
					method = methods[i];

					m_methodAttributes = (AjaxMethodAttribute[])methods[i].GetCustomAttributes(typeof(AjaxMethodAttribute), true);
					m_namespaceAttributes = ns;
					m_serverCacheAttributes = (AjaxServerCacheAttribute[])methods[i].GetCustomAttributes(typeof(AjaxServerCacheAttribute), true);

					return method;
				}
			}

			for (int i = 0; i < methods.Length; i++)
			{
				if (!isAjaxMethod[i])
					continue;

				if (methods[i].Name == methodName)
				{
					method = methods[i];

					m_methodAttributes = (AjaxMethodAttribute[])methods[i].GetCustomAttributes(typeof(AjaxMethodAttribute), true);
					m_namespaceAttributes = (AjaxNamespaceAttribute[])methods[i].GetCustomAttributes(typeof(AjaxNamespaceAttribute), true);
					m_serverCacheAttributes = (AjaxServerCacheAttribute[])methods[i].GetCustomAttributes(typeof(AjaxServerCacheAttribute), true);

					return method;
				}
			}

			return null;
		}

		#region Internal Properties

        /// <summary>
        /// Gets the context.
        /// </summary>
        /// <value>The context.</value>
		internal HttpContext Context
		{
			get
			{
				return context;
			}
		}

        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <value>The type.</value>
		internal Type Type
		{
			get
			{
				return type;
			}
		}

		#endregion

		#region Virtual Members

        /// <summary>
        /// Gets a value indicating whether this instance can handle request.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance can handle request; otherwise, <c>false</c>.
        /// </value>
		internal virtual bool CanHandleRequest
		{
			get
			{
				return false;
			}
		}

        /// <summary>
        /// Determines whether [is valid ajax token].
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if [is valid ajax token] otherwise, <c>false</c>.
        /// </returns>
		public virtual bool IsValidAjaxToken()
		{
			if(context == null)
				return false;

			if (Utility.Settings == null || Utility.Settings.Security == null || Utility.Settings.Security.SecurityProvider == null)
				return true;

			if (Utility.Settings.Security.SecurityProvider.AjaxTokenEnabled == false)
				return true;
				
			string token = context.Request.Headers["X-" + Constant.AjaxID + "-Token"];

			if (token != null)
			{
				try
				{
					if (Utility.Settings.Security.SecurityProvider.IsValidAjaxToken(token, Utility.Settings.TokenSitePassword))
						return true;
				}
				catch (Exception)
				{
				}
			}

			return false;
		}

        /// <summary>
        /// Retreives the parameters.
        /// </summary>
        /// <returns></returns>
		public virtual object[] RetreiveParameters()
		{
			return null;
		}

        /// <summary>
        /// Serves as a hash function for a particular type. <see cref="M:System.Object.GetHashCode"></see> is suitable for use in hashing algorithms and data structures like a hash table.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"></see>.
        /// </returns>
		public override int GetHashCode()
		{
			throw new NotImplementedException();
		}


        /// <summary>
        /// Serializes the object.
        /// </summary>
        /// <param name="o">The o.</param>
        /// <returns></returns>
		public virtual string SerializeObject(object o)
		{
			return "";
		}

        /// <summary>
        /// Gets a value indicating whether this instance is encryption able.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is encryption able; otherwise, <c>false</c>.
        /// </value>
		public virtual bool IsEncryptionAble
		{
			get
			{
				return false;
			}
		}

        /// <summary>
        /// Gets the ajax method.
        /// </summary>
        /// <value>The ajax method.</value>
		public virtual MethodInfo AjaxMethod
		{
			get
			{
				throw new NotImplementedException();
			}
		}

        /// <summary>
        /// Gets the method attributes.
        /// </summary>
        /// <value>The method attributes.</value>
		public virtual AjaxMethodAttribute[] MethodAttributes
		{
			get
			{
				return m_methodAttributes;
			}
		}

        /// <summary>
        /// Gets the namespace attributes.
        /// </summary>
        /// <value>The namespace attributes.</value>
		public virtual AjaxNamespaceAttribute[] NamespaceAttributes
		{
			get
			{
				return m_namespaceAttributes;
			}
		}

        /// <summary>
        /// Gets the server cache attributes.
        /// </summary>
        /// <value>The server cache attributes.</value>
		public virtual AjaxServerCacheAttribute[] ServerCacheAttributes
		{
			get
			{
				return m_serverCacheAttributes;
			}
		}

        /// <summary>
        /// Gets the type of the content.
        /// </summary>
        /// <value>The type of the content.</value>
		public virtual string ContentType
		{
			get
			{
				return "text/plain";
			}
		}

        public virtual void DemandInvocation()
        {
            if (!AjaxSecurityChecker.Instance.OnCheckMethodPermissions(AjaxMethod))
                throw new UnauthorizedAccessException();
        }

		#endregion
	}
}
