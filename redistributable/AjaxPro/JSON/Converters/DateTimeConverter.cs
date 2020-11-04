/*
 * DateTimeConverter.cs
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
 * MS   06-07-09    added new Date and new Date(Date.UTC parsing
 * MS	06-09-22	added UniversalSortableDateTimePattern parsing
 *					added new oldStyle/renderDateTimeAsString configruation to enable string output of DateTime
 *					changed JSONLIB stand-alone library will return DateTimes as UniversalSortableDateTimePattern
 * MS	06-09-26	improved performance using StringBuilder
 * MS	06-09-29	added new oldStyle/noUtcTime configuration
 *					fixed using rednerDateTimeAsString serialization
 * MS	08-03-21	using ASP.NET AJAX format for dates (new default)
 * 
 */
using System;
using System.Text;
using System.Text.RegularExpressions;

namespace AjaxPro
{
	/// <summary>
	/// Provides methods to serialize and deserialize a DateTime object.
	/// </summary>
	public class DateTimeConverter : IJavaScriptConverter
	{
		private Regex r = new Regex(@"(\d{4}),(\d{1,2}),(\d{1,2}),(\d{1,2}),(\d{1,2}),(\d{1,2}),(\d{1,3})", RegexOptions.Compiled);
		private double UtcOffsetMinutes = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).TotalMinutes;

		/// <summary>
		/// Initializes a new instance of the <see cref="DateTimeConverter"/> class.
		/// </summary>
		public DateTimeConverter()
			: base()
		{
			m_serializableTypes = new Type[] { typeof(DateTime) };
			m_deserializableTypes = new Type[] { typeof(DateTime) };
		}

		/// <summary>
		/// Converts an IJavaScriptObject into an NET object.
		/// </summary>
		/// <param name="o">The IJavaScriptObject object to convert.</param>
		/// <param name="t"></param>
		/// <returns>Returns a .NET object.</returns>
		public override object Deserialize(IJavaScriptObject o, Type t)
		{
			JavaScriptObject ht = o as JavaScriptObject;

			if (o is JavaScriptSource)
			{
				// new Date(Date.UTC(2006,6,9,5,36,18,875))

				string s = o.ToString();

				if (s.StartsWith("new Date(Date.UTC(") && s.EndsWith("))"))
				{
					s = s.Substring(18, s.Length - 20);

					//Regex r = new Regex(@"(\d{4}),(\d{1,2}),(\d{1,2}),(\d{1,2}),(\d{1,2}),(\d{1,2}),(\d{1,3})", RegexOptions.Compiled);
					//Match m = r.Match(s);

					//if (m.Groups.Count != 8)
					//    throw new NotSupportedException();

					//int Year = int.Parse(m.Groups[1].Value);
					//int Month = int.Parse(m.Groups[2].Value) + 1;
					//int Day = int.Parse(m.Groups[3].Value);
					//int Hour = int.Parse(m.Groups[4].Value);
					//int Minute = int.Parse(m.Groups[5].Value);
					//int Second = int.Parse(m.Groups[6].Value);
					//int Millisecond = int.Parse(m.Groups[7].Value);

					//DateTime d = new DateTime(Year, Month, Day, Hour, Minute, Second, Millisecond);

					string[] p = s.Split(',');
					return new DateTime(int.Parse(p[0]), int.Parse(p[1]) + 1, int.Parse(p[2]), int.Parse(p[3]), int.Parse(p[4]), int.Parse(p[5]), int.Parse(p[6])).AddMinutes(UtcOffsetMinutes);
				}
				else if (s.StartsWith("new Date(") && s.EndsWith(")"))
				{
					long nanosecs = long.Parse(s.Substring(9, s.Length - 10)) * 10000;
#if(NET20)
					nanosecs += new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).Ticks;
					DateTime d1 = new DateTime(nanosecs, DateTimeKind.Utc);
#else
                    nanosecs += new DateTime(1970, 1, 1, 0, 0, 0, 0).Ticks;
                    DateTime d1 = new DateTime(nanosecs);
#endif

					return (Utility.Settings.OldStyle.Contains("noUtcTime") ? d1 : d1.AddMinutes(UtcOffsetMinutes)); // TimeZone.CurrentTimeZone.GetUtcOffset(d1).TotalMinutes);
				}
			}
			else if (o is JavaScriptString)
			{
				string d = o.ToString();

#if(NET20)
				if (d.StartsWith("/Date(") && d.EndsWith(")/"))
				// if (d.Length >= 9 && d.Substring(0, 6) == "/Date(" && d.Substring(d.Length -2) == ")/")
				{
					DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, new System.Globalization.GregorianCalendar(), System.DateTimeKind.Utc);

					return new DateTime(
							long.Parse(d.Substring(6, d.Length - 6 - 2)) * TimeSpan.TicksPerMillisecond, DateTimeKind.Utc).AddTicks(Epoch.Ticks);
				}

