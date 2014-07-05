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

namespace ASC.Mail.Net
{
    #region usings

    using System.Net;

    #endregion

    /// <summary>
    /// Provides data for the ValidateIPAddress event for servers.
    /// </summary>
    public class ValidateIP_EventArgs
    {
        #region Members

        private readonly IPEndPoint m_pLocalEndPoint;
        private readonly IPEndPoint m_pRemoteEndPoint;
        private string m_ErrorText = "";
        private bool m_Validated = true;

        #endregion

        #region Properties

        /// <summary>
        /// IP address of computer, which is sending mail to here.
        /// </summary>
        public string ConnectedIP
        {
            get { return m_pRemoteEndPoint.Address.ToString(); }
        }

        /// <summary>
        /// Gets local endpoint.
        /// </summary>
        public IPEndPoint LocalEndPoint
        {
            get { return m_pLocalEndPoint; }
        }

        /// <summary>
        /// Gets remote endpoint.
        /// </summary>
        public IPEndPoint RemoteEndPoint
        {
            get { return m_pRemoteEndPoint; }
        }

        /// <summary>
        /// Gets or sets if IP is allowed access.
        /// </summary>
        public bool Validated
        {
            get { return m_Validated; }

            set { m_Validated = value; }
        }

        /// <summary>
        /// Gets or sets user data what is stored to session.Tag property.
        /// </summary>
        public object SessionTag { get; set; }

        /// <summary>
        /// Gets or sets error text what is sent to connected socket. NOTE: This is only used if Validated = false.
        /// </summary>
        public string ErrorText
        {
            get { return m_ErrorText; }

            set { m_ErrorText = value; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="localEndPoint">Server IP.</param>
        /// <param name="remoteEndPoint">Connected client IP.</param>
        public ValidateIP_EventArgs(IPEndPoint localEndPoint, IPEndPoint remoteEndPoint)
        {
            m_pLocalEndPoint = localEndPoint;
            m_pRemoteEndPoint = remoteEndPoint;
        }

        #endregion
    }
}