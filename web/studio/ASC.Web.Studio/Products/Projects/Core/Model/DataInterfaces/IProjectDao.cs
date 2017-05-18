/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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

        List<Participant> GetTeam(Project project);

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
    }
}
