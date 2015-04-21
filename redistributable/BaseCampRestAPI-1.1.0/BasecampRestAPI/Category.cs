using System.Xml;
using Newtonsoft.Json.Linq;

namespace BasecampRestAPI
{
    /*
        <category>
            <id type="integer">#{id}</id>
            <name>#{name}</name>
            <project-id type="integer">#{project_id}</project-id>
            <elements-count type="integer">#{elements_count}</elements-count>
            <type>#{type}</type>
        </category>
    */
    public class Category : ICategory
	{
        private readonly IBaseCamp _camp;

        public static ICategory GetInstance(IBaseCamp camp, JObject node)
        {
            return new Category(camp, node);
        }
        private Category(IBaseCamp camp, JObject node)
        {
            _camp = camp;
            ID = node["id"].ToObject<int>();
            Name = node["name"].ToObject<string>();
            ProjectID = node["project-id"].ToObject<int>();
            Type = (node["type"].ToObject<string>() == "PostCategory") ?
                CategoryType.Post : CategoryType.Attachment;
        }
        
        #region Implementation of ICategory

		public int ID { get; private set; }
        public string Name { get; private set; }
		public int ProjectID { get; private set; }
		public CategoryType Type { get; private set; }

		#endregion

		static public string PathForId(int id)
		{
			return string.Format("categories/{0}.json", id);
		}
	}
}
