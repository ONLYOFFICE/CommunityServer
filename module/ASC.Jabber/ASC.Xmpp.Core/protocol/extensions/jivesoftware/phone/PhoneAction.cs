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

// // --------------------------------------------------------------------------------------------------------------------
// // <copyright company="Ascensio System Limited" file="PhoneAction.cs">
// //   
// // </copyright>
// // <summary>
// //   (c) Copyright Ascensio System Limited 2008-2012
// // </summary>
// // --------------------------------------------------------------------------------------------------------------------

using ASC.Xmpp.Core.utils.Xml.Dom;

namespace ASC.Xmpp.Core.protocol.extensions.jivesoftware.phone
{
    public class PhoneAction : Element
    {
        #region << Constructors >>

        /// <summary>
        /// </summary>
        public PhoneAction()
        {
            TagName = "phone-action";
            Namespace = Uri.JIVESOFTWARE_PHONE;
        }

        public PhoneAction(ActionType type) : this()
        {
            Type = type;
        }

        public PhoneAction(ActionType type, string extension) : this(type)
        {
            Extension = extension;
        }

        public PhoneAction(ActionType type, Jid jid) : this(type)
        {
            Jid = jid;
        }

        #endregion

        /*
         * Actions are sent by the client to perform tasks such as dialing, checking for messages, etc. Actions are sent as IQ's (type set), as with the following child stanza:
         * 
         * <phone-action xmlns="http://jivesoftware.com/xmlns/phone" type="DIAL">
         *    <extension>5035555555</extension>
         * </phone-action>
         *          
         * Currently supported types are DIAL and FORWARD.
         * In most implementations, issuing a dial command will cause the user's phone to ring.
         * Once the user picks up, the specified extension will be dialed.
         * 
         * Dialing can also be performed by jid too. The jid must be dialed must be mapped on the server to an extension
         * 
         * <phone-action type="DIAL">
         *  <jid>andrew@jivesoftware.com</jid>
         * </phone-action>
         * 
         * Issuing a action wth a type FORWARD should transfer a call that has already been 
         * established to a third party. The FORWARD type requires an extension or jid child element
         *
         *  <phone-action xmlns="http://jivesoftware.com/xmlns/phone" type="FORWARD">
         *      <extension>5035555555</extension>
         *  </phone-action>
         *
         */

        public ActionType Type
        {
            set { SetAttribute("type", value.ToString()); }
            get { return (ActionType) GetAttributeEnum("type", typeof (ActionType)); }
        }

        public string Extension
        {
            get { return GetTag("extension"); }
            set { SetTag("extension", value); }
        }

        public Jid Jid
        {
            get { return new Jid(GetTag("jid")); }
            set { SetTag("jid", value.ToString()); }
        }
    }
}