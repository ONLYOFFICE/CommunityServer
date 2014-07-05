/*
 * JavaScriptObject.cs
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
 * MS	06-05-31	using type safe generic list for .NET 2.0
 * 
 */
using System;
using System.Collections;
using System.Collections.Specialized;
#if(NET20)
using System.Collections.Generic;
#endif

namespace AjaxPro
{
	/// <summary>
	/// Represents a JavaScript ECMA object.
	/// </summary>
	public class JavaScriptObject : IJavaScriptObject
	{
#if(NET20)
		private Dictionary<string, IJavaScriptObject> list = new Dictionary<string, IJavaScriptObject>();
#else
		private HybridDictionary list = new HybridDictionary();
		private StringCollection keys = new StringCollection();
#endif

        /// <summary>
        /// Initializes a new JavaScript object instance.
        /// </summary>
		public JavaScriptObject() : base()
		{
		}

        /// <summary>
        /// Returns the string representation of the object.
        /// </summary>
        /// <value>The value.</value>
		public string Value
		{
			get
			{
				return JavaScriptSerializer.Serialize(list);
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

		#region IDictionary Members

        /// <summary>
        /// Returns the object defined for the name of the property.
        /// </summary>
        /// <value></value>
		public IJavaScriptObject this[string key]
		{
			get
			{
#if(NET20)
				return list[key];
#else
				return (IJavaScriptObject)list[key];
#endif
			}
			set
			{
				if (!this.Contains(key))
					throw new ArgumentException("The specified key does not exists in this collection.", "key");

				list[key] = value;
			}
		}

        /// <summary>
        /// Verify if the property does exist in the object.
        /// </summary>
        /// <param name="key">The name of the property.</param>
        /// <returns>Returns true if the property is defined.</returns>
		public bool Contains(string key)
		{
#if(NET20)
			return list.ContainsKey(key);
#else
			return list.Contains(key);
#endif
		}

        /// <summary>
        /// Adds a new property to the object.
        /// </summary>
        /// <param name="key">The name of the property.</param>
        /// <param name="value">The value of the property.</param>
		public void Add(string key, IJavaScriptObject value)
		{
#if(NET20)
			list.Add(key, value);
#else
			keys.Add(key);
			list.Add(key, value);
#endif
		}

//        public void Add(string key, string value)
//        {
//#if(NET20)
//            list.Add(key, new JavaScriptString(value));
//#else
//            keys.Add(key);
//            list.Add(key, new JavaScriptString(value));
//#endif
//        }

        /// <summary>
        /// Returns all keys that are used internal for the name of properties.
        /// </summary>
        /// <value>The keys.</value>
		public string[] Keys
		{
			get
			{
#if(NET20)
				string[] _keys = new string[list.Count];
				list.Keys.CopyTo(_keys, 0);
#else
				string[] _keys = new string[keys.Count];
				keys.CopyTo(_keys, 0);
#endif

				return _keys;
			}
		}

        /// <summary>
        /// Gets a value indicating whether this instance is fixed size.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is fixed size; otherwise, <c>false</c>.
        /// </value>
		public bool IsFixedSize
		{
			get
			{
				return false;
			}
		}

		#endregion
	}
}

