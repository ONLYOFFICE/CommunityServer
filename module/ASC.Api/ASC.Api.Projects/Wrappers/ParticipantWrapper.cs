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


using System.Runtime.Serialization;
using ASC.Api.Employee;
using ASC.Projects.Core.Domain;

namespace ASC.Api.Projects.Wrappers
{
    [DataContract(Name = "participant", Namespace = "")]
    public class ParticipantWrapper : EmployeeWraperFull
    {
        [DataMember]
        public bool CanReadFiles { get; set; }

        [DataMember]
        public bool CanReadMilestones { get; set; }

        [DataMember]
        public bool CanReadMessages { get; set; }

        [DataMember]
        public bool CanReadTasks { get; set; }

        [DataMember]
        public bool CanReadContacts { get; set; }

        [DataMember]
        public bool IsAdministrator { get; set; }

        [DataMember]
        public bool IsRemovedFromTeam { get; set; }

        public ParticipantWrapper(ProjectApiBase projectApiBase, Participant participant)
            : base(participant.UserInfo)
        {
            CanReadFiles = participant.CanReadFiles;
            CanReadMilestones = participant.CanReadMilestones;
            CanReadMessages = participant.CanReadMessages;
            CanReadTasks = participant.CanReadTasks;
            CanReadContacts = participant.CanReadContacts;
            IsAdministrator = projectApiBase.ProjectSecurity.IsAdministrator(participant.ID);
            IsRemovedFromTeam = participant.IsRemovedFromTeam;
        }
    }
}