				DateTime d2;

				if (DateTime.TryParseExact(o.ToString(),
					System.Globalization.DateTimeFormatInfo.InvariantInfo.UniversalSortableDateTimePattern,
					System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AllowWhiteSpaces, out d2
					) == true)
				{
					return (Utility.Settings.OldStyle.Contains("noUtcTime") ? d2 : d2.AddMinutes(UtcOffsetMinutes)); // TimeZone.CurrentTimeZone.GetUtcOffset(d2).TotalMinutes);
				}
#else
				try
				{
					DateTime d4 = DateTime.ParseExact(o.ToString(),
						System.Globalization.DateTimeFormatInfo.InvariantInfo.UniversalSortableDateTimePattern,
						System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AllowWhiteSpaces
					);

					return (Utility.Settings.OldStyle.Contains("noUtcTime") ? d4 : d4.AddMinutes(UtcOffsetMinutes)); // TimeZone.CurrentTimeZone.GetUtcOffset(d4).TotalMinutes);
				}
				catch(FormatException)
				{
				}
#endif
			}


			if (ht == null)
				throw new NotSupportedException();

			int Year2 = (int)JavaScriptDeserializer.Deserialize(ht["Year"], typeof(int));
			int Month2 = (int)JavaScriptDeserializer.Deserialize(ht["Month"], typeof(int));
			int Day2 = (int)JavaScriptDeserializer.Deserialize(ht["Day"], typeof(int));
			int Hour2 = (int)JavaScriptDeserializer.Deserialize(ht["Hour"], typeof(int));
			int Minute2 = (int)JavaScriptDeserializer.Deserialize(ht["Minute"], typeof(int));
			int Second2 = (int)JavaScriptDeserializer.Deserialize(ht["Second"], typeof(int));
			int Millisecond2 = (int)JavaScriptDeserializer.Deserialize(ht["Millisecond"], typeof(int));

			DateTime d5 = new DateTime(Year2, Month2, Day2, Hour2, Minute2, Second2, Millisecond2);
			return (Utility.Settings.OldStyle.Contains("noUtcTime") ? d5 : d5.AddMinutes(UtcOffsetMinutes)); // TimeZone.CurrentTimeZone.GetUtcOffset(d3).TotalMinutes);
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
			if (!(o is DateTime))
				throw new NotSupportedException();

			DateTime dt = (DateTime)o;

			bool noUtcTime = Utility.Settings.OldStyle.Contains("noUtcTime");
			if (!noUtcTime)
				dt = dt.ToUniversalTime();

#if(NET20)
			if (!AjaxPro.Utility.Settings.OldStyle.Contains("renderNotASPAJAXDateTime"))
			{
				DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, new System.Globalization.GregorianCalendar(), System.DateTimeKind.Utc);

				sb.Append("\"\\/Date(" + ((dt.Ticks - Epoch.Ticks) / TimeSpan.TicksPerMillisecond) + ")\\/\"");
				return;
			}
#endif


#if(JSONLIB)
			JavaScriptUtil.QuoteString(dt.ToString(System.Globalization.DateTimeFormatInfo.InvariantInfo.UniversalSortableDateTimePattern), sb);
#else



			if (AjaxPro.Utility.Settings.OldStyle.Contains("renderDateTimeAsString"))
			{
				JavaScriptUtil.QuoteString(dt.ToString(System.Globalization.DateTimeFormatInfo.InvariantInfo.UniversalSortableDateTimePattern), sb);
				return;
			}

			if (!noUtcTime)
				sb.AppendFormat("new Date(Date.UTC({0},{1},{2},{3},{4},{5},{6}))",
					dt.Year,
					dt.Month - 1,
					dt.Day,
					dt.Hour,
					dt.Minute,
					dt.Second,
					dt.Millisecond);
			else
				sb.AppendFormat("new Date({0},{1},{2},{3},{4},{5},{6})",
					dt.Year,
					dt.Month - 1,
					dt.Day,
					dt.Hour,
					dt.Minute,
					dt.Second,
					dt.Millisecond);
#endif
		}
	}
}