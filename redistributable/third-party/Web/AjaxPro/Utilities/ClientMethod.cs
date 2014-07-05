/*
 * ClientMethod.cs
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
 * MS	06-06-11	added .ToString for client-side usage
 * MS	07-04-24	added UseSimpleObjectNaming
 * 
 */
using System;
using System.Reflection;

namespace AjaxPro
{
	public delegate object[] GetDataHandler(string input, int count);

	public class ClientMethod
	{
        /// <summary>
        /// Returns a ClientMethod instance to get the name of the class and method name on the client-side JavaScript.
        /// </summary>
        /// <param name="method">The MethodInfo.</param>
        /// <returns>
        /// Returns the ClientMethod info, if it is not a AjaxMethod it will return null.
        /// </returns>
		public static ClientMethod FromMethodInfo(MethodInfo method)
		{
			if(method.GetCustomAttributes(typeof(AjaxPro.AjaxMethodAttribute), true).Length == 0)
				return null;

			AjaxPro.AjaxNamespaceAttribute[] classns = (AjaxPro.AjaxNamespaceAttribute[])method.ReflectedType.GetCustomAttributes(typeof(AjaxPro.AjaxNamespaceAttribute), true);
			AjaxPro.AjaxNamespaceAttribute[] methodns = (AjaxPro.AjaxNamespaceAttribute[])method.GetCustomAttributes(typeof(AjaxPro.AjaxNamespaceAttribute), true);

			ClientMethod cm = new ClientMethod();

			if (classns.Length > 0)
				cm.ClassName = classns[0].ClientNamespace;
			else
			{
				if (Utility.Settings.UseSimpleObjectNaming)
					cm.ClassName = method.ReflectedType.Name;
				else
					cm.ClassName = method.ReflectedType.FullName;
			}

			if(methodns.Length > 0)
				cm.MethodName += methodns[0].ClientNamespace;
			else
				cm.MethodName += method.Name;

			return cm;
		}

        /// <summary>
        /// Returns a ClientMethod instance to get the name of the class and method name on the client-side JavaScript.
        /// </summary>
        /// <param name="d">The Delegate.</param>
        /// <returns>
        /// Returns the ClientMethod info, if it is not a AjaxMethod it will return null.
        /// </returns>
		public static ClientMethod FromDelegate(Delegate d)
		{
			if(d == null)
				return null;

			return ClientMethod.FromMethodInfo(d.Method);
		}

		/// <summary>
		/// The name of the class used on the client-side JavaScript.
		/// </summary>
		public string ClassName;

		/// <summary>
		/// The name of the method used on the client-side JavaScript on the class ClassName.
		/// </summary>
		public string MethodName;


        /// <summary>
        /// Returns the full path to the method that can be used on client-side JavaScript code.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </returns>
		public override string ToString()
		{
			return this.ClassName + "." + this.MethodName;
		}
	}
}
