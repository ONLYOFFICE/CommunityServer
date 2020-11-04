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