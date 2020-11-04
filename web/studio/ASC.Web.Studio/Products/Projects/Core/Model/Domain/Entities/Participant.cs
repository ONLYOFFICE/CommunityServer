/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


using System;
using System.Collections;
using System.Diagnostics;
using ASC.Core;
using ASC.Core.Users;

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

        public bool IsRemovedFromTeam { get; set; }

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