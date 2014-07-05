using System;

namespace BasecampRestAPI
{
	public interface IComment
	{
		int ID { get; }
		//int CommentableId { get; }
		int AuthorID { get; }
		//string CommentableType { get; }
		string Body { get; }
		//string EmailedFrom { get; }
		//int AttachmentsCount { get; }
		DateTime CreatedAt { get; }
	}
}
