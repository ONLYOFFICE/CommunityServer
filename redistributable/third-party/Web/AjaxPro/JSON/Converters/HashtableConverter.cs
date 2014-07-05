/*
 * HashtableConverter.cs
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
 * MS	05-12-21	added Deserialize for Hashtables
 *					JavaScript object will now include the type for key and value
 * MS	06-04-25	removed unnecessarily used cast
 * MS	06-05-17	copy of old IDictionaryConverter
 * MS	06-05-23	using local variables instead of "new Type()" for get De-/SerializableTypes
 * MS	06-09-24	use QuoteString instead of Serialize
 * MS	06-09-26	improved performance using StringBuilder
 * MS	07-04-24	added renderJsonCompliant serialization
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
	/// Provides methods to serialize and deserialize an object that implements IDictionary.
	/// </summary>
	public class HashtableConverter : IJavaScriptConverter
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="HashtableConverter"/> class.
        /// </summary>
		public HashtableConverter()
			: base()
		{
			m_serializableTypes = new Type[] { typeof(Hashtable) };
			m_deserializableTypes = new Type[] { typeof(Hashtable) };
		}

        /// <summary>
        /// Converts an IJavaScriptObject into an NET object.
        /// </summary>
        /// <param name="o">The IJavaScriptObject object to convert.</param>
        /// <param name="t"></param>
        /// <returns>Returns a .NET object.</returns>
		public override object Deserialize(IJavaScriptObject o, Type t)
		{
			if (!AjaxPro.Utility.Settings.OldStyle.Contains("renderJsonCompliant"))
			{
				if (!(o is JavaScriptArray))
					throw new NotSupportedException();

				JavaScriptArray a = (JavaScriptArray)o;

				for (int i = 0; i < a.Count; i++)
					if (!(a[i] is JavaScriptArray))
						throw new NotSupportedException();

				IDictionary d = (IDictionary)Activator.CreateInstance(t);

				object key;
				object value;
				JavaScriptArray aa;

				for (int i = 0; i < a.Count; i++)
				{
					aa = (JavaScriptArray)a[i];
					key = JavaScriptDeserializer.Deserialize((IJavaScriptObject)aa[0], Type.GetType(((JavaScriptString)aa[2]).ToString()));
					value = JavaScriptDeserializer.Deserialize((IJavaScriptObject)aa[1], Type.GetType(((JavaScriptString)aa[3]).ToString()));

					d.Add(key, value);
				}

				return d;
			}

			if (!(o is JavaScriptObject))
				throw new NotSupportedException();

			JavaScriptObject oa = (JavaScriptObject)o;
			IDictionary od = (IDictionary)Activator.CreateInstance(t);

			foreach (string key in oa.Keys)
			{
				od.Add(key, JavaScriptDeserializer.Deserialize(oa[key], typeof(string)));
			}

			return od;
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
			IDictionary dic = o as IDictionary;

			if (dic == null)
				throw new NotSupportedException();

			IDictionaryEnumerator enumerable = dic.GetEnumerator();

			enumerable.Reset();
			bool b = true;

			if (!AjaxPro.Utility.Settings.OldStyle.Contains("renderJsonCompliant"))
				sb.Append("[");
			else
				sb.Append("{");

			while (enumerable.MoveNext())
			{
				if (b) { b = false; }
				else { sb.Append(","); }

				if (!AjaxPro.Utility.Settings.OldStyle.Contains("renderJsonCompliant"))
				{
					sb.Append('[');
					JavaScriptSerializer.Serialize(enumerable.Key, sb);
					sb.Append(',');
					JavaScriptSerializer.Serialize(enumerable.Value, sb);
					sb.Append(',');
					JavaScriptUtil.QuoteString(enumerable.Key.GetType().FullName, sb);
					sb.Append(',');
					JavaScriptUtil.QuoteString(enumerable.Value.GetType().FullName, sb);
					sb.Append(']');
				}
				else
				{
					if (enumerable.Key is String)
						JavaScriptSerializer.Serialize(enumerable.Key, sb);
					else if(enumerable.Key is Boolean)
						JavaScriptSerializer.Serialize((bool)enumerable.Key == true ? "true" : "false", sb);
					else
						JavaScriptUtil.QuoteString(enumerable.Key.ToString(), sb);

					sb.Append(':');
					JavaScriptSerializer.Serialize(enumerable.Value, sb);
				}
			}

			if (!AjaxPro.Utility.Settings.OldStyle.Contains("renderJsonCompliant"))
				sb.Append("]");
			else
				sb.Append("}");
		}
	}
}