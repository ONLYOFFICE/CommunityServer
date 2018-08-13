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

        public bool IsManager { get; set; }

        public ProjectTeamSecurity ProjectTeamSecurity
        {
            get
            {
                var result = ProjectTeamSecurity.None;

                if (!CanReadFiles)
                {
                    result |= ProjectTeamSecurity.Files;
                }
                if (!CanReadMilestones)
                {
                    result |= ProjectTeamSecurity.Milestone;
                }
                if (!CanReadMessages)
                {
                    result |= ProjectTeamSecurity.Messages;
                }
                if (!CanReadTasks)
                {
                    result |= ProjectTeamSecurity.Tasks;
                }
                if (!CanReadContacts)
                {
                    result |= ProjectTeamSecurity.Contacts;
                }

                return result;
            }
            set
            {
                CanReadFiles = (value & ProjectTeamSecurity.Files) != ProjectTeamSecurity.Files;
                CanReadMilestones = (value & ProjectTeamSecurity.Milestone) != ProjectTeamSecurity.Milestone;
                CanReadMessages = (value & ProjectTeamSecurity.Messages) != ProjectTeamSecurity.Messages;
                CanReadTasks = (value & ProjectTeamSecurity.Tasks) != ProjectTeamSecurity.Tasks;
                CanReadContacts = (value & ProjectTeamSecurity.Contacts) != ProjectTeamSecurity.Contacts;

                if (IsVisitor)
                    CanReadContacts = false;
            }
        }

        public Participant()
        {
            
        }

        public Participant(Guid userID)
        {
            ID = userID;
            UserInfo = CoreContext.UserManager.GetUsers(ID);
            IsVisitor = UserInfo.IsVisitor();
            IsFullAdmin = UserInfo.IsAdmin();
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