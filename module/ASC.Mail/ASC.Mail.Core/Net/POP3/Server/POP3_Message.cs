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