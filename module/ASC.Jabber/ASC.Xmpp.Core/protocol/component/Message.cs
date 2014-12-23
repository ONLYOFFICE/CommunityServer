/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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