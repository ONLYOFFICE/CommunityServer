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
using System.Diagnostics;

namespace ASC.Projects.Core.Domain
{
    [DebuggerDisplay("SubTask: ID = {ID}, Title = {Title}")]
    public class Subtask : DomainObject<int>
    {
        public override EntityType EntityType { get { return EntityType.SubTask; } }

        public String Title { get; set; }

        public Guid Responsible { get; set; }

        public TaskStatus Status { get; set; }

        public int Task { get; set; }

        public Task ParentTask { get; set; }

        public Guid CreateBy { get; set; }

        public DateTime CreateOn { get; set; }

        public DateTime LastModifiedOn { get; set; }

        public Guid LastModifiedBy { get; set; }

        public DateTime StatusChangedOn { get; set; }
    }
}
