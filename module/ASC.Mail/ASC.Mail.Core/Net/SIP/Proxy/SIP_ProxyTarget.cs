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