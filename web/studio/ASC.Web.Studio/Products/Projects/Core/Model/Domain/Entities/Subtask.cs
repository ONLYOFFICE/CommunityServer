/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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
using System.Diagnostics;

namespace ASC.Projects.Core.Domain
{
    ///<inherited>ASC.Projects.Core.Domain.DomainObject`1, ASC.Web.Projects</inherited>
    [DebuggerDisplay("SubTask: ID = {ID}, Title = {Title}")]
    public class Subtask : DomainObject<int>
    {
        public override EntityType EntityType { get { return EntityType.SubTask; } }

        ///<example>Title</example>
        public String Title { get; set; }

        ///<example>2fdfe577-3c26-4736-9df9-b5a683bb8520</example>
        public Guid Responsible { get; set; }

        ///<example type="int">1</example>
        public TaskStatus Status { get; set; }

        ///<example type="int">1</example>
        public int Task { get; set; }

        ///<example>null</example>
        public Task ParentTask { get; set; }

        ///<example>2fdfe577-3c26-4736-9df9-b5a683bb8520</example>
        public Guid CreateBy { get; set; }

        ///<example>2020-12-22T04:11:57.0469085+00:00</example>
        public DateTime CreateOn { get; set; }

        ///<example>2020-12-22T04:11:57.0469085+00:00</example>
        public DateTime LastModifiedOn { get; set; }

        ///<example type="int">1</example>
        public Guid LastModifiedBy { get; set; }

        ///<example>2020-12-22T04:11:57.0469085+00:00</example>
        public DateTime StatusChangedOn { get; set; }
    }
}
