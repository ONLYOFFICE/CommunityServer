/*
 * AjaxEncryption.cs
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
 * MS	07-04-13	renamed class from AjaxEncryption to AjaxSecurity
 * MS	07-04-24	using new AjaxSecurityProvider
 * 
 */
using System;

namespace AjaxPro
{
	internal class AjaxSecurity
	{
		private string m_SecurityProviderType = null;
		private AjaxSecurityProvider m_SecurityProvider = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="AjaxEncryption"/> class.
        /// </summary>
        /// <param name="cryptType">Type of the crypt.</param>
		internal AjaxSecurity(string securityProviderType)
		{
			m_SecurityProviderType = securityProviderType;
		}

        /// <summary>
        /// Inits this instance.
        /// </summary>
        /// <returns></returns>
		internal bool Init()
		{
			try
			{
				if (m_SecurityProviderType != null)
				{
					Type t = Type.GetType(m_SecurityProviderType);

					if (t == null)
						return false;

					m_SecurityProvider = (AjaxSecurityProvider)Activator.CreateInstance(t, new object[0]);
				}
			}
			catch(Exception)
			{
				return false;
			}

			return true;
		}

		#region Public Properties

        /// <summary>
        /// Gets the security provider.
        /// </summary>
        /// <value>The crypt provider.</value>
		public AjaxSecurityProvider SecurityProvider
		{
			get{ return m_SecurityProvider; }
		}

		#endregion
	}
}
