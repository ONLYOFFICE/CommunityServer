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

using ASC.Xmpp.Core.utils.Xml.Dom;

namespace ASC.Xmpp.Core.protocol.iq.privacy
{
    /// <summary>
    ///   This class represents a rule which is used for blocking communication
    /// </summary>
    public class Item : Element
    {
        #region << Constructors >>

        /// <summary>
        ///   Default Contructor
        /// </summary>
        public Item()
        {
            TagName = "item";
            Namespace = Uri.IQ_PRIVACY;
        }

        /// <summary>
        /// </summary>
        /// <param name="action"> </param>
        /// <param name="order"> </param>
        public Item(Action action, int order) : this()
        {
            Action = action;
            Order = order;
        }

        /// <summary>
        /// </summary>
        /// <param name="action"> </param>
        /// <param name="order"> </param>
        /// <param name="block"> </param>
        public Item(Action action, int order, Stanza stanza) : this(action, order)
        {
            Stanza = stanza;
        }

        /// <summary>
        /// </summary>
        /// <param name="action"> </param>
        /// <param name="order"> </param>
        /// <param name="type"> </param>
        /// <param name="value"> </param>
        public Item(Action action, int order, Type type, string value) : this(action, order)
        {
            Type = type;
            Val = value;
        }

        /// <summary>
        /// </summary>
        /// <param name="action"> </param>
        /// <param name="order"> </param>
        /// <param name="type"> </param>
        /// <param name="value"> </param>
        /// <param name="block"> </param>
        public Item(Action action, int order, Type type, string value, Stanza stanza) : this(action, order, type, value)
        {
            Stanza = stanza;
        }

        #endregion

        public Action Action
        {
            get { return (Action) GetAttributeEnum("action", typeof (Action)); }
            set { SetAttribute("action", value.ToString()); }
        }

        public Type Type
        {
            get { return (Type) GetAttributeEnum("type", typeof (Type)); }
            set
            {
                if (value != Type.NONE)
                    SetAttribute("type", value.ToString());
                else
                    RemoveAttribute("type");
            }
        }

        /// <summary>
        ///   The order of this rule
        /// </summary>
        public int Order
        {
            get { return GetAttributeInt("order"); }
            set { SetAttribute("order", value); }
        }

        /// <summary>
        ///   The value to match of this rule
        /// </summary>
        public string Val
        {
            get { return GetAttribute("value"); }
            set { SetAttribute("value", value); }
        }

        /// <summary>
        ///   Block Iq stanzas
        /// </summary>
        public bool BlockIq
        {
            get { return HasTag("iq"); }
            set
            {
                if (value)
                    SetTag("iq");
                else
                    RemoveTag("iq");
            }
        }

        /// <summary>
        ///   Block messages
        /// </summary>
        public bool BlockMessage
        {
            get { return HasTag("message"); }
            set
            {
                if (value)
                    SetTag("message");
                else
                    RemoveTag("message");
            }
        }

        /// <summary>
        ///   Block incoming presence
        /// </summary>
        public bool BlockIncomingPresence
        {
            get { return HasTag("presence-in"); }
            set
            {
                if (value)
                    SetTag("presence-in");
                else
                    RemoveTag("presence-in");
            }
        }

        /// <summary>
        ///   Block outgoing presence
        /// </summary>
        public bool BlockOutgoingPresence
        {
            get { return HasTag("presence-out"); }
            set
            {
                if (value)
                    SetTag("presence-out");
                else
                    RemoveTag("presence-out");
            }
        }

        /// <summary>
        ///   which stanzas should be blocked?
        /// </summary>
        public Stanza Stanza
        {
            get
            {
                Stanza result = Stanza.All;

                if (BlockIq)
                    result |= Stanza.Iq;
                if (BlockMessage)
                    result |= Stanza.Message;
                if (BlockIncomingPresence)
                    result |= Stanza.IncomingPresence;
                if (BlockOutgoingPresence)
                    result |= Stanza.OutgoingPresence;

                return result;
            }
            set
            {
                if (value == Stanza.All)
                {
                    // Block All Communications
                    BlockIq = false;
                    BlockMessage = false;
                    BlockIncomingPresence = false;
                    BlockOutgoingPresence = false;
                }
                else
                {
                    BlockIq = ((value & Stanza.Iq) == Stanza.Iq);
                    BlockMessage = ((value & Stanza.Message) == Stanza.Message);
                    BlockIncomingPresence = ((value & Stanza.IncomingPresence) == Stanza.IncomingPresence);
                    BlockOutgoingPresence = ((value & Stanza.OutgoingPresence) == Stanza.OutgoingPresence);
                }
            }
        }
    }
}