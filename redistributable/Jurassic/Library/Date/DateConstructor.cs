using System;
using System.Collections.Generic;

namespace Jurassic.Library
{
    /// <summary>
    /// Represents the built-in javascript Date object.
    /// </summary>
    [Serializable]
    public class DateConstructor : ClrFunction
    {
        //     INITIALIZATION
        //_________________________________________________________________________________________


        /// <summary>
        /// Creates a new Date object.
        /// </summary>
        /// <param name="prototype"> The next object in the prototype chain. </param>
        internal DateConstructor(ObjectInstance prototype)
            : base(prototype, "Date", new DateInstance(prototype.Engine.Object.InstancePrototype, double.NaN))
        {
            this.Length = 7;
        }



        //     JAVASCRIPT INTERNAL FUNCTIONS
        //_________________________________________________________________________________________


        /// <summary>
        /// Called when the Date object is invoked like a function, e.g. var x = Date().
        /// Returns a string representing the current time.
        /// </summary>
        [JSCallFunction]
        public string Call()
        {
            return new DateInstance(this.InstancePrototype).ToStringJS();
        }

        /// <summary>
        /// Creates a new Date object and sets its value to the current time.
        /// </summary>
        public DateInstance Construct()
        {
            return new DateInstance(this.InstancePrototype);
        }

        /// <summary>
        /// Creates a new Date object from a millisecond value.
        /// </summary>
        /// <param name="milliseconds"> The number of milliseconds since 1 January 1970 00:00:00 UTC. </param>
        public DateInstance Construct(double milliseconds)
        {
            return new DateInstance(this.InstancePrototype, milliseconds);
        }

        /// <summary>
        /// Creates a new Date object from a string.
        /// </summary>
        /// <param name="dateStr"> A string representing a date, expressed in RFC 1123 format. </param>
        public DateInstance Construct(string dateStr)
        {
            return new DateInstance(this.InstancePrototype, dateStr);
        }

        /// <summary>
        /// Creates a new Date object from various date components, expressed in local time.
        /// </summary>
        /// <param name="year"> The full year. </param>
        /// <param name="month"> The month as an integer between 0 and 11 (january to december). </param>
        /// <param name="day"> The day of the month, from 1 to 31.  Defaults to 1. </param>
        /// <param name="hour"> The number of hours since midnight, from 0 to 23.  Defaults to 0. </param>
        /// <param name="minute"> The number of minutes, from 0 to 59.  Defaults to 0. </param>
        /// <param name="second"> The number of seconds, from 0 to 59.  Defaults to 0. </param>
        /// <param name="millisecond"> The number of milliseconds, from 0 to 999.  Defaults to 0. </param>
        /// <remarks>
        /// If any of the parameters are out of range, then the other values are modified accordingly.
        /// </remarks>
        public DateInstance Construct(int year, int month, [DefaultParameterValue(1)] int day = 1, [DefaultParameterValue(0)] int hour = 0,
            [DefaultParameterValue(0)] int minute = 0, [DefaultParameterValue(0)] int second = 0, [DefaultParameterValue(0)] int millisecond = 0)
        {
            return new DateInstance(this.InstancePrototype, year, month, day, hour, minute, second, millisecond);
        }

