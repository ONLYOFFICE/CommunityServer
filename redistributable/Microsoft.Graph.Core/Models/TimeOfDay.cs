// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System;

    using Newtonsoft.Json;

    /// <summary>
    /// Time of day model.
    /// </summary>
    [JsonConverter(typeof(TimeOfDayConverter))]
    public class TimeOfDay
    {
        internal DateTime DateTime { get; set; }

        internal TimeOfDay(DateTime dateTime)
        {
            this.DateTime = dateTime;
        }

        /// <summary>
        /// Create a new TimeOfDay from hours, minutes, and seconds.
        /// </summary>
        /// <param name="hour">The hour.</param>
        /// <param name="minute">The minute.</param>
        /// <param name="second">The second.</param>
        public TimeOfDay(int hour, int minute, int second)
            : this(new DateTime(1, 1, 1, hour, minute, second))
        {
        }
        
        /// <summary>
        /// The hour.
        /// </summary>
        public int Hour
        {
            get
            {
                return this.DateTime.Hour;
            }
        }

        /// <summary>
        /// The minute.
        /// </summary>
        public int Minute
        {
            get
            {
                return this.DateTime.Minute;
            }
        }

        /// <summary>
        /// The second.
        /// </summary>
        public int Second
        {
            get
            {
                return this.DateTime.Second;
            }
        }

        /// <summary>
        /// The time of day, formatted as "HH:mm:ss".
        /// </summary>
        /// <returns>The string time of day.</returns>
        public override string ToString()
        {
            return this.DateTime.ToString("HH:mm:ss");
        }
    }
}
