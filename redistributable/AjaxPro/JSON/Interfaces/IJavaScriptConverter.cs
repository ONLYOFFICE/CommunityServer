/*
 * IJavaScriptConverter.cs
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
 * MS	06-05-23	added de-/serialzableTypes variable instead of "new Type[]"
 * MS	06-05-30	added TrySerializeValue and TryDeserializeValue
 * MS	06-06-12	added StringDictionary argument for .Initialize
 * MS	06-09-26	improved performance using StringBuilder, new Serialize and TrySerializeValue
 * 
 * 
 * 
 */
using System;
using System.Text;
using System.Collections.Specialized;

namespace AjaxPro
{
	/// <summary>
	/// Represents an IJavaScriptConverter.
	/// </summary>
	public class IJavaScriptConverter
	{
		protected bool m_AllowInheritance = false;
		protected Type[] m_serializableTypes = new Type[0];
		protected Type[] m_deserializableTypes = new Type[0];

        /// <summary>
        /// Initializes the converter. This method will be called when the application is starting and
        /// any converter is loaded.
        /// </summary>
        /// <param name="d">The d.</param>
		public virtual void Initialize(StringDictionary d)
		{
		}

        /// <summary>
        /// Render the JavaScript code for prototypes or any other JavaScript method needed from this converter
        /// on the client-side.
        /// </summary>
        /// <returns>Returns JavaScript code.</returns>
		public virtual string GetClientScript()
		{
			return "";
		}

        /// <summary>
        /// Converts a .NET object into a JSON string.
        /// </summary>
        /// <param name="o">The object to convert.</param>
        /// <returns>Returns a JSON string.</returns>
		public virtual string Serialize(object o)
		{
			throw new NotImplementedException("Converter for type '" + o.GetType().FullName + "'.");
		}

        /// <summary>
        /// Serializes the specified o.
        /// </summary>
        /// <param name="o">The o.</param>
        /// <param name="sb">The sb.</param>
		public virtual void Serialize(object o, StringBuilder sb)
		{
			sb.Append(Serialize(o));
		}

		/// <summary>
		/// Converts an IJavaScriptObject into an NET object.
		/// </summary>
		/// <param name="o">The IJavaScriptObject object to convert.</param>
		/// <param name="type">The type to convert the object to.</param>
		/// <returns>Returns a .NET object.</returns>
		public virtual object Deserialize(IJavaScriptObject o, Type t)
		{
			return null;
		}

		#region Try'nParse Methods

        /// <summary>
        /// Tries the serialize value.
        /// </summary>
        /// <param name="o">The o.</param>
        /// <param name="t">The t.</param>
        /// <param name="json">The json.</param>
        /// <returns></returns>
		[Obsolete("The recommended alternative is IJavaScriptConverter.TrySerializeValue(object, t, sb).", true)]
		public virtual bool TrySerializeValue(object o, Type t, out string json)
		{
			StringBuilder sb = new StringBuilder();
			json = null;

			if (TrySerializeValue(o, t, sb))
			{
				json = sb.ToString();
				return true;
			}

			return false;
		}

        /// <summary>
        /// Tries the serialize value.
        /// </summary>
        /// <param name="o">The o.</param>
        /// <param name="t">The t.</param>
        /// <param name="sb">The sb.</param>
        /// <returns></returns>
		public virtual bool TrySerializeValue(object o, Type t, StringBuilder sb)
		{
			if (m_AllowInheritance)
			{
				for (int i = 0; i < m_serializableTypes.Length; i++)
				{
					if (m_serializableTypes[i].IsAssignableFrom(t))
					{
						this.Serialize(o, sb);
						return true;
					}
				}
			}

			return false;
		}

        /// <summary>
        /// Tries the deserialize value.
        /// </summary>
        /// <param name="jso">The jso.</param>
        /// <param name="t">The t.</param>
        /// <param name="o">The o.</param>
        /// <returns></returns>
		public virtual bool TryDeserializeValue(IJavaScriptObject jso, Type t, out object o)
		{
			if (m_AllowInheritance)
			{
				for (int i = 0; i < m_deserializableTypes.Length; i++)
				{
					if (m_deserializableTypes[i].IsAssignableFrom(t))
					{
						o = this.Deserialize(jso, t);
						return true;
					}
				}
			}

			o = null;
			return false;
		}

		#endregion

		#region Properties

        /// <summary>
        /// Returns every type that can be used with this converter to serialize an object.
        /// </summary>
        /// <value>The serializable types.</value>
		public virtual Type[] SerializableTypes
		{
			get
			{
				return m_serializableTypes;
			}
		}

        /// <summary>
        /// Returns every type that can be used with this converter to deserialize an JSON string.
        /// </summary>
        /// <value>The deserializable types.</value>
		public virtual Type[] DeserializableTypes
		{
			get
			{
				return m_deserializableTypes;
			}
		}

        /// <summary>
        /// Gets the name of the converter.
        /// </summary>
        /// <value>The name of the converter.</value>
		public virtual string ConverterName
		{
			get
			{
				return this.GetType().Name;
			}
		}

		#endregion
	}
}