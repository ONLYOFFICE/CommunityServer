using System;

namespace BasecampRestAPI
{
	public interface IMilestone
	{
		int ID { get; }
		bool Completed { get; }
		DateTime CreatedOn { get; }
		DateTime Deadline { get; set; }
		string Title { get; set; }
		//bool WantsNotification { get; }
		//int ResponsiblePartyID { get; }
		//int CreatorID { get; }
		int ProjectID { get; }
	}
}