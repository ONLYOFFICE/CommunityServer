using System.Collections.Generic;

namespace BasecampRestAPI
{
	public interface IToDoList
	{
		int ID { get; }
		int ProjectID { get; }
		int MilestoneID { get; }
        List<IToDoListItem> Items { get; }
	}
}