        /// <summary>
        /// Creates a new Date object from various date components, expressed in local time.
        /// </summary>
        /// <param name="year"> The full year. </param>
        /// <param name="month"> The month as an integer between 0 and 11 (january to december). </param>
        /// <param name="day"> The day of the month, from 1 to 31.  Defaults to 1. </param>
        /// <param name="hour"> The number of hours since midnight, from 0 to 23.  Defaults to 0. </param>
        /// <param name="minute"> The number of minutes, from 0 to 59.  Defaults to 0. </param>
        /// <param name="second"> The number of seconds, from 0 to 59.  Defaults to 0. </param>
        /// <param name="millisecond"> The number of milliseconds, from 0 to 999.  Defaults to 0. </param>
        /// <remarks>
        /// If any of the parameters are out of range, then the other values are modified accordingly.
        /// </remarks>
        [JSConstructorFunction]
        public DateInstance Construct(params object[] components)
        {
            if (components.Length == 0)
                return Construct();
            if (components.Length == 1)
            {
                if (TypeUtilities.IsString(components[0]) == true)
                    return Construct(TypeConverter.ToString(components[0]));
                return Construct(TypeConverter.ToNumber(components[0]));
            }

            // If any of the parameters are out of range, the date is undefined.
            for (int i = 0; i < Math.Min(components.Length, 7); i++)
            {
                double component = TypeConverter.ToNumber(components[i]);
                if (component < int.MinValue || component > int.MaxValue || double.IsNaN(component) == true)
                    return Construct(double.NaN);
            }
            int year = TypeConverter.ToInteger(components[0]);
            int month = TypeConverter.ToInteger(components[1]);
            int day = components.Length >= 3 ? TypeConverter.ToInteger(components[2]) : 1;
            int hour = components.Length >= 4 ? TypeConverter.ToInteger(components[3]) : 0;
            int minute = components.Length >= 5 ? TypeConverter.ToInteger(components[4]) : 0;
            int second = components.Length >= 6 ? TypeConverter.ToInteger(components[5]) : 0;
            int millisecond = components.Length >= 7 ? TypeConverter.ToInteger(components[6]) : 0;
            return new DateInstance(this.InstancePrototype, year, month, day, hour, minute, second, millisecond);
        }




        //     JAVASCRIPT FUNCTIONS
        //_________________________________________________________________________________________


        /// <summary>
        /// Returns the current date and time as the number of milliseconds elapsed since January 1,
        /// 1970, 00:00:00 UTC.
        /// </summary>
        /// <returns> The current date and time as the number of milliseconds elapsed since January 1,
        /// 1970, 00:00:00 UTC. </returns>
        [JSInternalFunction(Name = "now")]
        public static double Now()
        {
            return DateInstance.Now();
        }

        /// <summary>
        /// Given the components of a UTC date, returns the number of milliseconds since January 1,
        /// 1970, 00:00:00 UTC to that date.
        /// </summary>
        /// <param name="year"> The full year. </param>
        /// <param name="month"> The month as an integer between 0 and 11 (january to december). </param>
        /// <param name="day"> The day of the month, from 1 to 31.  Defaults to 1. </param>
        /// <param name="hour"> The number of hours since midnight, from 0 to 23.  Defaults to 0. </param>
        /// <param name="minute"> The number of minutes, from 0 to 59.  Defaults to 0. </param>
        /// <param name="second"> The number of seconds, from 0 to 59.  Defaults to 0. </param>
        /// <param name="millisecond"> The number of milliseconds, from 0 to 999.  Defaults to 0. </param>
        /// <returns> The number of milliseconds since January 1, 1970, 00:00:00 UTC to the given
        /// date. </returns>
        /// <remarks>
        /// This method differs from the Date constructor in two ways:
        /// 1. The date components are specified in UTC time rather than local time.
        /// 2. A number is returned instead of a Date instance.
        /// 
        /// If any of the parameters are out of range, then the other values are modified accordingly.
        /// </remarks>
        [JSInternalFunction(Name = "UTC")]
        public static double UTC(int year, int month, [DefaultParameterValue(1)] int day = 1, [DefaultParameterValue(0)] int hour = 0,
            [DefaultParameterValue(0)] int minute = 0, [DefaultParameterValue(0)] int second = 0, [DefaultParameterValue(0)] int millisecond = 0)
        {
            return DateInstance.UTC(year, month, day, hour, minute, second, millisecond);
        }

        /// <summary>
        /// Parses a string representation of a date, and returns the number of milliseconds since
        /// January 1, 1970, 00:00:00 UTC.
        /// </summary>
        /// <param name="dateStr"> A string representing a date, expressed in RFC 1123 format. </param>
        [JSInternalFunction(Name = "parse")]
        public static double Parse(string dateStr)
        {
            return DateInstance.Parse(dateStr);
        }
    }
}
