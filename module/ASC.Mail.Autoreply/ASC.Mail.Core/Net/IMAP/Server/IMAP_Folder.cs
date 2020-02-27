/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
    /// IMAP folder.
    /// </summary>
    public class IMAP_Folder
    {
        #region Members

        private readonly string m_Folder = "";
        private bool m_Selectable = true;

        #endregion

        #region Properties

        /// <summary>
        /// Gets IMAP folder name. Eg. Inbox, Inbox/myFolder, ... .
        /// </summary>
        public string Folder
        {
            get { return m_Folder; }
        }

        /// <summary>
        /// Gets or sets if folder is selectable (SELECT command can select this folder).
        /// </summary>
        public bool Selectable
        {
            get { return m_Selectable; }

            set { m_Selectable = value; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="folder">Full path to folder, path separator = '/'. Eg. Inbox/myFolder .</param>
        /// <param name="selectable">Gets or sets if folder is selectable(SELECT command can select this folder).</param>
        public IMAP_Folder(string folder, bool selectable)
        {
            m_Folder = folder;
            m_Selectable = selectable;
        }

        #endregion
    }
}