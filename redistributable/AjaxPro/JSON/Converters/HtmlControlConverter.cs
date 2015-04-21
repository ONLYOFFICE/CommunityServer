/*
 * HtmlControlConverter.cs
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
 * MS	06-09-26	improved performance using StringBuilder
 * 
 * 
 */
using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace AjaxPro
{
	/// <summary>
	/// Provides methods to serialize and deserialize an object that is inherited from HtmlControl.
	/// </summary>
	public class HtmlControlConverter : IJavaScriptConverter
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlControlConverter"/> class.
        /// </summary>
		public HtmlControlConverter() : base()
		{
			m_serializableTypes = new Type[]
				{
					typeof(HtmlControl),
					typeof(HtmlAnchor),
					typeof(HtmlButton),
					typeof(HtmlImage),
					typeof(HtmlInputButton),
					typeof(HtmlInputCheckBox),
					typeof(HtmlInputRadioButton),
					typeof(HtmlInputText),
					typeof(HtmlSelect),
					typeof(HtmlTableCell),
					typeof(HtmlTable),
					typeof(HtmlTableRow),
					typeof(HtmlTextArea)
				};
			m_deserializableTypes = new Type[]
				{
					typeof(HtmlControl),
					typeof(HtmlAnchor),
					typeof(HtmlButton),
					typeof(HtmlImage),
					typeof(HtmlInputButton),
					typeof(HtmlInputCheckBox),
					typeof(HtmlInputRadioButton),
					typeof(HtmlInputText),
					typeof(HtmlSelect),
					typeof(HtmlTableCell),
					typeof(HtmlTable),
					typeof(HtmlTableRow),
					typeof(HtmlTextArea)
				};
		}

        /// <summary>
        /// Converts an IJavaScriptObject into an NET object.
        /// </summary>
        /// <param name="o">The IJavaScriptObject object to convert.</param>
        /// <param name="t"></param>
        /// <returns>Returns a .NET object.</returns>
		public override object Deserialize(IJavaScriptObject o, Type t)
		{
			if(!typeof(HtmlControl).IsAssignableFrom(t) || !(o is JavaScriptString))
				throw new NotSupportedException();

			return HtmlControlFromString(o.ToString(), t);
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
			if(!(o is Control))
				throw new NotSupportedException();

			sb.Append(HtmlControlToString((HtmlControl)o));
		}

		#region Internal Methods

        /// <summary>
        /// Corrects the attributes.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
		internal static string CorrectAttributes(string input)
		{
			string s = @"selected=""selected""";
			Regex r = new Regex(s, RegexOptions.Singleline | RegexOptions.IgnoreCase);
			input =  r.Replace(input, @"selected=""true""");

			s = @"multiple=""multiple""";
			r = new Regex(s, RegexOptions.Singleline | RegexOptions.IgnoreCase);
			input =  r.Replace(input, @"multiple=""true""");

			s = @"disabled=""disabled""";
			r = new Regex(s, RegexOptions.Singleline | RegexOptions.IgnoreCase);
			input =  r.Replace(input, @"disabled=""true""");

			return input;
		}

        /// <summary>
        /// HTMLs the control to string.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <returns></returns>
		internal static string HtmlControlToString(HtmlControl control)
		{
			StringWriter writer = new StringWriter(new StringBuilder());
			
			control.RenderControl(new HtmlTextWriter(writer));

			return JavaScriptSerializer.Serialize(writer.ToString());    
		}

        /// <summary>
        /// HTMLs the control from string.
        /// </summary>
        /// <param name="html">The HTML.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
		internal static HtmlControl HtmlControlFromString(string html, Type type)
		{
			if(!typeof(HtmlControl).IsAssignableFrom(type))
				throw new InvalidCastException("The target type is not a HtmlControlType");

			html = AddRunAtServer(html, (Activator.CreateInstance(type) as HtmlControl).TagName);

			if(type.IsAssignableFrom(typeof(HtmlSelect)))
				html = CorrectAttributes(html);

			Control o = HtmlControlConverterHelper.Parse(html);;
			
			if(o.GetType() == type)
				return (o as HtmlControl);
			else
			{
				foreach(Control con in o.Controls)
				{
					if(con.GetType() == type)
					{
						return (con as HtmlControl);
					}
				}
			}

			return null;
		}

        /// <summary>
        /// Adds the run at server.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="tagName">Name of the tag.</param>
        /// <returns></returns>
		internal static string AddRunAtServer(string input, string tagName)
		{
			// <select[^>]*?(?<InsertPos>\s*)>
			string pattern = "<" + Regex.Escape(tagName) + @"[^>]*?(?<InsertPos>\s*)/?>";
			Regex regEx = new Regex(pattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);
			Match match = regEx.Match(input);

			if (match.Success)
			{
				Group group = match.Groups["InsertPos"];
				return input.Insert(group.Index + group.Length, " runat=\"server\"");
			}
			else
				return input;
		}

		#endregion
	}

	internal class HtmlControlConverterHelper : TemplateControl
	{
        /// <summary>
        /// Parses the specified control string.
        /// </summary>
        /// <param name="controlString">The control string.</param>
        /// <returns></returns>
		internal static Control Parse(string controlString)
		{
			HtmlControlConverterHelper control = new HtmlControlConverterHelper();
#if(NET20)
			control.AppRelativeVirtualPath = "~";
#endif

			return control.ParseControl(controlString);
		}
	}
}
