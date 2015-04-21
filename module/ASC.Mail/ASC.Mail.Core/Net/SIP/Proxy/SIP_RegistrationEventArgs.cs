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


namespace ASC.Mail.Net.SIP.Proxy
{
    #region usings

    using System;

    #endregion

    /// <summary>
    /// This class provides data for <b>SIP_Registrar.AorRegistered</b>,<b>SIP_Registrar.AorUnregistered</b> and <b>SIP_Registrar.AorUpdated</b> event.
    /// </summary>
    public class SIP_RegistrationEventArgs : EventArgs
    {
        #region Members

        private readonly SIP_Registration m_pRegistration;

        #endregion

        #region Properties

        /// <summary>
        /// Gets SIP registration.
        /// </summary>
        public SIP_Registration Registration
        {
            get { return m_pRegistration; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="registration">SIP reggistration.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>registration</b> is null reference.</exception>
        public SIP_RegistrationEventArgs(SIP_Registration registration)
        {
            if (registration == null)
            {
                throw new ArgumentNullException("registration");
            }

            m_pRegistration = registration;
        }

        #endregion
    }
}