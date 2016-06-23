/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


namespace ASC.Mail.Net.IMAP.Server
{
    /// <summary>
    /// Provides data for IMAP events.
    /// </summary>
    public class Mailbox_EventArgs
    {
        #region Members

        private readonly string m_Folder = "";
        private readonly string m_NewFolder = "";

        #endregion

        #region Properties

        /// <summary>
        /// Gets folder.
        /// </summary>
        public string Folder
        {
            get { return m_Folder; }
        }

        /// <summary>
        /// Gets new folder name, this is available for rename only.
        /// </summary>
        public string NewFolder
        {
            get { return m_NewFolder; }
        }

        /// <summary>
        /// Gets or sets custom error text, which is returned to client.
        /// </summary>
        public string ErrorText { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="folder"></param>
        public Mailbox_EventArgs(string folder)
        {
            m_Folder = folder;
        }

        /// <summary>
        /// Folder rename constructor.
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="newFolder"></param>
        public Mailbox_EventArgs(string folder, string newFolder)
        {
            m_Folder = folder;
            m_NewFolder = newFolder;
        }

        #endregion
    }
}