/*
 * JavaScriptArray.cs
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
 * MS	06-05-31	added new ctor for initial array
 * MS	06-07-20	fixed Add method, removed second ambigous one
 * MS	06-09-20	fixed allowing null values
 * 
 */
using System;
using System.Collections;

namespace AjaxPro
{
	/// <summary>
	/// Represents a JavaScript ECMA array.
	/// </summary>
	public class JavaScriptArray : ArrayList, IJavaScriptObject
	{
        /// <summary>
        /// Initializes a new JavaScript array instance.
        /// </summary>
		public JavaScriptArray()
			: base()
		{
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="JavaScriptArray"/> class.
        /// </summary>
        /// <param name="items">The items.</param>
		public JavaScriptArray(IJavaScriptObject[] items)
			: base()
		{
			for (int i = 0; i < items.Length; i++)
				this.Add(items[i]);
		}

        /// <summary>
        /// Gets the <see cref="AjaxPro.IJavaScriptObject"/> at the specified index.
        /// </summary>
        /// <value></value>
		public new IJavaScriptObject this[int index]
		{
			get
			{
				return (IJavaScriptObject)base[index];
			}
		}

        /// <summary>
        /// Adds an object to the end of the <see cref="T:System.Collections.ArrayList"></see>.
        /// </summary>
        /// <param name="value">The <see cref="T:System.Object"></see> to be added to the end of the <see cref="T:System.Collections.ArrayList"></see>. The value can be null.</param>
        /// <returns>
        /// The <see cref="T:System.Collections.ArrayList"></see> index at which the value has been added.
        /// </returns>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.ArrayList"></see> is read-only.-or- The <see cref="T:System.Collections.ArrayList"></see> has a fixed size. </exception>
		public override int Add(object value)
		{
			if (value is IJavaScriptObject || value == null)
				return base.Add(value);

			throw new NotSupportedException();
		}

        /// <summary>
        /// Returns the string representation of the object.
        /// </summary>
        /// <value></value>
		public string Value
		{
			get
			{
				return JavaScriptSerializer.Serialize(this.ToArray());
			}
		}

        /// <summary>
        /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </returns>
		public override string ToString()
		{
			return this.Value;
		}
	}
}
