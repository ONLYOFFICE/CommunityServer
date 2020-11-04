/*
 * JavaScriptBoolean.cs
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
 * MS	06-04-03	return the correct .Value
 * MS	06-04-29	fixed ToString and Value properties
 * 
 * 
 * 
 */
using System;
using System.Collections;

namespace AjaxPro
{
	/// <summary>
	/// Represents a JavaScript ECMA boolean.
	/// </summary>
	public class JavaScriptBoolean : IJavaScriptObject
	{
		private bool _value = false;

        /// <summary>
        /// Initializes a new JavaScript boolean instance.
        /// </summary>
		public JavaScriptBoolean() : base()
		{

		}

        /// <summary>
        /// Initializes a new JavaScript boolean instance.
        /// </summary>
        /// <param name="value">The pre-defined value.</param>
		public JavaScriptBoolean(bool value) : base()
		{
			_value = value;
		}

        /// <summary>
        /// Returns the string representation of the object.
        /// </summary>
        /// <value>The value.</value>
		public string Value
		{
			get
			{
				return JavaScriptSerializer.Serialize(_value);
			}
		}

		#region Public Operators

        /// <summary>
        /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </returns>
		public override string ToString()
		{
			return bool.Parse(this.Value).ToString();
		}

        /// <summary>
        /// Implicit operators the specified o.
        /// </summary>
        /// <param name="o">The o.</param>
        /// <returns></returns>
		public static implicit operator bool(JavaScriptBoolean o)
		{
			return bool.Parse(o.Value);
		}

        /// <summary>
        /// Implicit operators the specified o.
        /// </summary>
        /// <param name="o">The o.</param>
        /// <returns></returns>
		public static implicit operator string(JavaScriptBoolean o)
		{
			return o.ToString();
		}

		#endregion
	}
}