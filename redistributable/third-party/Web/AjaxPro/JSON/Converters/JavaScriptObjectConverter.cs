/*
 * JavaScriptObjectConverter.cs
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
 * MS	06-04-29	initial version
 * MS	06-05-23	using local variables instead of "new Type()" for get De-/SerializableTypes
 * MS	06-07-19	fixed if method argument is from type IJavaScriptObject, now done in IAjaxProcessor
 * MS	06-07-20	added missing deserialize method
 * MS	06-09-24	use QuoteString instead of Serialize
 * MS	06-09-26	improved performance using StringBuilder
 * 
 * 
 */
using System;
using System.Text;

namespace AjaxPro
{
	/// <summary>
	/// Provides methods to serialize and deserialize a JavaScriptObject object.
	/// </summary>
	public class IJavaScriptObjectConverter : IJavaScriptConverter
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="IJavaScriptObjectConverter"/> class.
        /// </summary>
		public IJavaScriptObjectConverter()
			: base()
		{
			m_serializableTypes = new Type[] {
				typeof(IJavaScriptObject),
				typeof(JavaScriptArray),
				typeof(JavaScriptBoolean),
				typeof(JavaScriptNumber),
				typeof(JavaScriptObject),
				typeof(JavaScriptString),
				typeof(JavaScriptSource)
			};

			m_deserializableTypes = m_serializableTypes;
		}

        /// <summary>
        /// Converts an IJavaScriptObject into an NET object.
        /// </summary>
        /// <param name="o">The IJavaScriptObject object to convert.</param>
        /// <param name="t"></param>
        /// <returns>Returns a .NET object.</returns>
		public override object Deserialize(IJavaScriptObject o, Type t)
		{
			return o;
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
			JavaScriptObject j = o as JavaScriptObject;

			if (j == null)
			{
				sb.Append(((IJavaScriptObject)o).Value);
				return;
			}

			bool b = true;

			sb.Append("{");

			foreach (string key in j.Keys)
			{
				if(b){ b = false; }
				else{ sb.Append(","); }

				JavaScriptUtil.QuoteString(key, sb);
				sb.Append(":");

				sb.Append(JavaScriptSerializer.Serialize((IJavaScriptObject)j[key]));
			}

			sb.Append("}");
		}

		//public override bool TryDeserializeValue(IJavaScriptObject jso, Type t, out object o)
		//{
		//    if (typeof(IJavaScriptObject).IsAssignableFrom(t))
		//    {
		//        o = jso;
		//        return true;
		//    }

		//    return base.TryDeserializeValue(jso, t, out o);
		//}
	}
}
