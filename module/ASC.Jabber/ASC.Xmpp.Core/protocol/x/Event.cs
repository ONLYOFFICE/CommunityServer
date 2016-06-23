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


using ASC.Xmpp.Core.utils.Xml.Dom;

namespace ASC.Xmpp.Core.protocol.x
{

    #region usings

    #endregion

    /// <summary>
    ///   JEP-0022: Message Events This JEP defines protocol extensions used to request and respond to events relating to the delivery, display, and composition of messages.
    /// </summary>
    public class Event : Element
    {
        #region Constructor

        /// <summary>
        /// </summary>
        public Event()
        {
            TagName = "x";
            Namespace = Uri.X_EVENT;
        }

        #endregion

        #region Properties

        /// <summary>
        ///   In threaded chat conversations, this indicates that the recipient is composing a reply to a message. The event is to be raised by the recipient's Jabber client. A Jabber client is allowed to raise this event multiple times in response to the same request, providing the original event is cancelled first.
        /// </summary>
        public bool Composing
        {
            get { return HasTag("composing"); }

            set
            {
                RemoveTag("composing");
                if (value)
                {
                    AddTag("composing");
                }
            }
        }

        /// <summary>
        ///   Indicates that the message has been delivered to the recipient. This signifies that the message has reached the recipient's Jabber client, but does not necessarily mean that the message has been displayed. This event is to be raised by the Jabber client.
        /// </summary>
        public bool Delivered
        {
            get { return HasTag("delivered"); }

            set
            {
                RemoveTag("delivered");
                if (value)
                {
                    AddTag("delivered");
                }
            }
        }

        /// <summary>
        ///   Once the message has been received by the recipient's Jabber client, it may be displayed to the user. This event indicates that the message has been displayed, and is to be raised by the Jabber client. Even if a message is displayed multiple times, this event should be raised only once.
        /// </summary>
        public bool Displayed
        {
            get { return HasTag("displayed"); }

            set
            {
                RemoveTag("displayed");
                if (value)
                {
                    AddTag("displayed");
                }
            }
        }

        /// <summary>
        ///   'id' attribute of the original message to which this event notification pertains. (If no 'id' attribute was included in the original message, then the id tag must still be included with no
        /// </summary>
        public string Id
        {
            get { return GetTag("id"); }

            set { SetTag("id", value); }
        }

        /// <summary>
        ///   Indicates that the message has been stored offline by the intended recipient's server. This event is triggered only if the intended recipient's server supports offline storage, has that support enabled, and the recipient is offline when the server receives the message for delivery.
        /// </summary>
        public bool Offline
        {
            get { return HasTag("offline"); }

            set
            {
                RemoveTag("offline");
                if (value)
                {
                    AddTag("offline");
                }
            }
        }

        #endregion
    }
}