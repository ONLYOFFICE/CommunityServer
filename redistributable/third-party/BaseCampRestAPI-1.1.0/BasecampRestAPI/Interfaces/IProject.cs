using System;
using System.Collections.Generic;

namespace BasecampRestAPI
{
	public interface IProject
	{
		int ID { get; }
		string Name { get; }
        string Description { get; }
		bool IsClosed { get; }
		IPerson[] People { get; }
		IToDoList[] ToDoLists { get; }
		IMilestone[] Milestones { get; }
		IPost[] RecentMessages { get; }
        IAttachment[] Attachments { get; }
        ITimeEntry[] TimeEntries { get; }
        ICategory[] Categories { get; }

		IMilestone GetMilestoneById(int id);
		ICategory[] GetCategoriesForType(CategoryType type);
		IPost[] GetMessagesForCategory(ICategory category);
	}
}