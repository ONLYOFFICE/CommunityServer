/*
 * AjaxCacheAttribute.cs
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
using System;

namespace AjaxPro
{
    /// <summary>
    /// AJAX Server Attribute Cache
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public class AjaxServerCacheAttribute : Attribute
	{
		private TimeSpan cacheDuration;
		private bool isCacheEnabled = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="AjaxServerCacheAttribute"/> class.
        /// </summary>
        /// <param name="seconds">The seconds.</param>
		public AjaxServerCacheAttribute(int seconds)
		{
			if(seconds > 0)
			{
				cacheDuration = new TimeSpan(0, 0, 0, seconds, 0);
				isCacheEnabled = true;
			}
		}

		#region Internal Properties

        /// <summary>
        /// Gets a value indicating whether this instance is cache enabled.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is cache enabled; otherwise, <c>false</c>.
        /// </value>
		internal bool IsCacheEnabled
		{
			get
			{
				return isCacheEnabled;
			}
		}

        /// <summary>
        /// Gets the cache duration.
        /// </summary>
        /// <value>The cache duration.</value>
		internal TimeSpan CacheDuration
		{
			get
			{
				return cacheDuration;
			}
		}

		#endregion
	}
}
