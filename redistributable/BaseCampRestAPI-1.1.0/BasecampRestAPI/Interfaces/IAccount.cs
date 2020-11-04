using System;

namespace BasecampRestAPI
{
	public interface IAccount
	{
		int Id { get; }
		string Name { get; }
		int AccountHolderId { get; }
		bool SSLEnabled { get; }
		bool EmailNotificationEnabled { get; }
		bool TimeTrackingEnabled { get; }
		DateTime UpdatedAt { get; }
	}
}
