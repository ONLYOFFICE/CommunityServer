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


namespace ASC.Mail.Net.SIP.Proxy
{
    #region usings

    using System;
    using Stack;

    #endregion

    /// <summary>
    /// Represents SIP proxy target in the SIP proxy "target set". Defined in RFC 3261 16.
    /// </summary>
    public class SIP_ProxyTarget
    {
        #region Members

        private readonly SIP_Flow m_pFlow;
        private readonly SIP_Uri m_pTargetUri;

        #endregion

        #region Properties

        /// <summary>
        /// Gets target URI.
        /// </summary>
        public SIP_Uri TargetUri
        {
            get { return m_pTargetUri; }
        }

        /// <summary>
        /// Gets data flow. Value null means that new flow must created.
        /// </summary>
        public SIP_Flow Flow
        {
            get { return m_pFlow; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="targetUri">Target request-URI.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>targetUri</b> is null reference.</exception>
        public SIP_ProxyTarget(SIP_Uri targetUri) : this(targetUri, null) {}

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="targetUri">Target request-URI.</param>
        /// <param name="flow">Data flow to try for forwarding..</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>targetUri</b> is null reference.</exception>
        public SIP_ProxyTarget(SIP_Uri targetUri, SIP_Flow flow)
        {
            if (targetUri == null)
            {
                throw new ArgumentNullException("targetUri");
            }

            m_pTargetUri = targetUri;
            m_pFlow = flow;
        }

        #endregion
    }
}