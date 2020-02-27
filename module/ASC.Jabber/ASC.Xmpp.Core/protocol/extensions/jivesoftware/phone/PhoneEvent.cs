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


using ASC.Xmpp.Core.utils.Xml.Dom;

namespace ASC.Xmpp.Core.protocol.extensions.jivesoftware.phone
{
    /* 
     *
     *
     * <message from="x" id="1137178511.247" to="y">
     *      <phone-event xmlns="http://jivesoftware.com/xmlns/phone" callID="1137178511.247" type="ON_PHONE" device="SIP/3001">
     *          <callerID></callerID>
     *      </phone-event>
     * </message>
     * 
     */

    /// <summary>
    ///   Events are sent to the user when their phone is ringing, when a call ends, etc. This packet is send within a message packet (subelement of message)
    /// </summary>
    public class PhoneEvent : Element
    {
        #region << Constructors >>

        public PhoneEvent()
        {
            TagName = "phone-event";
            Namespace = Uri.JIVESOFTWARE_PHONE;
        }

        public PhoneEvent(PhoneStatusType status) : this()
        {
            Type = status;
        }

        public PhoneEvent(PhoneStatusType status, string device) : this(status)
        {
            Device = device;
        }

        public PhoneEvent(PhoneStatusType status, string device, string id) : this(status, device)
        {
            CallId = id;
        }

        public PhoneEvent(PhoneStatusType status, string device, string id, string callerId) : this(status, device, id)
        {
            CallerId = callerId;
        }

        #endregion

        public string CallId
        {
            get { return GetAttribute("callID"); }
            set { SetAttribute("callID", value); }
        }

        public string Device
        {
            get { return GetAttribute("device"); }
            set { SetAttribute("device", value); }
        }

        public PhoneStatusType Type
        {
            set { SetAttribute("type", value.ToString()); }
            get { return (PhoneStatusType) GetAttributeEnum("type", typeof (PhoneStatusType)); }
        }

        public string CallerId
        {
            get { return GetTag("callerID"); }
            set { SetTag("callerID", value); }
        }

        public string CallerIdName
        {
            get { return GetTag("callerIDName"); }
            set { SetTag("callerIDName", value); }
        }
    }
}