/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

namespace ASC.Mail.Net.FTP.Server
{
    #region usings

    using System;
    using System.Data;
    using System.IO;

    #endregion

    /// <summary>
    /// Provides data for the filesytem related events for FTP_Server.
    /// </summary>
    public class FileSysEntry_EventArgs
    {
        #region Members

        private readonly DataSet m_DsDirInfo;
        private readonly string m_Name = "";
        private readonly string m_NewName = "";
        private readonly FTP_Session m_pSession;
        private bool m_Validated = true;

        #endregion

        #region Properties

        /// <summary>
        /// Gets reference to FTP session.
        /// </summary>
        public FTP_Session Session
        {
            get { return m_pSession; }
        }

        /// <summary>
        /// Gets directory or file name with path.
        /// </summary>
        public string Name
        {
            get { return m_Name; }
        }

        /// <summary>
        /// Gets new directory or new file name with path. This filled for Rename event only.
        /// </summary>
        public string NewName
        {
            get { return m_NewName; }
        }

        /// <summary>
        /// Gets or sets file stream.
        /// </summary>
        public Stream FileStream { get; set; }

        /// <summary>
        /// Gets or sets if operation was successful. NOTE: default value is true.
        /// </summary>
        public bool Validated
        {
            get { return m_Validated; }

            set { m_Validated = value; }
        }

        /// <summary>
        /// Gets reference to dir listing info. Please Fill .Tables["DirInfo"] table with required fields.
        /// </summary>
        public DataSet DirInfo
        {
            get { return m_DsDirInfo; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="newName"></param>
        /// <param name="session"></param>
        public FileSysEntry_EventArgs(FTP_Session session, string name, string newName)
        {
            m_Name = name;
            m_NewName = newName;
            m_pSession = session;

            m_DsDirInfo = new DataSet();
            DataTable dt = m_DsDirInfo.Tables.Add("DirInfo");
            dt.Columns.Add("Name");
            dt.Columns.Add("Date", typeof (DateTime));
            dt.Columns.Add("Size", typeof (long));
            dt.Columns.Add("IsDirectory", typeof (bool));
        }

        #endregion
    }
}