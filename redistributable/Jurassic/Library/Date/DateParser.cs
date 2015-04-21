using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Jurassic.Library
{
    /// <summary>
    /// Parses a javascript date string.
    /// </summary>
    public static class DateParser
    {
        /// <summary>
        /// Parses a javascript date string.
        /// </summary>
        /// <param name="input"> The string to parse as a date. </param>
        /// <returns> A date. </returns>
        internal static DateTime Parse(string input)
        {
            /* Regex tested using http://derekslager.com/blog/posts/2007/09/a-better-dotnet-regular-expression-tester.ashx
             * These should succeed:
             * 2010
             * 2010-03
             * 2010-02-07
             * 2010T12:34
             * 2010-02T12:34:56
             * 2010-02-07T12:34:56.012
             * 2010T12:34Z
             * 2010-02T12:34:56Z
             * 2010-02-07T12:34:56.012Z
             * 2010T12:34+09:00
             * 2010-02T12:34:56+09:00
             * 2010-02-07T12:34:56.012-09:00
             * 2010-02-05T12:34:56.012
             * 
             * And these should fail:
             * 201
             * 2010-1
             * T12:34
             * 12:34
             * 2010-02T1:34:56
             * 2010-02T12:3:56
             * 2010-02T12:53:1
             * 2010-02T12:53:12.1
             * 2010-02T12:53:12.12
             */

            var regex = new Regex(
                @"^(  (?<year> [0-9]{4} )
                   (- (?<month> [0-9]{2} )
                   (- (?<day> [0-9]{2} ))?)?)
                   (T (?<hour> [0-9]{2} )
                    : (?<minute> [0-9]{2} )
                   (: (?<second> [0-9]{2} )
                  (\. (?<millisecond> [0-9]{3} ))?)?
                      (?<zone> Z | (?<zoneHours> [+-][0-9]{2} ) : (?<zoneMinutes> [0-9]{2} ) )?)?$",
                RegexOptions.ExplicitCapture | RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace);
            
            // Match the regex.
            var match = regex.Match(input);
            if (match.Success == true)
            {
                // Get the group values.
                int year, month, day, hour, minute, second, millisecond, offsetInMinutes = 0;
                if (int.TryParse(match.Groups["year"].Value, out year) == false)
                    year = 1970;
                if (int.TryParse(match.Groups["month"].Value, out month) == false)
                    month = 1;
                if (int.TryParse(match.Groups["day"].Value, out day) == false)
                    day = 1;
                int.TryParse(match.Groups["hour"].Value, out hour);
                int.TryParse(match.Groups["minute"].Value, out minute);
                int.TryParse(match.Groups["second"].Value, out second);
                int.TryParse(match.Groups["millisecond"].Value, out millisecond);

                // Validate the components.
                if (month < 1 || month > 12)
                    return DateTime.MinValue;
                if (day < 1 || day > 31)
                    return DateTime.MinValue;
                if (hour > 24 || (hour == 24 && (minute > 0 || second > 0 || millisecond > 0)))
                    return DateTime.MinValue;
                if (minute >= 60)
                    return DateTime.MinValue;
                if (second >= 60)
                    return DateTime.MinValue;

                // 24:00 is valid according to the spec.
                if (hour == 24)
                {
                    hour = 0;
                    offsetInMinutes += 24 * 60;
                }

                // Parse the zone information (the default is UTC).
                if (match.Groups["zone"].Value != string.Empty && match.Groups["zone"].Value != "Z")
                {
                    // Parse the numeric values.
                    int zoneHours, zoneMinutes;
                    if (int.TryParse(match.Groups["zoneHours"].Value, out zoneHours) == false)
                        return DateTime.MinValue;
                    if (int.TryParse(match.Groups["zoneMinutes"].Value, out zoneMinutes) == false)
                        return DateTime.MinValue;

                    // Validate the components.
                    if (zoneHours >= 24)
                        return DateTime.MinValue;
                    if (zoneMinutes >= 60)
                        return DateTime.MinValue;

                    // Calculate the zone offset, in minutes.
                    offsetInMinutes -= zoneHours < 0 ? zoneHours * 60 - zoneMinutes : zoneHours * 60 + zoneMinutes;
                }

                // Create a date from the components.
                var result = new DateTime(year, month, day, hour, minute, second, millisecond, DateTimeKind.Utc);
                if (offsetInMinutes != 0)
                    result = result.AddMinutes(offsetInMinutes);
                return result;
            }

            // Otherwise, parse as an unstructured string.
            return ParseUnstructured(input);
        }

        private enum ChunkClassification
        {
            Unknown,
            Date,
            Time,
            Year,
            Month,
            Zone,
            Number,
        }

        /// <summary>
        /// Parses an unstructured javascript date string.
        /// </summary>
        /// <param name="input"> The string to parse as a date. </param>
        /// <returns> A date. </returns>
        private static DateTime ParseUnstructured(string input)
        {
            // Initialize the lookup tables, if necessary.
            if (dayOfWeekNames == null)
            {
                var temp1 = PopulateDayOfWeekNames();
                var temp2 = PopulateMonthNames();
                var temp3 = PopulateTimeZones();
                System.Threading.Thread.MemoryBarrier();
                dayOfWeekNames = temp1;
                monthNames = temp2;
                timeZoneNames = temp3;
            }

            // split the string into bite-sized pieces.
            var words = input.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);

            // Classify each word.
            int year = -1, month = -1, day = -1;
            int hour = 0, minute = 0, second = 0, millisecond = 0;
            bool twelveHourTime = false;
            DateTimeKind kind = DateTimeKind.Local;
            int offsetInMinutes = 0;
            List<int> unclassifiedNumbers = new List<int>();
            for (int i = 0; i < words.Length; i++)
            {
                string word = words[i];

                // Check if the word is a number.
                int numericValue;
                if (word.StartsWith("GMT", StringComparison.OrdinalIgnoreCase) ||
                    word.StartsWith("UTC", StringComparison.OrdinalIgnoreCase) ||
                    word.StartsWith("+", StringComparison.Ordinal) ||
                    word.StartsWith("-", StringComparison.Ordinal))
                {
                    // If the word starts with 'GMT', 'UTC', '+', '-' then guess it is a zone.
                    kind = DateTimeKind.Utc;
                    if (word.StartsWith("GMT", StringComparison.OrdinalIgnoreCase) ||
                        word.StartsWith("UTC", StringComparison.OrdinalIgnoreCase))
                        word = word.Substring(3);

                    // Time zone offset is [+-]hhmm.  Convert to minutes.
                    int timeZoneOffset;
                    int.TryParse(word, out timeZoneOffset);
                    offsetInMinutes -= (timeZoneOffset / 100) * 60 + (timeZoneOffset % 100);
                }
                else if (monthNames.TryGetValue(word, out numericValue) == true)
                {
                    // If the word is a month name, guess the word is a month.
                    month = numericValue;
                }
                else if (timeZoneNames.TryGetValue(word, out numericValue) == true)
                {
                    // If the word is a time zone name, the word is a zone.
                    kind = DateTimeKind.Utc;
                    offsetInMinutes -= numericValue * 60;
                }
                else if (word.Equals("AM", StringComparison.OrdinalIgnoreCase))
                {
                    // This is a 12-hour time.
                    twelveHourTime = true;
                }
                else if (word.Equals("PM", StringComparison.OrdinalIgnoreCase))
                {
                    // This is a 12-hour time.
                    twelveHourTime = true;
                    offsetInMinutes = 60 * 12;
                }
                else if (int.TryParse(word, out numericValue) == true)
                {
                    // Guess that a number >= 1000 is the year.
                    if (numericValue >= 1000)
                        year = numericValue;
                    else
                        unclassifiedNumbers.Add(numericValue);
                }
                else if (word.IndexOfAny(new char[] { '/', '-' }) >= 0)
                {
                    // If the word contains a slash or a dash, guess the word is a date.
                    string[] components = word.Split('/', '-');
                    if (components.Length != 3)
                        return DateTime.MinValue;
                    if (int.TryParse(components[0], out month) == false)
                        return DateTime.MinValue;
                    if (int.TryParse(components[1], out day) == false)
                        return DateTime.MinValue;
                    if (int.TryParse(components[2], out year) == false)
                        return DateTime.MinValue;
                }
                else if (word.IndexOf(':') >= 0)
                {
                    // If the word contains a colon, guess the word is a time.
                    string[] components = word.Split(':');
                    if (components.Length < 2 || components.Length > 3)
                        return DateTime.MinValue;
                    if (int.TryParse(components[0], out hour) == false)
                        return DateTime.MinValue;
                    if (int.TryParse(components[1], out minute) == false)
                        return DateTime.MinValue;
                    if (components.Length >= 3 && int.TryParse(components[2], out second) == false)
                        return DateTime.MinValue;
                }
                else if (dayOfWeekNames.Contains(word) == true)
                {
                    // Day of week name is ignored.
                }
                else if (word.StartsWith("(", StringComparison.Ordinal) == true)
                {
                    // Extraneous text can start with a parenthesis, this will stop parsing.
                    break;
                }
                else
                {
                    // Error.
                    return DateTime.MinValue;
                }
            }

            // Now assign unclassified numbers to month, day, year, in that order.
            if (month == -1)
            {
                if (unclassifiedNumbers.Count == 0)
                    return DateTime.MinValue;
                month = unclassifiedNumbers[0];
                unclassifiedNumbers.RemoveAt(0);
            }
            if (day == -1)
            {
                if (unclassifiedNumbers.Count == 0)
                    return DateTime.MinValue;
                day = unclassifiedNumbers[0];
                unclassifiedNumbers.RemoveAt(0);
            }
            if (year == -1)
            {
                if (unclassifiedNumbers.Count == 0)
                    return DateTime.MinValue;
                year = unclassifiedNumbers[0];
                if (year >= 70 && year < 100)
                    year += 1900;   // two digit dates are okay from 1970 - 1999.
                unclassifiedNumbers.RemoveAt(0);
            }
            if (unclassifiedNumbers.Count != 0)
                return DateTime.MinValue;

            // Validate the components.
            if (year < 70)
                return DateTime.MinValue;
            if (month < 1 || month > 12)
                return DateTime.MinValue;
            if (day < 1 || day > 31)
                return DateTime.MinValue;
            if (hour >= 24 || (twelveHourTime && hour >= 13))
                return DateTime.MinValue;
            if (minute >= 60)
                return DateTime.MinValue;
            if (second >= 60)
                return DateTime.MinValue;

            // If there exists an AM/PM designator, 12:00 is the same as 0:00.
            if (twelveHourTime && hour == 12)
                hour = 0;

            // Create a date from the components.
            var result = new DateTime(year, month, 1, hour, minute, second, millisecond, kind);
            result = result.AddDays(day - 1);   // The day can wrap around to the next month.
            if (offsetInMinutes != 0)
                result = result.AddMinutes(offsetInMinutes);
            return result;
        }

        // A dictionary containing the names of all the days of the week.
        private static HashSet<string> dayOfWeekNames;

        /// <summary>
        /// Constructs a HashSet containing the names of days of the week.
        /// </summary>
        private static HashSet<string> PopulateDayOfWeekNames()
        {
            var result = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

            // Add the abbreviated day names for the culture to the dictionary.
            var abbreviatedNames = CultureInfo.InvariantCulture.DateTimeFormat.AbbreviatedDayNames;
            foreach (string dayName in abbreviatedNames)
                result.Add(dayName);

            // Add the full day names for the culture to the dictionary.
            var fullNames = CultureInfo.InvariantCulture.DateTimeFormat.DayNames;
            foreach (string dayName in fullNames)
                result.Add(dayName);

            return result;
        }

        // A dictionary containing the names of all the months and a mapping to the number of the
        // month (1-12).
        private static Dictionary<string, int> monthNames;

        /// <summary>
        /// Constructs a dictionary containing the names of all the months and a mapping to the
        /// number of the month (1-12).
        /// </summary>
        private static Dictionary<string, int> PopulateMonthNames()
        {
            var monthNames = new Dictionary<string, int>(48, StringComparer.InvariantCultureIgnoreCase);

            // Add the abbreviated month names for the culture to the dictionary.
            var abbreviatedNames = CultureInfo.InvariantCulture.DateTimeFormat.AbbreviatedMonthNames;
            for (int i = 0; i < 12; i++)
                monthNames[abbreviatedNames[i]] = i + 1;

            // Add the full month names for the culture to the dictionary.
            var fullNames = CultureInfo.InvariantCulture.DateTimeFormat.MonthNames;
            for (int i = 0; i < 12; i++)
                monthNames[fullNames[i]] = i + 1;

            return monthNames;
        }

        // A dictionary containing the time zone names.
        private static Dictionary<string, int> timeZoneNames;

        /// <summary>
        /// Constructs a dictionary containing the names of all the time zones and a mapping to the
        /// time zone offset (in hours).
        /// </summary>
        private static Dictionary<string, int> PopulateTimeZones()
        {
            var result = new Dictionary<string, int>(15, StringComparer.InvariantCultureIgnoreCase)
            {
                { "UT", 0 },
                { "UTC", 0 },
                { "GMT", 0 },
                { "EST", -5 },
                { "EDT", -4 },
                { "CST", -6 },
                { "CDT", -5 },
                { "MST", -7 },
                { "MDT", -6 },
                { "PST", -8 },
                { "PDT", -7 },
                { "Z", 0 },
                { "A", -1 },
                { "M", -12 },
                { "N", 1 },
                { "Y", 12 },
            };
            return result;
        }

        
    }
}
