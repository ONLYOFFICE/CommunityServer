using System;
using System.Collections.Generic;
using System.Xml;
using Newtonsoft.Json.Linq;

namespace BasecampRestAPI
{
    /*
    {
      "id": 3525720,
      "subject": "Thanks for choosing Basecamp!",
      "content": "<div>Thanks for choosing Basecamp to help manage and collaborate on your projects. We think you're going to love it.</div><div><br></div><div>We've been hard at work on this all new version of Basecamp for nearly a year. We've taken everything we've learned from the previous eight years, and made the all new Basecamp better than ever.</div><div><br></div><div>With Basecamp, your company will be able to keep all your projects together in one safe place online. Basecamp will become your company's central repository for everything project related. Discussions, to-dos, decisions, files, ideas, and schedules all live inside Basecamp.</div><div><br></div><div>The new Basecamp is more useful, more usable, more beautiful, and much faster than before. It's focused, clear, intuitive and easy to use.</div><div><br></div><div>If you ever have any questions please get in touch. You can contact our support team any time by clicking the green \"Support\" tab in the upper left corner of the window. Or you can contact our CEO directly at jason@37signals.com. If he can't help personally, he'll be happy to direct you to someone who can.</div><div><br></div><div>Thanks again for choosing Basecamp.</div><div><br></div><div>-The team at Basecamp<br><br><p>PS. Here's the Basecamp logo&#160;if you ever want to link us up on your site ;)</p>\n</div>",
      "created_at": "2012-07-30T10:57:13+04:00",
      "updated_at": "2012-09-10T13:11:12+04:00",
      "creator": {
        "id": 2047475,
        "name": "Basecamp",
        "avatar_url": "http://asset0.37img.com/global/79383b7fc061815979c8c638784be44d4aa4628aaa16ee5614658a2b4d662eb9ce6fc85f0ea77f81b6e6bd3cdb9c266f42cbf1d2426968d83cb50f0d3078fcd65e6055e264b268aa44ffaa94d9475efa/avatar.gif?r=3"
      },
      "comments": [
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
      ],
      "attachments": [
        {
          "key": "dc337cc7718d2c7a0cb71556f180bfd89341c162",
          "name": "bigbasecamplogo.png",
          "byte_size": 23704,
          "content_type": "image/png",
          "created_at": "2012-07-30T10:57:13+04:00",
          "url": "https://basecamp.com/1937390/api/v1/projects/848689-explore-basecamp/attachments/9783040/dc337cc7718d2c7a0cb71556f180bfd89341c162/original/bigbasecamplogo.png",
          "creator": {
            "id": 2047475,
            "name": "Basecamp",
            "avatar_url": "http://asset0.37img.com/global/79383b7fc061815979c8c638784be44d4aa4628aaa16ee5614658a2b4d662eb9ce6fc85f0ea77f81b6e6bd3cdb9c266f42cbf1d2426968d83cb50f0d3078fcd65e6055e264b268aa44ffaa94d9475efa/avatar.gif?r=3"
          }
        }
      ],
      "subscribers": [
        {
          "id": 2047474,
          "name": "Paul Bannov"
        }
      ]
    }
     * */
    public class Post : IPost
	{
		private readonly IBaseCamp _camp;

		public static IPost GetInstance(IBaseCamp camp, JObject node)
		{
			return new Post(camp, node);
		}
        private Post(IBaseCamp camp, JObject node)
		{
			_camp = camp;
            ID = node["id"].ToObject<int>();
            Title = node["subject"].ToObject<string>();
            Body = node["content"].ToObject<string>();
            PostedOn = node["created_at"].ToObject<DateTime>();
            AuthorID = node["creator"]["id"].ToObject<int>();
            RecentComments = new List<IComment>();
            foreach (var comment in node["comments"])
            {
                RecentComments.Add(Comment.GetInstance(camp, (JObject)comment));
            }

		}

		#region Implementation of IPost

		public int ID { get; private set; }
		public string Title { get; private set; }
		public string Body { get; private set; }
		public DateTime PostedOn { get; private set; }
        public int AuthorID { get; private set; }

        public List<IComment> RecentComments { get; private set; }

		#endregion

		public static string PathForId(int id)
		{
			return string.Format("posts/{0}.json", id);
		}
	}
}
