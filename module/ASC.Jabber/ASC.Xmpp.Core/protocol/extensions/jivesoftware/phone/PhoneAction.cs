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