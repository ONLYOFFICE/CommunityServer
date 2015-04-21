using System;
using System.Xml;
using Newtonsoft.Json.Linq;

namespace BasecampRestAPI
{
	public class Milestone : IMilestone
	{
		private readonly IBaseCamp _camp;

		public static IMilestone GetInstance(IBaseCamp camp, JObject node)
		{
			return new Milestone(camp, node);
		}
        private Milestone(IBaseCamp camp, JObject node)
		{
			_camp = camp;
            ID = node["id"].ToObject<int>();
            Title = node["title"].ToObject<string>();
            CreatedOn = node["created-on"].ToObject<DateTime>();
            Deadline = node["deadline"].ToObject<DateTime>();
            Title = node["title"].ToObject<string>();
            Completed = node["completed"].ToObject<bool>();
            ProjectID = node["project-id"].ToObject<int>();
		}

		#region Implementation of IMilestone

		public int ID { get; private set; }
		public bool Completed { get; private set; }
		public DateTime CreatedOn { get; private set; }
		public DateTime Deadline { get; set; }
		public string Title { get; set; }
        public int ProjectID { get; private set; }

		#endregion
	}
}
