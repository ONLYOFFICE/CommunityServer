using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Newtonsoft.Json.Linq;

namespace BasecampRestAPI
{
    /*
    {
      "id": 848689,
      "name": "Explore Basecamp!",
      "description": "A sample project to help you discover Basecamp.",
      "archived": false,
      "created_at": "2012-07-30T10:57:12+04:00",
      "updated_at": "2012-07-30T10:57:14+04:00",
      "starred": false,
      "url": "https://basecamp.com/1937390/api/v1/projects/848689-explore-basecamp.json"
    }
    */
    public class Project : IProject
	{
        private string RequestPathForAction(string action)
		{
			return string.Format("projects/{0}/{1}", ID, action);
		}

		private readonly IRestWebService _service;
        private IBaseCamp Camp { get; set; }

		public static IProject GetInstance(IBaseCamp baseCamp, JObject node)
		{
			return new Project(baseCamp, baseCamp.Service, node);
		}
        private Project(IBaseCamp baseCamp, IRestWebService service, JObject node)
		{
			Camp = baseCamp;
			_service = service;
            ID = node["id"].ToObject<int>();
            Name = node["name"].ToObject<string>();
            Description = node["description"].ToObject<string>();
            IsClosed = node["archived"].ToObject<bool>();
		}

		#region Implementation of IProject

		public int ID { get; private set; }

		public string Name { get; private set; }

        public string Description { get; private set; }

		public bool IsClosed { get; private set; }

		public IPerson[] People
		{
			get
			{
                var requestPath = RequestPathForAction("accesses.json");
                var people = new List<IPerson>();
                try
                {
                    foreach (JObject node in _service.GetRequestResponseElement(requestPath))
                    {
                        people.Add(Person.GetInstance(Camp, node));
                    }
                }
                catch
                {
                    return people.ToArray();
                }
                return people.ToArray();
			}
		}
		public IToDoList[] ToDoLists
		{
			get
			{
                var requestPath = RequestPathForAction("todolists.json");
                var lists = new List<IToDoList>();
                try
                {
                    foreach (var node in _service.GetRequestResponseElement(requestPath))
                    {
                        var path = string.Format("projects/{0}/{1}/{2}.json", ID, "todolists", node["id"]);
                        foreach (JObject node1 in _service.GetRequestResponseElement(path))
                        {
                            lists.Add(ToDoList.GetInstance(Camp, node1, ID));
                        }
                    }
                }
                catch
                {
                    return lists.ToArray();
                }
                return lists.ToArray();
			}
		}
		public IMilestone[] Milestones
		{
            get
            {
                var requestPath = RequestPathForAction("milestones/list.xml");
                var milestones = new List<IMilestone>();
                try
                {
                    foreach (JObject node in _service.GetRequestResponseElement(requestPath, "<request/>"))
                    {
                        milestones.Add(Milestone.GetInstance(Camp, node));
                    }
                }
                catch
                {
                    return milestones.ToArray();
                }
                return milestones.ToArray();
            }
		}
		public IPost[] RecentMessages
		{
			get
			{
				return MessagesForAction("topics.json");
			}
		}
        public IAttachment[] Attachments
        {
            get
            {
                return ProjectAttachments("attachments.json");
            }
        }
        public ITimeEntry[] TimeEntries
        {
            get
            {
                return ProjectTimeEntries("time_entries.xml");
            }
        }
        public ICategory[] Categories
        {
            get
            {
                return ProjectCategories("categories.xml");
            }
        }

		public IMilestone GetMilestoneById(int id)
		{
			foreach (IMilestone milestone in Milestones)
			{
				if (milestone.ID == id)
				{
					return milestone;
				}
			}
            return null;
		}
		public ICategory[] GetCategoriesForType(CategoryType type)
		{
			var requestPath = RequestPathForAction(string.Format("categories.xml?type={0}", type.ToString().ToLower()));
			var categories = new List<ICategory>();
            try
            {
                foreach (JObject node in _service.GetRequestResponseElement(requestPath))
                {
                    categories.Add(Category.GetInstance(Camp, node));
                }
            }
            catch
            {
                return categories.ToArray();
            }
			return categories.ToArray();
		}
		public IPost[] GetMessagesForCategory(ICategory category)
		{
			return MessagesForAction(string.Format("cat/{0}/posts.xml", category.ID));
		}

		#endregion

		private IPost[] MessagesForAction(string action)
		{
            var messages = new List<IPost>();
            try
            {
                foreach (var node in _service.GetRequestResponseElement(RequestPathForAction(action)).Cast<JObject>().Where(node => node["topicable"]["type"].ToObject<string>() == "Message"))
                {
                    var path = string.Format("projects/{0}/{1}/{2}.json", ID, "messages", node["topicable"]["id"]);
                    foreach (JObject node1 in _service.GetRequestResponseElement(path))
                    {
                        messages.Add(Post.GetInstance(Camp, node1));
                    }
                }
            }
            catch
            {
                return messages.ToArray();
            }
            return messages.ToArray();
		}
        private IAttachment[] ProjectAttachments(string action)
        {
            var attachments = new List<IAttachment>();
            try
            {
                var isCompleted = false;
                var count = 0;
                var parameter = string.Empty;

                while (!isCompleted)
                {
                    var list = _service.GetRequestResponseElement(RequestPathForAction(action + parameter));

                    foreach (JObject node in list)
                    {
                        attachments.Add(Attachment.GetInstance(Camp, node));
                    }

                    if (list.Count < 100)
                    {
                        isCompleted = true;
                    }
                    else
                    {
                        count = count + 100;
                        parameter = string.Format("?n={0}", count);
                    }
                }
            }
            catch
            {
                return attachments.ToArray();
            }
            return attachments.ToArray();
        }
        private ITimeEntry[] ProjectTimeEntries(string action)
        {
            var timeEntries = new List<ITimeEntry>();
            try
            {
                var element = _service.GetRequestResponseElement(RequestPathForAction(action));
                foreach (JObject node in element)
                {
                    timeEntries.Add(TimeEntry.GetInstance(Camp, node));
                }
            }
            catch
            {
                return timeEntries.ToArray();
            }
            return timeEntries.ToArray();
        }
        private ICategory[] ProjectCategories(string action)
        {
            var categories = new List<ICategory>();
            try
            {
                foreach (JObject node in _service.GetRequestResponseElement(RequestPathForAction(action)))
                {
                    categories.Add(Category.GetInstance(Camp, node));
                }
            }
            catch
            {
                return categories.ToArray();
            }
            return categories.ToArray();
        }
		private static string GetNotificationXml(IEnumerable<IPerson> notifications)
		{
			var result = string.Empty;
			foreach (var person in notifications)
			{
				result += string.Format("<notify>{0}</notify>\n", person.ID);
			}
			return result;
		}
	}
}