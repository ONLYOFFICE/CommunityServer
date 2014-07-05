#region Copyright © 2005 Paul Welter. All rights reserved.
/*
Copyright © 2005 Paul Welter. All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions
are met:

1. Redistributions of source code must retain the above copyright
   notice, this list of conditions and the following disclaimer.
2. Redistributions in binary form must reproduce the above copyright
   notice, this list of conditions and the following disclaimer in the
   documentation and/or other materials provided with the distribution.
3. The name of the author may not be used to endorse or promote products
   derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE AUTHOR "AS IS" AND ANY EXPRESS OR
IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE. 
*/
#endregion



using System;
using System.Globalization;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace MSBuild.Community.Tasks
{
	/// <summary>
	/// Gets the current date and time.
	/// </summary>
	/// <remarks>
	/// See
	/// <a href="ms-help://MS.VSCC.v80/MS.MSDN.v80/MS.NETDEVFX.v20.en/cpref8/html/T_System_Globalization_DateTimeFormatInfo.htm">
	/// DateTimeFormatInfo</a>
	/// for the syntax of the format string.
	/// </remarks>
	/// <example>Using the Time task to get the Month, Day,
	/// Year, Hour, Minute, and Second:
	/// <code><![CDATA[
	/// <Time>
	///   <Output TaskParameter="Month" PropertyName="Month" />
	///   <Output TaskParameter="Day" PropertyName="Day" />
	///   <Output TaskParameter="Year" PropertyName="Year" />
	///   <Output TaskParameter="Hour" PropertyName="Hour" />
	///	  <Output TaskParameter="Minute" PropertyName="Minute" />
	///	  <Output TaskParameter="Second" PropertyName="Second" />
	///	</Time>
	///	<Message Text="Current Date and Time: $(Month)/$(Day)/$(Year) $(Hour):$(Minute):$(Second)" />]]></code>
	/// Set property "BuildDate" to the current date and time:
	/// <code><![CDATA[
	/// <Time Format="yyyyMMddHHmmss">
	///     <Output TaskParameter="FormattedTime" PropertyName="buildDate" />
	/// </Time>]]></code>
	/// </example>
	public class Time : Task
	{
		#region Fields

		private System.DateTime mDate;
		private System.DateTimeKind dateTimeKind = DateTimeKind.Local;
		private string format;

		private string mMonth;
		private string mDay;
		private string mYear;
		private string mHour;
		private string mMinute;
		private string mSecond;
		private string mMillisecond;
		private string mTicks;
		private string mKind;
		private string mTimeOfDay;
		private string mDayOfYear;
		private string mDayOfWeek;
		private string mFormattedTime;
		private string mShortDate;
		private string mLongDate;
		private string mShortTime;
		private string mLongTime;

		#endregion

		#region Input Parameters
		/// <summary>
		/// Gets or sets the format string
		/// for output parameter <see cref="FormattedTime"/>.
		/// </summary>
		/// <remarks>
		/// See
		/// <a href="ms-help://MS.VSCC.v80/MS.MSDN.v80/MS.NETDEVFX.v20.en/cpref8/html/T_System_Globalization_DateTimeFormatInfo.htm">
		/// DateTimeFormatInfo</a>
		/// for the syntax of the format string.
		/// </remarks>
		public string Format
		{
			get { return format; }
			set { format = value; }
		}

		#endregion Input Parameters

		#region Output Parameters

		/// <summary>
		/// Gets the month component of the date represented by this instance.
		/// </summary>
		[Output]
		public string Month
		{
			get { return mMonth; }
		}

		/// <summary>
		/// Gets the day of the month represented by this instance.
		/// </summary>
		[Output]
		public string Day
		{
			get { return mDay; }
		}

		/// <summary>
		/// Gets the year component of the date represented by this instance.
		/// </summary>
		[Output]
		public string Year
		{
			get { return mYear; }
		}

		/// <summary>
		/// Gets the hour component of the date represented by this instance.
		/// </summary>
		[Output]
		public string Hour
		{
			get { return mHour; }
		}

		/// <summary>
		/// Gets the minute component of the date represented by this instance.
		/// </summary>
		[Output]
		public string Minute
		{
			get { return mMinute; }
		}

		/// <summary>
		/// Gets the seconds component of the date represented by this instance.
		/// </summary>
		[Output]
		public string Second
		{
			get { return mSecond; }
		}

		/// <summary>
		/// Gets the milliseconds component of the date represented by this instance.
		/// </summary>
		[Output]
		public string Millisecond
		{
			get { return mMillisecond; }
		}

		/// <summary>
		/// Gets the number of ticks that represent the date and time of this instance.
		/// </summary>
		[Output]
		public string Ticks
		{
			get { return mTicks; }
		}

		/// <summary>
		/// Gets or sets a value that indicates whether the time represented by this instance is based
		/// on local time, Coordinated Universal Time (UTC), or neither.
		/// </summary>
		/// <remarks>
		/// Possible values are:
		/// <list type="ul">
		/// <item>Local (default)</item>,
		/// <item>Utc</item>,
		/// <item>Unspecified</item>
		/// </list>
		/// </remarks>
        /// <enum cref="DateTimeKind"/>
		[Output]
		public string Kind
		{
			get { return mKind; }
			set
			{
				dateTimeKind = (DateTimeKind)Enum.Parse(typeof(DateTimeKind), value);
			}
		}

		/// <summary>
		/// Gets the time of day for this instance.
		/// </summary>
		[Output]
		public string TimeOfDay
		{
			get { return mTimeOfDay; }
		}

		/// <summary>
		/// Gets the day of the year represented by this instance.
		/// </summary>
		[Output]
		public string DayOfYear
		{
			get { return mDayOfYear; }
		}

		/// <summary>
		/// Gets the day of the week represented by this instance.
		/// </summary>
		[Output]
		public string DayOfWeek
		{
			get { return mDayOfWeek; }
		}

		/// <summary>
		/// Gets the value of this instance to its equivalent string representation.
		/// If input parameter <see cref="Format"/> is provided,
		/// the value is formatted according to it.
		/// </summary>
		[Output]
		public string FormattedTime
		{
			get { return mFormattedTime; }
		}

		/// <summary>
		/// Gets the value of this instance to its equivalent short date string representation.
		/// </summary>
		[Output]
		public string ShortDate
		{
			get { return mShortDate; }
		}

		/// <summary>
		/// Gets the value of this instance to its equivalent long date string representation.
		/// </summary>
		[Output]
		public string LongDate
		{
			get { return mLongDate; }
		}

		/// <summary>
		/// Gets the value of this instance to its equivalent short time string representation.
		/// </summary>
		[Output]
		public string ShortTime
		{
			get { return mShortTime; }
		}

		/// <summary>
		/// Gets the value of this instance to its equivalent long time string representation.
		/// </summary>
		[Output]
		public string LongTime
		{
			get { return mLongTime; }
		}

		#endregion Output Parameters

		#region Public Properties

		/// <summary>
		/// Gets the internal time value.
		/// </summary>
		public DateTime DateTimeValue
		{
			get { return mDate; }
		}

		#endregion Public Properties

		#region Task Overrides
		/// <summary>
		/// When overridden in a derived class, executes the task.
		/// </summary>
		/// <returns>
		/// True if the task successfully executed; otherwise, false.
		/// </returns>
		public override bool Execute()
		{
			bool bSuccess = true;

			try
			{
				Log.LogMessage(MessageImportance.Low, Properties.Resources.TimeGettingCurrentDate);
				GetDate();
			}
			catch (FormatException ex)
			{
				// Log failure
				Log.LogErrorFromException(ex);
				Log.LogMessage(MessageImportance.High, Properties.Resources.TimeFormatException);
				bSuccess = false;
			}

			return bSuccess;
		}

		#endregion Task Overrides

		#region Private Methods

		private void GetDate()
		{
			if (dateTimeKind == DateTimeKind.Utc)
			{
				mDate = System.DateTime.UtcNow;
			}
			else
			{
				mDate = System.DateTime.Now;
			}

			mMonth = mDate.Month.ToString(DateTimeFormatInfo.InvariantInfo);
			mDay = mDate.Day.ToString(DateTimeFormatInfo.InvariantInfo);
			mYear = mDate.Year.ToString(DateTimeFormatInfo.InvariantInfo);
			mHour = mDate.Hour.ToString(DateTimeFormatInfo.InvariantInfo);
			mMinute = mDate.Minute.ToString(DateTimeFormatInfo.InvariantInfo);
			mSecond = mDate.Second.ToString(DateTimeFormatInfo.InvariantInfo);
			mMillisecond = mDate.Millisecond.ToString(DateTimeFormatInfo.InvariantInfo);
			mTicks = mDate.Ticks.ToString(DateTimeFormatInfo.InvariantInfo);
			mKind = mDate.Kind.ToString();
			mTimeOfDay = mDate.TimeOfDay.ToString();
			mDayOfYear = mDate.DayOfYear.ToString(DateTimeFormatInfo.InvariantInfo);
			mDayOfWeek = mDate.DayOfWeek.ToString();
			mShortDate = mDate.ToShortDateString();
			mLongDate = mDate.ToLongDateString();
			mShortTime = mDate.ToShortTimeString();
			mLongTime = mDate.ToLongTimeString();

			if (format == null)
			{
				mFormattedTime = mDate.ToString(DateTimeFormatInfo.InvariantInfo);
			}
			else
			{
				mFormattedTime = mDate.ToString(format, DateTimeFormatInfo.InvariantInfo);
			}
		}

		#endregion Private Methods

	}
}
