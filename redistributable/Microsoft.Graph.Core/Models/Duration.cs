// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using Newtonsoft.Json;
    using System;
    using System.Xml;

    /// <summary>
    /// Represents an edm.duration value.
    /// </summary>
    [JsonConverter(typeof(DurationConverter))]
    public class Duration
    {
        internal TimeSpan TimeSpan { get; set; }

        /// <summary>
        /// Create a Duration object from a TimeSpan.
        /// </summary>
        /// <param name="timeSpan"></param>
        public Duration(TimeSpan timeSpan)
        {
            this.TimeSpan = timeSpan;
        }

        /// <summary>
        /// Create a Duration object from an ISO8601 duration.
        /// </summary>
        /// <param name="duration">An ISO8601 duration. http://en.wikipedia.org/wiki/ISO_8601#Durations </param>
        public Duration(string duration)
        {
            // Convert an ISO8601 duration to a TimeSpan. 
            this.TimeSpan = XmlConvert.ToTimeSpan(duration);
        }

        /// <summary>
        /// Convert the stored TimeSpan into an ISO8601 duration.
        /// </summary>
        /// <returns>An ISO8601 duration. For example, PT1M is "period time of 1 minute"</returns>
        public override string ToString()
        {
            return XmlConvert.ToString(this.TimeSpan);
        }
    }
}
