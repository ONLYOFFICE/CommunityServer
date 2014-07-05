/*
 * JavaScriptUtil.cs
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
 * MS	06-04-04	fixed GetEnumRepresentation if type.FullName has no "."
 * MS	06-04-12	fixed \0 return value for QuoteString, will be simply removed from the string
 * MS	06-04-29	added ConvertXmlToIJavaScriptObject and fixed ConvertIJavaScriptObjectToXML
 * MS	06-05-17	fixed enum support, now supporting any type for enum values
 * MS	06-05-30	fixed QuoteString to include the "
 *					added QuoteHtmlAttribute
 * MS	06-06-09	removed addNamespace use, added new GetClientNamespaceRepresentation method
 * MS	06-60-19	added GetIJavaScriptObjectFromXmlNode
 * MS	06-09-15	fixed bug when using special chars in a string below ASCII 32
 * MS	06-09-26	improved performance using StringBuilder for quotestring methods
 * MS	06-09-29	removed addNamespace use for GetEnumRepresentation
 * MS	06-10-03	fixed GetClientNamespaceRepresentation when using more than one point in namespace
 * MS	07-04-24	fixed client-side namespace representation
 * MS	08-03-20	fixed client-side namespace (missing last part)
 * 
 */
using System;
using System.Xml;
using System.Text;
using System.Globalization;

namespace AjaxPro
{
	/// <summary>
	/// Provides helper methods for JavaScript.
	/// </summary>
	public sealed class JavaScriptUtil
	{
		/// <summary>
		/// Get the client-side JavaScript namespace script.
		/// </summary>
		/// <param name="ns">The full JavaScript namespace as a string.</param>
		/// <returns>
		/// Returns the JavaScript code to create client-side namespaces.
		/// </returns>
		internal static string GetClientNamespaceRepresentation(string ns)
		{
			if (ns == null)
				return "";

			StringBuilder sb = new StringBuilder();

			string[] nsParts = ns.Split('.');

			if (nsParts.Length <= 1) return "";

			string _ns = nsParts[0];

			sb.Append("if(typeof " + _ns + " == \"undefined\") " + _ns + "={};\r\n");

			for (int i = 1; i < nsParts.Length; i++)
			{
				_ns += "." + nsParts[i];
				sb.Append("if(typeof " + _ns + " == \"undefined\") " + _ns + "={};\r\n");
			}

			return sb.ToString();
		}

		/// <summary>
		/// Quote the given string to be used in JSON.
		/// </summary>
		/// <param name="s">The string to quote.</param>
		/// <param name="quoteChar">The parameter you want to use for quoting.</param>
		/// <returns>Returns the quoted string.</returns>
		internal static string QuoteString(string s, char quoteChar)
		{
			StringBuilder sb = new StringBuilder();
			QuoteString(s, quoteChar, sb);
			return sb.ToString();
		}

		/// <summary>
		/// Quotes the string.
		/// </summary>
		/// <param name="s">The s.</param>
		/// <param name="quoteChar">The quote char.</param>
		/// <param name="sb">The sb.</param>
		internal static void QuoteString(string s, char quoteChar, StringBuilder sb)
		{
			if (s == null || (s.Length == 1 && s[0] == '\0'))
			{
				sb.Append(new String(quoteChar, 2));
				return;
			}

			char c;
			int len = s.Length;

			sb.EnsureCapacity(sb.Length + s.Length + 2);

			sb.Append(quoteChar);

			for (int i = 0; i < len; i++)
			{
				c = s[i];
				switch (c)
				{
					case '\\': sb.Append("\\\\"); break;
					case '\b': sb.Append("\\b"); break;
					case '\t': sb.Append("\\t"); break;
					case '\r': sb.Append("\\r"); break;
					case '\n': sb.Append("\\n"); break;
					case '\f': sb.Append("\\f"); break;
					default:
						if (c < ' ')
						{
							sb.Append("\\u");
							sb.Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));

						}
						else if (c == quoteChar)
						{
							sb.Append("\\");
							sb.Append(c);
						}
						else
						{
							sb.Append(c);
						}
						break;
				}
			}

			sb.Append(quoteChar);
		}

		/// <summary>
		/// Quote the given string to be used in JSON.
		/// </summary>
		/// <param name="s">The string to quote.</param>
		/// <returns>Returns the quoted string.</returns>
		public static string QuoteString(string s)
		{
			return QuoteString(s, '"');
		}

		/// <summary>
		/// Quotes the string.
		/// </summary>
		/// <param name="s">The s.</param>
		/// <param name="sb">The sb.</param>
		public static void QuoteString(string s, StringBuilder sb)
		{
			QuoteString(s, '"', sb);
		}

		/// <summary>
		/// Quote the given string to be used in HTML attributes.
		/// </summary>
		/// <param name="s">The string to quote.</param>
		/// <returns>Returns the quoted string.</returns>
		public static string QuoteHtmlAttribute(string s)
		{
			string r = QuoteString(s, '\'');
			r = r.Replace("\"", "&#34;");	// html attributes are ending with ", so we have to replace
			// it by the html entitiy &#34;
			return r;
		}
