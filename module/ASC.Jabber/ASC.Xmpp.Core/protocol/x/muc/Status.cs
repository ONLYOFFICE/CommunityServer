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


#region using

using ASC.Xmpp.Core.utils.Xml.Dom;

#endregion

namespace ASC.Xmpp.Core.protocol.x.muc
{

    #region usings

    #endregion

    /*
    <x xmlns='http://jabber.org/protocol/muc#user'>
        <status code='100'/>
    </x>    
    */

    /// <summary>
    ///   Summary description for MucUser.
    /// </summary>
    public class Status : Element
    {
        #region Constructor

        /// <summary>
        /// </summary>
        public Status()
        {
            TagName = "status";
            Namespace = Uri.MUC_USER;
        }

        /// <summary>
        /// </summary>
        /// <param name="code"> </param>
        public Status(StatusCode code) : this()
        {
            Code = code;
        }

        /// <summary>
        /// </summary>
        /// <param name="code"> </param>
        public Status(int code) : this()
        {
            SetAttribute("code", code);
        }

        #endregion

        #region Properties

        /// <summary>
        /// </summary>
        public StatusCode Code
        {
            get { return (StatusCode) GetAttributeEnum("code", typeof (StatusCode)); }

            set { SetAttribute("code", value.ToString()); }
        }

        #endregion
    }
}