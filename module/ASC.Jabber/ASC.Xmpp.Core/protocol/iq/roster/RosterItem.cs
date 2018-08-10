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


namespace ASC.Xmpp.Core.protocol.iq.roster
{
    // jabber:iq:roster
    // <iq from="user@server.com/Office" id="doroster_1" type="result">
    //		<query xmlns="jabber:iq:roster">
    //			<item subscription="both" name="juiliet" jid="11111@icq.myjabber.net"><group>ICQ</group></item>
    //			<item subscription="both" name="roman" jid="22222@icq.myjabber.net"><group>ICQ</group></item>
    //			<item subscription="both" name="angie" jid="33333@icq.myjabber.net"><group>ICQ</group></item>
    //			<item subscription="both" name="bob" jid="44444@icq.myjabber.net"><group>ICQ</group></item>
    //		</query>
    // </iq> 

    // # "none" -- the user does not have a subscription to the contact's presence information, and the contact does not have a subscription to the user's presence information
    // # "to" -- the user has a subscription to the contact's presence information, but the contact does not have a subscription to the user's presence information
    // # "from" -- the contact has a subscription to the user's presence information, but the user does not have a subscription to the contact's presence information
    // # "both" -- both the user and the contact have subscriptions to each other's presence information

    // TODO rename to Ask and move to a seperate file, so it matches better to all other enums
    public enum AskType
    {
        NONE = -1,
        subscribe,
        unsubscribe
    }

    // TODO rename to Subscription and move to a seperate file, so it matches better to all other enums
    public enum SubscriptionType
    {
        none = -1,
        to,
        from,
        both,
        remove
    }

    /// <summary>
    ///   Item is used in jabber:iq:roster, jabber:iq:search
    /// </summary>
    public class RosterItem : Base.RosterItem
    {
        public RosterItem()
        {
            Namespace = Uri.IQ_ROSTER;
        }

        public RosterItem(Jid jid) : this()
        {
            Jid = jid;
        }

        public RosterItem(Jid jid, string name) : this(jid)
        {
            Name = name;
        }

        public SubscriptionType Subscription
        {
            get { return (SubscriptionType) GetAttributeEnum("subscription", typeof (SubscriptionType)); }
            set { SetAttribute("subscription", value.ToString()); }
        }

        public AskType Ask
        {
            get { return (AskType) GetAttributeEnum("ask", typeof (AskType)); }
            set
            {
                if (value == AskType.NONE)
                    RemoveAttribute("ask");
                else
                    SetAttribute("ask", value.ToString());
            }
        }
    }
}