#if(!JSONLIB)
		/// <summary>
		/// Converts a enum type to a JavaScript representation.
		/// </summary>
		/// <param name="type">The type of the enum.</param>
		/// <returns>
		/// Returns a JavaScript that will add a local variable to the page.
		/// </returns>
		internal static string GetEnumRepresentation(Type type)
		{
			if (type.IsEnum == false)
				return "";

			StringBuilder sb = new StringBuilder();

			AjaxNamespaceAttribute[] ema = (AjaxNamespaceAttribute[])type.GetCustomAttributes(typeof(AjaxNamespaceAttribute), true);

			if (ema.Length > 0 && ema[0].ClientNamespace.Replace(".", "").Length > 0)
			{
				sb.Append(GetClientNamespaceRepresentation(ema[0].ClientNamespace));
				sb.Append(ema[0].ClientNamespace + ".");
				sb.Append(type.Name);
			}
			else
			{
				sb.Append(GetClientNamespaceRepresentation(type.FullName.IndexOf(".") > 0 ? type.FullName.Substring(0, type.FullName.LastIndexOf(".")) : type.FullName));
				sb.Append(type.FullName);
			}
			sb.Append(" = {\r\n");

			string[] names = Enum.GetNames(type);

			int c = 0;
			foreach (object o in Enum.GetValues(type))
			{
				sb.Append("\t\"");
				sb.Append(names[c]);
				sb.Append("\":");
				sb.Append(JavaScriptSerializer.Serialize(o));

				if (c < names.Length - 1)
					sb.Append(",\r\n");

				c++;
			}

			sb.Append("\r\n}");

			return sb.ToString();
		}
