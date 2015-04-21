/*
 * JavaScriptSource.cs
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
 * MS	06-06-19	initial version
 * 
 */
using System;
using System.Collections;

namespace AjaxPro
{
	/// <summary>
	/// Represents a JavaScript ECMA new Object source code.
	/// </summary>
	public class JavaScriptSource : IJavaScriptObject
	{
		private string _value = string.Empty;

		/// <summary>
		/// Initializes a new JavaScript string instance.
		/// </summary>
		public JavaScriptSource()
			: base()
		{
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="JavaScriptSource"/> class.
        /// </summary>
        /// <param name="s">The s.</param>
		public JavaScriptSource(string s)
			: base()
		{
			this.Append(s);
		}

        /// <summary>
        /// Returns the string representation of the object.
        /// </summary>
        /// <value>The value.</value>
		public string Value
		{
			get
			{
				return _value;
			}
		}

		#region Internal Methods

        /// <summary>
        /// Appends the specified s.
        /// </summary>
        /// <param name="s">The s.</param>
		internal void Append(string s)
		{
			_value += s;
		}

		#endregion

		#region Public Operators

        /// <summary>
        /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </returns>
		public override string ToString()
		{
			return _value;
		}

        /// <summary>
        /// Implicit operators the specified o.
        /// </summary>
        /// <param name="o">The o.</param>
        /// <returns></returns>
		public static implicit operator string(JavaScriptSource o)
		{
			return o.ToString();
		}

        /// <summary>
        /// Operator +s the specified a.
        /// </summary>
        /// <param name="a">A.</param>
        /// <param name="s">The s.</param>
        /// <returns></returns>
		public static JavaScriptSource operator +(JavaScriptSource a, string s)
		{
			a.Append(s);

			return a;
		}

		#endregion
	}
}
