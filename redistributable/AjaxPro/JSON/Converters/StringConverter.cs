/*
 * StringConverter.cs
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
 * MS	06-05-24	initial version
 *					allowNumberBooleanAsString
 * MS	06-09-26	improved performance using StringBuilder
 * 
 * 
 */
using System;
using System.Text;
using System.Data;

namespace AjaxPro
{
	/// <summary>
	/// Provides methods to serialize and deserialize a String object.
	/// </summary>
	public class StringConverter : IJavaScriptConverter
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="StringConverter"/> class.
        /// </summary>
		public StringConverter()
			: base()
		{
			m_serializableTypes = new Type[] {
				typeof(String), 
				typeof(Char)
			};
			m_deserializableTypes = m_serializableTypes;
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
			JavaScriptUtil.QuoteString(o.ToString(), sb);
		}

        /// <summary>
        /// Converts an IJavaScriptObject into an NET object.
        /// </summary>
        /// <param name="o">The IJavaScriptObject object to convert.</param>
        /// <param name="t"></param>
        /// <returns>Returns a .NET object.</returns>
		public override object Deserialize(IJavaScriptObject o, Type t)
		{
#if(!JSONLIB)
			if (!Utility.Settings.OldStyle.Contains("allowNumberBooleanAsString"))
#endif
			{
				if (o is JavaScriptNumber)
					return JavaScriptDeserializer.Deserialize(o, typeof(Int64));
				else if (o is JavaScriptBoolean)
					return JavaScriptDeserializer.Deserialize(o, typeof(Boolean));
			}

			if (t == typeof(char))
			{
				string s = o.ToString();

				if (s.Length == 0)
					return '\0';
				return s[0];
			}

			return o.ToString();
		}

        /// <summary>
        /// </summary>
        /// <param name="jso"></param>
        /// <param name="t"></param>
        /// <param name="o"></param>
        /// <returns></returns>
		public override bool TryDeserializeValue(IJavaScriptObject jso, Type t, out object o)
		{
			if (t.IsAssignableFrom(typeof(string)))
			{
				o = this.Deserialize(jso, t);
				return true;
			}

			return base.TryDeserializeValue(jso, t, out o);
		}
	}
}
