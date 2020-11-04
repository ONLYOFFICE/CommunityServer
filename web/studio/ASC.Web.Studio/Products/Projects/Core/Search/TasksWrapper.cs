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


using ASC.ElasticSearch;
using ASC.Projects.Core.Domain;
using ASC.Web.Projects.Configuration;

namespace ASC.Web.Projects.Core.Search
{
    public sealed class TasksWrapper : Wrapper
    {
        [Column("title", 1)]
        public string Title { get; set; }

        [Column("description", 2)]
        public string Description { get; set; }

        //[Join(JoinTypeEnum.Sub, "id:task_id", "tenant_id:tenant_id")]
        //public List<SubtasksWrapper> Subtasks { get; set; }

        protected override string Table { get { return "projects_tasks"; } }

        public static implicit operator TasksWrapper(Task task)
        {
            return ProductEntryPoint.Mapper.Map<TasksWrapper>(task);
            //var result = Mapper.Map<TasksWrapper>(task);
            //result.Subtasks = task.SubTasks.Select(r=> new SubtasksWrapper { Id = r.ID, Title = r.Title}).ToList();
            //return result;
        }
    }
}