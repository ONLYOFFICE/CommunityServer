/*
 * IEnumerableConverter.cs
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
 * MS	06-04-25	removed unnecessarily used cast
 * MS	06-05-23	using local variables instead of "new Type()" for get De-/SerializableTypes
 * MS	06-09-26	improved performance using StringBuilder
 * 
 * 
 * 
 */
using System;
using System.Reflection;
using System.Text;
using System.Collections;
using System.Collections.Specialized;

namespace AjaxPro
{
	/// <summary>
	/// Provides methods to serialize and deserialize an object that implements IEnumerable.
	/// </summary>
	public class IEnumerableConverter : IJavaScriptConverter
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="IEnumerableConverter"/> class.
        /// </summary>
		public IEnumerableConverter() : base()
		{
			m_AllowInheritance = true;

			m_serializableTypes = new Type[] { typeof(IEnumerable) };
		}

        /// <summary>
        /// Converts a .NET object into a JSON string.
        /// </summary>
        /// <param name="o">The object to convert.</param>
        /// <returns>Returns a JSON string.</returns>
		public override string Serialize(object o)
		{
			StringBuilder sb = new StringBuilder();
			Serialize(o, sb);
			return sb.ToString();
		}

        /// <summary>
        /// Serializes the specified o.
        /// </summary>
        /// <param name="o">The o.</param>
        /// <param name="sb">The sb.</param>
		public override void Serialize(object o, StringBuilder sb)
		{
			IEnumerable enumerable = o as IEnumerable;

			if(enumerable == null)
				throw new NotSupportedException();

			bool b = true;

			sb.Append("[");
				
			foreach(object obj in enumerable)
			{
				if(b){ b = false; }
				else{ sb.Append(","); }

				sb.Append(JavaScriptSerializer.Serialize(obj));
			}

			sb.Append("]");
		}

        /// <summary>
        /// Tries the serialize value.
        /// </summary>
        /// <param name="o">The o.</param>
        /// <param name="t">The t.</param>
        /// <param name="sb">The sb.</param>
        /// <returns></returns>
		public override bool TrySerializeValue(object o, Type t, StringBuilder sb)
		{
			if (typeof(IDictionary).IsAssignableFrom(t))
			{
				return false;
			}

			return base.TrySerializeValue(o, t, sb);
		}

        /// <summary>
        /// </summary>
        /// <param name="jso"></param>
        /// <param name="t"></param>
        /// <param name="o"></param>
        /// <returns></returns>
		public override bool TryDeserializeValue(IJavaScriptObject jso, Type t, out object o)
		{
			if (typeof(IDictionary).IsAssignableFrom(t))
			{
				o = null;
				return false;
			}

			return base.TryDeserializeValue(jso, t, out o);
		}
	}
}
