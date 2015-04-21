namespace BasecampRestAPI
{
	public enum CategoryType
	{
		Post,
		Attachment
	};

	public interface ICategory
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
		int ID { get; }
		string Name { get; }
		int ProjectID { get; }
		//int ElementsCount { get; }
		CategoryType Type { get; }
	}
}