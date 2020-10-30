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
    public sealed class SubtasksWrapper : Wrapper
    {
        [Column("title", 1)]
        public string Title { get; set; }

        [ColumnMeta("task_id", 2)]
        public int Task { get; set; }

        protected override string Table { get { return "projects_subtasks"; } }

        public static implicit operator SubtasksWrapper(Subtask subtask)
        {
            return ProductEntryPoint.Mapper.Map<SubtasksWrapper>(subtask);
        }
    }
}