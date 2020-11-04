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
    public interface ITaskDao
    {
        List<Task> GetAll();

        List<Task> GetByProject(int projectId, TaskStatus? status, Guid participant);

        List<Task> GetByResponsible(Guid responsibleId, IEnumerable<TaskStatus> statuses);

        List<Task> GetMilestoneTasks(int milestoneId);

        List<Task> GetByFilter(TaskFilter filter, bool isAdmin, bool checkAccess);

        TaskFilterCountOperationResult GetByFilterCount(TaskFilter filter, bool isAdmin, bool checkAccess);

        List<Tuple<Guid, int, int>> GetByFilterCountForReport(TaskFilter filter, bool isAdmin, bool checkAccess);

        IEnumerable<TaskFilterCountOperationResult> GetByFilterCountForStatistic(TaskFilter filter, bool isAdmin, bool checkAccess);

        List<Task> GetById(ICollection<int> ids);

        Task GetById(int id);

        bool IsExists(int id);

        List<object[]> GetTasksForReminder(DateTime deadline);


        Task Create(Task task);
        Task Update(Task task);

        void Delete(Task task);


        void SaveRecurrence(Task task, string cron, DateTime startDate, DateTime endDate);

        void DeleteReccurence(int taskId);

        List<object[]> GetRecurrence(DateTime date);


        void AddLink(TaskLink link);

        void RemoveLink(TaskLink link);

        IEnumerable<TaskLink> GetLinks(int taskID);

        IEnumerable<TaskLink> GetLinks(List<Task> tasks);

        bool IsExistLink(TaskLink link);

        List<Task> GetTasks(Exp where);
    }
}
