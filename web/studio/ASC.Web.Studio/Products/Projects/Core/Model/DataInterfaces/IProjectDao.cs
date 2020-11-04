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


#region Usings

using System;
using System.Collections.Generic;
using ASC.Projects.Core.Domain;
using System.Collections;
using ASC.Common.Data.Sql.Expressions;

#endregion

namespace ASC.Projects.Core.DataInterfaces
{
    public interface IProjectDao
    {
        List<Project> GetAll(ProjectStatus? status, int max);

        List<Project> GetLast(ProjectStatus? status, int offset, int max);
        
        List<Project> GetByParticipiant(Guid participantId, ProjectStatus status);

        List<Project> GetFollowing(Guid participantId);

        List<Project> GetOpenProjectsWithTasks(Guid participantId);
            
        
        DateTime GetMaxLastModified();

        void UpdateLastModified(int projectID);
            
        Project GetById(int projectId);

        List<Project> GetById(ICollection projectIDs);

        bool IsExists(int projectId);

        bool IsFollow(int projectId, Guid participantId);
        
        int Count();

        List<int> GetTaskCount(List<int> projectId, TaskStatus? taskStatus, bool isAdmin);

        int GetMessageCount(int projectId);
        
        int GetTotalTimeCount(int projectId);
        
        int GetMilestoneCount(int projectId, params MilestoneStatus[] milestoneStatus);

        Project Create(Project project);
        Project Update(Project project);

        void Delete(int projectId, out List<int> messages, out List<int> tasks);


        void AddToTeam(int projectId, Guid participantId);

        void RemoveFromTeam(int projectId, Guid participantId);

        bool IsInTeam(int projectId, Guid participantId);

        List<Participant> GetTeam(Project project, bool withExluded = false);

        List<Participant> GetTeam(IEnumerable<Project> projects);

        List<ParticipantFull> GetTeamUpdates(DateTime from, DateTime to);

        DateTime GetTeamMaxLastModified();

        void SetTeamSecurity(int projectId, Guid participantId, ProjectTeamSecurity teamSecurity);

        ProjectTeamSecurity GetTeamSecurity(int projectId, Guid participantId);


        List<Project> GetByFilter(TaskFilter filter, bool isAdmin, bool checkAccess);

        int GetByFilterCount(TaskFilter filter, bool isAdmin, bool checkAccess);


        List<Project> GetByContactID(int contactID);

        void AddProjectContact(int projectID, int contactID);

        void DeleteProjectContact(int projectID, int contactid);

        
        void SetTaskOrder(int projectID, string order);

        string GetTaskOrder(int projectID);

        IEnumerable<Project> GetProjects(Exp where);
    }
}
