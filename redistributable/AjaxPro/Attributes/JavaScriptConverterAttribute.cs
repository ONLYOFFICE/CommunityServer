/*
 * JavaScriptConverterAttribute.cs
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
	/// Represents an attribute to mark a class to be converted by a specified IJavaScriptConverter.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	[Obsolete("The recommended alternative is adding the converter to ajaxNet/ajaxSettings/jsonConverters.", true)]
	public class JavaScriptConverterAttribute : Attribute
	{
		private Type type = null;

        /// <summary>
        /// Marks a class to be converted by the specified JavaScript converter.
        /// </summary>
        /// <param name="type">The IJavaScriptConverter to use to serialize or deserialize.</param>
		public JavaScriptConverterAttribute(Type type)
		{
			if(!(typeof(IJavaScriptConverter).IsAssignableFrom(type)))
				throw new InvalidCastException();

			this.type = type;
		}

		#region Internal Methods

        /// <summary>
        /// Gets the converter.
        /// </summary>
        /// <value>The converter.</value>
		internal IJavaScriptConverter Converter
		{
			get
			{
				object o = Activator.CreateInstance(type);
				return (IJavaScriptConverter)o;
			}
		}

		#endregion
	}
}
