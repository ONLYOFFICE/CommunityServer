/*
 * DecimalConverter.cs
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
 * MS	06-09-26	improved performance using StringBuilder
 * 
 * 
 * 
 */
using System;
using System.Text;
using System.Data;

namespace AjaxPro
{
	/// <summary>
	/// Provides methods to serialize and deserialize a Decimal object.
	/// </summary>
	public class DecimalConverter : IJavaScriptConverter
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="DecimalConverter"/> class.
        /// </summary>
		public DecimalConverter()
			: base()
		{
			m_serializableTypes = new Type[] {
				typeof(Decimal)
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
			// this.SerializeValue(Decimal.GetBits(d), sb);

			// The following code is not correct, but while JavaScript cannot
			// handle the decimal value correct we are using double instead.

			JavaScriptSerializer.Serialize((Double)((Decimal)o), sb);
		}

        /// <summary>
        /// Converts an IJavaScriptObject into an NET object.
        /// </summary>
        /// <param name="o">The IJavaScriptObject object to convert.</param>
        /// <param name="t"></param>
        /// <returns>Returns a .NET object.</returns>
		public override object Deserialize(IJavaScriptObject o, Type t)
		{
			JavaScriptNumber n = o as JavaScriptNumber;

			if (n == null)
				throw new NotSupportedException();

			string s =n.Value;

			// TODO: return the default value for this primitive data type
			s = s.Replace(".", System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator);

			Decimal d = Decimal.Parse(s,
				System.Globalization.NumberStyles.Currency |
				System.Globalization.NumberStyles.AllowExponent);

			return d;
		}
	}
}
