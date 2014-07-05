/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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