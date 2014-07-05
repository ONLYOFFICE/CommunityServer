/*
 * ExceptionConverter.cs
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
 * MS	06-09-24	use QuoteString instead of Serialize
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
	/// Provides methods to serialize and deserialize a Exception object.
	/// </summary>
	public class ExceptionConverter : IJavaScriptConverter
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionConverter"/> class.
        /// </summary>
		public ExceptionConverter()
			: base()
		{
			m_AllowInheritance = true;

			m_serializableTypes = new Type[] {
				typeof(Exception),
				typeof(NotImplementedException),
				typeof(NotSupportedException),
				typeof(NullReferenceException),
				typeof(System.Security.SecurityException)
			};
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
			Exception ex = (Exception)o;

			// The following line is NON-JSON format, it is used to 
			// return null to res.value and have an additional property res.error
			// in the object the callback JavaScript method will get.

			sb.Append("{\"Message\":");
			JavaScriptUtil.QuoteString(ex.Message, sb);
			sb.Append(",\"Type\":");
			JavaScriptUtil.QuoteString(o.GetType().FullName, sb);
#if(!JSONLIB)
			if (AjaxPro.Utility.Settings.DebugEnabled)
			{
				sb.Append(",\"Stack\":");
				JavaScriptUtil.QuoteString(ex.StackTrace, sb);

				if (ex.TargetSite != null)
				{
					sb.Append(",\"TargetSite\":");
					JavaScriptUtil.QuoteString(ex.TargetSite.ToString(), sb);
				}

				sb.Append(",\"Source\":");
				JavaScriptUtil.QuoteString(ex.Source, sb);
			}
#endif
			sb.Append("}");
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
			Exception ex = o as Exception;
			if (ex != null)
			{
				this.Serialize(ex, sb);
				return true;
			}

			return base.TrySerializeValue(o, t, sb);
		}
	}
}
