using System;
using System.Collections.Generic;

namespace BasecampRestAPI
{
	public interface IToDoListItem
	{
		int ID { get; }
		string Content { get; set; }
		DateTime CreatedOn { get; }
		int CreatorID { get; }
        DateTime Deadline { get; }
		bool Completed { get; set; }
		int ResponsibleID { get; }
        List<IComment> RecentComments { get; }

	}
}