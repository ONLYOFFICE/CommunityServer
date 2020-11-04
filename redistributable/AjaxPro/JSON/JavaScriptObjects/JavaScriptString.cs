/*
 * JavaScriptString.cs
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
 * MS	06-05-31	added ctor for string and char
 * BT	06-09-13	changed underlying string to StringBuilder object
 * 
 */
using System;
using System.Text;
using System.Collections;

namespace AjaxPro
{
	/// <summary>
	/// Represents a JavaScript ECMA string.
	/// </summary>
	public class JavaScriptString : IJavaScriptObject
	{
		private StringBuilder _value = new StringBuilder();

		/// <summary>
		/// Initializes a new JavaScript string instance.
		/// </summary>
		public JavaScriptString()
			: base()
		{
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="JavaScriptString"/> class.
        /// </summary>
        /// <param name="s">The s.</param>
		public JavaScriptString(string s)
			: base()
		{
			this.Append(s);
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="JavaScriptString"/> class.
        /// </summary>
        /// <param name="c">The c.</param>
		public JavaScriptString(char c)
			: base()
		{
			this.Append(c);
		}

        /// <summary>
        /// Returns the string representation of the object.
        /// </summary>
        /// <value>The value.</value>
		public string Value
		{
			get
			{
				return JavaScriptSerializer.Serialize(_value.ToString());
			}
		}

		#region Internal Methods

        /// <summary>
        /// Appends the specified s.
        /// </summary>
        /// <param name="s">The s.</param>
		internal void Append(string s)
		{
			_value.Append(s);
		}

        /// <summary>
        /// Appends the specified c.
        /// </summary>
        /// <param name="c">The c.</param>
		internal void Append(char c)
		{
			_value.Append(c);
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
			return _value.ToString();
		}

        /// <summary>
        /// Implicit operators the specified o.
        /// </summary>
        /// <param name="o">The o.</param>
        /// <returns></returns>
		public static implicit operator string(JavaScriptString o)
		{
			return o.ToString();
		}

        /// <summary>
        /// Operator +s the specified a.
        /// </summary>
        /// <param name="a">A.</param>
        /// <param name="s">The s.</param>
        /// <returns></returns>
		public static JavaScriptString operator +(JavaScriptString a, string s)
		{
			a.Append(s);

			return a;
		}

        /// <summary>
        /// Operator +s the specified a.
        /// </summary>
        /// <param name="a">A.</param>
        /// <param name="c">The c.</param>
        /// <returns></returns>
		public static JavaScriptString operator +(JavaScriptString a, char c)
		{
			a.Append(c);

			return a;
		}

		#endregion
	}
}

