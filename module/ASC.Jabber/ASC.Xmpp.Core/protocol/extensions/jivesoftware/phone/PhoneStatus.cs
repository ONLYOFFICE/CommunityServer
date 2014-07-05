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
// // <copyright company="Ascensio System Limited" file="PhoneStatus.cs">
// //   
// // </copyright>
// // <summary>
// //   (c) Copyright Ascensio System Limited 2008-2012
// // </summary>
// // --------------------------------------------------------------------------------------------------------------------

using ASC.Xmpp.Core.utils.Xml.Dom;

namespace ASC.Xmpp.Core.protocol.extensions.jivesoftware.phone
{
    /// <summary>
    ///   A user's presence is updated when on a phone call. The Jive Messenger/Asterisk implementation will update the user's presence automatically by adding the following packet extension to the user's presence: &lt;phone-status xmlns="http://jivesoftware.com/xmlns/phone" status="ON_PHONE" &gt; Jive Messenger can also be configured to change the user's availability to "Away -- on the phone" when the user is on a call (in addition to the packet extension). This is useful when interacting with clients that don't understand the extended presence information or when using transports to other IM networks where extended presence information is not available.
    /// </summary>
    public class PhoneStatus : Element
    {
        /*
         * <phone-status xmlns="http://jivesoftware.com/xmlns/phone" status="ON_PHONE" >; 
         * 
         */

        public PhoneStatus()
        {
            TagName = "phone-status";
            Namespace = Uri.JIVESOFTWARE_PHONE;
        }

        public PhoneStatusType Status
        {
            set { SetAttribute("status", value.ToString()); }
            get { return (PhoneStatusType) GetAttributeEnum("status", typeof (PhoneStatusType)); }
        }
    }
}