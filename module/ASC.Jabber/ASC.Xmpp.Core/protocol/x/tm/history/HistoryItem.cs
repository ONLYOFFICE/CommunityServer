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
// // <copyright company="Ascensio System Limited" file="HistoryItem.cs">
// //   
// // </copyright>
// // <summary>
// //   (c) Copyright Ascensio System Limited 2008-2012
// // </summary>
// // --------------------------------------------------------------------------------------------------------------------

#region using

using ASC.Xmpp.Core.protocol.client;
using ASC.Xmpp.Core.protocol.extensions.html;
using ASC.Xmpp.Core.protocol.extensions.nickname;
using ASC.Xmpp.Core.protocol.extensions.shim;
using ASC.Xmpp.Core.utils.Xml.Dom;

#endregion

namespace ASC.Xmpp.Core.protocol.x.tm.history
{
    public class HistoryItem : Element
    {
        public HistoryItem()
        {
            TagName = "item";
            Namespace = Uri.X_TM_IQ_HISTORY;
        }

        public string Body
        {
            get { return GetTag("body"); }

            set { SetTag("body", value); }
        }

        /// <summary>
        ///   subject of this message. Its like a subject in a email. The Subject is optional.
        /// </summary>
        public string Subject
        {
            get { return GetTag("subject"); }

            set
            {
                if (HasTag("subject"))
                {
                    RemoveTag("subject");
                }

                if (!string.IsNullOrEmpty(value))
                {
                    SetTag("subject", value);
                }
            }
        }

        /// <summary>
        ///   messages and conversations could be threaded. You can compare this with threads in newsgroups or forums. Threads are optional.
        /// </summary>
        public string Thread
        {
            get { return GetTag("thread"); }

            set
            {
                if (HasTag("thread"))
                {
                    RemoveTag("thread");
                }

                if (!string.IsNullOrEmpty(value))
                {
                    SetTag("thread", value);
                }
            }
        }

        public MessageType Type
        {
            get { return (MessageType) GetAttributeEnum("type", typeof (MessageType)); }

            set
            {
                if (value == MessageType.normal)
                {
                    RemoveAttribute("type");
                }
                else
                {
                    SetAttribute("type", value.ToString());
                }
            }
        }

        /// <summary>
        ///   Error Child Element
        /// </summary>
        public client.Error Error
        {
            get { return SelectSingleElement(typeof (client.Error)) as client.Error; }

            set
            {
                if (HasTag(typeof (client.Error)))
                {
                    RemoveTag(typeof (client.Error));
                }

                if (value != null)
                {
                    AddChild(value);
                }
            }
        }

        public Jid From
        {
            get { return GetAttributeJid("from"); }
            set { SetAttribute("from", value); }
        }

        /// <summary>
        ///   The html part of the message if you want to support the html-im Jep. This part of the message is optional.
        /// </summary>
        public Html Html
        {
            get { return (Html) SelectSingleElement(typeof (Html)); }

            set
            {
                RemoveTag(typeof (Html));
                if (value != null)
                {
                    AddChild(value);
                }
            }
        }

        public Event XEvent
        {
            get { return SelectSingleElement(typeof (Event)) as Event; }

            set
            {
                if (HasTag(typeof (Event)))
                {
                    RemoveTag(typeof (Event));
                }

                if (value != null)
                {
                    AddChild(value);
                }
            }
        }

        /// <summary>
        ///   The event Element for JEP-0022 Message events
        /// </summary>
        public Delay XDelay
        {
            get { return SelectSingleElement(typeof (Delay)) as Delay; }

            set
            {
                if (HasTag(typeof (Delay)))
                {
                    RemoveTag(typeof (Delay));
                }

                if (value != null)
                {
                    AddChild(value);
                }
            }
        }

        /// <summary>
        ///   Stanza Headers and Internet Metadata
        /// </summary>
        public Headers Headers
        {
            get { return SelectSingleElement(typeof (Headers)) as Headers; }

            set
            {
                if (HasTag(typeof (Headers)))
                {
                    RemoveTag(typeof (Headers));
                }

                if (value != null)
                {
                    AddChild(value);
                }
            }
        }

        /// <summary>
        ///   Nickname Element
        /// </summary>
        public Nickname Nickname
        {
            get { return SelectSingleElement(typeof (Nickname)) as Nickname; }

            set
            {
                if (HasTag(typeof (Nickname)))
                {
                    RemoveTag(typeof (Nickname));
                }

                if (value != null)
                {
                    AddChild(value);
                }
            }
        }

        public static HistoryItem FromMessage(Message message)
        {
            message = (Message) message.Clone();
            var item = new HistoryItem
                           {
                               From = message.From,
                               Body = message.Body,
                               Subject = message.Subject,
                               Thread = message.Thread,
                               Nickname = message.Nickname,
                               Type = message.Type,
                               Html = message.Html,
                               Headers = message.Headers,
                               XDelay = message.XDelay,
                               XEvent = message.XEvent
                           };
            return item;
        }
    }
}