using System;
using System.Xml;
using Newtonsoft.Json.Linq;

namespace BasecampRestAPI
{
	//<person>
    //{
    //  "id": 2047474,
    //  "name": "Paul Bannov",
    //  "email_address": "paul.bannov@gmail.com",
    //  "avatar_url": "http://asset0.37img.com/global/default_avatar_v1_4/avatar.gif?r=3",
    //  "updated_at": "2012-08-21T10:19:37+04:00",
    //  "url": "https://basecamp.com/1937390/api/v1/people/2047474-paul-bannov.json"
    //}
	//</person>
    //additional data <can-post>1</can-post>
	public class Person : IPerson
	{

		public static IPerson GetInstance(IBaseCamp camp, JObject node)
		{
			return new Person(camp, node);
		}
		private Person(IBaseCamp camp, JObject node)
		{
			ID = node["id"].ToObject<int>();
			EmailAddress = node["email_address"].ToObject<string>();
            UserName = node["name"].ToObject<string>();

            if (node["avatar_url"] != null)
                AvatarUrl = node["avatar_url"].ToObject<string>();
		}

		#region Implementation of IPerson

		public int ID { get; private set; }
		public string EmailAddress { get; private set; }
		public string UserName { get; private set; }
        public string AvatarUrl { get; private set; }

		#endregion
	}
}
