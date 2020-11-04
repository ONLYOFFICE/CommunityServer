/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


namespace ASC.Mail.Net
{
    #region usings

    using System.Timers;

    #endregion

    /// <summary>
    /// Simple timer implementation.
    /// </summary>
    public class TimerEx : Timer
    {
        #region Constructor

        /// <summary>
        /// Default contructor.
        /// </summary>
        public TimerEx() {}

        /// <summary>
        /// Default contructor.
        /// </summary>
        /// <param name="interval">The time in milliseconds between events.</param>
        public TimerEx(double interval) : base(interval) {}

        /// <summary>
        /// Default contructor.
        /// </summary>
        /// <param name="interval">The time in milliseconds between events.</param>
        /// <param name="autoReset">Specifies if timer is auto reseted.</param>
        public TimerEx(double interval, bool autoReset) : base(interval)
        {
            AutoReset = autoReset;
        }

        #endregion

        // TODO: We need to do this class CF compatible.
    }
}