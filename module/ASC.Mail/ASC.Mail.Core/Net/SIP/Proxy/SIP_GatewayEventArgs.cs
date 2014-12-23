/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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