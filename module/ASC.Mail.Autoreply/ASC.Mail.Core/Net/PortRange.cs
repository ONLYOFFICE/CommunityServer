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

    using System;

    #endregion

    /// <summary>
    /// This class holds UDP or TCP port range.
    /// </summary>
    public class PortRange
    {
        #region Members

        private readonly int m_End = 1100;
        private readonly int m_Start = 1000;

        #endregion

        #region Properties

        /// <summary>
        /// Gets start port.
        /// </summary>
        public int Start
        {
            get { return m_Start; }
        }

        /// <summary>
        /// Gets end port.
        /// </summary>
        public int End
        {
            get { return m_End; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="start">Start port.</param>
        /// <param name="end">End port.</param>
        /// <exception cref="ArgumentOutOfRangeException">Is raised when any of the aruments value is out of range.</exception>
        public PortRange(int start, int end)
        {
            if (start < 1 || start > 0xFFFF)
            {
                throw new ArgumentOutOfRangeException("Argument 'start' value must be > 0 and << 65 535.");
            }
            if (end < 1 || end > 0xFFFF)
            {
                throw new ArgumentOutOfRangeException("Argument 'end' value must be > 0 and << 65 535.");
            }
            if (start > end)
            {
                throw new ArgumentOutOfRangeException(
                    "Argumnet 'start' value must be >= argument 'end' value.");
            }

            m_Start = start;
            m_End = end;
        }

        #endregion
    }
}