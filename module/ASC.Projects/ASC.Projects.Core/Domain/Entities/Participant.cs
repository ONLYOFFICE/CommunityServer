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

using System;
using System.Diagnostics;
using ASC.Core;
using ASC.Core.Users;
using System.Collections;

namespace ASC.Projects.Core.Domain
{
    [DebuggerDisplay("{UserInfo.ToString()}")]
    public class Participant : IComparable
    {
        public Guid ID { get; private set; }

        public int ProjectID { get; set; }

        public bool CanReadFiles { get; private set; }

        public bool CanReadMilestones { get; private set; }

        public bool CanReadMessages { get; private set; }

        public bool CanReadTasks { get; private set; }

        public bool CanReadContacts { get; set; }

        public bool IsVisitor { get; private set; }

        public bool IsFullAdmin { get; private set; }

        public UserInfo UserInfo { get; private set; }

        public bool IsAdmin { get; set; }

        public Participant(Guid userID)
        {
            ID = userID;
            UserInfo = CoreContext.UserManager.GetUsers(ID);
            IsVisitor = UserInfo.IsVisitor();
            IsFullAdmin = UserInfo.IsAdmin();
        }

        public Participant(Guid userID, ProjectTeamSecurity security)
            : this(userID)
        {
            CanReadFiles = (security & ProjectTeamSecurity.Files) != ProjectTeamSecurity.Files;
            CanReadMilestones = (security & ProjectTeamSecurity.Milestone) != ProjectTeamSecurity.Milestone;
            CanReadMessages = (security & ProjectTeamSecurity.Messages) != ProjectTeamSecurity.Messages;
            CanReadTasks = (security & ProjectTeamSecurity.Tasks) != ProjectTeamSecurity.Tasks;
            CanReadContacts = (security & ProjectTeamSecurity.Contacts) != ProjectTeamSecurity.Contacts;

            if (IsVisitor)
                CanReadContacts = false;
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var p = obj as Participant;
            return p != null && p.ID == ID;
        }

        public int CompareTo(object obj)
        {
            var other = obj as Participant;
            return other == null
                       ? Comparer.Default.Compare(this, obj)
                       : UserFormatter.Compare(UserInfo, other.UserInfo);
        }
    }
}