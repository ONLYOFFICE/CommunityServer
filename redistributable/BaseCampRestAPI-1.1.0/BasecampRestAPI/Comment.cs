using System;
using System.Xml;
using Newtonsoft.Json.Linq;

namespace BasecampRestAPI
{
    /*
{
  "id": 22198595,
  "content": "kjhkjhkjhkj",
  "created_at": "2012-09-10T13:11:12+04:00",
  "updated_at": "2012-09-10T13:11:12+04:00",
  "attachments": [],
  "creator": {
    "id": 2047474,
    "name": "Paul Bannov",
    "avatar_url": "http://asset0.37img.com/global/default_avatar_v1_4/avatar.gif?r=3"
  }
}
     */
    public class Comment : IComment
	{
		private readonly IBaseCamp _camp;

		public static IComment GetInstance(IBaseCamp camp, JObject node)
		{
			return new Comment(camp, node);
		}
        private Comment(IBaseCamp camp, JObject node)
		{
			_camp = camp;
            ID = node["id"].ToObject<int>();
            AuthorID = node["creator"]["id"].ToObject<int>();
            Body = node["content"].ToObject<string>();
            CreatedAt = node["created_at"].ToObject<DateTime>();
		}

		#region Implementation of IComment

		public int ID { get; private set; }
		//public int CommentableID { get; private set; }
        public int AuthorID { get; private set; }
		//public string CommentableType { get; private set; }
		public string Body { get; private set; }
		//public string EmailedFrom { get; private set; }
		//public int AttachmentsCount { get; private set; }
		public DateTime CreatedAt { get; private set; }

		#endregion
	}
}
