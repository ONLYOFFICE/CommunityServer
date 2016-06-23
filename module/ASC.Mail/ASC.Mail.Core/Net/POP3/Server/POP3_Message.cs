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


namespace ASC.Mail.Net.POP3.Server
{
    /// <summary>
    /// Holds POP3_Message info (ID,Size,...).
    /// </summary>
    public class POP3_Message
    {
        #region Members

        private string m_ID = ""; // Holds message ID.
        private POP3_MessageCollection m_pOwner;
        private string m_UID = "";

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets message ID.
        /// </summary>
        public string ID
        {
            get { return m_ID; }

            set { m_ID = value; }
        }

        /// <summary>
        /// Gets or sets message UID. This UID is reported in UIDL command.
        /// </summary>
        public string UID
        {
            get { return m_UID; }

            set { m_UID = value; }
        }

        /// <summary>
        /// Gets or sets message size in bytes.
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// Gets or sets message state flag.
        /// </summary>
        public bool MarkedForDelete { get; set; }

        /// <summary>
        /// Gets or sets user data for message.
        /// </summary>
        public object Tag { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="onwer">Owner collection.</param>
        /// <param name="id">Message ID.</param>
        /// <param name="uid">Message UID.</param>
        /// <param name="size">Message size in bytes.</param>
        internal POP3_Message(POP3_MessageCollection onwer, string id, string uid, long size)
        {
            m_pOwner = onwer;
            m_ID = id;
            m_UID = uid;
            Size = size;
        }

        #endregion
    }
}