using System;
using System.Collections.Generic;
using System.Xml;
using Newtonsoft.Json.Linq;

namespace BasecampRestAPI
{
    /*
{
  "id": 2004111,
  "name": "Some quick things to explore in Basecamp",
  "description": "A nice way to get acclimated.",
  "created_at": "2012-07-30T10:57:12+04:00",
  "updated_at": "2012-09-10T14:47:59+04:00",
  "completed": false,
  "position": 1,
  "remaining_count": 4,
  "completed_count": 0,
  "creator": {
    "id": 2047475,
    "name": "Basecamp",
    "avatar_url": "http://asset0.37img.com/global/79383b7fc061815979c8c638784be44d4aa4628aaa16ee5614658a2b4d662eb9ce6fc85f0ea77f81b6e6bd3cdb9c266f42cbf1d2426968d83cb50f0d3078fcd65e6055e264b268aa44ffaa94d9475efa/avatar.gif?r=3"
  },
  "todos": {
    "remaining": [
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
    ],
    "completed": []
  },
  "comments": [],
  "subscribers": []
}
     */

    public class ToDoList : IToDoList
	{
		private readonly IBaseCamp _camp;
        private readonly IRestWebService _service;

        public static ToDoList GetInstance(IBaseCamp baseCamp, JObject node, int projectID)
		{
            return new ToDoList(baseCamp, baseCamp.Service, node, projectID);
		}
		private ToDoList(IBaseCamp baseCamp, IRestWebService service, JObject node, int projectID)
		{
			_camp = baseCamp;
			_service = service;
            ID = node["id"].ToObject<int>();
		    ProjectID = projectID;
            Items = new List<IToDoListItem>();

            foreach (var toDoListItem in node["todos"]["remaining"])
            {
                var path = string.Format("projects/{0}/{1}/{2}.json", ProjectID, "todos", toDoListItem["id"]);
                foreach (JObject node1 in _service.GetRequestResponseElement(path))
                {
                    Items.Add(ToDoListItem.GetInstance(_camp, node1));
                }

            }
            foreach (var toDoListItem in node["todos"]["completed"])
            {
                var path = string.Format("projects/{0}/{1}/{2}.json", ProjectID, "todos", toDoListItem["id"]);
                foreach (JObject node1 in _service.GetRequestResponseElement(path))
                {
                    Items.Add(ToDoListItem.GetInstance(_camp, node1));
                }
            }
		}
		

		#region Implementation of IToDoList

		public int ID { get; private set; }
		public int ProjectID { get; private set; }
        public int MilestoneID { get; private set; }
        public List<IToDoListItem> Items { get; private set; }

		#endregion
	}
}
