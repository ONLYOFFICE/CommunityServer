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
    /// <summary>
    /// Socket log entry.
    /// </summary>
    public class SocketLogEntry
    {
        #region Members

        private readonly long m_Size;
        private readonly string m_Text = "";
        private readonly SocketLogEntryType m_Type = SocketLogEntryType.FreeText;

        #endregion

        #region Properties

        /// <summary>
        /// Gets log text.
        /// </summary>
        public string Text
        {
            get { return m_Text; }
        }

        /// <summary>
        /// Gets size of data readed or sent.
        /// </summary>
        public long Size
        {
            get { return m_Size; }
        }

        /// <summary>
        /// Gets log entry type.
        /// </summary>
        public SocketLogEntryType Type
        {
            get { return m_Type; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="text">Log text.</param>
        /// <param name="size">Data size.</param>
        /// <param name="type">Log entry type</param>
        public SocketLogEntry(string text, long size, SocketLogEntryType type)
        {
            m_Text = text;
            m_Type = type;
            m_Size = size;
        }

        #endregion
    }
}