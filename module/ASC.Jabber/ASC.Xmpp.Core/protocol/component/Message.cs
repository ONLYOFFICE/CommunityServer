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


#region Using directives

using ASC.Xmpp.Core.protocol.client;

#endregion

namespace ASC.Xmpp.Core.protocol.component
{
    /// <summary>
    ///   Summary description for Message.
    /// </summary>
    public class Message : client.Message
    {
        #region << Constructors >>

        public Message()
        {
            Namespace = Uri.ACCEPT;
        }

        public Message(Jid to)
            : base(to)
        {
            Namespace = Uri.ACCEPT;
        }

        public Message(Jid to, string body)
            : base(to, body)
        {
            Namespace = Uri.ACCEPT;
        }

        public Message(Jid to, Jid from)
            : base(to, from)
        {
            Namespace = Uri.ACCEPT;
        }

        public Message(string to, string body)
            : base(to, body)
        {
            Namespace = Uri.ACCEPT;
        }

        public Message(Jid to, string body, string subject)
            : base(to, body, subject)
        {
            Namespace = Uri.ACCEPT;
        }

        public Message(string to, string body, string subject)
            : base(to, body, subject)
        {
            Namespace = Uri.ACCEPT;
        }

        public Message(string to, string body, string subject, string thread)
            : base(to, body, subject, thread)
        {
            Namespace = Uri.ACCEPT;
        }

        public Message(Jid to, string body, string subject, string thread)
            : base(to, body, subject, thread)
        {
            Namespace = Uri.ACCEPT;
        }

        public Message(string to, MessageType type, string body)
            : base(to, type, body)
        {
            Namespace = Uri.ACCEPT;
        }

        public Message(Jid to, MessageType type, string body)
            : base(to, type, body)
        {
            Namespace = Uri.ACCEPT;
        }

        public Message(string to, MessageType type, string body, string subject)
            : base(to, type, body, subject)
        {
            Namespace = Uri.ACCEPT;
        }

        public Message(Jid to, MessageType type, string body, string subject)
            : base(to, type, body, subject)
        {
            Namespace = Uri.ACCEPT;
        }

        public Message(string to, MessageType type, string body, string subject, string thread)
            : base(to, type, body, subject, thread)
        {
            Namespace = Uri.ACCEPT;
        }

        public Message(Jid to, MessageType type, string body, string subject, string thread)
            : base(to, type, body, subject, thread)
        {
            Namespace = Uri.ACCEPT;
        }

        public Message(Jid to, Jid from, string body)
            : base(to, from, body)
        {
            Namespace = Uri.ACCEPT;
        }

        public Message(Jid to, Jid from, string body, string subject)
            : base(to, from, body, subject)
        {
            Namespace = Uri.ACCEPT;
        }

        public Message(Jid to, Jid from, string body, string subject, string thread)
            : base(to, from, body, subject, thread)
        {
            Namespace = Uri.ACCEPT;
        }

        public Message(Jid to, Jid from, MessageType type, string body)
            : base(to, from, type, body)
        {
            Namespace = Uri.ACCEPT;
        }

        public Message(Jid to, Jid from, MessageType type, string body, string subject)
            : base(to, from, type, body, subject)
        {
            Namespace = Uri.ACCEPT;
        }

        public Message(Jid to, Jid from, MessageType type, string body, string subject, string thread)
            : base(to, from, type, body, subject, thread)
        {
            Namespace = Uri.ACCEPT;
        }

        #endregion

        /// <summary>
        ///   Error Child Element
        /// </summary>
        public new Error Error
        {
            get { return SelectSingleElement(typeof (Error)) as Error; }
            set
            {
                if (HasTag(typeof (Error)))
                    RemoveTag(typeof (Error));

                if (value != null)
                    AddChild(value);
            }
        }
    }
}