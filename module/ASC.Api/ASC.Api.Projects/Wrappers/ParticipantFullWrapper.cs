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
using System.Runtime.Serialization;
using ASC.Api.Employee;
using ASC.Projects.Core.Domain;

namespace ASC.Api.Projects.Wrappers
{
    [DataContract(Name = "participant_full", Namespace = "")]
    public class ParticipantFullWrapper
    {
        [DataMember(Order = 100)]
        public ProjectWrapper Project { get; set; }

        [DataMember(Order = 200)]
        public EmployeeWraper Participant { get; set; }

        [DataMember(Order = 201)]
        public DateTime Created { get; set; }

        [DataMember(Order = 202)]
        public DateTime Updated { get; set; }

        [DataMember(Order = 203)]
        public bool Removed { get; set; }


        public ParticipantFullWrapper(ProjectApiBase projectApiBase,ParticipantFull participant)
        {
            Project = projectApiBase.ProjectWrapperSelector(participant.Project);
            Participant = projectApiBase.GetEmployeeWraper(participant.ID);
            Created = participant.Created;
            Updated = participant.Updated;
            Removed = participant.Removed;
        }
    }
}