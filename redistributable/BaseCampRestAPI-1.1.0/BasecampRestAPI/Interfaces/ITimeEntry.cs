using System;

namespace BasecampRestAPI
{
	public interface ITimeEntry
	{
		int ID { get; }
		DateTime Date { get; }
		double Hours { get; }
		string Description { get; }
		int ProjectID { get; }
		int PersonID { get; }
		int ToDoItemID { get; }
	}
}