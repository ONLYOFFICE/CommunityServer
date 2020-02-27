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