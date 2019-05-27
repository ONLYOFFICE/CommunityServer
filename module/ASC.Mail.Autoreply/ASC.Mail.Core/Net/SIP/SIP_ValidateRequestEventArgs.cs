/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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


namespace ASC.Mail.Net.SIP.Stack
{
    #region usings

    using System;
    using System.Net;

    #endregion

    /// <summary>
    /// This class provides data for SIP_Stack.ValidateRequest event.
    /// </summary>
    public class SIP_ValidateRequestEventArgs : EventArgs
    {
        #region Members

        private readonly IPEndPoint m_pRemoteEndPoint;
        private readonly SIP_Request m_pRequest;

        #endregion

        #region Properties

        /// <summary>
        /// Gets incoming SIP request.
        /// </summary>
        public SIP_Request Request
        {
            get { return m_pRequest; }
        }

        /// <summary>
        /// Gets IP end point what made request.
        /// </summary>
        public IPEndPoint RemoteEndPoint
        {
            get { return m_pRemoteEndPoint; }
        }

        /// <summary>
        /// Gets or sets response code. Value null means SIP stack will handle it.
        /// </summary>
        public string ResponseCode { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="request">Incoming SIP request.</param>
        /// <param name="remoteEndpoint">IP end point what made request.</param>
        public SIP_ValidateRequestEventArgs(SIP_Request request, IPEndPoint remoteEndpoint)
        {
            m_pRequest = request;
            m_pRemoteEndPoint = remoteEndpoint;
        }

        #endregion
    }
}