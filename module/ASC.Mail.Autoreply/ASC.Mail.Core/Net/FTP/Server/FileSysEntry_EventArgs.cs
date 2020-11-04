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