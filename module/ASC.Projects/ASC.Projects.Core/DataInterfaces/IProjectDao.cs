/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

#region Usings

using System;
using System.Collections.Generic;
using ASC.Projects.Core.Domain;
using System.Collections;

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

        Project GetFullProjectById(int projectId);

        List<Project> GetById(ICollection projectIDs);

        bool IsExists(int projectId);

        bool IsFollow(int projectId, Guid participantId);
        
        int Count();

        List<int> GetTaskCount(List<int> projectId, TaskStatus? taskStatus, bool isAdmin);

        int GetMessageCount(int projectId);
        
        int GetTotalTimeCount(int projectId);
        
        int GetMilestoneCount(int projectId, params MilestoneStatus[] milestoneStatus);

        Project Save(Project project);

        void Delete(int projectId);


        void AddToTeam(int projectId, Guid participantId);

        void RemoveFromTeam(int projectId, Guid participantId);

        bool IsInTeam(int projectId, Guid participantId);

        List<Participant> GetTeam(int projectId);

        List<Participant> GetTeam(List<int> projectId);

        List<ParticipantFull> GetTeamUpdates(DateTime from, DateTime to);

        DateTime GetTeamMaxLastModified();

        void SetTeamSecurity(int projectId, Guid participantId, ProjectTeamSecurity teamSecurity);

        ProjectTeamSecurity GetTeamSecurity(int projectId, Guid participantId);


        List<Project> GetByFilter(TaskFilter filter, bool isAdmin);

        int GetByFilterCount(TaskFilter filter, bool isAdmin);


        List<Project> GetByContactID(int contactID);

        void AddProjectContact(int projectID, int contactID);

        void DeleteProjectContact(int projectID, int contactid);

        
        void SetTaskOrder(int projectID, string order);

        string GetTaskOrder(int projectID);
    }
}
