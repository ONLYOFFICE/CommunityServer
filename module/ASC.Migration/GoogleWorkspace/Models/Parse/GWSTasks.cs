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
