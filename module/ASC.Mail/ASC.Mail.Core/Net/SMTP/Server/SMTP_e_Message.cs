/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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


namespace ASC.Mail.Net.SMTP.Server
{
    #region usings

    using System;
    using System.IO;

    #endregion

    /// <summary>
    /// This class provided data for <b cref="SMTP_Session.GetMessageStream">SMTP_Session.GetMessageStream</b> event.
    /// </summary>
    public class SMTP_e_Message : EventArgs
    {
        #region Members

        private readonly SMTP_Session m_pSession;
        private Stream m_pStream;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="session">Owner SMTP server session.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>session</b> is null reference.</exception>
        public SMTP_e_Message(SMTP_Session session)
        {
            if (session == null)
            {
                throw new ArgumentNullException("session");
            }

            m_pSession = session;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets owner SMTP session.
        /// </summary>
        public SMTP_Session Session
        {
            get { return m_pSession; }
        }

        /// <summary>
        /// Gets or stes stream where to store incoming message.
        /// </summary>
        /// <exception cref="ArgumentNullException">Is raised when null reference is passed.</exception>
        public Stream Stream
        {
            get { return m_pStream; }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("Stream");
                }

                m_pStream = value;
            }
        }

        #endregion
    }
}