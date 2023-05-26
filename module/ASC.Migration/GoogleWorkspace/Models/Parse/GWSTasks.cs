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
using System.Collections.Generic;

using Newtonsoft.Json;

namespace ASC.Migration.GoogleWorkspace.Models.Parse
{
    public class GwsTasksRoot
    {
        [JsonProperty("kind")]
        public string Kind { get; set; }

        [JsonProperty("items")]
        public List<GwsTaskList> Items { get; set; }
    }

    public class GwsTaskList
    {
        [JsonProperty("kind")]
        public string Kind { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("updated")]
        public DateTimeOffset Updated { get; set; }

        [JsonProperty("items", NullValueHandling = NullValueHandling.Ignore)]
        public List<GwsTask> Items { get; set; }
    }

    public class GwsTask
    {
        [JsonProperty("notes")]
        public string Notes { get; set; }

        [JsonProperty("kind")]
        public string Kind { get; set; }

        [JsonProperty("created")]
        public DateTimeOffset Created { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("completed", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? Completed { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("task_type")]
        public string TaskType { get; set; }

        [JsonProperty("updated")]
        public DateTimeOffset Updated { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("due", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? Due { get; set; }
    }
}
