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