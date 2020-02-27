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


using System;
using ASC.Xmpp.Core.utils;
using ASC.Xmpp.Core.utils.Xml.Dom;

namespace ASC.Xmpp.Core.protocol.x
{

    #region usings

    #endregion

    // <presence to="gnauck@myjabber.net/myJabber v3.5" from="yahoo.myjabber.net/registered">
    // 		<status>Extended Away</status>
    // 		<show>xa</show><priority>5</priority>
    // 		<x stamp="20050206T13:09:50" from="gnauck@myjabber.net/myJabber v3.5" xmlns="jabber:x:delay"/>    
    // </presence> 

    /// <summary>
    ///   <para>Delay class for Timestamps</para> <para>Mainly used in offline and groupchat messages. This is the time when the message was received by the server</para>
    /// </summary>
    public class Delay : Element
    {
        #region Constructor

        /// <summary>
        /// </summary>
        public Delay()
        {
            TagName = "x";
            Namespace = Uri.X_DELAY;
        }

        #endregion

        #region Properties

        /// <summary>
        /// </summary>
        public Jid From
        {
            get
            {
                if (HasAttribute("from"))
                {
                    return new Jid(GetAttribute("from"));
                }
                else
                {
                    return null;
                }
            }

            set { SetAttribute("from", value.ToString()); }
        }

        /// <summary>
        /// </summary>
        public DateTime Stamp
        {
            get { return Time.Date(GetAttribute("stamp")); }

            set { SetAttribute("stamp", Time.Date(value)); }
        }

        #endregion
    }
}