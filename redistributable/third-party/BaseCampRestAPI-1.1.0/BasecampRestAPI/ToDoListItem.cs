using System;
using System.Collections.Generic;
using System.Xml;
using Newtonsoft.Json.Linq;

namespace BasecampRestAPI
{
    /*
      {
        "id": 11699318,
        "todolist_id": 2004111,
        "position": 4,
        "content": "Explore Basecamp's totally unique page stacking interface. Click me to see how it works.",
        "completed": false,
        "created_at": "2012-07-30T10:57:12+04:00",
        "updated_at": "2012-07-30T10:57:12+04:00",
        "comments_count": 1,
        "due_on": null,
        "due_at": null,
        "creator": {
          "id": 2047475,
          "name": "Basecamp",
          "avatar_url": "http://asset0.37img.com/global/79383b7fc061815979c8c638784be44d4aa4628aaa16ee5614658a2b4d662eb9ce6fc85f0ea77f81b6e6bd3cdb9c266f42cbf1d2426968d83cb50f0d3078fcd65e6055e264b268aa44ffaa94d9475efa/avatar.gif?r=3"
        },
        "url": "https://basecamp.com/1937390/api/v1/projects/848689-explore-basecamp/todos/11699318-explore-basecamp-s.json"
      }
     */
    public class ToDoListItem : IToDoListItem
	{
		private readonly IBaseCamp _camp;
		private readonly IRestWebService _service;

        public static IToDoListItem GetInstance(IBaseCamp camp, JObject node)
		{
			return new ToDoListItem(camp, camp.Service, node);
		}
		private ToDoListItem(IBaseCamp camp, IRestWebService service, JObject node)
		{
			_camp = camp;
			_service = service;
			ID = node["id"].ToObject<int>();
			Content = node["content"].ToObject<string>();
            CreatedOn = node["created_at"].ToObject<DateTime>();
			CreatorID = node["creator"]["id"].ToObject<int>();
			Completed = node["completed"].ToObject<bool>();
            RecentComments = new List<IComment>();

            if (node["assignee"] != null)
			{
                ResponsibleID = node["assignee"]["id"].ToObject<int>();
			}
			else
			{
                ResponsibleID = -1;
			}

            if (node["due_on"] != null && node["due_on"].ToObject<string>() != null)
            {
                Deadline = node["due_on"].ToObject<DateTime>();
            }

            foreach (var comment in node["comments"])
            {
                RecentComments.Add(Comment.GetInstance(camp, (JObject)comment));
            }
		}

		#region Implementation of IToDoListItem

		public int ID { get; private set; }
		public string Content { get; set; }
		public DateTime CreatedOn { get; private set; }
		public int CreatorID { get; private set; }
        public DateTime Deadline { get; private set; }
		public bool Completed { get; set; }
        public int ResponsibleID { get; private set; }
        public List<IComment> RecentComments { get; private set; }

		#endregion

        private string PathForAction(string actionPath)
        {
            return string.Format("todo_items/{0}/{1}", ID, actionPath);
        }

        public static string PathForId(int id)
        {
            return string.Format("todo_items/{0}.json", id);
        }
	}
}