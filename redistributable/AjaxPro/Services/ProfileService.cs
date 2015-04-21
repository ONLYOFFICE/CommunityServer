/*
 * ProfileService.cs
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
 * MS	05-12-20	initial version
 * MS	06-04-16	changed methods to static
 * MS	06-04-25	changed GetProfile, added ProfileBaseConverter
 * 
 * 
 * 
 */
using System;
using System.Data;
using System.Collections;
using System.Configuration;
using System.Web;
using System.Web.Profile;

namespace AjaxPro.Services
{
	[AjaxNamespace("AjaxPro.Services.Profile")]
	public class ProfileService
	{
        /// <summary>
        /// Gets the profile.
        /// </summary>
        /// <returns></returns>
		[AjaxMethod]
		public static ProfileBase GetProfile()
		{
			return HttpContext.Current.Profile;
		}

        /// <summary>
        /// Gets the profile property.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns></returns>
        [AjaxMethod]
        public static object GetProfileProperty(string property)
        {
            ProfileBase profile = HttpContext.Current.Profile;
            if (profile == null)
            {
                return null;
            }
            return profile[property];
        }

        /// <summary>
        /// Sets the profile.
        /// </summary>
        /// <param name="o">The o.</param>
        /// <returns></returns>
        [AjaxMethod]
        public static bool SetProfile(JavaScriptObject o)
        {
            ProfileBase profile = HttpContext.Current.Profile;
            foreach (string key in o.Keys)
            {
				profile[key] = JavaScriptDeserializer.Deserialize((IJavaScriptObject)o[key], profile[key].GetType());
            }

			return true;
        }
	}
}
#endif