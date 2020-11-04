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
using ASC.Common.Data.Sql.Expressions;
using ASC.Projects.Core.Domain;

#endregion

namespace ASC.Projects.Core.DataInterfaces
{
    public interface IMilestoneDao
    {
        List<Milestone> GetAll();

        List<Milestone> GetByProject(int projectId);

        List<Milestone> GetByStatus(int projectId, MilestoneStatus milestoneStatus);

        List<Milestone> GetUpcomingMilestones(int offset, int max, params int[] projects);

        List<Milestone> GetLateMilestones(int offset, int max);

        List<Milestone> GetByDeadLine(DateTime deadline);

        List<Milestone> GetByFilter(TaskFilter filter, bool isAdmin, bool checkAccess);

        int GetByFilterCount(TaskFilter filter, bool isAdmin, bool checkAccess);

        List<Tuple<Guid, int, int>> GetByFilterCountForReport(TaskFilter filter, bool isAdmin, bool checkAccess);

        List<object[]> GetInfoForReminder(DateTime deadline);
        
        Milestone GetById(int id);

        List<Milestone> GetById(int[] id);

        bool IsExists(int id);

        Milestone Save(Milestone milestone);

        void Delete(int id);

        string GetLastModified();

        List<Milestone> GetMilestones(Exp where);
    }
}
