/*
 * AjaxSecurityProvider.cs
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
 * MS	07-04-13	initial version
 *					copied parts from IAjaxCryptProvider
 *					changed to abstract class from interface
 * 
 * 
 */
using System;

namespace AjaxPro
{
	public abstract class AjaxSecurityProvider
	{
        /// <summary>
        /// Encrypts the specified json.
        /// </summary>
        /// <param name="json">The json.</param>
        /// <returns></returns>
		public virtual string Encrypt(string json)
		{
			return json;
		}
        
		/// <summary>
        /// Decrypts the specified jsoncrypt.
        /// </summary>
        /// <param name="jsoncrypt">The jsoncrypt.</param>
        /// <returns></returns>
		public virtual string Decrypt(string jsoncrypt)
		{
			return jsoncrypt;
		}

        /// <summary>
        /// Gets the client script.
        /// </summary>
        /// <value>The client script.</value>
		public virtual string ClientScript
		{
			get
			{
				return null;
			}
		}

		/// <summary>
		/// Checks if the given token is valid.
		/// </summary>
		/// <param name="token">The token from the Ajax request.</param>
		/// <param name="sitePassword"></param>
		/// <returns>Returns true if token is valid and AjaxMethod could be invoked.</returns>
		public virtual bool IsValidAjaxToken(string token, string sitePassword)
		{
			return (token == sitePassword);
		}

		/// <summary>
		/// Create a new token.
		/// </summary>
		/// <param name="sitePassword"></param>
		/// <returns></returns>
		public virtual string GetAjaxToken(string sitePassword)
		{
			return sitePassword;
		}

		public virtual bool AjaxTokenEnabled
		{
			get
			{
				return false;
			}
		}
	}
}
