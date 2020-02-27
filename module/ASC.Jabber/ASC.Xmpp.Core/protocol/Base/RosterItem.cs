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

namespace ASC.Xmpp.Core.protocol.Base
{

    #region usings

    #endregion

    // jabber:iq:roster
    // <iq from="gnauck@myjabber.net/Office" id="doroster_1" type="result">
    // 		<query xmlns="jabber:iq:roster">
    // 			<item subscription="both" name="Nachtkrapp" jid="50198521@icq.myjabber.net"><group>ICQ</group></item>
    // 			<item subscription="both" name="czerkasov" jid="62764180@icq.myjabber.net"><group>ICQ</group></item>
    // 			<item subscription="both" name="Poacher" jid="92179686@icq.myjabber.net"><group>ICQ</group></item>
    // 			<item subscription="both" name="Diabolo" jid="102840558@icq.myjabber.net"><group>ICQ</group></item>
    // 		</query>
    // </iq> 

    // # "none" -- the user does not have a subscription to the contact's presence information, and the contact does not have a subscription to the user's presence information
    // # "to" -- the user has a subscription to the contact's presence information, but the contact does not have a subscription to the user's presence information
    // # "from" -- the contact has a subscription to the user's presence information, but the user does not have a subscription to the contact's presence information
    // # "both" -- both the user and the contact have subscriptions to each other's presence information

    /// <summary>
    ///   Item is used in jabber:iq:roster, x roster
    /// </summary>
    public class RosterItem : Item
    {
        #region Constructor

        #endregion

        #region Methods

        /// <summary>
        ///   Groups a roster Item is assigned to
        /// </summary>
        public ElementList GetGroups()
        {
            return SelectElements("group");
        }

        /// <summary>
        ///   Add a new group to the Rosteritem
        /// </summary>
        /// <param name="groupname"> </param>
        public void AddGroup(string groupname)
        {
            var g = new Group(groupname);
            AddChild(g);
        }

        /// <summary>
        /// </summary>
        /// <param name="groupname"> </param>
        /// <returns> </returns>
        public bool HasGroup(string groupname)
        {
            ElementList groups = GetGroups();
            foreach (Group g in groups)
            {
                if (g.Name == groupname)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// </summary>
        /// <param name="groupname"> </param>
        public void RemoveGroup(string groupname)
        {
            ElementList groups = GetGroups();
            foreach (Group g in groups)
            {
                if (g.Name == groupname)
                {
                    g.Remove();
                    return;
                }
            }
        }

        #endregion
    }
}