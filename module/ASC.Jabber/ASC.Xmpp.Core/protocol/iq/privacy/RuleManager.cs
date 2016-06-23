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