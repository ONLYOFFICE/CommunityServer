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

namespace ASC.Xmpp.Core.protocol.x.muc.iq.admin
{
    /// <summary>
    /// </summary>
    public class Item : muc.Item
    {
        #region Constructor

        /// <summary>
        /// </summary>
        public Item()
        {
            Namespace = Uri.MUC_ADMIN;
        }

        /// <summary>
        /// </summary>
        /// <param name="affiliation"> </param>
        public Item(Affiliation affiliation) : this()
        {
            Affiliation = affiliation;
        }

        /// <summary>
        /// </summary>
        /// <param name="affiliation"> </param>
        /// <param name="jid"> </param>
        public Item(Affiliation affiliation, Jid jid) : this(affiliation)
        {
            Jid = jid;
        }

        /// <summary>
        /// </summary>
        /// <param name="role"> </param>
        public Item(Role role) : this()
        {
            Role = role;
        }

        /// <summary>
        /// </summary>
        /// <param name="role"> </param>
        /// <param name="jid"> </param>
        public Item(Role role, Jid jid) : this(role)
        {
            Jid = jid;
        }

        /// <summary>
        /// </summary>
        /// <param name="jid"> </param>
        public Item(Jid jid) : this()
        {
            Jid = jid;
        }

        /// <summary>
        /// </summary>
        /// <param name="affiliation"> </param>
        /// <param name="role"> </param>
        public Item(Affiliation affiliation, Role role) : this(affiliation)
        {
            Role = role;
        }

        /// <summary>
        /// </summary>
        /// <param name="affiliation"> </param>
        /// <param name="role"> </param>
        /// <param name="jid"> </param>
        public Item(Affiliation affiliation, Role role, Jid jid) : this(affiliation, role)
        {
            Jid = jid;
        }

        /// <summary>
        /// </summary>
        /// <param name="affiliation"> </param>
        /// <param name="role"> </param>
        /// <param name="reason"> </param>
        public Item(Affiliation affiliation, Role role, string reason) : this(affiliation, role)
        {
            Reason = reason;
        }

        /// <summary>
        /// </summary>
        /// <param name="affiliation"> </param>
        /// <param name="role"> </param>
        /// <param name="jid"> </param>
        /// <param name="reason"> </param>
        public Item(Affiliation affiliation, Role role, Jid jid, string reason) : this(affiliation, role, jid)
        {
            Reason = reason;
        }

        #endregion
    }
}