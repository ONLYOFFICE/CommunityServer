using System;
using System.Collections.Generic;
using System.Xml;
using Newtonsoft.Json.Linq;

namespace BasecampRestAPI
{
	public class BaseCamp : IBaseCamp
	{
		public static BaseCamp GetInstance(string url, string userName, string password)
		{
			return GetInstance(url, userName, password, ProductionWebRequestFactory.GetInstance());
		}

		public static BaseCamp GetInstance(string url, string userName, string password, IWebRequestFactory factory)
		{
			return GetInstance(RestWebService.GetInstance(url, userName, password, factory));
		}
		public static BaseCamp GetInstance(IRestWebService service)
		{
			return new BaseCamp(service);
		}

		private BaseCamp(IRestWebService service)
		{
			Service = service;
		}

		public IRestWebService Service { get; private set; }

		public IPerson[] People
		{
			get
			{
				var people = new List<IPerson>();
				foreach (JObject node in Service.GetRequestResponseElement("people.json"))
				{
					people.Add(Person.GetInstance(this, node));
				}
				return people.ToArray();
			}
		}

		public IProject[] Projects
		{
			get
			{
				var projects = new List<IProject>();
                foreach (JObject node in Service.GetRequestResponseElement("projects.json"))
				{
					projects.Add(Project.GetInstance(this, node));
                }
                foreach (JObject node in Service.GetRequestResponseElement("projects/archived.json"))
                {
                    projects.Add(Project.GetInstance(this, node));
                }
				return projects.ToArray();
			}
		}


		public IComment[] CommentsForResource(string resource, int id, int projectID)
		{
			var comments = new List<IComment>();
			var path = string.Format("projects/{0}/{1}/{2}.json", projectID, resource, id);
			foreach (JObject node in Service.GetRequestResponseElement(path)["comments"])
			{
				comments.Add(Comment.GetInstance(this, node));
			}
			return comments.ToArray();
		}
	}
}
