/*
 * AjaxNamespaceAttribute.cs
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
 * MS	06-09-26	put regex to private const
 * MS	06-09-29	added enum support
 * 
 * 
 */
using System;

namespace AjaxPro
{
	/// <summary>
	/// This attribute can be used to specified a different namespace for the client-side representation.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Enum, AllowMultiple = false)]
	public class AjaxNamespaceAttribute : Attribute
	{
		private string _clientNS = null;
		private System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex("^[a-zA-Z_]{1}([a-zA-Z_]*([\\d]*)?)*((\\.)?[a-zA-Z_]+([\\d]*)?)*$", System.Text.RegularExpressions.RegexOptions.Compiled);

        /// <summary>
        /// This attribute can be used to specified a different namespace for the client-side representation.
        /// </summary>
        /// <param name="clientNS">The namespace to be used on the client-side JavaScript.</param>
		public AjaxNamespaceAttribute(string clientNS)
		{
            if(!r.IsMatch(clientNS) || clientNS.StartsWith(".") || clientNS.EndsWith("."))
                throw new NotSupportedException("The namespace '" + clientNS + "' is not supported.");

			_clientNS = clientNS;
		}

		#region Internal Properties

        /// <summary>
        /// Gets the client namespace.
        /// </summary>
        /// <value>The client namespace.</value>
		internal string ClientNamespace
		{
			get
			{
                if (_clientNS != null && _clientNS.Trim().Length > 0)
                    return _clientNS.Replace("-", "_").Replace(" ", "_");

				return _clientNS;
			}
		}

		#endregion
	}
}
