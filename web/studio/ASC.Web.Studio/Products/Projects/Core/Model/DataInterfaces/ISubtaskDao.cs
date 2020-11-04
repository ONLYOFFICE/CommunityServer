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
using System.Collections.Generic;
using ASC.Common.Data.Sql.Expressions;
using ASC.Projects.Core.Domain;

namespace ASC.Projects.Core.DataInterfaces
{
    public interface ISubtaskDao
    {
        List<Subtask> GetSubtasks(int taskid);

        void GetSubtasksForTasks(ref List<Task> tasks);

        void CloseAllSubtasks(Task task);

        List<Subtask> GetById(ICollection<int> ids);

        Subtask GetById(int id);

        List<Subtask> GetUpdates(DateTime from, DateTime to);

        List<Subtask> GetByResponsible(Guid id, TaskStatus? status);

        int GetSubtaskCount(int taskid, params TaskStatus[] statuses);

        Subtask Save(Subtask task);

        void Delete(int id);

        List<Subtask> GetSubtasks(Exp where);
    }
}
