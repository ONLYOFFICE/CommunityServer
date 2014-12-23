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

using ASC.Xmpp.Core.protocol.iq.roster;

namespace ASC.Xmpp.Core.protocol.iq.privacy
{
    /// <summary>
    ///   Helper class for creating rules for communication blocking
    /// </summary>
    public class RuleManager
    {
        /// <summary>
        ///   Block stanzas by Jid
        /// </summary>
        /// <param name="JidToBlock"> </param>
        /// <param name="Order"> </param>
        /// <param name="stanza"> stanzas you want to block </param>
        /// <returns> </returns>
        public Item BlockByJid(Jid JidToBlock, int Order, Stanza stanza)
        {
            return new Item(Action.deny, Order, Type.jid, JidToBlock.ToString(), stanza);
        }


        /// <summary>
        ///   Block stanzas for a given roster group
        /// </summary>
        /// <param name="group"> </param>
        /// <param name="Order"> </param>
        /// <param name="stanza"> stanzas you want to block </param>
        /// <returns> </returns>
        public Item BlockByGroup(string group, int Order, Stanza stanza)
        {
            return new Item(Action.deny, Order, Type.group, group, stanza);
        }

        /// <summary>
        ///   Block stanzas by subscription type
        /// </summary>
        /// <param name="subType"> </param>
        /// <param name="Order"> </param>
        /// <param name="stanza"> stanzas you want to block </param>
        /// <returns> </returns>
        public Item BlockBySubscription(SubscriptionType subType, int Order, Stanza stanza)
        {
            return new Item(Action.deny, Order, Type.subscription, subType.ToString(), stanza);
        }

        /// <summary>
        ///   Block globally (all users) the given stanzas
        /// </summary>
        /// <param name="Order"> </param>
        /// <param name="stanza"> stanzas you want to block </param>
        /// <returns> </returns>
        public Item BlockGlobal(int Order, Stanza stanza)
        {
            return new Item(Action.deny, Order, stanza);
        }

        /// <summary>
        ///   Allow stanzas by Jid
        /// </summary>
        /// <param name="JidToBlock"> </param>
        /// <param name="Order"> </param>
        /// <param name="stanza"> stanzas you want to block </param>
        /// <returns> </returns>
        public Item AllowByJid(Jid JidToBlock, int Order, Stanza stanza)
        {
            return new Item(Action.allow, Order, Type.jid, JidToBlock.ToString(), stanza);
        }

        /// <summary>
        ///   Allow stanzas for a given roster group
        /// </summary>
        /// <param name="group"> </param>
        /// <param name="Order"> </param>
        /// <param name="stanza"> stanzas you want to block </param>
        /// <returns> </returns>
        public Item AllowByGroup(string group, int Order, Stanza stanza)
        {
            return new Item(Action.allow, Order, Type.group, group, stanza);
        }

        /// <summary>
        ///   Allow stanzas by subscription type
        /// </summary>
        /// <param name="subType"> </param>
        /// <param name="Order"> </param>
        /// <param name="stanza"> stanzas you want to block </param>
        /// <returns> </returns>
        public Item AllowBySubscription(SubscriptionType subType, int Order, Stanza stanza)
        {
            return new Item(Action.allow, Order, Type.subscription, subType.ToString(), stanza);
        }
    }
}