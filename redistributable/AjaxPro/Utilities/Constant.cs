/*
 * Constant.cs
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
 * MS	06-03-10	changed assembly version to correct version from assembly info
 * MS	06-04-04	fixed if external version is using different assembly name
 * 
 * 
 */
using System;

namespace AjaxPro
{
	public sealed class Constant
	{
		/// <summary>
		/// The AjaxID used to save objects using a unnique key in IDictionary objects.
		/// </summary>
		public const string AjaxID = "AjaxPro";

		/// <summary>
		/// The assembly name to get embedded resources
		/// </summary>
#if(NET20external)
		internal const string AssemblyName = "AjaxPro.2";
#else
		internal const string AssemblyName = "AjaxPro";
#endif

        /// <summary>
		/// The assembly version.
		/// </summary>
        public const string AssemblyVersion = "9.2.17.1";
	}
}
