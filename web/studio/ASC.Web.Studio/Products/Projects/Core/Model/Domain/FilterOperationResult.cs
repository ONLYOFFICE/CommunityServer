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

namespace ASC.Projects.Core.Domain
{
    public class TaskFilterCountOperationResult
    {
        public Guid UserId { get; set; }

        public int TasksOpen { get; set; }

        public int TasksClosed { get; set; }

        public int TasksTotal
        {
            get { return TasksOpen + TasksClosed; }
        }
    }

    public class TaskFilterOperationResult
    {
        public IEnumerable<Task> FilterResult { get; set; }
        
        public TaskFilterCountOperationResult FilterCount { get; set; }

        public TaskFilterOperationResult()
            : this(new TaskFilterCountOperationResult())
        {
        }

        public TaskFilterOperationResult(TaskFilterCountOperationResult count)
            : this(new List<Task>(), count)
        {
        }

        public TaskFilterOperationResult(IEnumerable<Task> tasks, TaskFilterCountOperationResult count)
        {
            FilterResult = tasks;
            FilterCount = count;
        }
    }
}
