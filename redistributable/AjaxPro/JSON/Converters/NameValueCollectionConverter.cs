/*
 * NameValueCollection.cs
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
 * MS	06-06-09	removed addNamespace use
 * MS	06-06-13	fixed if key includes blanks
 *					fixed __type is not a key
 *					fixed missing initial values (for AjaxMethod invoke return value!!)
 * MS	06-06-14	changed access to keys and values
 * MS	06-09-22	added inheritance to get HttpValueCollection working again
 * MS	06-09-24	use QuoteString instead of Serialize
 * MS	06-09-26	improved performance using StringBuilder
 * MS	07-04-24	added renderJsonCompliant serialization
 * 
 * 
 */
using System;
using System.Text;
using System.Collections;
using System.Collections.Specialized;

namespace AjaxPro
{
	/// <summary>
	/// Provides methods to serialize and deserialize a NameValueCollection.
	/// </summary>
	public class NameValueCollectionConverter : IJavaScriptConverter
	{
		private string clientType = "Ajax.Web.NameValueCollection";

        /// <summary>
        /// Initializes a new instance of the <see cref="NameValueCollectionConverter"/> class.
        /// </summary>
		public NameValueCollectionConverter()
			: base()
		{
			m_serializableTypes = new Type[] { typeof(NameValueCollection) };
			m_deserializableTypes = new Type[] { typeof(NameValueCollection) };

			m_AllowInheritance = true;
		}

        /// <summary>
        /// Render the JavaScript code for prototypes or any other JavaScript method needed from this converter
        /// on the client-side.
        /// </summary>
        /// <returns>Returns JavaScript code.</returns>
		public override string GetClientScript()
		{
			if (AjaxPro.Utility.Settings.OldStyle.Contains("renderJsonCompliant"))
				return "";

			return JavaScriptUtil.GetClientNamespaceRepresentation(clientType) + @"
" + clientType + @" = function(items) {
	this.__type = ""System.Collections.Specialized.NameValueCollection"";
	this.keys = [];
	this.values = [];

	if(items != null && !isNaN(items.length)) {
		for(var i=0; i<items.length; i++)
			this.add(items[i][0], items[i][1]);
	}
};
Object.extend(" + clientType + @".prototype, {
	add: function(k, v) {
		if(k == null || k.constructor != String || v == null || v.constructor != String)
			return -1;
		this.keys.push(k);
		this.values.push(v);
		return this.values.length -1;
	},
	containsKey: function(key) {
		for(var i=0; i<this.keys.length; i++) {
			if(this.keys[i] == key) return true;
		}
		return false;
	},
	getKeys: function() {
		return this.keys;
	},
	getValue: function(k) {
		for(var i=0; i<this.keys.length && i<this.values.length; i++) {
			if(this.keys[i] == k) return this.values[i];
		}
		return null;
	},
	setValue: function(k, v) {
		if(k == null || k.constructor != String || v == null || v.constructor != String)
			return -1;
		for(var i=0; i<this.keys.length && i<this.values.length; i++) {
			if(this.keys[i] == k) this.values[i] = v;
			return i;
		}
		return this.add(k, v);
	},
	toJSON: function() {
		return AjaxPro.toJSON({__type:this.__type,keys:this.keys,values:this.values});
	}
}, true);
";
		}

        /// <summary>
        /// Converts an IJavaScriptObject into an NET object.
        /// </summary>
        /// <param name="o">The IJavaScriptObject object to convert.</param>
        /// <param name="t"></param>
        /// <returns>Returns a .NET object.</returns>
		public override object Deserialize(IJavaScriptObject o, Type t)
		{
			JavaScriptObject jso = o as JavaScriptObject;
			if (!typeof(NameValueCollection).IsAssignableFrom(t) || jso == null)
				throw new NotSupportedException();

			NameValueCollection list = (NameValueCollection)Activator.CreateInstance(t);

			if (!AjaxPro.Utility.Settings.OldStyle.Contains("renderJsonCompliant"))
			{

				if (!jso.Contains("keys") || !jso.Contains("values"))
					throw new ArgumentException("Missing values for 'keys' and 'values'.");

				JavaScriptArray keys = (JavaScriptArray)jso["keys"];
				JavaScriptArray values = (JavaScriptArray)jso["values"];

				if (keys.Count != values.Count)
					throw new IndexOutOfRangeException("'keys' and 'values' have different length.");

				for (int i = 0; i < keys.Count; i++)
				{
					list.Add(keys[i].ToString(), values[i].ToString());
				}
			}
			else
			{
				foreach (string key in jso.Keys)
				{
					list.Add(key, jso[key].ToString());
				}
			}

			return list;
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
			NameValueCollection list = o as NameValueCollection;

			if (list == null)
				throw new NotSupportedException();

			bool b = true;

			if (!AjaxPro.Utility.Settings.OldStyle.Contains("renderJsonCompliant"))
			{
				sb.Append("new ");
				sb.Append(clientType);
				sb.Append("(");
				sb.Append("[");

				for (int i = 0; i < list.Keys.Count; i++)
				{
					if (!b) sb.Append(",");

					sb.Append('[');
					JavaScriptUtil.QuoteString(list.Keys[i], sb);
					sb.Append(',');
					JavaScriptUtil.QuoteString(list[list.Keys[i]], sb);
					sb.Append(']');

					b = false;
				}

				sb.Append("]");
				sb.Append(")");
			}
			else
			{
				sb.Append("{");

				for (int i = 0; i < list.Keys.Count; i++)
				{
					if (!b) sb.Append(",");

					JavaScriptUtil.QuoteString(list.Keys[i], sb);
					sb.Append(':');
					JavaScriptUtil.QuoteString(list[list.Keys[i]], sb);

					b = false;
				}

				sb.Append("}");
			}
		}
	}
}