#endif

		#region JSON/XML serialization

		/// <summary>
		/// Converts an IJavaScriptObject to an XML document.
		/// </summary>
		/// <param name="o">The IJavaScript object to convert.</param>
		/// <returns>Returns an XML document.</returns>
		public static XmlDocument ConvertIJavaScriptObjectToXml(IJavaScriptObject o)
		{
			XmlDocument doc = new XmlDocument();
			doc.LoadXml("<root/>");

			SerializeToAjaxXml(doc.DocumentElement, o);

			return doc;
		}

		/// <summary>
		/// Serializes to ajax XML.
		/// </summary>
		/// <param name="n">The n.</param>
		/// <param name="o">The o.</param>
		internal static void SerializeToAjaxXml(XmlNode n, IJavaScriptObject o)
		{
			if (o is JavaScriptArray)
			{
				XmlElement p = n.OwnerDocument.CreateElement("array");

				foreach (IJavaScriptObject a in (JavaScriptArray)o)
					SerializeToAjaxXml(p, a);

				n.AppendChild(p);
			}
			else if (o is JavaScriptBoolean)
			{
				XmlElement p = n.OwnerDocument.CreateElement("boolean");
				p.InnerText = ((bool)(o as JavaScriptBoolean)) == true ? "true" : "false";

				n.AppendChild(p);
			}
			else if (o is JavaScriptNumber)
			{
				XmlElement p = n.OwnerDocument.CreateElement("number");
				p.InnerText = o.ToString();

				n.AppendChild(p);
			}
			else if (o is JavaScriptString)
			{
				XmlElement p = n.OwnerDocument.CreateElement("string");
				p.InnerText = o.ToString();

				n.AppendChild(p);
			}
			else if (o is JavaScriptObject)
			{
				XmlElement p = n.OwnerDocument.CreateElement("object");

				foreach (string key in ((JavaScriptObject)o).Keys)
				{
					XmlElement e = n.OwnerDocument.CreateElement("property");
					e.SetAttribute("name", key);

					p.AppendChild(e);

					SerializeToAjaxXml(e, (IJavaScriptObject)((JavaScriptObject)o)[key]);
				}

				n.AppendChild(p);
			}
		}

		/// <summary>
		/// Converts an Ajax.NET Professional XML JSON document to an IJavaScript object.
		/// Note: this is not a method to convert any XML document!
		/// </summary>
		/// <param name="doc">The XML JSON document.</param>
		/// <returns>Returns an IJavaScriptObject.</returns>
		public static IJavaScriptObject ConvertXmlToIJavaScriptObject(XmlDocument doc)
		{
			if (doc == null || doc.DocumentElement == null || doc.DocumentElement.ChildNodes.Count != 1)
				return null;

			return DeserialzeFromAjaxXml(doc.DocumentElement.ChildNodes[0]);
		}

		/// <summary>
		/// Deserialzes from ajax XML.
		/// </summary>
		/// <param name="n">The n.</param>
		/// <returns></returns>
		internal static IJavaScriptObject DeserialzeFromAjaxXml(XmlNode n)
		{
			switch (n.Name)
			{
				case "array":

					JavaScriptArray a = new JavaScriptArray();

					foreach (XmlNode item in n.ChildNodes)
						a.Add(DeserialzeFromAjaxXml(item));

					return a;

				case "boolean":

					JavaScriptBoolean b = new JavaScriptBoolean(n.InnerText == "true");
					return b;

				case "number":

					JavaScriptNumber i = new JavaScriptNumber();
					i.Append(n.InnerText);

					return i;

				case "string":

					JavaScriptString s = new JavaScriptString();
					s.Append(n.InnerText);

					return s;

				case "object":

					JavaScriptObject o = new JavaScriptObject();

					foreach (XmlNode p in n.SelectNodes("property"))
					{
						if (p.Attributes["name"] == null || p.ChildNodes.Count != 1)
							continue;

						o.Add(p.Attributes["name"].Value, DeserialzeFromAjaxXml(p.ChildNodes[0]));
					}

					return o;
			}

			return null;
		}

		#endregion

		#region XMLDocument serialization to IJavaScriptObject

		/// <summary>
		/// Converts an XML document to an IJavaScriptObject (JSON).
		/// <see cref="http://www.xml.com/pub/a/2006/05/31/converting-between-xml-and-json.html?page=1">Stefan Goessner</see>
		/// 	<see cref="http://developer.yahoo.com/common/json.html#xml">Yahoo XML JSON</see>
		/// </summary>
		/// <param name="n">The XmlNode to serialize to JSON.</param>
		/// <returns>A IJavaScriptObject.</returns>
		public static IJavaScriptObject GetIJavaScriptObjectFromXmlNode(XmlNode n)
		{
			if (n == null)
				return null;

			//if (xpath == "" || xpath == "/")
			//    xpath = n.Name;

			System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(@"\w+|\W+", System.Text.RegularExpressions.RegexOptions.Compiled);
			JavaScriptObject o = new JavaScriptObject();

			if (n.NodeType == XmlNodeType.Element)
			{
				for (int i = 0; i < n.Attributes.Count; i++)
				{
					o.Add("@" + n.Attributes[i].Name, new JavaScriptString(n.Attributes[i].Value));
				}

				if (n.FirstChild != null)	// element has child nodes
				{
					int textChild = 0;
					bool hasElementChild = false;

					for (XmlNode e = n.FirstChild; e != null; e = e.NextSibling)
					{
						if (e.NodeType == XmlNodeType.Element) hasElementChild = true;
						if (e.NodeType == XmlNodeType.Text && r.IsMatch(e.InnerText)) textChild++;	// non-whitespace text
					}

					if (hasElementChild)
					{
						if (textChild < 2)	// structured element with evtl. a single text node
						{
							for (XmlNode e = n.FirstChild; e != null; e = e.NextSibling)
							{
								if (e.NodeType == XmlNodeType.Text)
								{
									o.Add("#text", new JavaScriptString(e.InnerText));
								}
								else if (o.Contains(e.Name))
								{
									if (o[e.Name] is JavaScriptArray)
									{
										((JavaScriptArray)o[e.Name]).Add(GetIJavaScriptObjectFromXmlNode(e));
									}
									else
									{
										IJavaScriptObject _o = o[e.Name];
										JavaScriptArray a = new JavaScriptArray();
										a.Add(_o);
										a.Add(GetIJavaScriptObjectFromXmlNode(e));
										o[e.Name] = a;
									}
								}
								else
								{
									o.Add(e.Name, GetIJavaScriptObjectFromXmlNode(e));
								}
							}
						}
					}
					else if (textChild > 0)
					{
						if (n.Attributes.Count == 0)
							return new JavaScriptString(n.InnerText);
						else
							o.Add("#text", new JavaScriptString(n.InnerText));
					}
				}
				if (n.Attributes.Count == 0 && n.FirstChild == null)
					return new JavaScriptString(n.InnerText);
			}
			else if (n.NodeType == XmlNodeType.Document)
				return GetIJavaScriptObjectFromXmlNode(((XmlDocument)n).DocumentElement);
			else
				throw new NotSupportedException("Unhandled node type '" + n.NodeType + "'.");

			return o;
		}

		#endregion
	}
}