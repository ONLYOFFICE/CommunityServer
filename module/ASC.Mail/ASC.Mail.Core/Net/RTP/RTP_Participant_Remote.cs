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

namespace ASC.Mail.Net.RTP
{
    #region usings

    using System;
    using System.Text;

    #endregion

    /// <summary>
    /// Implements RTP <b>participant</b>. Defined in RFC 3550.
    /// </summary>
    public class RTP_Participant_Remote : RTP_Participant
    {
        #region Events

        /// <summary>
        /// Is raised when participant data changed.
        /// </summary>
        public event EventHandler<RTP_ParticipantEventArgs> Changed = null;

        #endregion

        #region Members

        private string m_Email;
        private string m_Location;
        private string m_Name;
        private string m_Note;
        private string m_Phone;
        private string m_Tool;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the real name, eg. "John Doe". Value null means not specified.
        /// </summary>
        public string Name
        {
            get { return m_Name; }
        }

        /// <summary>
        /// Gets email address. For example "John.Doe@example.com". Value null means not specified.
        /// </summary>
        public string Email
        {
            get { return m_Email; }
        }

        /// <summary>
        /// Gets phone number. For example "+1 908 555 1212". Value null means not specified.
        /// </summary>
        public string Phone
        {
            get { return m_Phone; }
        }

        /// <summary>
        /// Gets location string. It may be geographic address or for example chat room name.
        /// Value null means not specified.
        /// </summary>
        public string Location
        {
            get { return m_Location; }
        }

        /// <summary>
        /// Gets streaming application name/version.
        /// Value null means not specified.
        /// </summary>
        public string Tool
        {
            get { return m_Tool; }
        }

        /// <summary>
        /// Gets note text. The NOTE item is intended for transient messages describing the current state
        /// of the source, e.g., "on the phone, can't talk". Value null means not specified.
        /// </summary>
        public string Note
        {
            get { return m_Note; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="cname">Canonical name of participant. For example: john.doe@domain.com-randomTag.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>cname</b> is null reference.</exception>
        internal RTP_Participant_Remote(string cname) : base(cname) {}

        #endregion

        #region Methods

        /// <summary>
        /// Returns participant as string.
        /// </summary>
        /// <returns>Returns participant as string.</returns>
        public override string ToString()
        {
            StringBuilder retVal = new StringBuilder();

            retVal.AppendLine("CNAME: " + CNAME);
            if (!string.IsNullOrEmpty(m_Name))
            {
                retVal.AppendLine("Name: " + m_Name);
            }
            if (!string.IsNullOrEmpty(m_Email))
            {
                retVal.AppendLine("Email: " + m_Email);
            }
            if (!string.IsNullOrEmpty(m_Phone))
            {
                retVal.AppendLine("Phone: " + m_Phone);
            }
            if (!string.IsNullOrEmpty(m_Location))
            {
                retVal.AppendLine("Location: " + m_Location);
            }
            if (!string.IsNullOrEmpty(m_Tool))
            {
                retVal.AppendLine("Tool: " + m_Tool);
            }
            if (!string.IsNullOrEmpty(m_Note))
            {
                retVal.AppendLine("Note: " + m_Note);
            }

            return retVal.ToString().TrimEnd();
        }

        #endregion

        // TODO: PRIV

        #region Utility methods

        /// <summary>
        /// Raises <b>Changed</b> event.
        /// </summary>
        private void OnChanged()
        {
            if (Changed != null)
            {
                Changed(this, new RTP_ParticipantEventArgs(this));
            }
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// Updates participant data from SDES items.
        /// </summary>
        /// <param name="sdes">SDES chunk.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>sdes</b> is null reference value.</exception>
        internal void Update(RTCP_Packet_SDES_Chunk sdes)
        {
            if (sdes == null)
            {
                throw new ArgumentNullException("sdes");
            }

            bool changed = false;
            if (!string.IsNullOrEmpty(sdes.Name) && !string.Equals(m_Name, sdes.Name))
            {
                m_Name = sdes.Name;
                changed = true;
            }
            if (!string.IsNullOrEmpty(sdes.Email) && !string.Equals(m_Email, sdes.Email))
            {
                m_Email = sdes.Email;
                changed = true;
            }
            if (!string.IsNullOrEmpty(sdes.Phone) && !string.Equals(Phone, sdes.Phone))
            {
                m_Phone = sdes.Phone;
                changed = true;
            }
            if (!string.IsNullOrEmpty(sdes.Location) && !string.Equals(m_Location, sdes.Location))
            {
                m_Location = sdes.Location;
                changed = true;
            }
            if (!string.IsNullOrEmpty(sdes.Tool) && !string.Equals(m_Tool, sdes.Tool))
            {
                m_Tool = sdes.Tool;
                changed = true;
            }
            if (!string.IsNullOrEmpty(sdes.Note) && !string.Equals(m_Note, sdes.Note))
            {
                m_Note = sdes.Note;
                changed = true;
            }

            if (changed)
            {
                OnChanged();
            }
        }

        #endregion
    }
}