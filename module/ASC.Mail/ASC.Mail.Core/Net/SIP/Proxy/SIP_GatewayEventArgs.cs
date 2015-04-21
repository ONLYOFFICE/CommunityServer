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
    using System.Collections.Generic;

    #endregion

    /// <summary>
    /// This class provides data for SIP_ProxyCore.GetGateways event.
    /// </summary>
    public class SIP_GatewayEventArgs
    {
        #region Members

        private readonly List<SIP_Gateway> m_pGateways;
        private readonly string m_UriScheme = "";
        private readonly string m_UserName = "";

        #endregion

        #region Properties

        /// <summary>
        /// Gets URI scheme which gateways to get.
        /// </summary>
        public string UriScheme
        {
            get { return m_UriScheme; }
        }

        /// <summary>
        /// Gets authenticated user name.
        /// </summary>        
        public string UserName
        {
            get { return m_UserName; }
        }

        /*
        /// <summary>
        /// Gets or sets if specified user has 
        /// </summary>
        public bool IsForbidden
        {
            get{ return false; }

            set{ }
        }*/

        /// <summary>
        /// Gets gateways collection.
        /// </summary>
        public List<SIP_Gateway> Gateways
        {
            get { return m_pGateways; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="uriScheme">URI scheme which gateways to get.</param>
        /// <param name="userName">Authenticated user name.</param>
        /// <exception cref="ArgumentException">If any argument has invalid value.</exception>
        public SIP_GatewayEventArgs(string uriScheme, string userName)
        {
            if (string.IsNullOrEmpty(uriScheme))
            {
                throw new ArgumentException("Argument 'uriScheme' value can't be null or empty !");
            }

            m_UriScheme = uriScheme;
            m_UserName = userName;
            m_pGateways = new List<SIP_Gateway>();
        }

        #endregion
    }
}