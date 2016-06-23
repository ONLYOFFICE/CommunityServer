/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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

    using System.Net;

    #endregion

    /// <summary>
    /// 
    /// </summary>
    public class SIP_Route
    {
        #region Properties

        /// <summary>
        /// Gets regex match pattern.
        /// </summary>
        public string MatchPattern
        {
            get { return ""; }
        }

        /// <summary>
        /// Gets matched URI.
        /// </summary>
        public string Uri
        {
            get { return ""; }
        }

        /// <summary>
        /// Gets SIP targets for <b>Uri</b>.
        /// </summary>
        public string[] Targets
        {
            get { return null; }
        }

        /// <summary>
        /// Gets targets processing mode.
        /// </summary>
        public SIP_ForkingMode ProcessMode
        {
            get { return SIP_ForkingMode.Parallel; }
        }

        /// <summary>
        /// Gets if user needs to authenticate to use this route.
        /// </summary>
        public bool RequireAuthentication
        {
            get { return true; }
        }

        /// <summary>
        /// Gets targets credentials.
        /// </summary>
        public NetworkCredential[] Credentials
        {
            get { return null; }
        }

        #endregion
    }
}