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


namespace ASC.Mail.Net.FTP
{
    #region usings

    using System;

    #endregion

    /// <summary>
    /// This class holds single file or directory in the FTP server.
    /// </summary>
    public class FTP_ListItem
    {
        #region Members

        private readonly bool m_IsDir;
        private readonly DateTime m_Modified;
        private readonly string m_Name = "";
        private readonly long m_Size;

        #endregion

        #region Properties

        /// <summary>
        /// Gets if current item is directory.
        /// </summary>
        public bool IsDir
        {
            get { return m_IsDir; }
        }

        /// <summary>
        /// Gets if current item is file.
        /// </summary>
        public bool IsFile
        {
            get { return !m_IsDir; }
        }

        /// <summary>
        /// Gets the name of the file or directory.
        /// </summary>
        public string Name
        {
            get { return m_Name; }
        }

        /// <summary>
        /// Gets file size in bytes.
        /// </summary>
        public long Size
        {
            get { return m_Size; }
        }

        /// <summary>
        /// Gets last time file or direcory was modified.
        /// </summary>
        public DateTime Modified
        {
            get { return m_Modified; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="name">Directory or file name.</param>
        /// <param name="size">File size in bytes, zero for directory.</param>
        /// <param name="modified">Directory or file last modification time.</param>
        /// <param name="isDir">Specifies if list item is directory or file.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>name</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        public FTP_ListItem(string name, long size, DateTime modified, bool isDir)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (name == "")
            {
                throw new ArgumentException("Argument 'name' value must be specified.");
            }

            m_Name = name;
            m_Size = size;
            m_Modified = modified;
            m_IsDir = isDir;
        }

        #endregion
    }
}