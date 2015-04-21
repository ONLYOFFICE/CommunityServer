using System;
using System.Collections.Generic;

namespace BasecampRestAPI
{
	public interface IPost
	{
		int ID { get; }
		string Title { get; }
		string Body { get; }
		DateTime PostedOn { get; }
		int AuthorID { get; }
		List<IComment> RecentComments { get; }
	}
}
