using System.Xml;
using System;
using Newtonsoft.Json.Linq;

namespace BasecampRestAPI
{
    /*
      {
        "key": "40b8a84cb1a30dbe04457dc99e094b6299deea41",
        "name": "bearwave.gif",
        "byte_size": 508254,
        "content_type": "image/gif",
        "created_at": "2012-03-27T22:48:49-04:00",
        "url": "https://basecamp.com/1111/api/v1/projects/2222/attachments/3333/40b8a84cb1a30dbe04457dc99e094b6299deea41/original/bearwave.gif",
        "creator": {
          "id": 73,
          "name": "Nick Quaranto",
          "avatar_url": "https://asset0.37img.com/global/4113d0a133a32931be8934e70b2ea21efeff72c1/avatar.96.gif?r=3"
        },
        "attachable": {
          "id": 70219655,
          "type": "Upload",
          "url": "https://basecamp.com/1111/api/v1/projects/2222/uploads/70219655.json"
        }
      }
    */
    public class Attachment : IAttachment
	{
        private readonly IBaseCamp _camp;
        
        public static IAttachment GetInstance(IBaseCamp camp, JObject node)
		{
			return new Attachment(camp, node);
		}
        private Attachment(IBaseCamp camp, JObject node)
		{
			_camp = camp;
            AuthorID = node["creator"]["id"].ToObject<int>();
            CreatedOn = node["created_at"].ToObject<DateTime>();
            ByteSize = node["byte_size"].ToObject<int>();
            DownloadUrl = node["url"].ToObject<string>();
            ID = node["key"].ToObject<string>();
			Name = node["name"].ToObject<string>();
            OwnerID = node["attachable"]["id"].ToObject<int>();
            OwnerType = node["attachable"]["type"].ToObject<string>();
		}

		#region Implementation of IAttachment

        public int ByteSize { get; private set; }
        public string DownloadUrl { get; private set; }
        public string ID { get; private set; }
        public string Name { get; private set; }
        public int OwnerID { get; private set; }
        public string OwnerType { get; private set; }
        public int AuthorID { get; private set; }
        public DateTime CreatedOn { get; private set; }

		#endregion
	}
}
