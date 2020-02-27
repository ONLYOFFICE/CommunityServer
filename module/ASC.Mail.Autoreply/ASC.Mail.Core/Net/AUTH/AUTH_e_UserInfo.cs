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


namespace ASC.Mail.Net.AUTH
{
    #region usings

    using System;

    #endregion

    /// <summary>
    /// This class provides data for server authentication mechanisms <b>GetUserInfo</b> event.
    /// </summary>
    public class AUTH_e_UserInfo : EventArgs
    {
        #region Members

        private readonly string m_UserName = "";
        private string m_Password = "";

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets if specified user exists.
        /// </summary>
        public bool UserExists { get; set; }

        /// <summary>
        /// Gets user name.
        /// </summary>
        public string UserName
        {
            get { return m_UserName; }
        }

        /// <summary>
        /// Gets or sets user password.
        /// </summary>
        public string Password
        {
            get { return m_Password; }

            set { m_Password = value; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="userName">User name.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>userName</b> is null reference.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        public AUTH_e_UserInfo(string userName)
        {
            if (userName == null)
            {
                throw new ArgumentNullException("userName");
            }
            if (userName == string.Empty)
            {
                throw new ArgumentException("Argument 'userName' value must be specified.", "userName");
            }

            m_UserName = userName;
        }

        #endregion
    }
}