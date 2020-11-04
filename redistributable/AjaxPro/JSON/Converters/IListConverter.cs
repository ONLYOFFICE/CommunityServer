/*
 * IListConverter.cs
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
 * MS	06-05-23	using local variables instead of "new Type()" for get De-/SerializableTypes
 * MS	06.06.09	added IEnumerable type
 * MS	06-06-09	removed addNamespace use
 * MS	06-09-26	improved performance using StringBuilder
 * 
 */
using System;
using System.Reflection;
using System.Text;
using System.Collections;
using System.Collections.Specialized;
#if(NET20)
using System.Collections.Generic;
#endif

namespace AjaxPro
{
	/// <summary>
	/// Provides methods to serialize and deserialize an object that implements IList.
	/// </summary>
	/// <remarks>
	/// The .Add methods argument type is used to get the type for the list.
	/// </remarks>
	public class IListConverter : IJavaScriptConverter
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="IListConverter"/> class.
        /// </summary>
		public IListConverter() : base()
		{
			m_AllowInheritance = true;

			m_serializableTypes = new Type[] {
#if(NET20)
				typeof(IList<>),
#endif
				typeof(IList), typeof(IEnumerable),
				typeof(string[]), typeof(int[]), typeof(bool[])
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
			IEnumerable enumerable = (IEnumerable)o;
			bool b = true;

			sb.Append("[");

			foreach (object obj in enumerable)
			{
				if (!b) sb.Append(",");

				sb.Append(JavaScriptSerializer.Serialize(obj));

				b = false;
			}

			sb.Append("]");
		}

        /// <summary>
        /// Converts an IJavaScriptObject into an NET object.
        /// </summary>
        /// <param name="o">The IJavaScriptObject object to convert.</param>
        /// <param name="t"></param>
        /// <returns>Returns a .NET object.</returns>
		public override object Deserialize(IJavaScriptObject o, Type t)
		{
			JavaScriptArray list = o as JavaScriptArray;

			if (list == null)
				throw new NotSupportedException();

			if (t.IsArray)
			{
				Type type = Type.GetType(t.AssemblyQualifiedName.Replace("[]", ""));
				Array a = Array.CreateInstance(type, (list != null ? list.Count : 0));

				try
				{
					if (list != null)
					{
						for (int i = 0; i < list.Count; i++)
						{
							object v = JavaScriptDeserializer.Deserialize(list[i], type);
							a.SetValue(v, i);
						}
					}
				}
				catch (System.InvalidCastException iex)
				{
					throw new InvalidCastException("Array ('" + t.Name + "') could not be filled with value of type '" + type.Name + "'.", iex);
				}

				return a;
			}

			if(!typeof(IList).IsAssignableFrom(t) || !(o is JavaScriptArray))
				throw new NotSupportedException();

			IList l = (IList)Activator.CreateInstance(t);

			MethodInfo mi = t.GetMethod("Add");
			ParameterInfo pi = mi.GetParameters()[0];

			for(int i=0; i<list.Count; i++)
			{
				l.Add(JavaScriptDeserializer.Deserialize(list[i], pi.ParameterType));
			}

			return l;
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
			if (t.IsArray) // || typeof(IEnumerable).IsAssignableFrom(t))
			{
				this.Serialize(o, sb);
				return true;
			}

			return base.TrySerializeValue(o, t, sb);
		}
	}
}
