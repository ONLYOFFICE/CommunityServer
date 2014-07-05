/*
 * AuthenticationService.cs
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
 * MS	05-12-20	initial version
 * MS	06-04-16	changed methods to static
 * 
 * 
 * 
 * 
 */
using System;
using System.Web.Security;

namespace AjaxPro.Services
{
	[AjaxNamespace("AjaxPro.Services.Authentication")]
	public class AuthenticationService
	{
        /// <summary>
        /// Logins the specified username.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
		[AjaxMethod]
		public static bool Login(string username, string password)
		{
#if(NET20)
			if(Membership.Provider.ValidateUser(username, password))
#else
			if(FormsAuthentication.Authenticate(username, password))
#endif
			{
				FormsAuthentication.SetAuthCookie(username, false);
				return true;
			}

			return false;
		}

        /// <summary>
        /// Logouts this instance.
        /// </summary>
		[AjaxMethod]
		public static void Logout()
		{
			FormsAuthentication.SignOut();
		}

        /// <summary>
        /// Validates the user.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
		[AjaxMethod]
		public static bool ValidateUser(string username, string password)
		{
#if(NET20)
			return Membership.Provider.ValidateUser(username, password);
#else
			throw new NotImplementedException("ValidateUser is not yet implemented.");
#endif
		}
	}
}
