/*
 * JavaScriptDeserializer.cs
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
 * MS	06-04-11	fixed array deserializer
 *					fixed ArrayList deserializer, using Int64 if JavaScriptNumber, Boolean if JavaScriptBoolean
 * MS	06-04-12	fixed type char (BUG: will return "\"" instead of ""
 * MS	06-04-13	fixed type char when char is empty JSON string
 * MS	06-04-26	fixed missing enum deserializer
 *					added support for Nullable<>
 * MS	06-05-16	added Generic.IDictionary support
 * MS	06-05-22	added oldStyle/allowNumberBooleanAsString to be compatible
 * MS	06-05-23	added support for Enum that are not integers
 * MS	06-05-30	changed to new converter usage
 * MS	06-07-11	added generic method for DeserializeFromJson
 * MS	06-09-26	improved performance removing three-times cast
 * 
 * 
 */
using System;
using System.Text;
using System.Reflection;
using System.Collections;

namespace AjaxPro
{
	/// <summary>
	/// Provides methods for deserializing .NET objects from a Java Script Object Notation (JSON) string to an .NET object.
	/// </summary>
	public sealed class JavaScriptDeserializer
	{
        /// <summary>
        /// Converts an JSON string into an NET object.
        /// </summary>
        /// <param name="json">The JSON string representation.</param>
        /// <param name="type">The type to convert the string to.</param>
        /// <returns>Returns a .NET object.</returns>
        /// <example>
        /// using System;
        /// using AjaxPro;
        /// namespace Demo
        /// {
        ///		public class WebForm1 : System.Web.UI.Page
        ///		{
        ///			private void Page_Load(object sender, System.EventArgs e)
        ///			{
        ///				string json = "[1,2,3,4,5,6]";
        ///				object o = JavaScriptDeserializer.Deserialize(json, typeof(int[]);
        ///				if(o != null)
        ///				{
        ///					foreach(int i in (int[])o)
        ///					{
        ///						Response.Write(i.ToString());
        ///					}
        ///				}
        ///			}
        ///		}
        /// }
        /// </example>
		public static object DeserializeFromJson(string json, Type type)
		{
			JSONParser p = new JSONParser();
			IJavaScriptObject o = p.GetJSONObject(json);

			return JavaScriptDeserializer.Deserialize(o, type);
		}

#if(NET20)
		
		public static T Deserialize<T>(IJavaScriptObject o)
		{
			return (T)Deserialize(o, typeof(T));
		}

        /// <summary>
        /// Converts an JSON string into an NET object.
        /// </summary>
        /// <param name="json">The JSON string representation.</param>
        /// <returns>Returns a type safe .NET object.</returns>
        /// <typeparam name="T">The type to create from JSON string.</typeparam>
		public static T DeserializeFromJson<T>(string json)
		{
			return (T)DeserializeFromJson(json, typeof(T));
		}
#endif

        /// <summary>
        /// Deserializes the specified json.
        /// </summary>
        /// <param name="json">The json.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
		[Obsolete("The recommended alternative is AjaxPro.JavaScriptDeserializer.DeserializeFromJson(string, Type).")]
		public static object Deserialize(string json, Type type)
		{
			return DeserializeFromJson(json, type);
		}

        /// <summary>
        /// Converts an IJavaScriptObject into an NET object.
        /// </summary>
        /// <param name="o">The IJavaScriptObject object to convert.</param>
        /// <param name="type">The type to convert the object to.</param>
        /// <returns>Returns a .NET object.</returns>
		public static object Deserialize(IJavaScriptObject o, Type type)
		{
			if (o == null)
			{
				return null;
			}

			// If the IJavaScriptObject is an JavaScriptObject and we have a key
			// __type we will override the Type that is passed to this method. This
			// will allow us to use implemented classes where the method will use
			// only the interface.

			JavaScriptObject jso = o as JavaScriptObject;

			if (jso != null && jso.Contains("__type"))
			{
				Type t = Type.GetType(jso["__type"].ToString());
				if (type == null || type.IsAssignableFrom(t))
					type = t;
			}


			IJavaScriptConverter c = null;
#if(NET20)
			if (Utility.Settings.DeserializableConverters.TryGetValue(type, out c))
			{
#else
			if(Utility.Settings.DeserializableConverters.ContainsKey(type))
			{
				c = (IJavaScriptConverter)Utility.Settings.DeserializableConverters[type];
#endif
				return c.Deserialize(o, type);
			}

			object v;
#if(NET20)
			foreach(IJavaScriptConverter c2 in Utility.Settings.DeserializableConverters.Values)
			{
				if(c2.TryDeserializeValue(o, type, out v))
					return v;
			}
#else
			IEnumerator m = Utility.Settings.DeserializableConverters.Values.GetEnumerator();
			while(m.MoveNext())
			{
				if (((IJavaScriptConverter)m.Current).TryDeserializeValue(o, type, out v))
					return v;
			}
#endif


#if(NET20)
			if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
			{
				//Type n = typeof(Nullable<>);
				//Type constructed = n.MakeGenericType(type.GetGenericArguments()[0]);

				Type tval = type.GetGenericArguments()[0];
				object va = Deserialize(o, tval);
				object val = Convert.ChangeType(va, tval);

				return Activator.CreateInstance(type, val);
			}
#endif

			if (typeof(IJavaScriptObject).IsAssignableFrom(type))
			{
				return o;
			}

			if (o is JavaScriptObject)
			{
				return DeserializeCustomObject(o as JavaScriptObject, type);
			}

			throw new NotImplementedException("The object of type '" + o.GetType().FullName + "' could not converted to type '" + type + "'.");
		}

		#region Internal Methods

        /// <summary>
        /// Deserializes the custom object.
        /// </summary>
        /// <param name="o">The o.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
		internal static object DeserializeCustomObject(JavaScriptObject o, Type type)
		{
			object c = Activator.CreateInstance(type);

			// TODO: is this a security problem?
			// if(o.GetType().GetCustomAttributes(typeof(AjaxClassAttribute), true).Length == 0)
			//	throw new System.Security.SecurityException("Could not create class '" + type.FullName + "' because of missing AjaxClass attribute.");

			MemberInfo[] members = type.GetMembers(BindingFlags.GetField | BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance);
			foreach (MemberInfo memberInfo in members)
			{
				if (memberInfo.MemberType == MemberTypes.Field)
				{
					if (o.Contains(memberInfo.Name))
					{
						FieldInfo fieldInfo = (FieldInfo)memberInfo;

						fieldInfo.SetValue(c, JavaScriptDeserializer.Deserialize((IJavaScriptObject)o[fieldInfo.Name], fieldInfo.FieldType));
					}
				}
				else if (memberInfo.MemberType == MemberTypes.Property)
				{
					if (o.Contains(memberInfo.Name))
					{
						PropertyInfo propertyInfo = (PropertyInfo)memberInfo;

						if (propertyInfo.CanWrite)
							propertyInfo.SetValue(c, JavaScriptDeserializer.Deserialize((IJavaScriptObject)o[propertyInfo.Name], propertyInfo.PropertyType), new object[0]);
					}
				}
			}

			return c;
		}

		#endregion
	}
}


